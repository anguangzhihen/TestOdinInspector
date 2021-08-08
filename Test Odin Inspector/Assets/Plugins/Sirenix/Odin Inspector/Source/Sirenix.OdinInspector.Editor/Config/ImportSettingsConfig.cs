#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ImportSettingsConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.IO;
    using Sirenix.Serialization.Utilities.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Configurations for Odin DLLs import settings.
    /// </summary>
    [SirenixEditorConfig]
    public class ImportSettingsConfig : GlobalConfig<ImportSettingsConfig>
    {
        private const string AOTAssemblyFolder = "NoEmitAndNoEditor";

        private const string JITAssemblyFolder = "NoEditor";

        private const int LabelWidth = 270;

        private static bool editorOnlyMode;

        private static bool isHeaderInfoBoxFolded = true;

        [SerializeField, HideInInspector]
        private bool automateBeforeBuild = true;

        private OdinAssemblyOptions currentOption = OdinAssemblyOptions.Recommended;

        /// <summary>
        /// Gets or sets a value indicating whether or not Odin should automatically configure the import settings of its DLLs in a preprocess build step.
        /// Keep in mind that this feature is only supported by Unity version 5.6 and up.
        /// </summary>
        [ShowInInspector]
        [BoxGroup("AutomateBox", ShowLabel = false)]
        [LabelWidth(LabelWidth), SuffixLabel("$AutomateSuffix")]
        [DisableIf("editorOnlyMode"), EnableIf("IsAutomationSupported")]
        public bool AutomateBeforeBuild
        {
            get
            {
                return IsAutomationSupported && this.automateBeforeBuild;
            }
            set
            {
                this.automateBeforeBuild = value;
                if (this.automateBeforeBuild && IsAutomationSupported == false)
                {
                    Debug.LogWarning("Automatic configuration of Odin DLL import settings is only supported by Unity versions 5.6 and up.");
                }
            }
        }

        [EnableGUI, LabelWidth(LabelWidth)]
        [BoxGroup("SettingsBox", ShowLabel = false)]
        [ShowInInspector, DisplayAsString, SuffixLabel("$BuildTargetSuffix")]
        [DisableIf("editorOnlyMode")]
        private BuildTarget CurrentBuildTarget
        {
            get { return EditorUserBuildSettings.activeBuildTarget; }
        }

        [EnableGUI, LabelWidth(LabelWidth)]
        [BoxGroup("SettingsBox", ShowLabel = false)]
        [ShowInInspector, DisplayAsString, SuffixLabel("$ScriptingBackendSuffix")]
        [DisableIf("editorOnlyMode")]
        private ScriptingImplementation CurrentScriptingBackend
        {
            get { return AssemblyImportSettingsUtilities.GetCurrentScriptingBackend(); }
        }

        [EnableGUI, LabelWidth(LabelWidth)]
        [BoxGroup("SettingsBox", ShowLabel = false)]
        [ShowInInspector, DisplayAsString, SuffixLabel("$ApiLevelSuffix")]
        [DisableIf("editorOnlyMode")]
        private ApiCompatibilityLevel CurrentApiCompatibilityLevel
        {
            get { return AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel(); }
        }

        [EnableGUI, LabelWidth(LabelWidth)]
        [ShowInInspector, DisplayAsString]
        [BoxGroup("SelectApplyBox", ShowLabel = false)]
        [DisableIf("editorOnlyMode"), SuffixLabel("$RecommendedSuffix")]
        private string CurrentRecommendedBuildConfiguration
        {
            get
            {
                if (editorOnlyMode)
                {
                    return "Editor Only Mode enabled.";
                }
                else
                {
                    return GetRecommendedOption().ToString();
                }
            }
        }

        [ShowInInspector]
        [LabelWidth(LabelWidth), EnumToggleButtons, EnableIf("EnableApplyButton")]
        [BoxGroup("SelectApplyBox", ShowLabel = false)]
        [HorizontalGroup("SelectApplyBox/Select", LabelWidth + 320)]
        public OdinAssemblyOptions AssemblyBuildConfiguration { get { return this.currentOption; } set { this.currentOption = value; } }

        private string AutomateSuffix { get { return IsAutomationSupported ? "Recommended" : "The automation feature is only available in Unity 5.6 and up"; } }

        private string BuildTargetSuffix
        {
            get { return AssemblyImportSettingsUtilities.PlatformSupportsJIT(EditorUserBuildSettings.activeBuildTarget) ? "Supports JIT" : "Only AOT"; }
        }

        private string ScriptingBackendSuffix
        {
            get { return AssemblyImportSettingsUtilities.ScriptingBackendSupportsJIT(AssemblyImportSettingsUtilities.GetCurrentScriptingBackend()) ? "Supports JIT" : "Only AOT"; }
        }

        private string ApiLevelSuffix
        {
            get { return AssemblyImportSettingsUtilities.ApiCompatibilityLevelSupportsJIT(AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel()) ? "Supports JIT" : "Only AOT"; }
        }

        private string RecommendedSuffix
        {
            get { return GetRecommendedOption() == OdinAssemblyOptions.JIT ? "All settings support JIT" : "Some settings are only AOT"; }
        }

        private bool EnableApplyButton { get { return editorOnlyMode == false && (IsAutomationSupported == false || IsAutomationSupported && this.automateBeforeBuild == false); } }

        /// <summary>
        /// Gets a value indicating whether or not automatic configuration of Odin's DLL import settings is supported by the current Unity version.
        /// </summary>
        public static bool IsAutomationSupported { get { return UnityVersion.IsVersionOrGreater(5, 6); } }

        [BoxGroup("SelectApplyBox", ShowLabel = false)]
        [Button(ButtonSizes.Large), EnableIf("EnableApplyButton")]
        private void Apply()
        {
            UnityEditorEventUtility.EditorApplication_delayCall += this.ApplyDelayed;
        }

        private void ApplyDelayed()
        {
            this.ApplyImportSettings();
        }

        public void ApplyImportSettings()
        {
            if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled())
            {
                throw new InvalidOperationException("Editor Only Mode enabled.");
            }

            //string assemblyPath = SirenixAssetPaths.SirenixAssembliesPath;
            //var aotAssemblies = Directory.GetFiles(assemblyPath + AOTAssemblyFolder, "*.dll");
            //var jitAssemblies = Directory.GetFiles(assemblyPath + JITAssemblyFolder, "*.dll");

            var assemblyDir = new DirectoryInfo(SirenixAssetPaths.SirenixAssembliesPath).FullName;
            var projectAssetsPath = Directory.GetCurrentDirectory().TrimEnd('\\', '/');

            var isPackage = PathUtilities.HasSubDirectory(new DirectoryInfo(projectAssetsPath), new DirectoryInfo(assemblyDir)) == false;

            var aotDirPath = assemblyDir + "NoEmitAndNoEditor/";
            var jitDirPath = assemblyDir + "NoEditor/";

            var aotDir = new DirectoryInfo(aotDirPath);
            var jitDir = new DirectoryInfo(jitDirPath);

            var aotAssemblies = new List<string>();
            var jitAssemblies = new List<string>();

            foreach (var file in aotDir.GetFiles("*.dll"))
            {
                string path = file.FullName;
                if (isPackage)
                {
                    path = SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path.Substring(assemblyDir.Length);
                }
                else
                {
                    path = path.Substring(projectAssetsPath.Length + 1);
                }

                aotAssemblies.Add(path);
            }

            foreach (var file in jitDir.GetFiles("*.dll"))
            {
                string path = file.FullName;
                if (isPackage)
                {
                    path = SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path.Substring(assemblyDir.Length);
                }
                else
                {
                    path = path.Substring(projectAssetsPath.Length + 1);
                }

                jitAssemblies.Add(path);
            }

            var option = this.currentOption;
            if (option == OdinAssemblyOptions.Recommended)
            {
                option = this.GetRecommendedOption();
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                switch (option)
                {
                    case OdinAssemblyOptions.AOT:
                        SetImportSettings(EditorUserBuildSettings.activeBuildTarget, aotAssemblies, OdinAssemblyImportSettings.IncludeInBuildOnly);
                        SetImportSettings(EditorUserBuildSettings.activeBuildTarget, jitAssemblies, OdinAssemblyImportSettings.ExcludeFromAll);
                        break;

                    case OdinAssemblyOptions.JIT:
                        SetImportSettings(EditorUserBuildSettings.activeBuildTarget, aotAssemblies, OdinAssemblyImportSettings.ExcludeFromAll);
                        SetImportSettings(EditorUserBuildSettings.activeBuildTarget, jitAssemblies, OdinAssemblyImportSettings.IncludeInBuildOnly);
                        break;

                    default:
                        throw new ArgumentException("Unknown Odin assembly option: " + this.currentOption + ". Please select either AOT or JIT");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static void SetImportSettings(BuildTarget platform, List<string> assemblyPaths, OdinAssemblyImportSettings importSettings)
        {
            foreach (var path in assemblyPaths)
            {
                var p = path.Replace('\\', '/');
                AssemblyImportSettingsUtilities.SetAssemblyImportSettings(platform, p, importSettings);
            }
        }

        [OnInspectorGUI, PropertyOrder(-1000000)]
        private void DrawEditorOnlyMode()
        {
            if (Event.current.type == EventType.Layout)
            {
                // EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled() is heavy, so instead we call it once per frame and that should be enough.
                editorOnlyMode = EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled();
            }

            if (editorOnlyMode)
            {
                SirenixEditorGUI.InfoMessageBox("Editor Only Mode is currently enabled. These configurations are currently irrelevant.");
            }
        }

        [OnInspectorGUI, PropertyOrder(-10000)]
        private void DrawHeaderInfoBox()
        {
            isHeaderInfoBoxFolded = SirenixEditorGUI.DetailedMessageBox(
                "Odin will automatically detect your current build target and make sure it uses the assemblies best suited for your target platform.\n" +
                "\n" +
                "Click here to learn more.",
                "The Odin Serializer has two sets of assemblies: one set for AOT platforms and one for those platforms where JIT'ing is supported. " +
                "JIT is usually the most performant, but is not supported on all platforms. Finding out whether your setup supports it, " +
                "goes beyond what Unity's Import Settings has to offer, which is why this tool becomes necessary.\n" +
                "\n" +
                "Odin has a predefined set of known setups where JIT is supported. If your setup doesn't match any of those, then it'll choose to use the AOT assemblies.\n" +
                "\n" +
                "Enabling the \"Automate Before Build\" option will enable a preprocess build step, that will configure the import settings automatically, based on your current build settings.\n" +
                "\n" +
                "If you've stumbled on a setup where we're using AOT when we could be using JIT, or the other way around, " +
                "then you can always disable the preprocess build set and manually configure your assemblies.",
                MessageType.Info,
                isHeaderInfoBoxFolded);
        }

        public OdinAssemblyOptions GetRecommendedOption()
        {
            return 
                AssemblyImportSettingsUtilities.IsJITSupported(
                    EditorUserBuildSettings.activeBuildTarget,
                    AssemblyImportSettingsUtilities.GetCurrentScriptingBackend(),
                    AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel())
                ? OdinAssemblyOptions.JIT : OdinAssemblyOptions.AOT;
        }

        public enum OdinAssemblyOptions
        {
            Recommended = 0,
            AOT = 1,
            JIT = 2,
        }
    }
}
#endif