#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUITable.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using UnityEngine;

    /// <summary>
    /// <para>A Utility class for creating tables in Unity's editor GUI.</para>
    /// <para>A table can either be created from scratch using new GUITable(xCount,yCount), or created using one of the static GUITable.Create overloads.</para>
    /// <para>See the online documentation, for examples and more information.</para>
    /// </summary>
    /// <example>
    /// <para>Creating a matrix table for a two-dimentional array.</para>
    /// <code>
    /// private GUITable table;
    ///
    /// private void Init()
    /// {
    ///     bool[,] boolArr = new bool[20,20];
    ///
    ///     this.table = GUITable.Create(
    ///         twoDimArray: boolArr,
    ///         drawElement: (rect, x, y) => boolArr[x, y] = EditorGUI.Toggle(rect, boolArr[x, y]),
    ///         horizontalLabel: "Optional Horizontal Label",               // horizontalLabel is optional and can be null.
    ///         columnLabels: (rect, x) => GUI.Label(rect, x.ToString()),   // columnLabels is optional and can be null.
    ///         verticalLabel: "Optional Vertical Label",                   // verticalLabel is optional and can be null.
    ///         rowLabels: (rect, x) => GUI.Label(rect, x.ToString())       // rowLabels is optional and can be null.
    ///     );
    /// }
    ///
    /// private void OnGUI()
    /// {
    ///     this.table.DrawTable();
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>Creating a table for a list.</para>
    /// <code>
    /// private GUITable table;
    ///
    /// private void Init()
    /// {
    ///     Listt&lt;SomeClasst&gt; someList = new List&lt;SomeClass&gt;() { new SomeClass(), new SomeClass(), new SomeClass() };
    ///
    ///     this.table = GUITable.Create(someList, "Optional Title",
    ///         new GUITableColumn()
    ///         {
    ///             ColumnTitle = "A",
    ///             OnGUI = (rect, i) => someList[i].A = EditorGUI.TextField(rect, someList[i].A),
    ///             Width = 200,
    ///             MinWidth = 100,
    ///         },
    ///         new GUITableColumn()
    ///         {
    ///             ColumnTitle = "B",
    ///             OnGUI = (rect, i) => someList[i].B = EditorGUI.IntField(rect, someList[i].B),
    ///             Resizable = false,
    ///         },
    ///         new GUITableColumn()
    ///         {
    ///             ColumnTitle = "C",
    ///             OnGUI = (rect, i) => someList[i].C = EditorGUI.IntField(rect, someList[i].C),
    ///             SpanColumnTitle = true,
    ///         }
    ///     );
    /// }
    ///
    /// private void OnGUI()
    /// {
    ///     this.table.DrawTable();
    /// }
    ///
    /// private class SomeClass
    /// {
    ///     public string A;
    ///     public int B;
    ///     public int C;
    ///     public int D;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>Styling a cell.</para>
    /// <para>Each <see cref="GUITableCell"/> has two events, OnGUI and OnGUIStyle. OnGUIStyle is called right before OnGUI, but only in repaint events.</para>
    /// <code>
    /// guiTable[x,y].GUIStyle += rect => EditorGUI.DrawRect(rect, Color.red);
    /// </code>
    /// </example>
    /// <example>
    /// <para>Row and column span.</para>
    /// <para>A cell will span and cover all neighbour cells that are null.</para>
    /// <code>
    /// // Span horizontally:
    /// guiTable[x - 2,y] = null;
    /// guiTable[x - 1,y] = null;
    /// guiTable[x,y].SpanX = true;
    /// guiTable[x + 1,y] = null;
    ///
    /// // Span vertically:
    /// guiTable[x,y - 2] = null;
    /// guiTable[x,y - 1] = null;
    /// guiTable[x,y].SpanY = true;
    /// guiTable[x,y + 1] = null;
    /// </code>
    /// </example>
    /// <seealso cref="GUITable"/>
    /// <seealso cref="GUITableCell"/>
    public partial class GUITable
    {
        private readonly GUITableCell[,] cells;
        private readonly ColumnInfo[] columnInfos;
        private readonly float[] rowHeights;
        private bool isDirty;
        private Rect tableRect;
        private Vector2 minTalbeSize;
        private int numOfAutoWidthColumns;

        /// <summary>
        /// The row count.
        /// </summary>
        public readonly int RowCount;

        /// <summary>
        /// The column count.
        /// </summary>
        public readonly int ColumnCount;

        /// <summary>
        /// The Table Rect.
        /// </summary>
        public Rect TableRect { get { return this.tableRect; } }

        /// <summary>
        /// Whether to respect the current GUI indent level.
        /// </summary>
        public bool RespectIndentLevel = true;

        /// <summary>
        /// Gets or sets a <see cref="GUITableCell"/> from the <see cref="GUITable"/>.
        /// </summary>
        public GUITableCell this[int x, int y]
        {
            get { return this.cells[x, y]; }
            set
            {
                if (value != null)
                {
                    value.Table = this;
                    value.X = x;
                    value.Y = y;
                }
                this.cells[x, y] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GUITable"/> class.
        /// </summary>
        public GUITable(int columnCount, int rowCount)
        {
            this.cells = new GUITableCell[columnCount, rowCount];
            this.RowCount = rowCount;
            this.ColumnCount = columnCount;
            this.rowHeights = new float[rowCount];
            this.columnInfos = new ColumnInfo[columnCount];

            for (int i = 0; i < this.columnInfos.Length; i++)
            {
                this.columnInfos[i] = new ColumnInfo() { Table = this };
            }
        }

        /// <summary>
        /// Draws the table.
        /// </summary>
        public void DrawTable()
        {
            var e = Event.current.type;

            if (this.minTalbeSize.y == 0 || this.isDirty)
            {
                this.ReCalculateSizes();
            }

            // If there are not auto-width columns it means that the table has a fixed width, and doesn't expand.
            // Otherwise it does expand.
            // Here we also make sure that we allocate atleast 10 pixels per auto-width column.
            GUILayoutOptions.GUILayoutOptionsInstance guiLayoutOptions;
            if (this.numOfAutoWidthColumns == 0)
            {
                guiLayoutOptions = GUILayoutOptions.ExpandWidth(false).Width(this.minTalbeSize.x);
            }
            else
            {
                guiLayoutOptions = GUILayoutOptions.ExpandWidth().MinWidth(this.minTalbeSize.x + this.numOfAutoWidthColumns * 10);
            }

            var newRect = GUILayoutUtility.GetRect(0, this.minTalbeSize.y > 0 ? this.minTalbeSize.y : 10, guiLayoutOptions);

            if (this.RespectIndentLevel)
            {
                newRect = UnityEditor.EditorGUI.IndentedRect(newRect);
            }

            // Recalcualte sizes if resized.
            if (e == EventType.Repaint)
            {
                if (this.tableRect.width != newRect.width || this.tableRect.x != newRect.x || this.tableRect.y != newRect.y)
                {
                    this.tableRect = newRect;
                    this.ReCalculateSizes();
                }
                else
                {
                    this.tableRect = newRect;
                }
            }

            // Handle resizing:
            for (int x = 0; x < this.ColumnCount - 1; x++)
            {
                if (x < this.ColumnCount - 1 && this.columnInfos[x + 1].Resizable == false)
                {
                    continue;
                }
                if (this.columnInfos[x].Resizable == false)
                {
                    continue;
                }

                GUITableCell resizeCell = null;
                for (int y = 0; y < this.RowCount; y++)
                {
                    var candidate = this.cells[x, y];
                    if (candidate != null && candidate.SpanX == false)
                    {
                        resizeCell = candidate;
                        break;
                    }
                }

                if (resizeCell != null)
                {
                    var rect = resizeCell.Rect;
                    rect.x = rect.xMax - 5;
                    rect.width = 10;

                    var mouseDelta = SirenixEditorGUI.SlideRect(rect).x;
                    if (mouseDelta != 0)
                    {
                        if (mouseDelta > 0)
                        {
                            var ci = this.columnInfos[x];
                            ColumnInfo nextResizableCol = null;

                            for (int j = x + 1; j < this.ColumnCount; j++)
                            {
                                if (this.columnInfos[j].Resizable)
                                {
                                    nextResizableCol = this.columnInfos[j];
                                    break;
                                }
                            }

                            if (nextResizableCol != null)
                            {
                                float remaining = nextResizableCol.ColumnWidth - nextResizableCol.ColumnMinWidth;
                                if (nextResizableCol != null && remaining > 0)
                                {
                                    mouseDelta = Mathf.Min(mouseDelta, remaining);
                                    ci.ResizeOffset += mouseDelta;
                                    nextResizableCol.ResizeOffset -= mouseDelta;
                                }

                                this.ReCalculateSizes();
                            }
                        }
                        else
                        {
                            var ci = this.columnInfos[x + 1];
                            mouseDelta *= -1;
                            ColumnInfo prevResizableCol = null;

                            for (int j = x; j >= 0; j--)
                            {
                                if (this.columnInfos[j].Resizable)
                                {
                                    prevResizableCol = this.columnInfos[j];
                                    break;
                                }
                            }

                            if (prevResizableCol != null)
                            {
                                float remaining = prevResizableCol.ColumnWidth - prevResizableCol.ColumnMinWidth;
                                if (prevResizableCol != null && remaining > mouseDelta)
                                {
                                    mouseDelta = Mathf.Min(mouseDelta, remaining);
                                    ci.ResizeOffset += mouseDelta;
                                    prevResizableCol.ResizeOffset -= mouseDelta;
                                }
                            }

                            this.ReCalculateSizes();
                        }
                    }
                }
            }

            GUIHelper.PushIndentLevel(0);

            // Draw Cells:
            for (int x = 0; x < this.ColumnCount; x++)
            {
                for (int y = 0; y < this.RowCount; y++)
                {
                    var cell = this.cells[x, y];
                    if (cell != null)
                    {
                        cell.Draw();
                    }
                }
            }

            GUIHelper.PopIndentLevel();
        }

        /// <summary>
        /// Recaluclates cell and column sizes in the next frame.
        /// </summary>
        public void MarkDirty()
        {
            this.isDirty = true;
            GUIHelper.RequestRepaint();
        }

        private void SpanCellX(ref Rect rect, int x, int y)
        {
            for (int j = x - 1; j >= 0; j--)
            {
                if (this.cells[j, y] != null) break;
                rect.xMin = this.columnInfos[j].ColumnStart;
            }

            for (int j = x + 1; j < this.ColumnCount; j++)
            {
                if (this.cells[j, y] != null) break;
                rect.xMax = this.columnInfos[j].ColumnEnd;
            }
        }

        private void SpanCellY(ref Rect rect, int x, int y)
        {
            for (int j = y + 1; j < this.RowCount; j++)
            {
                if (this.cells[x, j] != null) break;
                rect.height += this.rowHeights[j];
            }

            for (int j = y - 1; j >= 0; j--)
            {
                if (this.cells[x, j] != null) break;
                rect.yMin -= this.rowHeights[j];
            }
        }

        /// <summary>
        /// <para>Recalculates the layout for the entire table.</para>
        /// <para>This method gets called whenever the table is initialized, resized or adjusted. If you are manipulating
        /// the width or height of individual table cells, remember to call this method when you're done.</para>
        /// </summary>
        public void ReCalculateSizes()
        {
            // Finds all fixed sized coulmns by going through all rows, one column at a time.
            // The widest cell is then picked as the column width.
            // 0 == auto, and if all is auto, the remaining width is equally distributed between them whem in the step below.
            this.minTalbeSize.x = 0;
            this.numOfAutoWidthColumns = this.ColumnCount;
            for (int x = 0; x < this.ColumnCount; x++)
            {
                float width = 0;
                float minWidth = 0;
                if (this.ColumnCount != 1)
                {
                    for (int y = 0; y < this.RowCount; y++)
                    {
                        var cell = this.cells[x, y];
                        if (cell != null)
                        {
                            width = Mathf.Max(width, cell.Width);
                            minWidth = Mathf.Max(minWidth, cell.MinWidth);
                        }
                    }
                }

                var col = this.columnInfos[x];
                col.IsAutoSize = width <= 0;
                col.ColumnMinWidth = minWidth;
                if (!col.IsAutoSize)
                {
                    this.minTalbeSize.x += width;
                    this.numOfAutoWidthColumns--;
                }

                col.ColumnWidth = width;
            }

            // Distribute all remaining width equality between all auto width columns (Wdith = 0 is auto).
            // We also find all column positions which we need for creating cell rects.
            float autoWidth = ((this.tableRect.width - this.minTalbeSize.x) / (this.numOfAutoWidthColumns));
            float currX = this.tableRect.x;
            for (int x = 0; x < this.columnInfos.Length; x++)
            {
                var ci = this.columnInfos[x];

                if (ci.ColumnWidth == 0)
                {
                    ci.ColumnWidth = Mathf.Max(0, autoWidth);
                }

                ci.ColumnStart = currX;
                ci.ColumnWidth = Mathf.Max(ci.ColumnWidth + ci.ResizeOffset, ci.ColumnMinWidth);
                ci.ColumnEnd = ci.ColumnStart + ci.ColumnWidth;
                currX += ci.ColumnWidth;
            }

            // Go thgough all columns, one row at a time and find row heights.
            this.minTalbeSize.y = 0;
            for (int y = 0; y < this.RowCount; y++)
            {
                float height = 0;
                for (int x = 0; x < this.ColumnCount; x++)
                {
                    var cell = this.cells[x, y];
                    if (cell != null)
                    {
                        height = Mathf.Max(height, cell.Height);
                    }
                }
                this.minTalbeSize.y += height;
                this.rowHeights[y] = height;
            }

            // Not what all that is done, we can calculate all cell rects.
            for (int x = 0; x < this.ColumnCount; x++)
            {
                var ci = this.columnInfos[x];
                float currY = this.tableRect.y;
                for (int y = 0; y < this.RowCount; y++)
                {
                    float height = this.rowHeights[y];
                    var cell = this.cells[x, y];

                    if (cell != null)
                    {
                        var rect = new Rect(ci.ColumnStart, currY, ci.ColumnWidth, height);

                        // TODO: Handle splan x and y in a better way.
                        // Right now you can only span in one direction.
                        if (cell.SpanX) this.SpanCellX(ref rect, x, y);
                        if (cell.SpanY) this.SpanCellY(ref rect, x, y);

                        cell.Rect = rect;
                    }
                    currY += height;
                }
            }

            this.isDirty = false;
        }

        private class ColumnInfo
        {
            private float resizeOffset;
            public GUITable Table;
            public bool Resizable = true;
            public bool IsAutoSize;
            public float ColumnEnd;
            public float ColumnStart;
            public float ColumnWidth;
            public float ColumnMinWidth = 4;

            // Resize offsets are kept as percentages to better preserve the layout when resizing.
            public float ResizeOffset
            {
                set { this.resizeOffset = value / this.Table.tableRect.width; }
                get { return this.resizeOffset * this.Table.tableRect.width; }
            }
        }
    }
}
#endif