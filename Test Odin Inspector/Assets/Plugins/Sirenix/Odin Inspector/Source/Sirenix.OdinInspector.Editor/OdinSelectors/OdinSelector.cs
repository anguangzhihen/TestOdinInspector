#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using System.Linq;

    /// <summary>
    /// OdinSelectors is an abstract base class that combines OdinMenuTrees and OdinEditorWindows to help making feature-rich selectors and popup selectors.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MySelector : OdinSelector&lt;SomeType&gt;
    /// {
    ///     private readonly List&lt;SomeType&gt; source;
    ///     private readonly bool supportsMultiSelect;
    /// 
    ///     public MySelector(List&lt;SomeType&gt; source, bool supportsMultiSelect)
    ///     {
    ///         this.source = source;
    ///         this.supportsMultiSelect = supportsMultiSelect;
    ///     }
    /// 
    ///     protected override void BuildSelectionTree(OdinMenuTree tree)
    ///     {
    ///         tree.Config.DrawSearchToolbar = true;
    ///         tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
    ///         
    ///         tree.Add("Defaults/None", null);
    ///         tree.Add("Defaults/A", new SomeType());
    ///         tree.Add("Defaults/B", new SomeType());
    /// 
    ///         tree.AddRange(this.source, x =&gt; x.Path, x =&gt; x.SomeTexture);
    ///     }
    /// 
    ///     [OnInspectorGUI]
    ///     private void DrawInfoAboutSelectedItem()
    ///     {
    ///         SomeType selected = this.GetCurrentSelection().FirstOrDefault();
    /// 
    ///         if (selected != null)
    ///         {
    ///             GUILayout.Label("Name: " + selected.Name);
    ///             GUILayout.Label("Data: " + selected.Data);
    ///         }
    ///     }
    /// }
    /// </code>
    /// Usage:
    /// <code>
    /// void OnGUI()
    /// {
    ///     if (GUILayout.Button("Open My Selector"))
    ///     {
    ///         List&lt;SomeType&gt; source = this.GetListOfThingsToSelectFrom();
    ///         MySelector selector = new MySelector(source, false);
    /// 
    ///         selector.SetSelection(this.someValue);
    /// 
    ///         selector.SelectionCancelled += () =&gt; { };  // Occurs when the popup window is closed, and no slection was confirmed.
    ///         selector.SelectionChanged += col =&gt; { };
    ///         selector.SelectionConfirmed += col =&gt; this.someValue = col.FirstOrDefault();
    /// 
    ///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
    ///     }
    /// }
    /// 
    /// // All Odin Selectors can be rendered anywhere with Odin.
    /// [ShowInInspector]
    /// MySelector inlineSelector;
    /// </code>
    /// </example>
    /// <seealso cref="EnumSelector{T}"/>
    /// <seealso cref="TypeSelector"/>
    /// <seealso cref="GenericSelector{T}"/>
    /// <seealso cref="OdinMenuTree"/>
    /// <seealso cref="OdinEditorWindow"/>
    public abstract class OdinSelector<T>
    {
        private static EditorWindow selectorFieldWindow = null;
        private static IEnumerable<T> selectedValues = null;
        private static bool selectionWasConfirmed = false;
        private static bool selectionWasChanged = false;
        private static int confirmedPopupControlId = -1;
        private static int focusedControlId = -1;
        private static GUIStyle titleStyle = null;

        private OdinEditorWindow popupWindowInstance;
        private OdinMenuTree selectionTree;

        /// <summary>
        /// If true, a confirm selection button will be drawn in the title-bar.
        /// </summary>
        [HideInInspector]
        public bool DrawConfirmSelectionButton = false;

        [SerializeField, HideInInspector]
        private OdinMenuTreeDrawingConfig config = new OdinMenuTreeDrawingConfig()
        {
            SearchToolbarHeight = 22,
            AutoScrollOnSelectionChanged = true,
            DefaultMenuStyle = new OdinMenuStyle()
            {
                Height = 22
            }
        };

        private static bool wasKeyboard;

        private static int prevKeyboardId;

        /// <summary>
        /// Enables the single click to select.
        /// </summary>
        public void EnableSingleClickToSelect()
        {
            this.SelectionTree.EnumerateTree(x =>
            {
                x.OnDrawItem -= EnableSingleClickToSelect;
                x.OnDrawItem -= EnableSingleClickToSelect;
                x.OnDrawItem += EnableSingleClickToSelect;
            });
        }

        private void EnableSingleClickToSelect(OdinMenuItem obj)
        {
            var t = Event.current.type;

            if (t == EventType.Layout)
            {
                return;
            }

            if (obj.Rect.Contains(Event.current.mousePosition) == false)
            {
                return;
            }

            GUIHelper.RequestRepaint();

            if (Event.current.type == EventType.MouseDrag)
            {
                if (obj is T)
                {
                    if (this.IsValidSelection(Enumerable.Repeat((T)obj.Value, 1)))
                    {
                        obj.Select();
                    }
                }
            }

            bool confirm = t == EventType.MouseUp && (obj.ChildMenuItems.Count == 0);
            if (confirm)
            {
                obj.MenuTree.Selection.ConfirmSelection();
                Event.current.Use();
            }
        }

        /// <summary>
        /// Occurs when the window is closed, and no slection was confirmed.
        /// </summary>
        public event Action SelectionCancelled;

        /// <summary>
        /// Occurs when the menuTrees selection is changed and IsValidSelection returns true.
        /// </summary>
        public event Action<IEnumerable<T>> SelectionChanged;

        /// <summary>
        /// Occurs when the menuTrees selection is confirmed and IsValidSelection returns true.
        /// </summary>
        public event Action<IEnumerable<T>> SelectionConfirmed;

        /// <summary>
        /// Gets the selection menu tree.
        /// </summary>
        public OdinMenuTree SelectionTree
        {
            get
            {
                if (this.selectionTree == null)
                {
                    this.selectionTree = new OdinMenuTree(true);
                    this.selectionTree.Config = this.config;
                    OdinMenuTree.ActiveMenuTree = this.selectionTree;

                    this.BuildSelectionTree(this.selectionTree);

                    this.selectionTree.Selection.SelectionConfirmed += x =>
                    {
                        if (this.SelectionConfirmed != null)
                        {
                            IEnumerable<T> selected = this.GetCurrentSelection();
                            if (this.IsValidSelection(selected))
                            {
                                this.SelectionConfirmed(selected);
                            }
                        }
                    };

                    this.selectionTree.Selection.SelectionChanged += x =>
                    {
                        TriggerSelectionChanged();
                    };
                }

                return this.selectionTree;
            }
        }

        /// <summary>
        /// Gets the title. No title will be drawn if the string is null or empty.
        /// </summary>
        public virtual string Title { get { return null; } }

        /// <summary>
        /// Gets the current selection from the menu tree whether it's valid or not.
        /// </summary>
        public virtual IEnumerable<T> GetCurrentSelection()
        {
            return this.SelectionTree.Selection
                .Select(x => x.Value)
                .OfType<T>();
        }

        /// <summary>
        /// Determines whether the specified collection is a valid collection.
        /// If false, the SlectionChanged and SelectionConfirm events will not be called.
        /// By default, this returns true if the collection contains one or more items.
        /// </summary>
        public virtual bool IsValidSelection(IEnumerable<T> collection)
        {
            return true;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        public virtual void SetSelection(IEnumerable<T> selection)
        {
            this.SelectionTree.Selection.Clear();

            if (selection == null) return;

            foreach (var item in selection)
            {
                this.SetSelection(item);
            }
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        public virtual void SetSelection(T selected)
        {
            if (selected == null) return;

            var items = this.SelectionTree.EnumerateTree()
                .Where(x => x.Value is T)
                .Where(x => EqualityComparer<T>.Default.Equals((T)x.Value, selected))
                .ToList();

            items.ForEach(x => x.Select(true));
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified rect position.
        /// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
        /// </summary>
        public OdinEditorWindow ShowInPopup()
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;

            OdinEditorWindow window;

            var width = this.DefaultWindowWidth();
            if (width == 0)
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this);
            }
            else
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this, width);
            }

            this.SetupWindow(window, prevSelectedWindow);

            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified rect position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Rect btnRect)
        {
            return ShowInPopup(btnRect, btnRect.width);
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified rect position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Rect btnRect, float windowWidth)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, btnRect, windowWidth);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// The mouse position is used as the position for the window.
        /// Opens up the selector instance in a popup at the specified position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(float windowWidth)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, windowWidth);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Vector2 position, float windowWidth)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, position, windowWidth);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified rect position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Rect btnRect, Vector2 windowSize)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, btnRect, windowSize);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified position.
        /// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Vector2 position)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window;

            var width = this.DefaultWindowWidth();
            if (width == 0)
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this, position);
            }
            else
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this, position, width);
            }

            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup with the specified width and height.
        /// The mouse position is used as the position for the window.
        /// </summary>
        public OdinEditorWindow ShowInPopup(float width, float height)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, width, height);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Builds the selection tree.
        /// </summary>
        protected abstract void BuildSelectionTree(OdinMenuTree tree);

        /// <summary>
        /// When ShowInPopup is called, without a specifed window width, this methods gets called.
        /// Here you can calculate and give a good default width for the popup. 
        /// The default implementation returns 0, which will let the popup window determain the width itself. This is usually a fixed value.
        /// </summary>
        protected virtual float DefaultWindowWidth()
        {
            return 0;
        }

        /// <summary>
        /// Triggers the selection changed event, but only if the current selection is valid.
        /// </summary>
        protected void TriggerSelectionChanged()
        {
            if (this.SelectionChanged != null)
            {
                IEnumerable<T> selected = this.GetCurrentSelection();
                if (this.IsValidSelection(selected))
                {
                    this.SelectionChanged(selected);
                }
            }
        }

        /// <summary>
        /// Draw the selecotr manually.
        /// </summary>
        public void OnInspectorGUI()
        {
            this.DrawSelectionTree();
        }

        /// <summary>
        /// Draws the selection tree. This gets drawn using the OnInspectorGUI attribute.
        /// </summary>
        [OnInspectorGUI]
        [PropertyOrder(-1)]
        protected virtual void DrawSelectionTree()
        {
            var rect = EditorGUILayout.BeginVertical();
            {
                EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
                GUILayout.Space(1);

                bool drawTitle = !string.IsNullOrEmpty(this.Title);
                bool drawSearchToolbar = this.SelectionTree.Config.DrawSearchToolbar;
                bool drawButton = this.DrawConfirmSelectionButton;

                if (drawTitle || drawSearchToolbar || drawButton)
                {
                    SirenixEditorGUI.BeginHorizontalToolbar(this.SelectionTree.Config.SearchToolbarHeight);
                    {
                        if (drawTitle)
                        {
                            if (titleStyle == null)
                            {
                                titleStyle = new GUIStyle(SirenixGUIStyles.LeftAlignedCenteredLabel) { padding = new RectOffset(10, 10, 0, 0) };
                            }

                            var labelRect = GUILayoutUtility.GetRect(new GUIContent(this.Title), titleStyle, GUILayoutOptions.ExpandWidth(false).Height(this.SelectionTree.Config.SearchToolbarHeight));

                            if (Event.current.type == EventType.Repaint)
                            {
                                labelRect.y -= 2;
                                GUI.Label(labelRect.AlignCenterY(16), this.Title, titleStyle);
                            }
                        }

                        if (drawSearchToolbar)
                        {
                            this.SelectionTree.DrawSearchToolbar(GUIStyle.none);
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                        }

                        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1), SirenixGUIStyles.BorderColor);

                        if (drawButton && SirenixEditorGUI.ToolbarButton(new GUIContent(EditorIcons.TestPassed)))
                        {
                            this.SelectionTree.Selection.ConfirmSelection();
                        }
                    }
                    SirenixEditorGUI.EndHorizontalToolbar();
                }

                var prev = this.SelectionTree.Config.DrawSearchToolbar;
                this.SelectionTree.Config.DrawSearchToolbar = false;
                if (this.SelectionTree.MenuItems.Count == 0)
                {
                    GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
                    SirenixEditorGUI.InfoMessageBox("There are no possible values to select.");
                    GUILayout.EndVertical();
                }
                this.SelectionTree.DrawMenuTree();
                this.SelectionTree.Config.DrawSearchToolbar = prev;

                SirenixEditorGUI.DrawBorders(rect, 1);
            }
            EditorGUILayout.EndVertical();
        }

        private void SetupWindow(OdinEditorWindow window, EditorWindow prevSelectedWindow)
        {
            var prevFocusId = GUIUtility.hotControl;
            var prevKeybaorFocus = GUIUtility.keyboardControl;
            this.popupWindowInstance = window;

            window.WindowPadding = new Vector4();

            bool wasConfirmed = false;

            this.SelectionTree.Selection.SelectionConfirmed += x =>
            {
                var ctrl = Event.current != null && Event.current.modifiers != EventModifiers.Control;

                UnityEditorEventUtility.DelayAction(() =>
                {
                    if (this.IsValidSelection(this.GetCurrentSelection()))
                    {
                        wasConfirmed = true;

                        // This is so that users can hold control to trigger changes, without the window automatically closing.
                        if (ctrl)
                        {
                            window.Close();

                            if (prevSelectedWindow)
                            {
                                prevSelectedWindow.Focus();
                            }
                        }
                    }
                });
            };

            window.OnBeginGUI += () =>
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    UnityEditorEventUtility.DelayAction(() =>
                    {
                        window.Close();
                    });

                    if (prevSelectedWindow)
                    {
                        prevSelectedWindow.Focus();
                    }

                    Event.current.Use();
                }
            };

            window.OnClose += () =>
            {
                if (!wasConfirmed && this.SelectionCancelled != null)
                {
                    this.SelectionCancelled();
                }

                GUIUtility.hotControl = prevFocusId;
                GUIUtility.keyboardControl = prevKeybaorFocus;
            };
        }

        internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, string label, GUIStyle style, int id, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter)
            where TSelector : OdinSelector<T>
        {
            return DrawSelectorButton(buttonRect, new GUIContent(label), style, id, out bindSelector, out resultGetter);
        }

        internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, GUIContent label, GUIStyle style, int id, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter)
            where TSelector : OdinSelector<T>
        {
            var wasPressed = false;
            bindSelector = null;
            resultGetter = null;

            if (Event.current.type == EventType.Repaint)
            {
                var showIsDown = GUIUtility.hotControl == id || focusedControlId == id;
                style = style ?? EditorStyles.popup;
                style.Draw(buttonRect, label, showIsDown, showIsDown, false, GUIUtility.keyboardControl == id);
            }

            bool openPopup = false;

            if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown && GUIUtility.keyboardControl == id)
            {
                GUIUtility.hotControl = id;
                wasKeyboard = true;
            }
            else if (GUIUtility.hotControl == id && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp && GUIUtility.keyboardControl == id)
            {
                openPopup = true;
                wasKeyboard = true;
            }
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && buttonRect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                wasKeyboard = false;
            }
            else if (GUIUtility.hotControl == id && Event.current.type == EventType.MouseUp && Event.current.button == 0 && buttonRect.Contains(Event.current.mousePosition))
            {
                openPopup = true;
                wasKeyboard = false;
            }

            if (openPopup)
            {
                prevKeyboardId = GUIUtility.keyboardControl;
                selectedValues = null;
                selectionWasConfirmed = false;
                selectionWasChanged = false;
                focusedControlId = id;
                selectorFieldWindow = EditorWindow.focusedWindow;

                GUIUtility.hotControl = id;

                if (wasKeyboard)
                {
                    GUIUtility.keyboardControl = id;
                }

                bindSelector = selector =>
                {
                    selector.SelectionChanged += x =>
                    {
                        selectedValues = x;
                        selectionWasChanged = true;
                        confirmedPopupControlId = id;
                    };

                    selector.SelectionConfirmed += x =>
                    {
                        selectionWasConfirmed = true;
                        selectedValues = x;
                        confirmedPopupControlId = id;
                    };

                    var window = selector.popupWindowInstance;
                    if (window != null)
                    {
                        window.OnClose += () => focusedControlId = -1;
                        window.OnClose += () => confirmedPopupControlId = id;
                    }
                };

                wasPressed = true;
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint && selectorFieldWindow == GUIHelper.CurrentWindow && id == confirmedPopupControlId)
            {
                //selectorFieldWindow = null;

                if (wasKeyboard)
                {
                    GUIUtility.keyboardControl = prevKeyboardId;
                }
                else
                {
                    GUIUtility.keyboardControl = 0;
                }

                if (focusedControlId == -1)
                {
                    confirmedPopupControlId = 0;
                }

                if (selectionWasConfirmed)
                {
                    confirmedPopupControlId = 0;
                    focusedControlId = -1;
                    GUI.changed = true;
                    selectionWasConfirmed = false;
                    GUIHelper.RequestRepaint();
                    resultGetter = () => selectedValues ?? Enumerable.Empty<T>();
                }
                else if (selectionWasChanged)
                {
                    selectionWasChanged = false;
                    GUIHelper.RequestRepaint();
                    resultGetter = () => selectedValues ?? Enumerable.Empty<T>();
                }

            }

            return wasPressed;
        }

        private static GUIContent tmpValueLabel;

        /// <summary>
        /// Draws dropwdown field, that creates and binds the selector to the dropdown field.
        /// </summary>
        public static IEnumerable<T> DrawSelectorDropdown(Rect rect, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null)
        {
            return DrawSelectorDropdown(rect, new GUIContent(btnLabel), createSelector, style);
        }

        /// <summary>
        /// Draws dropwdown field, that creates and binds the selector to the dropdown field.
        /// </summary>
        public static IEnumerable<T> DrawSelectorDropdown(Rect rect, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null)
        {
            tmpValueLabel = tmpValueLabel ?? new GUIContent();

            int id = GUIUtility.GetControlID(FocusType.Keyboard);

            Action<OdinSelector<T>> bindSelector;
            Func<IEnumerable<T>> getResult;

            tmpValueLabel.image = btnLabel.image;
            tmpValueLabel.text = EditorGUI.showMixedValue ? SirenixEditorGUI.MixedValueDashChar : btnLabel.text;
            tmpValueLabel.tooltip = btnLabel.tooltip;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = id;
            }

            style = style ?? EditorStyles.popup;
            if (DrawSelectorButton(rect, tmpValueLabel, style, id, out bindSelector, out getResult))
            {
                var selector = createSelector(rect);
                bindSelector(selector);
            }

            if (getResult != null)
            {
                return getResult();
            }

            return null;
        }

        /// <summary>
        /// Draws dropwdown field, that creates and binds the selector to the dropdown field.
        /// </summary>
        public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return DrawSelectorDropdown(label, new GUIContent(btnLabel), createSelector, style, options);
        }

        /// <summary>
        /// Draws dropwdown field, that creates and binds the selector to the dropdown field.
        /// </summary>
        public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null, params GUILayoutOption[] options)
        {
            tmpValueLabel = tmpValueLabel ?? new GUIContent();

            int id;
            bool hasFocus;
            Rect rect;
            Action<OdinSelector<T>> bindSelector;
            Func<IEnumerable<T>> getResult;

            tmpValueLabel.image = btnLabel.image;
            tmpValueLabel.text = EditorGUI.showMixedValue ? SirenixEditorGUI.MixedValueDashChar : btnLabel.text;
            tmpValueLabel.tooltip = btnLabel.tooltip;

            SirenixEditorGUI.GetFeatureRichControlRect(label, out id, out hasFocus, out rect, options);
            style = style ?? EditorStyles.popup;
            if (DrawSelectorButton(rect, tmpValueLabel, style, id, out bindSelector, out getResult))
            {
                var selector = createSelector(rect);
                bindSelector(selector);
            }

            if (getResult != null)
            {
                return getResult();
            }

            return null;
        }
    }
}
#endif