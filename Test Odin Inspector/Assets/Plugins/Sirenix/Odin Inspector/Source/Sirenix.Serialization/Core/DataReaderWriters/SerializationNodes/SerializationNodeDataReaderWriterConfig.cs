//-----------------------------------------------------------------------
// <copyright file="SerializationNodeDataReaderWriterConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Shared config class for <see cref="SerializationNodeDataReader"/> and <see cref="SerializationNodeDataWriter"/>.
    /// </summary>
    public static class SerializationNodeDataReaderWriterConfig
    {
        /// <summary>
        /// The string to use to separate node id's from their names.
        /// </summary>
        public const string NodeIdSeparator = "|";
    }
}