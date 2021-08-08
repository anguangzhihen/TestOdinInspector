#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TableListAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    /// <summary>
    /// The TableList attirbute drawer.
    /// </summary>
    /// <seealso cref="TableListAttribute"/>
    public class TableListAttributeDrawer : OdinAttributeDrawer<TableListAttribute>
    {
        private IOrderedCollectionResolver resolver;
        private LocalPersistentContext<bool> isPagingExpanded;
        private LocalPersistentContext<Vector2> scrollPos;
        private LocalPersistentContext<int> currPage;
        private GUITableRowLayoutGroup table;
        private HashSet<string> seenColumnNames;
        private List<Column> columns;
        private ObjectPicker picker;
        private int colOffset;
        private GUIContent indexLabel;
        private bool isReadOnly;
        private int indexLabelWidth;
        private Rect columnHeaderRect;
        private GUIPagingHelper paging;
        private bool drawAsList;
        private bool isFirstFrame = true;

        /// <summary>
        /// Determines whether this instance [can draw attribute property] the specified property.
        /// </summary>
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ChildResolver is IOrderedCollectionResolver;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.drawAsList = false;
            this.isReadOnly = this.Attribute.IsReadOnly || !this.Property.ValueEntry.IsEditable;
            this.indexLabelWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent("100")).x + 15;
            this.indexLabel = new GUIContent();
            this.colOffset = 0;
            this.seenColumnNames = new HashSet<string>();
            this.table = new GUITableRowLayoutGroup();
            this.table.MinScrollViewHeight = this.Attribute.MinScrollViewHeight;
            this.table.MaxScrollViewHeight = this.Attribute.MaxScrollViewHeight;
            this.resolver = this.Property.ChildResolver as IOrderedCollectionResolver;
            this.scrollPos = this.GetPersistentValue("scrollPos", Vector2.zero);
            this.currPage = this.GetPersistentValue("currPage", 0);
            this.isPagingExpanded = this.GetPersistentValue("expanded", false);
            this.columns = new List<Column>(10);
            this.paging = new GUIPagingHelper();
            this.paging.NumberOfItemsPerPage = this.Attribute.NumberOfItemsPerPage > 0 ? this.Attribute.NumberOfItemsPerPage : GeneralDrawerConfig.Instance.NumberOfItemsPrPage;
            this.paging.IsExpanded = this.isPagingExpanded.Value;
            this.paging.IsEnabled = GeneralDrawerConfig.Instance.ShowPagingInTables || this.Attribute.ShowPaging;
            this.paging.CurrentPage = this.currPage.Value;
            this.Property.ValueEntry.OnChildValueChanged += OnChildValueChanged;

            if (this.Attribute.AlwaysExpanded)
            {
                this.Property.State.Expanded = true;
            }

            var p = this.Attribute.CellPadding;
            if (p > 0)
            {
                this.table.CellStyle = new GUIStyle() { padding = new RectOffset(p, p, p, p) };
            }

            GUIHelper.RequestRepaint(3);

            if (this.Attribute.ShowIndexLabels)
            {
                this.colOffset++;
                this.columns.Add(new Column(this.indexLabelWidth, true, false, null, ColumnType.Index));
            }

            if (!this.isReadOnly)
            {
                this.columns.Add(new Column(22, true, false, null, ColumnType.DeleteButton));
            }
        }

        /// <summary>
        /// Draws the property layout.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.drawAsList)
            {
                if (GUILayout.Button("Draw as table"))
                {
                    this.drawAsList = false;
                }

                this.CallNextDrawer(label);
                return;
            }


            this.picker = ObjectPicker.GetObjectPicker(this, this.resolver.ElementType);
            this.paging.Update(this.resolver.MaxCollectionLength);
            this.currPage.Value = this.paging.CurrentPage;
            this.isPagingExpanded.Value = this.paging.IsExpanded;

            var rect = SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            {
                if (!this.Attribute.HideToolbar)
                {
                    this.DrawToolbar(label);
                }

                if (this.Attribute.AlwaysExpanded)
                {
                    this.Property.State.Expanded = true;

                    this.DrawColumnHeaders();
                    this.DrawTable();
                }
                else
                {
                    if (SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded) && this.Property.Children.Count > 0)
                    {
                        this.DrawColumnHeaders();
                        this.DrawTable();
                    }
                    SirenixEditorGUI.EndFadeGroup();
                }
            }
            SirenixEditorGUI.EndIndentedVertical();
            if (Event.current.type == EventType.Repaint)
            {
                rect.yMin -= 1;
                rect.height -= 3;
                SirenixEditorGUI.DrawBorders(rect, 1);
            }

            this.DropZone(rect);
            this.HandleObjectPickerEvents();

            if (Event.current.type == EventType.Repaint)
            {
                this.isFirstFrame = false;
            }
        }

        private void OnChildValueChanged(int index)
        {
            var valueEntry = this.Property.Children[index].ValueEntry;
            if (valueEntry == null)
            {
                return;
            }

            if (!typeof(UnityEngine.ScriptableObject).IsAssignableFrom(valueEntry.TypeOfValue))
            {
                return;
            }

            for (int i = 0; i < valueEntry.ValueCount; i++)
            {
                var uObj = valueEntry.WeakValues[i] as UnityEngine.Object;
                if (uObj)
                {
                    EditorUtility.SetDirty(uObj);
                }
            }
        }

        private void DropZone(Rect rect)
        {
            if (this.isReadOnly) return;

            var eventType = Event.current.type;
            if ((eventType == EventType.DragUpdated || eventType == EventType.DragPerform) && rect.Contains(Event.current.mousePosition))
            {
                UnityEngine.Object[] objReferences = null;

                if (DragAndDrop.objectReferences.Any(n => n != null && this.resolver.ElementType.IsAssignableFrom(n.GetType())))
                {
                    objReferences = DragAndDrop.objectReferences.Where(x => x != null && this.resolver.ElementType.IsAssignableFrom(x.GetType())).Reverse().ToArray();
                }
                else if (this.resolver.ElementType.InheritsFrom(typeof(Component)))
                {
                    objReferences = DragAndDrop.objectReferences.OfType<GameObject>().Select(x => x.GetComponent(this.resolver.ElementType)).Where(x => x != null).Reverse().ToArray();
                }
                else if (this.resolver.ElementType.InheritsFrom(typeof(Sprite)) && DragAndDrop.objectReferences.Any(n => n is Texture2D && AssetDatabase.Contains(n)))
                {
                    objReferences = DragAndDrop.objectReferences.OfType<Texture2D>().Select(x =>
                    {
                        var path = AssetDatabase.GetAssetPath(x);
                        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }).Where(x => x != null).Reverse().ToArray();
                }

                bool acceptsDrag = objReferences != null && objReferences.Length > 0;

                if (acceptsDrag)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var obj in objReferences)
                        {
                            object[] values = new object[this.Property.ParentValues.Count];
                            for (int i = 0; i < values.Length; i++)
                            {
                                values[i] = obj;
                            }
                            this.resolver.QueueAdd(values);
                        }
                    }
                }
            }
        }

        private void AddColumns(int rowIndexFrom, int rowIndexTo)
        {
            if (Event.current.type != EventType.Layout)
            {
                return;
            }

            for (int y = rowIndexFrom; y < rowIndexTo; y++)
            {
                int skip = 0;
                var row = this.Property.Children[y];
                for (int x = 0; x < row.Children.Count; x++)
                {
                    var col = row.Children[x];
                    if (this.seenColumnNames.Add(col.Name))
                    {
                        var hide = GetColumnAttribute<HideInTablesAttribute>(col);
                        if (hide != null)
                        {
                            skip++;
                            continue;
                        }

                        var preserve = false;
                        var resizable = true;
                        var preferWide = true;
                        var width = this.Attribute.DefaultMinColumnWidth;

                        var colAttr = GetColumnAttribute<TableColumnWidthAttribute>(col);
                        if (colAttr != null)
                        {
                            preserve = !colAttr.Resizable;
                            resizable = colAttr.Resizable;
                            width = colAttr.Width;
                            preferWide = false;
                        }

                        Column newCol = new Column(width, preserve, resizable, col.Name, ColumnType.Property);
                        newCol.NiceName = col.NiceName;
                        newCol.NiceNameLabelWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent(newCol.NiceName)).x;
                        newCol.PreferWide = preferWide;

                        var index = x + this.colOffset - skip;
                        this.columns.Insert(Math.Min(index, this.columns.Count), newCol);

                        GUIHelper.RequestRepaint(3);
                    }
                }
            }
        }

        private void DrawToolbar(GUIContent label)
        {
            const int iconBtnSize = 23;
            var rect = GUILayoutUtility.GetRect(0, 22);
            var isRepaint = Event.current.type == EventType.Repaint;

            // Background
            if (isRepaint)
            {
                SirenixGUIStyles.ToolbarBackground.Draw(rect, GUIContent.none, 0);
            }

            // Add
            if (!this.isReadOnly)
            {
                var btnRect = rect.AlignRight(iconBtnSize);
                btnRect.width -= 1;
                rect.xMax = btnRect.xMin;
                if (GUI.Button(btnRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
                {
                    this.picker.ShowObjectPicker(
                        null,
                        this.Property.GetAttribute<AssetsOnlyAttribute>() == null && !typeof(ScriptableObject).IsAssignableFrom(this.resolver.ElementType),
                        rect,
                        !this.Property.ValueEntry.SerializationBackend.SupportsPolymorphism);
                }
                EditorIcons.Plus.Draw(btnRect, 16);
            }

            // Draw as list toggle
            if (!this.isReadOnly)
            {
                var btnRect = rect.AlignRight(iconBtnSize);
                rect.xMax = btnRect.xMin;
                if (GUI.Button(btnRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
                {
                    this.drawAsList = !this.drawAsList;
                }

                EditorIcons.HamburgerMenu.Draw(btnRect, 13);
            }

            // Paging
            this.paging.DrawToolbarPagingButtons(ref rect, this.Property.State.Expanded, true);

            // Label
            if (label == null)
            {
                label = GUIHelper.TempContent(""); // Use an empty label so the foldout doesn't disapear
            }

            //if (label != null)
            {
                var labelRect = rect;
                labelRect.x += 5;
                labelRect.y += 3;
                labelRect.height = 16;
                if (this.Property.Children.Count > 0)
                {
                    GUIHelper.PushHierarchyMode(false);
                    if (this.Attribute.AlwaysExpanded)
                    {
                        GUI.Label(labelRect, label);
                    }
                    else
                    {
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(labelRect, this.Property.State.Expanded, label);
                    }
                    GUIHelper.PushHierarchyMode(true);
                }
                else if (isRepaint)
                {
                    GUI.Label(labelRect, label);
                }
            }
        }

        private void DrawColumnHeaders()
        {
            if (this.Property.Children.Count == 0)
            {
                return;
            }

            this.columnHeaderRect = GUILayoutUtility.GetRect(0, 21);

            this.columnHeaderRect.height += 1;
            this.columnHeaderRect.y -= 1;

            if (Event.current.type == EventType.Repaint)
            {
                SirenixEditorGUI.DrawBorders(this.columnHeaderRect, 1);
                EditorGUI.DrawRect(this.columnHeaderRect, SirenixGUIStyles.ColumnTitleBg);
            }

            var offset = this.columnHeaderRect.width - this.table.ContentRect.width;
            this.columnHeaderRect.width -= offset;
            GUITableUtilities.ResizeColumns(this.columnHeaderRect, this.columns);

            if (Event.current.type == EventType.Repaint)
            {
                GUITableUtilities.DrawColumnHeaderSeperators(this.columnHeaderRect, this.columns, SirenixGUIStyles.BorderColor);

                var rect = this.columnHeaderRect;
                for (int i = 0; i < this.columns.Count; i++)
                {
                    var col = this.columns[i];
                    if (rect.x > this.columnHeaderRect.xMax)
                    {
                        break;
                    }

                    rect.width = col.ColWidth;
                    rect.xMax = Mathf.Min(this.columnHeaderRect.xMax, rect.xMax);

                    if (col.NiceName != null)
                    {
                        var lblRect = rect;
                        GUI.Label(lblRect, col.NiceName, SirenixGUIStyles.LabelCentered);
                    }

                    rect.x += col.ColWidth;
                }
            }
        }

        private void DrawTable()
        {
            GUIHelper.PushHierarchyMode(false);
            this.table.DrawScrollView = this.Attribute.DrawScrollView && (this.paging.IsExpanded || !this.paging.IsEnabled);
            this.table.ScrollPos = this.scrollPos.Value;
            this.table.BeginTable(this.paging.EndIndex - this.paging.StartIndex);
            {
                this.AddColumns(this.table.RowIndexFrom, this.table.RowIndexTo);
                this.DrawListItemBackGrounds();

                var currX = 0f;
                for (int i = 0; i < this.columns.Count; i++)
                {
                    var col = this.columns[i];

                    var colWidth = (int)col.ColWidth;
                    if (this.isFirstFrame && col.PreferWide)
                    {
                        // First frame is often rendered with minWidth becase we don't know the full width yet.
                        // resulting in very tall rows. This tweak will give a better first guess at how tall a row is.
                        colWidth = 200;
                    }

                    this.table.BeginColumn((int)currX, colWidth);
                    GUIHelper.PushLabelWidth(colWidth * 0.3f);
                    currX += col.ColWidth;
                    for (int j = this.table.RowIndexFrom; j < this.table.RowIndexTo; j++)
                    {
                        this.table.BeginCell(j);
                        DrawCell(col, j);
                        this.table.EndCell(j);
                    }
                    GUIHelper.PopLabelWidth();
                    this.table.EndColumn();
                }

                this.DrawRightClickContextMenuAreas();
            }
            this.table.EndTable();
            this.scrollPos.Value = this.table.ScrollPos;
            this.DrawColumnSeperators();

            GUIHelper.PopHierarchyMode();

            if (this.columns.Count > 0 && this.columns[0].ColumnType == ColumnType.Index)
            {
                // The indexLabelWidth changes: (1 - 10 - 100 - 1000)
                this.columns[0].ColWidth = this.indexLabelWidth;
                this.columns[0].MinWidth = this.indexLabelWidth;
            }
        }

        private void DrawColumnSeperators()
        {
            if (Event.current.type == EventType.Repaint)
            {
                var bcol = SirenixGUIStyles.BorderColor;
                bcol.a *= 0.4f;
                var r = this.table.OuterRect;
                GUITableUtilities.DrawColumnHeaderSeperators(r, this.columns, bcol);
            }
        }

        private void DrawListItemBackGrounds()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            for (int i = this.table.RowIndexFrom; i < this.table.RowIndexTo; i++)
            {
                var col = new Color();
                var rect = this.table.GetRowRect(i);
                col = i % 2 == 0 ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd;
                EditorGUI.DrawRect(rect, col);
            }
        }

        private void DrawRightClickContextMenuAreas()
        {
            for (int i = this.table.RowIndexFrom; i < this.table.RowIndexTo; i++)
            {
                var rect = this.table.GetRowRect(i);
                this.Property.Children[i].Update();
                PropertyContextMenuDrawer.AddRightClickArea(this.Property.Children[i], rect);
            }
        }

        private void DrawCell(Column col, int rowIndex)
        {
            rowIndex += this.paging.StartIndex;

            if (col.ColumnType == ColumnType.Index)
            {
                Rect rect = GUILayoutUtility.GetRect(0, 16);
                rect.xMin += 5;
                rect.width -= 2;
                if (Event.current.type == EventType.Repaint)
                {
                    indexLabel.text = rowIndex.ToString();
                    GUI.Label(rect, indexLabel, SirenixGUIStyles.Label);
                    var labelWidth = (int)SirenixGUIStyles.Label.CalcSize(indexLabel).x;
                    this.indexLabelWidth = Mathf.Max(this.indexLabelWidth, labelWidth + 15);
                }
            }
            else if (col.ColumnType == ColumnType.DeleteButton)
            {
                Rect rect = GUILayoutUtility.GetRect(20, 20).AlignCenter(16);
                if (SirenixEditorGUI.IconButton(rect, EditorIcons.X))
                {
                    this.resolver.QueueRemoveAt(rowIndex);
                }
            }
            else if (col.ColumnType == ColumnType.Property)
            {
                var cell = this.Property.Children[rowIndex].Children[col.Name];
                if (cell != null)
                {
                    cell.Draw(null);
                }
            }
            else
            {
                throw new NotImplementedException(col.ColumnType.ToString());
            }
        }

        private void HandleObjectPickerEvents()
        {
            if (this.picker.IsReadyToClaim && Event.current.type == EventType.Repaint)
            {
                var value = this.picker.ClaimObject();
                object[] values = new object[this.Property.Tree.WeakTargets.Count];
                values[0] = value;
                for (int j = 1; j < values.Length; j++)
                {
                    values[j] = SerializationUtility.CreateCopy(value);
                }
                this.resolver.QueueAdd(values);
            }
        }

        private IEnumerable<InspectorProperty> EnumerateGroupMembers(InspectorProperty groupProperty)
        {
            for (int i = 0; i < groupProperty.Children.Count; i++)
            {
                var info = groupProperty.Children[i].Info;
                if (info.PropertyType != PropertyType.Group)
                {
                    yield return groupProperty.Children[i];
                }
                else
                {
                    foreach (var item in EnumerateGroupMembers(groupProperty.Children[i]))
                    {
                        yield return item;
                    }
                }
            }
        }

        private T GetColumnAttribute<T>(InspectorProperty col)
            where T : Attribute
        {
            T colAttr;
            if (col.Info.PropertyType == PropertyType.Group)
            {
                colAttr = EnumerateGroupMembers(col)
                    .Select(c => c.GetAttribute<T>())
                    .FirstOrDefault(c => c != null);
            }
            else
            {
                colAttr = col.GetAttribute<T>();
            }

            return colAttr;
        }

        private enum ColumnType
        {
            Property,
            Index,
            DeleteButton,
        }

        private class Column : IResizableColumn
        {
            public string Name;
            public float ColWidth;
            public float MinWidth;
            public bool Preserve;
            public bool Resizable;
            public string NiceName;
            public int NiceNameLabelWidth;
            public ColumnType ColumnType;
            public bool PreferWide;

            public Column(int minWidth, bool preserveWidth, bool resizable, string name, ColumnType colType)
            {
                this.MinWidth = minWidth;
                this.ColWidth = minWidth;
                this.Preserve = preserveWidth;
                this.Name = name;
                this.ColumnType = colType;
                this.Resizable = resizable;
            }

            float IResizableColumn.ColWidth { get { return this.ColWidth; } set { this.ColWidth = value; } }
            float IResizableColumn.MinWidth { get { return this.MinWidth; } }
            bool IResizableColumn.PreserveWidth { get { return this.Preserve; } }
            bool IResizableColumn.Resizable { get { return this.Resizable; } }
        }
    }

    [ResolverPriority(2)]
    internal class ScriptableObjectTableListResolver<T> : BaseMemberPropertyResolver<T>
    where T : UnityEngine.ScriptableObject
    {
        private List<OdinPropertyProcessor> processors;

        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            return property.Parent != null && property.Parent.GetAttribute<TableListAttribute>() != null && property.Parent.ChildResolver is IOrderedCollectionResolver;
        }

        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            if (this.processors == null)
            {
                this.processors = OdinPropertyProcessorLocator.GetMemberProcessors(this.Property);
            }

            var includeSpeciallySerializedMembers = InspectorPropertyInfoUtility.TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(typeof(T));
            var infos = InspectorPropertyInfoUtility.CreateMemberProperties(this.Property, typeof(T), includeSpeciallySerializedMembers);

            for (int i = 0; i < this.processors.Count; i++)
            {
                ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = typeof(T);
                this.processors[i].ProcessMemberProperties(infos);
            }

            return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(this.Property, typeof(T), infos, includeSpeciallySerializedMembers);
        }
    }
}
#endif