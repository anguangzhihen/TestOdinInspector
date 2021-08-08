//-----------------------------------------------------------------------
// <copyright file="DelayedPropertyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;
    
    /// <summary>
    /// Delays applying changes to properties while they still being edited in the inspector.
    /// Similar to Unity's built-in Delayed attribute, but this attribute can also be applied to properties.
    /// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class DelayedPropertyAttribute : Attribute
	{ }
}