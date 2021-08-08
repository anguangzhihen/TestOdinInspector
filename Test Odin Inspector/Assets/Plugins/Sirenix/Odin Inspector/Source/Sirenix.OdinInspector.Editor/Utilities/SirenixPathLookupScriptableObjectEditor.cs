#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SirenixPathLookupScriptableObjectEditor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SirenixPathLookupScriptableObject))]
    internal class SirenixPathLookupScriptableObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Don't delete this file!", SirenixGUIStyles.SectionHeader);
            GUILayout.Space(15);
            GUILayout.Label("Odin will use this file to locate where Odin is installed. That way you can freely move around the Sirenix " +
                "folder and place it anywhere you like.",
                SirenixGUIStyles.MultiLineLabel
                );
            GUILayout.Space(15);
            GUILayout.Label("<b>Why does Odin need to know where it's located?</b>", SirenixGUIStyles.MultiLineLabel);
            GUILayout.Label("There are a number of features that need to know the location of Odin. Odin will use it to make " +
                "sure you get a smooth update, without you needing to manually delete files that are no longer needed. " +
                "It'll also auto-generate various config files in the correct locations, so we don't need to include " +
                "those in the UnityPackage, making sure we never override any of your settings when you update. Many other " +
                "systems also use it, like the assembly import settings automation, the AOT support generation, the Editor " +
                "Only Mode switching logic, and so on.", 
                SirenixGUIStyles.MultiLineLabel);

            SirenixEditorGUI.DrawThickHorizontalSeperator(4, 10, 10);

            GUILayout.Label("Expected Asset Guid: " + SirenixAssetPaths.SirenixAssetPathsSOGuid);
            var path = AssetDatabase.GetAssetPath(this.target);
            var guid = (path == null ? "Not an Asset" : AssetDatabase.AssetPathToGUID(path));
            GUILayout.Label("Actual Asset Guid:     " + guid);
        }
    }
}
#endif