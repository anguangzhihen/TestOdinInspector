#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ReferenceValueConflictDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// <para>
    /// When multiple objects are selected and inspected, this his drawer ensures UnityEditor.EditorGUI.showMixedValue
    /// gets set to true if there are any conflicts in the selection for any given property.
    /// Otherwise the next drawer is called.
    /// </para>
    /// <para>This drawer also implements <see cref="IDefinesGenericMenuItems"/> and provides a right-click context menu item for resolving conflicts if any.</para>
    /// </summary>
    [DrawerPriority(0.5, 0, 0)]
    [AllowGUIEnabledForReadonly]
    public sealed class ReferenceValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
    {
        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return !property.IsTreeRoot && property.Tree.WeakTargets.Count > 1;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            if (entry.ValueState == PropertyValueState.ReferenceValueConflict)
            {
                GUIHelper.PushGUIEnabled(GUI.enabled && entry.IsEditable);

                if (typeof(UnityEngine.Object).IsAssignableFrom(entry.TypeOfValue))
                {
                    bool prev = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = true;
                    this.CallNextDrawer(label);
                    EditorGUI.showMixedValue = prev;
                }
                else
                {
                    bool prev = EditorGUI.showMixedValue;

                    EditorGUI.showMixedValue = true;
                    entry.SmartValue = SirenixEditorFields.PolymorphicObjectField(label, entry.SmartValue, entry.BaseValueType, entry.Property.GetAttribute<AssetsOnlyAttribute>() == null) as T;
                    EditorGUI.showMixedValue = prev;
                }

                GUIHelper.PopGUIEnabled();
            }
            else
            {
                this.CallNextDrawer(label);
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (property.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
            {
                var tree = property.Tree;

                if (typeof(UnityEngine.Object).IsAssignableFrom(tree.TargetType))
                {
                    for (int i = 0; i < tree.WeakTargets.Count; i++)
                    {
                        object value = property.ValueEntry.WeakValues[i];
                        string valueString = value == null ? "null" : value.GetType().GetNiceName();
                        string contentString = "Resolve type conflict with.../" + ((UnityEngine.Object)tree.WeakTargets[i]).name + " (" + valueString + ")";

                        genericMenu.AddItem(new GUIContent(contentString), false, () =>
                        {
                            property.Tree.DelayActionUntilRepaint(() =>
                            {
                                for (int j = 0; j < property.ValueEntry.WeakValues.Count; j++)
                                {
                                    property.ValueEntry.WeakValues[j] = value;
                                }
                            });
                        });
                    }
                }
            }
        }
    }
}
#endif