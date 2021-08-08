#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityPropertyDrawer.cs" company="Sirenix IVS">
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
    /// Unity property drawer.
    /// </summary>
    [DrawerPriority(0, 0, 0.5), OdinDontRegisterAttribute]
    public class UnityPropertyDrawer<TDrawer, TDrawnType> : OdinValueDrawer<TDrawnType> where TDrawer : UnityEditor.PropertyDrawer, new()
    {
        private static readonly FieldInfo InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", Flags.InstanceAnyVisibility);
        private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

        protected TDrawer drawer;
        protected object propertyHandler;
        protected bool delayApplyValueUntilRepaint;

        /// <summary>
        /// Initializes the property drawer.
        /// </summary>
        public UnityPropertyDrawer()
        {
            this.drawer = new TDrawer();

            if (UnityPropertyHandlerUtility.IsAvailable)
            {
                this.propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(this.drawer);
            }
        }

        static UnityPropertyDrawer()
        {
            if (InternalFieldInfoFieldInfo == null)
            {
                Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(UnityPropertyDrawer<TDrawer, TDrawnType>).GetNiceName() + "' has been disabled.");
            }
            else
            {
                SetFieldInfo = EmitUtilities.CreateInstanceFieldSetter<TDrawer, FieldInfo>(InternalFieldInfoFieldInfo);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            if (SetFieldInfo == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(UnityPropertyDrawer<TDrawer, TDrawnType>).GetNiceName() + "' has been disabled.");
                return;
            }

            FieldInfo fieldInfo;
            SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                if (UnityVersion.IsVersionOrGreater(2017, 1))
                {
                    this.CallNextDrawer(label);
                }
                else
                {
                    SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
                }

                return;
            }

            label = label ?? GUIContent.none;
            SetFieldInfo(ref this.drawer, fieldInfo);

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<TDrawnType>)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<TDrawnType> target = (EmittedScriptableObject<TDrawnType>)targetObjects[i];
                    target.SetValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
            }
            else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                    target.SetWeakValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
                unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            }

            float height;

            if (this.propertyHandler != null)
            {
                height = UnityPropertyHandlerUtility.PropertyHandlerGetHeight(this.propertyHandler, unityProperty.Copy(), label, false);
            }
            else
            {
                height = this.drawer.GetPropertyHeight(unityProperty.Copy(), label);
            }

            Rect position = EditorGUILayout.GetControlRect(false, height);

            EditorGUI.BeginChangeCheck();
            
            if (this.propertyHandler != null)
            {
                UnityPropertyHandlerUtility.PropertyHandlerOnGUI(this.propertyHandler, position, unityProperty, label, false);
            }
            else
            {
                this.drawer.OnGUI(position, unityProperty, label);
            }

            bool changed = EditorGUI.EndChangeCheck();

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<TDrawnType>)
            {
                if (unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed)
                {
                    if (this.delayApplyValueUntilRepaint)
                    {
                        this.Property.Tree.DelayActionUntilRepaint(() => ApplyValueStrong(entry, unityProperty));
                    }
                    else
                    {
                        ApplyValueStrong(entry, unityProperty);
                    }
                }
            }
            else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
            {
                if (unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed)
                {
                    if (this.delayApplyValueUntilRepaint)
                    {
                        this.Property.Tree.DelayActionUntilRepaint(() => ApplyValueWeak(entry, unityProperty));
                    }
                    else
                    {
                        ApplyValueWeak(entry, unityProperty);
                    }
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

        private static void ApplyValueWeak(IPropertyValueEntry<TDrawnType> entry, SerializedProperty unityProperty)
        {
            var targetObjects = unityProperty.serializedObject.targetObjects;

            for (int i = 0; i < targetObjects.Length; i++)
            {
                EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                entry.Values[i] = (TDrawnType)target.GetWeakValue();
            }

            entry.Values.ForceMarkDirty();
        }

        private static void ApplyValueStrong(IPropertyValueEntry<TDrawnType> entry, SerializedProperty unityProperty)
        {
            var targetObjects = unityProperty.serializedObject.targetObjects;

            for (int i = 0; i < targetObjects.Length; i++)
            {
                EmittedScriptableObject<TDrawnType> target = (EmittedScriptableObject<TDrawnType>)targetObjects[i];
                entry.Values[i] = target.GetValue();
            }

            entry.Values.ForceMarkDirty();
        }
    }
}
#endif