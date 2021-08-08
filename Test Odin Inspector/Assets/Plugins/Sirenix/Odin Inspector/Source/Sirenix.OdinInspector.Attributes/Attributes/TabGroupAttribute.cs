//-----------------------------------------------------------------------
// <copyright file="TabGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Internal;

    /// <summary>
    /// <para>TabGroup is used on any property, and organizes properties into different tabs.</para>
    /// <para>Use this to organize different value to make a clean and easy to use inspector.</para>
    /// </summary>
	/// <remarks>
    /// <para>Use groups to create multiple tab groups, each with multiple tabs and even sub tabs.</para>
    /// </remarks>
	/// <example>
	/// <para>The following example shows how to create a tab group with two tabs.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	///	{
	///		[TabGroup("First")]
	///		public int MyFirstInt;
	///
	///		[TabGroup("First")]
	///		public int AnotherInt;
	///
	///		[TabGroup("Second")]
	///		public int MySecondInt;
	///	}
    /// </code>
    /// </example>
	/// <example>
	/// <para>The following example shows how multiple groups of tabs can be created.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[TabGroup("A", "FirstGroup")]
	///		public int FirstGroupA;
	///
	///		[TabGroup("B", "FirstGroup")]
	///		public int FirstGroupB;
	///
	///		// The second tab group has been configured to have constant height across all tabs.
	///		[TabGroup("A", "SecondGroup", true)]
	///		public int SecondgroupA;
	///
	///		[TabGroup("B", "SecondGroup")]
	///		public int SecondGroupB;
	///
	///		[TabGroup("B", "SecondGroup")]
	///		public int AnotherInt;
	///	}
	/// </code>
    /// </example>
    /// <example>
    /// <para>This example demonstrates how multiple tabs groups can be combined to create tabs in tabs.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [TabGroup("ParentGroup", "First Tab")]
    ///     public int A;
    ///     
    ///     [TabGroup("ParentGroup", "Second Tab")]
    ///     public int B;
    ///     
    ///     // Specify 'First Tab' as a group, and another child group to the 'First Tab' group.
    ///     [TabGroup("ParentGroup/First Tab/InnerGroup", "Inside First Tab A")]
    ///     public int C;
    ///     
    ///     [TabGroup("ParentGroup/First Tab/InnerGroup", "Inside First Tab B")]
    ///     public int D;
    ///     
    ///     [TabGroup("ParentGroup/Second Tab/InnerGroup", "Inside Second Tab")]
    ///     public int E;
    /// }
    /// </code>
    /// </example>
	/// <seealso cref="TabListAttribute"/>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class TabGroupAttribute : PropertyGroupAttribute, ISubGroupProviderAttribute
    {
        /// <summary>
        /// The default tab group name which is used when the single-parameter constructor is called.
        /// </summary>
        public const string DEFAULT_NAME = "_DefaultTabGroup";

        /// <summary>
        /// Name of the tab.
        /// </summary>
        public string TabName;

        /// <summary>
        /// Should this tab be the same height as the rest of the tab group.
        /// </summary>
        public bool UseFixedHeight;

        /// <summary>
        /// If true, the content of each page will not be contained in any box.
        /// </summary>
        public bool Paddingless;

        /// <summary>
        /// If true, the tab group will be hidden if it only contains one tab.
        /// </summary>
        public bool HideTabGroupIfTabGroupOnlyHasOneTab;

        /// <summary>
        /// Organizes the property into the specified tab in the default group.
        /// Default group name is '_DefaultTabGroup'
        /// </summary>
        /// <param name="tab">The tab.</param>
        /// <param name="useFixedHeight">if set to <c>true</c> [use fixed height].</param>
        /// <param name="order">The order.</param>
        public TabGroupAttribute(string tab, bool useFixedHeight = false, float order = 0)
            : this(DEFAULT_NAME, tab, useFixedHeight, order)
        { }

        /// <summary>
        /// Organizes the property into the specified tab in the specified group.
        /// </summary>
        /// <param name="group">The group to attach the tab to.</param>
        /// <param name="tab">The name of the tab.</param>
        /// <param name="useFixedHeight">Set to true to have a constant height across the entire tab group.</param>
        /// <param name="order">The order of the group.</param>
        public TabGroupAttribute(string group, string tab, bool useFixedHeight = false, float order = 0)
            : base(group, order)
        {
            this.TabName = tab;
            this.UseFixedHeight = useFixedHeight;

            this.Tabs = new List<string>();
            if (tab != null)
            {
                this.Tabs.Add(tab);
            }

            this.Tabs = new List<string>(this.Tabs);
        }

        /// <summary>
        /// Name of all tabs in this group.
        /// </summary>
        public List<string> Tabs { get; private set; }

        /// <summary>
        /// Combines the tab group with another group.
        /// </summary>
        /// <param name="other">The other group.</param>
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            base.CombineValuesWith(other);

            var otherTab = other as TabGroupAttribute;
            if (otherTab.TabName != null)
            {
                this.UseFixedHeight = this.UseFixedHeight || otherTab.UseFixedHeight;
                this.Paddingless = this.Paddingless || otherTab.Paddingless;
                this.HideTabGroupIfTabGroupOnlyHasOneTab = this.HideTabGroupIfTabGroupOnlyHasOneTab || otherTab.HideTabGroupIfTabGroupOnlyHasOneTab;

                if (this.Tabs.Contains(otherTab.TabName) == false)
                {
                    this.Tabs.Add(otherTab.TabName);
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <returns>Not yet documented.</returns>
        IList<PropertyGroupAttribute> ISubGroupProviderAttribute.GetSubGroupAttributes()
        {
            int count = 0;

            List<PropertyGroupAttribute> result = new List<PropertyGroupAttribute>(this.Tabs.Count);

            foreach (var tab in this.Tabs)
            {
                result.Add(new TabSubGroupAttribute(this.GroupID + "/" + tab, count++));
            }

            return result;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <returns>Not yet documented.</returns>
        string ISubGroupProviderAttribute.RepathMemberAttribute(PropertyGroupAttribute attr)
        {
            var tabAttr = (TabGroupAttribute)attr;
            return this.GroupID + "/" + tabAttr.TabName;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public class TabSubGroupAttribute : PropertyGroupAttribute
        {
            public TabSubGroupAttribute(string groupId, float order) : base(groupId, order)
            {
            }
        }
    }
}