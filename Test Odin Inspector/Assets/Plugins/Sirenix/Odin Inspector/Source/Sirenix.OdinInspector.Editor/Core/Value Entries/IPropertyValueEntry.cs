#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IPropertyValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty"/>, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    public interface IPropertyValueEntry : IDisposable
    {
        /// <summary>
        /// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="PropertyTree.WeakTargets"/>.
        /// </summary>
        int ValueCount { get; }

        /// <summary>
        /// Whether this value entry is editable or not.
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        /// If this value entry has the override type <see cref="PropertyValueState.Reference"/>, this is the path of the property it references.
        /// </summary>
        string TargetReferencePath { get; }

        /// <summary>
        /// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
        /// <para>Note that this is *not* always equal to <see cref="InspectorPropertyInfo.SerializationBackend"/>.</para>
        /// </summary>
        SerializationBackend SerializationBackend { get; }

        /// <summary>
        /// The property whose values this value entry represents.
        /// </summary>
        InspectorProperty Property { get; }

        /// <summary>
        /// Provides access to the weakly typed values of this value entry.
        /// </summary>
        IPropertyValueCollection WeakValues { get; }

        /// <summary>
        /// Whether this value entry has been changed from its prefab counterpart.
        /// </summary>
        bool ValueChangedFromPrefab { get; }

        /// <summary>
        /// Whether this value entry has had its list length changed from its prefab counterpart.
        /// </summary>
        bool ListLengthChangedFromPrefab { get; }

        /// <summary>
        /// Whether this value entry has had its dictionary values changes from its prefab counterpart.
        /// </summary>
        bool DictionaryChangedFromPrefab { get; }

        /// <summary>
        /// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        object WeakSmartValue { get; set; }

        /// <summary>
        /// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
        /// </summary>
        Type ParentType { get; }

        /// <summary>
        /// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="BaseValueType"/>.
        /// </summary>
        Type TypeOfValue { get; }

        /// <summary>
        /// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
        /// </summary>
        Type BaseValueType { get; }

        /// <summary>
        /// The special state of the value entry.
        /// </summary>
        PropertyValueState ValueState { get; }

        /// <summary>
        /// Whether this value entry is an alias, or not. Value entry aliases are used to provide strongly typed value entries in the case of polymorphism.
        /// </summary>
        bool IsAlias { get; }

        /// <summary>
        /// The context container of this property.
        /// </summary>
        PropertyContextContainer Context { get; }

        /// <summary>
        /// Whether this type is marked as an atomic type using a <see cref="IAtomHandler"/>.
        /// </summary>
        bool IsMarkedAtomic { get; }

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges"/>, when any values have changed.
        /// </summary>
        event Action<int> OnValueChanged;

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges"/>, when any child values have changed.
        /// </summary>
        event Action<int> OnChildValueChanged;

        /// <summary>
        /// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
        /// </summary>
        void Update();

        /// <summary>
        /// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
        /// </summary>
        /// <returns>True if any changes were made, otherwise, false.</returns>
        bool ApplyChanges();

        /// <summary>
        /// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
        /// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
        /// </summary>
        bool ValueTypeValuesAreEqual(IPropertyValueEntry other);

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="Utilities.TypeExtensions.GetEqualityComparerDelegate{T}"/> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        bool ValueIsPrefabDifferent(object value, int index);
    }

    /// <summary>
    /// Represents the strongly typed values of an <see cref="InspectorProperty"/>, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    public interface IPropertyValueEntry<TValue> : IPropertyValueEntry
    {
        /// <summary>
        /// Provides access to the strongly typed values of this value entry.
        /// </summary>
        IPropertyValueCollection<TValue> Values { get; }

        /// <summary>
        /// <para>A strongly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        TValue SmartValue { get; set; }

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="Utilities.TypeExtensions.GetEqualityComparerDelegate{T}"/> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        bool ValueIsPrefabDifferent(TValue value, int index);
    }
}
#endif