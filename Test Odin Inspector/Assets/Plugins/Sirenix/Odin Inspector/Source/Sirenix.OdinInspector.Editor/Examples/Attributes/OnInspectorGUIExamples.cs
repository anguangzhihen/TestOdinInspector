#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnInspectorGUIExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(OnInspectorGUIAttribute))]
    internal class OnInspectorGUIExamples
    {
        [OnInspectorInit("@Texture = EditorIcons.OdinInspectorLogo")]
        [OnInspectorGUI("DrawPreview", append: true)]
        public Texture2D Texture;

        private void DrawPreview()
        {
            if (this.Texture == null) return;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(this.Texture);
            GUILayout.EndVertical();
        }

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("OnInspectorGUI can also be used on both methods and properties", UnityEditor.MessageType.Info);
        }
#endif
    }
}
#endif