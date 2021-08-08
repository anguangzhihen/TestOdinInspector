//-----------------------------------------------------------------------
// <copyright file="PropertySpaceAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>The PropertySpace attribute have the same function as Unity's existing Space attribute, but can be applied anywhere as opposed to just fields.</para>
    /// </summary>
    /// <example>
    /// <para>The following example demonstrates the usage of the PropertySpace attribute.</para>
    /// <code>
    /// [PropertySpace] // Defaults to a space of 8 pixels just like Unity's Space attribute.
    /// public int MyField;
    /// 
    /// [ShowInInspector, PropertySpace(16)]
    /// public int MyProperty { get; set; }
    /// 
    /// [ShowInInspector, PropertySpace(16, 16)]
    /// public int MyProperty { get; set; }
    /// 
    /// [Button, PropertySpace(32)]
    /// public void MyMethod()
    /// {
    ///     ...
    /// }
    /// 
    /// [PropertySpace(-8)] // A negative space can also be remove existing space between properties.
    /// public int MovedUp;
    /// </code>
    /// </example>
    /// <seealso cref="ShowInInspectorAttribute"/>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="PropertyTooltipAttribute"/>
    /// <seealso cref="PropertyOrderAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true), DontApplyToListElements]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class PropertySpaceAttribute : Attribute
    {
        /// <summary>
        /// The space between properties in pixels.
        /// </summary>
        public float SpaceBefore;

        /// <summary>
        /// The space between properties in pixels.
        /// </summary>
        public float SpaceAfter;

        /// <summary>
        /// Adds a space of 8 pixels between properties.
        /// </summary>
        public PropertySpaceAttribute()
        {
            this.SpaceBefore = 8f;
            this.SpaceAfter = 0f;
        }

        /// <summary>
        /// Adds a space between properties.
        /// </summary>
        public PropertySpaceAttribute(float spaceBefore)
        {
            this.SpaceBefore = spaceBefore;
            this.SpaceAfter = 0f;
        }

        /// <summary>
        /// Adds a space between properties.
        /// </summary>
        public PropertySpaceAttribute(float spaceBefore, float spaceAfter)
        {
            this.SpaceBefore = spaceBefore;
            this.SpaceAfter = spaceAfter;
        }
    }
}