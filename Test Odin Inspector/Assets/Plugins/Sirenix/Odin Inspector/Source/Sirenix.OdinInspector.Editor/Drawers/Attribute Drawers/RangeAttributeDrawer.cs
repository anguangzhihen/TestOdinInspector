#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RangeAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws byte properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeByteDrawer : OdinAttributeDrawer<RangeAttribute, byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            int value = SirenixEditorFields.RangeIntField(label, entry.SmartValue, Math.Max(byte.MinValue, (int)attribute.min), Math.Min(byte.MaxValue, (int)attribute.max));

            if (value < byte.MinValue)
            {
                value = byte.MinValue;
            }
            else if (value > byte.MaxValue)
            {
                value = byte.MaxValue;
            }

            entry.SmartValue = (byte)value;
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeDoubleDrawer : OdinAttributeDrawer<RangeAttribute, double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            double value = entry.SmartValue;

            if (value < float.MinValue)
            {
                value = float.MinValue;
            }
            else if (value > float.MaxValue)
            {
                value = float.MaxValue;
            }

            EditorGUI.BeginChangeCheck();
            value = SirenixEditorFields.RangeFloatField(label, (float)value, attribute.min, attribute.max);
            if (EditorGUI.EndChangeCheck())
            {
                entry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeFloatDrawer : OdinAttributeDrawer<RangeAttribute, float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            entry.SmartValue = SirenixEditorFields.RangeFloatField(label, entry.SmartValue, attribute.min, attribute.max);
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    public sealed class RangeAttributeDecimalDrawer : OdinAttributeDrawer<RangeAttribute, decimal>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            EditorGUI.BeginChangeCheck();
            float value = SirenixEditorFields.RangeFloatField(label, (float)entry.SmartValue, attribute.min, attribute.max);

            if (EditorGUI.EndChangeCheck())
            {
                entry.SmartValue = (decimal)value;
            }
        }
    }

    /// <summary>
    /// Draws short properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeInt16Drawer : OdinAttributeDrawer<RangeAttribute, short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            int value = SirenixEditorFields.RangeIntField(label, entry.SmartValue, Math.Max(short.MinValue, (int)attribute.min), Math.Min(short.MaxValue, (int)attribute.max));

            if (value < short.MinValue)
            {
                value = short.MinValue;
            }
            else if (value > short.MaxValue)
            {
                value = short.MaxValue;
            }

            entry.SmartValue = (short)value;
        }
    }

    /// <summary>
    /// Draws int properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeInt32Drawer : OdinAttributeDrawer<RangeAttribute, int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            entry.SmartValue = SirenixEditorFields.RangeIntField(label, entry.SmartValue, (int)attribute.min, (int)attribute.max);
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeInt64Drawer : OdinAttributeDrawer<RangeAttribute, long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            long uValue = entry.SmartValue;

            if (uValue < int.MinValue)
            {
                uValue = int.MinValue;
            }
            else if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = SirenixEditorFields.RangeIntField(label, (int)uValue, (int)attribute.min, (int)attribute.max);

            entry.SmartValue = value;
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeSByteDrawer : OdinAttributeDrawer<RangeAttribute, sbyte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            int value = SirenixEditorFields.RangeIntField(label, entry.SmartValue, Math.Max(sbyte.MinValue, (int)attribute.min), Math.Min(sbyte.MaxValue, (int)attribute.max));

            if (value < sbyte.MinValue)
            {
                value = sbyte.MinValue;
            }
            else if (value > sbyte.MaxValue)
            {
                value = sbyte.MaxValue;
            }

            entry.SmartValue = (sbyte)value;
        }
    }

    /// <summary>
    /// Draws ushort properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeUInt16Drawer : OdinAttributeDrawer<RangeAttribute, ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            int value = SirenixEditorFields.RangeIntField(label, entry.SmartValue, Math.Max(ushort.MinValue, (int)attribute.min), Math.Min(ushort.MaxValue, (int)attribute.max));

            if (value < ushort.MinValue)
            {
                value = ushort.MinValue;
            }
            else if (value > ushort.MaxValue)
            {
                value = ushort.MaxValue;
            }

            entry.SmartValue = (ushort)value;
        }
    }

    /// <summary>
    /// Draws uint properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeUInt32Drawer : OdinAttributeDrawer<RangeAttribute, uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            uint uValue = entry.SmartValue;

            if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = SirenixEditorFields.RangeIntField(label, (int)uValue, Math.Max(0, (int)attribute.min), (int)attribute.max);

            if (value < 0)
            {
                value = 0;
            }

            entry.SmartValue = (uint)value;
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class RangeAttributeUInt64Drawer : OdinAttributeDrawer<RangeAttribute, ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            ulong uValue = entry.SmartValue;

            if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = SirenixEditorFields.RangeIntField(label, (int)uValue, Math.Max(0, (int)attribute.min), (int)attribute.max);

            if (value < 0)
            {
                value = 0;
            }

            entry.SmartValue = (ulong)value;
        }
    }
}
#endif