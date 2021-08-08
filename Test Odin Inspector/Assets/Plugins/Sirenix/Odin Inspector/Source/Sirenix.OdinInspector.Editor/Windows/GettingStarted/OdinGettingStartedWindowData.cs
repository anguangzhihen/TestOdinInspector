#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinGettingStartedWindowData.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    internal class OdinGettingStartedWindowData
    {
        private static string DemoFolder { get { return SirenixAssetPaths.SirenixPluginPath + "Demos/"; } }

        private const string EDITOR_WINDOW_DEMO_FOLDER =                "Editor Windows";
        private const string BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_CS =     "/Scripts/Editor/BasicOdinEditorExampleWindow.cs";
        private const string OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_CS =   "/Scripts/Editor/OverrideGetTargetsExampleWindow.cs";
        private const string QUICKLY_INSPECT_OBJECTS_CS =               "/Scripts/Editor/QuicklyInspectObjects.cs";
        private const string ODIN_MENU_EDITOR_WINDOW_EXAMPLE_CS =       "/Scripts/Editor/OdinMenuEditorWindowExample.cs";
        private const string QUICKLY_INSPECT_OBJECTS_TYPE =             "Sirenix.OdinInspector.Demos.QuicklyInspectObjects";
        private const string BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_TYPE =   "Sirenix.OdinInspector.Demos.BasicOdinEditorExampleWindow";
        private const string ODIN_MENU_EDITOR_WINDOW_EXAMPLE_TYPE =     "Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample";
        private const string OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_TYPE = "Sirenix.OdinInspector.Demos.OverrideGetTargetsExampleWindow";

        private const string SAMPLE_RPG_EDITOR_FOLDER =                  "Sample - RPG Editor";
        private const string RPG_EDITOR_WINDOW_CS =                      "/Scripts/Editor/RPGEditorWindow.cs";
        private const string RPG_EDITOR_WINDOW_TYPE =                    "Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow";

        private const string CUSTOM_ATTRIBUTE_PROCESSORS_FOLDER =        "Custom Attribute Processors";
        private const string CUSTOM_ATTRIBUTE_PROCESSORS_SCENE =         "/Custom Attribute Processors.unity";

        private const string CUSTOM_DRAWERS_FOLDER =                     "Custom Drawers";
        private const string CUSTOM_DRAWERS_SCENE =                      "/Custom Drawers.unity";

        private const string ATTRIBUTES_OVERVIEW_FOLDER =                "Attributes Overview";
        private const string ATTRIBUTES_OVERVIEW_SCENE =                 "/Attributes Overview.unity";

        public static Page OdinEditorWindowsIntroduction
        {
            get
            {
                return new Page()
                {
                    Title = "Odin Editor Windows",
                    Sections = new Section[]
                    {
                   new Section()
                   {
                       Title = "Odin Editor Window Examples",
                       Cards = new Card[]
                       {
                           new Card()
                           {
                               Title =                  "Basic Odin Editor Window",
                               Description =            "Inherit from OdinEditorWindow instead of EditorWindow. This will enable you to render fields, properties and methods " +
                                                        "and make editor windows using attributes, without writing any custom editor code.",
                               Package =                DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + ".unitypackage",
                               AssetPathFromPackage =   DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_CS,
                               CustomActions = new BtnAction[]
                               {
                                   new BtnAction("Open Window", () => AssemblyUtilities.GetTypeByCachedFullName(BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_TYPE).GetMethod("OpenWindow", Flags.AllMembers).Invoke(null, null)),
                                   new BtnAction("Open Script", () => AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_CS)))
                               }
                           },
                           new Card()
                           {
                               Title =                  "Override GetTargets()",
                               Description =            "Odin Editor Windows are not limited to drawing themselves; you can override GetTarget() or GetTargets() to make them display " +
                                                        "scriptable objects, components or any arbitrary types (except value types like structs).",
                               Package =                DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + ".unitypackage",
                               AssetPathFromPackage =   DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_CS,
                               CustomActions = new BtnAction[]
                               {
                                   new BtnAction("Open Window", () => AssemblyUtilities.GetTypeByCachedFullName(OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_TYPE).GetMethod("OpenWindow", Flags.AllMembers).Invoke(null, null)),
                                   new BtnAction("Open Script", () => AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_CS)))
                               }
                           },
                           new Card()
                           {
                               Title =                  "Quickly inspect objects",
                               Description =            "Call OdinEditorWindow.InspectObject(myObj) to quickly pop up an editor window for any given object. This is a great way to quickly debug objects or make custom editor windows on the spot!",
                               Package =                DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + ".unitypackage",
                               AssetPathFromPackage =   DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + QUICKLY_INSPECT_OBJECTS_CS,
                               CustomActions = new BtnAction[]
                               {
                                   new BtnAction("Open Window", () => OdinEditorWindow.InspectObject(Activator.CreateInstance(AssemblyUtilities.GetTypeByCachedFullName(QUICKLY_INSPECT_OBJECTS_TYPE)))),
                                   new BtnAction("Open Script", () => AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + QUICKLY_INSPECT_OBJECTS_CS)))
                               }
                           },
                           new Card()
                           {
                               Title =                  "Odin Menu Editor Windows",
                               Description =            "Derive from OdinMenuEditorWindow to create windows that inspect a custom tree of target objects. " +
                                                        "These are great for organizing your project, and managing Scriptable Objects etc." +
                                                        " Odin itself uses this to draw its preferences window.",
                               Package =                DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + ".unitypackage",
                               AssetPathFromPackage =   DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_CS,
                               CustomActions = new BtnAction[]
                               {
                                   new BtnAction("Open Window", () => AssemblyUtilities.GetTypeByCachedFullName(ODIN_MENU_EDITOR_WINDOW_EXAMPLE_TYPE).GetMethod("OpenWindow", Flags.AllMembers).Invoke(null, null)),
                                   new BtnAction("Open Script", () => AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DemoFolder + EDITOR_WINDOW_DEMO_FOLDER + ODIN_MENU_EDITOR_WINDOW_EXAMPLE_CS))),
                               }
                           }
                       }
                   }
                    },
                };
            }
        }

        public static Page MainPage
        {
            get
            {
                return new Page()
                {
                    Title = "Odin Inspector",
                    Sections = new Section[]
                    {
                    new Section()
                    {
                        Title = "Getting Started",
                        Cards = new Card[]
                        {
                            new Card()
                            {
                                Title =                "Odin Attributes Overview",
                                //Description =          "The best way to get started using Odin is to open up the Attributes Overview Demo scene and look through all of the examples.",
                                Description =           "The best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin Inspector > Attribute Overview.",
                                //AssetPathFromPackage = DemoFolder + ATTRIBUTES_OVERVIEW_FOLDER + ATTRIBUTES_OVERVIEW_SCENE,
                                //Package =              DemoFolder + ATTRIBUTES_OVERVIEW_FOLDER + ".unitypackage",
                                CustomActions = new BtnAction[]
                                {
                                    new BtnAction("Open Attributes Overview", () =>
                                    {
                                        //OpenScene(DemoFolder + ATTRIBUTES_OVERVIEW_FOLDER + ATTRIBUTES_OVERVIEW_SCENE);
                                        AttributesExampleWindow.OpenWindow();
                                    })
                                }
                            },
                            new Card()
                            {
                                Title =         "Odin Editor Windows",
                                Description =   "You can use Odin to rapidly create custom Editor Windows to help organize your project data. " +
                                                "This is where Odin can really help boost your workflow.",
                                SubPage =       OdinEditorWindowsIntroduction,
                                SubPageTitle =  "Learn More"
                            },
                            new Card()
                            {
                                Title =         "The Static Inspector",
                                Description =   "If you're a programmer, then you're likely going find the static inspector helpful during debugging and testing. " +
                                                "Just open up the window, and start using it! You can find the utility under 'Tools > Odin Inspector > Static Inspector'.",
                                CustomActions = new BtnAction[]
                                {
                                    new BtnAction("Open the Static Inspector", () => StaticInspectorWindow.InspectType(typeof(UnityEngine.Time), StaticInspectorWindow.AccessModifierFlags.All, StaticInspectorWindow.MemberTypeFlags.AllButObsolete)),
                                }
                            },
                            new Card()
                            {
                                Title =         "The Serialization Debugger",
                                Description =   "If you are utilizing Odin's serialization, the Serialization Debugger can show you which members of any given " +
                                                "type are being serialized, and whether they are serialized by Unity, Odin or both. " +
                                                "You can find the utility under 'Tools > Odin Inspector > Serialization Debugger' or from the context menu in the inspector.",
                                CustomActions = new BtnAction[]
                                {
                                    new BtnAction("Open the Serialization Debugger", () => SerializationDebuggerWindow.ShowWindow()),
                                }
                            }
                        }
                    },
                    new Section()
                    {
                        Title = "Advanced Topics",
                        Cards = new Card[]
                        {
                            new Card()
                            {
                                Title =                "Custom Drawers",
                                Description =          "Making custom drawers in Odin is 10x faster and 10x more powerful than in vanilla Unity. " +
                                                       "Drawers are strongly typed, with generic resolution, and have full support for the layout system - " +
                                                       "no need to calculate any property heights.",
                                AssetPathFromPackage = DemoFolder + CUSTOM_DRAWERS_FOLDER + CUSTOM_DRAWERS_SCENE,
                                Package =              DemoFolder + CUSTOM_DRAWERS_FOLDER + ".unitypackage",
                                CustomActions = new BtnAction[]
                                {
                                    new BtnAction("Open Example Scene", () =>
                                    {
                                        OpenScene(DemoFolder + CUSTOM_DRAWERS_FOLDER + CUSTOM_DRAWERS_SCENE);
                                    })
                                }
                            },
                            new Card()
                            {
                                Title =                 "Attribute Processors",
                                Description =           "You can take complete control over how Odin finds its members to display and which attributes to put on those members. " +
                                                        "This can be extremely useful for automation and providing support and editor customizations for third-party libraries " +
                                                        "you don't own the code for.",
                                AssetPathFromPackage = DemoFolder + CUSTOM_ATTRIBUTE_PROCESSORS_FOLDER + CUSTOM_ATTRIBUTE_PROCESSORS_SCENE,
                                Package =              DemoFolder + CUSTOM_ATTRIBUTE_PROCESSORS_FOLDER + ".unitypackage",
                                CustomActions = new BtnAction[]
                                {
                                    new BtnAction("Open Example Scene", () =>
                                    {
                                        OpenScene(DemoFolder + CUSTOM_ATTRIBUTE_PROCESSORS_FOLDER + CUSTOM_ATTRIBUTE_PROCESSORS_SCENE);
                                    })
                                }
                            }
                        }
                    },
                    new Section()
                    {
                        Title = "Sample Projects",
                        Cards = new Card[]
                        {
                            new Card()
                            {
                                Title =                 "RPG Editor",
                                Description =           "This project showcases Odin Editor Windows, Odin Selectors, various attribute combinations, and custom " +
                                                        "drawers to build a feature-rich editor window for managing scriptable objects.",
                                AssetPathFromPackage =  DemoFolder + SAMPLE_RPG_EDITOR_FOLDER + RPG_EDITOR_WINDOW_CS,
                                Package =               DemoFolder + SAMPLE_RPG_EDITOR_FOLDER + ".unitypackage",
                                CustomActions = new BtnAction[]
                                {
                                   new BtnAction("Open Window", () => AssemblyUtilities.GetTypeByCachedFullName(RPG_EDITOR_WINDOW_TYPE).GetMethod("Open", Flags.AllMembers).Invoke(null, null)),
                                   new BtnAction("Open Script", () => AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DemoFolder + SAMPLE_RPG_EDITOR_FOLDER + RPG_EDITOR_WINDOW_CS)))
                                },
                            }
                        }
                    },
                    new Section()
                    {
                        Title = "Online Resources",
                        OnInspectorGUI = (window) =>
                        {
                            window.DrawFooter();
                        }
                    }
                    },
                };
            }
        }


        private static void OpenScene(string scenePath)
        {
            UnityEditorEventUtility.DelayAction(() =>
            {
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                AssetDatabase.OpenAsset(scene);
                if (scene as SceneAsset)
                {
                    UnityEditorEventUtility.DelayAction(() =>
                    {
                        GameObject.FindObjectsOfType<Transform>()
                            .Where(x => x.parent == null && x.childCount > 0)
                            .OrderByDescending(x => x.GetSiblingIndex())
                            .Select(x => x.transform.GetChild(0).gameObject)
                            .ForEach(x => EditorGUIUtility.PingObject(x));
                    });
                }
            });
        }

        [HideReferenceObjectPicker]
        public class Page
        {
            public string Title;
            public Section[] Sections = new Section[0];
        }

        [HideReferenceObjectPicker]
        public class Section
        {
            [FoldoutGroup("$Title")]
            public string Title;

            [FoldoutGroup("$Title")]
            public Card[] Cards = new Card[0];

            public int ColCount = 2;

            public Action<OdinGettingStartedWindow> OnInspectorGUI = (x) => { };
        }

        [HideReferenceObjectPicker]
        public class Card
        {
            [FoldoutGroup("$Title")]
            public string Title;

            [Multiline]
            [FoldoutGroup("$Title")]
            public string Description;

            [FoldoutGroup("$Title")]
            public string Package;

            [FoldoutGroup("$Title")]
            public string AssetPathFromPackage;

            [FoldoutGroup("$Title")]
            public Page SubPage;

            [FoldoutGroup("$Title")]
            public string SubPageTitle;

            [FoldoutGroup("$Title")]
            public GUIStyle Style;

            [FoldoutGroup("$Title")]
            public BtnAction[] CustomActions = new BtnAction[0];
        }

        [HideReferenceObjectPicker]
        public class BtnAction
        {
            public string Name;
            public Action Action;

            public BtnAction(string name, Action action)
            {
                this.Name = name;
                this.Action = action;
            }
        }
    }
}
#endif