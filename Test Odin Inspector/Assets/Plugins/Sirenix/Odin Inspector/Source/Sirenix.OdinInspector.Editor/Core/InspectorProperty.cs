#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorProperty.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Serialization;
    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Represents a property in the inspector, and provides the hub for all functionality related to that property.
    /// </summary>
    public sealed class InspectorProperty : IDisposable
    {
        private int maxDrawCount = 0;
        private Stack<int> drawCountStack = new Stack<int>();
        private int lastUpdatedTreeID = -1;
        private string unityPropertyPath;
        private string prefabModificationPath;
        private List<int> drawerChainIndices = new List<int>();
        private List<BakedDrawerChain> drawerChains;
        private readonly List<Attribute> processedAttributes = new List<Attribute>();
        private ImmutableList<Attribute> processedAttributesImmutable;
        private bool? supportsPrefabModifications = null;
        private List<PropertyComponent> components = new List<PropertyComponent>();
        private ImmutableList<PropertyComponent> componentsImmutable;
        private List<PropertyState> states;
        private List<Rect> lastDrawnValueRects = new List<Rect>();
        private int lastUpdatedStateUpdatersID = -1;
        private StateUpdater[] stateUpdaters;

        public bool AnimateVisibility = true;

        public bool IsTreeRoot { get { return this == this.Tree.RootProperty; } }

        /// <summary>
        /// Gets the property which is the ultimate root of this property's serialization.
        /// </summary>
        public InspectorProperty SerializationRoot { get; private set; }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The nice name of the property, usually as converted by <see cref="ObjectNames.NicifyVariableName(string)"/>.
        /// </summary>
        public string NiceName { get; private set; }
        
        /// <summary>
        /// The cached label of the property, usually containing <see cref="NiceName"/>.
        /// </summary>
        public GUIContent Label { get; set; }

        /// <summary>
        /// The full Odin path of the property. To get the Unity property path, see <see cref="UnityPropertyPath"/>.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The child index of this property.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the resolver for this property's children.
        /// </summary>
        public OdinPropertyResolver ChildResolver { get; private set; }
        
        /// <summary>
        /// <para>The current recursive draw depth, incremented for each time that the property has caused itself to be drawn recursively.</para>
        /// <para>Note that this is the <i>current</i> recursion level, not the total amount of recursions so far this frame.</para>
        /// </summary>
        public int RecursiveDrawDepth
        {
            get
            {
                return this.drawCountStack.Count;
            }
        }

        /// <summary>
        /// The amount of times that the property has been drawn so far this frame.
        /// </summary>
        public int DrawCount
        {
            get
            {
                if (this.drawCountStack.Count == 0)
                {
                    return this.maxDrawCount;
                }

                return this.drawCountStack.Peek();
            }
        }

        /// <summary>
        /// How deep in the drawer chain the property currently is, in the current drawing session as determined by <see cref="DrawCount"/>.
        /// </summary>
        public int DrawerChainIndex
        {
            get
            {
                while (this.drawerChainIndices.Count <= this.DrawCount)
                {
                    this.drawerChainIndices.Add(0);
                }

                return this.drawerChainIndices[this.DrawCount];
            }
        }

        /// <summary>
        /// Whether this property supports having prefab modifications applied or not.
        /// </summary>
        public bool SupportsPrefabModifications
        {
            get
            {
                if (this.supportsPrefabModifications == null)
                {
                    if (!this.Tree.PrefabModificationHandler.HasPrefabs)
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else if (this.Tree.PrefabModificationHandler.HasNestedOdinPrefabData)
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else if (this == this.Tree.RootProperty)
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else if (this.ValueEntry == null || (this.ParentValueProperty != null && !this.ParentValueProperty.IsTreeRoot && !this.ParentValueProperty.SupportsPrefabModifications))
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else if (this.ValueEntry.SerializationBackend == SerializationBackend.None)
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else if (this.GetAttribute<DoesNotSupportPrefabModificationsAttribute>() != null || this.Info.GetAttribute<DoesNotSupportPrefabModificationsAttribute>() != null)
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else if (this.ChildResolver is IMaySupportPrefabModifications)
                    {
                        this.supportsPrefabModifications = (this.ChildResolver as IMaySupportPrefabModifications).MaySupportPrefabModifications;
                    }
                    else
                    {
                        this.supportsPrefabModifications = false;
                    }
                }

                return this.supportsPrefabModifications.Value;
            }
        }

        /// <summary>
        /// Gets an immutable list of the components attached to the property.
        /// </summary>
        public ImmutableList<PropertyComponent> Components
        {
            get
            {
                if (this.componentsImmutable == null)
                {
                    if (this.components == null)
                    {
                        this.CreateComponents();
                    }

                    this.componentsImmutable = new ImmutableList<PropertyComponent>(this.components);
                }

                return this.componentsImmutable;
            }
        }

        /// <summary>
        /// Gets an immutable list of processed attributes for the property.
        /// </summary>
        public ImmutableList<Attribute> Attributes
        {
            get
            {
                if (this.processedAttributesImmutable == null)
                {
                    this.processedAttributesImmutable = new ImmutableList<Attribute>(this.processedAttributes);
                }

                return this.processedAttributesImmutable;
            }
        }

        /// <summary>
        /// Gets an array of the state updaters of the property. Don't change the contents of this array!
        /// </summary>
        public StateUpdater[] StateUpdaters
        {
            get
            {
                if (this.stateUpdaters == null)
                {
                    this.GetNewStateUpdaters();
                    this.UpdateStates(this.Tree.UpdateID);
                }

                return this.stateUpdaters;
            }
        }

        /// <summary>
        /// The value entry that represents the base value of this property.
        /// </summary>
        public PropertyValueEntry BaseValueEntry { get; private set; }

        /// <summary>
        /// The value entry that represents the strongly typed value of the property; this is possibly an alias entry in case of polymorphism.
        /// </summary>
        public IPropertyValueEntry ValueEntry { get; private set; }

        /// <summary>
        /// The parent of the property. If null, this property is a root-level property in the <see cref="PropertyTree"/>.
        /// </summary>
        public InspectorProperty Parent { get; private set; }

        /// <summary>
        /// The <see cref="InspectorPropertyInfo"/> of this property.
        /// </summary>
        public InspectorPropertyInfo Info { get; private set; }

        /// <summary>
        /// The <see cref="PropertyTree"/> that this property exists in.
        /// </summary>
        public PropertyTree Tree { get; private set; }

        /// <summary>
        /// The children of this property.
        /// </summary>
        public PropertyChildren Children { get; private set; }

        /// <summary>
        /// The context container of this property.
        /// </summary>
        public PropertyContextContainer Context { get; private set; }

        /// <summary>
        /// The last rect that this property was drawn within.
        /// </summary>
        public Rect LastDrawnValueRect
        {
            get
            {
                if (this.DrawCount <= 0)
                {
                    return new Rect();
                }

                if (this.DrawCount > this.lastDrawnValueRects.Count)
                {
                    this.lastDrawnValueRects.SetLength(this.DrawCount);
                }

                return this.lastDrawnValueRects[this.DrawCount - 1];
            }
        }

        /// <summary>
        /// The type on which this property is declared. This is the same as <see cref="InspectorPropertyInfo.TypeOfOwner"/>.
        /// </summary>
        public Type ParentType { get; private set; }

        /// <summary>
        /// The parent values of this property, by selection index; this represents the values that 'own' this property, on which it is declared.
        /// </summary>
        public ImmutableList ParentValues { get; private set; }

        public InspectorProperty ParentValueProperty { get; private set; }

        /// <summary>
        /// <para>The full Unity property path of this property; note that this is merely a converted version of <see cref="Path"/>, and not necessarily a path to an actual Unity property.</para>
        /// <para>In the case of Odin-serialized data, for example, no Unity properties will exist at this path.</para>
        /// </summary>
        public string UnityPropertyPath
        {
            get
            {
                if (this.unityPropertyPath == null)
                {
                    this.unityPropertyPath = InspectorUtilities.ConvertToUnityPropertyPath(this.Path);
                }

                return this.unityPropertyPath;
            }
        }

        /// <summary>
        /// <para>The full path of this property as used by deep reflection, containing all the necessary information to find this property through reflection only. This is used as the path for prefab modifications.</para>
        /// </summary>
        [Obsolete("Use PrefabModificationPath instead, which serves the exact same function.", false)]
        public string DeepReflectionPath { get { return this.PrefabModificationPath; } }

        /// <summary>
        /// <para>The full path of this property as used by prefab modifications and the deep reflection system, containing all the necessary information to find this property through reflection only.</para>
        /// </summary>
        public string PrefabModificationPath
        {
            get
            {
                if (this.prefabModificationPath == null)
                {
                    this.prefabModificationPath = InspectorUtilities.ConvertToDeepReflectionPath(this.Path);
                }

                return this.prefabModificationPath;
            }
        }

        /// <summary>
        /// The PropertyState of the property at the current draw count index.
        /// </summary>
        public PropertyState State
        {
            get
            {
                var index = this.DrawCount - 1;
                if (index < 0) index = 0;

                if (this.states == null)
                {
                    this.states = new List<PropertyState>();
                }

                while (this.states.Count <= index)
                {
                    this.states.Add(null);
                }

                var state = this.states[index];

                if (state == null)
                {
                    state = new PropertyState(this, index);
                    this.states[index] = state;
                }

                return state;
            }
        }

        private InspectorProperty()
        {
        }

        /// <summary>
        /// Gets the component of a given type on the property, or null if the property does not have a component of the given type.
        /// </summary>
        public T GetComponent<T>() where T : PropertyComponent
        {
            T result;

            if (this.components == null || this.components.Count != this.Tree.ComponentProviders.Count)
                this.CreateComponents();

            for (int i = 0; i < this.components.Count; i++)
            {
                result = this.components[i] as T;

                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Marks the property's serialization root values dirty if they are derived from UnityEngine.Object.
        /// </summary>
        public void MarkSerializationRootDirty()
        {
            if (this.SerializationRoot == null) return;
            
            foreach (var value in this.SerializationRoot.ValueEntry.WeakValues)
            {
                UnityEngine.Object obj = value as UnityEngine.Object;

                if (obj != null)
                {
                    InspectorUtilities.RegisterUnityObjectDirty(obj);
                }
            }
        }

        /// <summary>
        /// Records the property's serialization root for undo to prepare for undoable changes, with a custom string that includes the property path and Unity object name. If a message is specified, it is included in the custom undo string.
        /// </summary>
        public void RecordForUndo(string message = null, bool forceCompleteObjectUndo = false)
        {
            // Undo is disabled for the property tree
            if (!this.Tree.WillUndo) return;

            var serializationRoot = this.SerializationRoot;

            if (serializationRoot == null) return;

            if (!forceCompleteObjectUndo && this.ValueEntry != null && UnityPolymorphicSerializationBackend.SerializeReferenceAttribute != null)
            {
                // [SerializeReference] needs a complete object undo.

                var attrs = this.Info.Attributes;

                for (int i = 0; i < attrs.Count; i++)
                {
                    if (attrs[i].GetType() == UnityPolymorphicSerializationBackend.SerializeReferenceAttribute)
                    {
                        forceCompleteObjectUndo = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < serializationRoot.ValueEntry.ValueCount; i++)
            {
                UnityEngine.Object unityObj = serializationRoot.ValueEntry.WeakValues[i] as UnityEngine.Object;

                if (unityObj != null)
                {
                    string recordMessage;

                    if (this == this.Tree.RootProperty)
                    {
                        recordMessage = message == null ?
                            "Change " + unityObj.name :
                            "Change " + unityObj.name + ": " + message;
                    }
                    else
                    {
                        recordMessage = message == null ?
                            "Change " + this.PrefabModificationPath + " on " + unityObj.name :
                            "Change " + this.PrefabModificationPath + " on " + unityObj.name + ": " + message;
                    }

                    if (forceCompleteObjectUndo)
                    {
                        Undo.RegisterCompleteObjectUndo(unityObj, recordMessage);
                    }
                    else
                    {
                        Undo.RecordObject(unityObj, recordMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the first attribute of a given type on this property.
        /// </summary>
        public T GetAttribute<T>() where T : Attribute
        {
            T result;

            for (int i = 0; i < this.processedAttributes.Count; i++)
            {
                result = this.processedAttributes[i] as T;

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first attribute of a given type on this property, which is not contained in a given hashset.
        /// </summary>
        /// <param name="exclude">The attributes to exclude.</param>
        public T GetAttribute<T>(HashSet<Attribute> exclude) where T : Attribute
        {
            for (int i = 0; i < this.processedAttributes.Count; i++)
            {
                T attr = this.processedAttributes[i] as T;

                if (attr != null && (exclude == null || !exclude.Contains(attr)))
                {
                    return attr;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all attributes of a given type on the property.
        /// </summary>
        public IEnumerable<T> GetAttributes<T>() where T : Attribute
        {
            T result;

            for (int i = 0; i < this.processedAttributes.Count; i++)
            {
                result = this.processedAttributes[i] as T;

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "InspectorProperty (" + this.Path + ")";
        }

        public BakedDrawerChain GetActiveDrawerChain()
        {
            bool isNewlyCreated;
            return this.GetActiveDrawerChain(out isNewlyCreated);
        }

        private BakedDrawerChain GetActiveDrawerChain(out bool isNewlyCreated)
        {
            if (this.drawerChains == null) this.drawerChains = new List<BakedDrawerChain>();

            var index = this.DrawCount - 1;
            if (index < 0) index = 0;

            BakedDrawerChain result;
            if (this.drawerChains.Count <= index)
            {
                result = this.Tree.DrawerChainResolver.GetDrawerChain(this).Bake();
                this.drawerChains.Add(result);

                // This is important; drawer initialization may request the newly created drawer chain,
                //  and so we must be able to fetch it before we initialize the drawers in the chain.
                for (int i = 0; i < result.BakedDrawerArray.Length; i++)
                {
                    result.BakedDrawerArray[i].Initialize(this);
                }

                isNewlyCreated = true;
            }
            else
            {
                isNewlyCreated = false;
                result = this.drawerChains[index];
            }
            
            return result;
        }

        public void RefreshSetup()
        {
            this.RefreshSetup(true);
        }

        private void RefreshSetup(bool disposeOld)
        {
            if (disposeOld)
            {
                this.DisposeExistingSetup();
            }

            if (this.stateUpdaters != null)
            {
                this.stateUpdaters = null;
            }

            if (this.drawerChains != null)
            {
                this.drawerChains.Clear();
            }
            
            if (this.states != null)
            {
                for (int i = 0; i < this.states.Count; i++)
                {
                    if (this.states[i] != null)
                        this.states[i].Reset();
                }
            }

            if (this.components == null || this.components.Count != this.Tree.ComponentProviders.Count)
            {
                this.CreateComponents();
            }
            else
            {
                for (int i = 0; i < this.components.Count; i++)
                {
                    this.components[i].Reset();
                }
            }

            this.RefreshProcessedAttributes();
            this.ChildResolver = this.Tree.PropertyResolverLocator.GetResolver(this);
            this.Children = new PropertyChildren(this);
            this.Children.Update();
            this.GetNewStateUpdaters();
            this.UpdateStates(this.Tree.UpdateID);
        }

        private void CreateComponents()
        {
            if (this.components == null)
                this.components = new List<PropertyComponent>(this.Tree.ComponentProviders.Count);
            else
            {
                for (int i = 0; i < this.components.Count; i++)
                {
                    var disposable = this.components[i] as IDisposable;
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                this.components.Clear();
            }

            for (int i = 0; i < this.Tree.ComponentProviders.Count; i++)
            {
                this.components.Add(this.Tree.ComponentProviders[i].CreateComponent(this));
            }
        }

        private void RefreshProcessedAttributes()
        {
            this.processedAttributes.Clear();

            for (int i = 0; i < this.Info.Attributes.Count; i++)
            {
                this.processedAttributes.Add(this.Info.Attributes[i]);
            }

            var processors = this.Tree.AttributeProcessorLocator.GetSelfProcessors(this);

            for (int i = 0; i < processors.Count; i++)
            {
                try
                {
                    processors[i].ProcessSelfAttributes(this, this.processedAttributes);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        internal void OnStateUpdate(int treeID)
        {
            this.Update();
            this.UpdateStates(treeID);

            foreach (var prop in this.Children.GetExistingChildren())
            {
                prop.OnStateUpdate(treeID);
            }
        }

        /// <summary>
        /// Draws this property in the inspector.
        /// </summary>
        public void Draw()
        {
            this.Draw(this.Label);
        }

        /// <summary>
        /// Draws this property in the inspector with a given default label. This default label may be overridden by attributes on the drawn property.
        /// </summary>
        public void Draw(GUIContent defaultLabel)
        {
            this.Update();

            bool popGUIEnabled = false;
            bool popDraw = true;

            try
            {
                this.PushDraw();

                var chain = this.GetActiveDrawerChain();
                var state = this.State;

                var fadeGroup = this.AnimateVisibility;

                bool show = fadeGroup ? SirenixEditorGUI.BeginFadeGroup(state, state.VisibleLastLayout) : state.VisibleLastLayout;

                if (show)
                {
                    if ((this.RecursiveDrawDepth + InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth) > GeneralDrawerConfig.Instance.MaxRecursiveDrawDepth)
                    {
                        SirenixEditorGUI.ErrorMessageBox("The property '" + this.NiceName + "' has exceeded the maximum recursive draw depth limit of " + GeneralDrawerConfig.Instance.MaxRecursiveDrawDepth + ".");
                        return;
                    }

                    if (!this.IsTreeRoot
                        && this.ValueEntry != null
                        && this.ValueEntry.SerializationBackend == SerializationBackend.Odin
                        && !this.SupportsPrefabModifications
                        && this.Tree.PrefabModificationHandler.HasPrefabs
                        && !GUIHelper.IsDrawingDictionaryKey
                        && this.Info.PropertyType == PropertyType.Value)
                    {
                        if (this.ParentValueProperty != null && (this.ParentValueProperty.IsTreeRoot || this.ParentValueProperty.SupportsPrefabModifications) && GeneralDrawerConfig.Instance.ShowPrefabModificationsDisabledMessage)
                        {
                            string objText = this.Tree.PrefabModificationHandler.HasNestedOdinPrefabData ? "this instance" : "prefab instances";
                            SirenixEditorGUI.InfoMessageBox("The property '" + this.NiceName + "' does not support being modified on " + objText + ". (You can disable this message in the general drawer config.)");
                        }

                        GUIHelper.PushGUIEnabled(false);
                        popGUIEnabled = true;
                    }
                
                    chain.Reset();

                    var e = Event.current.type;
                    var currDrawCount = this.DrawCount;
                    var measureBox = e == EventType.Repaint || (e != EventType.Layout && this.LastDrawnValueRect.height == 0);

                    if (measureBox)
                    {
                        GUIHelper.BeginLayoutMeasuring();
                    }

                    {
                        try
                        {
                            if (this.stateUpdaters != null)
                            {
                                for (int i = 0; i < this.stateUpdaters.Length; i++)
                                {
                                    var updater = this.stateUpdaters[i];

                                    if (updater.ErrorMessage != null)
                                    {
                                        SirenixEditorGUI.ErrorMessageBox("Error in state updater '" + updater.GetType().GetNiceName() + "':\n\n" + updater.ErrorMessage);
                                    }
                                }
                            }

                            if (chain.MoveNext())
                            {
                                bool setIsBoldState = this.ValueEntry != null && e == EventType.Repaint;
                                bool isPrefabChanged = false;
                                if (setIsBoldState)
                                {
                                    isPrefabChanged = this.ValueEntry.ValueChangedFromPrefab;
                                    var boldState = isPrefabChanged;

                                    if (GUIHelper.IsDrawingDictionaryKey)
                                    {
                                        // Always propagate changed state down through dictionary keys
                                        boldState |= GUIHelper.IsBoldLabel;
                                    }

                                    GUIHelper.PushIsBoldLabel(boldState);
                                }
                            
                                var popPushGUIDisabled = this.ValueEntry != null && !this.ValueEntry.IsEditable;

                                popPushGUIDisabled |= !state.EnabledLastLayout;

                                if (popPushGUIDisabled)
                                {
                                    GUIHelper.PushGUIEnabled(false);
                                }

#if ODIN_TRIAL
                                bool former = true;
                                if (TrialUtilities.IsReallyExpired)
                                {
                                    former = GUI.enabled;
                                    GUI.enabled = false;
                                }
#endif
                                chain.Current.DrawProperty(defaultLabel);
#if ODIN_TRIAL
                                if (TrialUtilities.IsReallyExpired)
                                {
                                    GUI.enabled = former;
                                }
#endif

                                if (popPushGUIDisabled)
                                {
                                    GUIHelper.PopGUIEnabled();
                                }

                                if (setIsBoldState)
                                {
                                    GUIHelper.PopIsBoldLabel();
                                }

                                if (isPrefabChanged && e == EventType.Repaint && GeneralDrawerConfig.Instance.ShowPrefabModifiedValueBar && this.LastDrawnValueRect != default(Rect))
                                {
                                    Color prefabChangeMarginBarColor = new Color(0.003921569f, 0.6f, 0.9215686f, 0.75f);

                                    var rect = this.LastDrawnValueRect;
                                    rect.width = 2;
                                    rect.x -= 2.5f;
                                    rect.x += GUIHelper.CurrentIndentAmount;

                                    if (this.ChildResolver is ICollectionResolver)
                                    {
                                        rect.height -= 3.5f;
                                    }

                                    GUIHelper.PushGUIEnabled(true);
                                    SirenixEditorGUI.DrawSolidRect(rect, prefabChangeMarginBarColor);
                                    GUIHelper.PopGUIEnabled();
                                }
                            }
                            else
                            {
                                if (this.Info.PropertyType == PropertyType.Method)
                                {
                                    EditorGUILayout.LabelField(this.NiceName, "No drawers could be found for the method property '" + this.Name + "'.");
                                }
                                else if (this.Info.PropertyType == PropertyType.Group)
                                {
                                    var attr = this.GetAttribute<PropertyGroupAttribute>() ?? this.Info.GetAttribute<PropertyGroupAttribute>();

                                    if (attr != null)
                                    {
                                        EditorGUILayout.LabelField(this.NiceName, "No drawers could be found for the property group '" + this.Name + "' with property group attribute type '" + attr.GetType().GetNiceName() + "'.");
                                    }
                                    else
                                    {
                                        EditorGUILayout.LabelField(this.NiceName, "No drawers could be found for the property group '" + this.Name + "'.");
                                    }
                                }
                                //else if (property.Info.GetAttribute<HideInInspector>() == null)
                                //{
                                //    EditorGUILayout.LabelField(property.NiceName, "No drawers could be found for the value property '" + property.Name + "' of type '" + property.ValueEntry.TypeOfValue.GetNiceName() + "'.");
                                //}
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.IsExitGUIException())
                            {
                                popDraw = false;
                                throw ex.AsExitGUIException();
                            }
                            else
                            {
                                var msg =
                                    "This error occurred while being drawn by Odin. \n" +
                                    "Current IMGUI event: " + Event.current.type + "\n" +
                                    "Odin Property Path: " + this.Path + "\n" +
                                    "Odin Drawer Chain:\n" + string.Join("\n", chain.BakedDrawerArray.Select(n => " > " + n.GetType().GetNiceName()).ToArray()) + ".";

                                Debug.LogException(new OdinPropertyException(msg, ex));
                            }
                        }
                    }

                    if (measureBox)
                    {
                        if (currDrawCount > this.lastDrawnValueRects.Count)
                        {
                            this.lastDrawnValueRects.SetLength(currDrawCount);
                        }

                        this.lastDrawnValueRects[currDrawCount - 1] = GUIHelper.EndLayoutMeasuring();
                    }
                }

                if (fadeGroup)
                {
                    SirenixEditorGUI.EndFadeGroup();
                }
            }
            finally
            {
                if (popDraw)
                {
                    this.PopDraw();
                }

                if (popGUIEnabled)
                {
                    GUIHelper.PopGUIEnabled();
                }
            }
        }

        /// <summary>
        /// Push a draw session. This is used by <see cref="DrawCount"/> and <see cref="RecursiveDrawDepth"/>.
        /// </summary>
        public void PushDraw()
        {
            this.maxDrawCount++;
            this.drawCountStack.Push(this.maxDrawCount);
        }

        /// <summary>
        /// Increments the current drawer chain index. This is used by <see cref="DrawerChainIndex"/>.
        /// </summary>
        public void IncrementDrawerChainIndex()
        {
            while (this.drawerChainIndices.Count <= this.DrawCount)
            {
                this.drawerChainIndices.Add(0);
            }

            this.drawerChainIndices[this.DrawCount]++;
        }

        /// <summary>
        /// Pop a draw session. This is used by <see cref="DrawCount"/> and <see cref="RecursiveDrawDepth"/>.
        /// </summary>
        public void PopDraw()
        {
            this.drawCountStack.Pop();
        }

        public bool IsReachableFromRoot()
        {
            bool reachable = false;

            try
            {
                if (this.Parent == null)
                {
                    var root = this.Tree.RootProperty;
                    reachable = this == root || root.Children[this.Name] == this;
                    return reachable;
                }

                if (!this.Parent.IsReachableFromRoot())
                    return false;

                reachable = this.Parent.Children[this.Name] == this;
                return reachable;
            }
            finally
            {
                if (reachable)
                {
                    this.Update();
                }
            }
        }

        /// <summary>
        /// Gets the next property in the <see cref="PropertyTree"/>, or null if none is found.
        /// </summary>
        /// <param name="includeChildren">Whether to include children or not.</param>
        /// <param name="visibleOnly">Whether to only include visible properties.</param>
        public InspectorProperty NextProperty(bool includeChildren = true, bool visibleOnly = false)
        {
            if (includeChildren)
            {
                if (visibleOnly)
                {
                    for (int i = 0; i < this.Children.Count; i++)
                    {
                        var child = this.Children[i];
                        if (child.State.Visible) return child;
                    }
                }
                else if (this.Children.Count > 0)
                {
                    return this.Children.Get(0);
                }
            }
            
            InspectorProperty former = null;
            InspectorProperty current = this;

            InspectorProperty treeRoot = this.Tree.RootProperty;

            while (true)
            {
                do
                {
                    former = current;
                    current = current.Parent;
                }
                while (current != null && current != treeRoot && former.Index + 1 >= former.Parent.Children.Count);

                if (current != null)
                {
                    if (visibleOnly)
                    {
                        for (int i = former.Index + 1; i < current.Children.Count; i++)
                        {
                            var child = current.Children[i];
                            if (child.State.Visible) return child;
                        }

                        continue;
                    }
                    else if (former.Index + 1 < current.Children.Count) return current.Children[former.Index + 1];
                    else continue;
                }
                //else if (former.Index + 1 < this.Tree.RootPropertyCount)
                //{
                //    if (visibleOnly)
                //    {
                //        for (int i = former.Index + 1; i < this.Tree.RootPropertyCount; i++)
                //        {
                //            var root = this.Tree.GetRootProperty(i);
                //            if (root.State.Visible) return root;
                //        }

                //        return null;
                //    }
                //    else return this.Tree.GetRootProperty(former.Index + 1);
                //}
                else return null;
            }
        }

        /// <summary>
        /// Finds the first parent property that matches a given predicate.
        /// </summary>
        public InspectorProperty FindParent(Func<InspectorProperty, bool> predicate, bool includeSelf)
        {
            var current = includeSelf ? this : this.Parent;

            while (current != null)
            {
                if (predicate(current)) return current;
                current = current.Parent;
            }

            return null;
        }

        /// <summary>
        /// Finds the first child recursively, that matches a given predicate.
        /// </summary>
        public InspectorProperty FindChild(Func<InspectorProperty, bool> predicate, bool includeSelf)
        {
            if (includeSelf && predicate(this))
            {
                return this;
            }

            return this.Children.Recurse().FirstOrDefault(predicate);
        }

        internal void ClearDrawCount()
        {
            this.maxDrawCount = 0;
            this.drawCountStack.Clear();

            for (int i = 0; i < this.drawerChainIndices.Count; i++)
            {
                this.drawerChainIndices[i] = 0;
            }
        }

        /// <summary>
        /// Updates the property. This method resets the temporary context, and updates the value entry and the property children.
        /// </summary>
        /// <param name="forceUpdate">If true, the property will update regardless of whether it has already updated for the current <see cref="PropertyTree.UpdateID"/>.</param>
        public bool Update(bool forceUpdate = false)
        {
            bool newId = this.Tree.UpdateID != this.lastUpdatedTreeID;

            if (forceUpdate == false && !newId)
            {
                // We've already updated this property during this property tree update
                return false;
            }

            if (newId)
            {
                this.ClearDrawCount();
            }

            this.lastUpdatedTreeID = this.Tree.UpdateID;

            this.UpdateValueEntry();

            if (this.stateUpdaters == null || this.Children == null || this.ChildResolver == null || (this.ValueEntry != null && this.ChildResolver.ResolverForType != null && this.ValueEntry.TypeOfValue != this.ChildResolver.ResolverForType))
            {
                this.RefreshSetup();
            }
            else
            {
                // RefreshSetup already calls Children.Update.
                this.Children.Update();
            }

            //
            // Updating the prefab modification state of the property
            // must happen after everything else, including children,
            // has been updated.
            //

            if (this.ValueEntry != null)
            {
                if (this.ValueEntry.SerializationBackend == SerializationBackend.Odin)
                {
                    var change = this.Tree.PrefabModificationHandler.GetPrefabModificationType(this);

                    this.BaseValueEntry.ValueChangedFromPrefab = change == PrefabModificationType.Value;
                    this.BaseValueEntry.ListLengthChangedFromPrefab = change == PrefabModificationType.ListLength;
                    this.BaseValueEntry.DictionaryChangedFromPrefab = change == PrefabModificationType.Dictionary;
                }
                else
                {
                    this.BaseValueEntry.ValueChangedFromPrefab = false;
                    this.BaseValueEntry.ListLengthChangedFromPrefab = false;
                }
            }

            this.UpdateStates(this.lastUpdatedTreeID);

            return true;
        }

        private void UpdateStates(int treeID)
        {
            if (this.stateUpdaters == null)
            {
                this.GetNewStateUpdaters();
            }

            if (this.lastUpdatedStateUpdatersID == treeID) return;
            this.lastUpdatedStateUpdatersID = treeID;

            for (int i = 0; i < this.stateUpdaters.Length; i++)
            {
                try
                {
                    this.stateUpdaters[i].OnStateUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if (this.states != null)
            {
                for (int i = 0; i < this.states.Count; i++)
                {
                    if (this.states[i] != null)
                    {
                        this.states[i].Update();
                    }
                }
            }
        }

        private void GetNewStateUpdaters()
        {
            this.stateUpdaters = this.Tree.StateUpdaterLocator.GetStateUpdaters(this);

            for (int i = 0; i < this.stateUpdaters.Length; i++)
            {
                try
                {
                    this.stateUpdaters[i].Initialize(this);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Populates a generic menu with items from all drawers for this property that implement <see cref="IDefinesGenericMenuItems"/>.
        /// </summary>
        public void PopulateGenericMenu(GenericMenu genericMenu)
        {
            if (genericMenu == null)
            {
                throw new ArgumentNullException("genericMenu");
            }

            OdinDrawer[] drawers = this.GetActiveDrawerChain().BakedDrawerArray;

            // TODO: I've inserted a fix here to ensure that the correct DrawChainIndex value is set before
            // IDefinesGenericMenuItems.PopulateGenericMenu is called. I've done this because PropertyContexts
            // gets fetched depending upon this value. Without this a drawer would be unable to get the correct
            // PropertyContext instance.
            var count = this.DrawCount;
            var prevIndex = this.DrawerChainIndex;

            try
            {
                for (int i = 0; i < drawers.Length; i++)
                {
                    var drawer = drawers[i] as IDefinesGenericMenuItems;

                    if (drawer != null)
                    {
                        this.drawerChainIndices[count] = i + 1; // Apparently drawer indices are 1-indexed.
                        drawer.PopulateGenericMenu(this, genericMenu);
                    }
                }
            }
            finally
            {
                this.drawerChainIndices[count] = prevIndex;
            }
        }

        /// <summary>
        /// Determines whether this property is the child of another property in the hierarchy.
        /// </summary>
        /// <param name="other">The property to check whether this property is the child of.</param>
        /// <exception cref="System.ArgumentNullException">other is null</exception>
        public bool IsChildOf(InspectorProperty other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            InspectorProperty parent = this.Parent;

            while (parent != null)
            {
                if (parent == other)
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this property is a parent of another property in the hierarchy.
        /// </summary>
        /// <param name="other">The property to check whether this property is the parent of.</param>
        /// <exception cref="System.ArgumentNullException">other is null</exception>
        public bool IsParentOf(InspectorProperty other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            InspectorProperty parent = other.Parent;

            while (parent != null)
            {
                if (parent == this)
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }

        internal static InspectorProperty Create(PropertyTree tree, InspectorProperty parent, InspectorPropertyInfo info, int index, bool isRoot)
        {
            // Validate parameters first
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }

            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            if (parent != null)
            {
                if (tree != parent.Tree)
                {
                    throw new ArgumentException("The given tree and the given parent's tree are not the same tree.");
                }

                if (index < 0 || index >= parent.Children.Count)
                {
                    throw new IndexOutOfRangeException("The given index for the property to create is out of bounds.");
                }
            }
            else if (!isRoot) throw new ArgumentException("The property to be created has been given no parent, and is not the tree root.");

            // Now start building a property
            InspectorProperty property = new InspectorProperty();

            // Set some basic values
            property.Tree = tree;
            property.Info = info;
            property.Parent = parent;
            property.Index = index;
            property.Context = new PropertyContextContainer(property);
            
            // Find property path
            {
                if (parent != null)
                {
                    property.Path = parent.Children.GetPath(index);
                }
                else
                {
                    property.Path = info.PropertyName;
                }

                if (property.Path == null)
                {
                    Debug.Log("Property path is null for property " + ObjectNames.NicifyVariableName(info.PropertyName.TrimStart('#', '$')) + "!");
                }
            }

            // Find parent value property
            if (parent != null)
            {
                InspectorProperty current = property;

                do
                {
                    current = current.Parent;
                }
                while (current != null && current.BaseValueEntry == null);

                property.ParentValueProperty = current;
            }

            // Set parent type and values
            if (property.ParentValueProperty != null)
            {
                property.ParentType = property.ParentValueProperty.ValueEntry.TypeOfValue;
                property.ParentValues = new ImmutableList(property.ParentValueProperty.ValueEntry.WeakValues);
            }
            else
            {
                property.ParentType = tree.TargetType;
                property.ParentValues = new ImmutableList(tree.WeakTargets);
            }

            // Find serializing/owning property
            {
                InspectorProperty current = property.ParentValueProperty;

                while (current != null && !current.ValueEntry.TypeOfValue.InheritsFrom(typeof(UnityEngine.Object)))
                {
                    current = current.ParentValueProperty;
                }

                if (current != null)
                {
                    property.SerializationRoot = current;
                }
                else
                {
                    property.SerializationRoot = isRoot ? property : tree.RootProperty;
                }
            }

            // Set name and label
            {
                property.Name = info.PropertyName;

                var mi = property.Info.GetMemberInfo() as MethodInfo;
                if (mi != null)
                {
                    var name = property.Name;
                    var parensIndex = name.IndexOf('(');

                    if (parensIndex >= 0)
                    {
                        name = name.Substring(0, parensIndex);
                    }

                    property.NiceName = name.TrimStart('#', '$').SplitPascalCase();
                }
                else
                {
                    property.NiceName = ObjectNames.NicifyVariableName(property.Name.TrimStart('#', '$'));
                }

                property.Label = new GUIContent(property.NiceName);
            }

            // Create a value entry if necessary
            if (property.Info.PropertyType == PropertyType.Value)
            {
                property.BaseValueEntry = PropertyValueEntry.Create(property, info.TypeOfValue, isRoot);
                property.ValueEntry = property.BaseValueEntry;
            }

            property.CreateComponents();

            // Do NOT update the property here. Property updating may cause this property to be requested before
            // it has been registered, resulting in an infinite loop. It is the calling code's responsibility to
            // update the property before usage.

            if (!isRoot)
            {
                property.RefreshProcessedAttributes();
                property.ChildResolver = tree.PropertyResolverLocator.GetResolver(property);
                property.Children = new PropertyChildren(property);
            }

            return property;
        }

        private void UpdateValueEntry()
        {
            // Ensure we have the right sort of value entry

            if (this.Info.PropertyType != PropertyType.Value)
            {
                // Groups and methods have no value entries
                if (this.ValueEntry != null || this.BaseValueEntry != null)
                {
                    this.ValueEntry = null;
                    this.BaseValueEntry = null;
                    this.RefreshSetup();
                }
                return;
            }

            this.BaseValueEntry.Update();

            if (!this.Info.TypeOfValue.IsValueType)
            {
                Type containedType = this.BaseValueEntry.TypeOfValue;

                if (containedType != this.BaseValueEntry.BaseValueType)
                {
                    if (this.ValueEntry == null || (this.ValueEntry.IsAlias && this.ValueEntry.TypeOfValue != containedType) || (!this.ValueEntry.IsAlias && this.ValueEntry.TypeOfValue != this.ValueEntry.BaseValueType))
                    {
                        this.DisposeExistingSetup();
                        this.ValueEntry = PropertyValueEntry.CreateAlias(this.BaseValueEntry, containedType);
                        this.RefreshSetup(false);
                    }
                }
                else if (this.ValueEntry != this.BaseValueEntry)
                {
                    this.DisposeExistingSetup();
                    this.ValueEntry = this.BaseValueEntry;
                    this.RefreshSetup(false);
                }
            }
            else if (this.ValueEntry == null)
            {
                this.DisposeExistingSetup();
                this.ValueEntry = this.BaseValueEntry;
                this.RefreshSetup(false);
            }

            if (this.ValueEntry != this.BaseValueEntry)
            {
                this.ValueEntry.Update();
            }
        }

        public void Dispose()
        {
            this.DisposeExistingSetup();
        }

        public void CleanForCachedReuse()
        {
            foreach (var child in this.Children.GetExistingChildren())
            {
                child.CleanForCachedReuse();
            }

            if (this.drawerChains != null)
            {
                foreach (var drawerChain in this.drawerChains)
                {
                    foreach (var drawer in drawerChain.BakedDrawerArray)
                    {
                        IDisposable disposable = drawer as IDisposable;
                        if (disposable != null)
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }

                this.drawerChains.Clear();
            }

            if (this.stateUpdaters != null)
            {
                for (int i = 0; i < this.stateUpdaters.Length; i++)
                {
                    IDisposable disposable = this.stateUpdaters[i] as IDisposable;
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                this.stateUpdaters = null;
            }

            if (this.components != null)
            {
                for (int i = 0; i < this.components.Count; i++)
                {
                    var disposable = this.components[i] as IDisposable;
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

            if (this.states != null)
            {
                for (int i = 0; i < this.states.Count; i++)
                {
                    var state = this.states[i];

                    if (state != null)
                    {
                        state.CleanForCachedReuse();
                    }
                }
            }

            this.components = null;
            this.componentsImmutable = null;
        }

        private void DisposeExistingSetup()
        {
            if (this.drawerChains != null)
            {
                foreach (var drawerChain in this.drawerChains)
                {
                    foreach (var drawer in drawerChain.BakedDrawerArray)
                    {
                        IDisposable disposable = drawer as IDisposable;
                        if (disposable != null)
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }

                this.drawerChains.Clear();
            }

            if (this.stateUpdaters != null)
            {
                for (int i = 0; i < this.stateUpdaters.Length; i++)
                {
                    IDisposable disposable = this.stateUpdaters[i] as IDisposable;
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                this.stateUpdaters = null;
            }

            if (this.components != null)
            {
                for (int i = 0; i < this.components.Count; i++)
                {
                    var disposable = this.components[i] as IDisposable;
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                this.components.Clear();
            }

            if (this.ChildResolver is IDisposable)
            {
                try
                {
                    (this.ChildResolver as IDisposable).Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if (this.ValueEntry != null)
            {
                try
                {
                    this.ValueEntry.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            
            if (this.Children != null)
            {
                foreach (var child in this.Children.GetExistingChildren())
                {
                    try
                    {
                        child.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            this.Tree.NotifyPropertyDisposed(this);
        }

        private static InspectorProperty PropertyQueryLookup(InspectorProperty context, string path)
        {
            var parent = context.ParentValueProperty;

            while (parent != null && !parent.Info.HasBackingMembers)
            {
                parent = parent.ParentValueProperty;
            }

            if (parent == null) parent = context.Tree.RootProperty;

            var result = parent.Children[path];

            if (result == null)
            {
                if (parent == context.Tree.RootProperty)
                    result = context.Tree.GetPropertyAtPath(path);
                else
                    result = context.Tree.GetPropertyAtPath(parent.Path + "." + path);
            }

            if (result == null)
            {
                throw new Exception("Property query could not find the property '" + path + "' in the context of the property '" + context.NiceName + "'.");
            }

            if (Event.current != null)
            {
                // If we're currently in an IMGUI drawing context, then we make sure the property's drawers are initialized right away,
                // as they may create custom states that the caller of the property query lookup (often an expression) will depend upon.
                result.GetActiveDrawerChain();
            }

            return result;
        }
    }
}
#endif