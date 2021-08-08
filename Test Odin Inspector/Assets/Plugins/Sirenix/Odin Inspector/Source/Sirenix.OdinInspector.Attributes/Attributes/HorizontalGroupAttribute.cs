//-----------------------------------------------------------------------
// <copyright file="HorizontalGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>HorizontalGroup is used group multiple properties horizontally in the inspector.</para>
    /// <para>The width can either be specified as percentage or pixels.</para>
    /// <para>All values between 0 and 1 will be treated as a percentage.</para>
    /// <para>If the width is 0 the column will be automatically sized.</para>
    /// <para>Margin-left and right can only be specified in pixels.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how three properties have been grouped together horizontally.</para>
    /// <code>
    /// // The width can either be specified as percentage or pixels.
    /// // All values between 0 and 1 will be treated as a percentage.
    /// // If the width is 0 the column will be automatically sized.
    /// // Margin-left and right can only be specified in pixels.
    /// 
    /// public class HorizontalGroupAttributeExamples : MonoBehaviour
    /// {
    ///     [HorizontalGroup]
    ///     public int A;
    /// 
    ///     [HideLabel, LabelWidth (150)]
    ///     [HorizontalGroup(150)]
    ///     public LayerMask B;
    /// 
    ///     // LabelWidth can be helpfull when dealing with HorizontalGroups.
    ///     [HorizontalGroup("Group 1"), LabelWidth(15)]
    ///     public int C;
    /// 
    ///     [HorizontalGroup("Group 1"), LabelWidth(15)]
    ///     public int D;
    /// 
    ///     [HorizontalGroup("Group 1"), LabelWidth(15)]
    ///     public int E;
    /// 
    ///     // Having multiple properties in a column can be achived using multiple groups. Checkout the "Combining Group Attributes" example.
    ///     [HorizontalGroup("Split", 0.5f, PaddingRight = 15)]
    ///     [BoxGroup("Split/Left"), LabelWidth(15)]
    ///     public int L;
    /// 
    ///     [BoxGroup("Split/Right"), LabelWidth(15)]
    ///     public int M;
    /// 
    ///     [BoxGroup("Split/Left"), LabelWidth(15)]
    ///     public int N;
    /// 
    ///     [BoxGroup("Split/Right"), LabelWidth(15)]
    ///     public int O;
    /// 
    ///     // Horizontal Group also has supprot for: Title, MarginLeft, MarginRight, PaddingLeft, PaddingRight, MinWidth and MaxWidth.
    ///     [HorizontalGroup("MyButton", MarginLeft = 0.25f, MarginRight = 0.25f)]
    ///     public void SomeButton()
    ///     {
    /// 
    ///     }
    /// }
    /// </code>
    /// </example>
	/// <seealso cref="VerticalGroupAttribute"/>
	/// <seealso cref="BoxGroupAttribute"/>
	/// <seealso cref="TabGroupAttribute"/>
	/// <seealso cref="ToggleGroupAttribute"/>
	/// <seealso cref="ButtonGroupAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class HorizontalGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// The width. Values between 0 and 1 will be treated as percentage, 0 = auto, otherwise pixels.
        /// </summary>
        public float Width;

        /// <summary>
        /// The margin left. Values between 0 and 1 will be treated as percentage, 0 = ignore, otherwise pixels.
        /// </summary>
        public float MarginLeft;

        /// <summary>
        /// The margin right. Values between 0 and 1 will be treated as percentage, 0 = ignore, otherwise pixels.
        /// </summary>
        public float MarginRight;

        /// <summary>
        /// The padding left. Values between 0 and 1 will be treated as percentage, 0 = ignore, otherwise pixels.
        /// </summary>
        public float PaddingLeft;

        /// <summary>
        /// The padding right. Values between 0 and 1 will be treated as percentage, 0 = ignore, otherwise pixels.
        /// </summary>
        public float PaddingRight;

        /// <summary>
        /// The minimum Width. Values between 0 and 1 will be treated as percentage, 0 = ignore, otherwise pixels.
        /// </summary>
        public float MinWidth;

        /// <summary>
        /// The maximum Width. Values between 0 and 1 will be treated as percentage, 0 = ignore, otherwise pixels.
        /// </summary>
        public float MaxWidth;

        /// <summary>
        /// Adds a title above the horizontal group.
        /// </summary>
        public string Title;

        /// <summary>
        /// The label width, 0 = auto.
        /// </summary>
        public float LabelWidth;

        /// <summary>
        /// Organizes the property in a horizontal group.
        /// </summary>
        /// <param name="group">The group for the property.</param>
        /// <param name="width">The width of the property. Values between 0 and 1 are interpolated as a percentage, otherwise pixels.</param>
        /// <param name="marginLeft">The left margin in pixels.</param>
        /// <param name="marginRight">The right margin in pixels.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        public HorizontalGroupAttribute(string group, float width = 0, int marginLeft = 0, int marginRight = 0, float order = 0)
            : base(group, order)
        {
            this.Width = width;
            this.MarginLeft = marginLeft;
            this.MarginRight = marginRight;
        }

        /// <summary>
        /// Organizes the property in a horizontal group.
        /// </summary>
        /// <param name="width">The width of the property. Values between 0 and 1 are interpolated as a percentage, otherwise pixels.</param>
        /// <param name="marginLeft">The left margin in pixels.</param>
        /// <param name="marginRight">The right margin in pixels.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        public HorizontalGroupAttribute(float width = 0, int marginLeft = 0, int marginRight = 0, float order = 0)
            : this("_DefaultHorizontalGroup", width, marginLeft, marginRight, order)
        {
        }

        /// <summary>
        /// Merges the values of two group attributes together.
        /// </summary>
        /// <param name="other">The other group to combine with.</param>
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            this.Title = this.Title ?? (other as HorizontalGroupAttribute).Title;
            this.LabelWidth = Math.Max(this.LabelWidth, (other as HorizontalGroupAttribute).LabelWidth);
        }
    }
}