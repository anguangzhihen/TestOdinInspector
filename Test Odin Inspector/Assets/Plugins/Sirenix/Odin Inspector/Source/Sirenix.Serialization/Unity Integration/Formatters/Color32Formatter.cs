//-----------------------------------------------------------------------
// <copyright file="Color32Formatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(Color32Formatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Color32"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.Color32}" />
    public class Color32Formatter : MinimalBaseFormatter<Color32>
    {
        private static readonly Serializer<byte> ByteSerializer = Serializer.Get<byte>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Color32 value, IDataReader reader)
        {
            value.r = Color32Formatter.ByteSerializer.ReadValue(reader);
            value.g = Color32Formatter.ByteSerializer.ReadValue(reader);
            value.b = Color32Formatter.ByteSerializer.ReadValue(reader);
            value.a = Color32Formatter.ByteSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Color32 value, IDataWriter writer)
        {
            Color32Formatter.ByteSerializer.WriteValue(value.r, writer);
            Color32Formatter.ByteSerializer.WriteValue(value.g, writer);
            Color32Formatter.ByteSerializer.WriteValue(value.b, writer);
            Color32Formatter.ByteSerializer.WriteValue(value.a, writer);
        }
    }
}