//-----------------------------------------------------------------------
// <copyright file="DisplayAsStringAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;

	/// <summary>
	/// <para>DisplayAsString is used on any property, and displays a string in the inspector as text.</para>
	/// <para>Use this for when you want to show a string in the inspector, but not allow for any editing.</para>
	/// </summary>
	/// <remarks>
	/// <para>DisplayAsString uses the property's ToString method to display the property as a string.</para>
	/// </remarks>
	/// <example>
	/// <para>The following example shows how DisplayAsString is used to display a string property as text in the inspector.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	///	{
	///		[DisplayAsString]
	///		public string MyInt = 5;
	///
	///		// You can combine with <see cref="HideLabelAttribute"/> to display a message in the inspector.
	///		[DisplayAsString, HideLabel]
	///		public string MyMessage = "This string will be displayed as text in the inspector";
	///		
	///		[DisplayAsString(false)]
	///		public string InlineMessage = "This string is very long, but has been configured to not overflow.";
	///	}
	/// </code>
	/// </example>
	/// <seealso cref="TitleAttribute"/>
	/// <seealso cref="MultiLinePropertyAttribute"/>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public sealed class DisplayAsStringAttribute : Attribute
	{
        /// <summary>
        /// If <c>true</c>, the string will overflow past the drawn space and be clipped when there's not enough space for the text.
        /// If <c>false</c> the string will expand to multiple lines, if there's not enough space when drawn.
        /// </summary>
        public bool Overflow;

		/// <summary>
		/// Displays the property as a string in the inspector.
		/// </summary>
		public DisplayAsStringAttribute()
		{
			this.Overflow = true;
		}
		
		/// <summary>
		/// Displays the property as a string in the inspector.
		/// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
		public DisplayAsStringAttribute(bool overflow)
		{
			this.Overflow = overflow;
		}
	}
}