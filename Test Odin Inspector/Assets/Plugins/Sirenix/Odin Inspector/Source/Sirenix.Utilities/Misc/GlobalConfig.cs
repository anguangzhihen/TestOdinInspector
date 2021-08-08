//-----------------------------------------------------------------------
// <copyright file="GlobalConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// <para>
    /// A GlobalConfig singleton, automatically created and saved as a ScriptableObject in the project at the specified path.
    /// This only happens if the UnityEditor is present. If it's not, a non-persistent ScriptableObject is created at run-time.
    /// </para>
    /// <para>
    /// Remember to locate the path within a resources folder if you want the config file to be loaded at runtime without the Unity editor being present.
    /// </para>
    /// <para>
    /// The asset path is specified by defining a <see cref="GlobalConfigAttribute"/>. If no attribute is defined it will be saved in the root assets folder.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// [GlobalConfig("Assets/Resources/MyConfigFiles/")]
    /// public class MyGlobalConfig : GlobalConfig&lt;MyGlobalConfig&gt;
    /// {
    ///     public int MyGlobalVariable;
    /// }
    ///
    /// void SomeMethod()
    /// {
    ///     int value = MyGlobalConfig.Instance.MyGlobalVariable;
    /// }
    /// </code>
    /// </example>
    public abstract class GlobalConfig<T> : ScriptableObject where T : GlobalConfig<T>, new()
    {
        private static GlobalConfigAttribute configAttribute;

        // Referenced via reflection by EditorOnlyModeConfig
        private static GlobalConfigAttribute ConfigAttribute
        {
            get
            {
                if (configAttribute == null)
                {
                    configAttribute = typeof(T).GetCustomAttribute<GlobalConfigAttribute>();

                    if (configAttribute == null)
                    {
                        configAttribute = new GlobalConfigAttribute(typeof(T).GetNiceName());
                    }
                }

                return configAttribute;
            }
        }

        private static T instance;

        /// <summary>
        /// Gets a value indicating whether this instance has instance loaded.
        /// </summary>
        public static bool HasInstanceLoaded
        {
            get
            {
                return GlobalConfig<T>.instance != null;
            }
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (GlobalConfig<T>.instance == null)
                {
                    //if (!ConfigAttribute.UseAsset)
                    //{
                    //    GlobalConfig<T>.instance = ScriptableObject.CreateInstance<T>();
                    //    GlobalConfig<T>.instance.name = typeof(T).GetNiceName();
                    //}
                    //else
                    {
                        GlobalConfig<T>.LoadInstanceIfAssetExists();

                        T inst = GlobalConfig<T>.instance;

#if UNITY_EDITOR
                        string fullPath = Application.dataPath + "/" + ConfigAttribute.AssetPath + typeof(T).GetNiceName() + ".asset";

                        if (inst == null && UnityEditor.EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
                        {
                            Debug.LogWarning(ConfigAttribute.AssetPath + typeof(T).GetNiceName() + ".asset" + " was prevented from being generated because the PREVENT_SIRENIX_FILE_GENERATION key was defined in Unity's EditorPrefs.");
                            GlobalConfig<T>.instance = ScriptableObject.CreateInstance<T>();
                            return GlobalConfig<T>.instance;
                        }

                        if (inst == null)
                        {
                            if (File.Exists(fullPath) && UnityEditor.EditorSettings.serializationMode == UnityEditor.SerializationMode.ForceText)
                            {
                                if (Editor.AssetScriptGuidUtility.TryUpdateAssetScriptGuid(fullPath, typeof(T)))
                                {
                                    Debug.Log("Could not load config asset at first, but successfully detected forced text asset serialization, and corrected the config asset m_Script guid.");
                                    GlobalConfig<T>.LoadInstanceIfAssetExists();
                                    inst = GlobalConfig<T>.instance;
                                }
                                else
                                {
                                    Debug.LogWarning("Could not load config asset, and failed to auto-correct config asset m_Script guid.");
                                }
                            }
                        }
#endif

                        if (inst == null)
                        {
                            inst = ScriptableObject.CreateInstance<T>();

#if UNITY_EDITOR
                            // TODO: What do we do if it gives us a path to a package?
                            // Can we figure out where the package is actually located, and can we assume we have write rights?
                            // Can we use purely the AssetDatabase and not do any IO manually?

                            if (!Directory.Exists(ConfigAttribute.AssetPathWithAssetsPrefix))
                            {
                                Directory.CreateDirectory(new DirectoryInfo(ConfigAttribute.AssetPathWithAssetsPrefix).FullName);
                                UnityEditor.AssetDatabase.Refresh();
                            }

                            string niceName = typeof(T).GetNiceName();

                            string assetPath;
                            if (ConfigAttribute.AssetPath.StartsWith("Assets/"))
                            {
                                assetPath = ConfigAttribute.AssetPath + niceName + ".asset";
                            }
                            else
                            {
                                assetPath = "Assets/" + ConfigAttribute.AssetPath + niceName + ".asset";
                            }

                            if (File.Exists(fullPath))
                            {
                                Debug.LogWarning(
                                    "Could not load config asset of type " + niceName + " from project path '" + assetPath + "', " +
                                    "but an asset file already exists at the path, so could not create a new asset either. The config " +
                                    "asset for '" + niceName + "' has been lost, probably due to an invalid m_Script guid. Set forced " +
                                    "text serialization in Edit -> Project Settings -> Editor -> Asset Serialization -> Mode and trigger " +
                                    "a script reload to allow Odin to auto-correct this.");
                            }
                            else
                            {
                                UnityEditor.AssetDatabase.CreateAsset(inst, assetPath);
                                UnityEditor.AssetDatabase.SaveAssets();
                                GlobalConfig<T>.instance = inst;
                                inst.OnConfigAutoCreated();
                                UnityEditor.EditorUtility.SetDirty(inst);
                                UnityEditor.AssetDatabase.SaveAssets();
                                UnityEditor.AssetDatabase.Refresh();

                            }
#endif
                        }

                        GlobalConfig<T>.instance = inst;
                    }
                }

                return GlobalConfig<T>.instance;
            }
        }

        /// <summary>
        /// Tries to load the singleton instance.
        /// </summary>
        public static void LoadInstanceIfAssetExists()
        {
            if (ConfigAttribute.IsInResourcesFolder)
            {
                string niceName = typeof(T).GetNiceName();
                GlobalConfig<T>.instance = Resources.Load<T>(ConfigAttribute.ResourcesPath + niceName);
            }
#if UNITY_EDITOR
            else
            {
                string niceName = typeof(T).GetNiceName();
                GlobalConfig<T>.instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(ConfigAttribute.AssetPath + niceName + ".asset");

                // It could be a package and not located in the Assets folder:
                if (GlobalConfig<T>.instance == null)
                {
                    GlobalConfig<T>.instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/" + ConfigAttribute.AssetPath + niceName + ".asset");
                }
            }

            // If it is relocated
            if (GlobalConfig<T>.instance == null)
            {
                var relocatedScriptableObject = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);
                if (relocatedScriptableObject.Length > 0)
                {
                    GlobalConfig<T>.instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(relocatedScriptableObject[0]));
                }
            }
#endif
        }

        /// <summary>
        /// Opens the config in a editor window. This is currently only used internally by the Sirenix.OdinInspector.Editor assembly.
        /// </summary>
        public void OpenInEditor()
        {
#if UNITY_EDITOR


            Type windowType = null;

            try
            {
                Assembly editorAssembly = null;

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "Sirenix.OdinInspector.Editor")
                    {
                        editorAssembly = assembly;
                        break;
                    }
                }

                if (editorAssembly != null)
                {
                    windowType = editorAssembly.GetType("Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
                }
            }
            catch
            {
            }


            if (windowType != null)
            {
                windowType.GetMethods().Where(x => x.Name == "OpenWindow" && x.GetParameters().Length == 1).First()
                    .Invoke(null, new object[] { this });
            }
            else
            {
                Debug.LogError("Failed to open window, could not find Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
            }
#else
            Debug.Log("Downloading, installing and launching the Unity Editor so we can open this config window in the editor, please stand by until pigs can fly and hell has frozen over...");
#endif
        }

        protected virtual void OnConfigAutoCreated()
        {
        }
    }
}