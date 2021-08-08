#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SirenixPreferencesWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Serialization;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using System;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.Modules;

    /// <summary>
    /// Sirenix preferences window.
    /// </summary>
    public class SirenixPreferencesWindow : OdinMenuEditorWindow
    {
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true)
            {
                { "General",                    GeneralDrawerConfig.Instance},
                { "Editor Types",               InspectorConfig.Instance},
                { "Persistent Context Cache",   PersistentContextCache.Instance},
                { "Color Palettes",             ColorPaletteManager.Instance},
                { "Serialization",              GlobalSerializationConfig.Instance},
                { "Import Settings",            ImportSettingsConfig.Instance},
                { "AOT Generation",             AOTGenerationConfig.Instance},
                { "Editor Only Mode",           EditorOnlyModeConfig.Instance},
                { "Modules",                    OdinModuleConfig.Instance},
            };

            return tree;
        }

        protected override void DrawMenu()
        {
            base.DrawMenu();
            var rect = GUIHelper.GetCurrentLayoutRect().Padding(4).AlignBottom(20);
            GUI.Label(rect, "Odin Inspector Version " + typeof(InspectorConfig).Assembly.GetName(false).Version.ToString(), SirenixGUIStyles.CenteredGreyMiniLabel);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.DefaultLabelWidth = 278;
            this.ResizableMenuWidth = false;
        }

        /// <summary>
        /// Open preferences page for configuration object.
        /// </summary>
        [Obsolete("Use OpenWindow instead.")]
        public static void OpenGlobalConfigWindow<T>(string title, UnityEngine.Object selectedConfig)
        {
            OpenWindow(selectedConfig);
        }

        /// <summary>
        /// Opens the Odin inspector preferences window.
        /// </summary>
        public static void OpenSirenixPreferences()
        {
            var window = GetWindow<SirenixPreferencesWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 600);
            window.titleContent = new GUIContent("Preferences", EditorIcons.OdinInspectorLogo);
        }

        /// <summary>
        /// Opens the Odin inspector preferences window.
        /// </summary>
        public static void OpenWindow(UnityEngine.Object selectedItem)
        {
            var window = GetWindow<SirenixPreferencesWindow>();
            window.TrySelectMenuItemWithObject(selectedItem);
            window.titleContent = new GUIContent("Preferences", EditorIcons.OdinInspectorLogo);
        }
    }
}
#endif