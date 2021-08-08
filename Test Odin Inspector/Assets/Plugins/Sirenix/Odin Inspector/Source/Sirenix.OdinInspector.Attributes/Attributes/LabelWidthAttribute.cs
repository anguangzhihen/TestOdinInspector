//-----------------------------------------------------------------------
// <copyright file="LabelWidthAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>LabelWidth is used to change the width of labels for properties.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how LabelText is applied to a few property fields.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
    /// {
    ///		[LabelWidth("3")]
    ///		public int MyInt3;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TitleAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class LabelWidthAttribute : Attribute
    {
        /// <summary>
        /// The new text of the label.
        /// </summary>
        public float Width;

        /// <summary>
        /// Give a property a custom label.
        /// </summary>
        /// <param name="width">The width of the label.</param>
        public LabelWidthAttribute(float width)
        {
            this.Width = width;
        }
    }
}