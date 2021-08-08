#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ModuleManager.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class ModuleManager
    {
        public ModuleDataManager DataManager;
        public List<ModuleDefinition> Modules;

        public static ModuleManager CreateDefault()
        {
            var result = new ModuleManager()
            {
                DataManager = new ModuleDataManager()
                {
                    DataPath = SirenixAssetPaths.OdinPath + "Modules",
                    InstallPath = SirenixAssetPaths.OdinPath + "Modules"
                },
                Modules = new List<ModuleDefinition>()
            };

            result.Modules.Add(new UnityMathematicsModuleDefinition()
            {
                ModuleManager = result
            });
            
            result.Modules.Add(new ECSModuleDefinition()
            {
                ModuleManager = result
            });

            //result.Modules.Add(new AddressablesModuleDefinition()
            //{
            //    ModuleManager = result,
            //});

            //result.Modules.Add(new DOTSModuleDefinition()
            //{
            //    ModuleManager = result
            //});

            return result;
        }

        public bool Refresh()
        {
            bool changed = false;

            foreach (var module in this.Modules)
            {
                var activated = module.CheckIsActivated();
                var supported = module.CheckSupportsCurrentEnvironment();

                if (activated && !supported)
                {
                    bool automate = false;

                    if (OdinModuleConfig.HasInstanceLoaded)
                    {
                        var config = OdinModuleConfig.Instance.GetConfig(module);

                        if (config != null && config.ActivationSettings == ActivationSettings.GlobalSettings)
                        {
                            if (OdinModuleConfig.Instance.ModuleTogglingSettings == OdinModuleConfig.ModuleAutomationSettings.Ask)
                            {
                                automate = AskActivationAutomationQuestion(module, false, OdinModuleConfig.Instance);
                            }
                            else automate = OdinModuleConfig.Instance.ModuleTogglingSettings == OdinModuleConfig.ModuleAutomationSettings.Automatic;
                        }
                    }

                    if (automate)
                    {
                        Debug.Log("Automatically deactivating Odin Module '" + module.NiceName + "', because its dependencies have gone missing...");
                        module.Deactivate();
                        changed = true;
                    }
                }
                else if (!activated && supported)
                {
                    bool automate = false;

                    if (!module.UnstableExperimental && OdinModuleConfig.HasInstanceLoaded)
                    {
                        var config = OdinModuleConfig.Instance.GetConfig(module);

                        if (config != null && config.ActivationSettings == ActivationSettings.GlobalSettings)
                        {
                            switch (OdinModuleConfig.Instance.ModuleTogglingSettings)
                            {
                                case OdinModuleConfig.ModuleAutomationSettings.Ask:
                                    automate = AskActivationAutomationQuestion(module, true, OdinModuleConfig.Instance);
                                    break;
                                case OdinModuleConfig.ModuleAutomationSettings.Automatic:
                                    automate = true;
                                    break;
                                case OdinModuleConfig.ModuleAutomationSettings.Manual:
                                default:
                                    automate = false;
                                    break;
                            }
                        }
                    }

                    if (automate)
                    {
                        Debug.Log("Automatically activating Odin Module '" + module.NiceName + "' version '" + module.LatestVersion + "', because its dependencies were detected...");
                        module.Activate();
                        changed = true;
                    }
                }
                else if (activated)
                {
                    var manifest = module.LoadManifest();

                    if (manifest != null && manifest.Version < module.LatestVersion)
                    {
                        // Can update
                        bool automate = false;

                        switch (OdinModuleConfig.Instance.ModuleUpdateSettings)
                        {
                            case OdinModuleConfig.ModuleAutomationSettings.Ask:
                                automate = AskUpdateAutomationQuestion(module, manifest.Version, OdinModuleConfig.Instance);
                                break;
                            case OdinModuleConfig.ModuleAutomationSettings.Automatic:
                                automate = true;
                                break;
                            case OdinModuleConfig.ModuleAutomationSettings.Manual:
                            default:
                                automate = false;
                                break;
                        }

                        if (automate)
                        {
                            Debug.Log("Automatically updating Odin Module '" + module.NiceName + "' from version '" + manifest.Version + "' to '" + module.LatestVersion + "'");
                            module.Deactivate();
                            module.Activate();
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        private bool AskActivationAutomationQuestion(ModuleDefinition module, bool activate, OdinModuleConfig instance)
        {
            string dependencies = "";

            if (!string.IsNullOrEmpty(module.DependenciesDescription))
            {
                dependencies = " (" + module.DependenciesDescription + ")";
            }

            string message = activate ?
                "The Odin Module '" + module.NiceName + "' is not activated, but the conditions for its activation have been detected" + dependencies + ". Do you want to automatically activate the module?" :
                "The Odin Module '" + module.NiceName + "' is activated, but some of its dependencies" + dependencies + " are missing and you will likely get compiler errors. Do you want to automatically deactivate the module?";

            int answer = EditorUtility.DisplayDialogComplex(
                "Odin Module automation",
                message,
                "Always automate modules",
                "Just this once",
                "Never do this");

            switch (answer)
            {
                case 0:         // Automate always
                    instance.ModuleTogglingSettings = OdinModuleConfig.ModuleAutomationSettings.Automatic;
                    EditorUtility.SetDirty(instance);
                    AssetDatabase.SaveAssets();
                    return true;
                case 1:         // Just this once
                    return true;
                case 2:         // Never do this
                default:
                    instance.ModuleTogglingSettings = OdinModuleConfig.ModuleAutomationSettings.Manual;
                    EditorUtility.SetDirty(instance);
                    AssetDatabase.SaveAssets();
                    return false;

            }
        }

        private bool AskUpdateAutomationQuestion(ModuleDefinition module, Version old, OdinModuleConfig instance)
        {
            string message = "The installed Odin Module '" + module.NiceName + "' is out of date (" + old + "), and a new version is available (" + module.LatestVersion + "). Do you want to automatically update the module?";

            int answer = EditorUtility.DisplayDialogComplex(
                "Odin Module automation",
                message,
                "Always update modules",
                "Just this once",
                "Never do this");

            switch (answer)
            {
                case 0:         // Automate always
                    instance.ModuleUpdateSettings = OdinModuleConfig.ModuleAutomationSettings.Automatic;
                    EditorUtility.SetDirty(instance);
                    AssetDatabase.SaveAssets();
                    return true;
                case 1:         // Just this once
                    return true;
                case 2:         // No
                default:
                    instance.ModuleUpdateSettings = OdinModuleConfig.ModuleAutomationSettings.Manual;
                    EditorUtility.SetDirty(instance);
                    AssetDatabase.SaveAssets();
                    return false;

            }
        }

        public void PackageAllModules()
        {
            foreach (var module in this.Modules)
            {
                var data = module.GetModuleDataForPackaging();

                foreach (var file in data.Files)
                {
                    Debug.Log("Packaging file '" + file.Path + "'...");
                }

                var bytes = ModuleData.Serialize(data);

                this.DataManager.SaveData(module.ID, bytes);
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif