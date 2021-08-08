//-----------------------------------------------------------------------
// <copyright file="QuaternionFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(QuaternionFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Quaternion"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.Quaternion}" />
    public class QuaternionFormatter : MinimalBaseFormatter<Quaternion>
    {
        private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Quaternion value, IDataReader reader)
        {
            value.x = QuaternionFormatter.FloatSerializer.ReadValue(reader);
            value.y = QuaternionFormatter.FloatSerializer.ReadValue(reader);
            value.z = QuaternionFormatter.FloatSerializer.ReadValue(reader);
            value.w = QuaternionFormatter.FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Quaternion value, IDataWriter writer)
        {
            QuaternionFormatter.FloatSerializer.WriteValue(value.x, writer);
            QuaternionFormatter.FloatSerializer.WriteValue(value.y, writer);
            QuaternionFormatter.FloatSerializer.WriteValue(value.z, writer);
            QuaternionFormatter.FloatSerializer.WriteValue(value.w, writer);
        }
    }
}