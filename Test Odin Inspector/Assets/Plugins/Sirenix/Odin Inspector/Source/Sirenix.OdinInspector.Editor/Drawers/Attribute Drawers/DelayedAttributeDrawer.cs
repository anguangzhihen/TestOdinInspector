#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DelayedAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws char properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeCharDrawer : OdinAttributeDrawer<DelayedAttribute, char>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            string s = new string(this.ValueEntry.SmartValue, 1);
            s = SirenixEditorFields.DelayedTextField(label, s, GUILayoutOptions.MinWidth(0));

            if (EditorGUI.EndChangeCheck() && s.Length > 0)
            {
                this.ValueEntry.SmartValue = s[0];
            }
        }
    }

    /// <summary>
    /// Draws string properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeStringDrawer : OdinAttributeDrawer<DelayedAttribute, string>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.ValueEntry.SmartValue = SirenixEditorFields.DelayedTextField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeSByteDrawer : OdinAttributeDrawer<DelayedAttribute, sbyte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));

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

    /// <summary>
    /// Draws byte properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeByteDrawer : OdinAttributeDrawer<DelayedAttribute, byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));

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

    /// <summary>
    /// Draws short properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeInt16Drawer : OdinAttributeDrawer<DelayedAttribute, short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));

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

    /// <summary>
    /// Draws ushort properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeUInt16Drawer : OdinAttributeDrawer<DelayedAttribute, ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));

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

    /// <summary>
    /// Draws int properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeInt32Drawer : OdinAttributeDrawer<DelayedAttribute, int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.ValueEntry.SmartValue = SirenixEditorFields.DelayedIntField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));
        }
    }

    /// <summary>
    /// Draws uint properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeUInt32Drawer : OdinAttributeDrawer<DelayedAttribute, uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            long value = SirenixEditorFields.DelayedLongField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));

            if (value < uint.MinValue)
            {
                value = uint.MinValue;
            }
            else if (value > uint.MaxValue)
            {
                value = uint.MaxValue;
            }

            this.ValueEntry.SmartValue = (uint)value;
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeInt64Drawer : OdinAttributeDrawer<DelayedAttribute, long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.ValueEntry.SmartValue = SirenixEditorFields.DelayedLongField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeUInt64Drawer : OdinAttributeDrawer<DelayedAttribute, ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ulong value = this.ValueEntry.SmartValue;
            string str = value.ToString();

            str = label == null ?
                EditorGUILayout.DelayedTextField(str, GUILayoutOptions.MinWidth(0)) :
                EditorGUILayout.DelayedTextField(label, str, GUILayoutOptions.MinWidth(0));

            if (GUI.changed && ulong.TryParse(str, out value))
            {
                this.ValueEntry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeFloatDrawer : OdinAttributeDrawer<DelayedAttribute, float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.ValueEntry.SmartValue = SirenixEditorFields.DelayedFloatField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeDoubleDrawer : OdinAttributeDrawer<DelayedAttribute, double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.ValueEntry.SmartValue = SirenixEditorFields.DelayedDoubleField(label, this.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0));
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    public sealed class DelayedAttributeDecimalDrawer : OdinAttributeDrawer<DelayedAttribute, decimal>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            decimal value = this.ValueEntry.SmartValue;
            string str = value.ToString();

            str = SirenixEditorFields.DelayedTextField(label, str, GUILayoutOptions.MinWidth(0));

            if (GUI.changed && decimal.TryParse(str, out value))
            {
                this.ValueEntry.SmartValue = value;
            }
        }
    }
}
#endif