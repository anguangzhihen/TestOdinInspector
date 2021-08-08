//-----------------------------------------------------------------------
// <copyright file="IOverridesSerializationFormat.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Indicates that an Odin-serialized Unity object controls its own serialization format. Every time it is serialized, it will be asked which format to use.
    /// </summary>
    public interface IOverridesSerializationFormat
    {
        /// <summary>
        /// Gets the format to use for serialization.
        /// </summary>
        DataFormat GetFormatToSerializeAs(bool isPlayer);
    }
}