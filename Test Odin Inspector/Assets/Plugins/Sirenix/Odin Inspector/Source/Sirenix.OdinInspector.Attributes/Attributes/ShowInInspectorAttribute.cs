//-----------------------------------------------------------------------
// <copyright file="ShowInInspectorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
	/// <para>ShowInInspector is used on any member, and shows the value in the inspector. Note that the value being shown due to this attribute DOES NOT mean that the value is being serialized.</para>
    /// </summary>
	/// <remarks>
    /// <para>This can for example be combined with <see cref="ReadOnlyAttribute"/> to allow for live debugging of values.</para>
    /// <note type="note"></note>
    /// </remarks>
	/// <example>
	/// <para>The following example shows how ShowInInspector is used to show properties in the inspector, that otherwise wouldn't.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[ShowInInspector]
	///		private int myField;
	///
	///		[ShowInInspector]
	///		public int MyProperty { get; set; }
	///	}
	/// </code>
    /// </example>
    [JetBrains.Annotations.MeansImplicitUse]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowInInspectorAttribute : Attribute
    {
    }
}