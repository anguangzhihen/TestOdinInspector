#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorUsage32AttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws Color properties marked with <see cref="UnityEngine.ColorUsageAttribute"/>.
    /// </summary>
    public sealed class ColorUsage32AttributeDrawer : OdinAttributeDrawer<ColorUsageAttribute, Color32>, IDefinesGenericMenuItems
    {
        private ColorPickerHDRConfig pickerConfig;

        protected override void Initialize()
        {
            this.pickerConfig = new ColorPickerHDRConfig(
                this.Attribute.minBrightness,
                this.Attribute.maxBrightness,
                this.Attribute.minExposureValue,
                this.Attribute.maxExposureValue);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null);

            bool disableContext = false;

            if (Event.current.OnMouseDown(rect, 1, false))
            {
                // Disable Unity's color field's own context menu
                GUIHelper.PushEventType(EventType.Used);
                disableContext = true;
            }

#pragma warning disable 0618 // Type or member is obsolete
            this.ValueEntry.SmartValue = EditorGUI.ColorField(rect, label ?? GUIContent.none, this.ValueEntry.SmartValue, true, this.Attribute.showAlpha, this.Attribute.hdr, this.pickerConfig);
#pragma warning restore 0618 // Type or member is obsolete

            if (disableContext)
            {
                GUIHelper.PopEventType();
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            ColorDrawer.PopulateGenericMenu((IPropertyValueEntry<Color32>)property.ValueEntry, genericMenu);
        }
    }
}
#endif