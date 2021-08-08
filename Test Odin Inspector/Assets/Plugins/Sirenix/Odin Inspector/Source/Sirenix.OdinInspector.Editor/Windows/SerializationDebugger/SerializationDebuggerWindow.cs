#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SerializationDebuggerWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The Odin Inspector Serialization Debugger Window.
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.Editor.OdinEditorWindow" />
    public sealed class SerializationDebuggerWindow : OdinEditorWindow
    {
        private const string TargetTypePrefKey = "SerializationDebuggerWindow.TargetType";

        [SerializeField, HideInInspector]
        private Type targetType;

        [SerializeField, HideInInspector]
        private bool odinContext;

        [NonSerialized]
        private OdinMenuTree serializationInfoTree;

        [NonSerialized]
        private SerializationBackendFlags backendFlags;

        [NonSerialized]
        private GUIStyle noteStyle;

        [OnInspectorGUI]
        private void DrawTopBar()
        {
            Rect rect = SirenixEditorGUI.BeginHorizontalToolbar();
            {
                var iconRect = rect.AlignLeft(SerializationInfoMenuItem.IconSize).AlignMiddle(SerializationInfoMenuItem.IconSize);
                iconRect.x += SerializationInfoMenuItem.IconSpacing * 2;
                GUI.color = (this.backendFlags & SerializationBackendFlags.Odin) != 0 ? Color.white : new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(iconRect.Padding(2), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleToFit);
                iconRect.x += SerializationInfoMenuItem.IconSize + SerializationInfoMenuItem.IconSpacing * 2;
                GUI.color = (this.backendFlags & SerializationBackendFlags.Unity) != 0 ? Color.white : new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(iconRect.Padding(2), EditorIcons.UnityLogo, ScaleMode.ScaleToFit);
                GUI.color = Color.white;

                var typeName = "   " + (this.targetType == null ? "Select Type" : this.targetType.GetNiceName().SplitPascalCase()) + "   ";
                GUILayout.Space(iconRect.xMax + 3);
                bool selectB = SirenixEditorGUI.ToolbarButton(new GUIContent(typeName));
                GUILayout.FlexibleSpace();
                bool selectA = SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleDown);

                if (selectA || selectB)
                {
                    var btnRect = GUIHelper.GetCurrentLayoutRect().HorizontalPadding(20).AlignTop(20);
                    btnRect = btnRect.AlignRight(400);
                    var source = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
                        .Where(x => !x.IsAbstract && x.IsClass && x.InheritsFrom<UnityEngine.Object>())
                        .Where(x => !x.Assembly.FullName.StartsWith("Sirenix"))
                        .OrderBy(x => x.Assembly.GetAssemblyTypeFlag())
                        .OrderBy(x => x.Assembly.GetAssemblyTypeFlag())
                        .ThenBy(x => x.Namespace)
                        .ThenByDescending(x => x.Name);

                    var p = new TypeSelector(source, false);

                    p.SelectionChanged += (types) =>
                    {
                        var t = types.FirstOrDefault();
                        if (t != null)
                        {
                            this.targetType = t;
                            this.odinContext = this.targetType.IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(true);
                            this.CreateMenuTree(true);
                        }
                    };

                    p.SetSelection(this.targetType);
                    p.ShowInPopup(300);
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void CreateMenuTree(bool force)
        {
            if (force || (this.targetType != null && this.serializationInfoTree == null))
            {
                EditorPrefs.SetString(TargetTypePrefKey, TwoWaySerializationBinder.Default.BindToName(this.targetType));

                this.backendFlags = this.targetType.IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(true) ? SerializationBackendFlags.UnityAndOdin : SerializationBackendFlags.Unity;

                var infos = MemberSerializationInfo.CreateSerializationOverview(this.targetType, this.backendFlags, this.odinContext);
                this.serializationInfoTree = new OdinMenuTree(false);
                this.serializationInfoTree.DefaultMenuStyle.Offset = (SerializationInfoMenuItem.IconSpacing * 2 + SerializationInfoMenuItem.IconSize) * 2 + SerializationInfoMenuItem.IconSpacing * 2;
                this.serializationInfoTree.DefaultMenuStyle.Height = 27;
                this.serializationInfoTree.DefaultMenuStyle.BorderPadding = 0;
                this.serializationInfoTree.Config.DrawSearchToolbar = true;
                this.serializationInfoTree.Config.AutoHandleKeyboardNavigation = true;
                foreach (var item in infos)
                {
                    this.serializationInfoTree.MenuItems.Add(new SerializationInfoMenuItem(this.serializationInfoTree, item.MemberInfo.Name, item));
                }
            }
        }

        [OnInspectorGUI]
        private void DrawSerializationInfoTree()
        {
            EditorGUILayout.BeginVertical(GUILayoutOptions.ExpandHeight());
            this.CreateMenuTree(false);

            if (this.serializationInfoTree != null)
            {
                this.serializationInfoTree.DrawMenuTree();
            }
            EditorGUILayout.EndVertical();
        }

        [OnInspectorGUI]
        private void DrawInfos()
        {
            if (this.serializationInfoTree == null)
            {
                return;
            }

            if (this.noteStyle == null)
            {
                this.noteStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
                this.noteStyle.active.textColor = this.noteStyle.normal.textColor;
                this.noteStyle.onActive.textColor = this.noteStyle.normal.textColor;
                this.noteStyle.onFocused.textColor = this.noteStyle.normal.textColor;
                this.noteStyle.focused.textColor = this.noteStyle.normal.textColor;
                this.noteStyle.margin = new RectOffset(20, 4, 0, 4);
                this.noteStyle.padding = new RectOffset(0, 0, 0, 0);
            }

            if (this.serializationInfoTree.Selection.Count > 0)
            {
                var info = this.serializationInfoTree.Selection[0].Value as MemberSerializationInfo;

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    var bgRect = GUIHelper.GetCurrentLayoutRect().Expand(0, 10);
                    SirenixEditorGUI.DrawSolidRect(bgRect, SirenixGUIStyles.DarkEditorBackground);
                    SirenixEditorGUI.DrawBorders(bgRect, 0, 0, 1, 0);

                    // Note text.
                    GUILayout.BeginVertical(GUILayoutOptions.MinHeight(80));
                    {
                        foreach (var note in info.Notes)
                        {
                            Rect noteRect = GUILayoutUtility.GetRect(GUIHelper.TempContent(note), this.noteStyle);
                            var dot = noteRect;
                            dot.x -= 8;
                            dot.y += 5;
                            dot.height = 4;
                            dot.width = 4;
                            SirenixEditorGUI.DrawSolidRect(dot, EditorGUIUtility.isProSkin ? Color.white : Color.black);
                            EditorGUI.SelectableLabel(noteRect, note, this.noteStyle);
                            GUILayout.Space(4);
                        }
                    }
                    GUILayout.EndVertical();

                    var r = GUIHelper.GetCurrentLayoutRect();
                    SirenixEditorGUI.DrawVerticalLineSeperator(r.x, r.y, r.height);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }

        [OnInspectorGUI]
        private void DrawGettingStartedHelp()
        {
            if (this.targetType == null)
            {
                GUIContent content = GUIHelper.TempContent("Select your script here to begin debugging the serialization.", EditorIcons.UnityInfoIcon);
                var size = new Vector2(Mathf.Max(this.position.width - 100, 200), 0);
                size.y = SirenixGUIStyles.MessageBox.CalcHeight(content, size.x);

                GUI.Label(new Rect(50, 40, size.x, size.y), content, SirenixGUIStyles.MessageBox);
                EditorIcons.ArrowUp.Draw(new Rect(95f, 25f, 20f, 20f), EditorIcons.ArrowUp.Raw);
            }
        }

        /// <summary>
        /// Opens the Serialization Debugger Window with the last debugged type.
        /// </summary>
        public static void ShowWindow()
        {
            Type target = null;
            string typeName = EditorPrefs.GetString(TargetTypePrefKey, null);
            if (typeName != null)
            {
                target = TwoWaySerializationBinder.Default.BindToType(typeName);
            }

            ShowWindow(target);
        }

        /// <summary>
        /// Opens the Serialization Debugger Window and debugs the given type.
        /// </summary>
        /// <param name="type">The type to debug serialization of.</param>
        public static void ShowWindow(Type type)
        {
            SerializationDebuggerWindow window = Resources.FindObjectsOfTypeAll<SerializationDebuggerWindow>().FirstOrDefault();

            if (window == null)
            {
                window = GetWindow<SerializationDebuggerWindow>("Serialization Debugger");
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500f, 400f);
            }

            window.targetType = type;
            window.Show();

            if (window.targetType != null)
            {
                window.CreateMenuTree(true);
                window.Repaint();
            }
        }

        private static void ComponentContextMenuItem(MenuCommand menuCommand)
        {
            ShowWindow(menuCommand.context.GetType());
        }

        /// <summary>
        /// Initializes the Serialization Debugger Window.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            this.WindowPadding = new Vector4();
            this.minSize = new Vector2(300, 300);
        }
    }
}
#endif