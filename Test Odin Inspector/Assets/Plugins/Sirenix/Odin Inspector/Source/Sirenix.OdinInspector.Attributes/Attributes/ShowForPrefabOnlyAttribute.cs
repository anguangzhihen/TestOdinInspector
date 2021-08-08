//-----------------------------------------------------------------------
// <copyright file="ShowForPrefabOnlyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>ShowForPrefabOnlyAttribute is used on any field or property, and only shows properties from prefab assets inspector.</para>
    /// <para>Use this to ensure the same value on a property, across all instances of a prefab.</para>
    /// </summary>
    /// <remarks>
    /// <para>On non-prefab objects or instances, this attribute does nothing, and allows properties to be edited as normal.</para>
    /// </remarks>
    /// <example>
    /// <para>The following example shows how ShowForPrefabOnlyAttribute is used on properties.</para>
    /// <code>
    /// public class MyComponent
    /// {
    ///		[ShowForPrefabOnlyAttribute]
    ///		public int MyInt;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="EnableForPrefabOnlyAttribute"/>
    /// <seealso cref="ShowIfAttribute"/>
    /// <seealso cref="HideIfAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    [Obsolete("Use HideInPrefabInstance or HideInPrefabAsset instead.", false)]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowForPrefabOnlyAttribute : Attribute
    {
    }
}