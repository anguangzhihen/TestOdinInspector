#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EmittedMonoBehaviour.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Base class for emitted MonoBehaviour-derived types that have been created by the <see cref="UnityPropertyEmitter"/>.
    /// </summary>
    public abstract class EmittedMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The field that backs the value of this MonoBehaviour.
        /// </summary>
        public abstract FieldInfo BackingFieldInfo { get; }

        /// <summary>
        /// Sets the value contained in this scriptable object.
        /// </summary>
        public abstract void SetWeakValue(object value);

        /// <summary>
        /// Gets the value contained in this scriptable object.
        /// </summary>
        public abstract object GetWeakValue();
    }

    /// <summary>
    /// Strongly typed base class for emitted MonoBehaviour-derived types that have been created by the <see cref="UnityPropertyEmitter"/>.
    /// </summary>
    public abstract class EmittedMonoBehaviour<T> : EmittedMonoBehaviour
    {
        /// <summary>
        /// Sets the value contained in this scriptable object.
        /// </summary>
        public override void SetWeakValue(object value)
        {
            this.SetValue((T)value);
        }

        /// <summary>
        /// Gets the value contained in this scriptable object.
        /// </summary>
        public override object GetWeakValue()
        {
            return this.GetValue();
        }

        /// <summary>
        /// Sets the value contained in this scriptable object.
        /// </summary>
        public abstract void SetValue(T value);

        /// <summary>
        /// Gets the value contained in this scriptable object.
        /// </summary>
        public abstract T GetValue();
    }
}
#endif