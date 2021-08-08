//-----------------------------------------------------------------------
// <copyright file="ListDrawerSettingsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Customize the behavior for lists and arrays in the inspector.
    /// </summary>
    /// <example>
    /// <para>This example shows how you can add your own custom add button to a list.</para>
    /// <code>
    /// [ListDrawerSettings(HideAddButton = true, OnTitleBarGUI = "DrawTitleBarGUI")]
    /// public List&lt;MyType&gt; SomeList;
    ///
    /// #if UNITY_EDITOR
    /// private void DrawTitleBarGUI()
    /// {
    ///     if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
    ///     {
    ///         this.SomeList.Add(new MyType());
    ///     }
    /// }
    /// #endif
    /// </code>
    /// </example>
    /// <remarks>
    /// This attribute is scheduled for refactoring.
    /// </remarks>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [DontApplyToListElements]
    public sealed class ListDrawerSettingsAttribute : Attribute
    {
        /// <summary>
        /// If true, the add button will not be rendered in the title toolbar. You can use OnTitleBarGUI to implement your own add button.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide add button]; otherwise, <c>false</c>.
        /// </value>
        public bool HideAddButton;

        /// <summary>
        /// If true, the remove button  will not be rendered on list items. You can use OnBeginListElementGUI and OnEndListElementGUI to implement your own remove button.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [hide remove button]; otherwise, <c>false</c>.    
        /// </value>
        public bool HideRemoveButton;

        /// <summary>
        /// Specify the name of a member inside each list element which defines the label being drawn for each list element.
        /// </summary>
        public string ListElementLabelName;

        /// <summary>
        /// Override the default behaviour for adding objects to the list. 
        /// If the referenced member returns the list type element, it will be called once per selected object.
        /// If the referenced method returns void, it will only be called once regardless of how many objects are selected.
        /// </summary>
        public string CustomAddFunction;

        public string CustomRemoveIndexFunction;

        public string CustomRemoveElementFunction;

        /// <summary>
        /// Calls a method before each list element. The member referenced must have a return type of void, and an index parameter of type int which represents the element index being drawn.
        /// </summary>
        public string OnBeginListElementGUI;

        /// <summary>
        /// Calls a method after each list element. The member referenced must have a return type of void, and an index parameter of type int which represents the element index being drawn.
        /// </summary>
        public string OnEndListElementGUI;

        /// <summary>
        /// If true, object/type pickers will never be shown when the list add button is clicked, and default(T) will always be added instantly instead, where T is the element type of the list.
        /// </summary>
        public bool AlwaysAddDefaultValue;

        /// <summary>
        /// Whether adding a new element should copy the last element. False by default.
        /// </summary>
        public bool AddCopiesLastElement = false;

        private string onTitleBarGUI = null;
        private int numberOfItemsPerPage;
        private bool paging;
        private bool draggable;
        private bool isReadOnly;
        private bool showItemCount;
        private bool pagingHasValue = false;
        private bool draggableHasValue = false;
        private bool isReadOnlyHasValue = false;
        private bool showItemCountHasValue = false;
        private bool expanded = false;
        private bool expandedHasValue = false;
        private bool numberOfItemsPerPageHasValue = false;
        private bool showIndexLabels = false;
        private bool showIndexLabelsHasValue = false;

        /// <summary>
        /// Override the default setting specified in the Advanced Odin Preferences window and explicitly tell whether paging should be enabled or not.
        /// </summary>
        public bool ShowPaging
        {
            get
            {
                return this.paging;
            }

            set
            {
                this.paging = value;
                this.pagingHasValue = true;
            }
        }

        /// <summary>
        /// Override the default setting specified in the Advanced Odin Preferences window and explicitly tell whether items should be draggable or not.
        /// </summary>
        public bool DraggableItems
        {
            get { return this.draggable; }
            set
            {
                this.draggable = value;
                this.draggableHasValue = true;
            }
        }

        /// <summary>
        /// Override the default setting specified in the Advanced Odin Preferences window and explicitly tells how many items each page should contain.
        /// </summary>
        public int NumberOfItemsPerPage
        {
            get { return this.numberOfItemsPerPage; }
            set
            {
                this.numberOfItemsPerPage = value;
                this.numberOfItemsPerPageHasValue = true;
            }
        }

        /// <summary>
        /// Mark a list as read-only. This removes all editing capabilities from the list such as Add, Drag and delete,
        /// but without disabling GUI for each element drawn as otherwise would be the case if the <see cref="ReadOnlyAttribute"/> was used.
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
            set
            {
                this.isReadOnly = value;
                this.isReadOnlyHasValue = true;
            }
        }

        /// <summary>
        /// Override the default setting specified in the Advanced Odin Preferences window and explicitly tell whether or not item count should be shown.
        /// </summary>
        public bool ShowItemCount
        {
            get { return this.showItemCount; }
            set
            {
                this.showItemCount = value;
                this.showItemCountHasValue = true;
            }
        }

        /// <summary>
        /// Override the default setting specified in the Advanced Odin Preferences window and explicitly tell whether or not the list should be expanded or collapsed by default.
        /// </summary>
        public bool Expanded
        {
            get { return this.expanded; }
            set
            {
                this.expanded = value;
                this.expandedHasValue = true;
            }
        }

        /// <summary>
        /// If true, a label is drawn for each element which shows the index of the element.
        /// </summary>
        public bool ShowIndexLabels
        {
            get { return this.showIndexLabels; }
            set
            {
                this.showIndexLabels = value;
                this.showIndexLabelsHasValue = true;
            }
        }

        /// <summary>
        /// Use this to inject custom GUI into the title-bar of the list.
        /// </summary>
        public string OnTitleBarGUI
        {
            get { return this.onTitleBarGUI; }
            set { this.onTitleBarGUI = value; }
        }

        /// <summary>
        /// Whether the Paging property is set.
        /// </summary>
        public bool PagingHasValue { get { return this.pagingHasValue; } }

        /// <summary>
        /// Whether the ShowItemCount property is set.
        /// </summary>
        public bool ShowItemCountHasValue { get { return this.showItemCountHasValue; } }

        /// <summary>
        /// Whether the NumberOfItemsPerPage property is set.
        /// </summary>
        public bool NumberOfItemsPerPageHasValue { get { return this.numberOfItemsPerPageHasValue; } }

        /// <summary>
        /// Whether the Draggable property is set.
        /// </summary>
        public bool DraggableHasValue { get { return this.draggableHasValue; } }

        /// <summary>
        /// Whether the IsReadOnly property is set.
        /// </summary>
        public bool IsReadOnlyHasValue { get { return this.isReadOnlyHasValue; } }

        /// <summary>
        /// Whether the Expanded property is set.
        /// </summary>
        public bool ExpandedHasValue { get { return this.expandedHasValue; } }

        /// <summary>
        /// Whether the ShowIndexLabels property is set.
        /// </summary>
        public bool ShowIndexLabelsHasValue { get { return this.showIndexLabelsHasValue; } }
    }
}