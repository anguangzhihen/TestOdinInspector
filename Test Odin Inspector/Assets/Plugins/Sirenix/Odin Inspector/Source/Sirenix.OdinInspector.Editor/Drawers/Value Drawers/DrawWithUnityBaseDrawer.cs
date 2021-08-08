#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityBaseDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Base class to derive from for value drawers that merely wish to cause a value to be drawn by Unity.
    /// </summary>
    public abstract class DrawWithUnityBaseDrawer<T> : OdinValueDrawer<T>
    {
        //private static readonly bool IsAtomic = AtomHandlerLocator.IsMarkedAtomic(typeof(T));

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

            bool isEmittedProperty = unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>;

            if (isEmittedProperty)
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

            if (label == null)
            {
                label = GUIContent.none;
            }

            if (!isEmittedProperty) EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(unityProperty, label, true);
            if (!isEmittedProperty && EditorGUI.EndChangeCheck())
            {
                entry.Values.ForceMarkDirty();
            }
            //else if (!isEmittedProperty && IsAtomic)
            //{
            //    for (int i = 0; i < entry.ValueCount; i++)
            //    {
            //        entry.Values[i] = entry.Values[i];
            //    }
            //}

            if (isEmittedProperty)
            {
                unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    entry.Values[i] = target.GetValue();
                }
            }
        }
    }
}
#endif