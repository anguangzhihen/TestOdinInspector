#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ProgressBarAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Common base implementation for progress bar attribute drawers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseProgressBarAttributeDrawer<T> : OdinAttributeDrawer<ProgressBarAttribute, T>
    {
        private ValueResolver<T> minResolver;
        private ValueResolver<T> maxResolver;
        private ValueResolver<Color> foregroundColorResolver;
        private ValueResolver<Color> backgroundColorResolver;
        private ValueResolver<string> labelResolver;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            this.minResolver = ValueResolver.Get<T>(this.Property, this.Attribute.MinGetter, ConvertUtility.Convert<double, T>(this.Attribute.Min));
            this.maxResolver = ValueResolver.Get<T>(this.Property, this.Attribute.MaxGetter, ConvertUtility.Convert<double, T>(this.Attribute.Max));
            this.foregroundColorResolver = ValueResolver.Get<Color>(this.Property, this.Attribute.ColorGetter, this.Attribute.Color);
            this.backgroundColorResolver = ValueResolver.Get<Color>(this.Property, this.Attribute.BackgroundColorGetter, ProgressBarConfig.Default.BackgroundColor);
            this.labelResolver = ValueResolver.GetForString(this.Property, this.Attribute.CustomValueStringGetter);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueResolver.DrawErrors(this.minResolver, this.maxResolver, this.foregroundColorResolver, this.backgroundColorResolver, this.labelResolver);

            ProgressBarConfig config = this.GetConfig();

            // Construct a Rect based on the configured height of the field.
            Rect rect = EditorGUILayout.GetControlRect(label != null, config.Height < EditorGUIUtility.singleLineHeight ? EditorGUIUtility.singleLineHeight : config.Height);

            T min = this.minResolver.GetValue();
            T max = this.maxResolver.GetValue();
            string valueLabel = this.labelResolver.GetValue();

            // Draw the field.
            EditorGUI.BeginChangeCheck();
            T value = this.DrawProgressBar(rect, label, ConvertUtility.Convert<T, double>(min), ConvertUtility.Convert<T, double>(max), config, valueLabel);
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = value;
            }
        }

        private ProgressBarConfig GetConfig()
        {
            var config = ProgressBarConfig.Default;
            config.Height = this.Attribute.Height;
            config.DrawValueLabel = this.Attribute.DrawValueLabelHasValue ? this.Attribute.DrawValueLabel : (this.Attribute.Segmented ? false : true);
            config.ValueLabelAlignment = this.Attribute.ValueLabelAlignmentHasValue ? this.Attribute.ValueLabelAlignment : (this.Attribute.Segmented ? TextAlignment.Right : TextAlignment.Center);

            if (this.Attribute.CustomValueStringGetter != null)
            {
                // Do not draw default label.
                config.DrawValueLabel = false;
            }

            // No point in updating the color in non-repaint events.
            if (Event.current.type == EventType.Repaint)
            {
                config.ForegroundColor = this.foregroundColorResolver.GetValue();
                config.BackgroundColor = this.backgroundColorResolver.GetValue();
            }

            return config;
        }

        /// <summary>
        /// Generic implementation of progress bar field drawing.
        /// </summary>
        protected abstract T DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel);

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected abstract double ConvertToDouble(T value);
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeByteDrawer : BaseProgressBarAttributeDrawer<byte>
    {
        /// <summary>
        /// Draws a progress bar for a byte property.
        /// </summary>
        protected override byte DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (byte)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (byte)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(byte value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeSbyteDrawer : BaseProgressBarAttributeDrawer<sbyte>
    {
        /// <summary>
        /// Draws a progress bar for a sbyte property.
        /// </summary>
        protected override sbyte DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (sbyte)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (sbyte)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(sbyte value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeShortDrawer : BaseProgressBarAttributeDrawer<short>
    {
        /// <summary>
        /// Draws a progress bar for a short property.
        /// </summary>
        protected override short DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (short)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (short)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(short value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeUshortDrawer : BaseProgressBarAttributeDrawer<ushort>
    {
        /// <summary>
        /// Draws a progress bar for a ushort property.
        /// </summary>
        protected override ushort DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (ushort)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (ushort)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(ushort value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeIntDrawer : BaseProgressBarAttributeDrawer<int>
    {
        /// <summary>
        /// Draws a progress bar for an int property.
        /// </summary>
        protected override int DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (int)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (int)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(int value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeUintDrawer : BaseProgressBarAttributeDrawer<uint>
    {
        /// <summary>
        /// Draws a progress bar for a uint property.
        /// </summary>
        protected override uint DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (uint)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (uint)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(uint value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeLongDrawer : BaseProgressBarAttributeDrawer<long>
    {
        /// <summary>
        /// Draws a progress bar for a long property.
        /// </summary>
        protected override long DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (long)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (long)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(long value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeUlongDrawer : BaseProgressBarAttributeDrawer<ulong>
    {
        /// <summary>
        /// Draws a progress bar for a ulong property.
        /// </summary>
        protected override ulong DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (ulong)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (ulong)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(ulong value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributeFloatDrawer : BaseProgressBarAttributeDrawer<float>
    {
        /// <summary>
        /// Draws a progress bar for a float property.
        /// </summary>
        protected override float DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (float)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (float)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(float value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributedoubleDrawer : BaseProgressBarAttributeDrawer<double>
    {
        /// <summary>
        /// Draws a progress bar for a double property.
        /// </summary>
        protected override double DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (double)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (double)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(double value)
        {
            return (double)value; ;
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    public sealed class ProgressBarAttributedecimalDrawer : BaseProgressBarAttributeDrawer<decimal>
    {
        /// <summary>
        /// Draws a progress bar for a decimal property.
        /// </summary>
        protected override decimal DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel)
        {
            if (this.Attribute.Segmented)
            {
                return (decimal)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)this.ValueEntry.SmartValue, (long)min, (long)max, config, valueLabel);
            }
            else
            {
                return (decimal)SirenixEditorFields.ProgressBarField(rect, label, (double)this.ValueEntry.SmartValue, min, max, config, valueLabel);
            }
        }

        /// <summary>
        /// Converts the generic value to a double.
        /// </summary>
        /// <param name="value">The generic value to convert.</param>
        /// <returns>The generic value as a double.</returns>
        protected override double ConvertToDouble(decimal value)
        {
            return (double)value; ;
        }
    }
}
#endif