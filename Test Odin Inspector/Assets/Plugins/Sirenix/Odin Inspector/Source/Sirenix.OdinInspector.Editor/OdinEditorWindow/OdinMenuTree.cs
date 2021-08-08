#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTree.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// OdinMenuTree provides a tree of <see cref="OdinMenuItem"/>s, and helps with selection, inserting menu items into the tree, and can handle keyboard navigation for you.
    /// </summary>
    /// <example>
    /// <code>
    /// OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
    /// {
    ///     { "Home",                           this,                           EditorIcons.House       },
    ///     { "Odin Settings",                  null,                           EditorIcons.SettingsCog },
    ///     { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
    ///     { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
    ///     { "Camera current",                 Camera.current                                          },
    ///     { "Some Class",                     this.someData                                           }
    /// };
    /// 
    /// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
    ///     .AddThumbnailIcons();
    /// 
    /// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
    /// 
    /// var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
    /// tree.MenuItems.Insert(2, customMenuItem);
    /// 
    /// tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
    /// tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
    /// </code>
    /// OdinMenuTrees are typically used with <see cref="OdinMenuEditorWindow"/>s but is made to work perfectly fine on its own for other use cases.
    /// OdinMenuItems can be inherited and and customized to fit your needs.
    /// <code>
    /// // Draw stuff
    /// someTree.DrawMenuTree();
    /// // Draw stuff
    /// someTree.HandleKeybaordMenuNavigation();
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuTree : IEnumerable
    {
        private static bool preventAutoFocus;

        /// <summary>
        /// Gets the currently active menu tree.
        /// </summary>
        public static OdinMenuTree ActiveMenuTree;
        private static HashSet<OdinMenuItem> cachedHashList = new HashSet<OdinMenuItem>();

        private readonly OdinMenuItem root;
        private readonly OdinMenuTreeSelection selection;
        private OdinMenuTreeDrawingConfig defaultConfig;
        private bool regainSearchFieldFocus;
        private bool hadSearchFieldFocus;
        private Rect outerScrollViewRect;
        private int hideScrollbarsWhileContentIsExpanding;
        private Rect innerScrollViewRect;
        private bool isFirstFrame = true;
        private int forceRegainFocusCounter = 0;
        private bool requestRepaint;
        private GUIFrameCounter frameCounter = new GUIFrameCounter();
        private bool hasRepaintedCurrentSearchResult = true;
        private bool scollToCenter;
        private OdinMenuItem scrollToWhenReady;
        private string searchFieldControlName;
        private bool isDirty;
        private bool updateSearchResults;
        private bool regainFocusWhenWindowFocus;
        private bool currWindowHasFocus;
        private EditorTimeHelper timeHelper = new EditorTimeHelper();

        internal static Rect VisibleRect;
        internal static EditorTimeHelper CurrentEditorTimeHelper;
        internal static float CurrentEditorTimeHelperDeltaTime;
        internal static Event CurrentEvent;
        internal static EventType CurrentEventType;

        public List<OdinMenuItem> FlatMenuTree = new List<OdinMenuItem>();

        internal OdinMenuItem Root
        {
            get { return this.root; }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        public OdinMenuTreeSelection Selection
        {
            get { return this.selection; }
        }

        /// <summary>
        /// Gets the root menu items.
        /// </summary>
        public List<OdinMenuItem> MenuItems
        {
            get { return this.root.ChildMenuItems; }
        }

        /// <summary>
        /// Gets the root menu item.
        /// </summary>
        public OdinMenuItem RootMenuItem
        {
            get { return this.root; }
        }

        /// <summary>
        /// If true, all indent levels will be ignored, and all menu items with IsVisible == true will be drawn.
        /// </summary>
        public bool DrawInSearchMode { get; private set; }

        /// <summary>
        /// Adds a menu item with the specified object instance at the the specified path.
        /// </summary>
        public void Add(string path, object instance)
        {
            this.AddObjectAtPath(path, instance);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        public void Add(string path, object instance, Texture icon)
        {
            this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        public void Add(string path, object instance, Sprite sprite)
        {
            this.AddObjectAtPath(path, instance).AddIcon(AssetPreview.GetAssetPreview(sprite));
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        public void Add(string path, object instance, EditorIcon icon)
        {
            this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Adds a collection of objects to the menu tree and returns all menu items created in random order.
        /// </summary>
        public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath)
        {
            if (collection == null)
            {
                return Enumerable.Empty<OdinMenuItem>();
            }

            cachedHashList.Clear();

            foreach (var item in collection)
            {
                cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
            }

            return cachedHashList;
        }

        /// <summary>
        /// Adds a collection of objects to the menu tree and returns all menu items created in random order.
        /// </summary>
        public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath, Func<T, Texture> getIcon)
        {
            if (collection == null)
            {
                return Enumerable.Empty<OdinMenuItem>();
            }

            cachedHashList.Clear();

            foreach (var item in collection)
            {
                if (getIcon != null)
                {
                    cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item).AddIcon(getIcon(item)));
                }
                else
                {
                    cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
                }
            }

            return cachedHashList;
        }

        /// <summary>
        /// Gets or sets the default menu item style from Config.DefaultStyle.
        /// </summary>
        public OdinMenuStyle DefaultMenuStyle
        {
            get { return this.Config.DefaultMenuStyle; }
            set { this.Config.DefaultMenuStyle = value; }
        }

        /// <summary>
        /// Gets or sets the default drawing configuration.
        /// </summary>
        public OdinMenuTreeDrawingConfig Config
        {
            get
            {
                this.defaultConfig = this.defaultConfig ?? new OdinMenuTreeDrawingConfig()
                {
                    DrawScrollView = true,
                    DrawSearchToolbar = false,
                    AutoHandleKeyboardNavigation = false
                };

                return this.defaultConfig;
            }
            set
            {
                this.defaultConfig = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        public OdinMenuTree()
            : this(false, new OdinMenuStyle())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        public OdinMenuTree(bool supportsMultiSelect)
            : this(supportsMultiSelect, new OdinMenuStyle())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        /// <param name="defaultMenuStyle">The default menu item style.</param>
        public OdinMenuTree(bool supportsMultiSelect, OdinMenuStyle defaultMenuStyle)
        {
            this.DefaultMenuStyle = defaultMenuStyle;
            this.selection = new OdinMenuTreeSelection(supportsMultiSelect);
            this.root = new OdinMenuItem(this, "root", null);
            this.SetupAutoScroll();
            this.searchFieldControlName = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        public OdinMenuTree(bool supportsMultiSelect, OdinMenuTreeDrawingConfig config)
        {
            this.Config = config;
            this.selection = new OdinMenuTreeSelection(supportsMultiSelect);
            this.root = new OdinMenuItem(this, "root", null);
            this.SetupAutoScroll();
        }

        private void SetupAutoScroll()
        {
            this.selection.SelectionChanged += (x) =>
            {
                if (this.Config.AutoScrollOnSelectionChanged && x == SelectionChangedType.ItemAdded)
                {
                    this.requestRepaint = true;
                    GUIHelper.RequestRepaint();

                    if (this.isFirstFrame)
                    {
                        this.ScrollToMenuItem(this.selection.LastOrDefault(), true);
                    }
                    else
                    {
                        this.ScrollToMenuItem(this.selection.LastOrDefault(), false);
                    }
                }
            };
        }

        /// <summary>
        /// Scrolls to the specified menu item.
        /// </summary>
        public void ScrollToMenuItem(OdinMenuItem menuItem, bool centerMenuItem = false)
        {
            if (menuItem != null)
            {
                this.scollToCenter = centerMenuItem;
                this.scrollToWhenReady = menuItem;

                if (!menuItem._IsVisible())
                {
                    foreach (var item in menuItem.GetParentMenuItemsRecursive(false))
                    {
                        item.Toggled = true;
                    }
                    return;
                }

                foreach (var item in menuItem.GetParentMenuItemsRecursive(false))
                {
                    item.Toggled = true;
                }

                if (this.outerScrollViewRect.height == 0 || menuItem.Rect.height <= 0.01f)
                {
                    return;
                }

                if (Event.current == null || Event.current.type != EventType.Repaint)
                {
                    return;
                }

                var config = this.Config;
                var rect = menuItem.Rect;

                float a, b;

                if (centerMenuItem)
                {
                    var r = this.outerScrollViewRect.AlignCenterY(rect.height);

                    a = rect.yMin - (this.innerScrollViewRect.y + config.ScrollPos.y - r.y);
                    b = (rect.yMax - r.height + this.innerScrollViewRect.y) - (config.ScrollPos.y + r.y);
                }
                else
                {
                    var viewRect = this.outerScrollViewRect;
                    viewRect.y = 0;

                    a = rect.yMin - (this.innerScrollViewRect.y + config.ScrollPos.y) - 1;
                    b = (rect.yMax - this.outerScrollViewRect.height + this.innerScrollViewRect.y) - config.ScrollPos.y;
                    a -= rect.height;
                    b += rect.height;
                }

                if (a < 0)
                {
                    config.ScrollPos.y += a;
                }

                if (b > 0)
                {
                    config.ScrollPos.y += b;
                }

                // Some windows takes a while to adjust themselves, where the inner and outer range are subject to change.
                if (this.frameCounter.FrameCount > 6)
                {
                    this.scrollToWhenReady = null;
                }
                else
                {
                    GUIHelper.RequestRepaint();
                }
            }
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        /// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
        public IEnumerable<OdinMenuItem> EnumerateTree(bool includeRootNode = false)
        {
            return this.root.GetChildMenuItemsRecursive(includeRootNode);
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
        public IEnumerable<OdinMenuItem> EnumerateTree(Func<OdinMenuItem, bool> predicate, bool includeRootNode)
        {
            return this.root.GetChildMenuItemsRecursive(includeRootNode).Where(predicate);
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        public void EnumerateTree(Action<OdinMenuItem> action)
        {
            this.root.GetChildMenuItemsRecursive(false).ForEach(action);
        }

        /// <summary>
        /// Draws the menu tree recursively.
        /// </summary>
        public void DrawMenuTree()
        {
            EditorTimeHelper prevTimeHelper = EditorTimeHelper.Time;
            EditorTimeHelper.Time = this.timeHelper;
            EditorTimeHelper.Time.Update();
            try
            {
                timeHelper.Update();
                this.frameCounter.Update();
                var config = this.Config;

                if (this.requestRepaint)
                {
                    GUIHelper.RequestRepaint();
                    this.requestRepaint = false;
                }

                if (config.DrawSearchToolbar)
                {
                    this.DrawSearchToolbar();
                }

                var outerRect = EditorGUILayout.BeginVertical();

                this.HandleActiveMenuTreeState(outerRect);

                if (config.DrawScrollView)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        this.outerScrollViewRect = outerRect;
                    }

                    if (this.hideScrollbarsWhileContentIsExpanding > 0)
                    {
                        config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUIStyle.none, GUIStyle.none, GUILayoutOptions.ExpandHeight(false));
                    }
                    else
                    {
                        config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUILayoutOptions.ExpandHeight(false));
                    }

                    var size = EditorGUILayout.BeginVertical();

                    // hideScrollbarsWhileContentIsExpanding:
                    // When drawn in confined areas, the scrollbars on the scrollview will flicker in and out while expanding.
                    // The code below ensures we hide the scorllbars remain invisible while the inner and outer scrollview heights are somewhat close to each other.
                    if (this.innerScrollViewRect.height == 0 || Event.current.type == EventType.Repaint)
                    {
                        var chancedSizeDiff = Mathf.Abs(this.innerScrollViewRect.height - size.height);
                        var boxDiff = Mathf.Abs(this.innerScrollViewRect.height - this.outerScrollViewRect.height);
                        var shouldHaveScrollViewRegardless = this.innerScrollViewRect.height - 40 > this.outerScrollViewRect.height;

                        if (!shouldHaveScrollViewRegardless && chancedSizeDiff > 0)
                        {
                            this.hideScrollbarsWhileContentIsExpanding = 5;
                            GUIHelper.RequestRepaint();
                        }
                        else if (Mathf.Abs(boxDiff) < 1)
                        {
                            this.hideScrollbarsWhileContentIsExpanding = 5;
                        }
                        else
                        {
                            this.hideScrollbarsWhileContentIsExpanding--;
                            if (this.hideScrollbarsWhileContentIsExpanding < 0)
                            {
                                this.hideScrollbarsWhileContentIsExpanding = 0;
                            }
                            else
                            {
                                GUIHelper.RequestRepaint();
                            }
                        }

                        this.innerScrollViewRect = size;
                    }

                    GUILayout.Space(-1);
                }

                if (this.isDirty && Event.current.type == EventType.Layout)
                {
                    this.UpdateMenuTree();
                    this.isDirty = false;
                }

                VisibleRect = GUIClipInfo.VisibleRect.Expand(300);
                CurrentEvent = Event.current;
                CurrentEventType = CurrentEvent.type;
                CurrentEditorTimeHelper = EditorTimeHelper.Time;
                CurrentEditorTimeHelperDeltaTime = CurrentEditorTimeHelper.DeltaTime;

                var tree = this.DrawInSearchMode ? this.FlatMenuTree : this.MenuItems;
                var count = tree.Count;

                if (config.EXPERIMENTAL_INTERNAL_DrawFlatTreeFastNoLayout)
                {
                    var itemHeight = this.DefaultMenuStyle.Height;
                    var height = count * itemHeight;
                    var rect = GUILayoutUtility.GetRect(0, height);

                    rect.height = itemHeight;

                    for (int i = 0; i < count; i++)
                    {
                        var item = tree[i];

                        item.EXPERIMENTAL_DontAllocateNewRect = true;
                        item.rect = rect;
                        item.DrawMenuItem(0);

                        rect.y += itemHeight;
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        tree[i].DrawMenuItems(0);
                    }
                }

                if (config.DrawScrollView)
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndVertical();

                if (config.AutoHandleKeyboardNavigation)
                {
                    this.HandleKeyboardMenuNavigation();
                }

                if (this.scrollToWhenReady != null)
                {
                    this.ScrollToMenuItem(this.scrollToWhenReady, this.scollToCenter);
                }

                if (Event.current.type == EventType.Repaint)
                {
                    this.isFirstFrame = false;
                }
            }
            finally
            {
                EditorTimeHelper.Time = prevTimeHelper;
            }
        }

        private void HandleActiveMenuTreeState(Rect outerRect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (this.currWindowHasFocus != GUIHelper.CurrentWindowHasFocus)
                {
                    this.currWindowHasFocus = GUIHelper.CurrentWindowHasFocus;

                    if (this.currWindowHasFocus && this.regainFocusWhenWindowFocus)
                    {
                        if (!OdinMenuTree.preventAutoFocus)
                        {
                            OdinMenuTree.ActiveMenuTree = this;
                        }

                        this.regainFocusWhenWindowFocus = false;
                    }
                }

                // Deactivate when another window is focused.
                if (!this.currWindowHasFocus && OdinMenuTree.ActiveMenuTree == this)
                {
                    OdinMenuTree.ActiveMenuTree = null;
                }

                // Whether we should activate the menu tree next time the window gets focus.
                if (this.currWindowHasFocus)
                {
                    this.regainFocusWhenWindowFocus = OdinMenuTree.ActiveMenuTree == this;
                }

                // Auto activate.
                if (this.currWindowHasFocus && OdinMenuTree.ActiveMenuTree == null)
                {
                    OdinMenuTree.ActiveMenuTree = this;
                }
            }

            this.MenuTreeActivationZone(outerRect);
        }

        internal void MenuTreeActivationZone(Rect rect)
        {
            if (OdinMenuTree.ActiveMenuTree == this)
            {
                return;
            }

            if (Event.current.rawType == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && GUIHelper.CurrentWindowHasFocus)
            {
                this.regainSearchFieldFocus = true;
                OdinMenuTree.preventAutoFocus = true;
                OdinMenuTree.ActiveMenuTree = this;
                UnityEditorEventUtility.EditorApplication_delayCall += () => preventAutoFocus = false;
                GUIHelper.RequestRepaint();
            }
        }

        /// <summary>
        /// Marks the dirty. This will cause a tree.UpdateTree() in the beginning of the next Layout frame.
        /// </summary>
        public void MarkDirty()
        {
            this.isDirty = true;
            this.updateSearchResults = true;
        }

        /// <summary>
        /// Draws the search toolbar.
        /// </summary>
        public void DrawSearchToolbar(GUIStyle toolbarStyle = null)
        {
            var config = this.Config;

            var searchFieldRect = GUILayoutUtility.GetRect(0, config.SearchToolbarHeight, GUILayoutOptions.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                (toolbarStyle ?? SirenixGUIStyles.ToolbarBackground).Draw(searchFieldRect, GUIContent.none, 0);
            }

            searchFieldRect = searchFieldRect.HorizontalPadding(5).AlignMiddle(16);
            searchFieldRect.xMin += 3;
            searchFieldRect.y += 1;

            EditorGUI.BeginChangeCheck();
            config.SearchTerm = this.DrawSearchField(searchFieldRect, config.SearchTerm, config.AutoFocusSearchBar);
            var changed = EditorGUI.EndChangeCheck();

            if ((changed || this.updateSearchResults) && this.hasRepaintedCurrentSearchResult)
            {
                this.updateSearchResults = false;

                // We want fast visual search feedback. If the user is typing faster than the window can repaint,
                // then no results will be visible while he's typing. this.hasRepaintedCurrentSearchResult fixes that.

                this.hasRepaintedCurrentSearchResult = false;
                bool doSearch = !string.IsNullOrEmpty(config.SearchTerm);
                if (doSearch)
                {
                    if (!this.DrawInSearchMode)
                    {
                        config.ScrollPos = new Vector2();
                    }

                    this.DrawInSearchMode = true;

                    if (config.SearchFunction != null)
                    {
                        // Custom search
                        this.FlatMenuTree.Clear();
                        foreach (var item in this.EnumerateTree())
                        {
                            if (config.SearchFunction(item))
                            {
                                this.FlatMenuTree.Add(item);
                            }
                        }
                    }
                    else
                    {
                        // Fuzzy search with sorting.
                        this.FlatMenuTree.Clear();
                        this.FlatMenuTree.AddRange(
                            this.EnumerateTree()
                            .Where(x => x.Value != null)
                            .Select(x =>
                            {
                                int score;
                                bool include = FuzzySearch.Contains(this.Config.SearchTerm, x.SearchString, out score);
                                return new { score = score, item = x, include = include };
                            })
                            .Where(x => x.include)
                            .OrderByDescending(x => x.score)
                            .Select(x => x.item));
                    }

                    this.root.UpdateFlatMenuItemNavigation();
                }
                else
                {
                    this.DrawInSearchMode = false;
                    // Ensure all selected elements are visible, and scroll to the last one.
                    this.FlatMenuTree.Clear();
                    var last = this.selection.LastOrDefault();
                    this.UpdateMenuTree();
                    this.Selection.SelectMany(x => x.GetParentMenuItemsRecursive(false)).ForEach(x => x.Toggled = true);
                    if (last != null)
                    {
                        this.ScrollToMenuItem(last);
                    }

                    this.root.UpdateFlatMenuItemNavigation();
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.hasRepaintedCurrentSearchResult = true;
            }
        }

        private string DrawSearchField(Rect rect, string searchTerm, bool autoFocus)
        {
            // We're hacking a bit here to ignore certain KeyCodes used for navigating the tree.
            // If anyone knows a good way of doing that, do tell. Keep in mind that keyboard navigation is handled after the entire tree is done drawing.
            var hasFocus = GUI.GetNameOfFocusedControl() == this.searchFieldControlName;

            if (this.hadSearchFieldFocus != hasFocus)
            {
                if (hasFocus)
                {
                    OdinMenuTree.ActiveMenuTree = this;
                }

                this.hadSearchFieldFocus = hasFocus;
            }

            bool ignore = hasFocus &&
               (Event.current.keyCode == KeyCode.DownArrow ||
                Event.current.keyCode == KeyCode.UpArrow ||
                Event.current.keyCode == KeyCode.LeftArrow ||
                Event.current.keyCode == KeyCode.RightArrow ||
                Event.current.keyCode == KeyCode.Return);

            if (ignore)
            {
                GUIHelper.PushEventType(Event.current.type);
            }

            searchTerm = SirenixEditorGUI.SearchField(rect, searchTerm, autoFocus && this.regainSearchFieldFocus && ActiveMenuTree == this, this.searchFieldControlName);

            if (this.regainSearchFieldFocus && Event.current.type == EventType.Layout)
            {
                this.regainSearchFieldFocus = false;
            }

            if (ignore)
            {
                GUIHelper.PopEventType();
                if (ActiveMenuTree == this)
                {
                    this.regainSearchFieldFocus = true;
                }
            }

            if (this.forceRegainFocusCounter < 20)
            {
                if (autoFocus && this.forceRegainFocusCounter < 4)
                {
                    if (ActiveMenuTree == this)
                    {
                        this.regainSearchFieldFocus = true;
                    }
                }

                GUIHelper.RequestRepaint();
                HandleUtility.Repaint();
                if (Event.current.type == EventType.Repaint)
                {
                    this.forceRegainFocusCounter++;
                }
            }

            return searchTerm;
        }

        /// <summary>
        /// Updates the menu tree. This method is usually called automatically when needed.
        /// </summary>
        public void UpdateMenuTree()
        {
            this.root.UpdateMenuTreeRecursive(true);
            this.root.UpdateFlatMenuItemNavigation();
        }

        /// <summary>
        /// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
        /// </summary>
        /// <returns>Returns true, if anything was changed via the keyboard.</returns>
        [Obsolete("Use HandleKeyboardMenuNavigation instead.", false)]
        public bool HandleKeybaordMenuNavigation()
        {
            return this.HandleKeyboardMenuNavigation();
        }

        /// <summary>
        /// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
        /// </summary>
        /// <returns>Returns true, if anything was changed via the keyboard.</returns>
        public bool HandleKeyboardMenuNavigation()
        {
            if (Event.current.type != EventType.KeyDown)
            {
                return false;
            }

            if (OdinMenuTree.ActiveMenuTree != this)
            {
                return false;
            }

            GUIHelper.RequestRepaint();

            var keycode = Event.current.keyCode;

            // Select first or last if no visisble items is slected.
            if (this.Selection.Count == 0 || !this.Selection.Any(x => x._IsVisible()))
            {
                var query = this.DrawInSearchMode ? this.FlatMenuTree : this.EnumerateTree().Where(x => x._IsVisible());

                OdinMenuItem next = null;
                if (keycode == KeyCode.DownArrow)
                {
                    next = query.FirstOrDefault();
                }
                else if (keycode == KeyCode.UpArrow)
                {
                    next = query.LastOrDefault();
                }
                else if (keycode == KeyCode.LeftAlt)
                {
                    next = query.FirstOrDefault();
                }
                else if (keycode == KeyCode.RightAlt)
                {
                    next = query.FirstOrDefault();
                }

                if (next != null)
                {
                    next.Select();
                    Event.current.Use();
                    return true;
                }
            }
            else
            {
                if (keycode == KeyCode.LeftArrow && !this.DrawInSearchMode)
                {
                    bool goUp = true;
                    foreach (var curr in this.Selection.ToList())
                    {
                        if (curr.Toggled == true && curr.ChildMenuItems.Any())
                        {
                            goUp = false;
                            curr.Toggled = false;
                        }

                        if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                        {
                            goUp = false;
                            foreach (var item in curr.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = curr.Toggled;
                            }
                        }
                    }

                    if (goUp)
                    {
                        keycode = KeyCode.UpArrow;
                    }

                    Event.current.Use();
                }

                if (keycode == KeyCode.RightArrow && !this.DrawInSearchMode)
                {
                    bool goDown = true;
                    foreach (var curr in this.Selection.ToList())
                    {
                        if (curr.Toggled == false && curr.ChildMenuItems.Any())
                        {
                            curr.Toggled = true;
                            goDown = false;
                        }

                        if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                        {
                            goDown = false;

                            foreach (var item in curr.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = curr.Toggled;
                            }
                        }
                    }

                    if (goDown)
                    {
                        keycode = KeyCode.DownArrow;
                    }

                    Event.current.Use();
                }

                if (keycode == KeyCode.UpArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var prev = last.PrevVisualMenuItem;

                        if (prev != null)
                        {
                            if (prev.IsSelected)
                            {
                                last.Deselect();
                            }
                            else
                            {
                                prev.Select(true);
                            }

                            Event.current.Use();
                            return true;
                        }
                    }
                    else
                    {
                        var prev = this.Selection.Last().PrevVisualMenuItem;
                        if (prev != null)
                        {
                            prev.Select();
                            Event.current.Use();
                            return true;
                        }
                    }
                }

                if (keycode == KeyCode.DownArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var next = last.NextVisualMenuItem;

                        if (next != null)
                        {
                            if (next.IsSelected)
                            {
                                last.Deselect();
                            }
                            else
                            {
                                next.Select(true);
                            }

                            Event.current.Use();
                            return true;
                        }
                    }
                    else
                    {
                        var next = this.Selection.Last().NextVisualMenuItem;
                        if (next != null)
                        {
                            next.Select();
                            Event.current.Use();
                            return true;
                        }
                    }
                }

                if (keycode == KeyCode.Return)
                {
                    this.Selection.ConfirmSelection();
                    Event.current.Use();
                    return true;
                }
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.MenuItems.GetEnumerator();
        }
    }
}
#endif