#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUITableUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public class ResizableColumn : IResizableColumn
    {
        public float ColWidth;
        public float MinWidth;
        public bool PreserveWidth;
        public bool Resizable = true;

        public static ResizableColumn FixedColumn(float width)
        {
            return new ResizableColumn()
            {
                ColWidth = width,
                PreserveWidth = true,
                MinWidth = width,
                Resizable = false,
            };
        }

        public static ResizableColumn FlexibleColumn(float width = 0, float minWidth = 0)
        {
            return new ResizableColumn()
            {
                ColWidth = width,
                PreserveWidth = true,
                MinWidth = minWidth,
                Resizable = true,
            };
        }

        public static ResizableColumn DynamicColumn(float width = 0, float minWidth = 0)
        {
            return new ResizableColumn()
            {
                ColWidth = width,
                PreserveWidth = false,
                MinWidth = minWidth,
                Resizable = true,
            };
        }

        bool IResizableColumn.Resizable { get { return this.Resizable; } }
        float IResizableColumn.ColWidth { get { return this.ColWidth; } set { this.ColWidth = value; } }
        float IResizableColumn.MinWidth { get { return this.MinWidth; } }
        bool IResizableColumn.PreserveWidth { get { return this.PreserveWidth; } }
    }

    public interface IResizableColumn
    {
        /// <summary>
        /// Gets or sets the width of the col.
        /// </summary>
        float ColWidth { get; set; }

        /// <summary>
        /// Gets or sets the minimum width.
        /// </summary>
        float MinWidth { get; }

        /// <summary>
        /// Gets a value indicating whether the width should be preserved when the table itself gets resiszed.
        /// </summary>
        bool PreserveWidth { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IResizableColumn"/> is resizable.
        /// </summary>
        bool Resizable { get; }
    }

    public static class GUITableUtilities
    {
        public static void ResizeColumns<T>(Rect rect, IList<T> columns)
            where T : IResizableColumn
        {
            var colCount = columns.Count;

            if (colCount == 0)
            {
                return;
            }

            if (Event.current.type != EventType.Layout && rect.width > 1.001f) // When resizing windows, width and height becomes 0.
            {
                for (int i = 0; i < 2; i++)
                {
                    var currWidth = columns.Sum(x => x.ColWidth);
                    var delta = rect.width - currWidth;
                    DistributeDelta(columns, delta, 0, columns.Count, false);
                }
            }

            var tmpRect = rect;

            for (int i = 0; i < colCount; i++)
            {
                var col = columns[i];
                tmpRect.width = col.ColWidth;
                tmpRect.x += tmpRect.width;

                if (i != colCount - 1 && CanResize(columns, i))
                {
                    var slideRect = tmpRect;
                    slideRect.x -= 4;
                    slideRect.width = 8;
                    var delta = SirenixEditorGUI.SlideRect(slideRect).x;

                    if (delta < 0)
                    {
                        delta = Mathf.Abs(delta);
                        var nextCol = columns[i + 1];

                        nextCol.ColWidth += delta;
                        var overflow = DistributeDelta(columns, -delta, i, 0, true);
                        nextCol.ColWidth += overflow;
                    }
                    else if (delta > 0)
                    {
                        col.ColWidth += delta;
                        var overflow = DistributeDelta(columns, -delta, i + 1, colCount, true);
                        col.ColWidth += overflow;
                    }
                }
            }
        }

        private static bool CanResize<T>(IList<T> columns, int index) where T : IResizableColumn
        {
            if (index == columns.Count - 1)
            {
                return columns[index].Resizable;
            }

            return columns[index].Resizable && columns[index + 1].Resizable;
        }

        private static float DistributeDelta<T>(IList<T> columns, float delta, int from, int to, bool push)
            where T : IResizableColumn
        {
            var ltr = from < to;
            var incriment = ltr ? 1 : -1;

            float distribute = 0f;
            float pushBuffer = 0f;

            if (push)
            {
                pushBuffer = delta;
            }
            else
            {
                var adjustableColCount = 0;
                for (int i = from; ltr ? i < to : i >= to; i += incriment)
                {
                    if (!columns[i].PreserveWidth)
                    {
                        adjustableColCount++;
                    }
                }

                distribute = delta / adjustableColCount;
            }

            for (int i = from; ltr ? i < to : i >= to; i += incriment)
            {
                var col = columns[i];
                var currWidth = col.ColWidth;

                if (!col.PreserveWidth)
                {
                    currWidth += distribute;
                }
                else if (currWidth < col.MinWidth)
                {
                    currWidth = col.MinWidth;
                }

                currWidth += pushBuffer;

                if (currWidth < col.MinWidth)
                {
                    pushBuffer = currWidth - col.MinWidth;
                    currWidth = col.MinWidth;
                }
                else
                {
                    pushBuffer = 0;
                }

                col.ColWidth = currWidth;
            }

            return pushBuffer;
        }

        public static void DrawColumnHeaderSeperators<T>(Rect rect, IList<T> columns, Color color)
            where T : IResizableColumn
        {
            if (columns == null)
            {
                return;
            }

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (columns.Count == 0)
            {
                return;
            }

            var xMax = rect.xMax;
            rect.x = (int)rect.x;
            rect.width = 1;
            rect.x -= 1;

            for (int i = 0; i < columns.Count - 1; i++)
            {
                rect.x += columns[i].ColWidth;
                if (rect.x > xMax) return;
                EditorGUI.DrawRect(rect, color);
            }
        }
    }
}
#endif