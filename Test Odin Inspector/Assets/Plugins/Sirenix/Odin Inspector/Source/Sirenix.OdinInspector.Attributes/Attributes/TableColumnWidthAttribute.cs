//-----------------------------------------------------------------------
// <copyright file="TableColumnWidthAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// The TableColumnWidth attribute is used to further customize the width of a column in tables drawn using the <see cref="TableListAttribute"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// [TableList]
    /// public List&lt;SomeType&gt; TableList = new List&lt;SomeType&gt;();
    /// 
    /// [Serializable]
    /// public class SomeType
    /// {
    ///     [LabelWidth(30)]
    ///     [TableColumnWidth(130, false)]
    ///     [VerticalGroup("Combined")]
    ///     public string A;
    /// 
    ///     [LabelWidth(30)]
    ///     [VerticalGroup("Combined")]
    ///     public string B;
    /// 
    ///     [Multiline(2), Space(3)]
    ///     public string fields;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TableListAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class TableColumnWidthAttribute : Attribute
    {
        /// <summary>
        /// The width of the column.
        /// </summary>
        public int Width;

        /// <summary>
        /// Whether the column should be resizable. True by default.
        /// </summary>
        public bool Resizable = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumnWidthAttribute"/> class.
        /// </summary>
        /// <param name="width">The width of the column in pixels.</param>
        /// <param name="resizable">If <c>true</c> then the column can be resized in the inspector.</param>
        public TableColumnWidthAttribute(int width, bool resizable = true)
        {
            this.Width = width;
            this.Resizable = resizable;
        }
    }
}