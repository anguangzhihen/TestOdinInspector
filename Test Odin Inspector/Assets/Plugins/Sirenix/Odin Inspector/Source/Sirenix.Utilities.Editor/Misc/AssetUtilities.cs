#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Utility functions for Unity assets.
    /// </summary>
    public static class AssetUtilities
    {
        /// <summary>
        /// Gets all assets of the specified type.
        /// </summary>
        public static IEnumerable<T> GetAllAssetsOfType<T>() where T : UnityEngine.Object
        {
            foreach (var item in GetAllAssetsOfType(typeof(T)))
            {
                yield return (T)item;
            }
        }

        private static List<Component> componentListBuffer = new List<Component>();

        /// <summary>
        /// Gets all assets of the specified type.
        /// </summary>
		/// <param name="type">The type of assets to find.</param>
        /// <param name="folderPath">The asset folder path.</param>
        public static IEnumerable<UnityEngine.Object> GetAllAssetsOfType(Type type, string folderPath = null)
        {
            foreach (var result in GetAllAssetsOfTypeWithProgress(type, folderPath))
            {
                yield return result.Asset;
            }
        }

        /// <summary>
        /// Gets all assets of the specified type.
        /// </summary>
        /// <param name="type">The type of assets to find.</param>
        /// <param name="folderPath">The asset folder path.</param>
        public static IEnumerable<AssetSearchResult> GetAllAssetsOfTypeWithProgress(Type type, string folderPath = null)
        {
            AssetSearchResult item = new AssetSearchResult();

            if (folderPath != null)
            {
                // It's okay to use 'Assets/' here as it's only used by the AssetList attribute drawer.
                folderPath = folderPath.Trim('/');
                if (folderPath.StartsWith("Assets/", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    folderPath = "Assets/" + folderPath;
                }
            }

            if (type == typeof(GameObject))
            {
                string[] goGuids = folderPath == null ? AssetDatabase.FindAssets("t:Prefab") : AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });

                item.NumberOfResults = goGuids.Length;

                for (int i = 0; i < goGuids.Length; i++)
                {
                    string goPath = AssetDatabase.GUIDToAssetPath(goGuids[i]);
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(goPath);

                    item.CurrentIndex = i;
                    item.Asset = go;
                    yield return item;
                }
            }
            else if (type.InheritsFrom(typeof(Component)))
            {
                string[] goGuids = folderPath == null ? AssetDatabase.FindAssets("t:Prefab") : AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });

                item.NumberOfResults = goGuids.Length;

                for (int i = 0; i < goGuids.Length; i++)
                {
                    string goPath = AssetDatabase.GUIDToAssetPath(goGuids[i]);
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(goPath);

                    go.GetComponents(type, componentListBuffer); // Consider using GetComponentsInChildren if performance is not a problem.

                    item.CurrentIndex = i;

                    for (int j = 0; j < componentListBuffer.Count; j++)
                    {
                        item.Asset = componentListBuffer[j];
                        yield return item;
                    }
                }
            }
            else
            {
                bool isProbablyUnityType = type.FullName.StartsWith("UnityEngine.") || type.FullName.StartsWith("UnityEditor.");
                string typeNameToUse = isProbablyUnityType ? type.Name : type.FullName;

                string[] guids = folderPath == null ? AssetDatabase.FindAssets("t:" + typeNameToUse) : AssetDatabase.FindAssets("t:" + typeNameToUse, new string[] { folderPath });

                item.NumberOfResults = guids.Length;
                for (int i = 0; i < guids.Length; i++)
                {
                    item.CurrentIndex = i;
                    item.Asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), type);
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Asset search helper.
        /// </summary>
        public struct AssetSearchResult
        {
            /// <summary>
            /// The asset object.
            /// </summary>
            public UnityEngine.Object Asset;

            /// <summary>
            /// Current index.
            /// </summary>
            public int CurrentIndex;

            /// <summary>
            /// Search result count.
            /// </summary>
            public int NumberOfResults;
        }

        private static readonly Type[] createableAssetTypes = new Type[] {
            typeof(ScriptableObject),
            typeof(MonoBehaviour),
            typeof(GameObject)
        };

        /// <summary>
        /// Tests if an asset can be created from a type.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <returns><c>true</c> if an asset can be created. Otherwise <c>false</c>.</returns>
        public static bool CanCreateNewAsset<T>()
        {
            for (int i = 0; i < createableAssetTypes.Length; i++)
            {
                if (typeof(T).InheritsFrom(createableAssetTypes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tests if an asset can be created from a type.
        /// </summary>
        /// <typeparam name="T">The type to test.</typeparam>
        /// <param name="baseType">The base asset type.</param>
        /// <returns><c>true</c> if an asset can be created. Otherwise <c>false</c>.</returns>
        public static bool CanCreateNewAsset<T>(out Type baseType)
        {
            for (int i = 0; i < createableAssetTypes.Length; i++)
            {
                if (typeof(T).InheritsFrom(createableAssetTypes[i]))
                {
                    baseType = createableAssetTypes[i];
                    return true;
                }
            }
            baseType = null;
            return false;
        }

        /// <summary>
        /// Gets project path to the specified asset.
        /// </summary>
        /// <param name="obj">The asset object.</param>
        /// <returns>The path to the asset.</returns>
        public static string GetAssetLocation(UnityEngine.Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            return path.Substring(0, path.LastIndexOf('/'));
        }

        /// <summary>
        /// Creates a new asset of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <param name="path">Project path to the new asset.</param>
        /// <param name="assetName">The name of the asset.</param>
        [Obsolete("This will eventually be removed and is only used by the AssetList attribute drawer. Use the AssetDatabase manually instead.")]
        public static void CreateNewAsset<T>(string path, string assetName) where T : UnityEngine.Object
        {
            Type assetBaseType;
            if (!CanCreateNewAsset<T>(out assetBaseType))
            {
                Debug.LogError("Unable to create new asset of type " + typeof(T).GetNiceName());
                return;
            }

            if (path == null)
            {
                path = "";
            }
            else
            {
                path = path.Trim().TrimStart('/').TrimEnd('/').Trim();
                if (path.ToLower(CultureInfo.InvariantCulture).StartsWith("assets", StringComparison.InvariantCulture))
                {
                    path = path.Substring(6, path.Length - 6).TrimStart('/');
                }
            }

            string fullPath = Application.dataPath + "/" + path;
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            assetName = assetName ?? typeof(T).GetNiceName();
            if (assetName.IndexOf('.') < 0)
            {
                assetName = assetName + "." + GetAssetFileExtensionName(assetBaseType);
            }
            path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + path + "/" + assetName);

            UnityEngine.Object asset;
            GameObject prefab = null;

            if (assetBaseType == typeof(ScriptableObject))
            {
                asset = ScriptableObject.CreateInstance(typeof(T));
            }
            else if (assetBaseType == typeof(MonoBehaviour))
            {
                GameObject go = new GameObject();

                //Undo.RegisterCreatedObjectUndo(go, "Create Asset");

                go.AddComponent(typeof(T));

                //Undo.AddComponent(go, typeof(T));
                //Undo.IncrementCurrentGroup();

                asset = go;
                prefab = go;
            }
            else if (assetBaseType == typeof(GameObject))
            {
                asset = prefab = new GameObject();
            }
            else
            {
                throw new NotImplementedException();
            }

            if (prefab != null)
            {
                asset = PrefabUtility.CreatePrefab(path, prefab);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(asset);

            if (prefab != null)
            {
                GameObject.DestroyImmediate(prefab);
            }
        }

        private static string GetAssetFileExtensionName(Type type)
        {
            if (type == typeof(ScriptableObject))
            {
                return "asset";
            }
            else if (type == typeof(GameObject) || type == typeof(MonoBehaviour))
            {
                return "prefab";
            }

            return null;
        }
    }
}
#endif