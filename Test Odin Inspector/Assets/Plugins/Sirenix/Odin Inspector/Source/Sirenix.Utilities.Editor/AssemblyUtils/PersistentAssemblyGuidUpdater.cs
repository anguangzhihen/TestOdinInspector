#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PersistentAssemblyGuidUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnityEditor;
    using Debug = UnityEngine.Debug;

    internal static class PersistentAssemblyGuidUpdater
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            OdinInspector.Editor.UnityEditorEventUtility.EditorApplication_delayCall += () => UpdateGuids(false);
        }

        //[MenuItem("Assets/Update Assembly Guids")]
        private static void UpdateGuids()
        {
            UpdateGuids(true);
        }

        private static void UpdateGuids(bool log)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            bool updateAssetDatabase = false;
            if (log) Debug.Log("Scanning " + assemblies.Length + " assemblies.");
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                if (assembly.SafeGetCustomAttributes(typeof(PersistentAssemblyAttribute), true).Length == 0)
                {
                    continue;
                }

                var assemblyGuids = assembly.SafeGetCustomAttributes(typeof(GuidAttribute), true);
                if (assemblyGuids.Length == 0)
                {
                    LogError(assembly, "Assembly has is marked with PersistentAssemblyAttribute, but not a GuidAttribute.");
                    continue;
                }

                var assemblyMetaFile = assembly.GetAssemblyFilePath() + ".meta";

                if (File.Exists(assemblyMetaFile) == false)
                {
                    if (log) Debug.Log("Skipping persistent assembly: " + assembly.FullName + ", No meta file was found at: " + assemblyMetaFile);
                    continue;
                }

                var lines = File.ReadAllLines(assemblyMetaFile);
                for (int j = 0; j < lines.Length; j++)
                {
                    var line = lines[j];
                    if (line.StartsWith("guid: ", StringComparison.InvariantCultureIgnoreCase) && line.Length >= (32 + 6)) // 32 chars for guid value, 6 for 'guid: '
                    {
                        var metaFileGuid = line.Substring(6, line.Length - 6).Trim();
                        if (IsValidGuid(metaFileGuid) == false)
                        {
                            LogError(assembly, "Invalid or unsupported guid format was found in meta file at line nr " + line + ". Line content: " + line);
                            break;
                        }

                        var assemblyGuid = (assemblyGuids[0] as GuidAttribute).Value.Replace("-", "").ToLower();
                        if (IsValidGuid(assemblyGuid) == false)
                        {
                            LogError(assembly, "Invalid guid format was specified in assembly. GuidAttribute value: " + assemblyGuid);
                            break;
                        }

                        if (assemblyGuid != metaFileGuid)
                        {
                            if (log) Debug.Log("Persistent assembly: " + assembly.FullName + ", already have the right guid (" + assemblyGuid + ")");

                            lines[j] = line.Substring(0, 6) + assemblyGuid;
                            updateAssetDatabase = true;
                            try
                            {
                                if (log) Debug.Log("Persistent assembly '" + assembly.FullName + "' guid is being updated from: '" + metaFileGuid + "' to '" + assemblyGuid + "'");
                                using (var fs = File.Open(assemblyMetaFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                                using (var sw = new StreamWriter(fs))
                                {
                                    for (int k = 0; k < lines.Length; k++)
                                    {
                                        sw.WriteLine(lines[k]);
                                    }
                                }
                                if (log) Debug.Log("Meta file for '" + assemblyMetaFile + "' has been updated");
                            }
                            catch (Exception ex)
                            {
                                LogError(assembly, ex.Message);
                                Debug.LogWarning("Could not write to meta file. Please start Unity in Administrative mode");
                                return;
                            }
                        }
                        else
                        {
                            if (log) Debug.Log("Persistent assembly: " + assembly.FullName + ", is already updated with the right guid: '" + assemblyGuid + "'");
                        }
                        break;
                    }
                }
            }

            if (log) Debug.Log("Finished scanning " + assemblies.Length + " assemblies. Refreshing AssetDatabase...");

            if (updateAssetDatabase)
            {
                AssetDatabase.Refresh();
            }
        }

        private static void LogError(Assembly assembly, string msg)
        {
            Debug.LogError("Could not update asset guid for assembly " + Path.GetFileName(assembly.GetAssemblyFilePath()) + ". " + msg);
        }

        private static bool IsValidGuid(string guid)
        {
            bool isValid = guid.Length == 32;
            if (isValid == false) return false;

            for (int i = 0; i < guid.Length; i++)
            {
                if (char.IsLetterOrDigit(guid[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif