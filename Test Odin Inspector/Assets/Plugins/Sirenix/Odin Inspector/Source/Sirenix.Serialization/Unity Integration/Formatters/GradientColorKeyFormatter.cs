//-----------------------------------------------------------------------
// <copyright file="GradientColorKeyFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(GradientColorKeyFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="GradientColorKey"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.GradientColorKey}" />
    public class GradientColorKeyFormatter : MinimalBaseFormatter<GradientColorKey>
    {
        private static readonly Serializer<Color> ColorSerializer = Serializer.Get<Color>();
        private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref GradientColorKey value, IDataReader reader)
        {
            value.color = GradientColorKeyFormatter.ColorSerializer.ReadValue(reader);
            value.time = GradientColorKeyFormatter.FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref GradientColorKey value, IDataWriter writer)
        {
            GradientColorKeyFormatter.ColorSerializer.WriteValue(value.color, writer);
            GradientColorKeyFormatter.FloatSerializer.WriteValue(value.time, writer);
        }
    }
}