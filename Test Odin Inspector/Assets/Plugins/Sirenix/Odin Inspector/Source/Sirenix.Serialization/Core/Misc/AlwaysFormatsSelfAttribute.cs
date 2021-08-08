//-----------------------------------------------------------------------
// <copyright file="AlwaysFormatsSelfAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Use this attribute to specify that a type that implements the <see cref="ISelfFormatter"/>
    /// interface should *always* format itself regardless of other formatters being specified.
    /// <para />
    /// This means that the interface will be used to format all types derived from the type that
    /// is decorated with this attribute, regardless of custom formatters for the derived types.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class AlwaysFormatsSelfAttribute : Attribute
    {
    }
}