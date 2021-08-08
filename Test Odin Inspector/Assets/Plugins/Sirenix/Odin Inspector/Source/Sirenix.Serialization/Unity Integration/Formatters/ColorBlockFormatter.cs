//-----------------------------------------------------------------------
// <copyright file="ColorBlockFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(ColorBlockFormatterLocator))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;
    using System.Reflection;
    using UnityEngine;

    public class ColorBlockFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, out IFormatter formatter)
        {
            if (step == FormatterLocationStep.BeforeRegisteredFormatters && type.FullName == "UnityEngine.UI.ColorBlock")
            {
                var formatterType = typeof(ColorBlockFormatter<>).MakeGenericType(type);
                formatter = (IFormatter)Activator.CreateInstance(formatterType);
                return true;
            }

            formatter = null;
            return false;
        }
    }

    /// <summary>
    /// Custom formatter for the <see cref="ColorBlock"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.UI.ColorBlock}" />
    public class ColorBlockFormatter<T> : MinimalBaseFormatter<T>
    {
        private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();
        private static readonly Serializer<Color> ColorSerializer = Serializer.Get<Color>();

        private static readonly PropertyInfo normalColor = typeof(T).GetProperty("normalColor");
        private static readonly PropertyInfo highlightedColor = typeof(T).GetProperty("highlightedColor");
        private static readonly PropertyInfo pressedColor = typeof(T).GetProperty("pressedColor");
        private static readonly PropertyInfo disabledColor = typeof(T).GetProperty("disabledColor");
        private static readonly PropertyInfo colorMultiplier = typeof(T).GetProperty("colorMultiplier");
        private static readonly PropertyInfo fadeDuration = typeof(T).GetProperty("fadeDuration");
        
        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref T value, IDataReader reader)
        {
            object boxed = value;

            normalColor.SetValue(boxed, ColorSerializer.ReadValue(reader), null);
            highlightedColor.SetValue(boxed, ColorSerializer.ReadValue(reader), null);
            pressedColor.SetValue(boxed, ColorSerializer.ReadValue(reader), null);
            disabledColor.SetValue(boxed, ColorSerializer.ReadValue(reader), null);
            colorMultiplier.SetValue(boxed, FloatSerializer.ReadValue(reader), null);
            fadeDuration.SetValue(boxed, FloatSerializer.ReadValue(reader), null);

            value = (T)boxed;
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref T value, IDataWriter writer)
        {
            ColorSerializer.WriteValue((Color)normalColor.GetValue(value, null), writer);
            ColorSerializer.WriteValue((Color)highlightedColor.GetValue(value, null), writer);
            ColorSerializer.WriteValue((Color)pressedColor.GetValue(value, null), writer);
            ColorSerializer.WriteValue((Color)disabledColor.GetValue(value, null), writer);
            FloatSerializer.WriteValue((float)colorMultiplier.GetValue(value, null), writer);
            FloatSerializer.WriteValue((float)fadeDuration.GetValue(value, null), writer);
        }
    }
}