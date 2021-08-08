//-----------------------------------------------------------------------
// <copyright file="KeyValuePairFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(KeyValuePairFormatter<,>))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System.Collections.Generic;

    /// <summary>
    /// Custom generic formatter for the generic type definition <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="BaseFormatter{System.Collections.Generic.KeyValuePair{TKey, TValue}}" />
    public sealed class KeyValuePairFormatter<TKey, TValue> : BaseFormatter<KeyValuePair<TKey, TValue>>
    {
        private static readonly Serializer<TKey> KeySerializer = Serializer.Get<TKey>();
        private static readonly Serializer<TValue> ValueSerializer = Serializer.Get<TValue>();

        /// <summary>
        /// Provides the actual implementation for serializing a value of type <see cref="T" />.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to serialize with.</param>
        protected override void SerializeImplementation(ref KeyValuePair<TKey, TValue> value, IDataWriter writer)
        {
            KeySerializer.WriteValue(value.Key, writer);
            ValueSerializer.WriteValue(value.Value, writer);
        }

        /// <summary>
        /// Provides the actual implementation for deserializing a value of type <see cref="T" />.
        /// </summary>
        /// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="BaseFormatter{T}.GetUninitializedObject" />.</param>
        /// <param name="reader">The reader to deserialize with.</param>

        protected override void DeserializeImplementation(ref KeyValuePair<TKey, TValue> value, IDataReader reader)
        {
            value = new KeyValuePair<TKey, TValue>(
                KeySerializer.ReadValue(reader),
                ValueSerializer.ReadValue(reader)
            );
        }
    }
}