#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetListAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Collections;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="AssetListAttribute"/>.
    /// Displays a configurable list of assets, where each item can be enabled or disabled.
    /// </summary>
    /// <seealso cref="AssetListAttribute"/>
    /// <seealso cref="AssetsOnlyAttribute"/>
    /// <seealso cref="SceneObjectsOnlyAttribute"/>
    /// <seealso cref="RequiredAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
    public sealed class AssetListAttributeDrawer<TList, TElement> : OdinAttributeDrawer<AssetListAttribute, TList>, IDefinesGenericMenuItems where TList : IList<TElement> where TElement : UnityEngine.Object
    {
        private static readonly NamedValue[] customFilterMethodArgs = new NamedValue[]
        {
            new NamedValue("asset", typeof(TElement))
        };

        private AssetList assetList;
        private PropertyTree propertyTree;
        private InspectorProperty listProperty;

        protected override void Initialize()
        {
            var property = this.Property;
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            this.assetList = new AssetList();

            this.assetList.AutoPopulate = attribute.AutoPopulate;
            this.assetList.AssetNamePrefix = attribute.AssetNamePrefix;
            this.assetList.Tags = attribute.Tags != null ? attribute.Tags.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;
            this.assetList.LayerNames = attribute.LayerNames != null ? attribute.LayerNames.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;
            this.assetList.List = entry;
            this.assetList.CollectionResolver = property.ChildResolver as IOrderedCollectionResolver;
            this.assetList.Property = entry.Property;

            if (attribute.Path != null)
            {
                var path = attribute.Path.TrimStart('/', ' ').TrimEnd('/', ' ');
                path = attribute.Path.Trim('/', ' ');

                path = "Assets/" + path + "/";
                path = Application.dataPath + "/" + path;

                this.assetList.AssetsFolderLocation = new DirectoryInfo(path);

                path = attribute.Path.Trim('/', ' ');
                this.assetList.PrettyPath = "/" + path.TrimStart('/');
            }

            if (attribute.CustomFilterMethod != null)
            {
                this.assetList.CustomFilterMethod = ValueResolver.Get<bool>(this.Property, attribute.CustomFilterMethod, customFilterMethodArgs);
            }

            if (Event.current != null)
            {
                // We can get away with lag on load.
                this.assetList.MaxSearchDurationPrFrameInMS = 20;
                this.assetList.EnsureListPopulation();
            }

            this.assetList.MaxSearchDurationPrFrameInMS = 1;

            this.propertyTree = PropertyTree.Create(this.assetList);
            this.propertyTree.UpdateTree();

            this.listProperty = this.propertyTree.GetPropertyAtPath("toggleableAssets");
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            if (property.ValueEntry.WeakSmartValue == null)
            {
                return;
            }

            this.propertyTree.GetRootProperty(0).Label = label;

            this.listProperty.State.Enabled = this.Property.State.Enabled;
            this.listProperty.State.Expanded = this.Property.State.Expanded;

            if (Event.current.type == EventType.Layout)
            {
                this.assetList.Property = entry.Property;
                this.assetList.EnsureListPopulation();
                this.assetList.SetToggleValues();
            }

            if (this.assetList.CustomFilterMethod != null && this.assetList.CustomFilterMethod.HasError)
            {
                this.assetList.CustomFilterMethod.DrawError();
            }

            this.assetList.Property = entry.Property;
            this.propertyTree.Draw(false);

            this.Property.State.Enabled = this.listProperty.State.Enabled;
            this.Property.State.Expanded = this.listProperty.State.Expanded;

            if (Event.current.type == EventType.Used)
            {
                this.assetList.UpdateList();
            }
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (this.assetList == null)
            {
                return;
            }

            if (this.assetList.List.SmartValue.Count != assetList.ToggleableAssets.Count)
            {
                genericMenu.AddItem(new GUIContent("Include All"), false, () => { this.assetList.UpdateList(true); });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Include All"));
            }
        }

        [Serializable, ShowOdinSerializedPropertiesInInspector]
        private class AssetList
        {
            [HideInInspector]
            public bool AutoPopulate;

            [HideInInspector]
            public string AssetNamePrefix;

            [HideInInspector]
            public string[] LayerNames;

            [HideInInspector]
            public string[] Tags;

            [HideInInspector]
            public IPropertyValueEntry<TList> List;

            [HideInInspector]
            public IOrderedCollectionResolver CollectionResolver;

            [HideInInspector]
            public DirectoryInfo AssetsFolderLocation;

            [HideInInspector]
            public string PrettyPath;

            [HideInInspector]
            public ValueResolver<bool> CustomFilterMethod;

            [HideInInspector]
            public InspectorProperty Property;

            public List<ToggleableAsset> ToggleableAssets
            {
                get
                {
                    return this.toggleableAssets;
                }
            }

            [SerializeField]
            [ListDrawerSettings(IsReadOnly = true, DraggableItems = false, OnTitleBarGUI = "OnListTitlebarGUI", ShowItemCount = false)]
            [DisableContextMenu(true, true)]
            [HideReferenceObjectPicker]
            private List<ToggleableAsset> toggleableAssets = new List<ToggleableAsset>();

            [SerializeField]
            [HideInInspector]
            private HashSet<TElement> toggledAssets = new HashSet<TElement>();

            [SerializeField]
            [HideInInspector]
            private Dictionary<TElement, ToggleableAsset> toggleableAssetLookup = new Dictionary<TElement, ToggleableAsset>();

            [NonSerialized]
            public bool IsPopulated = false;

            [NonSerialized]
            public double MaxSearchDurationPrFrameInMS = 1;

            [NonSerialized]
            public int NumberOfResultsToSearch = 0;

            [NonSerialized]
            public int TotalSearchCount = 0;

            [NonSerialized]
            public int CurrentSearchingIndex = 0;

            [NonSerialized]
            private IEnumerator populateListRoutine;

            private IEnumerator PopulateListRoutine()
            {
                while (true)
                {
                    if (this.IsPopulated)
                    {
                        yield return null;
                        continue;
                    }

                    HashSet<UnityEngine.Object> seenObjects = new HashSet<UnityEngine.Object>();
                    this.toggleableAssets.Clear();
                    this.toggleableAssetLookup.Clear();

                    IEnumerable<AssetUtilities.AssetSearchResult> allAssets;
#pragma warning disable CS0618 // Type or member is obsolete
                    if (this.PrettyPath == null)
                    {
                        allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement), null);
                    }
                    else
                    {
                        allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement), "Assets/" + this.PrettyPath.TrimStart('/'));
                    }
