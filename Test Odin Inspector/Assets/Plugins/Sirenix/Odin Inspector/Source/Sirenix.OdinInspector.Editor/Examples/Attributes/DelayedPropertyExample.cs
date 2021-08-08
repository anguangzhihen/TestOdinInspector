#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DelayedPropertyExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(DelayedAttribute))]
    [AttributeExample(typeof(DelayedPropertyAttribute))]
    internal class DelayedPropertyExample
    {
        // Delayed and DelayedProperty attributes are virtually identical...
        [Delayed]
        [OnValueChanged("OnValueChanged")]
        public int DelayedField;

        // ... but the DelayedProperty can, as the name suggests, also be applied to properties.
        [ShowInInspector, DelayedProperty]
        [OnValueChanged("OnValueChanged")]
        public string DelayedProperty { get; set; }

        private void OnValueChanged()
        {
            Debug.Log("Value changed!");
        }
    }
}
#endif