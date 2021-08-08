#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinModuleConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class ModuleConfiguration
    {
        public string ID;
        public ActivationSettings ActivationSettings;
    }

    public enum ActivationSettings
    {
        GlobalSettings = 0,
        Manual = 1
    }

    [SirenixEditorConfig, InitializeOnLoad]
    public class OdinModuleConfig : GlobalConfig<OdinModuleConfig>
    {
        private EditorTimeHelper timeHelper = new EditorTimeHelper();

        [SerializeField, HideInInspector]
        private List<ModuleConfiguration> configurations = new List<ModuleConfiguration>();

        public ModuleConfiguration GetConfig(ModuleDefinition module)
        {
            if (!this.ModuleManager.Modules.Contains(module)) return null;
            if (this.configurations == null) this.configurations = new List<ModuleConfiguration>();

            ModuleConfiguration result = null;

            for (int i = 0; i < this.configurations.Count; i++)
            {
                var config = this.configurations[i];

                if (config.ID == module.ID)
                {
                    result = config;
                }
                else
                {
                    // Prune the configs that no longer exist
                    bool existsInModuleManager = false;

                    for (int j = 0; j < this.ModuleManager.Modules.Count; j++)
                    {
                        if (this.ModuleManager.Modules[j].ID == config.ID)
                        {
                            existsInModuleManager = true;
                            break;
                        }
                    }

                    if (!existsInModuleManager)
                    {
                        this.configurations.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (result != null) return result;

            result = new ModuleConfiguration()
            {
                ID = module.ID
            };

            this.configurations.Add(result);
            this.SaveAssetChanges();
            return result;
        }

        public ModuleConfiguration GetConfig(string moduleID)
        {
            foreach (var module in this.ModuleManager.Modules)
            {
                if (module.ID == moduleID)
                    return this.GetConfig(module);
            }

            return null;
        }

        public void SaveAssetChanges()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [NonSerialized]
        private ModuleManager backing_moduleManager;

        public ModuleManager ModuleManager
        {
            get
            {
                if (this.backing_moduleManager == null)
                {
                    this.backing_moduleManager = ModuleManager.CreateDefault();
                }

                return this.backing_moduleManager;
            }
        }

        private OdinMenuTree moduleTree;
        private PropertyTree selectedModuleTree;

        private object nextSelection;
        private bool hasNextSelection;

        private float MenuWidth = 220;
        private bool ResizableMenuWidth = true;

        [HideInInspector]
        public ModuleAutomationSettings ModuleTogglingSettings;

        [HideInInspector]
        public ModuleAutomationSettings ModuleUpdateSettings;

        //[HideInInspector]
        //public bool AutomateModuleTogglingDecidedByUser = false;

        //[HideInInspector]
        //public bool AutomateModuleToggling = false;

        //[HideInInspector]
        //public bool AutoUpdateModules = false;

        //[HideInInspector]
        //public bool AutoUpdateModulesDecidedByUser = false;

        private static bool initialized;

        static OdinModuleConfig()
        {
            UnityEditorEventUtility.DelayAction(() =>
            {
                EnsureInitialized();

                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    RefreshModuleSetup();
                }
            });
        }

        private static void EnsureInitialized()
        {
            if (initialized) return;

            Type compilationPipelineType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.Compilation.CompilationPipeline");

            bool subscriptedToCompilationEvent = false;

            if (compilationPipelineType != null)
            {
                EventInfo compilationFinishedEvent = compilationPipelineType.GetEvent("compilationFinished", Flags.StaticPublic);

                if (compilationFinishedEvent != null)
                {
                    compilationFinishedEvent.GetAddMethod(true).Invoke(null, new object[] { (Action<object>)((_) => { EditorApplication.delayCall += () => TriggerModuleRefresh(); }) });
                    subscriptedToCompilationEvent = true;
                    //Debug.Log("Subscribed to CompilationPipeline.compilationStarted event...");
                }
                else if (UnityVersion.IsVersionOrGreater(2019, 1))
                {
                    Debug.LogWarning("Failed to find UnityEditor.Compilation.CompilationPipeline.compilationStarted event - Odin module automation may be broken in this version of Unity...");
                }
            }
            
            if (!subscriptedToCompilationEvent)
            {
                EditorApplication.update += EditorUpdate;
                //Debug.Log("Subscribed to EditorApplication.update...");
            }

            initialized = true;
        }

        private void OnEnable()
        {
            EnsureInitialized();
            this.SelectLastSelectedItem();
        }

        private void SelectLastSelectedItem()
        {
            EnsureInitialized();
            if (EditorPrefs.HasKey("ODIN_LastSelectedModule"))
            {
                string lastSelection = EditorPrefs.GetString("ODIN_LastSelectedModule");

                if (lastSelection == "ModuleConfig")
                {
                    this.nextSelection = "ModuleConfig";
                    this.hasNextSelection = true;
                }
                else
                {
                    this.nextSelection = ModuleManager.Modules.FirstOrDefault(n => n.GetType().Name == lastSelection);
                    this.hasNextSelection = true;
                }
            }
        }

        private static bool editorWasCompilingLastUpdate;

        private static void EditorUpdate()
        {
            EnsureInitialized();
            bool isCompiling = EditorApplication.isCompiling;

            if (isCompiling && !editorWasCompilingLastUpdate)
            {
                TriggerModuleRefresh();
            }

            editorWasCompilingLastUpdate = isCompiling;
        }

        private static void TriggerModuleRefresh()
        {
            try
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    RefreshModuleSetup();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void RefreshModuleSetup()
        {
            EnsureInitialized();
            var instance = OdinModuleConfig.Instance;

            if (instance == null)
            {
                Debug.LogWarning("Couldn't load Odin Module Config asset; Odin module automation will not work...");
            }
            else if (instance.ModuleManager.Refresh())
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);

                if (instance != null && instance.moduleTree != null)
                {
                    instance.moduleTree = null;
                    instance.SelectLastSelectedItem();
                }
            }
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            EnsureInitialized();
            if (this.moduleTree == null)
            {
                this.moduleTree = this.CreateModuleTree();
            }

            EditorTimeHelper prevTimeHelper = EditorTimeHelper.Time;
            EditorTimeHelper.Time = this.timeHelper;
            EditorTimeHelper.Time.Update();

            try
            {
                if (Event.current.type == EventType.Layout)
                {
                    if (this.moduleTree == null)
                    {
                        this.moduleTree = this.CreateModuleTree();
                    }

                    if (this.hasNextSelection)
                    {
                        if (this.selectedModuleTree != null)
                        {
                            this.selectedModuleTree.Dispose();
                            this.selectedModuleTree = null;
                        }

                        if (this.nextSelection != null)
                        {
                            if (this.nextSelection is string && ((string)this.nextSelection == "ModuleConfig"))
                            {
                                this.nextSelection = this.moduleTree.MenuItems[0].Value;
                            }

                            this.selectedModuleTree = PropertyTree.Create(this.nextSelection);
                        }

                        if (this.nextSelection is ModuleDefinition)
                        {
                            (this.nextSelection as ModuleDefinition).OnSelectedInInspector();
                            EditorPrefs.SetString("ODIN_LastSelectedModule", this.nextSelection.GetType().Name);
                        }
                        else
                        {
                            EditorPrefs.SetString("ODIN_LastSelectedModule", "ModuleConfig");
                        }

                        if (this.moduleTree.Selection.Count != 1)
                        {
                            var item = this.moduleTree.MenuItems.FirstOrDefault(n => n.Value == this.nextSelection);

                            if (item != null)
                            {
                                item.Select();
                            }
                        }

                        this.nextSelection = null;
                        this.hasNextSelection = false;
                    }
                }

                Rect menuBorderRect;

                GUILayout.Space(-4);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(-4);
                Rect menuRect = EditorGUILayout.BeginVertical(GUILayoutOptions.Width(this.MenuWidth).ExpandHeight());
                {
                    EditorGUI.DrawRect(menuRect.AddXMin(-3).AddYMax(3), SirenixGUIStyles.MenuBackgroundColor);

                    menuBorderRect = menuRect;
                    menuBorderRect.xMin = menuRect.xMax - 4;
                    menuBorderRect.xMax += 4;

                    if (this.ResizableMenuWidth)
                    {
                        EditorGUIUtility.AddCursorRect(menuBorderRect, MouseCursor.ResizeHorizontal);
                        this.MenuWidth += SirenixEditorGUI.SlideRect(menuBorderRect).x;
                    }

                    this.moduleTree.DrawMenuTree();


                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUILayoutOptions.ExpandWidth().ExpandHeight());
                {
                    if (this.selectedModuleTree != null)
                    {
                        this.selectedModuleTree.Draw(false);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUI.DrawRect(menuBorderRect.AlignCenter(1).AddYMax(4), SirenixGUIStyles.BorderColor);

                if (this.moduleTree != null)
                {
                    this.moduleTree.HandleKeyboardMenuNavigation();
                }
            }
            finally
            {
                EditorTimeHelper.Time = prevTimeHelper;
            }
        }

        private OdinMenuTree CreateModuleTree()//
        {
            var tree = new OdinMenuTree();

            tree.Config.DrawSearchToolbar = ModuleManager.Modules.Count > 10;
            tree.Selection.SupportsMultiSelect = false;

            tree.MenuItems.Add(new OdinMenuItem(tree, "Module Settings", new ModuleSettings()
            {
                ModuleToggling = this.ModuleTogglingSettings,
                ModuleUpdating = this.ModuleUpdateSettings,
            })
            {
                Icon = EditorIcons.SettingsCog.Active
            });

            foreach (var module in ModuleManager.Modules)
            {
                tree.MenuItems.Add(new ModuleMenuItem(tree, module.NiceName, module));
            }

            tree.Selection.SelectionChanged += (type) =>
            {
                var selectedItem = tree.Selection.LastOrDefault();

                if (selectedItem != null)
                {
                    this.nextSelection = selectedItem.Value;
                }
                else
                {
                    this.nextSelection = null;
                }

                this.hasNextSelection = true;
            };

            return tree;
        }

        public enum ModuleAutomationSettings
        {
            Ask,
            Automatic,
            Manual,
        }

        private class ModuleSettings
        {
            [OnInspectorGUI, PropertyOrder(-1)]
            private void OnInspectorGUI()
            {
                GUILayout.Label("Module Settings", ModuleDefinition.TitleStyle);
                SirenixEditorGUI.HorizontalLineSeparator(SirenixGUIStyles.BorderColor, 1);
            }

            [EnumToggleButtons]
            public ModuleAutomationSettings ModuleToggling;

            [EnumToggleButtons]
            public ModuleAutomationSettings ModuleUpdating;

            [OnInspectorGUI]
            private void DrawApplyButton()
            {
                GUILayout.FlexibleSpace();

                bool changed =
                    OdinModuleConfig.Instance.ModuleTogglingSettings != this.ModuleToggling
                    || OdinModuleConfig.Instance.ModuleUpdateSettings != this.ModuleUpdating;

                GUIHelper.PushGUIEnabled(changed);
                if (GUILayout.Button("Apply changes"))
                {
                    OdinModuleConfig.Instance.ModuleTogglingSettings = this.ModuleToggling;
                    OdinModuleConfig.Instance.ModuleUpdateSettings = this.ModuleUpdating;

                    EditorUtility.SetDirty(OdinModuleConfig.Instance);
                    AssetDatabase.SaveAssets();

                    OdinModuleConfig.RefreshModuleSetup();
                    GUIUtility.ExitGUI();
                }
                GUIHelper.PopGUIEnabled();
            }
        }

        private class ModuleMenuItem : OdinMenuItem
        {
            private static GUIStyle backing_StatusStyle;

            private static GUIStyle StatusStyle
            {
                get
                {
                    if (backing_StatusStyle == null)
                    {
                        backing_StatusStyle = new GUIStyle(SirenixGUIStyles.LeftAlignedGreyMiniLabel)
                        {
                            richText = true,
                        };
                    }
                    return backing_StatusStyle;
                }
            }

            private bool unstable;
            private string status;
            private float nextStatusUpdate;

            private static readonly OdinMenuStyle MenuItemStyle = new OdinMenuStyle()
            {
                Height = 40,
                LabelVerticalOffset = -6,
            };

            private static readonly OdinMenuStyle UnstableMenuItemStyle = new OdinMenuStyle()
            {
                Height = 52,
                LabelVerticalOffset = -12,
            };

            public ModuleMenuItem(OdinMenuTree tree, string name, object value) : base(tree, name, value)
            {
                UpdateStatus();
            }

            private void UpdateStatus()
            {
                ModuleDefinition module = (ModuleDefinition)this.Value;

                var manifest = module.LoadManifest();
                var supportsCurrentEnvironment = module.CheckSupportsCurrentEnvironment();

                this.Style = module.UnstableExperimental ? UnstableMenuItemStyle : MenuItemStyle;

                this.unstable = module.UnstableExperimental;

                if (manifest != null)
                {
                    bool canUpgrade = manifest.Version < module.LatestVersion;

                    if (canUpgrade)
                    {
                        this.Icon = EditorIcons.ArrowUp.Active;
                    }
                    else
                    {
                        this.Icon = EditorIcons.Checkmark.Active;
                    }

                    if (supportsCurrentEnvironment)
                    {
                        this.status = "Installed ( <color=#" + ColorUtility.ToHtmlStringRGBA(canUpgrade ? new Color(0.9f, 0.45f, 0.01f, 1f) : new Color(0.1f, 0.9f, 0.1f, 1f)) + ">" + manifest.Version + "</color> )";
                    }
                    else
                    {
                        this.status = "<color=#c0392b>Installed ( dependencies missing )</color>";
                        this.Icon = EditorIcons.UnityErrorIcon;
                    }
                }
                else
                {
                    if (supportsCurrentEnvironment)
                    {
                        this.Icon = EditorIcons.X.Active;
                        this.status = "Inactive ( available: <color=#" + ColorUtility.ToHtmlStringRGBA(new Color(0.1f, 0.9f, 0.1f, 1f)) + ">" + (module.LatestVersion ?? new Version(0, 0, 0, 0)).ToString() + "</color> )";
                    }
                    else
                    {
                        this.Icon = EditorIcons.AlertCircle.Active;
                        this.status = "Inactive ( <color=#c0392b>dependencies missing</color> )";
                    }
                }

                this.nextStatusUpdate = Time.realtimeSinceStartup + 5;
            }

            protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
            {
                if (Time.realtimeSinceStartup > this.nextStatusUpdate)
                {
                    this.UpdateStatus();
                }

                if (!EditorGUIUtility.isProSkin)
                {
                    StatusStyle.normal.textColor = this.IsSelected ? Color.white : Color.black;
                }

                GUI.Label(labelRect.AlignBottom(16).AddY(12), this.status, ModuleMenuItem.StatusStyle);

                if (this.unstable)
                {
                    GUI.Label(labelRect.AlignBottom(16).AddY(26), "<color=#c0392b>EXPERIMENTAL & UNSTABLE</color>", ModuleMenuItem.StatusStyle);
                }
            }
        }
    }
}
#endif