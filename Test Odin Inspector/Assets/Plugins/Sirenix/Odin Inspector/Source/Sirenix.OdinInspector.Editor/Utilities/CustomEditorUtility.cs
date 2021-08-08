#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CustomEditorUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Utilities;
    using UnityEngine;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Collections;

    public static class CustomEditorUtility
    {
        private static Type CustomEditorAttributesType;
        private static Type MonoEditorType;

        private static readonly FieldInfo CustomEditorAttributesType_CachedEditorForType;
        private static readonly FieldInfo CustomEditorAttributesType_CachedMultiEditorForType;
        private static readonly FieldInfo CustomEditorAttributesType_CustomEditors;
        private static readonly FieldInfo CustomEditorAttributesType_CustomMultiEditors;
        private static readonly FieldInfo CustomEditorAttributesType_Initialized;
        private static readonly FieldInfo CustomEditor_EditorForChildClassesField;

        private static readonly FieldInfo MonoEditorType_InspectedType;
        private static readonly FieldInfo MonoEditorType_InspectorType;
        private static readonly FieldInfo MonoEditorType_EditorForChildClasses;
        private static readonly FieldInfo MonoEditorType_IsFallback;

        private static readonly MethodInfo CustomEditorAttributesType_Rebuild;

        private static readonly bool IsBackedByADictionary;

        private static bool IsValid;

        static CustomEditorUtility()
        {
            try
            {
                CustomEditorAttributesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.CustomEditorAttributes");
                MonoEditorType = CustomEditorAttributesType.GetNestedType("MonoEditorType", BindingFlags.Public | BindingFlags.NonPublic);

                CustomEditorAttributesType_Initialized = CustomEditorAttributesType.GetField("s_Initialized", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                CustomEditorAttributesType_CachedEditorForType = CustomEditorAttributesType.GetField("kCachedEditorForType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                CustomEditorAttributesType_CachedMultiEditorForType = CustomEditorAttributesType.GetField("kCachedMultiEditorForType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                CustomEditorAttributesType_CustomEditors = CustomEditorAttributesType.GetField("kSCustomEditors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                CustomEditorAttributesType_CustomMultiEditors = CustomEditorAttributesType.GetField("kSCustomMultiEditors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                CustomEditorAttributesType_Rebuild = CustomEditorAttributesType.GetMethod("Rebuild", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                MonoEditorType_InspectedType = MonoEditorType.GetField("m_InspectedType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MonoEditorType_InspectorType = MonoEditorType.GetField("m_InspectorType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MonoEditorType_EditorForChildClasses = MonoEditorType.GetField("m_EditorForChildClasses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MonoEditorType_IsFallback = MonoEditorType.GetField("m_IsFallback", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                CustomEditor_EditorForChildClassesField = typeof(UnityEditor.CustomEditor).GetField("m_EditorForChildClasses", Flags.InstanceAnyVisibility);

                // CustomEditorAttributesType_SingleTypeDictField and CustomEditorAttributesType_MultiTypeDictField can be null
                if (CustomEditorAttributesType_Initialized == null
                    || CustomEditorAttributesType_CustomEditors == null
                    || CustomEditorAttributesType_CustomMultiEditors == null
                    || MonoEditorType_InspectedType == null
                    || MonoEditorType_InspectorType == null
                    || MonoEditorType_EditorForChildClasses == null
                    || MonoEditorType_IsFallback == null
                    || CustomEditor_EditorForChildClassesField == null)
                {
                    throw new NullReferenceException();
                }

                // This is true for some newer versions of 2017.4 and 2018.x
                IsBackedByADictionary = typeof(IDictionary).IsAssignableFrom(CustomEditorAttributesType_CustomEditors.FieldType);

                IsValid = true;
            }
            catch (NullReferenceException)
            {
                IsValid = false;
                Debug.LogError("Unity's internal custom editor management classes have changed in this version of Unity (" + Application.unityVersion + "). Odin may not be able to draw editors.");
            }
        }

        public static void ResetCustomEditors()
        {
            if (!IsValid) return;

            if (IsBackedByADictionary)
            {
                ((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
                ((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
            }
            else
            {
                if (CustomEditorAttributesType_CachedEditorForType != null)
                {
                    ((Dictionary<Type, Type>)CustomEditorAttributesType_CachedEditorForType.GetValue(null)).Clear();
                }
                if (CustomEditorAttributesType_CachedMultiEditorForType != null)
                {
                    ((Dictionary<Type, Type>)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null)).Clear();
                }
                ((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
                ((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
            }

            if (UnityVersion.IsVersionOrGreater(2019, 1))
            {
                // Manually trigger a Rebuild instead of setting Initialized to false.
                CustomEditorAttributesType_Rebuild.Invoke(null, null);
                CustomEditorAttributesType_Initialized.SetValue(null, true); // Ensure Unity doesn't do a second rebuild again.
            }
            else
            {
                CustomEditorAttributesType_Initialized.SetValue(null, false);
            }
        }

        public static void SetCustomEditor(Type inspectedType, Type editorType)
        {
            if (!IsValid) return;

            var attr = editorType.GetCustomAttribute<UnityEditor.CustomEditor>();
            if (attr == null) throw new ArgumentException("Editor type to set '" + editorType.GetNiceName() + "' has no CustomEditor attribute applied! Use a SetCustomEditor overload that takes isFallbackEditor and isEditorForChildClasses parameters.");
            SetCustomEditor(inspectedType, editorType, attr.isFallback, (bool)CustomEditor_EditorForChildClassesField.GetValue(attr));
        }

        public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses)
        {
            if (!IsValid) return;

            SetCustomEditor(inspectedType, editorType, isFallbackEditor, isEditorForChildClasses, editorType.IsDefined<UnityEditor.CanEditMultipleObjects>());
        }

        public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses, bool isMultiEditor)
        {
            if (!IsValid) return;

            object entry = Activator.CreateInstance(MonoEditorType);

            MonoEditorType_InspectedType.SetValue(entry, inspectedType);
            MonoEditorType_InspectorType.SetValue(entry, editorType);
            MonoEditorType_IsFallback.SetValue(entry, isFallbackEditor);
            MonoEditorType_EditorForChildClasses.SetValue(entry, isEditorForChildClasses);

            if (IsBackedByADictionary)
            {
                AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null), entry, inspectedType);

                if (isMultiEditor)
                {
                    AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null), entry, inspectedType);
                }
            }
            else
            {
                if (CustomEditorAttributesType_CachedEditorForType != null && CustomEditorAttributesType_CachedMultiEditorForType != null)
                {
                    // Just set the dictionary cache
                    ((IDictionary)CustomEditorAttributesType_CachedEditorForType.GetValue(null))[inspectedType] = editorType;

                    if (isMultiEditor)
                    {
                        ((IDictionary)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null))[inspectedType] = editorType;
                    }
                }

                // Insert a new type entry at the beginning of the relevant lists
                {
                    ((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Insert(0, entry);

                    if (isMultiEditor)
                    {
                        ((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Insert(0, entry);
                    }
                }
            }
        }

        private static void AddEntryToDictList(IDictionary dict, object entry, Type inspectedType)
        {
            IList list;

            if (dict.Contains(inspectedType))
            {
                list = (IList)dict[inspectedType];
            }
            else
            {
                list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(MonoEditorType));
                dict[inspectedType] = list;
            }

            list.Insert(0, entry);
        }
    }
}
#endif