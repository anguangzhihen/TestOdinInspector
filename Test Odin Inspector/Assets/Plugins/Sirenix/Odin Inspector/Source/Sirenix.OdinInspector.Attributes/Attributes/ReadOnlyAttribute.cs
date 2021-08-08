//-----------------------------------------------------------------------
// <copyright file="ReadOnlyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>ReadOnly is used on any property, and disabled the property from being changed in the inspector.</para>
    /// <para>Use this for when you want to see the value of a property in the inspector, but don't want it to be changed.</para>
    /// </summary>
	/// <remarks>
    /// <note type="note">This attribute only affects the inspector! Values can still be changed by script.</note>
    /// </remarks>
	/// <example>
    /// <para>The following example shows how a field can be displayed in the editor, but not be editable.</para>
    /// <code>
    /// public class Health : MonoBehaviour
	/// {
	///		public int MaxHealth;
	///
	///		[ReadOnly]
	///		public int CurrentHealth;
	/// }
    /// </code>
    /// </example>
	/// <example>
    /// <para>ReadOnly can also be combined with <see cref="ShowInInspectorAttribute"/>.</para>
    /// <code>
    /// public class Health : MonoBehaviour
	/// {
	///		public int MaxHealth;
	///
	///		[ShowInInspector, ReadOnly]
	///		private int currentHealth;
	/// }
    /// </code>
    /// </example>
	/// <seealso cref="ShowInInspectorAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }
}