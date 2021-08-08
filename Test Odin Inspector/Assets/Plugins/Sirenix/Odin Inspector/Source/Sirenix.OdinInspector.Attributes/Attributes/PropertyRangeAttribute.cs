//-----------------------------------------------------------------------
// <copyright file="PropertyRangeAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;

    /// <summary>
    /// <para>PropertyRange attribute creates a slider control to set the value of a property to between the specified range.</para>
    /// <para>This is equivalent to Unity's Range attribute, but this attribute can be applied to both fields and property.</para>
    /// </summary>
    /// <example>The following example demonstrates how PropertyRange is used.</example>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    /// 	[PropertyRange(0, 100)]
    ///		public int MyInt;
    ///		
    ///		[PropertyRange(-100, 100)]
    ///		public float MyFloat;
    ///		
    ///		[PropertyRange(-100, -50)]
    ///		public decimal MyDouble;
    ///		
    ///     // This attribute also supports dynamically referencing members by name to assign the min and max values for the range field.
    ///     [PropertyRange("DynamicMin", "DynamicMax"]
    ///     public float MyDynamicValue;
    ///     
    ///     public float DynamicMin, DynamicMax;
    ///	}
    /// </code>
    /// <seealso cref="ShowInInspectorAttribute"/>
    /// <seealso cref="PropertySpaceAttribute"/>
    /// <seealso cref="PropertyTooltipAttribute"/>
    /// <seealso cref="PropertyOrderAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public sealed class PropertyRangeAttribute : Attribute
	{
		/// <summary>
		/// The minimum value.
		/// </summary>
		public double Min;

		/// <summary>
		/// The maximum value.
		/// </summary>
		public double Max;

        /// <summary>
        /// The name of a field, property or method to get the min value from. Obsolete; use the MinGetter member instead.
        /// </summary>
        [Obsolete("Use the MinGetter member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string MinMember { get { return this.MinGetter; } set { this.MinGetter = value; } }

        /// <summary>
        /// A resolved string that should evaluate to a float value, and will be used as the min bounds.
        /// </summary>
        public string MinGetter;

        /// <summary>
        /// The name of a field, property or method to get the max value from. Obsolete; use the MaxGetter member instead.
        /// </summary>
        [Obsolete("Use the MaxGetter member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string MaxMember { get { return this.MaxGetter; } set { this.MaxGetter = value; } }

        /// <summary>
        /// A resolved string that should evaluate to a float value, and will be used as the max bounds.
        /// </summary>
        public string MaxGetter;

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public PropertyRangeAttribute(double min, double max)
		{
			this.Min = min < max ? min : max;
			this.Max = max > min ? max : min;
		}

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="minGetter">A resolved string that should evaluate to a float value, and will be used as the min bounds.</param>
        /// <param name="max">The maximum value.</param>
        public PropertyRangeAttribute(string minGetter, double max)
        {
            this.MinGetter = minGetter;
            this.Max = max;
        }

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="maxGetter">A resolved string that should evaluate to a float value, and will be used as the max bounds.</param>
        public PropertyRangeAttribute(double min, string maxGetter)
        {
            this.Min = min;
            this.MaxGetter = maxGetter;
        }

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="minGetter">A resolved string that should evaluate to a float value, and will be used as the min bounds.</param>
        /// <param name="maxGetter">A resolved string that should evaluate to a float value, and will be used as the max bounds.</param>
        public PropertyRangeAttribute(string minGetter, string maxGetter)
        {
            this.MinGetter = minGetter;
            this.MaxGetter = maxGetter;
        }
	}
}