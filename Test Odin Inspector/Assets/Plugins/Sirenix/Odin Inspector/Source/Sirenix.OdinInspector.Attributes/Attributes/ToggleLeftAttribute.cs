//-----------------------------------------------------------------------
// <copyright file="ToggleLeftAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>Draws the checkbox before the label instead of after.</para>
    /// </summary>
    /// <remarks>ToggleLeftAttribute can be used an all fields and properties of type boolean</remarks>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ToggleLeft]
    ///		public bool MyBoolean;
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class ToggleLeftAttribute : Attribute
    {
    }
}