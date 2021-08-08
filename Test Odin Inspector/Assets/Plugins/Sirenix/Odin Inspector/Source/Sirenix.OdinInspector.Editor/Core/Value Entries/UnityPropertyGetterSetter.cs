#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityPropertyGetterSetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using UnityEditor;
    using UnityEngine;

    public sealed class UnityPropertyGetterSetter<TOwner, TValue> : IValueGetterSetter<TOwner, TValue>
    {
        private static readonly Func<SerializedProperty, TValue> ValueGetter = SerializedPropertyUtilities.GetValueGetter<TValue>();
        private static readonly Action<SerializedProperty, TValue> ValueSetter = SerializedPropertyUtilities.GetValueSetter<TValue>();

        private InspectorProperty property;

        public UnityPropertyGetterSetter(InspectorProperty property)
        {
            this.property = property;
        }

        public bool IsReadonly { get { return !this.property.Info.IsEditable; } }

        public Type OwnerType
        {
            get
            {
                return typeof(TOwner);
            }
        }

        public Type ValueType
        {
            get
            {
                return typeof(TValue);
            }
        }

        public TValue GetValue(ref TOwner owner)
        {
            if (ValueGetter == null || ValueSetter == null)
            {
                Debug.LogError("Can't get a value of type " + typeof(TValue).GetNiceName() + " directly from a Unity property.");
                return default(TValue);
            }

            var unityProp = this.property.Tree.GetUnityPropertyForPath(this.property.UnityPropertyPath);

            if (unityProp == null || unityProp.serializedObject.targetObject is EmittedScriptableObject)
            {
                Debug.LogError("Could not get Unity property at path " + this.property.UnityPropertyPath + " on root object of type " + this.property.Tree.TargetType.GetNiceName());
                return default(TValue);
            }

            return ValueGetter(unityProp);
        }

        public object GetValue(object owner)
        {
            TOwner castOwner = (TOwner)owner;
            return this.GetValue(ref castOwner);
        }

        public void SetValue(ref TOwner owner, TValue value)
        {
            if (ValueGetter == null || ValueSetter == null)
            {
                Debug.LogError("Can't set a value of type " + typeof(TValue).GetNiceName() + " directly to a Unity property.");
                return;
            }

            var unityProp = this.property.Tree.GetUnityPropertyForPath(this.property.UnityPropertyPath);

            if (unityProp == null || unityProp.serializedObject.targetObject is EmittedScriptableObject)
            {
                Debug.LogError("Could not get Unity property at path " + this.property.UnityPropertyPath);
                return;
            }

            ValueSetter(unityProp, value);
        }

        public void SetValue(object owner, object value)
        {
            TOwner castOwner = (TOwner)owner;
            this.SetValue(ref castOwner, (TValue)value);
        }
    }
}
#endif