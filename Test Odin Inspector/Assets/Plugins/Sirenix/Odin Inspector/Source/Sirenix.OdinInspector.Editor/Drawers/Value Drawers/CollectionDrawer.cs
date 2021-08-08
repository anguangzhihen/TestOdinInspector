#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CollectionDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ActionResolvers;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor.Expressions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using Utilities.Editor;

    //
    // TODO: Rewrite ListDrawer completely!
    // Make a utility for drawing lists that dictioanries, hashsets, etc can utialize.
    // And use the new DragAndDropUtilities instead of the old and broken DragAndDropmanager.
    // Handle both drag and drop event, in the same method. Preferably, instead of having order dependency which is impossible
    // with cross window dragging etc...
    //

    public static class CollectionDrawerStaticInfo
    {
        public static InspectorProperty CurrentDraggingPropertyInfo;
        public static InspectorProperty CurrentDroppingPropertyInfo;
        public static DelayedGUIDrawer DelayedGUIDrawer = new DelayedGUIDrawer();
        public static Action NextCustomAddFunction;
    }

    internal class CollectionSizeDialogue
    {
        public int Size;
        private Action<int> confirm;
        private Action cancel;

        public CollectionSizeDialogue(Action<int> confirm, Action cancel, int size)
        {
            this.confirm = confirm;
            this.cancel = cancel;
            this.Size = size;
        }

        [Button(ButtonSizes.Medium), HorizontalGroup(0.5f)]
        public void Confirm()
        {
            this.confirm(this.Size);
        }

        [Button(ButtonSizes.Medium), HorizontalGroup]
        public void Cancel()
        {
            this.cancel();
        }
    }

    internal interface IHackyListDrawerInteractions
    {
        bool CanCreateValuesToAdd { get; }
        void CreateValuesToAdd(Action<object[]> onCreated, Rect potentialPopupPosition);
    }


    /// <summary>
    /// Property drawer for anything that has a <see cref="ICollectionResolver"/>.
    /// </summary>
    [AllowGUIEnabledForReadonly]
    [DrawerPriority(0, 0, 0.9)]
    public class CollectionDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems, IHackyListDrawerInteractions
    {
        private static GUILayoutOption[] listItemOptions = GUILayoutOptions.MinHeight(25).ExpandWidth(true);
        private ListDrawerConfigInfo info;
        private string errorMessage;

        private Action<object[]> onValuesCreated;
        private Action superHackyAddFunctionWeSeriouslyNeedANewListDrawer;

        bool IHackyListDrawerInteractions.CanCreateValuesToAdd { get { return this.info.GetCustomAddFunctionVoid == null; } }

        void IHackyListDrawerInteractions.CreateValuesToAdd(Action<object[]> onCreated, Rect potentialPopupPosition)
        {
            this.onValuesCreated = onCreated;
            this.StartCreatingValues(potentialPopupPosition);
        }

        private class FilteredPropertyChildren
        {
            public InspectorProperty Property;
            public PropertyChildren Children;
            public PropertySearchFilter SearchFilter;

            private List<InspectorProperty> FilteredChildren;

            public bool IsCurrentlyFiltered { get { return this.FilteredChildren != null; } }

            public int Count
            {
                get
                {
                    if (this.FilteredChildren == null) return this.Children.Count;
                    return this.FilteredChildren.Count;
                }
            }

            public InspectorProperty this[int index]
            {
                get
                {
                    if (this.FilteredChildren == null) return this.Children[index];
                    return this.FilteredChildren[index];
                }
            }

            public FilteredPropertyChildren(InspectorProperty property, PropertySearchFilter searchFilter)
            {
                this.Property = property;
                this.Children = property.Children;
                this.SearchFilter = searchFilter;
            }

            public void Update()
            {
                if (this.SearchFilter == null || string.IsNullOrEmpty(this.SearchFilter.SearchTerm))
                {
                    this.FilteredChildren = null;
                    return;
                }

                if (this.FilteredChildren != null)
                {
                    this.FilteredChildren.Clear();
                }
                else
                {
                    this.FilteredChildren = new List<InspectorProperty>();
                }

                for (int i = 0; i < this.Children.Count; i++)
                {
                    var child = this.Children[i];

                    if (this.SearchFilter.IsMatch(child, this.SearchFilter.SearchTerm))
                    {
                        this.FilteredChildren.Add(child);
                    }
                    else if (this.SearchFilter.Recursive)
                    {
                        foreach (var recursiveChild in child.Children.Recurse())
                        {
                            if (this.SearchFilter.IsMatch(recursiveChild, this.SearchFilter.SearchTerm))
                            {
                                this.FilteredChildren.Add(child);
                                break;
                            }
                        }
                    }
                }
            }

            public void ScheduleUpdate()
            {
                this.Property.Tree.DelayActionUntilRepaint(() =>
                {
                    this.Update();
                    GUIHelper.RequestRepaint();
                });
            }
        }

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.ChildResolver is ICollectionResolver;
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (property.ValueEntry.WeakSmartValue == null)
            {
                return;
            }

            var resolver = property.ChildResolver as ICollectionResolver;

            bool isReadOnly = resolver.IsReadOnly;

            var config = property.GetAttribute<ListDrawerSettingsAttribute>();
            bool isEditable = isReadOnly == false && property.ValueEntry.IsEditable && (config == null || (!config.IsReadOnlyHasValue) || (config.IsReadOnlyHasValue && config.IsReadOnly == false));
            bool pasteElement = isEditable && Clipboard.CanPaste(resolver.ElementType);
            bool clearList = isEditable && property.Children.Count > 0;
            bool setCollectionLength = isEditable && property.ChildResolver is IOrderedCollectionResolver && typeof(IList).IsAssignableFrom(typeof(T));

            //if (genericMenu.GetItemCount() > 0 && (pasteElement || clearList))
            //{
            //    genericMenu.AddSeparator(null);
            //}

            var windowWidth = 300;
            var rect = property.LastDrawnValueRect.AlignTop(1);
            rect.y += 14;
            rect.xMin += rect.width * 0.5f - (windowWidth * 0.5f);
            rect.position = GUIUtility.GUIToScreenPoint(rect.position);
            rect.width = 1;

            if (setCollectionLength)
            {
                if (this.info.GetCustomAddFunctionVoid != null)
                {
                    genericMenu.AddDisabledItem(new GUIContent("Set Collection Size - disabled by 'void " + info.CustomListDrawerOptions.CustomAddFunction + "'"));
                }
                else
                {
                    genericMenu.AddItem(new GUIContent("Set Collection Size"), false, () =>
                    {
                        EditorWindow window = null;

                        Action cancel = () =>
                        {
                            UnityEditorEventUtility.EditorApplication_delayCall += window.Close;
                        };

                        Action<int> confirm = (size) =>
                        {
                            UnityEditorEventUtility.EditorApplication_delayCall += window.Close;
                            SetCollectionSize(property, size);
                        };

                        var sizer = new CollectionSizeDialogue(confirm, cancel, property.ChildResolver.MaxChildCountSeen);
                        window = OdinEditorWindow.InspectObjectInDropDown(sizer, rect, windowWidth);
                        GUIHelper.RequestRepaint();
                    });
                }
            }

            if (pasteElement)
            {
                genericMenu.AddItem(new GUIContent("Paste Element"), false, () =>
                {
                    (property.ChildResolver as ICollectionResolver).QueueAdd(new object[] { Clipboard.Paste() });
                    GUIHelper.RequestRepaint();
                });
            }

            if (clearList)
            {
                genericMenu.AddSeparator("");
                genericMenu.AddItem(new GUIContent("Clear Collection"), false, () =>
                {
                    (property.ChildResolver as ICollectionResolver).QueueClear();
                    GUIHelper.RequestRepaint();
                });
            }
            else
            {
                genericMenu.AddSeparator("");
                genericMenu.AddDisabledItem(new GUIContent("Clear Collection"));
            }
        }

        private void SetCollectionSize(InspectorProperty p, int targetSize)
        {
            var resolver = p.ChildResolver as IOrderedCollectionResolver;

            for (int i = 0; i < p.ParentValues.Count; i++)
            {
                var collection = p.ValueEntry.WeakValues[i] as IList;
                var size = collection.Count;
                var delta = Math.Abs(targetSize - size);

                if (targetSize > size)
                {
                    for (int j = 0; j < delta; j++)
                    {
                        var value = this.GetValueToAdd(i);
                        resolver.QueueAdd(value, i);
                    }
                }
                else
                {
                    for (int j = 0; j < delta; j++)
                    {
                        resolver.QueueRemoveAt(size - (1 + j), i);
                    }
                }
            }
        }

        private object GetValueToAdd(int selectionIndex)
        {
            bool wasFallback;
            return this.GetValueToAdd(selectionIndex, out wasFallback);
        }

        private object GetValueToAdd(int selectionIndex, out bool wasFallback)
        {
            wasFallback = false;

            if (this.info.GetCustomAddFunction != null)
            {
                return this.info.GetCustomAddFunction.GetWeakValue(selectionIndex);
            }
            else if (this.info.CustomListDrawerOptions.AlwaysAddDefaultValue)
            {
                if (!this.info.Property.ValueEntry.SerializationBackend.SupportsPolymorphism)
                {
                    return UnitySerializationUtility.CreateDefaultUnityInitializedObject(this.info.CollectionResolver.ElementType);
                }
                else if (this.info.CollectionResolver.ElementType.IsValueType)
                {
                    return Activator.CreateInstance(this.info.CollectionResolver.ElementType);
                }
                else
                {
                    return null;
                }
            }
            else if (this.info.CustomListDrawerOptions.AddCopiesLastElement && this.info.Count > 0)
            {
                object lastObject = null;
                var lastElementProperty = this.info.FilteredChildren[this.info.Count - 1].ValueEntry;

                var collection = this.info.Property.ValueEntry.WeakValues[selectionIndex] as IEnumerable;
                if (collection != null)
                {
                    // Yes, it's intended.
                    foreach (var item in collection)
                    {
                        lastObject = item;
                    }
                }
                else
                {
                    lastObject = lastElementProperty.WeakValues[selectionIndex];
                }

                return SerializationUtility.CreateCopy(lastObject);
            }
            else if (this.info.CollectionResolver.ElementType.InheritsFrom<UnityEngine.Object>() && Event.current.modifiers == EventModifiers.Control)
            {
                return null;
            }

            wasFallback = true;
            var elementType = (this.Property.ChildResolver as ICollectionResolver).ElementType;
            if (!this.ValueEntry.SerializationBackend.SupportsPolymorphism)
            {
                return UnitySerializationUtility.CreateDefaultUnityInitializedObject(elementType);
            }
            else
            {
                return elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
            }
        }

        /// <summary>
        /// Initializes the drawer.
        /// </summary>
        protected override void Initialize()
        {
            var resolver = this.Property.ChildResolver as ICollectionResolver;
            bool isReadOnly = resolver.IsReadOnly;

            var customListDrawerOptions = this.Property.GetAttribute<ListDrawerSettingsAttribute>() ?? new ListDrawerSettingsAttribute();
            isReadOnly = this.ValueEntry.IsEditable == false || isReadOnly || customListDrawerOptions.IsReadOnlyHasValue && customListDrawerOptions.IsReadOnly;

            PropertySearchFilter searchFilter = null;

            var searchAttr = this.Property.GetAttribute<SearchableAttribute>();

            if (searchAttr != null)
            {
                searchFilter = new PropertySearchFilter(null, searchAttr);
            }

            if (customListDrawerOptions.ExpandedHasValue)
            {
                this.Property.State.Expanded = customListDrawerOptions.Expanded;
            }

            this.info = new ListDrawerConfigInfo()
            {
                StartIndex = 0,
                //Toggled = this.ValueEntry.Context.GetPersistent<bool>(this, "ListDrawerToggled", customListDrawerOptions.ExpandedHasValue ? customListDrawerOptions.Expanded : GeneralDrawerConfig.Instance.OpenListsByDefault),
                RemoveAt = -1,

                // Now set further down, so it can be kept updated every frame
                //Label = new GUIContent(label == null || string.IsNullOrEmpty(label.text) ? this.Property.ValueEntry.TypeOfValue.GetNiceName() : label.text, label == null ? string.Empty : label.tooltip),
                ShowAllWhilePaging = false,
                EndIndex = 0,
                CustomListDrawerOptions = customListDrawerOptions,
                BaseIsReadOnly = isReadOnly,
                BaseDraggable = !isReadOnly, // && (!customListDrawerOptions.IsReadOnlyHasValue),
                HideAddButton = isReadOnly || customListDrawerOptions.HideAddButton,
                HideRemoveButton = isReadOnly || customListDrawerOptions.HideRemoveButton,
                FilteredChildren = new FilteredPropertyChildren(this.Property, searchFilter)
            };

            this.info.ListConfig = GeneralDrawerConfig.Instance;
            this.info.Property = this.Property;

            if (customListDrawerOptions.DraggableHasValue && !customListDrawerOptions.DraggableItems)
            {
                this.info.BaseDraggable = false;
            }

            if (!(this.Property.ChildResolver is IOrderedCollectionResolver))
            {
                this.info.BaseDraggable = false;
            }

            if (this.info.CustomListDrawerOptions.OnBeginListElementGUI != null)
            {
                this.info.OnBeginListElementGUI = ActionResolver.Get(this.Property, this.info.CustomListDrawerOptions.OnBeginListElementGUI, new ActionResolvers.NamedValue("index", typeof(int)));
            }

            if (this.info.CustomListDrawerOptions.OnEndListElementGUI != null)
            {
                this.info.OnEndListElementGUI = ActionResolver.Get(this.Property, this.info.CustomListDrawerOptions.OnEndListElementGUI, new ActionResolvers.NamedValue("index", typeof(int)));
            }

            if (this.info.CustomListDrawerOptions.OnTitleBarGUI != null)
            {
                this.info.OnTitleBarGUI = ActionResolver.Get(this.Property, this.info.CustomListDrawerOptions.OnTitleBarGUI);
            }

            if (this.info.CustomListDrawerOptions.ListElementLabelName != null)
            {
                // This is one of those very rare cases where the value resolver system just doesn't quite cut it or fit the pattern.
                // But we do want to at least support expressions. So we do some fancy custom stuff here to reach rough feature parity.
                this.info.GetListElementLabelText = CreateListElementLabelNameGetter(this.info.CustomListDrawerOptions.ListElementLabelName, resolver.ElementType, ref this.errorMessage);
            }

            if (this.info.CustomListDrawerOptions.CustomAddFunction != null)
            {
                this.info.GetCustomAddFunction = ValueResolver.Get(resolver.ElementType, this.Property, this.info.CustomListDrawerOptions.CustomAddFunction);

                if (this.info.GetCustomAddFunction.HasError)
                {
                    this.info.GetCustomAddFunctionVoid = ActionResolver.Get(this.Property, this.info.CustomListDrawerOptions.CustomAddFunction);

                    if (!this.info.GetCustomAddFunctionVoid.HasError)
                    {
                        // Wipe out the former error, since we found a proper void/action overload
                        this.info.GetCustomAddFunction = null;
                    }
                }
            }

            // Resolve custom remove index method member reference.
            if (this.info.CustomListDrawerOptions.CustomRemoveIndexFunction != null)
            {
                if (this.Property.ChildResolver is IOrderedCollectionResolver == false)
                {
                    if (this.errorMessage != null) this.errorMessage += "\n\n";
                    this.errorMessage += "ListDrawerSettings.CustomRemoveIndexFunction is invalid on unordered collections. Use ListDrawerSetings.CustomRemoveElementFunction instead.";
                }
                else
                {
                    this.info.CustomRemoveIndexFunction = ActionResolver.Get(this.Property, this.info.CustomListDrawerOptions.CustomRemoveIndexFunction, new ActionResolvers.NamedValue("index", typeof(int)));
                }
            }
            // Resolve custom remove element method member reference.
            else if (this.info.CustomListDrawerOptions.CustomRemoveElementFunction != null)
            {
                this.info.CustomRemoveElementFunction = ActionResolver.Get(this.Property, this.info.CustomListDrawerOptions.CustomRemoveElementFunction, new ActionResolvers.NamedValue("removeElement", resolver.ElementType));
            }
        }

        private static Func<object, InspectorProperty, object> CreateListElementLabelNameGetter(string resolvedString, Type elementType, ref string errorMessage)
        {
            if (resolvedString.Length > 1 && resolvedString[0] == '@')
            {
                var expression = resolvedString.Substring(1);
                string exprError;

                Type[] parameters = new Type[] { typeof(InspectorProperty) };
                string[] parameterNames = new string[] { "property" };

                var exprDelegate = ExpressionUtility.ParseExpression(expression, false, elementType, parameters, parameterNames, out exprError, true);

                if (exprError != null)
                {
                    if (errorMessage != null) errorMessage += "\n\n";
                    errorMessage += exprError;
                    return null;
                }
                else
                {
                    var exprType = exprDelegate.Method.ReturnType;

                    if (exprType == typeof(void) || exprType == null)
                    {
                        if (errorMessage != null) errorMessage += "\n\n";
                        errorMessage += "ListElementLabelName expression '" + expression + "' is not allowed to evaluate to 'void'.";
                        return null;
                    }
                    else
                    {
                        object[] exprParameters = new object[2];

                        return (object instance, InspectorProperty property) =>
                        {
                            exprParameters[0] = instance;
                            exprParameters[1] = property;

                            return exprDelegate.DynamicInvoke(exprParameters);
                        };
                    }
                }
            }
            else
            {
                var fieldInfo = elementType.GetField(resolvedString, Flags.AllMembers);

                if (fieldInfo != null)
                {
                    if (fieldInfo.IsStatic)
                    {
                        return (instance, property) => fieldInfo.GetValue(null);
                    }
                    else
                    {
                        return (instance, property) => fieldInfo.GetValue(instance);
                    }
                }

                var propertyInfo = elementType.GetProperty(resolvedString, Flags.AllMembers);

                if (propertyInfo != null)
                {
                    if (propertyInfo.IsStatic())
                    {
                        return (instance, property) => propertyInfo.GetValue(null, null);
                    }
                    else
                    {
                        return (instance, property) => propertyInfo.GetValue(instance, null);
                    }
                }

                var methodInfo = elementType.GetMethod(resolvedString, Flags.AllMembers, null, Type.EmptyTypes, null);

                if (methodInfo != null)
                {
                    if (methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == null)
                    {
                        if (errorMessage != null) errorMessage += "\n\n";
                        errorMessage += "ListElementLabelName method '" + resolvedString + "' on element type '" + elementType.GetNiceName() + "' is not allowed to return void.";
                        return null;
                    }
                    else
                    {
                        if (methodInfo.IsStatic)
                        {
                            return (instance, prop) => methodInfo.Invoke(null, null);
                        }
                        else
                        {
                            return (instance, prop) => methodInfo.Invoke(instance, null);
                        }
                    }
                }
                else
                {
                    if (errorMessage != null) errorMessage += "\n\n";
                    errorMessage += "Couldn't find any field, property or parameterless method named '" + resolvedString + "' on element type '" + elementType.GetNiceName() + "' to use for ListElementLabelName.";
                    return null;
                }
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var resolver = this.Property.ChildResolver as ICollectionResolver;
            bool isReadOnly = resolver.IsReadOnly;

            if (this.errorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.errorMessage);
            }

            ActionResolver.DrawErrors(
                this.info.OnTitleBarGUI,
                this.info.GetCustomAddFunctionVoid,
                this.info.CustomRemoveIndexFunction,
                this.info.CustomRemoveElementFunction,
                this.info.OnBeginListElementGUI,
                this.info.OnEndListElementGUI);

            ValueResolver.DrawErrors(
                this.info.GetCustomAddFunction);

            if (this.info.Label == null || (label != null && label.text != this.info.Label.text))
            {
                this.info.Label = new GUIContent(label == null || string.IsNullOrEmpty(label.text) ? this.Property.ValueEntry.TypeOfValue.GetNiceName() : label.text, label == null ? string.Empty : label.tooltip);
            }

            this.info.BaseIsReadOnly = resolver.IsReadOnly;

            this.info.ListItemStyle.padding.left = this.info.Draggable ? 25 : 7;
            this.info.ListItemStyle.padding.right = this.info.BaseIsReadOnly || this.info.HideRemoveButton ? 4 : 20;

            if (Event.current.type == EventType.Repaint)
            {
                this.info.DropZoneTopLeft = GUIUtility.GUIToScreenPoint(new Vector2(0, 0));
            }

            this.info.CollectionResolver = this.Property.ChildResolver as ICollectionResolver;
            this.info.OrderedCollectionResolver = this.Property.ChildResolver as IOrderedCollectionResolver;

            this.info.Count = this.info.FilteredChildren.Count;
            this.info.IsEmpty = this.info.FilteredChildren.Count == 0;
            
            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            this.BeginDropZone();
            {
                this.DrawToolbar();
                if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(this.Property, this), this.Property.State.Expanded))
                {
                    GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth - this.info.ListItemStyle.padding.left);
                    this.DrawItems();
                    GUIHelper.PopLabelWidth();
                }
                SirenixEditorGUI.EndFadeGroup();
            }
            this.EndDropZone();
            SirenixEditorGUI.EndIndentedVertical();

            if (this.info.OrderedCollectionResolver != null)
            {
                if (this.info.RemoveAt >= 0 && Event.current.type == EventType.Repaint)
                {
                    try
                    {
                        if (this.info.CustomRemoveIndexFunction != null && !this.info.CustomRemoveIndexFunction.HasError)
                        {
                            this.Property.RecordForUndo("Custom List Remove (Index '" + this.info.RemoveAt + "')");
                            this.info.CustomRemoveIndexFunction.Context.NamedValues.Set("index", this.info.RemoveAt);
                            this.info.CustomRemoveIndexFunction.DoActionForAllSelectionIndices();
                            this.Property.MarkSerializationRootDirty();
                        }
                        else if (this.info.CustomRemoveElementFunction != null && !this.info.CustomRemoveElementFunction.HasError)
                        {
                            this.Property.RecordForUndo("Custom List Remove (Element)");

                            for (int i = 0; i < this.Property.ParentValues.Count; i++)
                            {
                                this.info.CustomRemoveElementFunction.Context.NamedValues.Set("removeElement", this.Property.Children[this.info.RemoveAt].ValueEntry.WeakValues[i]);
                                this.info.CustomRemoveElementFunction.DoAction(i);
                            }

                            this.Property.MarkSerializationRootDirty();
                        }
                        else
                        {
                            this.info.OrderedCollectionResolver.QueueRemoveAt(this.info.RemoveAt);
                        }
                    }
                    finally
                    {
                        this.info.RemoveAt = -1;
                        this.info.FilteredChildren.ScheduleUpdate();
                    }

                    GUIHelper.RequestRepaint();
                }
            }
            else if (this.info.RemoveValues != null && Event.current.type == EventType.Repaint)
            {
                try
                {
                    if (this.info.CustomRemoveElementFunction != null && !this.info.CustomRemoveElementFunction.HasError)
                    {
                        for (int i = 0; i < this.Property.ParentValues.Count; i++)
                        {
                            this.info.CustomRemoveElementFunction.Context.NamedValues.Set("removeElement", this.info.RemoveValues[i]);
                            this.info.CustomRemoveElementFunction.DoAction(i);
                        }
                    }
                    else
                    {
                        this.info.CollectionResolver.QueueRemove(this.info.RemoveValues);
                    }
                }
                finally
                {
                    this.info.RemoveValues = null;
                    this.info.FilteredChildren.ScheduleUpdate();
                }
                GUIHelper.RequestRepaint();
            }

            if (this.info.ObjectPicker != null && this.info.ObjectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
            {
                var value = this.info.ObjectPicker.ClaimObject();

                if (this.info.JumpToNextPageOnAdd)
                {
                    this.info.StartIndex = int.MaxValue;
                }

                object[] values = new object[this.info.Property.Tree.WeakTargets.Count];

                values[0] = value;
                for (int j = 1; j < values.Length; j++)
                {
                    values[j] = SerializationUtility.CreateCopy(value);
                }

                if (this.onValuesCreated != null)
                {
                    this.onValuesCreated(values);
                    this.onValuesCreated = null;
                }
                else
                {
                    this.info.CollectionResolver.QueueAdd(values);
                }
            }
        }

        private DropZoneHandle BeginDropZone()
        {
            if (this.info.OrderedCollectionResolver == null) return null;

            var dropZone = DragAndDropManager.BeginDropZone(this.info.Property.Tree.GetHashCode() + "-" + this.info.Property.Path, this.info.CollectionResolver.ElementType, true);

            if (Event.current.type == EventType.Repaint && DragAndDropManager.IsDragInProgress)
            {
                var rect = dropZone.Rect;
                dropZone.Rect = rect;
            }

            dropZone.Enabled = this.info.IsReadOnly == false;
            this.info.DropZone = dropZone;
            return dropZone;
        }

        private static UnityEngine.Object[] HandleUnityObjectsDrop(ListDrawerConfigInfo info)
        {
            if (info.IsReadOnly) return null;

            var eventType = Event.current.type;
            if (eventType == EventType.Layout)
            {
                info.IsAboutToDroppingUnityObjects = false;
            }
            if ((eventType == EventType.DragUpdated || eventType == EventType.DragPerform) && info.DropZone.Rect.Contains(Event.current.mousePosition))
            {
                UnityEngine.Object[] objReferences = null;

                if (DragAndDrop.objectReferences.Any(n => n != null && info.CollectionResolver.ElementType.IsAssignableFrom(n.GetType())))
                {
                    objReferences = DragAndDrop.objectReferences.Where(x => x != null && info.CollectionResolver.ElementType.IsAssignableFrom(x.GetType())).Reverse().ToArray();
                }
                else if (info.CollectionResolver.ElementType.InheritsFrom(typeof(Component)))
                {
                    objReferences = DragAndDrop.objectReferences.OfType<GameObject>().Select(x => x.GetComponent(info.CollectionResolver.ElementType)).Where(x => x != null).Reverse().ToArray();
                }
                else if (info.CollectionResolver.ElementType.InheritsFrom(typeof(Sprite)) && DragAndDrop.objectReferences.Any(n => n is Texture2D && AssetDatabase.Contains(n)))
                {
                    objReferences = DragAndDrop.objectReferences.OfType<Texture2D>().Select(x =>
                    {
                        var path = AssetDatabase.GetAssetPath(x);
                        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }).Where(x => x != null).Reverse().ToArray();
                }

                bool acceptsDrag = objReferences != null && objReferences.Length > 0;

                if (acceptsDrag)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                    info.IsAboutToDroppingUnityObjects = true;
                    info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        return objReferences;
                    }
                }
            }
            if (eventType == EventType.Repaint)
            {
                info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
            }
            return null;
        }

        private void EndDropZone()
        {
            if (this.info.OrderedCollectionResolver == null) return;

            if (this.info.DropZone.IsReadyToClaim)
            {
                if (info.InsertAt == -1)
                {
                    info.InsertAt = info.FilteredChildren.Count;
                }

                CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
                CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo = this.info.Property;
                object droppedObject = this.info.DropZone.ClaimObject();

                object[] values = new object[this.info.Property.Tree.WeakTargets.Count];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = droppedObject;
                }

                if (this.info.DropZone.IsCrossWindowDrag)
                {
                    // If it's a cross-window drag, the changes will for some reason be lost if we don't do this.
                    GUIHelper.RequestRepaint();
                    UnityEditorEventUtility.EditorApplication_delayCall += () =>
                    {
                        this.info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(this.info.InsertAt, 0, this.info.FilteredChildren.Count), values);
                    };
                }
                else
                {
                    this.info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(this.info.InsertAt, 0, this.info.FilteredChildren.Count), values);
                }
            }
            else if (this.info.IsReadOnly == false)
            {
                UnityEngine.Object[] droppedObjects = HandleUnityObjectsDrop(this.info);
                if (droppedObjects != null)
                {
                    if (info.InsertAt == -1)
                    {
                        info.InsertAt = info.FilteredChildren.Count;
                    }

                    foreach (var obj in droppedObjects)
                    {
                        object[] values = new object[this.info.Property.Tree.WeakTargets.Count];

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = obj;
                        }

                        this.info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(this.info.InsertAt, 0, this.info.FilteredChildren.Count), values);
                    }
                }
            }
            DragAndDropManager.EndDropZone();
        }

        private void DrawToolbar()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                // Label
                if (this.info.DropZone != null && DragAndDropManager.IsDragInProgress && this.info.DropZone.IsAccepted == false)
                {
                    GUIHelper.PushGUIEnabled(false);
                }

                if (this.info.Property.ValueEntry.ListLengthChangedFromPrefab)
                {
                    GUIHelper.PushIsBoldLabel(true);
                }

                bool drawFoldout = !(this.info.ListConfig.HideFoldoutWhileEmpty && this.info.IsEmpty || this.info.CustomListDrawerOptions.Expanded);

                Rect foldoutRect = default(Rect);

                if (!drawFoldout)
                {
                    GUILayout.Label(this.info.Label, GUILayoutOptions.ExpandWidth(false));
                }
                else
                {
                    var tmp = EditorGUIUtility.fieldWidth;
                    EditorGUIUtility.fieldWidth = 10;
                    foldoutRect = EditorGUILayout.GetControlRect(false);
                    EditorGUIUtility.fieldWidth = tmp;
                }

                if (this.info.Property.ValueEntry.ListLengthChangedFromPrefab)
                {
                    GUIHelper.PopIsBoldLabel();
                }

                //if (this.info.CustomListDrawerOptions.Expanded)
                //{
                //    //this.info.Toggled.Value = true; // Really?
                //    this.Property.State.Expanded = true;
                //}

                if (this.info.DropZone != null && DragAndDropManager.IsDragInProgress && this.info.DropZone.IsAccepted == false)
                {
                    GUIHelper.PopGUIEnabled();
                }

                GUILayout.FlexibleSpace();

                // Search field
                if (this.info.FilteredChildren.SearchFilter != null)
                {
                    var rect = EditorGUILayout.GetControlRect(false).AddYMin(2);

                    if (UnityVersion.IsVersionOrGreater(2019, 3))
                    {
                        rect = rect.AddY(-2);
                    }

                    var newTerm = SirenixEditorGUI.SearchField(rect, this.info.FilteredChildren.SearchFilter.SearchTerm, false, this.info.SearchFieldControlName);

                    if (newTerm != this.info.FilteredChildren.SearchFilter.SearchTerm)
                    {
                        if (!string.IsNullOrEmpty(newTerm))
                        {
                            //this.info.Toggled.Value = true;
                            this.Property.State.Expanded = true;
                        }

                        this.info.FilteredChildren.SearchFilter.SearchTerm = newTerm;
                        this.info.FilteredChildren.ScheduleUpdate();
                    }
                }

                // Actually draw the foldout at the allocated rect, now that it doesn't 
                //   affect the search field control ID, since it's drawn after
                if (drawFoldout)
                {
                    //this.info.Toggled.Value = SirenixEditorGUI.Foldout(foldoutRect, this.info.Toggled.Value, this.info.Label ?? GUIContent.none);
                    this.Property.State.Expanded = SirenixEditorGUI.Foldout(foldoutRect, this.Property.State.Expanded, this.info.Label ?? GUIContent.none);
                }

                //GUILayout.FlexibleSpace();

                // Item Count
                if (this.info.CustomListDrawerOptions.ShowItemCountHasValue ? this.info.CustomListDrawerOptions.ShowItemCount : this.info.ListConfig.ShowItemCount)
                {
                    if (this.info.Property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
                    {
                        GUILayout.Label(this.info.Count + " / " + this.info.CollectionResolver.MaxCollectionLength + " items", SirenixGUIStyles.CenteredGreyMiniLabel);
                    }
                    else if (this.info.FilteredChildren.IsCurrentlyFiltered)
                    {
                        GUILayout.Label(this.info.Count + " / " + this.info.Property.Children.Count + " items", EditorStyles.centeredGreyMiniLabel);
                    }
                    else
                    {
                        GUILayout.Label(this.info.IsEmpty ? "Empty" : this.info.Count + " items", SirenixGUIStyles.CenteredGreyMiniLabel);
                    }
                }

                bool paging = this.info.CustomListDrawerOptions.PagingHasValue ? this.info.CustomListDrawerOptions.ShowPaging : true;
                bool hidePaging =
                        //this.info.ListConfig.HidePagingWhileCollapsed && this.info.Toggled.Value == false ||
                        this.info.ListConfig.HidePagingWhileCollapsed && this.Property.State.Expanded == false ||
                        this.info.ListConfig.HidePagingWhileOnlyOnePage && this.info.Count <= this.info.NumberOfItemsPerPage;

                int numberOfItemsPrPage = Math.Max(1, this.info.NumberOfItemsPerPage);
                int numberOfPages = Mathf.CeilToInt(this.info.Count / (float)numberOfItemsPrPage);
                int pageIndex = this.info.Count == 0 ? 0 : (this.info.StartIndex / numberOfItemsPrPage) % this.info.Count;

                // Paging
                if (paging)
                {
                    //bool disablePaging = paging && !hidePaging && (DragAndDropManager.IsDragInProgress || this.info.ShowAllWhilePaging || this.info.Toggled.Value == false);
                    bool disablePaging = paging && !hidePaging && (DragAndDropManager.IsDragInProgress || this.info.ShowAllWhilePaging || this.Property.State.Expanded == false);
                    if (disablePaging)
                    {
                        GUIHelper.PushGUIEnabled(false);
                    }

                    if (!hidePaging)
                    {
                        if (pageIndex == 0) { GUIHelper.PushGUIEnabled(false); }

                        if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft, true))
                        {
                            if (Event.current.button == 0)
                            {
                                this.info.StartIndex -= numberOfItemsPrPage;
                            }
                            else
                            {
                                this.info.StartIndex = 0;
                            }
                        }
                        if (pageIndex == 0) { GUIHelper.PopGUIEnabled(); }

                        var userPageIndex = EditorGUILayout.IntField((numberOfPages == 0 ? 0 : (pageIndex + 1)), GUILayoutOptions.Width(10 + numberOfPages.ToString(CultureInfo.InvariantCulture).Length * 10)) - 1;
                        if (pageIndex != userPageIndex)
                        {
                            this.info.StartIndex = userPageIndex * numberOfItemsPrPage;
                        }

                        GUILayout.Label("/ " + numberOfPages);

                        if (pageIndex == numberOfPages - 1) { GUIHelper.PushGUIEnabled(false); }

                        if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight, true))
                        {
                            if (Event.current.button == 0)
                            {
                                this.info.StartIndex += numberOfItemsPrPage;
                            }
                            else
                            {
                                this.info.StartIndex = numberOfItemsPrPage * numberOfPages;
                            }
                        }
                        if (pageIndex == numberOfPages - 1) { GUIHelper.PopGUIEnabled(); }
                    }

                    pageIndex = this.info.Count == 0 ? 0 : (this.info.StartIndex / numberOfItemsPrPage) % this.info.Count;

                    var newStartIndex = Mathf.Clamp(pageIndex * numberOfItemsPrPage, 0, Mathf.Max(0, this.info.Count - 1));
                    if (newStartIndex != this.info.StartIndex)
                    {
                        this.info.StartIndex = newStartIndex;
                        var newPageIndex = this.info.Count == 0 ? 0 : (this.info.StartIndex / numberOfItemsPrPage) % this.info.Count;
                        if (pageIndex != newPageIndex)
                        {
                            pageIndex = newPageIndex;
                            this.info.StartIndex = Mathf.Clamp(pageIndex * numberOfItemsPrPage, 0, Mathf.Max(0, this.info.Count - 1));
                        }
                    }

                    this.info.EndIndex = Mathf.Min(this.info.StartIndex + numberOfItemsPrPage, this.info.Count);

                    if (disablePaging)
                    {
                        GUIHelper.PopGUIEnabled();
                    }
                }
                else
                {
                    this.info.StartIndex = 0;
                    this.info.EndIndex = this.info.Count;
                }

                if (paging && hidePaging == false && this.info.ListConfig.ShowExpandButton)
                {
                    if (this.info.Count < 300)
                    {
                        if (SirenixEditorGUI.ToolbarButton(this.info.ShowAllWhilePaging ? EditorIcons.TriangleUp : EditorIcons.TriangleDown, true))
                        {
                            this.info.ShowAllWhilePaging = !this.info.ShowAllWhilePaging;
                        }
                    }
                    else
                    {
                        this.info.ShowAllWhilePaging = false;
                    }
                }

                // Add Button
                if (this.info.IsReadOnly == false && !this.info.HideAddButton)
                {
                    this.info.ObjectPicker = ObjectPicker.GetObjectPicker(this.info, this.info.CollectionResolver.ElementType);
                    this.superHackyAddFunctionWeSeriouslyNeedANewListDrawer = CollectionDrawerStaticInfo.NextCustomAddFunction;
                    CollectionDrawerStaticInfo.NextCustomAddFunction = null;

                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
                    {
                        this.StartCreatingValues(GUIHelper.GetCurrentLayoutRect());
                    }

                    this.info.JumpToNextPageOnAdd = paging && (this.info.Count % numberOfItemsPrPage == 0) && (pageIndex + 1 == numberOfPages);
                }

                if (this.info.OnTitleBarGUI != null && !this.info.OnTitleBarGUI.HasError)
                {
                    this.info.OnTitleBarGUI.DoAction();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void StartCreatingValues(Rect potentialPopupPosition)
        {
            if (this.superHackyAddFunctionWeSeriouslyNeedANewListDrawer != null)
            {
                this.superHackyAddFunctionWeSeriouslyNeedANewListDrawer();
            }
            else if (this.info.GetCustomAddFunctionVoid != null && !this.info.GetCustomAddFunctionVoid.HasError)
            {
                this.info.GetCustomAddFunctionVoid.DoAction();

                var root = this.Property.SerializationRoot.ValueEntry.WeakValues[0] as UnityEngine.Object;

                if (root != null)
                {
                    InspectorUtilities.RegisterUnityObjectDirty(root);
                }
            }
            else
            {
                object[] objs = new object[this.info.Property.ValueEntry.ValueCount];

                bool wasFallback;

                objs[0] = this.GetValueToAdd(0, out wasFallback);

                if (wasFallback)
                {
                    this.info.ObjectPicker.ShowObjectPicker(
                        null,
                        this.info.Property.GetAttribute<AssetsOnlyAttribute>() == null,
                        potentialPopupPosition,
                        !this.info.Property.ValueEntry.SerializationBackend.SupportsPolymorphism);
                }
                else
                {
                    for (int i = 1; i < objs.Length; i++)
                    {
                        objs[i] = this.GetValueToAdd(i);
                    }

                    if (this.onValuesCreated != null)
                    {
                        this.onValuesCreated(objs);
                        this.onValuesCreated = null;
                    }
                    else
                    {
                        this.info.CollectionResolver.QueueAdd(objs);
                    }
                }
            }
        }

        private void DrawItems()
        {
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            {
                this.info.DraggingMousePosition = Event.current.mousePosition;
            }

            info.InsertAt = -1;
            int from = 0;
            int to = this.info.Count;
            bool paging = this.info.CustomListDrawerOptions.PagingHasValue ? this.info.CustomListDrawerOptions.ShowPaging : true;
            if (paging && this.info.ShowAllWhilePaging == false)
            {
                from = Mathf.Clamp(this.info.StartIndex, 0, this.info.Count);
                to = Mathf.Clamp(this.info.EndIndex, 0, this.info.Count);
            }

            var drawEmptySpace = this.info.DropZone != null && this.info.DropZone.IsBeingHovered || this.info.IsDroppingUnityObjects;
            float height = drawEmptySpace ? this.info.IsDroppingUnityObjects ? 16 : (DragAndDropManager.CurrentDraggingHandle.Rect.height) : 0;
            var rect = SirenixEditorGUI.BeginVerticalList();
            {
                for (int i = 0, j = from, k = from; j < to; i++, j++)
                {
                    var dragHandle = this.BeginDragHandle(j, i);
                    {
                        if (drawEmptySpace)
                        {
                            var topHalf = dragHandle.Rect;
                            topHalf.height /= 2;
                            if (topHalf.Contains(this.info.DraggingMousePosition) || topHalf.y > this.info.DraggingMousePosition.y && i == 0)
                            {
                                GUILayout.Space(height);
                                drawEmptySpace = false;
                                this.info.InsertAt = k;
                            }
                        }

                        if (dragHandle.IsDragging == false)
                        {
                            k++;
                            this.DrawItem(this.info.FilteredChildren[j], dragHandle);
                        }
                        else
                        {
                            //GUILayout.Space(3);
                            if (Event.current.type == EventType.Repaint && this.info.InsertAt != j)
                            {
                                var localJ = j;
                                var localInfo = this.info;
                                var p = GUIUtility.GUIToScreenPoint(new Vector2(dragHandle.Rect.x, dragHandle.Rect.y));
                                var r = dragHandle.Rect;
                                this.Property.Tree.DelayAction(() =>
                                {
                                    p = GUIUtility.ScreenToGUIPoint(p);
                                    r.x = p.x;
                                    r.y = p.y;
                                    if (localInfo.InsertAt > localJ)
                                    {
                                        EditorGUI.DrawRect(r.AlignTop(3).AddY(-3), SirenixGUIStyles.ListItemDragBgColor);
                                    }
                                    else
                                    {
                                        EditorGUI.DrawRect(r.AlignBottom(3).AddY(0), SirenixGUIStyles.ListItemDragBgColor);
                                    }
                                });
                            }

                            CollectionDrawerStaticInfo.DelayedGUIDrawer.Begin(dragHandle.Rect.width, dragHandle.Rect.height);
                            DragAndDropManager.AllowDrop = false;
                            this.DrawItem(this.info.FilteredChildren[j], dragHandle);
                            DragAndDropManager.AllowDrop = true;
                            CollectionDrawerStaticInfo.DelayedGUIDrawer.End();
                        }

                        if (drawEmptySpace)
                        {
                            var bottomHalf = dragHandle.Rect;
                            bottomHalf.height /= 2;
                            bottomHalf.y += bottomHalf.height;

                            if (bottomHalf.Contains(this.info.DraggingMousePosition) || bottomHalf.yMax < this.info.DraggingMousePosition.y && j + 1 == to)
                            {
                                GUILayout.Space(height);
                                drawEmptySpace = false;
                                this.info.InsertAt = Mathf.Min(k, to);
                            }
                        }
                    }
                    this.EndDragHandle(i);
                }

                if (drawEmptySpace)
                {
                    GUILayout.Space(height);
                    this.info.InsertAt = this.info.DraggingMousePosition.y > rect.center.y ? to : from;
                }

                if (to == this.info.FilteredChildren.Count && this.info.Property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
                {
                    SirenixEditorGUI.BeginListItem(false);
                    GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.centeredGreyMiniLabel);
                    SirenixEditorGUI.EndListItem();
                }
            }
            SirenixEditorGUI.EndVerticalList();
        }

        private void EndDragHandle(int i)
        {
            var handle = DragAndDropManager.EndDragHandle();

            if (handle.IsDragging)
            {
                this.info.Property.Tree.DelayAction(() =>
                {
                    if (DragAndDropManager.CurrentDraggingHandle != null)
                    {
                        CollectionDrawerStaticInfo.DelayedGUIDrawer.Draw(this.info.DraggingMousePosition - DragAndDropManager.CurrentDraggingHandle.MouseDownPostionOffset);
                    }
                });
            }
        }

        private DragHandle BeginDragHandle(int j, int i)
        {
            var child = this.info.FilteredChildren[j];
            var dragHandle = DragAndDropManager.BeginDragHandle(child, child.ValueEntry.WeakSmartValue, this.info.IsReadOnly ? DragAndDropMethods.Reference : DragAndDropMethods.Move);
            dragHandle.Enabled = this.info.Draggable;

            if (dragHandle.OnDragStarted)
            {
                CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo = null;
                CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = this.info.FilteredChildren[j];
                dragHandle.OnDragFinnished = dropEvent =>
                {
                    if (dropEvent == DropEvents.Moved)
                    {
                        if (dragHandle.IsCrossWindowDrag || (CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo != null && CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo.Tree != this.info.Property.Tree))
                        {
                            // Make sure drop happens a bit later, as deserialization and other things sometimes
                            // can override the change.
                            GUIHelper.RequestRepaint();
                            UnityEditorEventUtility.EditorApplication_delayCall += () =>
                            {
                                this.info.OrderedCollectionResolver.QueueRemoveAt(j);
                            };
                        }
                        else
                        {
                            this.info.OrderedCollectionResolver.QueueRemoveAt(j);
                        }
                    }

                    CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
                };
            }

            return dragHandle;
        }

        private Rect DrawItem(InspectorProperty itemProperty, DragHandle dragHandle)
        {
            var index = itemProperty.Index;

            var listItemInfo = itemProperty.Context.GetGlobal<ListItemInfo>("listItemInfo");

            Rect rect;
            rect = SirenixEditorGUI.BeginListItem(false, this.info.ListItemStyle, listItemOptions);
            {
                if (Event.current.type == EventType.Repaint && !this.info.BaseIsReadOnly)
                {
                    listItemInfo.Value.Width = rect.width;
                    dragHandle.DragHandleRect = new Rect(rect.x + 4, rect.y, 20, rect.height);
                    listItemInfo.Value.DragHandleRect = new Rect(rect.x + 4, rect.y + 2 + ((int)rect.height - 23) / 2, 20, 20);
                    listItemInfo.Value.RemoveBtnRect = new Rect(listItemInfo.Value.DragHandleRect.x + rect.width - 22, listItemInfo.Value.DragHandleRect.y + 1, 14, 14);

                    if (this.info.HideRemoveButton == false)
                    {

                    }
                    if (this.info.Draggable)
                    {
                        GUI.Label(listItemInfo.Value.DragHandleRect, EditorIcons.List.Inactive, GUIStyle.none);
                    }
                }

                GUIHelper.PushHierarchyMode(false);
                GUIContent label = null;

                if (this.info.CustomListDrawerOptions.ShowIndexLabelsHasValue)
                {
                    if (this.info.CustomListDrawerOptions.ShowIndexLabels)
                    {
                        label = new GUIContent(index.ToString());
                    }
                }
                else if (this.info.ListConfig.ShowIndexLabels)
                {
                    label = new GUIContent(index.ToString());
                }

                if (this.info.GetListElementLabelText != null)
                {
                    var value = itemProperty.ValueEntry.WeakSmartValue;

                    if (object.ReferenceEquals(value, null)) 
                    {
                        if (label == null)
                        {
                            label = new GUIContent("Null");
                        }
                        else
                        {
                            label.text += " : Null";
                        }
                    }
                    else
                    {
                        label = label ?? new GUIContent("");
                        if (label.text != "") label.text += " : ";
                         
                        object text = this.info.GetListElementLabelText(value, itemProperty);
                        label.text += (text == null ? "" : text.ToString());
                    }
                }

                if (this.info.OnBeginListElementGUI != null && !this.info.OnBeginListElementGUI.HasError)
                {
                    this.info.OnBeginListElementGUI.Context.NamedValues.Set("index", index);
                    this.info.OnBeginListElementGUI.DoAction();
                }
                itemProperty.Draw(label);

                if (this.info.OnEndListElementGUI != null && !this.info.OnEndListElementGUI.HasError)
                {
                    this.info.OnEndListElementGUI.Context.NamedValues.Set("index", index);
                    this.info.OnEndListElementGUI.DoAction();
                }

                GUIHelper.PopHierarchyMode();

                if (this.info.BaseIsReadOnly == false && this.info.HideRemoveButton == false)
                {
                    if (SirenixEditorGUI.IconButton(listItemInfo.Value.RemoveBtnRect, EditorIcons.X))
                    {
                        if (this.info.OrderedCollectionResolver != null)
                        {
                            if (index >= 0)
                            {
                                this.info.RemoveAt = index;
                            }
                        }
                        else
                        {
                            var values = new object[itemProperty.ValueEntry.ValueCount];

                            for (int i = 0; i < values.Length; i++)
                            {
                                values[i] = itemProperty.ValueEntry.WeakValues[i];
                            }

                            this.info.RemoveValues = values;
                        }
                    }
                }
            }
            SirenixEditorGUI.EndListItem();

            return rect;
        }

        private struct ListItemInfo
        {
            public float Width;
            public Rect RemoveBtnRect;
            public Rect DragHandleRect;
        }

        private class ListDrawerConfigInfo
        {
            public ICollectionResolver CollectionResolver;
            public IOrderedCollectionResolver OrderedCollectionResolver;
            public bool IsEmpty;
            public ListDrawerSettingsAttribute CustomListDrawerOptions;
            public int Count;
            public int StartIndex;
            public int EndIndex;
            public DropZoneHandle DropZone;
            public Vector2 DraggingMousePosition;
            public Vector2 DropZoneTopLeft;
            public int InsertAt;
            public int RemoveAt;
            public object[] RemoveValues;
            public bool ShowAllWhilePaging;
            public ObjectPicker ObjectPicker;
            public bool JumpToNextPageOnAdd;
            public GeneralDrawerConfig ListConfig;
            public InspectorProperty Property;
            public GUIContent Label;
            public bool IsAboutToDroppingUnityObjects;
            public bool IsDroppingUnityObjects;
            public bool HideAddButton;
            public bool HideRemoveButton;
            public FilteredPropertyChildren FilteredChildren;
            public bool BaseDraggable;
            public bool BaseIsReadOnly;
            public string SearchFieldControlName = "CollectionSearchFilter_" + Guid.NewGuid().ToString();

            public bool IsReadOnly { get { return this.BaseIsReadOnly || this.FilteredChildren.IsCurrentlyFiltered; } }
            public bool Draggable { get { return this.BaseDraggable && !this.FilteredChildren.IsCurrentlyFiltered; } }

            public ActionResolver OnTitleBarGUI;
            public ActionResolver GetCustomAddFunctionVoid;
            public ValueResolver GetCustomAddFunction;

            public ActionResolver CustomRemoveIndexFunction;
            public ActionResolver CustomRemoveElementFunction;

            public ActionResolver OnBeginListElementGUI;
            public ActionResolver OnEndListElementGUI;

            public Func<object, InspectorProperty, object> GetListElementLabelText;

            //public Action<object> GetCustomAddFunctionVoid;
            //public Func<object, object> GetCustomAddFunction;

            //public Action<object, int> CustomRemoveIndexFunction;
            //public Action<object, object> CustomRemoveElementFunction;

            //public Func<object, object> GetListElementLabelText;
            //public Action<object, int> OnBeginListElementGUI;
            //public Action<object, int> OnEndListElementGUI;

            public int NumberOfItemsPerPage
            {
                get
                {
                    return this.CustomListDrawerOptions.NumberOfItemsPerPageHasValue ? this.CustomListDrawerOptions.NumberOfItemsPerPage : this.ListConfig.NumberOfItemsPrPage;
                }
            }

            public GUIStyle ListItemStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(25, 20, 3, 3)
            };
        }
    }
}
#endif