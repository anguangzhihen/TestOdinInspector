#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUITableFactory.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.Utilities;
    using UnityEngine;

    public partial class GUITable
    {
        private void ApplyTitleStyle(GUITableCell from, GUITableCell to)
        {
            if (from == null) throw new NullReferenceException("from");
            if (to == null) throw new NullReferenceException("to");

            bool horizontal = from.X < to.X;
            bool vertical = from.Y < to.Y;

            var titleBgColor = SirenixGUIStyles.ListItemColorEven * 0.9f;
            titleBgColor.a = 0.7f;

            from.GUIStyle += rect =>
            {
                if (!horizontal && !vertical)
                {
                    SirenixEditorGUI.DrawSolidRect(rect, titleBgColor);
                    rect.height += 1;
                    rect.width += 1;
                    SirenixEditorGUI.DrawBorders(rect, 1, 1, 1, 1, SirenixGUIStyles.BorderColor);
                    return;
                }

                float heightOffset = 0;

                if (horizontal)
                {
                    var hRect = rect;
                    heightOffset += hRect.height;
                    hRect.xMax = to.Rect.xMax;
                    SirenixEditorGUI.DrawSolidRect(hRect, titleBgColor);
                    hRect.height += 1;
                    hRect.width += 1;
                    SirenixEditorGUI.DrawBorders(hRect, 1, 1, 1, 1, SirenixGUIStyles.BorderColor);
                }

                if (vertical)
                {
                    var hRect = rect;
                    hRect.yMin += heightOffset;
                    hRect.yMax = to.Rect.yMax;
                    SirenixEditorGUI.DrawSolidRect(hRect, titleBgColor);
                    hRect.height += 1;
                    hRect.width += 1;
                    SirenixEditorGUI.DrawBorders(hRect, 1, 1, 1, 1, SirenixGUIStyles.BorderColor);
                }
            };
        }

        private void ApplyListStyle(int xStart, int yStart, int xCount, int yCount, bool startBlack)
        {
            var xEnd = Mathf.Min(xStart + xCount, this.ColumnCount);
            var yEnd = Mathf.Min(yStart + yCount, this.RowCount);

            var fromCell = this.cells[xStart, yStart];
            var toCell = this.cells[xEnd, yEnd];

            // Horizontal Lines
            for (int y = yStart + (startBlack ? 0 : 1); y <= yEnd; y += 2)
            {
                if (this.cells[xStart, y] == null) continue;
                this.cells[xStart, y].GUIStyle += rect =>
                {
                    rect.xMax = toCell.Rect.xMax;
                    rect.x += 1;
                    rect.width -= 1;
                    SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.ListItemColorEven);
                };
            }

            // Horizontal Borders
            for (int y = yStart; y <= yEnd; y++)
            {
                if (this.cells[xStart, y] == null) continue;
                this.cells[xStart, y].GUIStyle += rect =>
                {
                    rect.xMax = toCell.Rect.xMax;
                    rect.height = 1;
                    SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor);
                };
            }

            // Vertical Borders
            for (int x = xStart; x <= xEnd; x++)
            {
                if (this.cells[x, yStart] == null) continue;
                this.cells[x, yStart].GUIStyle += rect =>
                {
                    rect.yMax = toCell.Rect.yMax;
                    rect.width = 1;
                    SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor);
                };
            }

            toCell.GUIStyle += rect =>
            {
                var borderRect = rect;
                borderRect.xMin = fromCell.Rect.xMin;
                borderRect.yMin = borderRect.yMax;
                borderRect.height = 1;
                SirenixEditorGUI.DrawSolidRect(borderRect, SirenixGUIStyles.BorderColor);

                borderRect = rect;
                borderRect.yMin = fromCell.Rect.yMin;
                borderRect.xMin = borderRect.xMax;
                borderRect.width = 1;
                SirenixEditorGUI.DrawSolidRect(borderRect, SirenixGUIStyles.BorderColor);
            };
        }

        /// <summary>
        /// Creates a table.
        /// </summary>
        public static GUITable Create(int colCount, int rowCount, Action<Rect, int, int> drawElement, string horizontalLabel, Action<Rect, int> columnLabels, string verticalLabel, Action<Rect, int> rowLabels, bool resizable = true)
        {
            int cols = colCount;
            int rows = rowCount;

            if (string.IsNullOrEmpty(verticalLabel) == false) cols++;
            if (rowLabels != null) cols++;
            if (string.IsNullOrEmpty(horizontalLabel) == false) rows++;
            if (columnLabels != null) rows++;

            GUITable table = new GUITable(cols, rows);

            cols = colCount;
            rows = rowCount;

            int colStart = 0;
            int rowStart = 0;

            if (string.IsNullOrEmpty(verticalLabel) == false)
            {
                colStart++;
                table[0, Mathf.Max(table.RowCount - 1, 2)] = new GUITableCell()
                {
                    OnGUI = rect =>
                    {
                        if (Event.current.type != EventType.Repaint) return;

                        var titleBgColor = SirenixGUIStyles.ListItemColorEven * 0.9f;
                        titleBgColor.a = 0.7f;
                        SirenixEditorGUI.DrawSolidRect(rect, titleBgColor);

                        rect.width += 1;
                        rect.height += 1;
                        SirenixEditorGUI.DrawBorders(rect, 1);
                        rect.width -= 1;
                        rect.height -= 1;

                        // Rotate -90 degreees:
                        // We can't simply rotate it in place, beacuse Unity will render it to a target before rotating it.
                        // So if that target is not partly visible on the screen horizontally, it will also So if that target is not partly visible on the screen horizontally, it will also
                        // appear partly visible when rendered vertically even tho every thing fits vertifally.
                        // So we need to move the rect in to the screen, so that there is room to render everthing horizontally,
                        // and that when it's rotated, everything is rendered. And bring it back into to correct position.
                        // There are propably better ways of doing it though.
                        var prev = GUI.matrix;
                        float prevCenter = rect.x + rect.width * 0.5f;
                        rect = rect.AlignCenter(rect.height, rect.height);
                        rect.x = 0;
                        float newCenter = rect.x + rect.width * 0.5f;
                        GUIUtility.RotateAroundPivot(-90, rect.center);
                        GUI.matrix = GUI.matrix * Matrix4x4.TRS(new Vector3(0, prevCenter - newCenter, 0), Quaternion.identity, Vector3.one);
                        GUI.Label(rect, verticalLabel, SirenixGUIStyles.LabelCentered);
                        GUI.matrix = prev;
                    },
                    SpanY = true,
                    Width = 22
                };
            }

            if (string.IsNullOrEmpty(horizontalLabel) == false)
            {
                rowStart++;

                table[Mathf.Min(2, table.ColumnCount - 1), 0] = new GUITableCell()
                {
                    OnGUI = rect =>
                    {
                        if (Event.current.type != EventType.Repaint) return;


                        var titleBgColor = SirenixGUIStyles.ListItemColorEven * 0.9f;
                        titleBgColor.a = 0.7f;
                        SirenixEditorGUI.DrawSolidRect(rect, titleBgColor);

                        GUI.Label(rect, horizontalLabel, SirenixGUIStyles.LabelCentered);
                        rect.width += 1;
                        rect.height += 1;
                        SirenixEditorGUI.DrawBorders(rect, 1);
                    },
                    SpanX = true
                };
            }

            if (rowLabels != null) colStart++;
            if (columnLabels != null) rowStart++;

            if (rowLabels != null)
            {
                for (int y = 0; y < rows; y++)
                {
                    int localY = y;
                    table[colStart - 1, rowStart + y] = new GUITableCell()
                    {
                        OnGUI = (rect) => rowLabels.Invoke(rect, localY),
                        Width = 25,
                    };
                }

                table.ApplyListStyle(colStart - 1, rowStart, 0, rows - 1, false);
                table.ApplyTitleStyle(
                    table[colStart - 1, rowStart],
                    table[colStart - 1, rowStart + rows - 1]);
            }

            if (columnLabels != null)
            {
                for (int x = 0; x < cols; x++)
                {
                    int localX = x;
                    table[colStart + x, rowStart - 1] = new GUITableCell()
                    {
                        OnGUI = (rect) => columnLabels.Invoke(rect, localX)
                    };
                }
                table.ApplyListStyle(colStart, rowStart - 1, cols - 1, 0, false);
                table.ApplyTitleStyle(
                    table[colStart, rowStart - 1],
                    table[colStart + cols - 1, rowStart - 1]);
            }

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int localX = x;
                    int localY = y;
                    table[x + colStart, y + rowStart] = new GUITableCell()
                    {
                        OnGUI = (rect) => { drawElement(rect, localX, localY); }
                    };
                }
            }

            // Make the top left corner transparent.
            // This prevents other cells from spanning into the top corner.
            for (int x = 0; x < colStart; x++)
            {
                for (int y = 0; y < rowStart; y++)
                {
                    table[x, y] = new GUITableCell();
                }
            }

            for (int i = 0; i < table.columnInfos.Length; i++)
            {
                table.columnInfos[i].Resizable = resizable;
            }

            table.ApplyListStyle(colStart, rowStart, cols - 1, rows - 1, true);
            return table;
        }

        /// <summary>
        /// Creates a table.
        /// </summary>
        public static GUITable Create(int rowCount, string title, params GUITableColumn[] columns)
        {
            bool hasColLabels = columns.Any(x => x.ColumnTitle != null);
            bool hasTitle = title != null;
            int extraLineCount = (hasTitle ? 1 : 0) + (hasColLabels ? 1 : 0);

            GUITable table = new GUITable(columns.Length, rowCount + extraLineCount);

            if (hasTitle)
            {
                var t = table[0, 0] = new GUITableCell()
                {
                    SpanX = true,
                    OnGUI = (rect) => GUI.Label(rect, title, SirenixGUIStyles.LabelCentered)
                };
                table.ApplyTitleStyle(t, t);
            }

            for (int x = 0; x < columns.Length; x++)
            {
                var column = columns[x];
                for (int y = 0; y < rowCount; y++)
                {
                    int localY = y;
                    table[x, y + extraLineCount] = new GUITableCell() { OnGUI = rect => column.OnGUI(rect.Padding(3), localY) };
                }
            }

            if (hasColLabels)
            {
                int colTitleStart = hasTitle ? 1 : 0;
                for (int x = 0; x < columns.Length; x++)
                {
                    var column = columns[x];
                    if (column.ColumnTitle != null)
                    {
                        table[x, colTitleStart] = new GUITableCell()
                        {
                            OnGUI = rect => GUI.Label(rect, column.ColumnTitle, SirenixGUIStyles.LabelCentered),
                            Width = column.Width,
                            SpanX = column.SpanColumnTitle
                        };
                    }
                    else
                    {
                        for (int y = 0; y < rowCount; y++)
                        {
                            var cell = table[x, y + extraLineCount];
                            if (cell != null)
                            {
                                cell.Width = column.Width;
                                cell.MinWidth = column.MinWidth;
                                break;
                            }
                        }
                    }
                    table.columnInfos[x].Resizable = column.Resizable;
                }
                table.ApplyListStyle(0, colTitleStart, columns.Length - 1, 0, false);
                table.ApplyTitleStyle(table[0, colTitleStart], table[columns.Length - 1, colTitleStart]);
            }

            if (rowCount > 0)
            {
                table.ApplyListStyle(0, extraLineCount, columns.Length - 1, rowCount - 1, true);
            }

            return table;
        }

        /// <summary>
        /// Creates a table.
        /// </summary>
        public static GUITable Create<T>(T[,] twoDimArray, Action<Rect, int, int> drawElement, string horizontalLabel, Action<Rect, int> columnLabels, string verticalLabel, Action<Rect, int> rowLabels)
        {
            return Create(twoDimArray.GetLength(0), twoDimArray.GetLength(1), drawElement, horizontalLabel, columnLabels, verticalLabel, rowLabels);
        }

        /// <summary>
        /// Creates a table.
        /// </summary>
        public static GUITable Create<T>(IList<T> list, string title, params GUITableColumn[] columns)
        {
            return Create(list.Count, title, columns);
        }
    }
}
#endif