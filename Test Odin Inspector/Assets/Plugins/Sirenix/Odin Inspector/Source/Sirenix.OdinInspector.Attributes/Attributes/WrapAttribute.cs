//-----------------------------------------------------------------------
// <copyright file="WrapAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;

	/// <summary>
	/// <para>Wrap is used on most primitive property, and allows for wrapping the value when it goes out of the defined range.</para>
	/// <para>Use this when you want a value that goes around in circle, like for example an angle.</para>
	/// </summary>
	/// <remarks>
	/// <note type="note">Currently unsigned primitives are not supported.</note>
	/// </remarks>
	/// <example>
	/// <para>The following example show how Wrap is used on a property.</para>
	/// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[Wrap(-100, 100)]
	///		public float MyFloat;
	///	}
	/// </code>
	/// </example>
	/// <seealso cref="AngleWrapAttribute"/>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public sealed class WrapAttribute : Attribute
	{
        /// <summary>
        /// The lowest value for the property.
        /// </summary>
        public double Min;

        /// <summary>
        /// The highest value for the property.
        /// </summary>
        public double Max;

		/// <summary>
		/// Wraps the value of the property round when the values goes out of range.
		/// </summary>
		/// <param name="min">The lowest value for the property.</param>
		/// <param name="max">The highest value for the property.</param>
		public WrapAttribute(double min, double max)
        { 
            this.Min = min < max ? min : max;
			this.Max = max > min ? max : min;
		}
	}
}