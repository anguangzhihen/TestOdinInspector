//-----------------------------------------------------------------------
// <copyright file="SirenixAssetPaths.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System.IO;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Paths to Sirenix assets.
    /// </summary>
    public static class SirenixAssetPaths
    {
        public const string DefaultSirenixPluginPath = "Assets/Plugins/Sirenix/";

        public const string SirenixAssetPathsSOGuid = "08379ccefc05200459f90a1c0711a340";

        public const string LookupAssetName = "OdinPathLookup.asset";

        /// <summary>
        /// Path to Odin Inspector folder.
        /// </summary>
        public static readonly string OdinPath;

        /// <summary>
        /// Path to Sirenix assets folder.
        /// </summary>
        public static readonly string SirenixAssetsPath;

        /// <summary>
        /// Path to Sirenix folder.
        /// </summary>
        public static readonly string SirenixPluginPath;

        /// <summary>
        /// Path to Sirenix assemblies.
        /// </summary>
        public static readonly string SirenixAssembliesPath;

        /// <summary>
        /// Path to Odin Inspector resources folder.
        /// </summary>
        public static readonly string OdinResourcesPath;

        /// <summary>
        /// Path to Odin Inspector configuration folder.
        /// </summary>
        public static readonly string OdinEditorConfigsPath;

        /// <summary>
        /// Path to Odin Inspector resources configuration folder.
        /// </summary>
        public static readonly string OdinResourcesConfigsPath;

        /// <summary>
        /// Path to Odin Inspector temporary folder.
        /// </summary>
        public static readonly string OdinTempPath;

        static SirenixAssetPaths()
        {
#if UNITY_EDITOR
            var log = false;
            var sb = new System.Text.StringBuilder();
            if (File.Exists("Assets/Plugins/Sirenix/Odin Inspector/Assets/Editor/" + LookupAssetName))
            {
                // Default:
                SirenixPluginPath = DefaultSirenixPluginPath;
            }
            else
            {
                sb.AppendLine("There were some problems trying to locate where Odin was installed, please read the messages below.");
                sb.AppendLine("- Odin was not found in it's default location: 'Assets/Plugins/Sirenix/'.");
                var pathLookupAssetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(SirenixAssetPathsSOGuid);

                if (!File.Exists(pathLookupAssetPath))
                {
                    pathLookupAssetPath = null;
                }

                if (string.IsNullOrEmpty(pathLookupAssetPath))
                {
                    log = true;
                    sb.AppendLine("- No SirenixPathLookupScriptableObject with the Guid: '" + SirenixAssetPathsSOGuid + "' was found.");
                    var results = UnityEditor.AssetDatabase.FindAssets("t:SirenixPathLookupScriptableObject") ?? new string[0];
                    var newPath = results.FirstOrDefault(x => x != null && x.Contains("Sirenix/"));
                    if (newPath == null)
                    {
                        var paths = UnityEditor.AssetDatabase.GetAllAssetPaths();

                        for (int i = 0; i < paths.Length; i++)
                        {
                            if (paths[i].FastEndsWith(LookupAssetName))
                            {
                                pathLookupAssetPath = paths[i];
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(pathLookupAssetPath))
                        {
                            sb.AppendLine("- We were unable to find Odin elsewhere. Please re-import Odin and make sure the " + LookupAssetName + " is included.");
                        }
                        else
                        {
                            sb.AppendLine("- We were able to find Odin using a very slow fallback method. This will increase your project reload times. To fix it, please re-import Odin and make sure the " + LookupAssetName + " file is included.");
                        }
                    }
                    else
                    {
                        sb.AppendLine("- The SirenixPathLookupScriptableObject was found, but must have had its Guid changed. Please report this issue with details about your project setup. You should be able to fix the issue by re-importing Odin.");
                        pathLookupAssetPath = newPath;
                    }
                }

                if (!string.IsNullOrEmpty(pathLookupAssetPath))
                {
                    var i = pathLookupAssetPath.LastIndexOf("Sirenix/", System.StringComparison.CurrentCultureIgnoreCase);

                    if (i < 0)
                    {
                        // TODO Do something
                        // TODO What something should I do todo?

                        i = pathLookupAssetPath.LastIndexOf("/Odin Inspector/", System.StringComparison.CurrentCultureIgnoreCase);

                        if (i < 0)
                        {

                            pathLookupAssetPath = DefaultSirenixPluginPath;
                            log = true;
                            sb.AppendLine("- The path Odin found ('" + pathLookupAssetPath + "') was invalid and contained neither 'Sirenix/' nor '/Odin Inspector/' - Odin's path setup has defaulted to the default path '" + DefaultSirenixPluginPath + "', which is likely wrong. You should be able to fix the issue by re-importing Odin.");
                        }
                        else
                        {
                            var path = pathLookupAssetPath.Substring(0, i);
                            var dir = new DirectoryInfo(path);

                            if (!dir.Exists)
                            {
                                SirenixPluginPath = DefaultSirenixPluginPath;
                                log = true;
                                sb.AppendLine("- The path Odin found ('" + pathLookupAssetPath + "') was invalid somehow and parts of it don't even exist (O_o?!) - Odin's path setup has defaulted to the default path '" + DefaultSirenixPluginPath + "', which is likely wrong. You should be able to fix the issue by re-importing Odin.");
                            }
                            else
                            {
                                pathLookupAssetPath = path.TrimEnd('/') + "/";
                            }
                        }
                    }
                    else
                    {
                        pathLookupAssetPath = pathLookupAssetPath.Substring(0, i + "Sirenix/".Length);
                    }

                    SirenixPluginPath = pathLookupAssetPath;
                }

                if (string.IsNullOrEmpty(pathLookupAssetPath) || log)
                {
                    Debug.LogError(sb.ToString());
                }
            }

            var companyName = ToPathSafeString(UnityEditor.PlayerSettings.companyName);
            var productName = ToPathSafeString(UnityEditor.PlayerSettings.productName);

            // Temp path
            OdinTempPath = Path.Combine(Path.GetTempPath().Replace('\\', '/'), "Sirenix/Odin/" + companyName + "/" + productName);
#else
            SirenixPluginPath = DefaultSirenixPluginPath;
#endif

            OdinPath = SirenixPluginPath + "Odin Inspector/";
            SirenixAssetsPath = SirenixPluginPath + "Assets/";
            SirenixAssembliesPath = SirenixPluginPath + "Assemblies/";
            OdinResourcesPath = OdinPath + "Config/Resources/Sirenix/";
            OdinEditorConfigsPath = OdinPath + "Config/Editor/";
            OdinResourcesConfigsPath = OdinResourcesPath;
        }

        private static string ToPathSafeString(string name, char replace = '_')
        {
            char[] invalids = Path.GetInvalidFileNameChars();
            return new string(name.Select(c => invalids.Contains(c) ? replace : c).ToArray());
        }
    }
}