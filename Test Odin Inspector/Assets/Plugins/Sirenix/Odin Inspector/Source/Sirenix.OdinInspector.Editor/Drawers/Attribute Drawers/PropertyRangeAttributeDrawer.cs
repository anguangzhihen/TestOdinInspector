#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyRangeAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws byte properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeByteDrawer : OdinAttributeDrawer<PropertyRangeAttribute, byte>
    {
        private ValueResolver<byte> getterMinValue;
        private ValueResolver<byte> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<byte>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<byte>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            byte min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (byte)this.Attribute.Min;
            byte max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (byte)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, this.ValueEntry.SmartValue, Mathf.Min(min, max), Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                if (value < byte.MinValue)
                {
                    value = byte.MinValue;
                }
                else if (value > byte.MaxValue)
                {
                    value = byte.MaxValue;
                }

                this.ValueEntry.SmartValue = (byte)value;
            }
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeDoubleDrawer : OdinAttributeDrawer<PropertyRangeAttribute, double>
    {
        private ValueResolver<double> getterMinValue;
        private ValueResolver<double> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<double>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<double>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // TODO: There's currently no SirenixEditorFields.DoubleRangeField, so we're making do with the float field. This should be fixed.
            double value = this.ValueEntry.SmartValue;
            if (value < float.MinValue)
            {
                value = float.MinValue;
            }
            else if (value > float.MaxValue)
            {
                value = float.MaxValue;
            }

            double min = this.getterMinValue != null ? this.getterMinValue.GetValue() : this.Attribute.Min;
            double max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            value = SirenixEditorFields.RangeFloatField(label, (float)value, (float)Math.Min(min, max), (float)Math.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeFloatDrawer : OdinAttributeDrawer<PropertyRangeAttribute, float>
    {
        private ValueResolver<float> getterMinValue;
        private ValueResolver<float> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<float>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<float>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            float min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (float)this.Attribute.Min;
            float max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (float)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            float value = SirenixEditorFields.RangeFloatField(label, this.ValueEntry.SmartValue, Mathf.Min(min, max), Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    public sealed class PropertyRangeAttributeDecimalDrawer : OdinAttributeDrawer<PropertyRangeAttribute, decimal>
    {
        private ValueResolver<decimal> getterMinValue;
        private ValueResolver<decimal> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<decimal>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<decimal>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            decimal min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (decimal)this.Attribute.Min;
            decimal max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (decimal)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            float value = SirenixEditorFields.RangeFloatField(label, (float)this.ValueEntry.SmartValue, (float)Math.Min(min, max), (float)Math.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = (decimal)value;
            }
        }
    }

    /// <summary>
    /// Draws short properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeInt16Drawer : OdinAttributeDrawer<PropertyRangeAttribute, short>
    {
        private ValueResolver<short> getterMinValue;
        private ValueResolver<short> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<short>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<short>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            short min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (short)this.Attribute.Min;
            short max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (short)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, this.ValueEntry.SmartValue, Mathf.Min(min, max), Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                if (value < short.MinValue)
                {
                    value = short.MinValue;
                }
                else if (value > short.MaxValue)
                {
                    value = short.MaxValue;
                }

                this.ValueEntry.SmartValue = (short)value;
            }
        }
    }

    /// <summary>
    /// Draws int properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeInt32Drawer : OdinAttributeDrawer<PropertyRangeAttribute, int>
    {
        private ValueResolver<int> getterMinValue;
        private ValueResolver<int> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<int>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<int>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            int min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (int)this.Attribute.Min;
            int max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (int)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, this.ValueEntry.SmartValue, Mathf.Min(min, max), Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                if (value < int.MinValue)
                {
                    value = int.MinValue;
                }
                else if (value > int.MaxValue)
                {
                    value = int.MaxValue;
                }

                this.ValueEntry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeInt64Drawer : OdinAttributeDrawer<PropertyRangeAttribute, long>
    {
        private ValueResolver<long> getterMinValue;
        private ValueResolver<long> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<long>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<long>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            long min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (long)this.Attribute.Min;
            long max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (long)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, (int)this.ValueEntry.SmartValue, (int)Math.Min(min, max), (int)Math.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeSByteDrawer : OdinAttributeDrawer<PropertyRangeAttribute, sbyte>
    {
        private ValueResolver<sbyte> getterMinValue;
        private ValueResolver<sbyte> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<sbyte>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<sbyte>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            sbyte min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (sbyte)this.Attribute.Min;
            sbyte max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (sbyte)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, this.ValueEntry.SmartValue, Mathf.Min(min, max), Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                if (value < sbyte.MinValue)
                {
                    value = sbyte.MinValue;
                }
                else if (value > sbyte.MaxValue)
                {
                    value = sbyte.MaxValue;
                }

                this.ValueEntry.SmartValue = (sbyte)value;
            }
        }
    }

    /// <summary>
    /// Draws ushort properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeUInt16Drawer : OdinAttributeDrawer<PropertyRangeAttribute, ushort>
    {
        private ValueResolver<ushort> getterMinValue;
        private ValueResolver<ushort> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<ushort>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<ushort>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ushort min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (ushort)this.Attribute.Min;
            ushort max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (ushort)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, this.ValueEntry.SmartValue, Mathf.Min(min, max), Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                if (value < ushort.MinValue)
                {
                    value = ushort.MinValue;
                }
                else if (value > ushort.MaxValue)
                {
                    value = ushort.MaxValue;
                }

                this.ValueEntry.SmartValue = (ushort)value;
            }
        }
    }

    /// <summary>
    /// Draws uint properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeUInt32Drawer : OdinAttributeDrawer<PropertyRangeAttribute, uint>
    {
        private ValueResolver<uint> getterMinValue;
        private ValueResolver<uint> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<uint>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<uint>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            uint min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (uint)this.Attribute.Min;
            uint max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (uint)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, (int)this.ValueEntry.SmartValue, (int)Mathf.Min(min, max), (int)Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                if (value < uint.MinValue)
                {
                    value = (int)uint.MinValue;
                }
                else
                {
                    this.ValueEntry.SmartValue = (uint)value;
                }

                this.ValueEntry.SmartValue = (uint)value;
            }
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class PropertyRangeAttributeUInt64Drawer : OdinAttributeDrawer<PropertyRangeAttribute, ulong>
    {
        private ValueResolver<ulong> getterMinValue;
        private ValueResolver<ulong> getterMaxValue;

        /// <summary>
        /// Initialized the drawer.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.MinGetter != null)
            {
                this.getterMinValue = ValueResolver.Get<ulong>(this.Property, this.Attribute.MinGetter);
            }
            if (this.Attribute.MaxGetter != null)
            {
                this.getterMaxValue = ValueResolver.Get<ulong>(this.Property, this.Attribute.MaxGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ulong min = this.getterMinValue != null ? this.getterMinValue.GetValue() : (ulong)this.Attribute.Min;
            ulong max = this.getterMaxValue != null ? this.getterMaxValue.GetValue() : (ulong)this.Attribute.Max;

            if (this.getterMinValue != null && this.getterMinValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMinValue.ErrorMessage);
            }
            if (this.getterMaxValue != null && this.getterMaxValue.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.getterMaxValue.ErrorMessage);
            }

            EditorGUI.BeginChangeCheck();
            int value = SirenixEditorFields.RangeIntField(label, (int)this.ValueEntry.SmartValue, (int)Mathf.Min(min, max), (int)Mathf.Max(min, max));
            if (EditorGUI.EndChangeCheck())
            {
                if (value < (int)ulong.MinValue)
                {
                    value = (int)ulong.MinValue;
                }
                else
                {
                    this.ValueEntry.SmartValue = (ulong)value;
                }

                this.ValueEntry.SmartValue = (ulong)value;
            }
        }
    }
}
#endif