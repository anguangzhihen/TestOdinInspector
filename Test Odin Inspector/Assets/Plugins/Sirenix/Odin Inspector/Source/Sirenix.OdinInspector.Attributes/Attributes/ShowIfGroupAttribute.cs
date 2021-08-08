//-----------------------------------------------------------------------
// <copyright file="ShowIfGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <p>ShowIfGroup allows for showing or hiding a group of properties based on a condition.</p>
    /// <p>The attribute is a group attribute and can therefore be combined with other group attributes, and even be used to show or hide entire groups.</p>
    /// <p>Note that in the vast majority of cases where you simply want to be able to control the visibility of a single group, it is better to use the VisibleIf parameter that *all* group attributes have.</p>
    /// </summary>
    /// <seealso cref="ShowIfAttribute"/>
    /// <seealso cref="HideIfAttribute"/>
    /// <seealso cref="HideIfGroupAttribute"/>
    /// <seealso cref="ShowInInspectorAttribute"/>
    /// <seealso cref="UnityEngine.HideInInspector"/>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowIfGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// Whether or not to visually animate group visibility changes. Alias for AnimateVisibility.
        /// </summary>
        public bool Animate { get { return base.AnimateVisibility; } set { base.AnimateVisibility = value; } }

        /// <summary>
        /// The optional member value.
        /// </summary>
        public object Value;

        /// <summary>
        /// Name of member to use when to hide the group. Defaults to the name of the group, by can be overriden by setting this property.
        /// </summary>
        [Obsolete("Use the Condition member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string MemberName { get { return this.Condition; } set { this.Condition = value; } }

        /// <summary>
        /// A resolved string that defines the condition to check the value of, such as a member name or an expression.
        /// </summary>
        public string Condition
        {

            get
            {
                return string.IsNullOrEmpty(base.VisibleIf)
                  ? this.GroupName
                  : base.VisibleIf;
            }
            set { base.VisibleIf = value; }
        }

        /// <summary>
        /// Makes a group that can be shown or hidden based on a condition.
        /// </summary>
        /// <param name="path">The group path.</param>
        /// <param name="animate">If <c>true</c> then a fade animation will be played when the group is hidden or shown.</param>
        public ShowIfGroupAttribute(string path, bool animate = true) : base(path)
        {
            this.Animate = animate;
        }

        /// <summary>
        /// Makes a group that can be shown or hidden based on a condition.
        /// </summary>
        /// <param name="path">The group path.</param>
        /// <param name="value">The value the member should equal for the property to shown.</param>
        /// <param name="animate">If <c>true</c> then a fade animation will be played when the group is hidden or shown.</param>
        public ShowIfGroupAttribute(string path, object value, bool animate = true) : base(path)
        {
            this.Value = value;
            this.Animate = animate;
        }

        /// <summary>
        /// Combines ShowIfGroup attributes.
        /// </summary>
        /// <param name="other">Another ShowIfGroup attribute.</param>
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var attr = other as ShowIfGroupAttribute;

            if (this.Value != null)
            {
                attr.Value = this.Value;
            }
        }
    }
}