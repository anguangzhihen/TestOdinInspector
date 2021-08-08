#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinGettingStartedWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// The Odin Getting Started Window
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.Editor.OdinEditorWindow" />
    public class OdinGettingStartedWindow : OdinEditorWindow
    {
        internal static GUIStyle sectionTitleLabelStyle;
        internal static GUIStyle cardTitleStyle;
        internal static GUIStyle logoTitleStyle;
        internal static GUIStyle cardStylePadding;
        internal static GUIStyle cardStyle;
        internal static GUIStyle cardHorizontalStyle;

        private Vector2 scrollPoss;
        private float width;
        private Rect currSectionRect;
        private int currBtnCount = 0;
        private static string editorOnlyMode;

        /// <summary>
        /// Shows the window.
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<OdinGettingStartedWindow>().position = GUIHelper.GetEditorWindowRect().AlignCenter(715, 660);
        }

        private SlidePageNavigationHelper<OdinGettingStartedWindowData.Page> pager;

        /// <summary>
        /// Initialize get called by OnEnable and by OnGUI after assembly reloads
        /// which often happens when you recompile or enter and exit play mode.
        /// </summary>
        protected override void Initialize()
        {
            this.pager = new SlidePageNavigationHelper<OdinGettingStartedWindowData.Page>();
            this.pager.TabGroup.ExpandHeight = false;
            this.pager.PushPage(OdinGettingStartedWindowData.MainPage, "Overview");
            this.WindowPadding = new Vector4(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a scroll view.
        /// </summary>
        public override bool UseScrollView
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Draws the Odin Editor Window.
        /// </summary>
        protected override void OnGUI()
        {
            this.InitStyles();
            var rect = EditorGUILayout.BeginVertical();
            {
                if (EditorGUIUtility.isProSkin)
                {
                    EditorGUI.DrawRect(new Rect(0, rect.yMax, this.position.width, this.position.height), SirenixGUIStyles.DarkEditorBackground);
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(0, 0, this.position.width, rect.yMax), SirenixGUIStyles.BoxBackgroundColor);
                }
                this.DrawHeader();
                this.DrawPaging();
            }
            EditorGUILayout.EndVertical();
            SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1, SirenixGUIStyles.BorderColor);


            this.DrawPages();
        }

        [ResponsiveButtonGroup(DefaultButtonSize = ButtonSizes.Medium)]
        private void Manual() { Application.OpenURL("https://odininspector.com/tutorials"); }

        [LabelText("API Docs")]
        [ResponsiveButtonGroup]
        private void APIDocumentation() { Application.OpenURL("https://odininspector.com/documentation"); }

        // TODO: We can re-add this if we add a dedicated FAQ page on odininspector.com.
        //[ResponsiveButtonGroup]
        //private void FAQ() { Application.OpenURL("http://sirenix.net/odininspector/faq"); }

        [ResponsiveButtonGroup]
        private void Roadmap() { Application.OpenURL("https://odininspector.com/roadmap"); }

        [ResponsiveButtonGroup]
        private void IssueTracker() { Application.OpenURL("https://bitbucket.org/sirenix/odin-inspector/issues"); }

        [ResponsiveButtonGroup]
        private void Support() { Application.OpenURL("https://odininspector.com/support"); }

        [ResponsiveButtonGroup]
        private void Discord() { Application.OpenURL("https://discord.gg/WTYJEra"); }

        private void DrawPaging()
        {
            var rect = GUILayoutUtility.GetRect(0, 25);
            this.pager.DrawPageNavigation(rect);
        }

        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 70);
            GUI.Label(rect.AlignCenterY(45), new GUIContent(" Odin Inspector", EditorIcons.OdinInspectorLogo), logoTitleStyle);

            if (editorOnlyMode == null)
            {
                editorOnlyMode = EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled() ? "Enabled" : "Disabled";
            }

            var versionLabel = new GUIContent("Odin Inspector " + OdinInspectorVersion.BuildName + " " + OdinInspectorVersion.Version);
            var editorOnlyModeLabel = new GUIContent("Editor Only Mode: " + editorOnlyMode);
            var w = Mathf.Max(SirenixGUIStyles.CenteredGreyMiniLabel.CalcSize(versionLabel).x, SirenixGUIStyles.CenteredGreyMiniLabel.CalcSize(editorOnlyModeLabel).x, 165);

            var rightRect = rect.AlignRight(w + 10);
            rightRect.x -= 10;
            rightRect.y += 8;
            rightRect.height = 17;
            if (Event.current.type == EventType.Repaint)
            {
                GUI.Label(rightRect, editorOnlyModeLabel, SirenixGUIStyles.CenteredGreyMiniLabel);
            }
            rightRect.y += 15;
            if (Event.current.type == EventType.Repaint)
            {
                GUI.Label(rightRect, versionLabel, SirenixGUIStyles.CenteredGreyMiniLabel);
            }
            rightRect.y += rightRect.height + 4;
            if (GUI.Button(rightRect, "View Release Notes", SirenixGUIStyles.MiniButton))
            {
                Application.OpenURL("https://odininspector.com/patch-notes");
            }

            SirenixEditorGUI.DrawHorizontalLineSeperator(rect.x, rect.y, rect.width);
            SirenixEditorGUI.DrawHorizontalLineSeperator(rect.x, rect.yMax, rect.width);
        }

        private void InitStyles()
        {
            sectionTitleLabelStyle = sectionTitleLabelStyle ?? new GUIStyle(SirenixGUIStyles.SectionHeaderCentered)
            {
                fontSize = 17,
                margin = new RectOffset(0, 0, 10, 10),
            };

            cardTitleStyle = cardTitleStyle ?? new GUIStyle(SirenixGUIStyles.SectionHeader)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, 0, 4)
            };

            logoTitleStyle = logoTitleStyle ?? new GUIStyle(SirenixGUIStyles.SectionHeader)
            {
                fontSize = 23,
                padding = new RectOffset(20, 20, 0, 0),
                alignment = TextAnchor.MiddleLeft
            };

            cardStylePadding = cardStylePadding ?? new GUIStyle()
            {
                padding = new RectOffset(15, 15, 15, 15),
                stretchHeight = false
            };

            cardStyle = cardStyle ?? new GUIStyle("sv_iconselector_labelselection")
            {
                padding = new RectOffset(15, 15, 15, 15),
                margin = new RectOffset(0, 0, 0, 0),
                stretchHeight = false
            };

            cardHorizontalStyle = cardHorizontalStyle ?? new GUIStyle()
            { 
                padding = new RectOffset(5, 4, 0, 0) 
            };
        }

        private void DrawPages()
        {
            GUIHelper.PushLabelWidth(10);
            this.scrollPoss = EditorGUILayout.BeginScrollView(this.scrollPoss, GUILayoutOptions.ExpandHeight(true));
            {
                var rect = EditorGUILayout.BeginVertical();
                if (this.width == 0 || Event.current.type == EventType.Repaint)
                {
                    this.width = rect.width;
                }
                this.pager.BeginGroup();
                foreach (var page in this.pager.EnumeratePages)
                {
                    if (page.BeginPage())
                    {
                        DrawPage(page.Value);
                    }
                    page.EndPage();
                }
                this.pager.EndGroup();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            GUIHelper.PopLabelWidth();
        }

        internal void DrawFooter()
        {
            base.OnGUI();
        }

        private void DrawPage(OdinGettingStartedWindowData.Page value)
        {
            for (var j = 0; j < value.Sections.Length; j++)
            {
                var section = value.Sections[j];
                int colCount = this.position.width < 470 ? 1 : section.ColCount;

                if (section.Title != null)
                {
                    GUILayout.Label(new GUIContent(section.Title), sectionTitleLabelStyle);
                }

                section.OnInspectorGUI(this);

                if (section.Cards.Length != 0)
                {
                    float height = 0;
                    bool end = false;
                    for (int i = 0; i < section.Cards.Length; i++)
                    {
                        if (i % colCount == 0)
                        {
                            if (i != 0 && colCount != 0)
                            {
                                end = false;
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(10);
                            }

                            var r = EditorGUILayout.BeginHorizontal(cardHorizontalStyle);
                            this.currSectionRect = r;

                            height = r.height;
                            end = true;
                        }

                        if (colCount == 0) GUILayout.FlexibleSpace();
                        var rect = EditorGUILayout.BeginVertical(cardStylePadding, GUILayoutOptions.Width(this.width / colCount - 12));
                        rect.height = height;
                        if (Event.current.type == EventType.Repaint)
                        {
                            if (section.Cards[i].Style != null)
                            {
                                section.Cards[i].Style.Draw(rect, GUIContent.none, 0);
                            }
                            else
                            {
                                GUIHelper.PushColor(new Color(1, 1, 1, EditorGUIUtility.isProSkin ? 0.25f : 0.45f));
                                cardStyle.Draw(rect, GUIContent.none, 0);
                                GUIHelper.PopColor();
                            }
                        }
                        DrawCard(section.Cards[i]);
                        EditorGUILayout.EndVertical();
                        if (i % colCount == 0) GUILayout.FlexibleSpace();
                    }

                    if (end)
                    {
                        EditorGUILayout.EndHorizontal();
                    }
                }
                GUILayout.Space(8);

                if (j != value.Sections.Length - 1)
                {
                    SirenixEditorGUI.DrawThickHorizontalSeparator(10, 0);
                }
            }
        }

        private void DrawCard(OdinGettingStartedWindowData.Card card)
        {
            if (card.Title != null)
            {
                GUILayout.Label(card.Title, cardTitleStyle);
            }

            if (card.Description != null)
            {
                GUILayout.Label(card.Description, SirenixGUIStyles.MultiLineLabel);
            }

            if (card.Title != null || card.Description != null)
            {
                GUILayout.Space(5);
            }

            currBtnCount = 0;
            bool needsImport = card.AssetPathFromPackage != null && !File.Exists(card.AssetPathFromPackage);


            if (needsImport) GUIHelper.PushGUIEnabled(false);
            for (int i = 0; i < card.CustomActions.Length; i++)
            {
                var action = card.CustomActions[i];
                if (Button(action.Name))
                {
                    UnityEditorEventUtility.DelayAction(() =>
                    {
                        action.Action();
                    });
                }
            }
            if (card.SubPage != null && Button(card.SubPageTitle ?? card.SubPage.Title))
            {
                this.pager.PushPage(card.SubPage, card.SubPage.Title);
            }
            if (needsImport) GUIHelper.PopGUIEnabled();

            if (needsImport)
            {
                if (card.Package != null)
                {
                    var hasPackage = File.Exists(card.Package);
                    var text = "Import " + Path.GetFileNameWithoutExtension(card.Package);

                    GUIHelper.PushGUIEnabled(hasPackage);
                    if (Button(text))
                    {
                        UnityEditorEventUtility.DelayAction(() =>
                        {
                            AssetDatabase.ImportPackage(card.Package, true);
                        });
                    }
                    GUIHelper.PopGUIEnabled();
                }
            }
        }

        private bool Button(string txt)
        {
            var rect = GUILayoutUtility.GetRect(0, 26);
            rect.y = this.currSectionRect.yMax - 15;
            rect.y -= ++currBtnCount * 26;
            rect.height = 22;

            return GUI.Button(rect, txt, SirenixGUIStyles.Button);
        }
    }
}
#endif