#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuItem.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using Sirenix.Utilities;
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using System.Collections;

    /// <summary>
    /// A menu item that represents one or more objects.
    /// </summary>
    /// <seealso cref="OdinMenuTree" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuItem
    {
        private static Color mouseOverColor = EditorGUIUtility.isProSkin ? new Color(1.000f, 1.000f, 1.000f, 0.028f) : new Color(1.000f, 1.000f, 1.000f, 0.3f);
        private static OdinMenuItem handleClickEventOnMouseUp;
        private static bool previousMenuItemWasSelected = false;

        private List<OdinMenuItem> childMenuItems;
        private int flatTreeIndex;
        private Texture iconSelected;
        private Texture icon;
        private Func<Texture> iconGetter;
        private bool isInitialized = false;
        private LocalPersistentContext<bool> isToggledContext;
        private OdinMenuTree menuTree;
        private string prevName;
        private string name;
        private bool isVisible = true;
        private OdinMenuItem nextMenuItem;
        private OdinMenuItem nextMenuItemFlat;
        //private IList objectInstances;
        private OdinMenuItem parentMenuItem;
        private OdinMenuItem previousMenuItem;
        private OdinMenuItem previousMenuItemFlat;
        private OdinMenuStyle style;
        private Rect triangleRect;
        private Rect labelRect;
        private bool? nonCachedToggledState;
        private object value;

        internal Rect rect;
        internal bool EXPERIMENTAL_DontAllocateNewRect;

        public bool MenuItemIsBeingRendered;

        /// <summary>
        /// The default toggled state
        /// </summary>
        public bool DefaultToggledState = false;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuItem"/> class.
        /// </summary>
        /// <param name="tree">The Odin menu tree instance the menu item belongs to.</param>
        /// <param name="name">The name of the menu item.</param>
        /// <param name="value">The instance the value item represents.</param>
        public OdinMenuItem(OdinMenuTree tree, string name, object value)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            if (name == null) throw new ArgumentNullException("name");

            this.menuTree = tree;
            this.name = name;
            this.SearchString = name;
            this.Value = value;
            this.childMenuItems = new List<OdinMenuItem>();
        }

        /// <summary>
        /// Occurs right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
        /// </summary>
        public Action<OdinMenuItem> OnDrawItem;

        /// <summary>
        /// Occurs when the user has right-clicked the menu item.
        /// </summary>
        public Action<OdinMenuItem> OnRightClick;

        /// <summary>
        /// Gets the child menu items.
        /// </summary>
        /// <value>
        /// The child menu items.
        /// </value>
        public virtual List<OdinMenuItem> ChildMenuItems
        {
            get { return this.childMenuItems; }
        }

        /// <summary>
        /// Gets the index location of the menu item.
        /// </summary>
        public int FlatTreeIndex
        {
            get { return this.flatTreeIndex; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is visible.
        /// Not that setting this to false will not hide its children as well. For that see use Toggled.
        /// </summary>
        [Obsolete("OdinMenuItems no longer has this concept which was previously used for filtering search results. Instead search results are cached to seperate list in order to support sorting.")]
        public virtual bool IsVisible
        {
            get { return this.isVisible; }
            set { this.isVisible = value; }
        }

        /// <summary>
        /// Gets or sets the icon that is used when the menu item is not selected.
        /// </summary>
        public Texture Icon
        {
            get { return this.icon; }
            set { this.icon = value; }
        }

        /// <summary>
        /// Gets or sets the icon that is used when the menu item is selected.
        /// </summary>
        public Texture IconSelected
        {
            get { return this.iconSelected; }
            set { this.iconSelected = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return this.menuTree.Selection.Contains(this); }
        }

        /// <summary>
        /// Gets the menu tree instance.
        /// </summary>
        public OdinMenuTree MenuTree
        {
            get { return this.menuTree; }
        }

        /// <summary>
        /// Gets or sets the raw menu item name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets the search string used when searching for menu items.
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets the next visual menu item.
        /// </summary>
        public OdinMenuItem NextVisualMenuItem
        {
            get
            {
                this.EnsureInitialized();

                // Search mode
                if (this.MenuTree.DrawInSearchMode)
                {
                    return this.nextMenuItemFlat;
                }

                // Performance optimization:
                if (this.ChildMenuItems.Count > 0 && this.nextMenuItem != null && this.Toggled == false && this._IsVisible())
                {
                    return this.nextMenuItem;
                }

                // Bruteforce search:
                return this.GetAllNextMenuItems().FirstOrDefault(x => x._IsVisible());
            }
        }

        /// <summary>
        /// Gets the parent menu item.
        /// </summary>
        public OdinMenuItem Parent
        {
            get
            {
                this.EnsureInitialized();
                return this.parentMenuItem;
            }
        }

        /// <summary>
        /// Gets the previous visual menu item.
        /// </summary>
        public OdinMenuItem PrevVisualMenuItem
        {
            get
            {
                this.EnsureInitialized();

                // Search mode
                if (this.MenuTree.DrawInSearchMode)
                {
                    return this.previousMenuItemFlat;
                }

                // Performance optimization:
                if (this.ChildMenuItems.Count > 0 && this.Toggled == false && this._IsVisible())
                {
                    if (this.previousMenuItem != null)
                    {
                        if (this.previousMenuItem.ChildMenuItems.Count == 0 || this.previousMenuItem.Toggled == false)
                        {
                            return this.previousMenuItem;
                        }
                    }
                    else if (this.parentMenuItem != null)
                    {
                        return this.parentMenuItem;
                    }
                }

                // Bruteforce search:
                return this.GetAllPreviousMenuItems().FirstOrDefault(x => x._IsVisible());
            }
        }

        /// <summary>
        /// Gets the drawn rect.
        /// </summary>
        public Rect Rect
        {
            get { return this.rect; }
        }

        /// <summary>
        /// Gets the drawn label rect.
        /// </summary>
        public Rect LabelRect
        {
            get { return this.labelRect; }
        }

        /// <summary>
        /// Gets or sets the style. If null is specified, then the menu trees DefaultMenuStyle is used.
        /// </summary>
        public OdinMenuStyle Style
        {
            get
            {
                if (this.style == null)
                {
                    this.style = this.menuTree.DefaultMenuStyle;
                }

                return this.style;
            }
            set { this.style = value; }
        }

        /// <summary>
        /// Deselects this instance.
        /// </summary>
        public bool Deselect()
        {
            return this.menuTree.Selection.Remove(this);
        }

        /// <summary>
        /// Selects the specified add to selection.
        /// </summary>
        public void Select(bool addToSelection = false)
        {
            if (addToSelection == false)
            {
                this.menuTree.Selection.Clear();
            }

            this.menuTree.Selection.Add(this);
        }

        /// <summary>
        /// Gets the child menu items recursive in a DFS.
        /// </summary>
        /// <param name="includeSelf">Whether to include it self in the collection.</param>
        public IEnumerable<OdinMenuItem> GetChildMenuItemsRecursive(bool includeSelf)
        {
            if (includeSelf) yield return this;

            foreach (var child in this.ChildMenuItems.SelectMany(x => x.GetChildMenuItemsRecursive(true)))
            {
                yield return child;
            }
        }

        /// <summary>
        /// Gets the child menu items recursive in a DFS.
        /// </summary>
        /// <param name="includeSelf">Whether to include it self in the collection.</param>
        /// <param name="includeRoot">Whether to include the root.</param>
        public IEnumerable<OdinMenuItem> GetParentMenuItemsRecursive(bool includeSelf, bool includeRoot = false)
        {
            if (includeSelf || this.Parent == null && includeRoot)
            {
                yield return this;
            }

            if (this.Parent != null)
            {
                foreach (var item in this.Parent.GetParentMenuItemsRecursive(true, includeRoot))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Gets the full menu item path.
        /// </summary>
        public string GetFullPath()
        {
            this.EnsureInitialized();

            var parent = this.Parent;

            if (parent == null)
            {
                return this.SmartName;
            }

            return parent.GetFullPath() + "/" + this.SmartName;
        }

        /// <summary>
        /// Gets the first object of the <see cref="ObjectInstances"/>
        /// </summary>
        [Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value instead.", false)]
        public virtual object ObjectInstance
        {
            get
            {
                if (this.Value == null)
                {
                    return null;
                }

                object val;

                var objectInstances = this.Value as IList;
                if (objectInstances == null || objectInstances.Count == 0)
                {
                    val = this.Value;
                }
                else
                {
                    val = objectInstances[0];
                }

                var instanceFunc = val as Func<object>;
                if (instanceFunc != null)
                {
                    return instanceFunc.Invoke();
                }

                return val;
            }
        }

        /// <summary>
        /// Gets the object instances the menu item represents
        /// </summary>
        [Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value as IEnumerable instead.", false)]
        public virtual IEnumerable<object> ObjectInstances
        {
            get
            {
                var objectInstances = this.Value as IList;
                if (objectInstances == null || objectInstances.Count == 0)
                {
                    yield break;
                }

                foreach (var item in objectInstances)
                {
                    if (item == null)
                    {
                        yield return null;
                    }

                    var instance = item;
                    var instanceFunc = instance as Func<object>;
                    if (instanceFunc != null)
                    {
                        yield return instanceFunc.Invoke();
                    }
                    else
                    {
                        yield return instance;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the object instance
        /// </summary>
        [Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value = obj instead.", false)]
        public void SetObjectInstance(object obj)
        {
            this.Value = obj;
        }

        /// <summary>
        /// Sets the object instances
        /// </summary>
        [Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value = obj instead.", false)]
        public void SetObjectInstances(IList objects)
        {
            this.Value = objects;
        }

        /// <summary>
        /// Gets or sets the value the menu item represents.
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets a nice menu item name. If the raw name value is null or a dollar sign, then the name is retrieved from the object itself via ToString().
        /// </summary>
        public virtual string SmartName
        {
            get
            {
                var val = this.value;
                var func = this.Value as Func<object>;
                if (func != null)
                {
                    val = func();
                }

                if (this.name == null || this.name == "$")
                {
                    if (val == null)
                    {
                        return "";
                    }

                    var unityObject = val as UnityEngine.Object;

                    if (unityObject)
                    {
                        return unityObject.name.SplitPascalCase();
                    }

                    return val.ToString();
                }

                // Note: this used to implement smart string resolver logic, which has now been removed for performance and API simplicity reasons.
                // This was done via the now obsoleted/removed StringMemberHelper.

                return this.name;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OdinMenuItem"/> is toggled / expanded. This value tries it best to be persistent.
        /// </summary>
        public virtual bool Toggled
        {
            get
            {
                if (this.childMenuItems.Count == 0)
                {
                    return false;
                }

                if (this.menuTree.Config.UseCachedExpandedStates)
                {
                    if (this.isToggledContext == null)
                    {
                        this.isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + this.GetFullPath(), this.DefaultToggledState));
                    }

                    return this.isToggledContext.Value;
                }

                if (!this.nonCachedToggledState.HasValue)
                {
                    this.nonCachedToggledState = this.DefaultToggledState;
                }

                return this.nonCachedToggledState.Value;
            }
            set
            {
                if (this.menuTree.Config.UseCachedExpandedStates)
                {
                    if (this.isToggledContext == null)
                    {
                        this.isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + this.GetFullPath(), this.DefaultToggledState));
                    }

                    this.isToggledContext.Value = value;
                }
                else
                {
                    this.nonCachedToggledState = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon getter.
        /// </summary>
        public Func<Texture> IconGetter
        {
            get
            {
                if (this.iconGetter == null)
                {
                    this.iconGetter = () =>
                    {
                        return this.IsSelected ? (this.IconSelected ? this.IconSelected : this.Icon) : this.Icon;
                    };
                }

                return this.iconGetter;
            }

            set
            {
                this.iconGetter = value;
            }
        }

        private float t = -1;
        private bool wasMouseDownEvent;

        /// <summary>
        /// Draws this menu item followed by all of its child menu items
        /// </summary>
        /// <param name="indentLevel">The indent level.</param>
        public virtual void DrawMenuItems(int indentLevel)
        {
            this.DrawMenuItem(indentLevel);

            var children = this.ChildMenuItems;
            var childCount = children.Count;

            if (childCount == 0) return;

            bool isVisible = this.Toggled;
            if (t < 0)
            {
                t = isVisible ? 1 : 0;
            }

            if (OdinMenuTree.CurrentEventType == EventType.Layout)
            {
                t = Mathf.MoveTowards(t, isVisible ? 1 : 0, OdinMenuTree.CurrentEditorTimeHelperDeltaTime * (1f / SirenixEditorGUI.DefaultFadeGroupDuration));
            }

            if (SirenixEditorGUI.BeginFadeGroup(t))
            {
                for (int i = 0; i < childCount; i++)
                {
                    children[i].DrawMenuItems(indentLevel + 1);
                }
            }
            SirenixEditorGUI.EndFadeGroup();
        }

        /// <summary>
        /// Draws the menu item with the specified indent level.
        /// </summary>
        public virtual void DrawMenuItem(int indentLevel)
        {
            Rect newRect = this.EXPERIMENTAL_DontAllocateNewRect ? this.rect : GUILayoutUtility.GetRect(0, this.Style.Height);

            var e = OdinMenuTree.CurrentEvent;
            var eType = OdinMenuTree.CurrentEventType;

            if (eType == EventType.Layout)
            {
                return;
            }

            if (eType == EventType.Repaint || eType != EventType.Layout && this.rect.width == 0)
            {
                this.rect = newRect;
            }

            float rectY = this.rect.y;

            if (rectY > 1000)
            {
                float visibleRectY = OdinMenuTree.VisibleRect.y;

                if (rectY + this.rect.height < visibleRectY || rectY > visibleRectY + OdinMenuTree.VisibleRect.height)
                {
                    this.MenuItemIsBeingRendered = false;
                    return;
                }
            }

            this.MenuItemIsBeingRendered = true;

            if (eType == EventType.Repaint)
            {
                float indent = this.Style.Offset + (indentLevel * this.Style.IndentAmount);
                this.labelRect = this.rect.AddXMin(indent);
                var selected = this.IsSelected;

                // bg
                if (selected)
                {
                    if (OdinMenuTree.ActiveMenuTree == this.menuTree)
                    {
                        if (EditorGUIUtility.isProSkin)
                        {
                            EditorGUI.DrawRect(this.rect, this.Style.SelectedColorDarkSkin);
                        }
                        else
                        {
                            EditorGUI.DrawRect(this.rect, this.Style.SelectedColorLightSkin);
                        }
                    }
                    else
                    {
                        if (EditorGUIUtility.isProSkin)
                        {
                            EditorGUI.DrawRect(this.rect, this.Style.SelectedInactiveColorDarkSkin);
                        }
                        else
                        {
                            EditorGUI.DrawRect(this.rect, this.Style.SelectedInactiveColorLightSkin);
                        }
                    }
                }

                // Hover
                if (!selected && this.rect.Contains(e.mousePosition))
                {
                    EditorGUI.DrawRect(this.rect, mouseOverColor);
                }

                // Triangle
                if (this.ChildMenuItems.Count > 0 && !this.MenuTree.DrawInSearchMode && this.Style.DrawFoldoutTriangle)
                {
                    var icon = this.Toggled ? EditorIcons.TriangleDown : EditorIcons.TriangleRight;

                    if (this.Style.AlignTriangleLeft)
                    {
                        this.triangleRect = this.labelRect.AlignLeft(this.Style.TriangleSize).AlignMiddle(this.Style.TriangleSize);
                        this.triangleRect.x -= this.Style.TriangleSize - this.Style.TrianglePadding;
                    }
                    else
                    {
                        this.triangleRect = this.rect.AlignRight(this.Style.TriangleSize).AlignMiddle(this.Style.TriangleSize);
                        this.triangleRect.x -= this.Style.TrianglePadding;
                    }

                    if (eType == EventType.Repaint)
                    {
                        if (EditorGUIUtility.isProSkin)
                        {
                            if (selected || this.triangleRect.Contains(e.mousePosition))
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Highlighted);
                            }
                            else
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Active);
                            }
                        }
                        else
                        {
                            if (selected)
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Raw);
                            }
                            else if (this.triangleRect.Contains(e.mousePosition))
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Active);
                            }
                            else
                            {
                                GUIHelper.PushColor(new Color(1, 1, 1, 0.7f));
                                GUI.DrawTexture(this.triangleRect, icon.Active);
                                GUIHelper.PopColor();
                            }
                        }
                    }
                }

                var ico = this.IconGetter();
                // Icon
                if (ico)
                {
                    var iconRect = this.labelRect.AlignLeft(this.Style.IconSize).AlignMiddle(this.Style.IconSize);
                    iconRect.x += this.Style.IconOffset;

                    if (!selected)
                    {
                        GUIHelper.PushColor(new Color(1, 1, 1, this.Style.NotSelectedIconAlpha));
                    }

                    GUI.DrawTexture(iconRect, ico, ScaleMode.ScaleToFit);

                    this.labelRect.xMin += this.Style.IconSize + this.Style.IconPadding;

                    if (!selected)
                    {
                        GUIHelper.PopColor();
                    }
                }

                // Label
                var labelStyle = selected ? this.Style.SelectedLabelStyle : this.Style.DefaultLabelStyle;
                this.labelRect = this.labelRect.AlignMiddle(16).AddY(this.Style.LabelVerticalOffset);
                GUI.Label(this.labelRect, this.SmartName, labelStyle);

                // Borders
                if (this.Style.Borders)
                {
                    var borderPadding = this.Style.BorderPadding;
                    var draw = true;

                    if (selected || previousMenuItemWasSelected)
                    {
                        borderPadding = 0;
                        if (!EditorGUIUtility.isProSkin)
                        {
                            draw = false;
                        }
                    }

                    previousMenuItemWasSelected = selected;

                    if (draw)
                    {
                        var border = this.rect;
                        border.x += borderPadding;
                        border.width -= borderPadding * 2;
                        SirenixEditorGUI.DrawHorizontalLineSeperator(border.x, border.y, border.width, this.Style.BorderAlpha);
                    }
                }
            }

            // Prepare mouse stuff.
            // If the user uses the MouseDown event to for instance start a drag.
            // The the menu item won't be selected, this fixes it so that we also listen to the mouse up event.

            // TODO: Add the concept of a menuitem being highligted, and change the click behaviour so that mouse down highlights,
            // and mouse up selects. On a related note.. also give DragAndDropUtilities.DragZone 
            // a bit af distance threshold before a drag actually starts, so that it becomes a little easier to click something
            // without starting a drag.
            this.wasMouseDownEvent = eType == EventType.MouseDown && this.rect.Contains(e.mousePosition);
            if (this.wasMouseDownEvent)
            {
                handleClickEventOnMouseUp = this;
            }

            // Draw user stuff.
            this.OnDrawMenuItem(this.rect, this.labelRect);

            if (this.OnDrawItem != null)
            {
                this.OnDrawItem(this);
            }

            // Handle mouse events.
            this.HandleMouseEvents(rect, this.triangleRect);
        }

        /// <summary>
        /// Override this to add custom GUI to the menu items.
        /// This is called right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
        /// </summary>
        protected virtual void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
        }

        internal void UpdateMenuTreeRecursive(bool isRoot = false)
        {
            this.isInitialized = true;
            OdinMenuItem prev = null;
            foreach (var child in this.ChildMenuItems)
            {
                child.parentMenuItem = null;
                child.nextMenuItem = null;
                child.previousMenuItemFlat = null;
                child.nextMenuItemFlat = null;
                child.previousMenuItem = null;

                if (!isRoot)
                {
                    child.parentMenuItem = this;
                }

                if (prev != null)
                {
                    prev.nextMenuItem = child;
                    child.previousMenuItem = prev;
                }

                prev = child;

                child.UpdateMenuTreeRecursive();
            }
        }

        internal void UpdateFlatMenuItemNavigation()
        {
            int i = 0;
            OdinMenuItem prev = null;
            IEnumerable<OdinMenuItem> query = this.menuTree.DrawInSearchMode ? this.menuTree.FlatMenuTree : this.menuTree.EnumerateTree();

            foreach (var item in query)
            {
                item.flatTreeIndex = i++;
                item.nextMenuItemFlat = null;
                item.previousMenuItemFlat = null;

                if (prev != null)
                {
                    item.previousMenuItemFlat = prev;
                    prev.nextMenuItemFlat = item;
                }

                prev = item;
            }
        }

        /// <summary>
        /// Handles the mouse events.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="triangleRect">The triangle rect.</param>
        protected void HandleMouseEvents(Rect rect, Rect triangleRect)
        {
            var e = Event.current.type;

            if (e == EventType.Used && this.wasMouseDownEvent)
            {
                this.wasMouseDownEvent = false;
                handleClickEventOnMouseUp = this;
            }

            bool isMouseClick =
                (e == EventType.MouseDown) ||
                (e == EventType.MouseUp && handleClickEventOnMouseUp == this);

            if (isMouseClick)
            {
                handleClickEventOnMouseUp = null;
                this.wasMouseDownEvent = false;

                if (!rect.Contains(Event.current.mousePosition))
                {
                    return;
                }

                var hasChildren = this.ChildMenuItems.Any();
                var selected = this.IsSelected;

                // No more pining.
                // bool isUnityObjectInstance = instance as UnityEngine.Object;
                // if (selected && isUnityObjectInstance)
                // {
                //     var unityObject = instance as UnityEngine.Object;
                //     var behaviour = unityObject as Behaviour;
                //     if (behaviour)
                //     {
                //         unityObject = behaviour.gameObject;
                //     }
                //     EditorGUIUtility.PingObject(unityObject);
                // }

                if (Event.current.button == 1)
                {
                    if (this.OnRightClick != null)
                    {
                        this.OnRightClick(this);
                    }
                }

                if (Event.current.button == 0)
                {
                    bool toggle = false;

                    if (hasChildren)
                    {
                        if (selected && Event.current.modifiers == EventModifiers.None)
                        {
                            toggle = true;
                        }
                        else if (triangleRect.Contains(Event.current.mousePosition))
                        {
                            toggle = true;
                        }
                    }

                    if (toggle && triangleRect.Contains(Event.current.mousePosition))
                    {
                        var state = !this.Toggled;

                        if (Event.current.modifiers == EventModifiers.Alt)
                        {
                            foreach (var item in this.GetChildMenuItemsRecursive(true))
                            {
                                item.Toggled = state;
                            }
                        }
                        else
                        {
                            this.Toggled = state;
                        }
                    }
                    else
                    {
                        bool shiftSelect =
                            this.menuTree.Selection.SupportsMultiSelect &&
                            Event.current.modifiers == EventModifiers.Shift &&
                            this.menuTree.Selection.Count > 0;

                        if (shiftSelect)
                        {
                            var curr = this.menuTree.Selection.First();
                            var maxIterations = Mathf.Abs(curr.FlatTreeIndex - this.FlatTreeIndex) + 1;
                            var down = curr.FlatTreeIndex < this.FlatTreeIndex;
                            this.menuTree.Selection.Clear();

                            for (int i = 0; i < maxIterations; i++)
                            {
                                if (curr == null)
                                {
                                    break;
                                }

                                curr.Select(true);

                                if (curr == this)
                                {
                                    break;
                                }

                                curr = down ? curr.NextVisualMenuItem : curr.PrevVisualMenuItem;
                            }
                        }
                        else
                        {
                            var ctrl = Event.current.modifiers == EventModifiers.Control;
                            if (ctrl && selected && this.MenuTree.Selection.SupportsMultiSelect)
                            {
                                this.Deselect();
                            }
                            else
                            {
                                this.Select(ctrl);
                            }

                            if (this.MenuTree.Config.ConfirmSelectionOnDoubleClick && Event.current.clickCount == 2)
                            {
                                this.MenuTree.Selection.ConfirmSelection();
                            }
                        }
                    }
                }

                GUIHelper.RemoveFocusControl();
                Event.current.Use();
            }
        }

        internal bool _IsVisible()
        {
            if (this.menuTree.DrawInSearchMode)
            {
                return this.menuTree.FlatMenuTree.Contains(this);
            }

            return this.ParentMenuItemsBottomUp(false).Any(x => x.Toggled == false) == false;
        }

        internal void SetChildMenuItems(List<OdinMenuItem> newChildMenuItems)
        {
            this.childMenuItems = newChildMenuItems ?? new List<OdinMenuItem>();
        }

        private IEnumerable<OdinMenuItem> GetAllNextMenuItems()
        {
            if (this.nextMenuItemFlat != null)
            {
                yield return this.nextMenuItemFlat;

                foreach (var item in this.nextMenuItemFlat.GetAllNextMenuItems())
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<OdinMenuItem> GetAllPreviousMenuItems()
        {
            if (this.previousMenuItemFlat != null)
            {
                yield return this.previousMenuItemFlat;

                foreach (var item in this.previousMenuItemFlat.GetAllPreviousMenuItems())
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<OdinMenuItem> ParentMenuItemsBottomUp(bool includeSelf = true)
        {
            if (this.parentMenuItem != null)
            {
                foreach (var item in this.parentMenuItem.ParentMenuItemsBottomUp())
                {
                    yield return item;
                }
            }

            if (includeSelf)
            {
                yield return this;
            }
        }

        private void EnsureInitialized()
        {
            if (this.isInitialized == false)
            {
                this.menuTree.UpdateMenuTree();

                if (this.isInitialized == false)
                {
                    Debug.LogWarning("Could not initialize menu item. Is the menu item not part of a menu tree?");
                }
            }
        }
    }
}
#endif