#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EditorPrefsUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    internal static class EditorPrefsUtilities
    {
        public static string ConvertToProjectKey(string key)
        {
            return Application.dataPath + key;
        }

        public static void SaveList(string key, List<string> list)
        {
            var listLengthKey = key + ".length";
            ClearList(key);

            EditorPrefs.SetInt(listLengthKey, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                EditorPrefs.SetString(key + "[" + i + "]", list[i]);
            }
        }

        public static List<string> LoadList(string key)
        {
            var listLengthKey = key + ".length";
            if (!EditorPrefs.HasKey(listLengthKey))
            {
                return new List<string>();
            }

            List<string> result = new List<string>();
            var listLength = EditorPrefs.GetInt(listLengthKey);
            for (int i = 0; i < listLength; i++)
            {
                result.Add(EditorPrefs.GetString(key + "[" + i + "]", ""));
            }
            return result;
        }

        public static void ClearList(string key)
        {
            var listLengthKey = key + ".length";
            var listLength = EditorPrefs.GetInt(listLengthKey, 0);
            for (int i = 0; i < listLength; i++)
            {
                EditorPrefs.DeleteKey(key + "[" + i + "]");
            }
            EditorPrefs.DeleteKey(listLengthKey);
        }
    }
}
#endif