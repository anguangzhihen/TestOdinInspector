#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinInspectorAboutWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using System;

    /// <summary>
    /// Adds menu items to the Unity Editor, draws the About window, and the preference window found under Edit > Preferences > Odin Inspector.
    /// </summary>
    public class OdinInspectorAboutWindow : EditorWindow
    {
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, this.position.width - 20f, this.position.height - 5f));

            string subtitle = OdinInspectorVersion.BuildName;

            SirenixEditorGUI.Title("Odin Inspector & Serializer", subtitle, TextAlignment.Left, true);
            DrawAboutGUI();
            GUILayout.EndArea();
            this.RepaintIfRequested();
        }

        [PreferenceItem("Odin Inspector")]
        private static void OnPreferencesGUI()
        {
            DrawAboutGUI();
            Rect rect = EditorGUILayout.GetControlRect();

            if (GUI.Button(new Rect(rect) { y = rect.y + 70f, height = 25f, }, "Get started using Odin"))
            {
                OdinGettingStartedWindow.ShowWindow();
            }

            if (GUI.Button(new Rect(rect) { y = rect.y + 70f + 30, height = 25f, }, "Show Odin Preferences"))
            {
                SirenixPreferencesWindow.OpenSirenixPreferences();
            }

            GUIHelper.RepaintIfRequested(GUIHelper.CurrentWindow);
        }

        internal static void DrawAboutGUI()
        {
            Rect position = new Rect(EditorGUILayout.GetControlRect()) { height = 90f };

            // Logo
            GUI.DrawTexture(position.SetWidth(86).SetHeight(75).AddY(4).AddX(-5), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleAndCrop);

            // About
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 0f - 2f, height = 18f, }, OdinInspectorVersion.Version, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 1f - 2f, height = 18f, }, "Developed and published by Sirenix", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 2f - 2f, height = 18f, }, "All rights reserved", SirenixGUIStyles.LeftAlignedGreyMiniLabel);

            var linkStyle = EditorStyles.miniButton;
            float width = linkStyle.CalcSize(GUIHelper.TempContent("www.odininspector.com")).x;

            // Links
            DrawLink(new Rect(position) { x = position.xMax - width, y = position.y + 20f * 0f, width = width, height = 14f, }, "www.odininspector.com",    "https://odininspector.com",    linkStyle);
        }

        private static void DrawLink(Rect rect, string label, string link, GUIStyle style)
        {
            if (GUI.Button(rect, label, style))
            {
                Application.OpenURL(link);
            }
        }
    }
}
#endif