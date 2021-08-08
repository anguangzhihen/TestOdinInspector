#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUITabPage.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A tab page created by <see cref="GUITabGroup"/>.
    /// </summary>
    /// <seealso cref="GUITabGroup"/>
    public class GUITabPage
    {
        private static GUIStyle innerContainerStyle;

        private static GUIStyle InnerContainerStyle
        {
            get
            {
                if (innerContainerStyle == null)
                {
                    innerContainerStyle = new GUIStyle()
                    {
                        padding = new RectOffset(3, 3, 3, 3),
                    };
                }

                return innerContainerStyle;
            }
        }

        internal int Order = 0;
        private GUITabGroup tabGroup;
        private Color prevColor;
        private static int pageIndexIncrementer = 0;
        private bool isSeen;
        private bool isMessured = false;

        /// <summary>
        /// Gets the title of the tab.
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// Gets the title of the tab.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the rect of the page.
        /// </summary>
        public Rect Rect { get; private set; }

        internal bool IsActive { get; set; }

        internal bool IsVisible { get; set; }

        internal GUITabPage(GUITabGroup tabGroup, string title)
        {
            this.Name = title;
            this.Title = title;
            this.tabGroup = tabGroup;
            this.IsActive = true;
        }

        internal void OnBeginGroup()
        {
            pageIndexIncrementer = 0;
            this.isSeen = false;
        }

        internal void OnEndGroup()
        {
            if (Event.current.type == EventType.Repaint)
            {
                this.IsActive = this.isSeen;
            }
        }

        /// <summary>
        /// Begins the page.
        /// </summary>
        public bool BeginPage()
        {
            if (this.tabGroup.FixedHeight && this.isMessured == false)
            {
                this.IsVisible = true;
            }

            this.isSeen = true;

            if (this.IsVisible)
            {
                var options = GUILayoutOptions.Width(this.tabGroup.InnerContainerWidth).ExpandHeight(this.tabGroup.ExpandHeight);
                var rect = EditorGUILayout.BeginVertical(InnerContainerStyle, options);

                GUIHelper.PushHierarchyMode(false);
                GUIHelper.PushLabelWidth(this.tabGroup.LabelWidth - 4);

                if (Event.current.type == EventType.Repaint)
                {
                    this.Rect = rect;
                }
                if (this.tabGroup.IsAnimating)
                {
                    this.prevColor = GUI.color;
                    var col = this.prevColor;
                    col.a *= this.tabGroup.CurrentPage == this ? this.tabGroup.T : 1 - this.tabGroup.T;
                    GUI.color = col;
                }
            }
            return this.IsVisible;
        }

        /// <summary>
        /// Ends the page.
        /// </summary>
        public void EndPage()
        {
            if (this.IsVisible)
            {
                GUIHelper.PopLabelWidth();
                GUIHelper.PopHierarchyMode();

                if (this.tabGroup.IsAnimating)
                {
                    GUI.color = this.prevColor;
                }
                EditorGUILayout.EndVertical();
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.isMessured = true;
                this.Order = pageIndexIncrementer++;
            }
        }
    }
}
#endif