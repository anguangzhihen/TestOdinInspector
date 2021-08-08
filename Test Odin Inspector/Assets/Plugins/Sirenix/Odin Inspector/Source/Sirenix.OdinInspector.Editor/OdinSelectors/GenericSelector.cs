#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GenericSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using System.Linq;
    using Sirenix.Utilities;

    /// <summary>
    /// In simple one-off use cases, making a custom OdinSelector might not be needed, as the GenericSelecor 
    /// can be populated with anything and customized a great deal.
    /// </summary>
    /// <example>
    /// <code>
    /// SomeType someValue;
    /// 
    /// [OnInspectorGUI]
    /// void OnInspectorGUI()
    /// {
    ///     if (GUILayout.Button("Open Generic Selector Popup"))
    ///     {
    ///         List&lt;SomeType&gt; source = ...;
    ///         GenericSelector&lt;SomeType&gt; selector = new GenericSelector&lt;SomeType&gt;("Title", false, x => x.Path, source);
    ///         selector.SetSelection(this.someValue);
    ///         selector.SelectionTree.Config.DrawSearchToolbar = false;
    ///         selector.SelectionTree.DefaultMenuStyle.Height = 22;
    ///         selector.SelectionConfirmed += selection =&gt; this.someValue = selection.FirstOrDefault()
    ///         var window = selector.ShowInPopup();
    ///         window.OnEndGUI += () =&gt; { EditorGUILayout.HelpBox("A quick way of injecting custom GUI to the editor window popup instance.", MessageType.Info); };
    ///         window.OnClose += selector.SelectionTree.Selection.ConfirmSelection; // Confirm selection when window clses.
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinSelector{T}"/>
    /// <seealso cref="EnumSelector{T}"/>
    /// <seealso cref="TypeSelector"/>
    /// <seealso cref="OdinMenuTree"/>
    /// <seealso cref="OdinEditorWindow"/>
    public class GenericSelector<T> : OdinSelector<T>
    {
        private int checkboxUpdateId = 0;
        private readonly string title;
        private readonly IEnumerable<GenericSelectorItem<T>> genericSelectorCollection;
        private readonly bool supportsMultiSelect;
        private readonly IEnumerable<T> collection;
        private Func<T, string> getMenuItemName;
        private HashSet<T> selection = new HashSet<T>();
        private bool requestCheckboxUpdate;

        /// <summary>
        /// Gets or sets a value indicating whether [flattened tree].
        /// </summary>
        public bool FlattenedTree { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [checkbox toggle].
        /// </summary>
        public bool CheckboxToggle { get; set; }

        public GenericSelector(string title, bool supportsMultiSelect, IEnumerable<GenericSelectorItem<T>> collection)
        {
            this.supportsMultiSelect = supportsMultiSelect;
            this.title = title;
            this.genericSelectorCollection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, IEnumerable<T> collection, bool supportsMultiSelect, Func<T, string> getMenuItemName = null)
        {
            this.title = title;
            this.supportsMultiSelect = supportsMultiSelect;
            this.getMenuItemName = getMenuItemName;
            this.collection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, Func<T, string> getMenuItemName, params T[] collection)
        {
            this.title = title;
            this.supportsMultiSelect = supportsMultiSelect;
            this.getMenuItemName = getMenuItemName ?? (x => x.ToString());
            this.collection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, params T[] collection)
            : this(title, supportsMultiSelect, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, params T[] collection)
            : this(title, false, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(params T[] collection)
            : this(null, false, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, Func<T, string> getMenuItemName, IEnumerable<T> collection)
        {
            this.title = title;
            this.supportsMultiSelect = supportsMultiSelect;
            this.getMenuItemName = getMenuItemName ?? (x => x.ToString());
            this.collection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, IEnumerable<T> collection)
            : this(title, supportsMultiSelect, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, IEnumerable<T> collection)
            : this(title, false, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(IEnumerable<T> collection)
            : this(null, false, null, collection)
        {
        }

        /// <summary>
        /// Gets the title. No title will be drawn if the string is null or empty.
        /// </summary>
        public override string Title { get { return this.title; } }

        /// <summary>
        /// Draws the selection tree. This gets drawn using the OnInspectorGUI attribute.
        /// </summary>
        protected override void DrawSelectionTree()
        {
            if (this.CheckboxToggle)
            {
                // Toggle selection when pressing space.
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space && this.SelectionTree == OdinMenuTree.ActiveMenuTree)
                {
                    var sel = this.SelectionTree.Selection
                        .SelectMany(x => x.GetChildMenuItemsRecursive(true))
                        .Select(x => x.Value)
                        .OfType<T>();

                    if (sel.Any())
                    {
                        var remove = this.selection.Contains(sel.FirstOrDefault());
                        sel.ForEach(x =>
                        {
                            if (remove) { this.selection.Remove(x); }
                            else { this.selection.Add(x); }
                        });
                    }

                    Event.current.Use();
                    this.checkboxUpdateId++;
                }
            }

            if (this.requestCheckboxUpdate && Event.current.type == EventType.Repaint)
            {
                this.requestCheckboxUpdate = false;
                this.checkboxUpdateId++;
            }


            base.DrawSelectionTree();
        }

        private void DrawCheckboxMenuItems(OdinMenuItem xx)
        {
            var allChilds = xx.GetChildMenuItemsRecursive(true)
                .Select(x => x.Value)
                .OfType<T>().ToList();

            bool isEmpty = allChilds.Count == 0;
            bool isSelected = false;
            bool isMixed = false;
            int prevUpdateId = -1;

            Action validate = () =>
            {
                if (isEmpty) return;
                isSelected = this.selection.Contains(allChilds[0]);
                //var a = this.selection.ToList();
                var b = allChilds[0];
                isMixed = false;
                for (int i = 1; i < allChilds.Count; i++)
                {
                    var sel = this.selection.Contains(allChilds[i]);
                    if (sel != isSelected)
                    {
                        isMixed = true;
                        break;
                    }
                }
            };

            xx.OnDrawItem += (menuItem) =>
            {
                if (isEmpty) return;

                var checkboxRect = xx.LabelRect;
                checkboxRect = checkboxRect.AlignMiddle(18).AlignLeft(16);
                checkboxRect.x -= 16;
                if (xx.IconGetter()) checkboxRect.x -= 16;

                if (Event.current.type != EventType.Repaint && xx.ChildMenuItems.Count == 0)
                {
                    checkboxRect = xx.Rect;
                }
                //else if (menuItem.ChildMenuItems.Count > 0)
                //{
                //    rect = xx.Rect;
                //}

                if (prevUpdateId != this.checkboxUpdateId)
                {
                    validate();
                    prevUpdateId = this.checkboxUpdateId;
                }

                EditorGUI.showMixedValue = isMixed;
                EditorGUI.BeginChangeCheck();
                var newSelected = EditorGUI.Toggle(checkboxRect, isSelected);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < allChilds.Count; i++)
                    {
                        if (newSelected)
                        {
                            this.selection.Add(allChilds[i]);
                        }
                        else
                        {
                            this.selection.Remove(allChilds[i]);
                        }
                    }
                    xx.Select();
                    validate();
                    this.requestCheckboxUpdate = true;
                    GUIHelper.RemoveFocusControl();
                }
                EditorGUI.showMixedValue = false;
            };
        }

        public override void SetSelection(T selected)
        {
            if (this.CheckboxToggle)
            {
                this.selection.Clear();
                this.selection.Add(selected);
            }
            else
            {
                base.SetSelection(selected);
            }
        }

        public override void SetSelection(IEnumerable<T> selection)
        {
            if (this.CheckboxToggle)
            {
                this.selection.Clear();
                foreach (var item in selection)
                {
                    this.selection.Add(item);
                }
            }
            else
            {
                base.SetSelection(selection);
            }
        }

        public override bool IsValidSelection(IEnumerable<T> collection)
        {
            if (this.SelectionTree.Selection.SupportsMultiSelect)
            {
                return true;
            }
            else
            {
                return base.IsValidSelection(collection);
            }
        }

        public override IEnumerable<T> GetCurrentSelection()
        {
            if (this.CheckboxToggle)
            {
                return this.selection;
            }
            else
            {
                return base.GetCurrentSelection();
            }
        }

        /// <summary>
        /// Builds the selection tree.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
            tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
            this.getMenuItemName = this.getMenuItemName ?? (x => x == null ? "" : x.ToString());

            if (this.FlattenedTree)
            {
                if (this.genericSelectorCollection != null)
                {
                    foreach (var item in this.genericSelectorCollection)
                    {
                        tree.MenuItems.Add(new OdinMenuItem(tree, item.GetNiceName(), item.Value));
                    }
                }
                else
                {
                    foreach (var item in this.collection)
                    {
                        tree.MenuItems.Add(new OdinMenuItem(tree, this.getMenuItemName(item), item));
                    }
                }
            }
            else
            {
                if (this.genericSelectorCollection != null)
                {
                    foreach (var item in this.genericSelectorCollection)
                    {
                        tree.AddObjectAtPath(item.GetNiceName(), item.Value);
                    }
                }
                else
                {
                    tree.AddRange(this.collection, this.getMenuItemName);
                }
            }

            if (this.CheckboxToggle)
            {
                tree.EnumerateTree().ForEach(DrawCheckboxMenuItems);
                tree.DefaultMenuStyle.TrianglePadding -= 17;
                tree.DefaultMenuStyle.Offset += 18;
                tree.DefaultMenuStyle.SelectedColorDarkSkin = new Color(1, 1, 1, 0.05f);
                tree.DefaultMenuStyle.SelectedColorLightSkin = new Color(1, 1, 1, 0.05f);
                tree.DefaultMenuStyle.SelectedLabelStyle = tree.DefaultMenuStyle.DefaultLabelStyle;
                tree.Config.ConfirmSelectionOnDoubleClick = false;
            }
        }
    }
}
#endif