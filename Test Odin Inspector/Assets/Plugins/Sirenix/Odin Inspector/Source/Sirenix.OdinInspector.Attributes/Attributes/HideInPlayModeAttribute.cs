//-----------------------------------------------------------------------
// <copyright file="HideInPlayModeAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

	using System;

	/// <summary>
    /// <para>HideInPlayMode is used on any property, and hides the property when not in editor mode.</para>
    /// <para>Use this when you only want a property to only be visible the editor.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how HideInPlayMode is used to hide a property when in play mode.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[HideInPlayMode]
    ///		public int MyInt;
    ///	}
    /// </code>
    /// </example>
	/// <seealso cref="HideInEditorModeAttribute"/>
    /// <seealso cref="DisableInPlayModeAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
	[AttributeUsage(AttributeTargets.All)]
    [DontApplyToListElements]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class HideInPlayModeAttribute : Attribute
	{ }
}