//-----------------------------------------------------------------------
// <copyright file="AnimationCurveFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(AnimationCurveFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="AnimationCurve"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.AnimationCurve}" />
    public class AnimationCurveFormatter : MinimalBaseFormatter<AnimationCurve>
    {
        private static readonly Serializer<Keyframe[]> KeyframeSerializer = Serializer.Get<Keyframe[]>();
        private static readonly Serializer<WrapMode> WrapModeSerializer = Serializer.Get<WrapMode>();

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <returns>
        /// A null value.
        /// </returns>
        protected override AnimationCurve GetUninitializedObject()
        {
            return null;
        }

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref AnimationCurve value, IDataReader reader)
        {
            var keys = KeyframeSerializer.ReadValue(reader);

            value = new AnimationCurve(keys);
            value.preWrapMode = WrapModeSerializer.ReadValue(reader);
            value.postWrapMode = WrapModeSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref AnimationCurve value, IDataWriter writer)
        {
            KeyframeSerializer.WriteValue(value.keys, writer);
            WrapModeSerializer.WriteValue(value.preWrapMode, writer);
            WrapModeSerializer.WriteValue(value.postWrapMode, writer);
        }
    }
}