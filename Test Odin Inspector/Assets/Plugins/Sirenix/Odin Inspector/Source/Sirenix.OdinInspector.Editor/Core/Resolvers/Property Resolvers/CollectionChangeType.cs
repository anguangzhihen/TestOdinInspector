#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CollectionChangeType.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Specifies the kinds of changes that can occur to collections.
    /// </summary>
    public enum CollectionChangeType
    {
        /// <summary>
        /// Unknown collection change, the change was not specified by the invoking code.
        /// </summary>
        Unspecified,
        /// <summary>
        /// The change is adding a value to the collection. Value and SelectionIndex will be set.
        /// </summary>
        Add,
        /// <summary>
        /// The change is inserting a value into the collection. Index, Value and SelectionIndex will be set.
        /// </summary>
        Insert,
        /// <summary>
        /// The change is removing a value from the collection. Value and SelectionIndex will be set.
        /// </summary>
        RemoveValue,
        /// <summary>
        /// The change is removing a value at an index from the collection. Index and SelectionIndex will be set.
        /// </summary>
        RemoveIndex,
        /// <summary>
        /// The change is clearing the collection. SelectionIndex will be set.
        /// </summary>
        Clear,
        /// <summary>
        /// The change is removing a key from the collection. Key and SelectionIndex will be set.
        /// </summary>
        RemoveKey,
        /// <summary>
        /// The change is setting the value of a key in the collection. Key, Value and SelectionIndex will be set.
        /// </summary>
        SetKey,
    }
}
#endif