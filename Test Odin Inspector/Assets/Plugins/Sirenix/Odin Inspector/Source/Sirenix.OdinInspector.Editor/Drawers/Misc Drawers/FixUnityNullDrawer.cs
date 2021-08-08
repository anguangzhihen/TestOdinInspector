#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FixUnityNullDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;

    [DrawerPriority(10, 0, 0)]
    public sealed class FixUnityNullDrawer<T> : OdinValueDrawer<T> where T : class
    {
        public override bool CanDrawTypeFilter(Type type)
        {
            return !typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            bool valueNeedsFixing = entry.ValueState == PropertyValueState.NullReference &&
                                    !entry.SerializationBackend.SupportsPolymorphism;

            if (valueNeedsFixing)
            {
                bool possibleRecursion = false;

                var prop = entry.Property.Parent;

                while (prop != null)
                {
                    if (prop.ValueEntry != null && (prop.ValueEntry.TypeOfValue == typeof(T) || prop.ValueEntry.BaseValueType == typeof(T)))
                    {
                        // We have a possible recursion
                        possibleRecursion = true;
                        break;
                    }

                    prop = prop.Parent;
                }

                if (possibleRecursion)
                {
                    SirenixEditorGUI.ErrorMessageBox("Possible Unity serialization recursion detected; cutting off drawing pre-emptively.");
                    return; // Get out of here
                }

                // If no recursion, fix value in layout
                if (Event.current.type == EventType.Layout)
                {
                    SerializedObject serializedObject = null;

                    if (Property.Info.IsUnityPropertyOnly)
                    {
                        // This forces SerializedObject update as a side effect of getting the property if there is a SerializedObject
                        // So it is important to get it before we make any changes here, so that changes will be correctly registered in Unity-backed properties.
                        serializedObject = Property.Tree.UnitySerializedObject;
                    }

                    for (int i = 0; i < entry.ValueCount; i++)
                    {
                        object value = UnitySerializationUtility.CreateDefaultUnityInitializedObject(typeof(T));
                        entry.WeakValues.ForceSetValue(i, value);
                    }

                    Property.RecordForUndo("Odin fixing null Unity-backed values");

                    entry.ApplyChanges();

                    var tree = Property.Tree;

                    if (Property.Info.IsUnityPropertyOnly && serializedObject != null)
                    {
                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }

                    Property.Update(true);
                }
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif