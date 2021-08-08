//-----------------------------------------------------------------------
// <copyright file="TableListAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
    using UnityEngine;

    /// <summary>
    /// Renders lists and arrays in the inspector as tables.
    /// </summary>
    /// <seealso cref="TableColumnWidthAttribute"/>
    /// <seealso cref="TableColumnWidthAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class TableListAttribute : Attribute
    {
        /// <summary>
        /// If ShowPaging is enabled, this will override the default setting specified in the Odin Preferences window.
        /// </summary>
        public int NumberOfItemsPerPage;

        /// <summary>
        /// Mark the table as read-only. This removes all editing capabilities from the list such as Add and delete,
        /// but without disabling GUI for each element drawn as otherwise would be the case if the <see cref="ReadOnlyAttribute"/> was used.
        /// </summary>
        public bool IsReadOnly;

        /// <summary>
        /// The default minimum column width - 40 by default. This can be overwriten by individual columns using the <see cref="TableColumnWidthAttribute"/>.
        /// </summary>
        public int DefaultMinColumnWidth = 40;

        /// <summary>
        /// If true, a label is drawn for each element which shows the index of the element.
        /// </summary>
        public bool ShowIndexLabels;

        /// <summary>
        /// Whether to draw all rows in a scroll-view.
        /// </summary>
        public bool DrawScrollView = true;

        /// <summary>
        /// The number of pixels before a scroll view appears. 350 by default.
        /// </summary>
        public int MinScrollViewHeight = 350;

        /// <summary>
        /// The number of pixels before a scroll view appears. 0 by default.
        /// </summary>
        public int MaxScrollViewHeight;

        /// <summary>
        /// If true, expanding and collapsing the table from the table title-bar is no longer an option.
        /// </summary>
        public bool AlwaysExpanded;

        /// <summary>
        /// Whether to hide the toolbar containing the add button and pagin etc.s
        /// </summary>
        public bool HideToolbar = false;

        /// <summary>
        /// The cell padding.
        /// </summary>
        public int CellPadding = 2;

        [SerializeField, HideInInspector]
        private bool showPagingHasValue = false;

        [SerializeField, HideInInspector]
        private bool showPaging = false;

        /// <summary>
        /// Whether paging buttons should be added to the title bar. The default value of this, can be customized from the Odin Preferences window.
        /// </summary>
        public bool ShowPaging
        {
            get { return this.showPaging; }
            set
            {
                this.showPaging = value;
                this.showPagingHasValue = true;
            }
        }

        /// <summary>
        /// Whether the ShowPaging property has been set.
        /// </summary>
        public bool ShowPagingHasValue { get { return this.showPagingHasValue; } }
       
        /// <summary>
        /// Sets the Min and Max ScrollViewHeight.
        /// </summary>
        public int ScrollViewHeight
        {
            get { return Math.Min(this.MinScrollViewHeight, this.MaxScrollViewHeight); }
            set { this.MinScrollViewHeight = this.MaxScrollViewHeight = value; }
        }
    }
}