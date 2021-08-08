#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetListAttributeOnSingleObjectDrawer.cs" company="Sirenix IVS">
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
    /// Not yet documented.
    /// </summary>
    [DrawerPriority(0, 0, 3001)]
    public class AssetListAttributeOnSingleObjectDrawer<TElement> : OdinAttributeDrawer<AssetListAttribute, TElement> where TElement : UnityEngine.Object
    {
        private static readonly NamedValue[] customFilterMethodArgs = new NamedValue[]
        {
            new NamedValue("asset", typeof(TElement))
        };

        private ValueResolver<bool> customFilterMethod;

        private List<UnityEngine.Object> availableAssets = new List<UnityEngine.Object>();
        private string[] tags;
        private string[] layerNames;
        private DirectoryInfo assetsFolderLocation;
        private string prettyPath;
        private bool isPopulated = false;
        private double maxSearchDurationPrFrameInMS = 1;
        private int numberOfResultsToSearch = 0;
        private int totalSearchCount = 0;
        private int currentSearchingIndex = 0;
        private IEnumerator populateListRoutine;
        private static GUIStyle padding;

        private static GUIStyle Padding
        {
            get
            {
                if (padding == null)
                {
                    padding = new GUIStyle() { padding = new RectOffset(5, 5, 3, 3) };
                }
                return padding;
            }
        }

        protected override void Initialize()
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            this.tags = attribute.Tags != null ? attribute.Tags.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;
            this.layerNames = attribute.LayerNames != null ? attribute.LayerNames.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;

            if (attribute.Path != null)
            {
                var path = attribute.Path.Trim('/', ' ');
                path = "Assets/" + path + "/";
                path = Application.dataPath + "/" + path;

                this.assetsFolderLocation = new DirectoryInfo(path);

                path = attribute.Path.TrimStart('/').TrimEnd('/');
                this.prettyPath = "/" + path.TrimStart('/');
            }

            if (attribute.CustomFilterMethod != null)
            {
                this.customFilterMethod = ValueResolver.Get<bool>(this.Property, attribute.CustomFilterMethod, customFilterMethodArgs);
            }

            if (Event.current != null)
            {
                // We can get away with lag on load.
                this.maxSearchDurationPrFrameInMS = 20;
                this.EnsureListPopulation();
            }

            this.maxSearchDurationPrFrameInMS = 1;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            var currentValue = (UnityEngine.Object)entry.WeakSmartValue;

            if (this.customFilterMethod != null && this.customFilterMethod.HasError)
            {
                this.customFilterMethod.DrawError();
            }
            else
            {
                this.EnsureListPopulation();
            }

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            {
                SirenixEditorGUI.BeginHorizontalToolbar();
                if (label != null)
                {
                    GUILayout.Label(label);
                }

                GUILayout.FlexibleSpace();
                if (this.prettyPath != null)
                {
                    GUILayout.Label(this.prettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    SirenixEditorGUI.VerticalLineSeparator();
                }

                if (this.isPopulated)
                {
                    GUILayout.Label(this.availableAssets.Count + " items", SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    GUIHelper.PushGUIEnabled(GUI.enabled && (this.availableAssets.Count > 0 && (this.customFilterMethod == null || !this.customFilterMethod.HasError)));
                }
                else
                {
                    GUILayout.Label("Scanning " + this.currentSearchingIndex + " / " + this.numberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    GUIHelper.PushGUIEnabled(false);
                }

                SirenixEditorGUI.VerticalLineSeparator();

                bool drawConflict = entry.Property.ParentValues.Count > 1;
                if (drawConflict == false)
                {
                    var index = this.availableAssets.IndexOf(currentValue) + 1;
                    if (index > 0)
                    {
                        GUILayout.Label(index.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    }
                    else
                    {
                        drawConflict = true;
                    }
                }

                if (drawConflict)
                {
                    GUILayout.Label("-", SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft) && this.isPopulated)
                {
                    var index = this.availableAssets.IndexOf(currentValue) - 1;
                    index = index < 0 ? this.availableAssets.Count - 1 : index;
                    entry.WeakSmartValue = this.availableAssets[index];
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleDown) && this.isPopulated)
                {
                    GenericMenu m = new GenericMenu();
                    var selected = currentValue;
                    int itemsPrPage = 40;
                    bool showPages = this.availableAssets.Count > 50;
                    string page = "";
                    int selectedPage = (this.availableAssets.IndexOf(entry.WeakSmartValue as UnityEngine.Object) / itemsPrPage);
                    for (int i = 0; i < this.availableAssets.Count; i++)
                    {
                        var obj = this.availableAssets[i];
                        if (obj != null)
                        {
                            var path = AssetDatabase.GetAssetPath(obj);
                            var name = string.IsNullOrEmpty(path) ? obj.name : path.Substring(7).Replace("/", "\\");
                            var localEntry = entry;

                            if (showPages)
                            {
                                var p = (i / itemsPrPage);
                                page = (p * itemsPrPage) + " - " + Mathf.Min(((p + 1) * itemsPrPage), this.availableAssets.Count - 1);
                                if (selectedPage == p)
                                {
                                    page += " (contains selected)";
                                }
                                page += "/";
                            }

                            m.AddItem(new GUIContent(page + name), obj == selected, () =>
                           {
                               localEntry.Property.Tree.DelayActionUntilRepaint(() => localEntry.WeakSmartValue = obj);
                           });
                        }
                    }
                    m.ShowAsContext();
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight) && this.isPopulated)
                {
                    var index = this.availableAssets.IndexOf(currentValue) + 1;
                    entry.WeakSmartValue = this.availableAssets[index % this.availableAssets.Count];
                }

                GUIHelper.PopGUIEnabled();

                SirenixEditorGUI.EndHorizontalToolbar();
                SirenixEditorGUI.BeginVerticalList();
                SirenixEditorGUI.BeginListItem(false, padding);
                this.CallNextDrawer(null);
                SirenixEditorGUI.EndListItem();
                SirenixEditorGUI.EndVerticalList();
            }
            SirenixEditorGUI.EndIndentedVertical();
        }

        private IEnumerator PopulateListRoutine()
        {
            while (true)
            {
                if (this.isPopulated)
                {
                    yield return null;
                    continue;
                }

                HashSet<UnityEngine.Object> seenObjects = new HashSet<UnityEngine.Object>();
                int[] layers = this.layerNames != null ? this.layerNames.Select(l => LayerMask.NameToLayer(l)).ToArray() : null;

                this.availableAssets.Clear();

                IEnumerable<AssetUtilities.AssetSearchResult> allAssets;
#pragma warning disable CS0618 // Type or member is obsolete
                if (this.prettyPath == null)
                {
                    allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(this.Property.ValueEntry.BaseValueType, null);
                }
                else
                {
                    allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(this.Property.ValueEntry.BaseValueType, "Assets/" + this.prettyPath.TrimStart('/'));
                }
#pragma warning restore CS0618 // Type or member is obsolete

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                foreach (var p in allAssets)
                {
                    if (sw.Elapsed.TotalMilliseconds > this.maxSearchDurationPrFrameInMS)
                    {
                        this.numberOfResultsToSearch = p.NumberOfResults;
                        this.currentSearchingIndex = p.CurrentIndex;

                        GUIHelper.RequestRepaint();
                        yield return null;
                        sw.Reset();
                        sw.Start();
                    }

                    var asset = p.Asset;

                    if (asset != null && seenObjects.Add(asset))
                    {
                        var go = asset as Component != null ? (asset as Component).gameObject : asset as GameObject == null ? null : asset as GameObject;

                        var assetName = go == null ? asset.name : go.name;

                        if (this.Attribute.AssetNamePrefix != null && assetName.StartsWith(this.Attribute.AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            continue;
                        }

                        if (this.assetsFolderLocation != null)
                        {
                            var path = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath + "/" + AssetDatabase.GetAssetPath(asset)));
                            if (this.assetsFolderLocation.HasSubDirectory(path) == false)
                            {
                                continue;
                            }
                        }

                        if (this.layerNames != null && go == null || this.tags != null && go == null)
                        {
                            continue;
                        }

                        if (go != null && this.tags != null && !this.tags.Contains(go.tag))
                        {
                            continue;
                        }

                        if (go != null && this.layerNames != null && !layers.Contains(go.layer))
                        {
                            continue;
                        }

                        if (this.customFilterMethod != null)
                        {
                            this.customFilterMethod.Context.NamedValues.Set("asset", asset);

                            if (!this.customFilterMethod.GetValue())
                            {
                                continue;
                            }
                        }
                        
                        this.availableAssets.Add(asset);
                    }
                }

                this.isPopulated = true;
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
    }
}
#endif