#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIPagingHelper.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A helper class to control paging of n number of elements in various situations.
    /// </summary>
    public class GUIPagingHelper
    {
        private Rect prevRect;
        private bool isEnabled = true;
        private int elementCount;
        [SerializeField]
        private int currentPage;
        private int startIndex;
        private int endIndex;
        private int pageCount;

        private int numberOfItemsPrPage;

        private int? nextPageNumber;
        private bool? nextIsExpanded;

        /// <summary>
        /// Disables the paging, and show all elements.
        /// </summary>
        [SerializeField]
        public bool IsExpanded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GUIPagingHelper"/> class.
        /// </summary>
        public GUIPagingHelper()
        {
            this.numberOfItemsPrPage = 1;
        }

        /// <summary>
        /// Updates all values based on <paramref name="elementCount"/> and <see cref="NumberOfItemsPrPage"/>.
        /// </summary>
        /// <remarks>
        /// Call update right before using <see cref="StartIndex"/> and <see cref="EndIndex"/> in your for loop.
        /// </remarks>
        /// <param name="elementCount">The total number of elements to apply paging for.</param>
        public void Update(int elementCount)
        {
            if (elementCount < 0)
            {
                throw new ArgumentOutOfRangeException("Non-negative number required.");
            }

            this.elementCount = elementCount;

            if (this.isEnabled)
            {
                this.pageCount = Mathf.Max(1, Mathf.CeilToInt(this.elementCount / (float)this.numberOfItemsPrPage));
                this.currentPage = Mathf.Clamp(this.currentPage, 0, this.pageCount - 1);
                this.startIndex = this.currentPage * this.numberOfItemsPrPage;
                this.endIndex = Mathf.Min(this.elementCount, this.startIndex + this.numberOfItemsPrPage);
            }
            else
            {
                this.startIndex = 0;
                this.endIndex = this.elementCount;
            }

            if (Event.current.type == EventType.Layout)
            {
                if (this.nextPageNumber != null)
                {
                    this.currentPage = this.nextPageNumber.Value;
                    this.nextPageNumber = null;
                }

                if (this.nextIsExpanded != null)
                {
                    this.IsExpanded = this.nextIsExpanded.Value;
                    this.nextIsExpanded = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.isEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is on the frist page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is on frist page; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnFirstPage
        {
            get
            {
                return this.currentPage == 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is on the last page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is on last page; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnLastPage
        {
            get
            {
                return this.currentPage == this.pageCount - 1;
            }
        }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        /// <value>
        /// The number of items pr page.
        /// </value>
        public int NumberOfItemsPerPage
        {
            get { return this.numberOfItemsPrPage; }
            set { this.numberOfItemsPrPage = Mathf.Max(value, 0); }
        }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        /// <value>
        /// The current page.
        /// </value>
        public int CurrentPage
        {
            get { return this.currentPage; }
            set
            {
                this.currentPage = Mathf.Clamp(value, 0, this.PageCount - 1);
            }
        }

        /// <summary>
        /// Gets the start index.
        /// </summary>
        /// <value>
        /// The start index.
        /// </value>
        public int StartIndex
        {
            get
            {
                if (this.IsExpanded)
                {
                    return 0;
                }

                return this.startIndex;
            }
        }

        /// <summary>
        /// Gets the end index.
        /// </summary>
        /// <value>
        /// The end index.
        /// </value>
        public int EndIndex
        {
            get
            {
                if (this.IsExpanded)
                {
                    return this.elementCount;
                }

                return this.endIndex;
            }
        }

        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        /// <value>
        /// The page count.
        /// </value>
        public int PageCount
        {
            get { return this.pageCount; }
        }

        /// <summary>
        /// Gets the total number of elements.
        /// Use <see cref="Update(int)"/> to change the value.
        /// </summary>
        public int ElementCount
        {
            get { return this.elementCount; }
        }

        /// <summary>
        /// Draws right-aligned toolbar paging buttons.
        /// </summary>
        public void DrawToolbarPagingButtons(ref Rect toolbarRect, bool showPaging, bool showItemCount, int btnWidth = 23)
        {
            if (this.prevRect.height == 0)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    this.prevRect = toolbarRect;
                }

                return;
            }

            //var isRepaint = Event.current.type == EventType.Repaint;
            var drawPaging = this.isEnabled && !this.IsExpanded && showPaging && this.pageCount > 1;   // btnWith * 2
            var drawExpand = this.isEnabled && this.pageCount > 1;                                                            // btnWith
            var drawPagingField = drawPaging;                                                           // 40?

            // Expand
            if (drawExpand)
            {
                var btnRect = toolbarRect.AlignRight(btnWidth, true);
                toolbarRect.xMax = btnRect.xMin;
                if (GUI.Button(btnRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
                {
                    GUIHelper.RemoveFocusControl();
                    this.nextIsExpanded = !this.IsExpanded;
                }
                (this.IsExpanded ? EditorIcons.TriangleUp : EditorIcons.TriangleDown).Draw(btnRect, 16);
            }

            // Right
            if (drawPaging)
            {
                //if (this.IsOnLastPage && isRepaint) GUIHelper.PushGUIEnabled(false);
                var btnRect = toolbarRect.AlignRight(btnWidth, true);
                if (GUI.Button(btnRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
                {
                    GUIHelper.RemoveFocusControl();
                    if (Event.current.button == 1)
                    {
                        this.nextPageNumber = this.PageCount - 1;
                    }
                    else
                    {
                        this.nextPageNumber = this.currentPage + 1;

                        if (this.nextPageNumber >= this.pageCount)
                        {
                            this.nextPageNumber = 0;
                        }
                    }
                }
                EditorIcons.TriangleRight.Draw(btnRect, 16);
                //if (this.IsOnLastPage && isRepaint) GUIHelper.PopGUIEnabled();
                toolbarRect.xMax = btnRect.xMin;
            }

            // Paging field
            if (drawPagingField)
            {
                var pageCountLbl = "/ " + this.PageCount.ToString();
                var lblLength = SirenixGUIStyles.Label.CalcSize(new GUIContent(pageCountLbl)).x;
                var lblRect = toolbarRect.AlignRight(lblLength + 5, true);
                toolbarRect.xMax = lblRect.xMin;
                var fldRect = toolbarRect.AlignRight(lblLength, true);
                toolbarRect.xMax = fldRect.xMin;
                fldRect.xMin += 4;
                fldRect.y -= 1;
                GUI.Label(lblRect, pageCountLbl, SirenixGUIStyles.LabelCentered);

                var next = SirenixEditorGUI.SlideRectInt(lblRect, 0, this.CurrentPage);
                if (next != this.CurrentPage)
                {
                    this.nextPageNumber = next;
                }

                next = EditorGUI.IntField(fldRect.AlignCenterY(15), this.CurrentPage + 1) - 1;
                if (next != this.CurrentPage)
                {
                    this.nextPageNumber = next;
                }
            }

            // Left
            if (drawPaging)
            {
                //if (this.IsOnFirstPage && isRepaint) GUIHelper.PushGUIEnabled(false);
                var btnRect = toolbarRect.AlignRight(btnWidth, true);
                if (GUI.Button(btnRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
                {
                    GUIHelper.RemoveFocusControl();
                    if (Event.current.button == 1)
                    {
                        this.nextPageNumber = 0;
                    }
                    else
                    {
                        this.nextPageNumber = this.currentPage - 1;

                        if (this.nextPageNumber < 0)
                        {
                            this.nextPageNumber = this.pageCount - 1;
                        }
                    }
                }
                EditorIcons.TriangleLeft.Draw(btnRect, 16);
                //if (this.IsOnFirstPage && isRepaint) GUIHelper.PopGUIEnabled();
                toolbarRect.xMax = btnRect.xMin;
            }

            // Item Count
            if (showItemCount && Event.current.type != EventType.Layout)
            {
                var lbl = new GUIContent(this.ElementCount == 0 ? "Empty" : this.ElementCount + " items");
                var width = SirenixGUIStyles.LeftAlignedGreyMiniLabel.CalcSize(lbl).x + 5;
                var lblRect = toolbarRect.AlignRight(width);
                GUI.Label(lblRect, lbl, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                toolbarRect.xMax = lblRect.xMin;
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.prevRect = toolbarRect;
            }
        }
    }
}
#endif