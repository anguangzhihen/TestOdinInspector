#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IValueEntryActualValueSetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// <para>An internally used interface that is used by value entries during <see cref="PropertyValueEntry.ApplyChanges"/>, that lets value entries force each other to set values directly to their target objects.</para>
    /// <para>This interface should not be used by people who do not know what they are doing.</para>
    /// </summary>
    public interface IValueEntryActualValueSetter
    {
        /// <summary>
        /// Sets the actual value of a value entry, for a given selection index.
        /// </summary>
        void SetActualValue(int index, object value);
    }

    /// <summary>
    /// <para>An internally used interface that is used by value entries during <see cref="PropertyValueEntry.ApplyChanges"/>, that lets value entries force each other to set values directly to their target objects.</para>
    /// <para>This interface should not be used by people who do not know what they are doing.</para>
    /// </summary>
    public interface IValueEntryActualValueSetter<TValue> : IValueEntryActualValueSetter
    {
        /// <summary>
        /// Sets the actual value of a value entry, for a given selection index.
        /// </summary>
        void SetActualValue(int index, TValue value);
    }
}
#endif