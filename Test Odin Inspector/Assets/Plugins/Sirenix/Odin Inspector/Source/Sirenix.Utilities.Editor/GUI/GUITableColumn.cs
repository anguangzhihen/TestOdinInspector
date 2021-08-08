#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUITableColumn.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using UnityEngine;

    /// <summary>
    /// GUITableColumns used creating a table list using GUITable.Create().
    /// </summary>
    /// <seealso cref="GUITable"/>
    /// <seealso cref="GUITableCell"/>
    public class GUITableColumn
    {
        /// <summary>
        /// Draws a cell at the given row index for this column.
        /// </summary>
        public Action<Rect, int> OnGUI;

        /// <summary>
        /// The column title text. If there are is columns with a title, there we not be rendered an additional table row for column titles.
        /// </summary>
        public string ColumnTitle;

        /// <summary>
        /// The minimum with of the column.
        /// </summary>
        public float MinWidth;

        /// <summary>
        /// The width of the Column.
        /// 0 = auto, and is also the default.
        /// </summary>
        public float Width;

        /// <summary>
        /// If true, the column becomes resiziable.
        /// Default is true.
        /// </summary>
        public bool Resizable = true;

        /// <summary>
        /// If true, the column title cell, will span horizontally to neighbour columns, which column titles are null.
        /// Default is false.
        /// </summary>
        public bool SpanColumnTitle = false;
    }
}
#endif