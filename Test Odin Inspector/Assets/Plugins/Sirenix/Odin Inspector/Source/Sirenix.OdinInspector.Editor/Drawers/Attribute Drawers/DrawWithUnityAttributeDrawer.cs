#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using Utilities.Editor;

    /// <summary>
    /// Draws properties marked with <see cref="DrawWithUnityAttribute"/>.
    /// </summary>
    /// <seealso cref="RequireComponent"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    /// <seealso cref="InlineEditorAttribute"/>
    /// <seealso cref="HideInInspector"/>

    [DrawerPriority(0, 0, 6000)]
    public class DrawWithUnityAttributeDrawer<T> : OdinAttributeDrawer<DrawWithUnityAttribute, T>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            FieldInfo fieldInfo;
            SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
                return;
            }

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    target.SetValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
                unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(unityProperty, label ?? GUIContent.none, true);
            bool changed = EditorGUI.EndChangeCheck();

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    entry.Values[i] = target.GetValue();
                }

                if (changed)
                {
                    entry.Values.ForceMarkDirty();
                }
            }
            else if (changed)
            {
                this.Property.Tree.DelayActionUntilRepaint(() =>
                {
                    var baseEntry = this.Property.BaseValueEntry;

                    for (int i = 0; i < baseEntry.ValueCount; i++)
                    {
                        (baseEntry as PropertyValueEntry).TriggerOnValueChanged(i);
                    }
                });
            }
        }
    }
}
#endif