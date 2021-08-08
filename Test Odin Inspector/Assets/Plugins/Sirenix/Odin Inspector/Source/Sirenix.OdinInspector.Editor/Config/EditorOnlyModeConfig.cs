#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EditorOnlyModeConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Editor Only Mode Utility.
    /// </summary>
    public sealed class EditorOnlyModeConfig
    {
        private static readonly object instance_LOCK = new object();
        private static EditorOnlyModeConfig instance;

        public static EditorOnlyModeConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instance_LOCK)
                    {
                        if (instance == null)
                        {
                            instance = new EditorOnlyModeConfig();
                        }
                    }
                }

                return instance;
            }
        }

        private const string BACKUP_FILE_SUFFIX = ".backup.txt";

        private const string SOURCE_CODE_NOT_SUPPORTED_MESSAGE = "Enabling and disabling Editor Only Mode is not supported when using Odin with source code.";

        private static readonly string[] SerializerScriptFiles = new string[]
        {
            "VectorIntFormatters.cs",
            "SerializedNetworkBehaviour.cs",
        };

        private static string ExcludeFromEverything = @"PluginImporter:
  serializedVersion: 1
  iconMap: {}
  executionOrder: {}
  isPreloaded: 0
  isOverridable: 0
  platformData:
    Any:
      enabled: 0
      settings:
        Exclude Android: 1
        Exclude Editor: 1
        Exclude Linux: 1
        Exclude Linux64: 1
        Exclude LinuxUniversal: 1
        Exclude N3DS: 1
        Exclude OSXIntel: 1
        Exclude OSXIntel64: 1
        Exclude OSXUniversal: 1
        Exclude PS4: 1
        Exclude PSM: 1
        Exclude PSP2: 1
        Exclude SamsungTV: 1
        Exclude Tizen: 1
        Exclude WebGL: 1
        Exclude WiiU: 1
        Exclude Win: 1
        Exclude Win64: 1
        Exclude WindowsStoreApps: 1
        Exclude XboxOne: 1
        Exclude iOS: 1
        Exclude tvOS: 1
    Editor:
      enabled: 0
      settings:
        DefaultValueInitialized: true
  userData:
  assetBundleName:
  assetBundleVariant: ";

        private static string ExcludeFromEverythingExceptEditor = @"PluginImporter:
  serializedVersion: 1
  iconMap: {}
  executionOrder: {}
  isPreloaded: 0
  isOverridable: 0
  platformData:
    Any:
      enabled: 0
      settings:
        Exclude Android: 1
        Exclude Editor: 0
        Exclude Linux: 1
        Exclude Linux64: 1
        Exclude LinuxUniversal: 1
        Exclude N3DS: 1
        Exclude OSXIntel: 1
        Exclude OSXIntel64: 1
        Exclude OSXUniversal: 1
        Exclude PS4: 1
        Exclude PSM: 1
        Exclude PSP2: 1
        Exclude SamsungTV: 1
        Exclude Tizen: 1
        Exclude WebGL: 1
        Exclude WiiU: 1
        Exclude Win: 1
        Exclude Win64: 1
        Exclude WindowsStoreApps: 1
        Exclude XboxOne: 1
        Exclude iOS: 1
        Exclude tvOS: 1
    Editor:
      enabled: 1
      settings:
        DefaultValueInitialized: true
  userData:
  assetBundleName:
  assetBundleVariant: ";

#pragma warning disable CS0414
#if EDITOR_ONLY_BUILD
        [OnInspectorGUI, PropertyOrder(-5000)]
        [InfoBox("Cannot enable serialization in an Editor Only build. You can download a non-Editor Only package from https://odininspector.com/downloads")]
        private void EditorOnlyBuildMessage() { }

        private readonly bool EditorOnlyBuild = true;
#else
        private readonly bool EditorOnlyBuild = false;