#pragma warning restore CS0618 // Type or member is obsolete

                    int[] layers = this.LayerNames != null ? this.LayerNames.Select(l => LayerMask.NameToLayer(l)).ToArray() : null;

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    foreach (var p in allAssets)
                    {
                        if (sw.Elapsed.TotalMilliseconds > this.MaxSearchDurationPrFrameInMS)
                        {
                            this.NumberOfResultsToSearch = p.NumberOfResults;
                            this.CurrentSearchingIndex = p.CurrentIndex;

                            GUIHelper.RequestRepaint();
                            //this.SetToggleValues(startIndex);

                            yield return null;
                            sw.Reset();
                            sw.Start();
                        }

                        var asset = p.Asset;

                        if (asset != null && seenObjects.Add(asset))
                        {
                            var go = asset as Component != null ? (asset as Component).gameObject : asset as GameObject == null ? null : asset as GameObject;

                            var assetName = go == null ? asset.name : go.name;

                            if (this.AssetNamePrefix != null && assetName.StartsWith(this.AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase) == false)
                            {
                                continue;
                            }

                            if (this.AssetsFolderLocation != null)
                            {
                                var path = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath + "/" + AssetDatabase.GetAssetPath(asset)));
                                if (this.AssetsFolderLocation.HasSubDirectory(path) == false)
                                {
                                    continue;
                                }
                            }

                            if (this.LayerNames != null && go == null || this.Tags != null && go == null)
                            {
                                continue;
                            }

                            if (go != null && this.Tags != null && !this.Tags.Contains(go.tag))
                            {
                                continue;
                            }

                            if (go != null && this.LayerNames != null && !layers.Contains(go.layer))
                            {
                                continue;
                            }

                            if (this.toggleableAssetLookup.ContainsKey(asset as TElement))
                            {
                                continue;
                            }

                            if (this.CustomFilterMethod != null)
                            {
                                this.CustomFilterMethod.Context.NamedValues.Set("asset", asset);

                                if (!this.CustomFilterMethod.GetValue())
                                {
                                    continue;
                                }
                            }
                            
                            var toggleable = new ToggleableAsset(asset as TElement, this.AutoPopulate);

                            this.toggleableAssets.Add(toggleable);
                            this.toggleableAssetLookup.Add(asset as TElement, toggleable);
                        }
                    }

                    this.SetToggleValues();

                    this.IsPopulated = true;
                    GUIHelper.RequestRepaint();
                    yield return null;
                }
            }

            public void EnsureListPopulation()
            {
                if (Event.current.type == EventType.Layout)
                {
                    if (this.populateListRoutine == null)
                    {
                        this.populateListRoutine = this.PopulateListRoutine();
                    }

                    this.populateListRoutine.MoveNext();
                }
            }

            public void SetToggleValues(int startIndex = 0)
            {
                if (this.List.SmartValue == null)
                {
                    return;
                }

                for (int i = startIndex; i < this.toggleableAssets.Count; i++)
                {
                    if (this.toggleableAssets[i] == null || this.toggleableAssets[i].Object == null)
                    {
                        this.Rescan();
                        break;
                    }

                    this.toggleableAssets[i].Toggled = false;
                }

                for (int i = this.List.SmartValue.Count - 1; i >= startIndex; i--)
                {
                    var asset = this.List.SmartValue[i] as TElement;
                    if (asset == null)
                    {
                        this.CollectionResolver.QueueRemoveAt(i);
                    }
                    else
                    {
                        ToggleableAsset toggleable;
                        if (this.toggleableAssetLookup.TryGetValue(asset, out toggleable))
                        {
                            toggleable.Toggled = true;
                        }
                        else
                        {
                            if (this.IsPopulated)
                            {
                                this.CollectionResolver.QueueRemoveAt(i);
                            }
                        }
                    }
                }
            }

            public void Rescan()
            {
                this.IsPopulated = false;
            }

            private void OnListTitlebarGUI()
            {
                if (this.PrettyPath != null)
                {
                    GUILayout.Label(this.PrettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    SirenixEditorGUI.VerticalLineSeparator();
                }

                if (this.IsPopulated)
                {
                    GUILayout.Label(this.List.SmartValue.Count + " / " + this.toggleableAssets.Count, SirenixGUIStyles.CenteredGreyMiniLabel);
                }
                else
                {
                    GUILayout.Label("Scanning " + this.CurrentSearchingIndex + " / " + this.NumberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }
                bool disableGUI = !this.IsPopulated;

                if (disableGUI)
                {
                    GUIHelper.PushGUIEnabled(false);
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh) && this.IsPopulated)
                {
                    this.Rescan();
                }

                if (AssetUtilities.CanCreateNewAsset<TElement>())
                {
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus) && this.IsPopulated)
                    {
                        string path = this.PrettyPath;
                        if (path == null)
                        {
                            var lastAsset = this.List.SmartValue.Count > 0 ? this.List.SmartValue[this.List.SmartValue.Count - 1] as TElement : null;
                            if (lastAsset == null)
                            {
                                var lastToggleable = this.toggleableAssets.LastOrDefault();
                                if (lastToggleable != null)
                                {
                                    lastAsset = lastToggleable.Object;
                                }
                            }
                            if (lastAsset != null)
                            {
                                path = AssetUtilities.GetAssetLocation(lastAsset);
                            }
                        }
#pragma warning disable CS0618 // Type or member is obsolete
                        AssetUtilities.CreateNewAsset<TElement>(path, null);
#pragma warning restore CS0618 // Type or member is obsolete
                        this.Rescan();
                    }
                }

                if (disableGUI)
                {
                    GUIHelper.PopGUIEnabled();
                }
            }

            public void UpdateList()
            {
                this.UpdateList(false);
            }

            public void UpdateList(bool includeAll)
            {
                if (this.List.SmartValue == null)
                {
                    return;
                }

                this.toggledAssets.Clear();
                for (int i = 0; i < this.toggleableAssets.Count; i++)
                {
                    if (includeAll || this.AutoPopulate || this.toggleableAssets[i].Toggled)
                    {
                        this.toggledAssets.Add(this.toggleableAssets[i].Object);
                    }
                }

                for (int i = this.List.SmartValue.Count - 1; i >= 0; i--)
                {
                    if (this.List.SmartValue[i] as TElement == null)
                    {
                        this.CollectionResolver.QueueRemoveAt(i);
                        this.Rescan();
                    }
                    else if (this.toggledAssets.Contains(this.List.SmartValue[i] as TElement) == false)
                    {
                        if (this.IsPopulated)
                        {
                            this.CollectionResolver.QueueRemoveAt(i);
                        }
                    }
                    else
                    {
                        this.toggledAssets.Remove(this.List.SmartValue[i] as TElement);
                    }
                }

                foreach (var asset in this.toggledAssets.GFIterator())
                {
                    this.CollectionResolver.QueueAdd(Enumerable.Repeat(asset, this.List.ValueCount).ToArray());
                }

                this.toggledAssets.Clear();
            }
        }

        [Serializable]
        private class ToggleableAsset
        {
            [HideInInspector]
            public bool AutoToggle;

            public bool Toggled;

            public TElement Object;

            public ToggleableAsset(TElement obj, bool autoToggle)
            {
                this.AutoToggle = autoToggle;
                this.Object = obj;
            }
        }

        private sealed class AssetInstanceDrawer : OdinValueDrawer<ToggleableAsset>
        {
            protected override void DrawPropertyLayout(GUIContent label)
            {
                var entry = this.ValueEntry;
                if (entry.SmartValue.AutoToggle)
                {
#pragma warning disable 0618 // Type or member is obsolete
                    SirenixEditorGUI.ObjectField(null, entry.SmartValue.Object, entry.SmartValue.Object.GetType(), false, true);
#pragma warning restore 0618 // Type or member is obsolete
                }
                else
                {
                    var rect = GUILayoutUtility.GetRect(16, 16, GUILayoutOptions.ExpandWidth(true));
                    var toggleRect = new Rect(rect.x, rect.y, 16, 16);
                    var objectFieldRect = new Rect(rect.x + 20, rect.y, rect.width - 20, 16);

                    if (Event.current.type != EventType.Repaint)
                    {
                        toggleRect.x -= 5;
                        toggleRect.y -= 5;
                        toggleRect.width += 10;
                        toggleRect.height += 10;
                    }
                    var prevChanged = GUI.changed;

                    entry.SmartValue.Toggled = GUI.Toggle(toggleRect, entry.SmartValue.Toggled, "");

                    if (prevChanged != GUI.changed)
                    {
                        entry.ApplyChanges();
                    }

                    GUIHelper.PushGUIEnabled(entry.SmartValue.Toggled);

#pragma warning disable 0618 // Type or member is obsolete
                    SirenixEditorGUI.ObjectField(objectFieldRect, null, entry.SmartValue.Object, entry.SmartValue.Object.GetType(), false, true);
#pragma warning restore 0618 // Type or member is obsolete

                    GUIHelper.PopGUIEnabled();
                }
            }
        }
    }
}
#endif