#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IValueGetterSetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// Used by all InspectorProperty to tell Odin how to set or get a value on any given property.
    /// </summary>
    public interface IValueGetterSetter
    {
        /// <summary>
        /// Whether the value is readonly.
        /// </summary>
        bool IsReadonly { get; }

        /// <summary>
        /// Gets the type of the owner.
        /// </summary>
        Type OwnerType { get; }

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Sets the weakly typed value on a given weakly typed owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        void SetValue(object owner, object value);

        /// <summary>
        /// Gets the value from a given weakly typed owner.
        /// </summary>
        /// <param name="owner">The weakly typed owner.</param>
        /// <returns>The found value.</returns>
        object GetValue(object owner);
    }

    /// <summary>
    /// Used by all <see cref="AliasGetterSetter{TOwner, TValue, TPropertyOwner, TPropertyValue}"/> to tell Odin how to set or get a value on any given property.
    /// </summary>
    public interface IValueGetterSetter<TOwner, TValue> : IValueGetterSetter
    {
        /// <summary>
        /// Sets the value on a given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        void SetValue(ref TOwner owner, TValue value);

        /// <summary>
        /// Gets the value from a given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        TValue GetValue(ref TOwner owner);
    }
}
#endif