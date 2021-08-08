#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUITabGroup.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The GUITabGroup is a utility class to draw animated tab groups.
    /// </summary>
    /// <example>
    /// <code>
    /// var tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(someKey);
    /// // Register your tabs before starting BeginGroup.
    /// var tab1 = tabGroup.RegisterTab("tab 1");
    /// var tab2 = tabGroup.RegisterTab("tab 2");
    ///
    /// tabGroup.BeginGroup(drawToolbar: true);
    /// {
    ///     if (tab1.BeginPage())
    ///     {
    ///         // Draw GUI for the first tab page;
    ///     }
    ///     tab1.EndPage();
    ///
    ///     if (tab2.BeginPage())
    ///     {
    ///         // Draw GUI for the second tab page;
    ///     }
    ///     tab2.EndPage();
    /// }
    /// tabGroup.EndGroup();
    ///
    /// // Control the animation speed.
    /// tabGroup.AnimationSpeed = 0.2f;
    ///
    /// // If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
    /// tabGroup.FixedHeight = true;
    ///
    /// // You can change page by calling:
    /// tabGroup.GoToNextPage();
    /// tabGroup.GoToPreviousPage();
    /// </code>
    /// </example>
    /// <seealso cref="SirenixEditorGUI"/>
    public class GUITabGroup
    {
        private GUILayoutOption[] options = GUILayoutOptions.ExpandWidth(true).ExpandHeight(false);
        private EditorTimeHelper time = new EditorTimeHelper();
        private GUITabPage currentPage;
        private GUITabPage targetPage;
        private Vector2 scrollPosition;
        private float currentHeight;
        private Dictionary<string, GUITabPage> pages = new Dictionary<string, GUITabPage>();

        /// <summary>
        /// The animation speed (1 / s)
        /// </summary>

        private float t = 1f;
        private bool isAnimating;
        private GUITabPage nextPage;
        private bool drawToolbar;
        private float toolbarHeight = 18;
        private Rect toolbarRect;

        /// <summary>
        /// The animation speed
        /// </summary>
        public float AnimationSpeed = 4;

        public bool FixedHeight;
        public bool ExpandHeight;

        public bool DrawNonSelectedTabsAsDisabled = false;

        private IEnumerable<GUITabPage> OrderedPages
        {
            get
            {
                return this.pages.Select(x => x.Value).OrderBy(x => x.Order);
            }
        }

        /// <summary>
        /// Gets the outer rect of the entire tab group.
        /// </summary>
        public Rect OuterRect { get; private set; }

        /// <summary>
        /// The inner rect of the current tab page.
        /// </summary>
        public Rect InnerRect { get; private set; }

        /// <summary>
        /// If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
        /// </summary>

        /// <summary>
        /// Sets the current page.
        /// </summary>
        /// <param name="page">The page to switch to.</param>
        public void SetCurrentPage(GUITabPage page)
        {
            if (!this.pages.ContainsValue(page))
            {
                throw new InvalidOperationException("Page is not part of TabGroup");
            }

            this.currentPage = page;
            this.targetPage = null;
        }

        public GUITabPage NextPage { get { return this.nextPage; } }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public GUITabPage CurrentPage { get { return this.targetPage ?? this.currentPage; } }

        /// <summary>
        /// Gets the t.
        /// </summary>
        public float T { get { return this.t; } }

        internal bool IsAnimating { get { return this.isAnimating; } }

        internal float InnerContainerWidth { get; private set; }

        internal float LabelWidth { get; private set; }

        /// <summary>
        /// The height of the tab buttons.
        /// </summary>
        public float ToolbarHeight
        {
            get { return this.toolbarHeight; }
            set { this.toolbarHeight = value; }
        }

        /// <summary>
        /// Registers the tab.
        /// </summary>
        public GUITabPage RegisterTab(string title)
        {
            if (title == null)
            {
                throw new ArgumentNullException("title");
            }
            GUITabPage result;
            if (this.pages.TryGetValue(title, out result) == false)
            {
                result = this.pages[title] = new GUITabPage(this, title);
            }

            return result;
        }

        /// <summary>
        /// Begins the group.
        /// </summary>
        /// <param name="drawToolbar">if set to <c>true</c> a tool-bar for changing pages is drawn.</param>
        /// <param name="style">The style.</param>
        public void BeginGroup(bool drawToolbar = true, GUIStyle style = null)
        {
            this.LabelWidth = GUIHelper.BetterLabelWidth;

            if (Event.current.type == EventType.Layout)
            {
                this.drawToolbar = drawToolbar;
            }

            style = style ?? SirenixGUIStyles.ToggleGroupBackground;

            this.InnerContainerWidth = this.OuterRect.width - (
                 style.padding.left +
                 style.padding.right +
                 style.margin.left +
                 style.margin.right
             );

            if (this.currentPage == null && this.pages.Count > 0)
            {
                this.currentPage = this.pages.Select(x => x.Value).OrderBy(x => x.Order).First();
            }

            if (this.currentPage != null && this.pages.ContainsKey(this.currentPage.Name) == false)
            {
                if (this.pages.Count > 0)
                {
                    this.currentPage = this.OrderedPages.First();
                }
                else
                {
                    this.currentPage = null;
                }
            }

            float maxHeight = 0;

            foreach (var page in this.pages.GFValueIterator())
            {
                page.OnBeginGroup();
                maxHeight = Mathf.Max(page.Rect.height, maxHeight);
                if (Event.current.type == EventType.Layout)
                {
                    if (page.IsVisible != (page.IsVisible = page == this.targetPage || page == this.currentPage))
                    {
                        if (this.targetPage == null)
                        {
                            this.scrollPosition.x = 0f;
                            this.currentHeight = this.currentPage.Rect.height;
                        }
                        else
                        {
                            this.scrollPosition.x = this.targetPage.Order >= this.currentPage.Order ? 0 : this.scrollPosition.x = this.OuterRect.width;
                            this.currentHeight = this.currentPage.Rect.height;
                        }
                    }
                }
            }

            GUILayout.Space(1);
            var outerRect = EditorGUILayout.BeginVertical(style, GUILayoutOptions.ExpandWidth(true).ExpandHeight(this.ExpandHeight));

            if (this.drawToolbar)
            {
                this.DrawToolbar();
            }

            if (this.InnerRect.width > 0 && !this.ExpandHeight)
            {
                if (this.options.Length == 2)
                {
                    if (this.currentPage != null)
                    {
                        this.currentHeight = this.currentPage.Rect.height;
                    }

                    this.options = GUILayoutOptions.ExpandWidth(true).ExpandHeight(this.ExpandHeight).Height(this.currentHeight);
                }

                if (this.FixedHeight)
                {
                    this.options[2] = GUILayout.Height(maxHeight);
                }
                else
                {
                    this.options[2] = GUILayout.Height(this.currentHeight);
                }
            }

            GUIHelper.PushGUIEnabled(false);
            GUILayout.BeginScrollView(this.scrollPosition, false, false, GUIStyle.none, GUIStyle.none, this.options);
            GUIHelper.PopGUIEnabled();
            var innerRect = EditorGUILayout.BeginHorizontal(GUILayoutOptions.ExpandHeight(this.ExpandHeight));

            if (Event.current.type == EventType.Repaint)
            {
                this.OuterRect = outerRect;
                this.InnerRect = innerRect;
            }
        }

        /// <summary>
        /// Ends the group.
        /// </summary>
        public void EndGroup()
        {
            EditorGUILayout.EndHorizontal();
            GUIHelper.PushGUIEnabled(false);
            GUILayout.EndScrollView();
            GUIHelper.PopGUIEnabled();

            EditorGUILayout.EndVertical();

            if (this.targetPage != this.currentPage)
            {
                GUIHelper.RequestRepaint();
            }

            if (this.currentPage != null && Event.current.type == EventType.Repaint)
            {
                if (this.isAnimating && this.targetPage != null && this.targetPage != this.currentPage)
                {
                    this.t = this.t + this.time.DeltaTime * this.AnimationSpeed;
                    this.scrollPosition.x = Mathf.Lerp(this.currentPage.Rect.x, this.targetPage.Rect.x, Mathf.Min(1f, MathUtilities.Hermite01(this.t)));
                    this.currentHeight = Mathf.Lerp(this.currentPage.Rect.height, this.targetPage.Rect.height, Mathf.Min(1f, MathUtilities.Hermite01(this.t)));

                    if (this.t >= 1f)
                    {
                        this.currentPage.IsVisible = false;
                        this.currentPage = this.targetPage;
                        this.targetPage = null;
                        this.scrollPosition.x = 0f;
                        this.currentHeight = this.currentPage.Rect.height;
                        this.t = 1f;
                    }
                }
                else
                {
                    this.t = 0f;
                    this.isAnimating = false;
                    this.scrollPosition.x = this.currentPage.Rect.x;
                    this.currentHeight = this.currentPage.Rect.height;
                    if (this.targetPage != null && this.targetPage != this.currentPage && this.targetPage.IsVisible)
                    {
                        this.isAnimating = true;
                        this.scrollPosition.x = this.targetPage.Order > this.currentPage.Order ? 0 : this.scrollPosition.x = this.OuterRect.width;
                        this.t = 0;
                    }
                }
            }

            foreach (var page in this.pages.GFValueIterator())
            {
                page.OnEndGroup();
            }
            this.time.Update();

            if (this.isAnimating == false && this.nextPage != null)
            {
                this.targetPage = this.nextPage;
                this.nextPage = null;
            }
        }

        private void DrawToolbar()
        {
            if (Event.current.type == EventType.Layout)
            {
                this.toolbarRect = this.OuterRect;
                this.toolbarRect.height = this.toolbarHeight;
                this.toolbarRect.x += 1;
                this.toolbarRect.width -= 1;
            }

            //if (Event.current.OnRepaint())
            //{
            //    SirenixEditorGUI.DrawBorders(new Rect(GUILayoutUtility.GetLastRect()) { height = this.toolbarHeight }, 1);
            //}

            SirenixEditorGUI.BeginHorizontalToolbar(this.toolbarHeight);
            foreach (var page in this.OrderedPages)
            {
                if (page.IsActive)
                {
                    bool isActive = page == (this.nextPage ?? this.CurrentPage); // What

                    if (this.DrawNonSelectedTabsAsDisabled && !isActive)
                    {
                        GUIHelper.PushGUIEnabled(false);
                    }

                    if (SirenixEditorGUI.ToolbarTab(isActive, page.Title))
                    {
                        if (this.currentPage != page)
                        {
                            this.nextPage = page;
                        }
                    }

                    if (this.DrawNonSelectedTabsAsDisabled && !isActive)
                    {
                        GUIHelper.PopGUIEnabled();
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            if (Event.current.OnRepaint())
            {
                SirenixEditorGUI.DrawBorders(new Rect(GUILayoutUtility.GetLastRect()) { height = this.toolbarHeight }, 1, 1, 0, 0);
            }

        }

        /// <summary>
        /// Goes to page.
        /// </summary>
        public void GoToPage(GUITabPage page)
        {
            this.nextPage = page;
        }

        public void GoToPage(string pageName)
        {
            GUITabPage page;
            if (this.pages.TryGetValue(pageName, out page))
            {
                this.GoToPage(page);
            }
            else
            {
                throw new InvalidOperationException("No such tab page exists");
            }
        }

        /// <summary>
        /// Goes to next page.
        /// </summary>
        public void GoToNextPage()
        {
            if (this.currentPage != null)
            {
                bool takeNext = false;
                var ordered = this.OrderedPages.ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    if (takeNext && ordered[i].IsActive)
                    {
                        this.nextPage = ordered[i];
                        break;
                    }
                    if (ordered[i] == (this.nextPage ?? this.CurrentPage))
                    {
                        takeNext = true;
                    }
                }
            }
        }

        /// <summary>
        /// Goes to previous page.
        /// </summary>
        public void GoToPreviousPage()
        {
            if (this.currentPage != null)
            {
                var ordered = this.OrderedPages.ToList();
                int prevIdx = -1;
                for (int i = 0; i < ordered.Count; i++)
                {
                    if (ordered[i] == (this.nextPage ?? this.CurrentPage))
                    {
                        if (prevIdx >= 0)
                        {
                            this.nextPage = ordered[prevIdx];
                        }
                        break;
                    }

                    if (ordered[i].IsActive)
                    {
                        prevIdx = i;
                    }
                }
            }
        }
    }
}
#endif