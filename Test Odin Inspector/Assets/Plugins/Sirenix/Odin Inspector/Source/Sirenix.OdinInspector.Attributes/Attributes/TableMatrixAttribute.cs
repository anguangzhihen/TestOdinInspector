//-----------------------------------------------------------------------
// <copyright file="TableMatrixAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// The TableMatrix attribute is used to further specify how Odin should draw two-dimensional arrays.
    /// </summary>
    /// <example>
    /// <code>
    /// // Inheriting from SerializedMonoBehaviour is only needed if you want Odin to serialize the multi-dimensional arrays for you.
    /// // If you prefer doing that yourself, you can still make Odin show them in the inspector using the ShowInInspector attribute.
    /// public class TableMatrixExamples : SerializedMonoBehaviour
    /// {
    ///     [InfoBox("Right-click and drag column and row labels in order to modify the tables."), PropertyOrder(-10), OnInspectorGUI]
    ///     private void ShowMessageAtOP() { }
    ///
    ///     [BoxGroup("Two Dimensional array without the TableMatrix attribute.")]
    ///     public bool[,] BooleanTable = new bool[15, 6];
    ///
    ///     [BoxGroup("ReadOnly table")]
    ///     [TableMatrix(IsReadOnly = true)]
    ///     public int[,] ReadOnlyTable = new int[5, 5];
    ///
    ///     [BoxGroup("Labled table")]
    ///     [TableMatrix(HorizontalTitle = "X axis", VerticalTitle = "Y axis")]
    ///     public GameObject[,] LabledTable = new GameObject[15, 10];
    ///
    ///     [BoxGroup("Enum table")]
    ///     [TableMatrix(HorizontalTitle = "X axis")]
    ///     public InfoMessageType[,] EnumTable = new InfoMessageType[4,4];
    ///
    ///     [BoxGroup("Custom table")]
    ///     [TableMatrix(DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false)]
    ///     public bool[,] CustomCellDrawing = new bool[30,30];
    ///
    ///     #if UNITY_EDITOR
    ///
    ///         private static bool DrawColoredEnumElement(Rect rect, bool value)
    ///         {
    ///             if (Event.current.type == EventType.MouseDown &#38;&#38; rect.Contains(Event.current.mousePosition))
    ///             {
    ///                 value = !value;
    ///                 GUI.changed = true;
    ///                 Event.current.Use();
    ///             }
    ///
    ///             UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0, 0, 0, 0.5f));
    ///
    ///             return value;
    ///         }
    ///
    ///     #endif
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class TableMatrixAttribute : Attribute
    {
        /// <summary>
        /// If true, inserting, removing and dragging columns and rows will become unavailable. But the cells themselves will remain modifiable.
        /// If you want to disable everything, you can use the <see cref="ReadOnly"/> attribute.
        /// </summary>
        public bool IsReadOnly = false;

        /// <summary>
        /// Whether or not columns are resizable.
        /// </summary>
        public bool ResizableColumns = true;

        /// <summary>
        /// The vertical title label.
        /// </summary>
        public string VerticalTitle = null;

        /// <summary>
        /// The horizontal title label.
        /// </summary>
        public string HorizontalTitle = null;

        /// <summary>
        /// Override how Odin draws each cell.                  <para />
        /// [TableMatrix(DrawElementMethod='DrawMyElement')]    <para />
        /// public MyType[,] myArray;                           <para />
        /// private static MyType DrawElement(Rect rect, MyType value) { return GUI.DrawMyType(rect, value); }
        /// </summary>
        public string DrawElementMethod = null;

        /// <summary>
        /// The height for all rows. 0 = default row height.
        /// </summary>
        public int RowHeight = 0;

        /// <summary>
        /// If true, the height of each row will be the same as the width of the first cell.
        /// </summary>
        public bool SquareCells = false;

        /// <summary>
        /// If true, no column indices drawn.
        /// </summary>
        public bool HideColumnIndices = false;

        /// <summary>
        /// If true, no row indices drawn.
        /// </summary>
        public bool HideRowIndices = false;

        /// <summary>
        /// Whether the drawn table should respect the current GUI indent level.
        /// </summary>
        public bool RespectIndentLevel = true;

        /// <summary>
        /// If true, tables are drawn with rows/columns reversed (C# initialization order).
        /// </summary>
        public bool Transpose = false;
    }
}