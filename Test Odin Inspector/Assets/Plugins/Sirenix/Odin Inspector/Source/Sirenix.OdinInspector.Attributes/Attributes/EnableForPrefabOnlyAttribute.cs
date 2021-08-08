//-----------------------------------------------------------------------
// <copyright file="EnableForPrefabOnlyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;

	/// <summary>
	/// <para>EnableForPrefabOnly is used on any field or property, and only allows editing of values from prefab assets inspector.</para>
	/// <para>Use this to ensure the same value on a property, across all instances of a prefab.</para>
	/// </summary>
	/// <remarks>
	/// <para>On non-prefab objects or instances, this attribute does nothing, and allows properties to be edited as normal.</para>
	/// </remarks>
	/// <example>
	/// <para>The following example shows how EnableForPrefabOnly is used on properties.</para>
	/// <code>
	/// public class MyComponent
	/// {
	///		[EnableForPrefabOnly]
	///		public int MyInt;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="EnableForPrefabOnlyAttribute"/>
	/// <seealso cref="EnableIfAttribute"/>
	/// <seealso cref="DisableIfAttribute"/>
	/// <seealso cref="ShowIfAttribute"/>
	/// <seealso cref="HideIfAttribute"/>
    [Obsolete("Use DisableInPrefabInstance or DisableInPrefabAsset instead.", false)]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class EnableForPrefabOnlyAttribute : Attribute
	{ }
}