//-----------------------------------------------------------------------
// <copyright file="ISupportsPrefabSerialization.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Indicates that an Odin-serialized Unity object supports prefab serialization.
    /// </summary>
    public interface ISupportsPrefabSerialization
    {
        /// <summary>
        /// Gets or sets the serialization data of the object.
        /// </summary>
        SerializationData SerializationData { get; set; }
    }
}