#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TwoDimensionalArrayDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.StaticInitializeBeforeDrawing(typeof(Sirenix.OdinInspector.Editor.Drawers.TwoDimensionalEnumArrayDrawerLocator))]

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Base class for two-dimensional array drawers.
    /// </summary>
    public abstract class TwoDimensionalArrayDrawer<TArray, TElement> : OdinValueDrawer<TArray> where TArray : IList
    {
        private static readonly NamedValue[] DrawElementNamedArgs = new NamedValue[]
        {
            new NamedValue("rect", typeof(Rect)),
            new NamedValue("element", typeof(TElement)),
            new NamedValue("value", typeof(TElement)),
            new NamedValue("array", typeof(TArray)),
            new NamedValue("x", typeof(int)),
            new NamedValue("y", typeof(int)),
        };

#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

        protected internal class Context
        {
            public int RowCount;
            public int ColCount;
            public GUITable Table;
            public TElement[,] Value;
            public int DraggingRow = -1;
            public int DraggingCol = -1;
            public TableMatrixAttribute Attribute;
            public ValueResolver<TElement> DrawElement;
            public ValueResolver<string> HorizontalTitleGetter;
            public ValueResolver<string> VerticalTitleGetter;
            public Vector2 dragStartPos;
            public bool IsDraggingColumn;
            public int ColumnDragFrom;
            public int ColumnDragTo;
            public bool IsDraggingRow;
            public int RowDragFrom;
            public int RowDragTo;
            public string ExtraErrorMessage;
        }

#pragma warning restore 1591 // Missing XML comment for publicly visible type or member

        private Context context;

        /// <summary>
        /// <para>Override this method in order to define custom type constraints to specify whether or not a type should be drawn by the drawer.</para>
        /// <para>Note that Odin's <see cref="DrawerLocator" /> has full support for generic class constraints, so most often you can get away with not overriding CanDrawTypeFilter.</para>
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.IsArray && type.GetArrayRank() == 2 && type.GetElementType() == typeof(TElement);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected virtual TableMatrixAttribute GetDefaultTableMatrixAttributeSettings()
        {
            return new TableMatrixAttribute();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected TableMatrixAttribute TableMatrixAttribute { get; private set; }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            TElement[,] value = ValueEntry.Values[0] as TElement[,];
            bool rowLengthConflict = false;
            bool colLengthConflict = false;

            var attribute = ValueEntry.Property.GetAttribute<TableMatrixAttribute>()
                ?? this.GetDefaultTableMatrixAttributeSettings();

            var colIndex = attribute.Transpose ? 1 : 0;
            var rowIndex = 1 - colIndex;
            int colCount = value.GetLength(colIndex);
            int rowCount = value.GetLength(rowIndex);

            for (int i = 1; i < ValueEntry.Values.Count; i++)
            {
                var arr = ValueEntry.Values[i] as TElement[,];
                colLengthConflict = colLengthConflict || arr.GetLength(colIndex) != colCount;
                rowLengthConflict = rowLengthConflict || arr.GetLength(rowIndex) != rowCount;
                colCount = Mathf.Min(colCount, arr.GetLength(colIndex));
                rowCount = Mathf.Min(rowCount, arr.GetLength(rowIndex));
            }

            if (context == null || colCount != context.ColCount || rowCount != context.RowCount)
            {
                context = new Context();
                context.Value = value;
                context.ColCount = colCount;
                context.RowCount = rowCount;
                context.Attribute = attribute;

                if (context.Attribute.DrawElementMethod != null)
                {
                    context.DrawElement = ValueResolver.Get<TElement>(this.Property, context.Attribute.DrawElementMethod, DrawElementNamedArgs);
                }

                context.HorizontalTitleGetter = ValueResolver.GetForString(this.Property, context.Attribute.HorizontalTitle);
                context.VerticalTitleGetter = ValueResolver.GetForString(this.Property, context.Attribute.VerticalTitle);

                context.Table = GUITable.Create(
                    Mathf.Max(colCount, 1) + (colLengthConflict ? 1 : 0), Mathf.Max(rowCount, 1) + (rowLengthConflict ? 1 : 0),
                    (rect, x, y) => this.DrawElement(rect, ValueEntry, context, x, y),
                    context.HorizontalTitleGetter.GetValue(),
                    context.Attribute.HideColumnIndices ? (Action<Rect, int>)null : (rect, x) => this.DrawColumn(rect, ValueEntry, context, x),
                    context.VerticalTitleGetter.GetValue(),
                    context.Attribute.HideRowIndices ? (Action<Rect, int>)null : (rect, y) => this.DrawRows(rect, ValueEntry, context, y),
                    context.Attribute.ResizableColumns
                );

                context.Table.RespectIndentLevel = context.Attribute.RespectIndentLevel;

                if (context.Attribute.RowHeight != 0)
                {
                    for (int y = 0; y < context.RowCount; y++)
                    {
                        int _y = context.Table.RowCount - 1 - y;

                        for (int x = 0; x < context.Table.ColumnCount; x++)
                        {
                            var cell = context.Table[x, _y];
                            if (cell != null)
                            {
                                cell.Height = context.Attribute.RowHeight;
                            }
                        }
                    }
                }

                if (colLengthConflict)
                {
                    context.Table[context.Table.ColumnCount - 1, 1].Width = 15;
                }

                if (colLengthConflict)
                {
                    for (int x = 0; x < context.Table.ColumnCount; x++)
                    {
                        context.Table[x, context.Table.RowCount - 1].Height = 15;
                    }
                }
            }

            if (context.Attribute.SquareCells)
            {
                SetSquareRowHeights(context);
            }

            this.TableMatrixAttribute = context.Attribute;

            context.Value = value;
            var prev = EditorGUI.showMixedValue;

            this.OnBeforeDrawTable(ValueEntry, context, label);

            ValueResolver.DrawErrors(context.DrawElement, context.HorizontalTitleGetter, context.VerticalTitleGetter);

            if (context.ExtraErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.ExtraErrorMessage);
            }

            if (context.DrawElement == null || !context.DrawElement.HasError)
            {
                try
                {
                    context.Table.DrawTable();
                    GUILayout.Space(3);
                }
                catch (ExitGUIException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    if (ex.IsExitGUIException())
                    {
                        throw ex.AsExitGUIException();
                    }

                    Debug.LogException(ex);
                }
            }

            EditorGUI.showMixedValue = prev;
        }

        private static void SetSquareRowHeights(Context context)
        {
            if (context.ColCount > 0 && context.RowCount > 0)
            {
                var lastCell = context.Table[context.ColCount - 1, context.RowCount - 1];
                if (lastCell != null && Mathf.Abs(lastCell.Rect.height - lastCell.Rect.width) > 0)
                {
                    for (int y = 0; y < context.RowCount; y++)
                    {
                        int _y = context.Table.RowCount - 1 - y;

                        for (int x = 0; x < context.Table.ColumnCount; x++)
                        {
                            var cell = context.Table[x, _y];
                            if (cell != null)
                            {
                                cell.Height = lastCell.Rect.width;
                            }
                        }
                    }
                    context.Table.ReCalculateSizes();
                    GUIHelper.RequestRepaint();
                }
            }
        }

        /// <summary>
        /// This method gets called from DrawPropertyLayout right before the table and error message is drawn.
        /// </summary>
        protected internal virtual void OnBeforeDrawTable(IPropertyValueEntry<TArray> entry, Context value, GUIContent label)
        {
        }

        private void DrawRows(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int rowIndex)
        {
            if (rowIndex < context.RowCount)
            {
                GUI.Label(rect, rowIndex.ToString(), SirenixGUIStyles.LabelCentered);

                // Handle Row dragging.
                if (!context.Attribute.IsReadOnly)
                {
                    var id = GUIUtility.GetControlID(FocusType.Passive);
                    if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        GUIHelper.RemoveFocusControl();
                        GUIUtility.hotControl = id;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        Event.current.Use();
                        context.RowDragFrom = rowIndex;
                        context.RowDragTo = rowIndex;
                        context.dragStartPos = Event.current.mousePosition;
                    }
                    else if (GUIUtility.hotControl == id)
                    {
                        if ((context.dragStartPos - Event.current.mousePosition).sqrMagnitude > 5 * 5)
                        {
                            context.IsDraggingRow = true;
                        }
                        if (Event.current.type == EventType.MouseDrag)
                        {
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            GUIUtility.hotControl = 0;
                            EditorGUIUtility.SetWantsMouseJumping(0);
                            Event.current.Use();
                            context.IsDraggingRow = false;


                            if (context.Attribute.Transpose)
                            {
                                ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.MoveColumn(arr, context.RowDragFrom, context.RowDragTo));
                            }
                            else
                            {
                                ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.MoveRow(arr, context.RowDragFrom, context.RowDragTo));
                            }
                        }
                    }

                    if (context.IsDraggingRow && Event.current.type == EventType.Repaint)
                    {
                        float mouseY = Event.current.mousePosition.y;
                        if (mouseY > rect.y - 1 && mouseY < rect.y + rect.height + 1)
                        {
                            Rect arrowRect;
                            if (mouseY > rect.y + rect.height * 0.5f)
                            {
                                arrowRect = rect.AlignBottom(16);
                                arrowRect.width = 16;
                                arrowRect.y += 8;
                                arrowRect.x -= 13;
                                context.RowDragTo = rowIndex;
                            }
                            else
                            {
                                arrowRect = rect.AlignTop(16);
                                arrowRect.width = 16;
                                arrowRect.y -= 8;
                                arrowRect.x -= 13;
                                context.RowDragTo = rowIndex - 1;
                            }
                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                                GUI.DrawTexture(arrowRect, EditorIcons.ArrowRight.Active);
                                //GL.sRGBWrite = false;

                                var lineRect = arrowRect;
                                lineRect.y = lineRect.center.y - 2 + 1;
                                lineRect.height = 3;
                                lineRect.x += 14;
                                lineRect.xMax = context.Table.TableRect.xMax;
                                EditorGUI.DrawRect(lineRect, new Color(0, 0, 0, 0.6f));
                            });
                        }

                        if (rowIndex == context.RowCount - 1)
                        {
                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                var cell = context.Table[context.Table.ColumnCount - 1, context.Table.RowCount - context.RowCount + context.RowDragFrom];
                                var rowRect = cell.Rect;
                                rowRect.xMin = rect.xMin;
                                SirenixEditorGUI.DrawSolidRect(rowRect, new Color(0, 0, 0, 0.2f));
                            });
                        }
                    }
                }
            }
            else
            {
                GUI.Label(rect, "...", EditorStyles.centeredGreyMiniLabel);
            }

            if (!context.Attribute.IsReadOnly && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Insert 1 above"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.InsertOneColumnLeft(arr, rowIndex) : MultiDimArrayUtilities.InsertOneRowAbove(arr, rowIndex)));
                menu.AddItem(new GUIContent("Insert 1 below"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.InsertOneColumnRight(arr, rowIndex) : MultiDimArrayUtilities.InsertOneRowBelow(arr, rowIndex)));
                menu.AddItem(new GUIContent("Duplicate"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.DuplicateColumn(arr, rowIndex) : MultiDimArrayUtilities.DuplicateRow(arr, rowIndex)));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.DeleteColumn(arr, rowIndex) : MultiDimArrayUtilities.DeleteRow(arr, rowIndex)));
                menu.ShowAsContext();
            }
        }

        private void DrawColumn(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int columnIndex)
        {
            if (columnIndex < context.ColCount)
            {
                GUI.Label(rect, columnIndex.ToString(), SirenixGUIStyles.LabelCentered);

                // Handle Column dragging.
                if (!context.Attribute.IsReadOnly)
                {
                    var id = GUIUtility.GetControlID(FocusType.Passive);
                    if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        GUIHelper.RemoveFocusControl();
                        GUIUtility.hotControl = id;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        Event.current.Use();
                        context.ColumnDragFrom = columnIndex;
                        context.ColumnDragTo = columnIndex;
                        context.dragStartPos = Event.current.mousePosition;
                    }
                    else if (GUIUtility.hotControl == id)
                    {
                        if ((context.dragStartPos - Event.current.mousePosition).sqrMagnitude > 5 * 5)
                        {
                            context.IsDraggingColumn = true;
                        }
                        if (Event.current.type == EventType.MouseDrag)
                        {
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            GUIUtility.hotControl = 0;
                            EditorGUIUtility.SetWantsMouseJumping(0);
                            Event.current.Use();
                            context.IsDraggingColumn = false;

                            if (context.Attribute.Transpose)
                            {
                                ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.MoveRow(arr, context.ColumnDragFrom, context.ColumnDragTo));
                            }
                            else
                            {
                                ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.MoveColumn(arr, context.ColumnDragFrom, context.ColumnDragTo));
                            }
                        }
                    }

                    if (context.IsDraggingColumn && Event.current.type == EventType.Repaint)
                    {
                        float mouseX = Event.current.mousePosition.x;
                        if (mouseX > rect.x - 1 && mouseX < rect.x + rect.width + 1)
                        {
                            Rect arrowRect;
                            if (mouseX > rect.x + rect.width * 0.5f)
                            {
                                arrowRect = rect.AlignRight(16);
                                arrowRect.height = 16;
                                arrowRect.y -= 13;
                                arrowRect.x += 8;
                                context.ColumnDragTo = columnIndex;
                            }
                            else
                            {
                                arrowRect = rect.AlignLeft(16);
                                arrowRect.height = 16;
                                arrowRect.y -= 13;
                                arrowRect.x -= 8;
                                context.ColumnDragTo = columnIndex - 1;
                            }

                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                                GUI.DrawTexture(arrowRect, EditorIcons.ArrowDown.Active);
                                //GL.sRGBWrite = false;

                                var lineRect = arrowRect;
                                lineRect.x = lineRect.center.x - 2 + 1;
                                lineRect.width = 3;
                                lineRect.y += 14;
                                lineRect.yMax = context.Table.TableRect.yMax;
                                EditorGUI.DrawRect(lineRect, new Color(0, 0, 0, 0.6f));
                            });
                        }

                        if (columnIndex == context.ColCount - 1)
                        {
                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                var cell = context.Table[context.Table.ColumnCount - context.ColCount + context.ColumnDragFrom, context.Table.RowCount - 1];
                                var rowRect = cell.Rect;
                                rowRect.yMin = rect.yMin;
                                SirenixEditorGUI.DrawSolidRect(rowRect, new Color(0, 0, 0, 0.2f));
                            });
                        }
                    }
                }
            }
            else
            {
                GUI.Label(rect, "-", EditorStyles.centeredGreyMiniLabel);
            }

            if (!context.Attribute.IsReadOnly && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Insert 1 left"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.InsertOneRowAbove(arr, columnIndex) : MultiDimArrayUtilities.InsertOneColumnLeft(arr, columnIndex)));
                menu.AddItem(new GUIContent("Insert 1 right"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.InsertOneRowBelow(arr, columnIndex) : MultiDimArrayUtilities.InsertOneColumnRight(arr, columnIndex)));
                menu.AddItem(new GUIContent("Duplicate"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.DuplicateRow(arr, columnIndex) : MultiDimArrayUtilities.DuplicateColumn(arr, columnIndex)));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, () => ApplyArrayModifications(entry, arr => this.TableMatrixAttribute.Transpose ? MultiDimArrayUtilities.DeleteRow(arr, columnIndex) : MultiDimArrayUtilities.DeleteColumn(arr, columnIndex)));
                menu.ShowAsContext();
            }
        }

        private void ApplyArrayModifications(IPropertyValueEntry<TArray> entry, Func<TElement[,], TElement[,]> modification)
        {
            for (int i = 0; i < entry.Values.Count; i++)
            {
                int localI = i;
                var newArr = modification((entry.Values[localI] as TElement[,]));
                entry.Property.Tree.DelayActionUntilRepaint(() =>
                {
                    entry.Values[localI] = (TArray)(object)newArr;
                });
            }
        }

        private void DrawElement(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int x, int y)
        {
            if (x < context.ColCount && y < context.RowCount)
            {
                var row = context.Attribute.Transpose ? x : y;
                var col = context.Attribute.Transpose ? y : x;

                bool showMixedValue = false;
                if (entry.Values.Count != 1)
                {
                    for (int i = 1; i < entry.Values.Count; i++)
                    {
                        var a = (entry.Values[i] as TElement[,])[col, row];
                        var b = (entry.Values[i - 1] as TElement[,])[col, row];

                        if (!CompareElement(a, b))
                        {
                            showMixedValue = true;
                            break;
                        }
                    }
                }

                EditorGUI.showMixedValue = showMixedValue;
                EditorGUI.BeginChangeCheck();
                var prevValue = context.Value[col, row];
                TElement value;

                if (context.DrawElement != null)
                {
                    context.DrawElement.Context.NamedValues.Set("rect", rect);
                    context.DrawElement.Context.NamedValues.Set("element", prevValue);
                    context.DrawElement.Context.NamedValues.Set("value", prevValue);
                    context.DrawElement.Context.NamedValues.Set("array", context.Value);
                    context.DrawElement.Context.NamedValues.Set("x", x);
                    context.DrawElement.Context.NamedValues.Set("y", y);

                    value = context.DrawElement.GetValue();
                }
                //else if (context.DrawElementXY != null)
                //{
                //    value = context.DrawElementXY(rect, (entry.Values[0] as TElement[,]), x, y);
                //}
                else
                {
                    value = DrawElement(rect, prevValue);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < entry.Values.Count; i++)
                    {
                        (entry.Values[i] as TElement[,])[col, row] = value;
                    }

                    entry.Values.ForceMarkDirty();
                }
            }
        }

        /// <summary>
        /// Compares the element.
        /// </summary>
        protected virtual bool CompareElement(TElement a, TElement b)
        {
            return EqualityComparer<TElement>.Default.Equals(a, b);
        }

        /// <summary>
        /// Draws a table cell element.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="value">The input value.</param>
        /// <returns>The output value.</returns>
        protected abstract TElement DrawElement(Rect rect, TElement value);
    }

    internal static class TwoDimensionalEnumArrayDrawerLocator
    {
        static TwoDimensionalEnumArrayDrawerLocator()
        {
            RegisterDrawer();
        }

        private static void RegisterDrawer()
        {
            HashSet<Type> canMatch = new HashSet<Type>()
            {
                typeof(TwoDimensionalEnumArrayDrawer<,>),
                typeof(TwoDimensionalUnityObjectArrayDrawer<,>),
                typeof(TwoDimensionalGenericArrayDrawer<,>),
            };

            DrawerUtilities.SearchIndex.MatchRules.Add(new TypeSearch.TypeMatchRule(
                "Two Dimensional Array Custom Matcher",
                (info, targets) =>
                {
                    if (targets.Length != 1) return null;
                    var target = targets[0];
                    if (!target.IsArray || target.GetArrayRank() != 2) return null;
                    if (!canMatch.Contains(info.MatchType)) return null;

                    var elementType = target.GetElementType();

                    if (elementType.IsEnum && info.MatchType == typeof(TwoDimensionalEnumArrayDrawer<,>))
                    {
                        return typeof(TwoDimensionalEnumArrayDrawer<,>)
                            .MakeGenericType(target, elementType);
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(elementType) && info.MatchType == typeof(TwoDimensionalUnityObjectArrayDrawer<,>))
                    {
                        return typeof(TwoDimensionalUnityObjectArrayDrawer<,>)
                            .MakeGenericType(target, elementType);
                    }
                    else if (info.MatchType == typeof(TwoDimensionalGenericArrayDrawer<,>))
                    {
                        return typeof(TwoDimensionalGenericArrayDrawer<,>)
                            .MakeGenericType(target, elementType);
                    }

                    return null;
                })
            );
        }
    }

    [DrawerPriority(0, 0, 0.9)]
    internal class TwoDimensionalGenericArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement>
        where TArray : IList
    {
        private static string drawElementErrorMessage =
            "Odin doesn't know how to draw a table matrix for this particular type. Make a custom DrawElementMethod via the TableMatrix attribute like so:" + "\n" +
            "" + "\n" +
            "[TableMatrix(DrawElementMethod = \"DrawElement\")]" + "\n" +
            "public " + typeof(TElement).GetNiceName() + "[,] myTable" + "\n" +
            "" + "\n" +
            "static " + typeof(TElement).GetNiceName() + " DrawElement(Rect rect, " + typeof(TElement).GetNiceName() + " value)" + "\n" +
            "{" + "\n" +
            "   // Draw and modify the value in the rect provided using classes such as:" + "\n" +
            "   // GUI, EditorGUI, SirenixEditorFields and SirenixEditorGUI." + "\n" +
            "   return newValue;" + "\n" +
            "}";

        protected internal override void OnBeforeDrawTable(IPropertyValueEntry<TArray> entry, Context context, GUIContent label)
        {
            if (context.DrawElement == null && context.ExtraErrorMessage == null)
            {
                context.ExtraErrorMessage = drawElementErrorMessage;
            }
        }

        /// <summary>
        /// Draws the element.
        /// </summary>
        protected override TElement DrawElement(Rect rect, TElement value)
        {
            return value;
        }
    }

    internal class TwoDimensionalUnityObjectArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement>
        where TArray : IList
        where TElement : UnityEngine.Object
    {
        protected override TElement DrawElement(Rect rect, TElement value)
        {
            bool ediable = !this.TableMatrixAttribute.IsReadOnly;
            value = SirenixEditorFields.PreviewObjectField(rect, value, false, ediable, ediable);
            return value;
        }

        protected override bool CompareElement(TElement a, TElement b)
        {
            return a == b;
        }
    }

    internal class TwoDimensionalEnumArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement>
        where TArray : IList
    {
        protected override TElement DrawElement(Rect rect, TElement value)
        {
            return (TElement)(object)SirenixEditorFields.EnumDropdown(rect.Padding(4), null, (Enum)(object)value, null);
        }
    }

    internal class TwoDimensionalAnimationCurveArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, AnimationCurve> where TArray : IList
    {
        protected override AnimationCurve DrawElement(Rect rect, AnimationCurve value)
        {
            if (value == null)
            {
                if (GUI.Button(rect.Padding(2), "Null - Create Animation Curve", EditorStyles.objectField))
                {
                    value = new AnimationCurve();
                }
                return value;
            }

            return EditorGUI.CurveField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalGuidArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Guid> where TArray : IList
    {
        protected override Guid DrawElement(Rect rect, Guid value)
        {
            return SirenixEditorFields.GuidField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalLayerMaskArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, LayerMask> where TArray : IList
    {
        protected override LayerMask DrawElement(Rect rect, LayerMask value)
        {
            return SirenixEditorFields.LayerMaskField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalStringArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, string> where TArray : IList
    {
        private static GUIStyle style = null;

        protected override string DrawElement(Rect rect, string value)
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.textField);
                style.alignment = TextAnchor.MiddleCenter;
            }

            return EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width + 1, rect.height + 1), value, style);
        }
    }

    internal class TwoDimensionalBoolArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, bool> where TArray : IList
    {
        protected override bool DrawElement(Rect rect, bool value)
        {
            if (Event.current.type == EventType.Repaint)
            {
                return EditorGUI.Toggle(rect.AlignCenter(16, 16), value);
            }
            else
            {
                return EditorGUI.Toggle(rect, value);
            }
        }
    }

    internal class TwoDimensionalIntArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, int> where TArray : IList
    {
        protected override int DrawElement(Rect rect, int value)
        {
            return SirenixEditorFields.IntField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalLongArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, long> where TArray : IList
    {
        protected override long DrawElement(Rect rect, long value)
        {
            return SirenixEditorFields.LongField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalFloatArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, float> where TArray : IList
    {
        protected override float DrawElement(Rect rect, float value)
        {
            return SirenixEditorFields.FloatField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalDoubleArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, double> where TArray : IList
    {
        protected override double DrawElement(Rect rect, double value)
        {
            return SirenixEditorFields.DoubleField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalDecimalArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, decimal> where TArray : IList
    {
        protected override decimal DrawElement(Rect rect, decimal value)
        {
            return SirenixEditorFields.DecimalField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalVector2ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector2> where TArray : IList
    {
        protected override Vector2 DrawElement(Rect rect, Vector2 value)
        {
            return SirenixEditorFields.Vector2Field(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalVector3ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector3> where TArray : IList
    {
        protected override Vector3 DrawElement(Rect rect, Vector3 value)
        {
            return SirenixEditorFields.Vector3Field(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalVector4ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector4> where TArray : IList
    {
        protected override Vector4 DrawElement(Rect rect, Vector4 value)
        {
            return SirenixEditorFields.Vector4Field(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalColorArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Color> where TArray : IList
    {
        protected override Color DrawElement(Rect rect, Color value)
        {
            return SirenixEditorFields.ColorField(rect.Padding(2), value);
        }
    }

    internal class TwoDimensionalQuaternionArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Quaternion> where TArray : IList
    {
        protected override Quaternion DrawElement(Rect rect, Quaternion value)
        {
            return SirenixEditorFields.RotationField(rect.Padding(2), value, QuaternionDrawMode.Eulers);
        }
    }
}
#endif