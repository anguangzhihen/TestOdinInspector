//-----------------------------------------------------------------------
// <copyright file="PropertyGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>Attribute to derive from if you wish to create a new property group type, such as box groups or tab groups.</para>
    /// <note type="note">Note that this attribute has special behaviour for "combining" several attributes into one, as one group,
    /// may be declared across attributes in several members, completely out of order. See <see cref="PropertyGroupAttribute.CombineValuesWith(PropertyGroupAttribute)"/>.</note>
    /// </summary>
    /// <remarks>
    /// <para>All group attributes for a group with the same name (and of the same attribute type) are combined into a single representative group attribute using the <see cref="PropertyGroupAttribute.CombineValuesWith(PropertyGroupAttribute)"/> method, which is called by the <see cref="PropertyGroupAttribute.Combine(PropertyGroupAttribute)"/> method.</para>
    /// <para>This behaviour is a little unusual, but it is important that you understand it when creating groups with many custom parameters that may have to be combined.</para>
    /// </remarks>
    /// <example>
    /// <para>This example shows how <see cref="BoxGroupAttribute"/> could be implemented.</para>
    /// <code>
    /// [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    /// public class BoxGroupAttribute : PropertyGroupAttribute
    /// {
    ///     public string Label { get; private set; }
    ///     public bool ShowLabel { get; private set; }
    ///     public bool CenterLabel { get; private set; }
    ///
    ///     public BoxGroupAttribute(string group, bool showLabel = true, bool centerLabel = false, float order = 0)
    ///         : base(group, order)
    ///     {
    ///         this.Label = group;
    ///         this.ShowLabel = showLabel;
    ///         this.CenterLabel = centerLabel;
    ///     }
    ///
    ///     protected override void CombineValuesWith(PropertyGroupAttribute other)
    ///     {
    ///         // The given attribute parameter is *guaranteed* to be of type BoxGroupAttribute.
    ///         var attr = other as BoxGroupAttribute;
    ///
    ///         // If this attribute has no label, we the other group's label, thus preserving the label across combines.
    ///         if (this.Label == null)
    ///         {
    ///             this.Label = attr.Label;
    ///         }
    ///
    ///         // Combine ShowLabel and CenterLabel parameters.
    ///         this.ShowLabel |= attr.ShowLabel;
    ///         this.CenterLabel |= attr.CenterLabel;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="BoxGroupAttribute"/>
    /// <seealso cref="ButtonGroupAttribute"/>
    /// <seealso cref="FoldoutGroupAttribute"/>
    /// <seealso cref="TabGroupAttribute"/>
    /// <seealso cref="ToggleGroupAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public abstract class PropertyGroupAttribute : Attribute
    {
        /// <summary>
        /// The ID used to grouping properties together.
        /// </summary>
        public string GroupID;

        /// <summary>
        /// The name of the group. This is the last part of the group ID if there is a path, otherwise it is just the group ID.
        /// </summary>
        public string GroupName;

        /// <summary>
        /// The order of the group.
        /// </summary>
        public float Order;

        /// <summary>
        /// Whether to hide the group by default when all its children are not visible. True by default.
        /// </summary>
        public bool HideWhenChildrenAreInvisible = true;

        /// <summary>
        /// If not null, this resolved string controls the group's visibility. Note that if <see cref="HideWhenChildrenAreInvisible"/> is true, there must be *both* a visible child *and* this condition must be true, before the group is shown.
        /// </summary>
        public string VisibleIf;

        /// <summary>
        /// Whether to animate the visibility changes of this group or make the visual transition instantly. True by default.
        /// </summary>
        public bool AnimateVisibility = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGroupAttribute"/> class.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="order">The group order.</param>
        public PropertyGroupAttribute(string groupId, float order)
        {
            this.GroupID = groupId;
            this.Order = order;

            var index = groupId.LastIndexOf('/');

            this.GroupName = index >= 0 && index < groupId.Length ? groupId.Substring(index + 1) : groupId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGroupAttribute"/> class.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        public PropertyGroupAttribute(string groupId)
            : this(groupId, 0)
        {
        }

        /// <summary>
        /// <para>Combines this attribute with another attribute of the same type. 
        /// This method invokes the virtual <see cref="CombineValuesWith(PropertyGroupAttribute)"/> method to invoke custom combine logic.</para>
        /// <para>All group attributes are combined to one attribute used by a single OdinGroupDrawer.</para> 
        /// <para>Example: <code>protected override void CombineValuesWith(PropertyGroupAttribute other) { this.Title = this.Title ?? (other as MyGroupAttribute).Title; }</code></para> 
        /// </summary>
        /// <param name="other">The attribute to combine with.</param>
        /// <returns>The instance that the method was invoked on.</returns>
        /// <exception cref="System.ArgumentNullException">The argument 'other' was null.</exception>
        /// <exception cref="System.ArgumentException">
        /// Attributes to combine are not of the same type.
        /// or
        /// PropertyGroupAttributes to combine must have the same group id.
        /// </exception>
        public PropertyGroupAttribute Combine(PropertyGroupAttribute other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.GetType() != this.GetType())
            {
                throw new ArgumentException("Attributes to combine are not of the same type.");
            }

            if (other.GroupID != this.GroupID)
            {
                throw new ArgumentException("PropertyGroupAttributes to combine must have the same group id.");
            }

            if (this.Order == 0)
            {
                this.Order = other.Order;
            }
            else if (other.Order != 0)
            {
                this.Order = Math.Min(this.Order, other.Order);
            }

            this.HideWhenChildrenAreInvisible &= other.HideWhenChildrenAreInvisible;

            if (this.VisibleIf == null)
            {
                this.VisibleIf = other.VisibleIf;
            }

            this.AnimateVisibility &= other.AnimateVisibility;

            this.CombineValuesWith(other);
            return this;
        }

        /// <summary>
        /// <para>Override this method to add custom combine logic to your group attribute. This method determines how your group's parameters combine when spread across multiple attribute declarations in the same class.</para>
        /// <para>Remember, in .NET, member order is not guaranteed, so you never know which order your attributes will be combined in.</para>
        /// </summary>
        /// <param name="other">The attribute to combine with. This parameter is guaranteed to be of the correct attribute type.</param>
        /// <example>
        /// <para>This example shows how <see cref="BoxGroupAttribute"/> attributes are combined.</para>
        /// <code>
        /// protected override void CombineValuesWith(PropertyGroupAttribute other)
        /// {
        ///     // The given attribute parameter is *guaranteed* to be of type BoxGroupAttribute.
        ///     var attr = other as BoxGroupAttribute;
        ///
        ///     // If this attribute has no label, we the other group's label, thus preserving the label across combines.
        ///     if (this.Label == null)
        ///     {
        ///         this.Label = attr.Label;
        ///     }
        ///
        ///     // Combine ShowLabel and CenterLabel parameters.
        ///     this.ShowLabel |= attr.ShowLabel;
        ///     this.CenterLabel |= attr.CenterLabel;
        /// }
        /// </code>
        /// </example>
        protected virtual void CombineValuesWith(PropertyGroupAttribute other)
        {
        }
    }
}