#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PreviewFieldAttributeDrawer.cs" company="Sirenix IVS">
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
    /// Draws properties marked with <see cref="PreviewFieldAttribute"/> as a square ObjectField which renders a preview for UnityEngine.Object types.
    /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
    /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
    /// </summary>

    [AllowGUIEnabledForReadonly]
    public sealed class PreviewFieldAttributeDrawer<T> : OdinAttributeDrawer<PreviewFieldAttribute, T>
        where T : UnityEngine.Object
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUI.BeginChangeCheck();

            ObjectFieldAlignment alignment;

            if (this.Attribute.AlignmentHasValue)
            {
                alignment = (ObjectFieldAlignment)this.Attribute.Alignment;
            }
            else
            {
                alignment = GeneralDrawerConfig.Instance.SquareUnityObjectAlignment;
            }

            this.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(
                label,
                this.ValueEntry.WeakSmartValue as UnityEngine.Object,
                this.ValueEntry.BaseValueType,
                this.ValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null,
                this.Attribute.Height == 0 ? GeneralDrawerConfig.Instance.SquareUnityObjectFieldHeight : this.Attribute.Height,
                alignment);

            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.Values.ForceMarkDirty();
            }
        }
    }
}
#endif