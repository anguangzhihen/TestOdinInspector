#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTreeDrawingConfig.cs" company="Sirenix IVS">
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

    /// <summary>
    /// The config used by OdinMenuTree to specify which features of the Menu Tree should be used when drawing.
    /// </summary>
    [Serializable]
    public class OdinMenuTreeDrawingConfig
    {
        private Func<OdinMenuItem, bool> searchFunction;

        [SerializeField]
        private OdinMenuStyle menuItemStyle;

        /// <summary>
        /// Gets or sets the default menu item style.
        /// </summary>
        public OdinMenuStyle DefaultMenuStyle
        {
            get
            {
                if (this.menuItemStyle == null)
                {
                    this.menuItemStyle = new OdinMenuStyle();
                }

                return this.menuItemStyle;
            }
            set
            {
                this.menuItemStyle = value;
            }
        }

        /// <summary>
        /// The automatic scroll on selection changed. True by default.
        /// </summary>
        public bool AutoScrollOnSelectionChanged = true;

        /// <summary>
        /// Whether to draw the tree in a scrollable view. True by default.
        /// </summary>
        public bool DrawScrollView = true;

        /// <summary>
        /// Whether to handle keyboard navigation after it's done drawing. True by default.
        /// </summary>
        public bool AutoHandleKeyboardNavigation = true;

        /// <summary>
        /// Whether to draw a searchbar above the menu tree. True by default.
        /// </summary>
        public bool DrawSearchToolbar = true;

        /// <summary>
        /// Whether to the menu items expanded state should be cached. True by default.
        /// </summary>
        public bool UseCachedExpandedStates = true;

        /// <summary>
        /// Whether to automatically set focus on the search bar when the tree is drawn for the first time. True by default.
        /// </summary>
        public bool AutoFocusSearchBar = true;

        /// <summary>
        /// The scroll-view position.
        /// </summary>
        public Vector2 ScrollPos = new Vector2();

        /// <summary>
        /// The search term.
        /// </summary>
        public string SearchTerm = "";

        /// <summary>
        /// The height of the search toolbar.
        /// </summary>
        public int SearchToolbarHeight = 24;

        public bool EXPERIMENTAL_INTERNAL_DrawFlatTreeFastNoLayout;

        /// <summary>
        /// Gets or sets the search function. Null by default.
        /// </summary>
        public Func<OdinMenuItem, bool> SearchFunction
        {
            get { return this.searchFunction; }
            set { this.searchFunction = value; }
        }

        /// <summary>
        /// By default, the MenuTree.Selection is confirmed when menu items are double clicked, 
        /// Set this to false if you don't want that behaviour.
        /// </summary>
        [HideInInspector]
        [Obsolete("Use ConfirmSelectionOnDoubleClick instead.", false)]
        public bool ConfirmSlecectionOnDoubleClick = true;

#pragma warning disable CS0618
        /// <summary>
        /// By default, the MenuTree.Selection is confirmed when menu items are double clicked, 
        /// Set this to false if you don't want that behaviour.
        /// </summary>
        public bool ConfirmSelectionOnDoubleClick
        {
            get { return this.ConfirmSlecectionOnDoubleClick; }
            set { this.ConfirmSlecectionOnDoubleClick = value; }
        }
#pragma warning restore CS0618
    }
}
#endif