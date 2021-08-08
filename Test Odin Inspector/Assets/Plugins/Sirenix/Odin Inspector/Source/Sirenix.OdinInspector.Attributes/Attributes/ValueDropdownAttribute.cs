//-----------------------------------------------------------------------
// <copyright file="ValueDropdownAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// <para>ValueDropdown is used on any property and creates a dropdown with configurable options.</para>
    /// <para>Use this to give the user a specific set of options to select from.</para>
    /// </summary>
    /// <remarks>
    /// <note type="note">Due to a bug in Unity, enums will sometimes not work correctly. The last example shows how this can be fixed.</note>
    /// </remarks>
    /// <example>
    /// <para>The following example shows a how the ValueDropdown can be used on an int property.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[ValueDropdown("myValues")]
    ///		public int MyInt;
    ///
    ///		// The selectable values for the dropdown.
    ///		private int[] myValues = { 1, 2, 3 };
    ///	}
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following example shows how ValueDropdownList can be used for objects, that do not implement a usable ToString.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[ValueDropdown("myVectorValues")]
    ///		public Vector3 MyVector;
    ///
    ///		// The selectable values for the dropdown, with custom names.
    ///		private ValueDropdownList&lt;Vector3&gt; myVectorValues = new ValueDropdownList&lt;Vector3&gt;()
    ///		{
    ///			{"Forward",	Vector3.forward	},
    ///			{"Back",	Vector3.back	},
    ///			{"Up",		Vector3.up		},
    ///			{"Down",	Vector3.down	},
    ///			{"Right",	Vector3.right	},
    ///			{"Left",	Vector3.left	},
    ///		};
    /// }
    /// </code>
    /// </example>
    ///	<example>
    ///	<para>The following example shows how the ValueDropdown can on any member that implements IList.</para>
    ///	<code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		// Member field of type float[].
    ///		private float[] valuesField;
    ///
    ///		[ValueDropdown("valuesField")]
    ///		public float MyFloat;
    ///
    ///		// Member property of type List&lt;thing&gt;.
    ///		private List&lt;string&gt; ValuesProperty { get; set; }
    ///
    ///		[ValueDropdown("ValuesProperty")]
    ///		public string MyString;
    ///
    ///		// Member function that returns an object of type IList.
    ///		private IList&lt;ValueDropdownItem&lt;int&gt;&gt; ValuesFunction()
    ///		{
    ///			return new ValueDropdownList&lt;int&gt;
    ///			{
    ///				{ "The first option",	1 },
    ///				{ "The second option",	2 },
    ///				{ "The third option",	3 },
    ///			};
    ///		}
    ///
    ///		[ValueDropdown("ValuesFunction")]
    ///		public int MyInt;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>Due to a bug in Unity, enums member arrays will in some cases appear as empty. This example shows how you can get around that.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		// Make the field static.
    ///		private static MyEnum[] MyStaticEnumArray = MyEnum[] { ... };
    ///
    ///		// Force Unity to serialize the field, and hide the property from the inspector.
    ///		[SerializeField, HideInInspector]
    ///		private MyEnum MySerializedEnumArray = MyEnum[] { ... };
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ValueDropdownList{T}"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ValueDropdownAttribute : Attribute
    {
        /// <summary>
        /// Name of any field, property or method member that implements IList. E.g. arrays or Lists. Obsolete; use the ValuesGetter member instead.
        /// </summary>
        [Obsolete("Use the ValuesGetter member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string MemberName { get { return this.ValuesGetter; } set { this.ValuesGetter = value; } }

        /// <summary>
        /// A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.
        /// </summary>
        public string ValuesGetter;

        /// <summary>
        /// The number of items before enabling search. Default is 10.
        /// </summary>
        public int NumberOfItemsBeforeEnablingSearch;

        /// <summary>
        /// False by default.
        /// </summary>
        public bool IsUniqueList;

        /// <summary>
        /// True by default. If the ValueDropdown attribute is applied to a list, then disabling this,
        /// will render all child elements normally without using the ValueDropdown. The ValueDropdown will
        /// still show up when you click the add button on the list drawer, unless <see cref="DisableListAddButtonBehaviour"/> is true.
        /// </summary>
        public bool DrawDropdownForListElements;

        /// <summary>
        /// False by default.
        /// </summary>
        public bool DisableListAddButtonBehaviour;

        /// <summary>
        /// If the ValueDropdown attribute is applied to a list, and <see cref="IsUniqueList"/> is set to true, then enabling this,
        /// will exclude existing values, instead of rendering a checkbox indicating whether the item is already included or not.
        /// </summary>
        public bool ExcludeExistingValuesInList;

        /// <summary>
        /// If the dropdown renders a tree-view, then setting this to true will ensure everything is expanded by default.
        /// </summary>
        public bool ExpandAllMenuItems;

        /// <summary>
        /// If true, instead of replacing the drawer with a wide dropdown-field, the dropdown button will be a little button, drawn next to the other drawer.
        /// </summary>
        public bool AppendNextDrawer;

        /// <summary>
        /// Disables the the GUI for the appended drawer. False by default.
        /// </summary>
        public bool DisableGUIInAppendedDrawer;

        /// <summary>
        /// By default, a single click selects and confirms the selection.
        /// </summary>
        public bool DoubleClickToConfirm;

        /// <summary>
        /// By default, the dropdown will create a tree view.
        /// </summary>
        public bool FlattenTreeView;

        /// <summary>
        /// Gets or sets the width of the dropdown. Default is zero.
        /// </summary>
        public int DropdownWidth;

        /// <summary>
        /// Gets or sets the height of the dropdown. Default is zero.
        /// </summary>
        public int DropdownHeight;

        /// <summary>
        /// Gets or sets the title for the dropdown. Null by default.
        /// </summary>
        public string DropdownTitle;

        /// <summary>
        /// False by default.
        /// </summary>
        public bool SortDropdownItems;

        /// <summary>
        /// Whether to draw all child properties in a foldout.
        /// </summary>
        public bool HideChildProperties = false;

        // Experimental feature, out-commented code for it should be in the drawer.
        // public bool InlineSelector;

        /// <summary>
        /// Creates a dropdown menu for a property.
        /// </summary>
        /// <param name="valuesGetter">A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.</param>
        public ValueDropdownAttribute(string valuesGetter)
        {
            this.NumberOfItemsBeforeEnablingSearch = 10;
            this.ValuesGetter = valuesGetter;
            this.DrawDropdownForListElements = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IValueDropdownItem
    {
        /// <summary>
        /// Gets the label for the dropdown item.
        /// </summary>
        /// <returns>The label text for the item.</returns>
        string GetText();

        /// <summary>
        /// Gets the value of the dropdown item.
        /// </summary>
        /// <returns>The value for the item.</returns>
        object GetValue();
    }

    /// <summary>
    /// Use this with <see cref="ValueDropdownAttribute"/> to specify custom names for values.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class ValueDropdownList<T> : List<ValueDropdownItem<T>>
    {
        /// <summary>
        /// Adds the specified value with a custom name.
        /// </summary>
        /// <param name="text">The name of the item.</param>
        /// <param name="value">The value.</param>
        public void Add(string text, T value)
        {
            this.Add(new ValueDropdownItem<T>(text, value));
        }

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(T value)
        {
            this.Add(new ValueDropdownItem<T>(value.ToString(), value));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ValueDropdownItem : IValueDropdownItem
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        public string Text;

        /// <summary>
        /// The value of the item.
        /// </summary>
        public object Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueDropdownItem{T}" /> class.
        /// </summary>
        /// <param name="text">The text to display for the dropdown item.</param>
        /// <param name="value">The value for the dropdown item.</param>
        public ValueDropdownItem(string text, object value)
        {
            this.Text = text;
            this.Value = value;
        }

        /// <summary>
        /// The name of this item.
        /// </summary>
        public override string ToString()
        {
            return this.Text ?? (this.Value + "");
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        string IValueDropdownItem.GetText()
        {
            return this.Text;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        object IValueDropdownItem.GetValue()
        {
            return this.Value;
        }

        // Custom equality comparisons are done in the drawer.
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ValueDropdownItem<T> : IValueDropdownItem
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        public string Text;

        /// <summary>
        /// The value of the item.
        /// </summary>
        public T Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueDropdownItem{T}" /> class.
        /// </summary>
        /// <param name="text">The text to display for the dropdown item.</param>
        /// <param name="value">The value for the dropdown item.</param>
        public ValueDropdownItem(string text, T value)
        {
            this.Text = text;
            this.Value = value;
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        string IValueDropdownItem.GetText()
        {
            return this.Text;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        object IValueDropdownItem.GetValue()
        {
            return this.Value;
        }

        /// <summary>
        /// The name of this item.
        /// </summary>
        public override string ToString()
        {
            return this.Text ?? (this.Value + "");
        }

        // Custom equality comparisons are done in the drawer.        
    }
}