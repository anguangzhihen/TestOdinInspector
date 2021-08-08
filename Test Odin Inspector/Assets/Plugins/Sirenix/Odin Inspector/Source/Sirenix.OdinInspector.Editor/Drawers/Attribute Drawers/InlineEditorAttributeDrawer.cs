#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InlineEditorAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Static GUI information reguarding the InlineEditor attribute.
    /// </summary>
    public static class InlineEditorAttributeDrawer
    {
        /// <summary>
        /// Gets a value indicating how many InlineEditors we are currently in.
        /// </summary>
        public static int CurrentInlineEditorDrawDepth { get; internal set; }
    }

    /// <summary>
    /// Draws properties marked with <see cref="InlineEditorAttribute"/>.
    /// </summary>
    /// <seealso cref="InlineEditorAttribute"/>
    /// <seealso cref="DrawWithUnityAttribute"/>
    [DrawerPriority(0, 0, 3000)]
    public class InlineEditorAttributeDrawer<T> : OdinAttributeDrawer<InlineEditorAttribute, T>, IDisposable where T : UnityEngine.Object
    {
        public static readonly bool IsGameObject = typeof(T) == typeof(GameObject);

        private static System.Reflection.PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible", Flags.AllMembers);
        private static Stack<LayoutSettings> layoutSettingsStack = new Stack<LayoutSettings>();
        private Editor editor;
        private Editor previewEditor;
        private UnityEngine.Object target;
        private Rect inlineEditorRect;
        private Vector2 scrollPos;
        private bool allowSceneObjects;
        private bool drawHeader;
        private bool drawGUI;
        private bool drawPreview;
        private bool alwaysVisible;
        private bool targetIsOpenForEdit;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.ExpandedHasValue && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth == 0)
            {
                this.Property.State.Expanded = this.Attribute.Expanded;
            }

            this.allowSceneObjects = this.Property.Attributes.OfType<AssetsOnlyAttribute>().Any() == false;
        }

        /// <summary>
        /// Draws the property layout.
        /// </summary>
        /// <param name="label">The label.</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect valueRect;
            switch (this.Attribute.ObjectFieldMode)
            {
                case InlineEditorObjectFieldModes.Boxed:
                    this.alwaysVisible = false;
                    SirenixEditorGUI.BeginToolbarBox();
                    SirenixEditorGUI.BeginToolbarBoxHeader();
                    if (this.ValueEntry.SmartValue)
                    {
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, label, out valueRect);
                        this.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(valueRect, this.ValueEntry.SmartValue, this.ValueEntry.BaseValueType, this.allowSceneObjects);
                    }
                    else
                    {
                        this.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(label, this.ValueEntry.SmartValue, this.ValueEntry.BaseValueType, this.allowSceneObjects);
                    }
                    SirenixEditorGUI.EndToolbarBoxHeader();
                    GUIHelper.PushHierarchyMode(false);
                    this.DrawEditor();
                    GUIHelper.PopHierarchyMode();
                    SirenixEditorGUI.EndToolbarBox();
                    break;
                case InlineEditorObjectFieldModes.Foldout:
                    this.alwaysVisible = false;
                    if (this.ValueEntry.SmartValue)
                    {
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, label, out valueRect);
                        this.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(valueRect, this.ValueEntry.SmartValue, this.ValueEntry.BaseValueType, this.allowSceneObjects);
                    }
                    else
                    {
                        this.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(label, this.ValueEntry.SmartValue, this.ValueEntry.BaseValueType, this.allowSceneObjects);
                    }
                    EditorGUI.indentLevel++;
                    this.DrawEditor();
                    EditorGUI.indentLevel--;
                    break;
                case InlineEditorObjectFieldModes.Hidden:
                    this.alwaysVisible = true;
                    if (!(UnityEngine.Object)this.ValueEntry.WeakSmartValue)
                    {
                        this.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(label, this.ValueEntry.SmartValue, this.ValueEntry.BaseValueType, this.allowSceneObjects);
                    }
                    this.DrawEditor();
                    break;
                case InlineEditorObjectFieldModes.CompletelyHidden:
                    this.alwaysVisible = true;
                    this.DrawEditor();
                    break;
            }
        }

        private void DrawEditor()
        {
            var obj = this.ValueEntry.SmartValue;

            if (this.ValueEntry.ValueState == PropertyValueState.ReferencePathConflict)
            {
                SirenixEditorGUI.InfoMessageBox("reference-path-conflict");
            }
            else
            {
                if (this.alwaysVisible || SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded))
                {
                    this.UpdateEditors();

                    if (this.Attribute.MaxHeight != 0)
                    {
                        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayoutOptions.MaxHeight(200));
                    }

                    var prev = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = false;
                    EditorGUI.BeginChangeCheck();
                    this.DoTheDrawing();
                    if (EditorGUI.EndChangeCheck())
                    {
                        var e = this.Property.BaseValueEntry as PropertyValueEntry;
                        if (e != null)
                        {
                            for (int i = 0; i < e.ValueCount; i++)
                            {
                                e.TriggerOnChildValueChanged(i);
                            }
                        }
                    }
                    EditorGUI.showMixedValue = prev;
                    if (this.Attribute.MaxHeight != 0)
                    {
                        EditorGUILayout.EndScrollView();
                    }
                }
                else
                {
                    if (this.editor != null)
                    {
                        this.DestroyEditors();
                    }
                }

                if (!this.alwaysVisible)
                {
                    SirenixEditorGUI.EndFadeGroup();
                }
            }

        }

        private void DoTheDrawing()
        {
            if (IsGameObject)
            {
                SirenixEditorGUI.MessageBox("Odin does not currently have a full GameObject inspector window substitute implemented, so a GameObject cannot be directly inspected inline in the editor.");
                SirenixEditorFields.UnityObjectField(this.ValueEntry.SmartValue, typeof(GameObject), true);

                GUILayout.BeginHorizontal();
                {
                    GUIHelper.PushGUIEnabled(this.ValueEntry.SmartValue != null);

                    string text = this.ValueEntry.SmartValue != null ? ("Open Inspector window for " + this.ValueEntry.SmartValue.name) : "Open Inspector window (null)";

                    if (GUILayout.Button(GUIHelper.TempContent(text)))
                    {
                        GUIHelper.OpenInspectorWindow(this.ValueEntry.SmartValue);
                        GUIUtility.ExitGUI();
                    }

                    text = this.ValueEntry.SmartValue != null ? ("Select " + this.ValueEntry.SmartValue.name) : "Select GO (null)";

                    if (GUILayout.Button(GUIHelper.TempContent(text)))
                    {
                        Selection.activeObject = this.ValueEntry.SmartValue;
                        GUIUtility.ExitGUI();
                    }

                    GUIHelper.PopGUIEnabled();
                }
                GUILayout.EndHorizontal();
                return;
            }

            if (this.editor != null && this.editor.SafeIsUnityNull() == false)
            {
                SaveLayoutSettings();
                InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth++;
                try
                {
                    if (!this.targetIsOpenForEdit)
                    {
                        GUIHelper.PushGUIEnabled(false);
                    }

                    bool split = this.drawGUI && this.drawPreview;
                    if (split)
                    {
                        GUILayout.BeginHorizontal();
                        if (Event.current.type == EventType.Repaint)
                        {
                            this.inlineEditorRect = GUIHelper.GetCurrentLayoutRect();
                        }
                        GUILayout.BeginVertical();
                    }

                    // Brace for impact
                    if (this.drawHeader)
                    {
                        var tmp = Event.current.rawType;
                        EditorGUILayout.BeginFadeGroup(0.9999f); // This one fixes some layout issues for reasons beyond me, but locks the input.
                        Event.current.type = tmp;                // Lets undo that shall we?
                        GUILayout.Space(0);                      // Yeah i know. But it removes some unwanted top padding.
                        this.editor.DrawHeader();
                        GUILayout.Space(1);                      // This adds the the 1 pixel border clipped from the fade group.
                        EditorGUILayout.EndFadeGroup();
                    }
                    else
                    {
                        // Many of unity editors will not work if the header is not drawn.
                        // So lets draw it anyway. -_-
                        GUIHelper.BeginDrawToNothing();
                        this.editor.DrawHeader();
                        GUIHelper.EndDrawToNothing();
                    }

                    if (this.drawGUI)
                    {
                        var prev = GeneralDrawerConfig.Instance.ShowMonoScriptInEditor;
                        try
                        {
                            GeneralDrawerConfig.Instance.ShowMonoScriptInEditor = false;
                            this.editor.OnInspectorGUI();
                        }
                        finally
                        {
                            GeneralDrawerConfig.Instance.ShowMonoScriptInEditor = prev;
                        }
                    }

                    if (split)
                    {
                        GUILayout.EndVertical();
                    }

                    if (!this.targetIsOpenForEdit)
                    {
                        GUIHelper.PopGUIEnabled();
                    }

                    // previewEditor.HasPreviewGUI() reports 'false' for GameObject from the scene. But the user has asked for a preview, so a preview they'll get!
                    if (this.drawPreview && (this.previewEditor.HasPreviewGUI() || this.previewEditor.target is GameObject))
                    {
                        Rect tmpRect;

                        var size = split ? this.Attribute.PreviewWidth : this.Attribute.PreviewHeight;

                        if (split)
                        {
                            tmpRect = GUILayoutUtility.GetRect(size + 15, size, GUILayoutOptions.Width(size).Height(size));
                        }
                        else
                        {
                            tmpRect = GUILayoutUtility.GetRect(0, size, GUILayoutOptions.Height(size).ExpandWidth(true));
                        }

                        if (!split && Event.current.type == EventType.Repaint || this.inlineEditorRect.height < 1)
                        {
                            this.inlineEditorRect = tmpRect;
                        }

                        var rect = this.inlineEditorRect;
                        if (split)
                        {
                            rect.xMin += rect.width - size;
                        }
                        rect.height = Mathf.Clamp(rect.height, 30, 1000);
                        rect.width = Mathf.Clamp(rect.width, 30, 1000);
                        var tmp = GUI.enabled;
                        GUI.enabled = true;
                        this.previewEditor.DrawPreview(rect);
                        GUI.enabled = tmp;
                    }

                    if (split)
                    {
                        GUILayout.EndHorizontal();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.IsExitGUIException())
                    {
                        throw ex.AsExitGUIException();
                    }
                    else
                    {
                        Debug.LogException(ex);
                    }
                }
                finally
                {
                    InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth--;
                    RestoreLayout();
                }
            }
        }

        private void UpdateEditors()
        {
            this.targetIsOpenForEdit = true;

            var unityObj = (UnityEngine.Object)this.ValueEntry.WeakSmartValue;

            if (this.editor != null && !unityObj)
            {
                this.DestroyEditors();
            }

            bool createNewEditor = unityObj != null && (this.editor == null || this.target != unityObj || this.target == null);

            if (createNewEditor && this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
            {
                if (this.ValueEntry.WeakValues[0] == null)
                {
                    createNewEditor = false;
                }

                if (createNewEditor)
                {
                    var type = this.ValueEntry.WeakValues[0].GetType();

                    for (int i = 1; i < this.ValueEntry.ValueCount; i++)
                    {
                        if (!this.ValueEntry.Values[i] || this.ValueEntry.Values[i].GetType() != type)
                        {
                            createNewEditor = false;
                            break;
                        }
                    }
                }

                if (!createNewEditor)
                {
                    SirenixEditorGUI.InfoMessageBox("Cannot perform multi-editing on objects of different type.");
                }
            }

            if (createNewEditor)
            {
                this.target = unityObj;
                bool isGameObject = unityObj as GameObject;
                this.drawHeader = isGameObject ? this.Attribute.DrawHeader : this.Attribute.DrawHeader;
                this.drawGUI = isGameObject ? false : this.Attribute.DrawGUI;
                this.drawPreview = this.Attribute.DrawPreview || isGameObject && this.Attribute.DrawGUI;

                if (this.editor != null)
                {
                    this.DestroyEditors();
                }

                this.editor = Editor.CreateEditor(this.ValueEntry.WeakValues.FilterCast<UnityEngine.Object>().ToArray());

                var component = this.target as Component;
                if (component != null)
                {
                    this.previewEditor = Editor.CreateEditor(component.gameObject);
                }
                else
                {
                    this.previewEditor = this.editor;
                }

                var materialEditor = this.editor as MaterialEditor;
                if (materialEditor != null && materialForceVisibleProperty != null)
                {
                    materialForceVisibleProperty.SetValue(materialEditor, true, null);
                }

                if (this.Attribute.DisableGUIForVCSLockedAssets && AssetDatabase.Contains(this.target))
                {
                    this.targetIsOpenForEdit = AssetDatabase.IsOpenForEdit(this.target);
                }
            }
        }

        private void DestroyEditors()
        {
            this.targetIsOpenForEdit = true;

            if (this.previewEditor != this.editor && this.previewEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(this.previewEditor);
                this.previewEditor = null;
            }

            if (this.editor != null)
            {
                UnityEngine.Object.DestroyImmediate(this.editor);
                this.editor = null;
            }
        }

        private static void SaveLayoutSettings()
        {
            layoutSettingsStack.Push(new LayoutSettings()
            {
                Skin = GUI.skin,
                Color = GUI.color,
                ContentColor = GUI.contentColor,
                BackgroundColor = GUI.backgroundColor,
                Enabled = GUI.enabled,
                IndentLevel = EditorGUI.indentLevel,
                FieldWidth = EditorGUIUtility.fieldWidth,
                LabelWidth = GUIHelper.ActualLabelWidth,
                HierarchyMode = EditorGUIUtility.hierarchyMode,
                WideMode = EditorGUIUtility.wideMode,
            });
        }

        private static void RestoreLayout()
        {
            var settings = layoutSettingsStack.Pop();

            GUI.skin = settings.Skin;
            GUI.color = settings.Color;
            GUI.contentColor = settings.ContentColor;
            GUI.backgroundColor = settings.BackgroundColor;
            GUI.enabled = settings.Enabled;
            EditorGUI.indentLevel = settings.IndentLevel;
            EditorGUIUtility.fieldWidth = settings.FieldWidth;
            GUIHelper.BetterLabelWidth = settings.LabelWidth;
            EditorGUIUtility.hierarchyMode = settings.HierarchyMode;
            EditorGUIUtility.wideMode = settings.WideMode;
        }

        void IDisposable.Dispose()
        {
            this.DestroyEditors();
        }

        private struct LayoutSettings
        {
            public GUISkin Skin;
            public Color Color;
            public Color ContentColor;
            public Color BackgroundColor;
            public bool Enabled;
            public int IndentLevel;
            public float FieldWidth;
            public float LabelWidth;
            public bool HierarchyMode;
            public bool WideMode;
        }
    }
}
#endif