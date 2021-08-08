#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="LayerMaskDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// LayerMask property drawer.
    /// </summary>
    public class LayerMaskDrawer : OdinValueDrawer<LayerMask>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            //EditorGUI.BeginChangeCheck();

            entry.SmartValue = SirenixEditorFields.LayerMaskField(label, entry.SmartValue);

            //return;
            //FieldInfo fieldInfo;
            //SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            //if (unityProperty == null)
            //{
            //    SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
            //    return;
            //}

            //if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<LayerMask>)
            //{
            //    var targetObjects = unityProperty.serializedObject.targetObjects;

            //    for (int i = 0; i < targetObjects.Length; i++)
            //    {
            //        EmittedScriptableObject<LayerMask> target = (EmittedScriptableObject<LayerMask>)targetObjects[i];
            //        target.SetValue(entry.Values[i]);
            //    }

            //    unityProperty.serializedObject.Update();
            //    unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            //}

            //EditorGUILayout.PropertyField(unityProperty, true);

            //if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<LayerMask>)
            //{
            //    unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            //    var targetObjects = unityProperty.serializedObject.targetObjects;

            //    for (int i = 0; i < targetObjects.Length; i++)
            //    {
            //        EmittedScriptableObject<LayerMask> target = (EmittedScriptableObject<LayerMask>)targetObjects[i];
            //        entry.Values[i] = target.GetValue();
            //    }
            //}
        }
    }
}
#endif