//-----------------------------------------------------------------------
// <copyright file="Vector3Formatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(Vector3Formatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Vector3"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.Vector3}" />
    public class Vector3Formatter : MinimalBaseFormatter<Vector3>
    {
        private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Vector3 value, IDataReader reader)
        {
            value.x = Vector3Formatter.FloatSerializer.ReadValue(reader);
            value.y = Vector3Formatter.FloatSerializer.ReadValue(reader);
            value.z = Vector3Formatter.FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Vector3 value, IDataWriter writer)
        {
            Vector3Formatter.FloatSerializer.WriteValue(value.x, writer);
            Vector3Formatter.FloatSerializer.WriteValue(value.y, writer);
            Vector3Formatter.FloatSerializer.WriteValue(value.z, writer);
        }
    }
}