//-----------------------------------------------------------------------
// <copyright file="AllowDeserializeInvalidDataAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    /// Applying this attribute to a type indicates that in the case where, when expecting to deserialize an instance of the type
    /// or any of its derived types, but encountering an incompatible, uncastable type in the data being read, the serializer
    /// should attempt to deserialize an instance of the expected type using the stored, possibly invalid data.
    /// </para>
    /// <para>
    /// This is equivalent to the <see cref="SerializationConfig.AllowDeserializeInvalidData"/> option, expect type-specific instead
    /// of global.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class AllowDeserializeInvalidDataAttribute : Attribute
    {
    }
}