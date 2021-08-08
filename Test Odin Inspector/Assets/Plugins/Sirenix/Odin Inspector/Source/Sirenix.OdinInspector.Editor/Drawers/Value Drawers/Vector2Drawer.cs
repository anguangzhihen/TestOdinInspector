#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Vector2Drawer.cs" company="Sirenix IVS">
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
    /// Vector2 proprety drawer.
    /// </summary>
    public sealed class Vector2Drawer : OdinValueDrawer<Vector2>, IDefinesGenericMenuItems
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
                this.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            Vector2 value = (Vector2)property.ValueEntry.WeakSmartValue;

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(value.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0)"), value == Vector2.zero, () => SetVector(property, Vector2.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1)"), value == Vector2.one, () => SetVector(property, Vector2.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0)"), value == Vector2.right, () => SetVector(property, Vector2.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0)"), value == Vector2.left, () => SetVector(property, Vector2.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1)"), value == Vector2.up, () => SetVector(property, Vector2.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1)"), value == Vector2.down, () => SetVector(property, Vector2.down));
        }

        private void SetVector(InspectorProperty property, Vector2 value)
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
                    property.ValueEntry.WeakValues[i] = ((Vector2)property.ValueEntry.WeakValues[i]).normalized;
                }
            });
        }
    }
}
#endif