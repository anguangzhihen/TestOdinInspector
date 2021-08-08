//-----------------------------------------------------------------------
// <copyright file="BoxGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
	/// <para>BoxGroup is used on any property and organizes the property in a boxed group.</para>
	/// <para>Use this to cleanly organize relevant values together in the inspector.</para>
    /// </summary>
	/// <example>
	/// <para>The following example shows how BoxGroup is used to organize properties together into a box.</para>
    /// <code>
    /// public class BoxGroupExamples : MonoBehaviour
    /// {
    ///     // Box with a centered title.
    ///     [BoxGroup("Centered Title", centerLabel: true)]
    ///     public int A;
    /// 
    ///     [BoxGroup("Centered Title", centerLabel: true)]
    ///     public int B;
    /// 
    ///     [BoxGroup("Centered Title", centerLabel: true)]
    ///     public int C;
    /// 
    ///     // Box with a title.
    ///     [BoxGroup("Left Oriented Title")]
    ///     public int D;
    /// 
    ///     [BoxGroup("Left Oriented Title")]
    ///     public int E;
    /// 
    ///     // Box with a title recieved from a field.
    ///     [BoxGroup("$DynamicTitle1"), LabelText("Dynamic Title")]
    ///     public string DynamicTitle1 = "Dynamic box title";
    /// 
    ///     [BoxGroup("$DynamicTitle1")]
    ///     public int F;
    /// 
    ///     // Box with a title recieved from a property.
    ///     [BoxGroup("$DynamicTitle2")]
    ///     public int G;
    /// 
    ///     [BoxGroup("$DynamicTitle2")]
    ///     public int H;
    /// 
    ///     // Box without a title.
    ///     [InfoBox("You can also hide the label of a box group.")]
    ///     [BoxGroup("NoTitle", false)]
    ///     public int I;
    /// 
    ///     [BoxGroup("NoTitle")]
    ///     public int J;
    /// 
    ///     [BoxGroup("NoTitle")]
    ///     public int K;
    /// 
    /// #if UNITY_EDITOR
    ///     public string DynamicTitle2
    ///     {
    ///         get { return UnityEditor.PlayerSettings.productName; }
    ///     }
    /// #endif
    /// 
    ///     [BoxGroup("Boxed Struct"), HideLabel]
    ///     public SomeStruct BoxedStruct;
    /// 
    ///     public SomeStruct DefaultStruct;
    /// 
    ///     [Serializable]
    ///     public struct SomeStruct
    ///     {
    ///         public int One;
    ///         public int Two;
    ///         public int Three;
    ///     }
    /// }
    /// </code>
    /// </example>
	/// <seealso cref="ButtonGroupAttribute"/>
	/// <seealso cref="FoldoutGroupAttribute"/>
	/// <seealso cref="HorizontalGroupAttribute"/>
	/// <seealso cref="TabGroupAttribute"/>
	/// <seealso cref="ToggleGroupAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class BoxGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// If <c>true</c> a label for the group will be drawn on top.
        /// </summary>
        public bool ShowLabel;

        /// <summary>
        /// If <c>true</c> the header label will be places in the center of the group header. Otherwise it will be in left side.
        /// </summary>
        public bool CenterLabel;

        /// <summary>
        /// If non-null, this is used instead of the group's name as the title label.
        /// </summary>
        public string LabelText;

        /// <summary>
        /// Adds the property to the specified box group.
        /// </summary>
        /// <param name="group">The box group.</param>
		/// <param name="showLabel">If <c>true</c> a label will be drawn for the group.</param>
        /// <param name="centerLabel">If set to <c>true</c> the header label will be centered.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        public BoxGroupAttribute(string group, bool showLabel = true, bool centerLabel = false, float order = 0)
            : base(group, order)
        {
            this.ShowLabel = showLabel;
            this.CenterLabel = centerLabel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxGroupAttribute"/> class. Use the other constructor overloads in order to show a header-label on the box group.
        /// </summary>
        public BoxGroupAttribute()
            : this("_DefaultBoxGroup", false)
        {
        }

        /// <summary>
        /// Combines the box group with another group.
        /// </summary>
        /// <param name="other">The other group.</param>
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var attr = other as BoxGroupAttribute;

            if (this.ShowLabel == false || attr.ShowLabel == false)
            {
                this.ShowLabel = false;
                attr.ShowLabel = false;
            }

            this.CenterLabel |= attr.CenterLabel;
        }
    }
}