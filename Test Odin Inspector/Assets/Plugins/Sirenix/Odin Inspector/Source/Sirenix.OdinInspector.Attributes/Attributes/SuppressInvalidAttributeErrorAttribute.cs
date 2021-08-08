//-----------------------------------------------------------------------
// <copyright file="SuppressInvalidAttributeErrorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>SuppressInvalidAttributeError is used on members to suppress the inspector error message you get when applying an attribute to a value that it's not supposed to work on.</para>
    /// <para>This can be very useful for applying attributes to generic parameter values, when it only applies to some of the possible types that the value might become.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows a case where the attribute might be useful.</para>
    /// <code>
    /// public class NamedValue&lt;T&gt;
    /// {
    ///     public string Name;
    ///
    ///     // The Range attribute will be applied if T is compatible with it, but if T is not compatible, an error will not be shown.
    ///		[SuppressInvalidAttributeError, Range(0, 10)]
    ///		public T Value;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class SuppressInvalidAttributeErrorAttribute : Attribute
    {
    }
}