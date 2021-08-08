#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerPriorityLevel.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// <para>
    /// DrawerPriorityLevel is used in conjunction with <see cref="DrawerPriority"/>.
    /// </para>
    /// </summary>
    /// <seealso cref="DrawerPriority"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    public enum DrawerPriorityLevel
    {
        /// <summary>
        /// Auto priority is defined by setting all of the components to zero.
        /// If no <see cref="DrawerPriorityAttribute"/> is defined on a drawer, it will default to AutoPriority.
        /// </summary>
        AutoPriority,

        /// <summary>
        /// The value priority. Mostly used by <see cref="OdinValueDrawer{T}"/>s.
        /// </summary>
        ValuePriority,

        /// <summary>
        /// The attribute priority. Mostly used by <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>s.
        /// </summary>
        AttributePriority,

        /// <summary>
        /// The wrapper priority. Mostly used by drawers used to decorate properties.
        /// </summary>
        WrapperPriority,

        /// <summary>
        /// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
        /// These drawers typically don't draw the property itself, and calls CallNextDrawer.
        /// </summary>
        SuperPriority
    }
}
#endif