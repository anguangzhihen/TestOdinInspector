//-----------------------------------------------------------------------
// <copyright file="NullableFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(NullableFormatter<>))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Formatter for all <see cref="System.Nullable{T}"/> types.
    /// </summary>
    /// <typeparam name="T">The type that is nullable.</typeparam>
    /// <seealso cref="BaseFormatter{T?}" />
    public sealed class NullableFormatter<T> : BaseFormatter<T?> where T : struct
    {
        private static readonly Serializer<T> TSerializer = Serializer.Get<T>();

        static NullableFormatter()
        {
            // This exists solely to prevent IL2CPP code stripping from removing the generic type's instance constructor
            // which it otherwise seems prone to do, regardless of what might be defined in any link.xml file.

            new NullableFormatter<int>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="NullableFormatter{T}"/>.
        /// </summary>
        public NullableFormatter()
        {
        }

        /// <summary>
        /// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
        /// </summary>
        /// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:OdinSerializer.BaseFormatter`1.GetUninitializedObject" />.</param>
        /// <param name="reader">The reader to deserialize with.</param>
        protected override void DeserializeImplementation(ref T? value, IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.Null)
            {
                value = null;
                reader.ReadNull();
            }
            else
            {
                value = TSerializer.ReadValue(reader);
            }
        }

        /// <summary>
        /// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to serialize with.</param>
        protected override void SerializeImplementation(ref T? value, IDataWriter writer)
        {
            if (value.HasValue)
            {
                TSerializer.WriteValue(value.Value, writer);
            }
            else
            {
                writer.WriteNull(null);
            }
        }
    }
}