//-----------------------------------------------------------------------
// <copyright file="IFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Serializes and deserializes a given type.
    /// <para />
    /// NOTE that if you are implementing a custom formatter and registering it using the <see cref="CustomFormatterAttribute"/>, it is not enough to implement <see cref="IFormatter"/> - you have to implement <see cref="IFormatter{T}"/>.
    /// </summary>
    public interface IFormatter
    {
        /// <summary>
        /// Gets the type that the formatter can serialize.
        /// </summary>
        /// <value>
        /// The type that the formatter can serialize.
        /// </value>
        Type SerializedType { get; }

        /// <summary>
        /// Serializes a value using a specified <see cref="IDataWriter" />.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to use.</param>
        void Serialize(object value, IDataWriter writer);

        /// <summary>
        /// Deserializes a value using a specified <see cref="IDataReader" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        object Deserialize(IDataReader reader);
    }

    /// <summary>
    /// Serializes and deserializes a given type T.
    /// </summary>
    /// <typeparam name="T">The type which can be serialized and deserialized by the formatter.</typeparam>
    public interface IFormatter<T> : IFormatter
    {
        /// <summary>
        /// Serializes a value of type <see cref="T" /> using a specified <see cref="IDataWriter" />.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to use.</param>
        void Serialize(T value, IDataWriter writer);

        /// <summary>
        /// Deserializes a value of type <see cref="T" /> using a specified <see cref="IDataReader" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        new T Deserialize(IDataReader reader);
    }
}