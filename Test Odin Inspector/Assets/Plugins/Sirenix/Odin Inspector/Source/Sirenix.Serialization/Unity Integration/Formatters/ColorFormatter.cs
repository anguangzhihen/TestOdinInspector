//-----------------------------------------------------------------------
// <copyright file="ColorFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(ColorFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Color"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.Color}" />
    public class ColorFormatter : MinimalBaseFormatter<Color>
    {
        private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Color value, IDataReader reader)
        {
            value.r = ColorFormatter.FloatSerializer.ReadValue(reader);
            value.g = ColorFormatter.FloatSerializer.ReadValue(reader);
            value.b = ColorFormatter.FloatSerializer.ReadValue(reader);
            value.a = ColorFormatter.FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Color value, IDataWriter writer)
        {
            ColorFormatter.FloatSerializer.WriteValue(value.r, writer);
            ColorFormatter.FloatSerializer.WriteValue(value.g, writer);
            ColorFormatter.FloatSerializer.WriteValue(value.b, writer);
            ColorFormatter.FloatSerializer.WriteValue(value.a, writer);
        }
    }
}