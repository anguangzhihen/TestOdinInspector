//-----------------------------------------------------------------------
// <copyright file="ExcludeDataFromInspectorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    /// Causes Odin's inspector to completely ignore a given member, preventing it from even being included in an Odin PropertyTree,
    /// and such will not cause any performance hits in the inspector.
    /// </para>
    /// <para>Note that Odin can still serialize an excluded member - it is merely ignored in the inspector itself.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [Obsolete("Use [HideInInspector] instead - it now also excludes the member completely from becoming a property in the property tree.", false)]
    public sealed class ExcludeDataFromInspectorAttribute : Attribute
    {
    }
}