#endif
#pragma warning restore CS0414

        [OnInspectorGUI, PropertyOrder(-2399)]
        [InfoBox(
            "If you're not interested in using Odin's serialization system - inheriting from classes such as SerializedMonoBehaviour and SerializedScriptableObject etc. - " +
            "you can disable the serialization system completely, without losing the ability to leverage all of the attributes and " +
            "editor functionality that Odin provides. This will also let you use Odin while targeting UWP platforms.\n\n" +

            "Disabling the serialization system will prevent almost all of Odin from being included in your builds. The only Odin code that will be included is the small " +
            "Sirenix.OdinInspector.Attributes assembly, containing only the attribute definitions. If you are using IL2CPP, " +
            "many of these attributes will likely be removed during Unity's code stripping step.\n\n" +

            "Note that Odin still uses the serialization system in the editor itself to provide you with various editor functionality. " +
            "You will still be able to inherit from classes like SerializedMonoBehaviour while in the editor, " +
            "but you will get a warning in the inspector if you do so, and you will get compiler errors if you try to build.\n")]
        private void TopMessage() { }

        private string[] platformSpecificAssemblyFiles;

        private string[] globalAssemblyFiles;

        private bool isUsingSourceCode;

        private bool isInEditorOnlyMode;

        private bool SerializationModeIsForceText
        {
            get { return EditorSettings.serializationMode == SerializationMode.ForceText; }
        }

        private EditorOnlyModeConfig()
        {
            this.Update();
        }

        /// <summary>
        /// Gaither all necessary information about the editor only state.
        /// </summary>
        public void Update()
        {
            var assemblyPath = SirenixAssetPaths.SirenixAssembliesPath;
            if (!Directory.Exists(assemblyPath))
            {
                this.isInEditorOnlyMode = false;
                this.isUsingSourceCode = true;
                return;
            }

            // All globalAssemblies that will get inlcuded normally in builds, except the Sirenix.OdinInspector.Attributes assmbly.
            var serializationConfigDll = SirenixAssetPaths.SirenixAssembliesPath + "Sirenix.Serialization.Config.dll";
            this.globalAssemblyFiles = File.Exists(serializationConfigDll) ? new string[] { serializationConfigDll } : new string[0];

            // Finds all platform specific assemblies which are located in special folders, These files are never active in the Editor.
            this.platformSpecificAssemblyFiles = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith(SirenixAssetPaths.SirenixAssembliesPath))
                .Where(p => p.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                .Where(p => p.StartsWith(SirenixAssetPaths.SirenixAssembliesPath + "NoEditor/") || p.StartsWith(SirenixAssetPaths.SirenixAssembliesPath + "NoEmitAndNoEditor/"))
                .ToArray();

            if (this.platformSpecificAssemblyFiles.Length + this.globalAssemblyFiles.Length == 0)
            {
                this.isUsingSourceCode = true;
            }
            else
            {
                this.isUsingSourceCode = false;
                this.isInEditorOnlyMode = this.platformSpecificAssemblyFiles
                    .Concat(this.globalAssemblyFiles)
                    .All(x => File.Exists(x + BACKUP_FILE_SUFFIX));
            }
        }

        [Button(ButtonSizes.Gigantic), HideIf("SerializationModeIsForceText"), DisableIf("EditorOnlyBuild")]
        [InfoBox("In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.")]
        [PropertyOrder(-10)]
        private void SetForceText()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Disables Editor Only Mode.
        /// </summary>
        [Button(ButtonSizes.Gigantic), GUIColor(1, 0.8f, 0), EnableIf("SerializationModeIsForceText"), ShowIf("ShowDisableEditorOnlyMode"), DisableIf("EditorOnlyBuild")]
        [PropertyOrder(-8)]
        public void DisableEditorOnlyMode()
        {
            if (this.isUsingSourceCode)
            {
                Debug.LogError(SOURCE_CODE_NOT_SUPPORTED_MESSAGE);
                return;
            }

            if (!this.isInEditorOnlyMode)
            {
                Debug.LogError("Editor mode is already disabled.");
                return;
            }

            UnityEditorEventUtility.EditorApplication_delayCall += () =>
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!this.SerializationModeIsForceText)
                {
                    Debug.LogError("In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.");
                    return;
                }

                this.Update();

                foreach (var dllFilePath in this.globalAssemblyFiles.Concat(this.platformSpecificAssemblyFiles))
                {
                    if (!File.Exists(dllFilePath + BACKUP_FILE_SUFFIX))
                    {
                        Debug.LogError("The old import settings was not found which was supposed to be located at: '" + dllFilePath + BACKUP_FILE_SUFFIX);
                    }
                    else
                    {
                        // Revert plugin import settings to the backed up settings.
                        this.SetPluginImportSettings(dllFilePath + ".meta", File.ReadAllText(dllFilePath + BACKUP_FILE_SUFFIX));
                        File.Delete(dllFilePath + BACKUP_FILE_SUFFIX);
                    }
                }

                // Rename link files
                var linkFiles = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension("link.xml" + BACKUP_FILE_SUFFIX), new string[] { SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('/') });
                for (int i = 0; i < linkFiles.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(linkFiles[i]);
                    var newPath = PathUtilities.GetDirectoryName(path).Replace('\\', '/').TrimEnd('/') + "/link.xml";
                    AssetDatabase.MoveAsset(path, newPath);
                }

                // Move serialization config back to where it belongs.
                var attribute = typeof(GlobalSerializationConfig).BaseType.GetProperty("ConfigAttribute", Flags.AllMembers).GetValue(null, null) as GlobalConfigAttribute;
                var serializationConfigPath = AssetDatabase.GetAssetPath(GlobalSerializationConfig.Instance);
                var fileName = Path.GetFileName(serializationConfigPath);
                var destPath = attribute.AssetPath.TrimEnd('/');
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                    AssetDatabase.Refresh();
                }

                // Move script files out of editor folder.
                {
                    //var folder = SirenixAssetPaths.OdinPath + "Scripts/";
                    //var from = folder + "Editor/VectorIntFormatters.cs";
                    //var to = folder + "VectorIntFormatters.cs";
                    //if (File.Exists(from))
                    //{
                    //    AssetDatabase.MoveAsset(from, to);
                    //}

                    var targetFolder = SirenixAssetPaths.OdinPath + "Scripts/";
                    var sourceFolder = targetFolder + "Editor/";

                    foreach (var f in SerializerScriptFiles)
                    {
                        var from = sourceFolder + f;
                        var to = targetFolder + f;
                        if (File.Exists(from))
                        {
                            File.Move(from, to);
                            File.Delete(from + ".meta");
                        }
                    }
                }

                AssetDatabase.MoveAsset(serializationConfigPath, destPath + "/" + fileName);
                AssetDatabase.Refresh();
                this.Update();
            };
        }

        /// <summary>
        /// Enables editor only mode.
        /// </summary>
        public void EnableEditorOnlyMode(bool force)
        {
            if (this.isUsingSourceCode)
            {
                Debug.LogError(SOURCE_CODE_NOT_SUPPORTED_MESSAGE);
                return;
            }

            if (this.isInEditorOnlyMode && !force)
            {
                Debug.LogError("Editor mode is already enabled.");
                return;
            }

            AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            if (!this.SerializationModeIsForceText)
            {
                Debug.LogError("In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.");
                return;
            }

            this.Update();

            // Make backups
            foreach (var dllFilePath in this.globalAssemblyFiles.Concat(this.platformSpecificAssemblyFiles))
            {
                if (File.Exists(dllFilePath + BACKUP_FILE_SUFFIX))
                {
                    File.Delete(dllFilePath + BACKUP_FILE_SUFFIX);
                }

                File.Copy(dllFilePath + ".meta", dllFilePath + BACKUP_FILE_SUFFIX);
            }

            // Rename link files
            var linkFiles = AssetDatabase.FindAssets("link", new string[] { SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('/') });
            for (int i = 0; i < linkFiles.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(linkFiles[i]);
                if (path.ToLower().EndsWith(".xml"))
                {
                    if (File.Exists(path + BACKUP_FILE_SUFFIX))
                    {
                        AssetDatabase.DeleteAsset(path + BACKUP_FILE_SUFFIX);
                    }

                    AssetDatabase.MoveAsset(path, path + BACKUP_FILE_SUFFIX);
                }
            }

            // Move serialization config out of the resouces folder:
            {
                var serializationConfigPath = AssetDatabase.GetAssetPath(GlobalSerializationConfig.Instance);
                var fileName = Path.GetFileName(serializationConfigPath);
                if (!File.Exists(SirenixAssetPaths.OdinEditorConfigsPath + fileName))
                {
                    AssetDatabase.MoveAsset(serializationConfigPath, SirenixAssetPaths.OdinEditorConfigsPath + fileName);
                }
                //AssetDatabase.Refresh();
            }

            // Move script files to Editor folder.
            {
                var sourceFolder = SirenixAssetPaths.OdinPath + "Scripts/";
                var targetFolder = sourceFolder + "Editor/";

                foreach (var f in SerializerScriptFiles)
                {
                    var from = sourceFolder + f;
                    var to = targetFolder + f;
                    if (File.Exists(from))
                    {
                        if (File.Exists(to))
                        {
                            File.Delete(to);
                        }

                        File.Move(from, to);
                        File.Delete(from + ".meta");
                    }
                }
            }

            // Apply import settings, which prevents assemblies from being included in builds.
            {
                foreach (var dllFilePath in this.platformSpecificAssemblyFiles)
                {
                    this.SetPluginImportSettings(dllFilePath + ".meta", ExcludeFromEverything);
                }

                foreach (var dllFilePath in this.globalAssemblyFiles)
                {
                    this.SetPluginImportSettings(dllFilePath + ".meta", ExcludeFromEverythingExceptEditor);
                }
            }

            AssetDatabase.Refresh();
            this.Update();
        }

        [Button(ButtonSizes.Gigantic), GUIColor(0, 1.0f, 0), EnableIf("SerializationModeIsForceText"), ShowIf("ShowEnableEditorOnlyMode"), DisableIf("EditorOnlyBuild")]
        [PropertyOrder(-8)]
        private void EnableEditorOnlyMode()
        {
            UnityEditorEventUtility.EditorApplication_delayCall += () =>
            {
                EnableEditorOnlyMode(false);
            };
        }

        private bool ShowEnableEditorOnlyMode { get { return !this.isInEditorOnlyMode && !this.isUsingSourceCode; } }
        private bool ShowDisableEditorOnlyMode { get { return this.isInEditorOnlyMode && !this.isUsingSourceCode; } }

        /// <summary>
        /// Checks to see whether Editor Only Mode is enabled.
        /// </summary>
        public bool IsEditorOnlyModeEnabled()
        {
#if EDITOR_ONLY_BUILD
            return true;
#else
            this.Update();
            return this.isInEditorOnlyMode && !this.isUsingSourceCode;
#endif
        }

        /// <summary>
        /// Checks to see whether Odin Inspector is installed in Source Code mode.
        /// </summary>
        public bool IsInSourceCode()
        {
            this.Update();
            return this.isUsingSourceCode;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (this.isUsingSourceCode)
            {
                Utilities.Editor.SirenixEditorGUI.ErrorMessageBox(SOURCE_CODE_NOT_SUPPORTED_MESSAGE);
            }
        }

        private bool TryThisNTimes(Action action, int numberOfTries = 20, int sleepBetweenTries = 10)
        {
            Exception exx = null;

            while (numberOfTries-- > 0)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception ex)
                {
                    exx = ex;
                    Thread.Sleep(sleepBetweenTries);
                }
            }
            Debug.LogException(exx);
            return false;
        }

        private void SetPluginImportSettings(string metaFile, string pluginImportSettings)
        {
            var pluginImportSettingsLines = new List<string>();
            var tmpLines = pluginImportSettings.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var foundImportSettings = false;

            foreach (var line in tmpLines)
            {
                if (foundImportSettings || line.StartsWith("PluginImporter:"))
                {
                    pluginImportSettingsLines.Add(line);
                    foundImportSettings = true;
                }
            }

            var currLines = new List<string>();
            TryThisNTimes(() =>
            {
                currLines.Clear();

                using (FileStream stream = new FileStream(metaFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    while (reader.Peek() >= 0)
                    {
                        currLines.Add(reader.ReadLine());
                    }
                }
            });

            var newLines = new List<string>();
            foreach (var line in currLines)
            {
                if (line.StartsWith("PluginImporter:"))
                {
                    break;
                }
                newLines.Add(line);
            }
            newLines.AddRange(pluginImportSettingsLines);

            TryThisNTimes(() =>
            {
                File.Delete(metaFile); // In later Unity versions, editing a metafile throws an exception if we don't delete it first.

                using (FileStream stream = new FileStream(metaFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    for (int i = 0; i < newLines.Count; i++)
                    {
                        writer.WriteLine(newLines[i]);
                    }
                }
            });
        }

#region Delete_Demos

        private bool HasDemos()
        {
            var dir = new DirectoryInfo(SirenixAssetPaths.SirenixPluginPath + "Demos");
            if (!dir.Exists)
            {
                return false;
            }

            return dir.GetDirectories().Any();
        }

        [OnInspectorGUI]
        [ShowIf("ShowDisableEditorOnlyMode")]
        [InfoBox("Since Editor Only Mode is enabled, remember to remove any Odin demos you might have imported before you build. Otherwise, you'll get compiler errors when you build, since most demos utilize Odin's serialization system which is not included in builds in Editor Only mode.", InfoMessageType.Warning)]
        private void DeleteImportedDemosInfo() { }

        [EnableIf("HasDemos")]
        [ShowIf("ShowDisableEditorOnlyMode")]
        [Button("$GetDeleteImportedDemosBtnText", ButtonSizes.Large)]
        private void DeleteImportedDemos()
        {
            var directoriesToDelete = new List<string>();

            if (Directory.Exists(SirenixAssetPaths.SirenixPluginPath + "Demos"))
            {
                directoriesToDelete.AddRange(new DirectoryInfo(SirenixAssetPaths.SirenixPluginPath + "Demos").GetDirectories().Select(x => x.FullName));
            }

            DeleteDirsAndFiles(directoriesToDelete);
        }

        private string GetDeleteImportedDemosBtnText()
        {
            if (this.HasDemos())
            {
                return "Delete all imported demos located in \"" + SirenixAssetPaths.SirenixPluginPath + "Demos/\"";
            }
            else
            {
                return "No imported demos was found in \"" + SirenixAssetPaths.SirenixPluginPath + "Demos/\"";
            }
        }

        private static void DeleteDirsAndFiles(List<string> directoriesToDelete)
        {
            foreach (var dir in directoriesToDelete.Select(x => x.Replace('\\', '/')))
            {
                var mdb = dir + ".mdb";
                var dirExist = Directory.Exists(dir);

                if (dirExist)
                {
                    var paths = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                    for (int i = 0; i < paths.Length; i++)
                    {
                        var p = paths[i].Replace('\\', '/');
                        DeleteFile(p);
                    };

                    DeleteDirectory(dir);
                }

                DeleteFile(mdb);
            }

            AssetDatabase.Refresh();
        }

        private static void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static void DeleteDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

#endregion Delete_Demos
    }
}
#endif