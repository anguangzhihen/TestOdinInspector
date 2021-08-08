#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DictionaryDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Property drawer for <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    public class DictionaryDrawer<TDictionary, TKey, TValue> : OdinValueDrawer<TDictionary> where TDictionary : IDictionary<TKey, TValue>
    {
        private const string CHANGE_ID = "DICTIONARY_DRAWER";
        private static readonly bool KeyIsValueType = typeof(TKey).IsValueType;
        private static GUIStyle addKeyPaddingStyle;
        private static GUIStyle listItemStyle;
        private static GUIStyle AddKeyPaddingStyle
        {
            get
            {
                if (addKeyPaddingStyle == null)
                {
                    addKeyPaddingStyle = new GUIStyle("CN Box")
                    {
                        overflow = new RectOffset(0, 0, 1, 0),
                        fixedHeight = 0,
                        stretchHeight = false,
                        padding = new RectOffset(10, 10, 10, 10)
                    };
                }

                return addKeyPaddingStyle;
            }
        }

        private GUIPagingHelper paging = new GUIPagingHelper();
        private GeneralDrawerConfig config;
        private float keyWidthOffset;
        private bool showAddKeyGUI = false;
        private bool? newKeyIsValid;
        private string newKeyErrorMessage;
        private TKey newKey;
        private TValue newValue;
        private StrongDictionaryPropertyResolver<TDictionary, TKey, TValue> dictionaryResolver;
        //private GUIContent label;
        private DictionaryDrawerSettings attrSettings;
        private bool disableAddKey;
        private GUIContent keyLabel;
        private GUIContent valueLabel;
        private float keyLabelWidth;
#pragma warning disable CS0414
        // TODO: Is this field necessary? Last time I changed something in the DictionaryDrawer I broke stuff, so this time I'm asking first.
        private float valueLabelWidth;
#pragma warning restore CS0414
        private TempKeyValuePair<TKey, TValue> tempKeyValue;
        private IPropertyValueEntry<TKey> tempKeyEntry;
        private IPropertyValueEntry<TValue> tempValueEntry;

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.ChildResolver is StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>;
        }

        protected override void Initialize()
        {
            listItemStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(7, 20, 3, 3)
            };

            var entry = this.ValueEntry;

            this.keyWidthOffset = 130;
            //this.label = this.Property.Label ?? new GUIContent(typeof(TDictionary).GetNiceName());
            this.attrSettings = entry.Property.GetAttribute<DictionaryDrawerSettings>() ?? new DictionaryDrawerSettings();
            this.disableAddKey = entry.Property.Tree.PrefabModificationHandler.HasPrefabs && entry.SerializationBackend == SerializationBackend.Odin && !entry.Property.SupportsPrefabModifications;
            this.keyLabel = new GUIContent(this.attrSettings.KeyLabel);
            this.valueLabel = new GUIContent(this.attrSettings.ValueLabel);
            this.keyLabelWidth = EditorStyles.label.CalcSize(this.keyLabel).x + 20;
            this.valueLabelWidth = EditorStyles.label.CalcSize(this.valueLabel).x + 20;

            if (!this.disableAddKey)
            {
                this.tempKeyValue = new TempKeyValuePair<TKey, TValue>();
                var tree = PropertyTree.Create(this.tempKeyValue);
                tree.UpdateTree();
                this.tempKeyEntry = (IPropertyValueEntry<TKey>)tree.GetPropertyAtPath("Key").ValueEntry;
                this.tempValueEntry = (IPropertyValueEntry<TValue>)tree.GetPropertyAtPath("Value").ValueEntry;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            this.dictionaryResolver = entry.Property.ChildResolver as StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>;
            this.config = GeneralDrawerConfig.Instance;
            this.paging.NumberOfItemsPerPage = this.config.NumberOfItemsPrPage;
            listItemStyle.padding.right = !entry.IsEditable || this.attrSettings.IsReadOnly ? 4 : 20;

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            {
                this.paging.Update(elementCount: entry.Property.Children.Count);
                this.DrawToolbar(entry, label);
                this.paging.Update(elementCount: entry.Property.Children.Count);

                if (!this.disableAddKey && this.attrSettings.IsReadOnly == false)
                {
                    this.DrawAddKey(entry);
                }

                float t;
                GUIHelper.BeginLayoutMeasuring();
                if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry.Property, this), this.Property.State.Expanded, out t))
                {
                    var rect = SirenixEditorGUI.BeginVerticalList(false);
                    if (this.attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
                    {
                        var maxWidth = rect.width - 90;
                        rect.xMin = this.keyWidthOffset + 22;
                        rect.xMax = rect.xMin + 10;
                        this.keyWidthOffset = this.keyWidthOffset + SirenixEditorGUI.SlideRect(rect).x;

                        if (Event.current.type == EventType.Repaint)
                        {
                            this.keyWidthOffset = Mathf.Clamp(this.keyWidthOffset, 90, maxWidth);
                        }

                        if (this.paging.ElementCount != 0)
                        {
                            var headerRect = SirenixEditorGUI.BeginListItem(false);
                            {
                                GUILayout.Space(14);
                                if (Event.current.type == EventType.Repaint)
                                {
                                    GUI.Label(headerRect.SetWidth(this.keyWidthOffset), this.keyLabel, SirenixGUIStyles.LabelCentered);
                                    GUI.Label(headerRect.AddXMin(this.keyWidthOffset), this.valueLabel, SirenixGUIStyles.LabelCentered);
                                    SirenixEditorGUI.DrawSolidRect(headerRect.AlignBottom(1), SirenixGUIStyles.BorderColor);
                                }
                            }
                            SirenixEditorGUI.EndListItem();
                        }
                    }

                    GUIHelper.PushHierarchyMode(false);
                    this.DrawElements(entry, label);
                    GUIHelper.PopHierarchyMode();
                    SirenixEditorGUI.EndVerticalList();
                }
                SirenixEditorGUI.EndFadeGroup();

                // Draw borders
                var outerRect = GUIHelper.EndLayoutMeasuring();
                if (t > 0.01f && Event.current.type == EventType.Repaint)
                {
                    Color col = SirenixGUIStyles.BorderColor;
                    outerRect.yMin -= 1;
                    SirenixEditorGUI.DrawBorders(outerRect, 1, col);
                    col.a *= t;
                    if (this.attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
                    {
                        // Draw Slide Rect Border
                        outerRect.width = 1;
                        outerRect.x += this.keyWidthOffset + 13;
                        SirenixEditorGUI.DrawSolidRect(outerRect, col);
                    }
                }
            }
            SirenixEditorGUI.EndIndentedVertical();
        }

        private void DrawAddKey(IPropertyValueEntry<TDictionary> entry)
        {
            if (entry.IsEditable == false || this.attrSettings.IsReadOnly)
            {
                return;
            }

            if (SirenixEditorGUI.BeginFadeGroup(this, this.showAddKeyGUI))
            {
                GUILayout.BeginVertical(AddKeyPaddingStyle);
                {
                    if (typeof(TKey) == typeof(string) && this.newKey == null)
                    {
                        this.newKey = (TKey)(object)"";
                        this.newKeyIsValid = null;
                    }

                    if (this.newKeyIsValid == null)
                    {
                        this.newKeyIsValid = CheckKeyIsValid(entry, this.newKey, out this.newKeyErrorMessage);
                    }

                    this.tempKeyEntry.Property.Tree.BeginDraw(false);

                    // Key
                    {
                        //this.TempKeyValue.key = this.NewKey;
                        this.tempKeyEntry.Property.Update();

                        EditorGUI.BeginChangeCheck();

                        this.tempKeyEntry.Property.Draw(this.keyLabel);

                        bool changed1 = EditorGUI.EndChangeCheck();
                        bool changed2 = this.tempKeyEntry.ApplyChanges();

                        if (changed1 || changed2)
                        {
                            this.newKey = this.tempKeyValue.Key;
                            UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;
                            GUIHelper.RequestRepaint();
                        }
                    }

                    // Value
                    {
                        //this.TempKeyValue.value = this.NewValue;
                        this.tempValueEntry.Property.Update();
                        this.tempValueEntry.Property.Draw(this.valueLabel);
                        this.tempValueEntry.ApplyChanges();
                        this.newValue = this.tempKeyValue.Value;
                    }

                    this.tempKeyEntry.Property.Tree.InvokeDelayedActions();
                    var changed = this.tempKeyEntry.Property.Tree.ApplyChanges();

                    if (changed)
                    {
                        this.newKey = this.tempKeyValue.Key;
                        UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;
                        GUIHelper.RequestRepaint();
                    }

                    this.tempKeyEntry.Property.Tree.EndDraw();

                    GUIHelper.PushGUIEnabled(GUI.enabled && this.newKeyIsValid.Value);
                    if (GUILayout.Button(this.newKeyIsValid.Value ? "Add" : this.newKeyErrorMessage))
                    {
                        var keys = new object[entry.ValueCount];
                        var values = new object[entry.ValueCount];

                        for (int i = 0; i < keys.Length; i++)
                        {
                            keys[i] = SerializationUtility.CreateCopy(this.newKey);
                        }

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = SerializationUtility.CreateCopy(this.newValue);
                        }

                        this.dictionaryResolver.QueueSet(keys, values);
                        UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;
                        GUIHelper.RequestRepaint();

                        entry.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            this.newValue = default(TValue);
                            this.tempKeyValue.Value = default(TValue);
                            this.tempValueEntry.Update();
                        });
                    }
                    GUIHelper.PopGUIEnabled();
                }
                GUILayout.EndVertical();
            }
            SirenixEditorGUI.EndFadeGroup();
        }

        private void DrawToolbar(IPropertyValueEntry<TDictionary> entry, GUIContent label)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                if (entry.ListLengthChangedFromPrefab) GUIHelper.PushIsBoldLabel(true);

                if (this.config.HideFoldoutWhileEmpty && this.paging.ElementCount == 0)
                {
                    if (label != null)
                    {
                        GUILayout.Label(label, GUILayoutOptions.ExpandWidth(false));
                    }
                }
                else
                {
                    var newState = label != null ? SirenixEditorGUI.Foldout(this.Property.State.Expanded, label)
                        : SirenixEditorGUI.Foldout(this.Property.State.Expanded, "");
                    if (!newState && this.Property.State.Expanded)
                    {
                        this.showAddKeyGUI = false;
                    }
                    this.Property.State.Expanded = newState;
                }

                if (entry.ListLengthChangedFromPrefab) GUIHelper.PopIsBoldLabel();

                GUILayout.FlexibleSpace();

                // Item Count
                if (this.config.ShowItemCount)
                {
                    if (entry.ValueState == PropertyValueState.CollectionLengthConflict)
                    {
                        int min = entry.Values.Min(x => x.Count);
                        int max = entry.Values.Max(x => x.Count);
                        GUILayout.Label(min + " / " + max + " items", EditorStyles.centeredGreyMiniLabel);
                    }
                    else
                    {
                        GUILayout.Label(this.paging.ElementCount == 0 ? "Empty" : this.paging.ElementCount + " items", EditorStyles.centeredGreyMiniLabel);
                    }
                }

                bool hidePaging =
                        this.config.HidePagingWhileCollapsed && this.Property.State.Expanded == false ||
                        this.config.HidePagingWhileOnlyOnePage && this.paging.PageCount == 1;

                if (!hidePaging)
                {
                    var wasEnabled = GUI.enabled;
                    bool pagingIsRelevant = this.paging.IsEnabled && this.paging.PageCount != 1;

                    GUI.enabled = wasEnabled && pagingIsRelevant && !this.paging.IsOnFirstPage;
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowLeft, true))
                    {
                        if (Event.current.button == 0)
                        {
                            this.paging.CurrentPage--;
                        }
                        else
                        {
                            this.paging.CurrentPage = 0;
                        }
                    }

                    GUI.enabled = wasEnabled && pagingIsRelevant;
                    var width = GUILayoutOptions.Width(10 + this.paging.PageCount.ToString().Length * 10);
                    this.paging.CurrentPage = EditorGUILayout.IntField(this.paging.CurrentPage + 1, width) - 1;
                    GUILayout.Label(GUIHelper.TempContent("/ " + this.paging.PageCount));

                    GUI.enabled = wasEnabled && pagingIsRelevant && !this.paging.IsOnLastPage;
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowRight, true))
                    {
                        if (Event.current.button == 0)
                        {
                            this.paging.CurrentPage++;
                        }
                        else
                        {
                            this.paging.CurrentPage = this.paging.PageCount - 1;
                        }
                    }

                    GUI.enabled = wasEnabled && this.paging.PageCount != 1;
                    if (this.config.ShowExpandButton)
                    {
                        if (SirenixEditorGUI.ToolbarButton(this.paging.IsEnabled ? EditorIcons.ArrowDown : EditorIcons.ArrowUp, true))
                        {
                            this.paging.IsEnabled = !this.paging.IsEnabled;
                        }
                    }
                    GUI.enabled = wasEnabled;
                }
                if (!this.disableAddKey && this.attrSettings.IsReadOnly != true)
                {
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
                    {
                        this.showAddKeyGUI = !this.showAddKeyGUI;

                        if (this.showAddKeyGUI)
                        {
                            this.Property.State.Expanded = true;
                        }
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private static GUIStyle oneLineMargin;

        private static GUIStyle OneLineMargin
        {
            get
            {
                if (oneLineMargin == null)
                {
                    oneLineMargin = new GUIStyle() { margin = new RectOffset(8, 0, 0, 0) };
                }
                return oneLineMargin;
            }
        }

        private static GUIStyle headerMargin;

        private static GUIStyle HeaderMargin
        {
            get
            {
                if (headerMargin == null)
                {
                    headerMargin = new GUIStyle() { margin = new RectOffset(40, 0, 0, 0) };
                }
                return headerMargin;
            }
        }

        private void DrawElements(IPropertyValueEntry<TDictionary> entry, GUIContent label)
        {
            for (int i = this.paging.StartIndex; i < this.paging.EndIndex; i++)
            {
                var keyValuePairProperty = entry.Property.Children[i];
                var keyValuePairValue = (keyValuePairProperty.ValueEntry as IPropertyValueEntry<EditableKeyValuePair<TKey, TValue>>).SmartValue;

                Rect rect = SirenixEditorGUI.BeginListItem(false, listItemStyle);
                {
                    if (this.attrSettings.DisplayMode != DictionaryDisplayOptions.OneLine)
                    {
                        bool defaultExpanded;
                        switch (this.attrSettings.DisplayMode)
                        {
                            case DictionaryDisplayOptions.CollapsedFoldout:
                                defaultExpanded = false;
                                break;

                            case DictionaryDisplayOptions.ExpandedFoldout:
                                defaultExpanded = true;
                                break;

                            default:
                                defaultExpanded = SirenixEditorGUI.ExpandFoldoutByDefault;
                                break;
                        }
                        var isExpanded = keyValuePairProperty.Context.GetPersistent(this, "Expanded", defaultExpanded);

                        SirenixEditorGUI.BeginBox();
                        SirenixEditorGUI.BeginToolbarBoxHeader();
                        {
                            if (keyValuePairValue.IsInvalidKey)
                            {
                                GUIHelper.PushColor(Color.red);
                            }
                            var btnRect = GUIHelper.GetCurrentLayoutRect().AlignLeft(HeaderMargin.margin.left);
                            btnRect.y += 1;
                            GUILayout.BeginVertical(HeaderMargin);
                            GUIHelper.PushIsDrawingDictionaryKey(true);

                            GUIHelper.PushLabelWidth(this.keyLabelWidth);

                            var keyProperty = keyValuePairProperty.Children[0];
                            var keyLabel = GUIHelper.TempContent(" ");
                            DrawKeyProperty(keyProperty, keyLabel);

                            GUIHelper.PopLabelWidth();

                            GUIHelper.PopIsDrawingDictionaryKey();
                            GUILayout.EndVertical();
                            if (keyValuePairValue.IsInvalidKey)
                            {
                                GUIHelper.PopColor();
                            }
                            isExpanded.Value = SirenixEditorGUI.Foldout(btnRect, isExpanded.Value, this.keyLabel);
                        }
                        SirenixEditorGUI.EndToolbarBoxHeader();

                        if (SirenixEditorGUI.BeginFadeGroup(isExpanded, isExpanded.Value))
                        {
                            keyValuePairProperty.Children[1].Draw(null);
                        }
                        SirenixEditorGUI.EndFadeGroup();

                        SirenixEditorGUI.EndToolbarBox();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.BeginVertical(GUILayoutOptions.Width(this.keyWidthOffset));
                        {
                            var keyProperty = keyValuePairProperty.Children[0];

                            if (keyValuePairValue.IsInvalidKey)
                            {
                                GUIHelper.PushColor(Color.red);
                            }

                            if (this.attrSettings.IsReadOnly) GUIHelper.PushGUIEnabled(false);

                            GUIHelper.PushIsDrawingDictionaryKey(true);
                            GUIHelper.PushLabelWidth(10);

                            DrawKeyProperty(keyProperty, null);

                            GUIHelper.PopLabelWidth();
                            GUIHelper.PopIsDrawingDictionaryKey();

                            if (this.attrSettings.IsReadOnly) GUIHelper.PopGUIEnabled();

                            if (keyValuePairValue.IsInvalidKey)
                            {
                                GUIHelper.PopColor();
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical(OneLineMargin);
                        {
                            GUIHelper.PushHierarchyMode(false);
                            var valueEntry = keyValuePairProperty.Children[1];
                            var tmp = GUIHelper.ActualLabelWidth;
                            GUIHelper.BetterLabelWidth = 150;
                            valueEntry.Draw(null);
                            GUIHelper.BetterLabelWidth = tmp;
                            GUIHelper.PopHierarchyMode();
                        }
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }

                    if (entry.IsEditable && !this.attrSettings.IsReadOnly && SirenixEditorGUI.IconButton(new Rect(rect.xMax - 24 + 5, rect.y + 4 + ((int)rect.height - 23) / 2, 14, 14), EditorIcons.X))
                    {
                        this.dictionaryResolver.QueueRemoveKey(Enumerable.Range(0, entry.ValueCount).Select(n => this.dictionaryResolver.GetKey(n, i)).ToArray());
                        UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;
                        GUIHelper.RequestRepaint();
                    }
                }
                SirenixEditorGUI.EndListItem();
            }

            if (this.paging.IsOnLastPage && entry.ValueState == PropertyValueState.CollectionLengthConflict)
            {
                SirenixEditorGUI.BeginListItem(false);
                GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.centeredGreyMiniLabel);
                SirenixEditorGUI.EndListItem();
            }
        }

        private void DrawKeyProperty(InspectorProperty keyProperty, GUIContent keyLabel)
        {
            EditorGUI.BeginChangeCheck();

#if SIRENIX_INTERNAL
            var keyValuePairValue = (keyProperty.Parent.ValueEntry as IPropertyValueEntry<EditableKeyValuePair<TKey, TValue>>).SmartValue;

            if (keyValuePairValue.IsTempKey && !keyValuePairValue.IsInvalidKey)
            {
                GUIHelper.PushColor(Color.green);
            }
#endif

            keyProperty.Draw(keyLabel);

#if SIRENIX_INTERNAL
            if (keyValuePairValue.IsTempKey && !keyValuePairValue.IsInvalidKey)
            {
                GUIHelper.PopColor();
            }
#endif

            var guiChanged = EditorGUI.EndChangeCheck();

            bool valuesAreDirty = ValuesAreDirty(keyProperty);

            if (!guiChanged && valuesAreDirty)
            {
                this.dictionaryResolver.ValueApplyIsTemporary = true;
                ApplyChangesToProperty(keyProperty);
                this.dictionaryResolver.ValueApplyIsTemporary = false;
            }
            else if (guiChanged && !valuesAreDirty)
            {
                MarkPropertyDirty(keyProperty);
            }
        }

        private static void MarkPropertyDirty(InspectorProperty keyProperty)
        {
            keyProperty.ValueEntry.WeakValues.ForceMarkDirty();

            if (KeyIsValueType)
            {
                for (int i = 0; i < keyProperty.Children.Count; i++)
                {
                    MarkPropertyDirty(keyProperty.Children[i]);
                }
            }
        }

        private static void ApplyChangesToProperty(InspectorProperty keyProperty)
        {
            if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty) keyProperty.ValueEntry.ApplyChanges();

            if (KeyIsValueType)
            {
                for (int i = 0; i < keyProperty.Children.Count; i++)
                {
                    ApplyChangesToProperty(keyProperty.Children[i]);
                }
            }
        }

        private static bool ValuesAreDirty(InspectorProperty keyProperty)
        {
            if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty)
            {
                return true;
            }

            if (KeyIsValueType)
            {
                for (int i = 0; i < keyProperty.Children.Count; i++)
                {
                    if (ValuesAreDirty(keyProperty.Children[i])) return true;
                }
            }

            return false;
        }

        private static bool CheckKeyIsValid(IPropertyValueEntry<TDictionary> entry, TKey key, out string errorMessage)
        {
            if (!KeyIsValueType && object.ReferenceEquals(key, null))
            {
                errorMessage = "Key cannot be null.";
                return false;
            }

            var keyStr = DictionaryKeyUtility.GetDictionaryKeyString(key);

            if (entry.Property.Children[keyStr] == null)
            {
                errorMessage = "";
                return true;
            }
            else
            {
                errorMessage = "An item with the same key already exists.";
                return false;
            }
        }
    }
}
#endif