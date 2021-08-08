#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ModuleDefinition.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public abstract class ModuleDefinition
    {
        private static Rect lastEnumButtonRectWhyUnity;

        private bool isActivatedCached;
        private bool supportsCurrentEnvironmentCached;
        private ModuleManifest installedManifestCached;
        private string statusStringCached;

        private static GUIStyle private_titleStyle;
        private static GUIStyle private_statusStyle;

        public static GUIStyle TitleStyle
        {
            get
            {
                if (private_titleStyle == null)
                {
                    private_titleStyle = new GUIStyle(EditorStyles.largeLabel)
                    {
                        fontSize = 14,
                        font = EditorStyles.boldFont
                    };
                }

                return private_titleStyle;
            }
        }

        public static GUIStyle StatusStyle
        {
            get
            {
                if (private_statusStyle == null)
                {
                    private_statusStyle = new GUIStyle(SirenixGUIStyles.SubtitleRight);
                    private_statusStyle.margin.top += 7;
                }

                return private_statusStyle;
            }
        }

        public virtual void OnSelectedInInspector()
        {
            this.isActivatedCached = this.CheckIsActivated();
            this.supportsCurrentEnvironmentCached = this.CheckSupportsCurrentEnvironment();

            if (this.isActivatedCached)
            {
                this.installedManifestCached = this.LoadManifest();
            }

            this.statusStringCached = this.isActivatedCached ?
                "Installed (" + this.installedManifestCached.Version + ")" :
                "Inactive (available: " + (this.LatestVersion ?? new Version(0, 0, 0, 0)).ToString() + ")";
        }

        [OnInspectorGUI, PropertyOrder(-10)]
        protected virtual void DrawTitle()
        {
            GUILayout.Label(this.NiceName, TitleStyle);

            var rect = GUILayoutUtility.GetLastRect();

            GUI.Label(rect.AlignCenterY(16).AddY(2).SetXMin(rect.center.x), this.statusStringCached, StatusStyle);

            SirenixEditorGUI.HorizontalLineSeparator(SirenixGUIStyles.BorderColor, 1);
        }

        [OnInspectorGUI, PropertyOrder(-5)]
        protected virtual void DrawDescription()
        {
            GUILayout.Label(this.Description, SirenixGUIStyles.MultiLineLabel);

            if (!string.IsNullOrEmpty(this.DependenciesDescription))
            {
                GUILayout.Space(6);
                GUILayout.Label("Dependencies: " + this.DependenciesDescription, SirenixGUIStyles.MultiLineLabel);
            }

            if (!string.IsNullOrEmpty(this.DocumentationLink))
            {
                GUILayout.Space(6);

                const string btnText = "Go to documentation.";

                GUIHelper.PushColor(new Color(0.3f, 0.6f, 0.7f, 1f));

                if (GUILayout.Button(btnText, EditorStyles.label))
                {
                    Application.OpenURL(this.DocumentationLink);
                }

                var btnRect = GUILayoutUtility.GetLastRect();

                SirenixEditorGUI.DrawSolidRect(btnRect.SetWidth(EditorStyles.label.CalcSize(GUIHelper.TempContent(btnText)).x).Expand(-2).AlignBottom(1).AddY(0), Color.white * 0.7f);

                GUIHelper.PopColor();
            }
        }

        [OnInspectorGUI, PropertyOrder(5)]
        protected virtual void DrawActivationButtons()
        {
            GUILayout.FlexibleSpace();

            if (!this.supportsCurrentEnvironmentCached)
            {
                GUIHelper.PushColor(new Color(0.7529412f, 0.2235294f, 0.1686275f, 1) * 1.3f);
                GUILayout.Label("Dependencies are missing for this module" + (this.DependenciesDescription != null ? ": " + this.DependenciesDescription : "") , SirenixGUIStyles.MultiLineLabel);
                GUIHelper.PopColor();
            }

            var config = OdinModuleConfig.Instance.GetConfig(this);

            bool allowManual = false;

            if (config != null)
            {
                if (config.ActivationSettings == ActivationSettings.Manual)
                    allowManual = true;
                else if (config.ActivationSettings == ActivationSettings.GlobalSettings && OdinModuleConfig.Instance.ModuleTogglingSettings != OdinModuleConfig.ModuleAutomationSettings.Automatic)
                    allowManual = true;
            }
            else allowManual = true;

            EditorGUILayout.BeginHorizontal();
            {
                if (!this.isActivatedCached)
                {
                    GUIHelper.PushGUIEnabled(allowManual);
                    if (GUILayout.Button("Activate", SirenixGUIStyles.ButtonLeft))
                    {
                        this.Activate();
                        this.isActivatedCached = true;
                        AssetDatabase.Refresh();
                        GUIUtility.ExitGUI();
                    }
                    GUIHelper.PopGUIEnabled();
                }
                else
                {
                    GUIHelper.PushGUIEnabled(allowManual);
                    if (GUILayout.Button("Deactivate", SirenixGUIStyles.ButtonLeft))
                    {
                        this.Deactivate();
                        this.isActivatedCached = false;
                        AssetDatabase.Refresh();
                        GUIUtility.ExitGUI();
                    }
                    GUIHelper.PopGUIEnabled();

                    var canUpdate = this.installedManifestCached != null && this.installedManifestCached.Version < this.LatestVersion;

                    if (canUpdate)
                    {
                        if (GUILayout.Button("Update to " + this.LatestVersion, SirenixGUIStyles.ButtonMid))
                        {
                            this.Deactivate();
                            this.Activate();
                            AssetDatabase.Refresh();
                            GUIUtility.ExitGUI();
                        }
                    }
                }

                string labelText;

                if (config == null)
                {
                    GUIHelper.PushGUIEnabled(false);
                    labelText = "Activation Rule: Config Broken";
                }
                else
                {
                    switch (config.ActivationSettings)
                    {
                        case ActivationSettings.Manual:
                            labelText = "Activation Rule: Manual";
                            break;
                        case ActivationSettings.GlobalSettings:
                        default:
                            labelText = "Activation Rule: Global Settings";
                            break;
                    }
                }

                var buttonClick = GUILayout.Button(GUIHelper.TempContent(labelText), SirenixGUIStyles.ButtonRight, GUILayoutOptions.Width(250));
                var rect = GUILayoutUtility.GetLastRect();

                if (rect.x != 0 && rect.y != 0)
                {
                    lastEnumButtonRectWhyUnity = rect;
                }

                if (buttonClick)
                {
                    var selector = new EnumSelector<ActivationSettings>();

                    selector.SelectionChanged += (values) =>
                    {
                        var value = values.FirstOrDefault();

                        if (value != config.ActivationSettings)
                        {
                            config.ActivationSettings = value;
                            OdinModuleConfig.Instance.SaveAssetChanges();
                            OdinModuleConfig.RefreshModuleSetup();
                        }
                    };


                    selector.ShowInPopup(new Vector2(lastEnumButtonRectWhyUnity.xMin, lastEnumButtonRectWhyUnity.yMax));

                    SirenixEditorGUI.DrawSolidRect(rect, Color.green);
                }

                if (config == null)
                {
                    GUIHelper.PopGUIEnabled();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public ModuleManager ModuleManager;

        public abstract string ID { get; }

        public abstract string NiceName { get; }

        public abstract Version LatestVersion { get; }

        public abstract string Description { get; }

        public abstract string BuildFromPath { get; }

        public virtual string DocumentationLink { get { return null; } }
        
        public virtual string DependenciesDescription { get { return null; } }

        public virtual bool UnstableExperimental { get { return false; } }

        public abstract bool CheckSupportsCurrentEnvironment();

        public virtual bool CheckIsActivated()
        {
            var path = this.ModuleManager.DataManager.InstallPath + "/" + this.ID + "/manifest.txt";
            return File.Exists(path);
        }

        protected virtual void OnBeforeActivate()
        {
        }

        protected virtual void OnAfterActivate()
        {
        }

        protected virtual void OnBeforeDeactivate()
        {
        }

        protected virtual void OnAfterDeactivate()
        {
        }

        public virtual void Activate()
        {
            if (this.CheckIsActivated()) return;

            this.OnBeforeActivate();

            var bytes = this.ModuleManager.DataManager.LoadData(this.ID);

            if (bytes == null)
            {
                throw new Exception("Could not load module data for module '" + this.ID + "': data could not be found.");
            }

            var data = ModuleData.Deserialize(bytes);

            if (bytes == null)
            {
                throw new Exception("Could not load module data for module '" + this.ID + "': data could not be parsed.");
            }

            var installFolder = this.ModuleManager.DataManager.InstallPath + "/" + this.ID;

            if (!Directory.Exists(installFolder))
            {
                Directory.CreateDirectory(installFolder);
            }

            foreach (var file in data.Files)
            {
                var path = installFolder + "/" + file.Path;
                File.WriteAllBytes(path, file.Data);
            }

            var manifestPath = installFolder + "/manifest.txt";
            var manifest = data.ToManifest();

            ModuleManifest.Save(manifestPath, manifest);

            this.OnAfterActivate();
        }

        public virtual void Deactivate()
        {
            if (!this.CheckIsActivated()) return;

            var installFolder = this.ModuleManager.DataManager.InstallPath + "/" + this.ID;
            var manifestPath = installFolder + "/manifest.txt";

            var manifest = ModuleManifest.Load(manifestPath);

            if (manifest == null)
            {
                throw new Exception("Could not load module manifest.");
            }

            this.OnBeforeDeactivate();

            foreach (var file in manifest.Files)
            {
                var fullPath = installFolder + "/" + file;

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }

            File.Delete(manifestPath);
            File.Delete(manifestPath + ".meta");

            DeleteIfEmpty(new DirectoryInfo(installFolder));

            this.OnAfterDeactivate();
        }

        public virtual ModuleData GetModuleDataForPackaging()
        {
            var folderPath = this.BuildFromPath;
            var dir = new DirectoryInfo(folderPath);

            if (!dir.Exists) throw new InvalidOperationException("Directory '" + dir.FullName + "' does not exist to build module package from.");

            var data = new ModuleData()
            {
                ID = this.ID,
                Version = this.LatestVersion,
                Files = dir.GetFiles("*", SearchOption.AllDirectories).Select(file =>
                {
                    var name = file.Name.ToLower();

                    if (name == "thumbs.db" || name == "manifest.txt" || name == "manifest.txt.meta")
                        return null;

                    var relativePath = PathUtilities.MakeRelative(dir.FullName, file.FullName);
                    var bytes = File.ReadAllBytes(file.FullName);

                    return new ModuleData.ModuleFile()
                    {
                        Path = relativePath,
                        Data = bytes
                    };
                }).Where(n => n != null).ToList()
            };

            return data;
        }

        public virtual ModuleManifest LoadManifest()
        {
            if (!this.CheckIsActivated()) return null;

            var installFolder = this.ModuleManager.DataManager.InstallPath + "/" + this.ID;
            var manifestPath = installFolder + "/manifest.txt";

            return ModuleManifest.Load(manifestPath);
        }

        protected static void DeleteIfEmpty(DirectoryInfo dir)
        {
            if (!dir.Exists) return;

            if (dir.Name.ToLower() == "__macosx")
            {
                if (dir.Parent != null)
                {
                    var metaFile = dir.Parent.FullName + "/" + dir.Name + ".meta";

                    if (File.Exists(metaFile))
                    {
                        File.Delete(metaFile);
                    }
                }

                dir.Delete(true);
                return;
            }

            foreach (var subDir in dir.GetDirectories())
            {
                DeleteIfEmpty(subDir);
            }

            if (dir.GetDirectories().Length != 0) return;

            var files = dir.GetFiles();
            if (files.Length > 2) return;

            foreach (var file in files)
            {
                var name = file.Name.ToLower();
                if (name != "thumbs.db" && name != ".ds_store") return;
            }

            if (dir.Parent != null)
            {
                var metaFile = dir.Parent.FullName + "/" + dir.Name + ".meta";

                if (File.Exists(metaFile))
                {
                    File.Delete(metaFile);
                }
            }

            dir.Delete(true);
        }
    }
}
#endif