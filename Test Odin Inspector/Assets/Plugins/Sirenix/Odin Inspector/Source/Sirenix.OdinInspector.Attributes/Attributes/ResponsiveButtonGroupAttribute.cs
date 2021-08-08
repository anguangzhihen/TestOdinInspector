//-----------------------------------------------------------------------
// <copyright file="ResponsiveButtonGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Groups buttons into a group that will position and resize the buttons based on the amount of available layout space.
    /// </summary>
    /// <example>
    /// <code>
    /// [ResponsiveButtonGroup]
    /// public void Foo() { }
    /// 
    /// [ResponsiveButtonGroup]
    /// public void Bar() { }
    /// 
    /// [ResponsiveButtonGroup]
    /// public void Baz() { }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// [ResponsiveButtonGroup(UniformLayout = true)]
    /// public void Foo() { }
    /// 
    /// [ResponsiveButtonGroup]
    /// public void Bar() { }
    /// 
    /// [ResponsiveButtonGroup]
    /// public void Baz() { }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// [ResponsiveButtonGroupAttribute(UniformLayout = true, DefaultButtonSize = ButtonSizes.Large)]
    /// public void Foo() { }
    /// 
    /// [GUIColor(0, 1, 0))]
    /// [Button(ButtonSizes.Large)]
    /// [ResponsiveButtonGroup]
    /// public void Bar() { }
    /// 
    /// [ResponsiveButtonGroup]
    /// public void Baz() { }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// [TabGroup("SomeTabGroup", "SomeTab")]
    /// [ResponsiveButtonGroup("SomeTabGroup/SomeTab/SomeBtnGroup")]
    /// public void Foo() { }
    /// 
    /// [ResponsiveButtonGroup("SomeTabGroup/SomeTab/SomeBtnGroup")]
    /// public void Bar() { }
    /// 
    /// [ResponsiveButtonGroup("SomeTabGroup/SomeTab/SomeBtnGroup")]
    /// public void Baz() { }
    /// </code>
    /// </example>
    [IncludeMyAttributes, ShowInInspector]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ResponsiveButtonGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// The default size of the button.
        /// </summary>
        public ButtonSizes DefaultButtonSize = ButtonSizes.Medium;

        /// <summary>
        /// If <c>true</c> then the widths of a line of buttons will be the same.
        /// </summary>
        public bool UniformLayout = false;

        /// <summary>
        /// Draws a button that will be placed in a group that will respond to the horizontal space available to the group.
        /// </summary>
        /// <param name="group">The name of the group to place the button in.</param>
        public ResponsiveButtonGroupAttribute(string group = "_DefaultResponsiveButtonGroup")
            : base(group)
        {
        }

        /// <summary>
        /// Merges the values of this group with another ResponsiveButtonGroupAttribute.
        /// </summary>
        /// <param name="other">The attribute to combine with.</param>
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var otherAttr = other as ResponsiveButtonGroupAttribute;
            if (other == null)
            {
                return;
            }

            if (otherAttr.DefaultButtonSize != ButtonSizes.Medium)
            {
                this.DefaultButtonSize = otherAttr.DefaultButtonSize;
            }
            else if (this.DefaultButtonSize != ButtonSizes.Medium)
            {
                otherAttr.DefaultButtonSize = this.DefaultButtonSize;
            }

            this.UniformLayout = this.UniformLayout || otherAttr.UniformLayout;
        }
    }
}