//-----------------------------------------------------------------------
// <copyright file="HideInEditorModeAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;

	/// <summary>
    /// <para>HideInEditorMode is used on any property, and hides the property when not in play mode.</para>
    /// <para>Use this when you only want a property to only be visible play mode.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how HideInEditorMode is used to hide a property when in the editor.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[HideInEditorMode]
    ///		public int MyInt;
    ///	}
    /// </code>
    /// </example>
	/// <seealso cref="HideInPlayModeAttribute"/>
    /// <seealso cref="DisableInPlayModeAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    [DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class HideInEditorModeAttribute : Attribute
	{ }
}