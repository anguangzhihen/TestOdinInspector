#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IPropertyValueCollection.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a weakly typed collection of values for a <see cref="PropertyValueEntry"/> - one value per selected inspector target.
    /// </summary>
    public interface IPropertyValueCollection : IList
    {
        /// <summary>
        /// Whether the values have been changed since <see cref="MarkClean"/> was last called.
        /// </summary>
        bool AreDirty { get; }

        /// <summary>
        /// Marks the value collection as being clean again. This is typically called at the end of the current GUI frame, during <see cref="PropertyValueEntry.ApplyChanges"/>.
        /// </summary>
        void MarkClean();

        /// <summary>
        /// Marks the value collection as being dirty, regardless of any value changes.
        /// </summary>
        void ForceMarkDirty();

        /// <summary>
        /// Reverts the value collection to its origin values (found in <see cref="Original"/>) from the last <see cref="PropertyValueEntry.Update"/> call, and marks the value collection as being clean again.
        /// </summary>
        void RevertUnappliedValues();

        /// <summary>
        /// <para>Force sets the value, ignoring whether it is editable or not.</para>
        /// <para>Note that this will fail on list element value entries where <see cref="IPropertyValueEntry.ListIsReadOnly"/> is true on the parent value entry.</para>
        /// </summary>
        /// <param name="index">The selection index of the value.</param>
        /// <param name="value">The value to be set.</param>
        void ForceSetValue(int index, object value);

        /// <summary>
        /// The original values of the value collection, such as they were immediately after the last <see cref="PropertyValueEntry.Update"/> call.
        /// </summary>
        IImmutableList Original { get; }
    }

    /// <summary>
    /// Represents a strongly typed collection of values for a <see cref="PropertyValueEntry{T}"/> - one value per selected inspector target.
    /// </summary>
    public interface IPropertyValueCollection<T> : IPropertyValueCollection, IList<T>
    {
        /// <summary>
        /// Gets the value at the given selection index.
        /// </summary>
        new T this[int index] { get; set; }

        /// <summary>
        /// The number of values in the collection.
        /// </summary>
        new int Count { get; }

        /// <summary>
        /// The original values of the value collection, such as they were immediately after the last <see cref="PropertyValueEntry.Update"/> call.
        /// </summary>
        new IImmutableList<T> Original { get; }

        /// <summary>
        /// <para>Force sets the value, ignoring whether it is editable or not.</para>
        /// <para>Note that this will fail on list element value entries where <see cref="IPropertyValueEntry.ListIsReadOnly"/> is true on the parent value entry.</para>
        /// </summary>
        /// <param name="index">The selection index of the value.</param>
        /// <param name="value">The value to be set.</param>
        void ForceSetValue(int index, T value);
    }
}
#endif