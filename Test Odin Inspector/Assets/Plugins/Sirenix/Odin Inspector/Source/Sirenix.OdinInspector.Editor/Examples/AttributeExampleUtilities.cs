#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AttributeExampleUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public static class AttributeExampleUtilities
    {
        private static readonly CategoryComparer CategorySorter = new CategoryComparer();

        private static readonly Type[] AttributeTypes;
        private static readonly Dictionary<Type, OdinRegisterAttributeAttribute> AttributeRegisterMap;

        static AttributeExampleUtilities()
        {
            AttributeRegisterMap = AssemblyUtilities.GetAllAssemblies()
                .SelectMany(a => a.GetAttributes<OdinRegisterAttributeAttribute>(true))
                .Concat(InternalAttributeRegistry.Attributes)
                .Cast<OdinRegisterAttributeAttribute>()
                .Where(attr => OdinInspectorVersion.IsEnterprise || !attr.IsEnterprise)
                .ToDictionary(x => x.AttributeType);

            AttributeTypes = AttributeRegisterMap.Keys.ToArray();
        }

        public static IEnumerable<Type> GetAllOdinAttributes()
        {
            return AttributeTypes;
        }

        public static IEnumerable<string> GetAttributeCategories(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration) && registration.Categories != null)
            {
                // TODO: Cache this?
                return registration.Categories.Split(',').Select(x => x.Trim());
            }
            else
            {
                return new string[] { "Uncategorized" };
            }
        }

        public static string GetAttributeDescription(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration))
            {
                return registration.Description;
            }
            else
            {
                return null;
            }
        }

        public static string GetOnlineDocumentationUrl(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration))
            {
                return registration.DocumentationUrl;
            }

            return null;
        }

        public static bool GetIsEnterprise(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration))
            {
                return registration.IsEnterprise;
            }

            return false;
        }

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            foreach (var a in GetAllOdinAttributes())
            {
                // TODO: tags?
                string search = a.Name + " " + string.Join(" ", GetAttributeExampleInfos(a).Select(x => x.Name).ToArray());

                foreach (var c in GetAttributeCategories(a))
                {
                    var item = new OdinMenuItem(tree, a.GetNiceName().Replace("Attribute", "").SplitPascalCase(), a)
                    {
                        Value = a,
                        SearchString = search,
                    };
                    search = null; // Only allow the user to find the first item of an attribute by search.

                    tree.AddMenuItemAtPath(c, item);
                }
            }

            tree.MenuItems.Sort(CategorySorter);
            tree.MarkDirty();
        }

        // TODO: The names of GetAttributeExampleInfos and GetExample methods are kinda confusing
        // and doesn't clearly indicate the difference between the two methods.
        public static AttributeExampleInfo[] GetAttributeExampleInfos(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            AttributeExampleInfo[] examples;
            if (InternalAttributeExampleInfoMap.Map.TryGetValue(attributeType, out examples) == false)
            {
                examples = new AttributeExampleInfo[0];
            }

            return examples;
        }

        public static OdinAttributeExampleItem GetExample<T>() where T : Attribute
        {
            return GetExample(typeof(T));
        }

        public static OdinAttributeExampleItem GetExample(Type attributeType)
        {
            OdinRegisterAttributeAttribute registration;
            AttributeRegisterMap.TryGetValue(attributeType, out registration);
            return new OdinAttributeExampleItem(attributeType, registration);
        }

        private class CategoryComparer : IComparer<OdinMenuItem>
        {
            private static readonly Dictionary<string, int> Order = new Dictionary<string, int>()
            {
                { "Essentials", -10 },
                { "Misc", 8 },
                { "Meta", 9 },
                { "Unity", 10 },
                { "Debug", 50 },
            };

            public int Compare(OdinMenuItem x, OdinMenuItem y)
            {
                int xOrder;
                int yOrder;
                if (Order.TryGetValue(x.Name, out xOrder) == false) xOrder = 0;
                if (Order.TryGetValue(y.Name, out yOrder) == false) yOrder = 0;

                if (xOrder == yOrder)
                {
                    return x.Name.CompareTo(y.Name);
                }
                else
                {
                    return xOrder.CompareTo(yOrder);
                }
            }
        }
    }

    public class OdinAttributeExampleItem
    {
        private static GUIStyle headerGroupStyle;
        private static GUIStyle tabGroupStyle;
        private static Color backgroundColor = new Color32(195, 195, 195, 255);

        private Type attributeType;
        private OdinRegisterAttributeAttribute registration;
        private AttributeExamplePreview[] examples;
        private GUITabGroup tabGroup;
        public readonly string Name;

        public bool DrawCodeExample { get; set; }

        public OdinAttributeExampleItem(Type attributeType, OdinRegisterAttributeAttribute registration)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            this.attributeType = attributeType;
            this.registration = registration;
            this.Name = this.attributeType.GetNiceName().SplitPascalCase();
            this.DrawCodeExample = true;

            var exampleInfos = AttributeExampleUtilities.GetAttributeExampleInfos(attributeType);
            this.examples = new AttributeExamplePreview[exampleInfos.Length];
            for (int i = 0; i < exampleInfos.Length; i++)
            {
                this.examples[i] = new AttributeExamplePreview(exampleInfos[i]);
            }

            this.tabGroup = new GUITabGroup()
            {
                ToolbarHeight = 30,
            };
            for (int i = 0; i < exampleInfos.Length; i++)
            {
                this.tabGroup.RegisterTab(exampleInfos[i].Name);
            }
        }

        [OnInspectorGUI]
        public void Draw()
        {
            headerGroupStyle = headerGroupStyle ?? new GUIStyle()
            {
                padding = new RectOffset(4, 6, 10, 4),
            };
            tabGroupStyle = tabGroupStyle ?? new GUIStyle(SirenixGUIStyles.BoxContainer)
            {
                padding = new RectOffset(0, 0, 0, 0),
            };

            GUILayout.BeginVertical(headerGroupStyle);
            GUILayout.Label(this.Name, SirenixGUIStyles.SectionHeader);

            if (string.IsNullOrEmpty(this.registration.DocumentationUrl) == false)
            {
                var rect = GUILayoutUtility.GetLastRect()
                    .AlignCenterY(20)
                    .AlignRight(120);

                if (GUI.Button(rect, "Documentation", SirenixGUIStyles.MiniButton))
                {
                    Help.BrowseURL(this.registration.DocumentationUrl);
                }
            }

            SirenixEditorGUI.DrawThickHorizontalSeparator(4, 10);

            if (string.IsNullOrEmpty(this.registration.Description) == false)
            {
                GUILayout.Label(this.registration.Description, SirenixGUIStyles.MultiLineLabel);
                SirenixEditorGUI.DrawThickHorizontalSeparator(10, 10);
            }

            if (this.examples.Length > 0)
            {
                var c = GUI.backgroundColor;
                GUI.backgroundColor = backgroundColor;
                this.tabGroup.BeginGroup(true, tabGroupStyle);
                GUI.backgroundColor = c;

                foreach (var example in this.examples)
                {
                    var tab = this.tabGroup.RegisterTab(example.ExampleInfo.Name);
                    if (tab.BeginPage())
                    {
                        example.Draw(this.DrawCodeExample);
                    }
                    tab.EndPage();
                }
                this.tabGroup.EndGroup();
            }
            else
            {
                GUILayout.Label("No examples available.");
            }
            GUILayout.EndVertical();
        }

        public void OnDeselected()
        {
            foreach (var example in this.examples)
            {
                example.OnDeselected();
            }
        }
    }

    internal class AttributeExamplePreview
    {
        private static GUIStyle exampleGroupStyle;
        private static GUIStyle previewStyle;
        private static GUIStyle codeTextStyle;
        private static Color previewBackgroundColorDark = new Color32(56, 56, 56, 255);
        private static Color previewBackgroundColorLight = new Color32(194, 194, 194, 255);

        public AttributeExampleInfo ExampleInfo;
        private PropertyTree tree;
        private string highlightedCode = null;
        private string highlightedCodeAsComponent = null;
        private Vector2 scrollPosition;
        //private bool showRaw;
        private bool showComponent;

        public AttributeExamplePreview(AttributeExampleInfo exampleInfo)
        {
            this.ExampleInfo = exampleInfo;

            try
            {
                this.highlightedCode = SyntaxHighlighter.Parse(this.ExampleInfo.Code);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                this.highlightedCode = this.ExampleInfo.Code;
                //this.showRaw = true;
            }

            try
            {
                this.highlightedCodeAsComponent = SyntaxHighlighter.Parse(this.ExampleInfo.CodeAsComponent);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                this.highlightedCodeAsComponent = this.ExampleInfo.CodeAsComponent;
                //this.showRaw = true;
            }
        }

        public void Draw(bool drawCodeExample)
        {
            if (exampleGroupStyle == null)
            {
                exampleGroupStyle = new GUIStyle(GUIStyle.none) { padding = new RectOffset(1, 1, 10, 0) };
            }
            if (previewStyle == null)
            {
                previewStyle = new GUIStyle(GUIStyle.none) { padding = new RectOffset(0, 0, 0, 0), };
            }

            GUILayout.BeginVertical(exampleGroupStyle);

            bool addSpacing = false;

            if (this.ExampleInfo.Description != null)
            {
                {
                    SirenixEditorGUI.BeginBox();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(23);
                    GUILayout.Label(this.ExampleInfo.Description, SirenixGUIStyles.MultiLineLabel);

                    var rect = GUILayoutUtility.GetLastRect();
                    EditorIcons.X.Draw(rect.SubX(26).SetWidth(26).AlignCenterXY(20), EditorIcons.Info.Active);

                    GUILayout.EndHorizontal();
                    SirenixEditorGUI.EndBox();
                }

                addSpacing = true;
            }

            if (this.ExampleInfo.ExampleType.IsDefined(typeof(ShowOdinSerializedPropertiesInInspectorAttribute), false))
            {
                SirenixEditorGUI.BeginBox();
                GUILayout.BeginHorizontal();
                GUILayout.Space(23);
                GUILayout.Label("Note that this example requires Odin's serialization to be enabled to work, since it uses types that Unity will not serialize. If you copy the example as a component using the 'Copy Component' or 'Create Component Script' buttons, the code will have been set up with Odin's serialization enabled already.", SirenixGUIStyles.MultiLineLabel);

                var rect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(rect.SubX(26).SetWidth(26).AlignCenterXY(20), EditorIcons.UnityWarningIcon);

                GUILayout.EndHorizontal();
                SirenixEditorGUI.EndBox();

                addSpacing = true;
            }

            if (addSpacing)
            {
                GUILayout.Space(12);
            }

            GUILayout.Label("Interactive Preview", SirenixGUIStyles.BoldTitle);
            GUILayout.BeginVertical(previewStyle, GUILayoutOptions.ExpandWidth(true));
            {
                Rect rect = GUIHelper.GetCurrentLayoutRect().Expand(4, 0);
                SirenixEditorGUI.DrawSolidRect(rect, EditorGUIUtility.isProSkin ? previewBackgroundColorDark : previewBackgroundColorLight);
                SirenixEditorGUI.DrawBorders(rect, 1);

                GUILayout.Space(8);
                this.tree = this.tree ?? PropertyTree.Create(this.ExampleInfo.PreviewObject);
                this.tree.Draw(false);
                GUILayout.Space(8);
            }
            GUILayout.EndVertical();

            if (drawCodeExample && this.ExampleInfo.Code != null)
            {
                GUILayout.Space(12);
                GUILayout.Label("Code", SirenixGUIStyles.BoldTitle);

                Rect rect = SirenixEditorGUI.BeginToolbarBox();
                SirenixEditorGUI.DrawSolidRect(rect.HorizontalPadding(1), SyntaxHighlighter.BackgroundColor);

                SirenixEditorGUI.BeginToolbarBoxHeader();
                {
                    //if (SirenixEditorGUI.ToolbarButton(this.showRaw ? "View Highlighted" : "View Raw"))
                    //{
                    //    this.showRaw = !this.showRaw;
                    //}

                    if (SirenixEditorGUI.ToolbarButton(this.showComponent ? "View Shortened Code" : "View Component Code"))
                    {
                        this.showComponent = !this.showComponent;
                    }

                    GUILayout.FlexibleSpace();
                    if (SirenixEditorGUI.ToolbarButton("Copy View"))
                    {
                        if (this.showComponent)
                        {
                            Clipboard.Copy(this.ExampleInfo.CodeAsComponent);
                        }
                        else
                        {
                            Clipboard.Copy(this.ExampleInfo.Code);
                        }
                    }
                    if (this.ExampleInfo.CodeAsComponent != null)
                    {
                        if (SirenixEditorGUI.ToolbarButton("Save Component Script"))
                        {
                            string filePath = EditorUtility.SaveFilePanelInProject("Create Component File", this.ExampleInfo.ExampleType.Name + "Component.cs", "cs", "Choose a location to save the example as a component script.");

                            if (!string.IsNullOrEmpty(filePath))
                            {
                                File.WriteAllText(filePath, this.ExampleInfo.CodeAsComponent);
                                AssetDatabase.Refresh();
                            }

                            GUIUtility.ExitGUI();
                        }
                    }
                }
                SirenixEditorGUI.EndToolbarBoxHeader();

                if (codeTextStyle == null)
                {
                    codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
                    codeTextStyle.normal.textColor = SyntaxHighlighter.TextColor;
                    codeTextStyle.active.textColor = SyntaxHighlighter.TextColor;
                    codeTextStyle.focused.textColor = SyntaxHighlighter.TextColor;
                    codeTextStyle.wordWrap = false;
                }

                GUIContent codeContent = this.showComponent ?
                    GUIHelper.TempContent(/*this.showRaw ? this.ExampleInfo.CodeAsComponent.TrimEnd('\n', '\r') : */this.highlightedCodeAsComponent)
                    : GUIHelper.TempContent(/*this.showRaw ? this.ExampleInfo.Code.TrimEnd('\n', '\r') : */this.highlightedCode);
                Vector2 size = codeTextStyle.CalcSize(codeContent);

                GUILayout.BeginHorizontal();
                GUILayout.Space(-3);
                GUILayout.BeginVertical();
                {
                    GUIHelper.PushEventType(Event.current.type == EventType.ScrollWheel ? EventType.Used : Event.current.type); // Prevent the code horizontal scroll view from eating the scroll event.

                    this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, true, false, GUI.skin.horizontalScrollbar, GUIStyle.none, GUILayout.MinHeight(size.y + 20));
                    var codeRect = GUILayoutUtility.GetRect(size.x + 50, size.y).AddXMin(4).AddY(2);

                    //if (this.showRaw)
                    //{
                    //    EditorGUI.SelectableLabel(codeRect, codeContent.text, codeTextStyle);
                    //    GUILayout.Space(-14);
                    //}
                    //else
                    //{
                    GUI.Label(codeRect, codeContent, codeTextStyle);
                    //}

                    GUILayout.EndScrollView();

                    GUIHelper.PopEventType();
                }
                GUILayout.EndVertical();
                GUILayout.Space(-3);
                GUILayout.EndHorizontal();
                GUILayout.Space(-3);

                SirenixEditorGUI.EndToolbarBox();
            }
            GUILayout.EndVertical();
        }

        public void OnDeselected()
        {
            if (this.tree != null)
            {
                this.tree.Dispose();
                this.tree = null;
            }
        }
    }
}
#endif