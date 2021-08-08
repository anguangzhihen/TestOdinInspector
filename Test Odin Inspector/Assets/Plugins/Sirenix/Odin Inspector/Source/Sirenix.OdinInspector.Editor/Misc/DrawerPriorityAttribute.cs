#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerPriorityAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
	/// <para>DrawerPriority is used on inspector drawers and indicates the priority of the drawer.</para>
	/// <para>Use this to make your custom drawer to come before or after other drawers, and potentially hide other drawers.</para>
    /// </summary>
	/// <example>
	/// <para>The following example shows how DrawerPriority could be apply to a value drawer.</para>
    /// <code>
	/// [DrawerPriority(DrawerPriorityLevel.ValuePriority)]
	///
    ///	public sealed class MyIntDrawer : InspectorValuePropertyDrawer&lt;int&gt;
    ///	{
	///		// ...
	///	}
	/// </code>
    /// </example>
	/// <example>
	/// <para>The following example shows how DrawerPriority is used to mark a custom int drawer as a high priority drawer.</para>
    /// <code>
	/// [DrawerPriority(1, 0, 0)]
	///
    ///	public sealed class MySpecialIntDrawer : InspectorValuePropertyDrawer&lt;int&gt;
    ///	{
	///		// ...
	///	}
	/// </code>
    /// </example>
	/// <seealso cref="DrawerPriority"/>
	/// <seealso cref="DrawerPriorityLevel"/>
    [AttributeUsage(AttributeTargets.Class)]
    public class DrawerPriorityAttribute : Attribute
    {
        /// <summary>
        /// The priority of the drawer.
        /// </summary>
        public DrawerPriority Priority { get; private set; }

        /// <summary>
        /// Indicates the priority of an inspector drawer.
        /// </summary>
        /// <param name="priority">Option for priority for the inspector drawer.</param>
        public DrawerPriorityAttribute(DrawerPriorityLevel priority)
        {
            this.Priority = new DrawerPriority(priority);
        }

        /// <summary>
        /// Indicates the priority of an inspector drawer.
        /// </summary>
        /// <param name="super">
        /// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
        /// These drawers typically don't draw the property itself, and calls CallNextDrawer.</param>
        /// <param name="wrapper">The wrapper priority. Mostly used by drawers used to decorate properties.</param>
        /// <param name="value">The value priority. Mostly used by <see cref="OdinValueDrawer{T}"/>s and <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>s.</param>
        public DrawerPriorityAttribute(double super = 0, double wrapper = 0, double value = 0)
        {
            this.Priority = new DrawerPriority(super, wrapper, value);
        }
    }
}
#endif