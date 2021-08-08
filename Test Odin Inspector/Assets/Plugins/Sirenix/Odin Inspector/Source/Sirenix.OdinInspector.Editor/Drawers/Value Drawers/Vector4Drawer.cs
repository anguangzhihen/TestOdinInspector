#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Vector4Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Vector4 property drawer.
    /// </summary>
    public sealed class Vector4Drawer : OdinValueDrawer<Vector4>, IDefinesGenericMenuItems
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            var contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                EditorGUI.BeginChangeCheck();
                var val = SirenixEditorFields.VectorPrefixSlideRect(labelRect, this.ValueEntry.SmartValue);
                if (EditorGUI.EndChangeCheck())
                {
                    this.ValueEntry.SmartValue = val;
                }

                var showLabels = SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                this.ValueEntry.Property.Children[3].Draw(showLabels ? GUIHelper.TempContent("w") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            Vector4 value = (Vector4)property.ValueEntry.WeakSmartValue;

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(value.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0, 0)"), value == Vector4.zero, () => SetVector(property, Vector3.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1, 1)"), value == Vector4.one, () => SetVector(property, Vector4.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0, 0)"), (Vector3)value == Vector3.right, () => SetVector(property, Vector3.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0, 0)"), (Vector3)value == Vector3.left, () => SetVector(property, Vector3.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0, 0)"), (Vector3)value == Vector3.up, () => SetVector(property, Vector3.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0, 0)"), (Vector3)value == Vector3.down, () => SetVector(property, Vector3.down));
            genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1, 0)"), (Vector3)value == Vector3.forward, () => SetVector(property, Vector3.forward));
            genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1, 0)"), (Vector3)value == Vector3.back, () => SetVector(property, Vector3.back));
        }

        private void SetVector(InspectorProperty property, Vector4 value)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = value;
                }
            });
        }

        private void NormalizeEntries(InspectorProperty property)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = Vector4.Normalize((Vector4)property.ValueEntry.WeakValues[i]);
                }
            });
        }
    }
}
#endif