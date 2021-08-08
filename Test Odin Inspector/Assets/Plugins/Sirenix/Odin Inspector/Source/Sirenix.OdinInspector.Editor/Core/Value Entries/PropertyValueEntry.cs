#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty"/>, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
    public abstract class PropertyValueEntry : IPropertyValueEntry
    {
        private struct TypePairKey
        {
            public Type ParentType;
            public Type ValueType;
        }

        private class TypePairKeyComparer : IEqualityComparer<TypePairKey>
        {
            public bool Equals(TypePairKey x, TypePairKey y)
            {
                return (object.ReferenceEquals(x.ParentType, y.ParentType) || x.ParentType == y.ParentType)
                    && (object.ReferenceEquals(x.ValueType, y.ValueType) || x.ValueType == y.ValueType);
            }

            public int GetHashCode(TypePairKey obj)
            {
                return obj.ParentType.GetHashCode() ^ obj.ValueType.GetHashCode();
            }
        }

        private static readonly Dictionary<TypePairKey, Type> GenericValueEntryVariants_Cache = new Dictionary<TypePairKey, Type>(new TypePairKeyComparer());
        private static readonly Dictionary<Type, Func<PropertyValueEntry>> GenericValueEntryVariants_EmittedCreator_Cache = new Dictionary<Type, Func<PropertyValueEntry>>(FastTypeComparer.Instance);

        private static readonly Dictionary<TypePairKey, Type> GenericAliasVariants_Cache = new Dictionary<TypePairKey, Type>(new TypePairKeyComparer());
        private static readonly Dictionary<Type, Func<PropertyValueEntry, IPropertyValueEntry>> GenericAlasVariants_EmittedCreator_Cache = new Dictionary<Type, Func<PropertyValueEntry, IPropertyValueEntry>>(FastTypeComparer.Instance);

        private static readonly Type[] TypeArrayWithOneElement_Cached = new Type[1];

        /// <summary>
        /// Delegate type used for the events <see cref="OnValueChanged"/> and <see cref="OnChildValueChanged"/>.
        /// </summary>
        public delegate void ValueChangedDelegate(int targetIndex);

        private InspectorProperty parentValueProperty;
        private InspectorProperty property;
        private bool isBaseEditable;
        private Type actualTypeOfValue;
        private bool baseValueIsValueType;

        /// <summary>
        /// <para>The nearest parent property that has a value.
        /// That is, the property from which this value
        /// entry will fetch its parentvalues from in order
        /// to extract its own values.</para>
        ///
        /// <para>If <see cref="ParentValueProperty"/> is null, this is a root property.</para>
        /// </summary>
        protected InspectorProperty ParentValueProperty { get { return this.parentValueProperty; } }

        /// <summary>
        /// Whether this value entry represents a boxed value type.
        /// </summary>
        protected bool IsBoxedValueType { get; private set; }

        /// <summary>
        /// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="PropertyTree.WeakTargets" />.
        /// </summary>
        public int ValueCount { get; private set; }

        /// <summary>
        /// Whether this value entry is editable or not.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                if (this.isBaseEditable)
                {
                    if (this.parentValueProperty != null)
                    {
                        var parentValueEntry = this.parentValueProperty.ValueEntry;

                        if (!parentValueEntry.IsEditable)
                        {
                            return false;
                        }

                        ICollectionResolver parentResolver = parentValueProperty.ChildResolver as ICollectionResolver;

                        if (parentResolver != null)
                        {
                            bool parentCollectionIsReadOnly = parentResolver.IsReadOnly;

                            if (parentCollectionIsReadOnly)
                            {
                                return false;
                            }

                            return true;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// If this value entry has the override type <see cref="PropertyValueState.Reference" />, this is the path of the property it references.
        /// </summary>
        public string TargetReferencePath { get; private set; }

        /// <summary>
        /// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
        /// <para>Note that this is *not* always equal to <see cref="InspectorPropertyInfo.SerializationBackend" />.</para>
        /// </summary>
        public SerializationBackend SerializationBackend { get; private set; }

        /// <summary>
        /// The property whose values this value entry represents.
        /// </summary>
        public InspectorProperty Property { get { return this.property; } }

        /// <summary>
        /// Provides access to the weakly typed values of this value entry.
        /// </summary>
        public abstract IPropertyValueCollection WeakValues { get; }

        /// <summary>
        /// Whether this value entry has been changed from its prefab counterpart.
        /// </summary>
        public bool ValueChangedFromPrefab { get; internal set; }

        /// <summary>
        /// Whether this value entry has had its list length changed from its prefab counterpart.
        /// </summary>
        public bool ListLengthChangedFromPrefab { get; internal set; }

        /// <summary>
        /// Whether this value entry has had its dictionary values changes from its prefab counterpart.
        /// </summary>
        public bool DictionaryChangedFromPrefab { get; internal set; }

        /// <summary>
        /// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public abstract object WeakSmartValue { get; set; }

        /// <summary>
        /// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
        /// </summary>
        public abstract Type ParentType { get; }

        /// <summary>
        /// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="BaseValueType" />.
        /// </summary>
        public Type TypeOfValue
        {
            get
            {
                if (this.actualTypeOfValue == null)
                {
                    this.actualTypeOfValue = this.BaseValueType;
                }

                return this.actualTypeOfValue;
            }
        }

        /// <summary>
        /// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
        /// </summary>
        public Type BaseValueType { get; private set; }

        /// <summary>
        /// The special state of the value entry.
        /// </summary>
        public PropertyValueState ValueState { get; private set; }

        /// <summary>
        /// Whether this value entry is an alias, or not. Value entry aliases are used to provide strongly typed value entries in the case of polymorphism.
        /// </summary>
        public bool IsAlias { get { return false; } }

        /// <summary>
        /// The context container of this property.
        /// </summary>
        public PropertyContextContainer Context { get { return this.Property.Context; } }

        /// <summary>
        /// Whether this type is marked as an atomic type using a <see cref="IAtomHandler"/>.
        /// </summary>
        public abstract bool IsMarkedAtomic { get; }

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges" />, when any values have changed.
        /// </summary>
        public event Action<int> OnValueChanged;

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges" />, when any child values have changed.
        /// </summary>
        public event Action<int> OnChildValueChanged;

        /// <summary>
        /// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
        /// </summary>
        public void Update()
        {
            this.UpdateValues();

            if (!this.baseValueIsValueType && (this.SerializationBackend.SupportsPolymorphism || typeof(UnityEngine.Object).IsAssignableFrom(this.BaseValueType)))
            {
                var type = this.GetMostPreciseContainedType();

                if (this.actualTypeOfValue != type)
                {
                    this.actualTypeOfValue = type;

                    this.IsBoxedValueType = this.BaseValueType == typeof(object) && type.IsValueType;
                }
            }

            this.ValueState = this.GetValueState();

            if (this.ValueState == PropertyValueState.Reference)
            {
                string targetReferencePath;
                this.property.Tree.ObjectIsReferenced(this.WeakValues[0], out targetReferencePath);
                this.TargetReferencePath = targetReferencePath;
            }
            else
            {
                this.TargetReferencePath = null;
            }
        }

        /// <summary>
        /// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
        /// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool ValueTypeValuesAreEqual(IPropertyValueEntry other);

        /// <summary>
        /// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
        /// </summary>
        /// <returns>
        /// True if any changes were made, otherwise, false.
        /// </returns>
        public abstract bool ApplyChanges();

        /// <summary>
        /// Determines the value state of this value entry.
        /// </summary>
        protected abstract PropertyValueState GetValueState();

        /// <summary>
        /// Determines what the most precise contained type is on this value entry.
        /// </summary>
        protected abstract Type GetMostPreciseContainedType();

        /// <summary>
        /// Updates all values in this value entry from the target tree values.
        /// </summary>
        protected abstract void UpdateValues();

        /// <summary>
        /// Initializes this value entry.
        /// </summary>
        protected abstract void Initialize();

        internal void TriggerOnValueChanged(int index)
        {
            Action action = () =>
            {
                if (this.OnValueChanged != null)
                {
                    try
                    {
                        this.OnValueChanged(index);
                    }
                    catch (ExitGUIException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsExitGUIException())
                        {
                            throw ex.AsExitGUIException();
                        }

                        Debug.LogException(ex);
                    }
                }

                this.Property.Tree.InvokeOnPropertyValueChanged(this.Property, index);
            };

            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                action();
            }
            else
            {
                this.Property.Tree.DelayActionUntilRepaint(action);
            }

            if (this.ParentValueProperty != null)
            {
                this.ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
            }
        }

        internal void TriggerOnChildValueChanged(int index)
        {
            this.Property.Tree.DelayActionUntilRepaint(() =>
            {
                if (this.OnChildValueChanged != null)
                {
                    try
                    {
                        this.OnChildValueChanged(index);
                    }
                    catch (ExitGUIException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsExitGUIException())
                        {
                            throw ex.AsExitGUIException();
                        }

                        Debug.LogException(ex);
                    }
                }
            });

            if (this.ParentValueProperty != null)
            {
                this.ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
            }
        }

        /// <summary>
        /// Creates an alias value entry of a given type, for a given value entry. This is used to implement polymorphism in Odin.
        /// </summary>
        public static IPropertyValueEntry CreateAlias(PropertyValueEntry entry, Type valueType)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            Type aliasEntryType;

            var typePairKey = default(TypePairKey);
            typePairKey.ParentType = entry.BaseValueType;
            typePairKey.ValueType = valueType;
            
            if (!GenericAliasVariants_Cache.TryGetValue(typePairKey, out aliasEntryType))
            {
                aliasEntryType = typeof(PropertyValueEntryAlias<,>).MakeGenericType(entry.BaseValueType, valueType);
                GenericAliasVariants_Cache.Add(typePairKey, aliasEntryType);
            }

            Func<PropertyValueEntry, IPropertyValueEntry> creator;

            if (!GenericAlasVariants_EmittedCreator_Cache.TryGetValue(aliasEntryType, out creator))
            {
                TypeArrayWithOneElement_Cached[0] = typeof(PropertyValueEntry);

                var method = new DynamicMethod("AliasCreator_" + Guid.NewGuid().ToString(), typeof(IPropertyValueEntry), TypeArrayWithOneElement_Cached);
                var il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Newobj, aliasEntryType.GetConstructor(TypeArrayWithOneElement_Cached));
                il.Emit(OpCodes.Ret);

                creator = (Func<PropertyValueEntry, IPropertyValueEntry>)method.CreateDelegate(typeof(Func<PropertyValueEntry, IPropertyValueEntry>));
                GenericAlasVariants_EmittedCreator_Cache.Add(aliasEntryType, creator);
            }
            
            return creator(entry);
        }

        /// <summary>
        /// Creates a value entry for a given property, of a given value type. Note that the created value entry is returned un-updated, and needs to have <see cref="Update"/> called on it before it can be used.
        /// </summary>
        internal static PropertyValueEntry Create(InspectorProperty property, Type valueType, bool isSecretRoot)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (property.Info.PropertyType != PropertyType.Value)
            {
                throw new ArgumentException("Cannot create a " + typeof(PropertyValueEntry).Name + " for a property which is not a value property.");
            }

            Type parentType;
            InspectorProperty parentValueProperty = property.ParentValueProperty;

            // We are the secret root property, and our parent values are selection indices
            if (isSecretRoot)
            {
                parentType = typeof(int);
            }
            // We have a parent value property
            else if (parentValueProperty != null)
            {
                parentType = parentValueProperty.ValueEntry.TypeOfValue;
            }
            // We are a root property, and our parent values are the tree targets
            else
            {
                parentType = property.Tree.TargetType;
            }

            Type genericVariantType;
            TypePairKey variantKey = default(TypePairKey);

            variantKey.ParentType = parentType;
            variantKey.ValueType = valueType;

            if (!GenericValueEntryVariants_Cache.TryGetValue(variantKey, out genericVariantType))
            {
                genericVariantType = typeof(PropertyValueEntry<,>).MakeGenericType(parentType, valueType);
                GenericValueEntryVariants_Cache.Add(variantKey, genericVariantType);
            }

            Func<PropertyValueEntry> creator;
            
            if (!GenericValueEntryVariants_EmittedCreator_Cache.TryGetValue(genericVariantType, out creator))
            {
                var builder = new DynamicMethod("PropertyValueEntry_InstanceCreator_" + Guid.NewGuid(), typeof(PropertyValueEntry), Type.EmptyTypes);
                var il = builder.GetILGenerator();

                il.Emit(OpCodes.Newobj, genericVariantType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);

                creator = (Func<PropertyValueEntry>)builder.CreateDelegate(typeof(Func<PropertyValueEntry>));
                GenericValueEntryVariants_EmittedCreator_Cache.Add(genericVariantType, creator);
            }

            PropertyValueEntry result = creator();

            result.BaseValueType = valueType;
            result.property = property;
            result.ValueCount = property.Tree.WeakTargets.Count;
            result.parentValueProperty = parentValueProperty;
            result.baseValueIsValueType = valueType.IsValueType;

            result.IsBoxedValueType = result.BaseValueType == typeof(object) && result.TypeOfValue.IsValueType;

            if (parentValueProperty != null)
            {
                result.SerializationBackend = property.Info.SerializationBackend;
                result.isBaseEditable = parentValueProperty.BaseValueEntry.isBaseEditable && property.Info.IsEditable;
            }
            else
            {
                result.SerializationBackend = property.Info.SerializationBackend;
                result.isBaseEditable = property.Info.IsEditable;
            }

            result.Initialize();

            return result;
        }

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="Utilities.TypeExtensions.GetEqualityComparerDelegate{T}" /> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        public abstract bool ValueIsPrefabDifferent(object value, int index);

        public void Dispose()
        {
            this.OnValueChanged = null;
            this.OnChildValueChanged = null;
        }
    }

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
    public abstract class PropertyValueEntry<TValue> : PropertyValueEntry, IPropertyValueEntry<TValue>, IValueEntryActualValueSetter<TValue>
    {
        /// <summary>
        /// An equality comparer for comparing values of type <see cref="TValue"/>. This is gotten using <see cref="TypeExtensions.GetEqualityComparerDelegate{T}"/>.
        /// </summary>
        public static readonly Func<TValue, TValue, bool> EqualityComparer = TypeExtensions.GetEqualityComparerDelegate<TValue>();

        /// <summary>
        /// Whether <see cref="TValue"/>.is a primitive type; that is, the type is primitive, a string, or an enum.
        /// </summary>
        protected static readonly bool ValueIsPrimitive = typeof(TValue).IsPrimitive || typeof(TValue) == typeof(string) || typeof(TValue).IsEnum;

        /// <summary>
        /// Whether <see cref="TValue"/> is a value type.
        /// </summary>
        protected static readonly bool ValueIsValueType = typeof(TValue).IsValueType;

        /// <summary>
        /// Whether <see cref="PropertyValueEntry.TypeOfValue"/> is derived from <see cref="UnityEngine.Object"/>.
        /// </summary>
        protected bool ValueIsUnityObject { get { return typeof(UnityEngine.Object).IsAssignableFrom(this.TypeOfValue); } }

        /// <summary>
        /// Whether the type of the value is marked atomic.
        /// </summary>
        protected static readonly bool ValueIsMarkedAtomic = typeof(TValue).IsMarkedAtomic();

        /// <summary>
        /// If the type of the value is marked atomic, this an instance of an atom handler for the value type.
        /// </summary>
        protected static readonly IAtomHandler<TValue> AtomHandler = ValueIsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null;

        private PropertyValueCollection<TValue> values;
        private bool isWaitingForDelayedValueSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueEntry{TValue}"/> class.
        /// </summary>
        protected PropertyValueEntry()
        {
        }

        /// <summary>
        /// Provides access to the weakly typed values of this value entry.
        /// </summary>
        public sealed override IPropertyValueCollection WeakValues { get { return this.values; } }

        /// <summary>
        /// Provides access to the strongly typed values of this value entry.
        /// </summary>
        public IPropertyValueCollection<TValue> Values { get { return this.values; } }

        /// <summary>
        /// Whether this type is marked as an atomic type using a <see cref="IAtomHandler"/>.
        /// </summary>
        public override bool IsMarkedAtomic { get { return ValueIsMarkedAtomic; } }

        /// <summary>
        /// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public override object WeakSmartValue
        {
            get { return this.SmartValue; }
            set
            {
                try
                {
                    this.SmartValue = (TValue)value;
                }
                catch (InvalidCastException)
                {
                    if (object.ReferenceEquals(value, null))
                    {
                        Debug.LogError("Invalid cast on set weak value! Could not cast value 'null' to the type '" + typeof(TValue).GetNiceName() + "' on property " + this.Property.Path + ".");
                    }
                    else
                    {
                        Debug.LogError("Invalid cast on set weak value! Could not cast value of type '" + value.GetType().GetNiceName() + "' to '" + typeof(TValue).GetNiceName() + "' on property " + this.Property.Path + ".");
                    }
                }
            }
        }

        /// <summary>
        /// <para>A strongly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public TValue SmartValue
        {
            get
            {
                return this.values[0];
            }

            set
            {
                // The value has already been set, and we are waiting for it to be applied
                //  properly at a safe time, since the value was a change of type and the
                //  property tree will change when it is applied.

                if (this.isWaitingForDelayedValueSet) return;

                if (ValueIsMarkedAtomic)
                {
                    if (!AtomHandler.Compare(value, this.AtomValuesArray[0]))
                    {
                        if (this.IsEditable == false)
                        {
                            Debug.LogWarning("Tried to change value of non-editable property '" + this.Property.NiceName + "' of type '" + this.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");

                            // Reset value, as this is illegal
                            if (!ValueIsValueType)
                            {
                                AtomHandler.Copy(ref this.AtomValuesArray[0], ref value);
                            }
                            return;
                        }

                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            this.values[i] = value;
                        }
                    }
                }
                else if (ValueIsPrimitive || ValueIsValueType)
                {
                    // Determine if the value has changed

                    if (!EqualityComparer(value, this.values[0]))
                    {
                        if (this.IsEditable == false)
                        {
                            Debug.LogWarning("Tried to change value of non-editable property '" + this.Property.NiceName + "' of type '" + this.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");
                            return;
                        }

                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            this.values[i] = value;
                        }
                    }
                }
                else if (!object.ReferenceEquals(value, this.SmartValue))    // If the reference has not changed; there is no reason to run all this code
                {
                    if (this.IsEditable == false)
                    {
                        Debug.LogWarning("Tried to change value of non-editable property '" + this.Property.NiceName + "' of type '" + this.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");
                        return;
                    }

                    Type currentType;

                    if (!object.ReferenceEquals(this.SmartValue, null))
                    {
                        currentType = (this.SmartValue as object).GetType();
                    }
                    else
                    {
                        currentType = typeof(TValue);
                    }

                    if (!object.ReferenceEquals(value, null) && value.GetType() != currentType)
                    {
                        // The actual type of the value is changing, meaning we have to delay the actual value set until a safe
                        //  time. If we don't, we might start getting cast exceptions or layout exceptions since things are changing
                        //  while we are still underway with drawing.
                        this.DelayedSmartValueReferenceSet(value);
                    }
                    else
                    {
                        this.SmartValueReferenceSet(value);
                    }
                }
            }
        }

        private void DelayedSmartValueReferenceSet(TValue value)
        {
            this.isWaitingForDelayedValueSet = true;

            this.Property.Tree.DelayActionUntilRepaint(() =>
            {
                this.isWaitingForDelayedValueSet = false;
                this.SmartValueReferenceSet(value);
            });
        }

        private void SmartValueReferenceSet(TValue value)
        {
            if (this.ValueCount == 1 || object.ReferenceEquals(value, null))
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    this.values[i] = value;
                }
            }
            else
            {
                // We are dealing with multiple references, meaning we have multiple *parallel trees* of references.
                // Now we need to haxx this to make it all work in a somewhat sensible way.
                //
                // In short, the idea of the code below is to determine whether we can mirror the reference
                // assignment "horizontally" through the tree on all targets by matching reference paths.
                //
                // If that isn't possible because the assigned reference has not been seen before, then we
                // simply assign the same reference to all targets. It will become "split" between them
                // when they are individually serialized and deserialized later in the editor.

                bool valueIsSet = false;
                string seenBeforePath;

                if (this.Property.Tree.ObjectIsReferenced(value, out seenBeforePath))
                {
                    InspectorProperty referencedProperty = this.Property.Tree.GetPropertyAtPath(seenBeforePath);

                    if (referencedProperty != null && referencedProperty.Info.PropertyType == PropertyType.Value && !referencedProperty.Info.TypeOfValue.IsValueType)
                    {
                        // We can mirror the values directly
                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            TValue mirroredValue = (TValue)referencedProperty.ValueEntry.WeakValues[i];
                            this.values[i] = mirroredValue;
                        }

                        valueIsSet = true;
                    }
                }

                if (!valueIsSet)
                {
                    for (int i = 0; i < this.ValueCount; i++)
                    {
                        this.values[i] = value;
                    }
                }
            }
        }

        /// <summary>
        /// An array containing the original values as they were at the beginning of frame.
        /// </summary>
        protected TValue[] OriginalValuesArray { get; private set; }

        /// <summary>
        /// An array containing the current modified set of values.
        /// </summary>
        protected TValue[] InternalValuesArray { get; private set; }

        /// <summary>
        /// An array containing the current modified set of atomic values.
        /// </summary>
        protected TValue[] AtomValuesArray { get; private set; }

        /// <summary>
        /// An array containing the original set of atomic values.
        /// </summary>
        protected TValue[] OriginalAtomValuesArray { get; private set; }

        /// <summary>
        /// Initializes this value entry.
        /// </summary>
        protected override void Initialize()
        {
            this.OriginalValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];
            this.InternalValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];

            if (this.IsMarkedAtomic)
            {
                this.AtomValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];
                this.OriginalAtomValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];
            }

            this.values = new PropertyValueCollection<TValue>(this.Property, this.InternalValuesArray, this.OriginalValuesArray, this.AtomValuesArray, this.OriginalAtomValuesArray);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected abstract void SetActualValueImplementation(int index, TValue value);

        /// <summary>
        /// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
        /// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
        /// </summary>
        public override bool ValueTypeValuesAreEqual(IPropertyValueEntry other)
        {
            if (!ValueIsValueType || !other.TypeOfValue.IsValueType || other.TypeOfValue != this.TypeOfValue)
            {
                return false;
            }

            IPropertyValueEntry<TValue> castOther = (IPropertyValueEntry<TValue>)other;

            if (other.ValueCount == 1 || other.ValueState == PropertyValueState.None)
            {
                TValue otherValue = castOther.Values[0];

                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (!EqualityComparer(this.Values[i], otherValue))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (this.ValueCount == 1 || this.ValueState == PropertyValueState.None)
            {
                TValue thisValue = this.Values[0];

                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (!EqualityComparer(thisValue, castOther.Values[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (this.ValueCount == other.ValueCount)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (!EqualityComparer(this.Values[i], castOther.Values[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        void IValueEntryActualValueSetter<TValue>.SetActualValue(int index, TValue value)
        {
            this.InternalValuesArray[index] = value;
            this.SetActualValueImplementation(index, value);
        }

        void IValueEntryActualValueSetter.SetActualValue(int index, object value)
        {
            this.InternalValuesArray[index] = (TValue)value;
            this.SetActualValueImplementation(index, (TValue)value);
        }

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="Utilities.TypeExtensions.GetEqualityComparerDelegate{T}" /> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        public override bool ValueIsPrefabDifferent(object value, int index)
        {
            if (object.ReferenceEquals(value, null))
            {
                if (ValueIsValueType)
                {
                    return true;
                }
            }
            else if (ValueIsValueType)
            {
                if (typeof(TValue) != value.GetType())
                {
                    return true;
                }
            }
            else if (!typeof(TValue).IsAssignableFrom(value.GetType()))
            {
                return true;
            }

            return this.ValueIsPrefabDifferent((TValue)value, index);
        }

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        public bool ValueIsPrefabDifferent(TValue value, int index)
        {
            TValue thisValue = this.Values[index];

            if (IsMarkedAtomic)
            {
                return !AtomHandler.Compare(value, thisValue);
            }

            if (ValueIsValueType)
            {
                if (ValueIsPrimitive)
                {
                    return !EqualityComparer(value, thisValue);
                }
                else
                {
                    return false;
                }
            }

            if (typeof(TValue) == typeof(string))
            {
                return !EqualityComparer(value, thisValue);
            }

            if (this.ValueIsUnityObject)
            {
                return !object.ReferenceEquals(thisValue, value);
            }

            Type a = null;
            Type b = null;

            if (!object.ReferenceEquals(value, null))
            {
                a = (value as object).GetType();
            }

            if (!object.ReferenceEquals(thisValue, null))
            {
                b = (thisValue as object).GetType();
            }

            return a != b;
        }
    }

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
    public sealed class PropertyValueEntry<TParent, TValue> : PropertyValueEntry<TValue>
    {
        private IValueGetterSetter<TParent, TValue> getterSetter;

        private static readonly bool ParentIsValueType = typeof(TParent).IsValueType;

        /// <summary>
        /// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
        /// </summary>
        public sealed override Type ParentType { get { return typeof(TParent); } }

        /// <summary>
        /// Determines what the most precise contained type is on this value entry.
        /// </summary>
        protected sealed override Type GetMostPreciseContainedType()
        {
            if (ValueIsValueType)
            {
                return typeof(TValue);
            }

            var values = this.InternalValuesArray;
            Type type = null;

            for (int i = 0; i < values.Length; i++)
            {
                // Don't have it as a strongly typed TValue, since people can "override" (shadow)
                // GetType() on derived classes with the "new" operator. By referencing the type
                // as a System.Object, we ensure the correct GetType() method is always called.
                //
                // (Yes, this has actually happened, and this was done to fix it.)
                object value = values[i];

                if (object.ReferenceEquals(value, null))
                {
                    return this.Property.Info.TypeOfValue;
                }

                if (i == 0)
                {
                    type = value.GetType();
                }
                else if (type != value.GetType())
                {
                    return this.Property.Info.TypeOfValue;
                }
            }

            return type;
        }

        /// <summary>
        /// Initializes this value entry.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            if (this.Property.Info.IsUnityPropertyOnly)
            {
                this.getterSetter = new UnityPropertyGetterSetter<TParent, TValue>(this.Property);
            }
            else if (!this.Property.Info.TryGetStrongGetterSetter(out this.getterSetter))
            {
                this.Property.Info.TryGetStrongGetterSetter(out this.getterSetter);
                throw new InvalidOperationException("Could not get proper value getter setter for property '" + this.Property.Path + "'.");
            }
        }

        /// <summary>
        /// Updates all values in this value entry from the target tree values.
        /// </summary>
        protected sealed override void UpdateValues()
        {
            for (int i = 0; i < this.ValueCount; i++)
            {
                TParent parent = this.GetParent(i);

                if (object.ReferenceEquals(parent, null))
                {
                    parent = this.GetParent(i);
                }

                TValue value = this.getterSetter.GetValue(ref parent);

                if (ValueIsMarkedAtomic)
                {
                    AtomHandler.Copy(ref value, ref this.AtomValuesArray[i]);
                    AtomHandler.Copy(ref value, ref this.OriginalAtomValuesArray[i]);
                }

                this.OriginalValuesArray[i] = value;
                this.InternalValuesArray[i] = value;
            }

            this.Values.MarkClean();
        }

        /// <summary>
        /// Determines the value state of this value entry.
        /// </summary>
        protected sealed override PropertyValueState GetValueState()
        {
            TValue[] values = this.InternalValuesArray;

            if (!ValueIsValueType && !ValueIsPrimitive && !ValueIsMarkedAtomic)
            {
                TValue value = values[0];
                string referencePath;

                if (object.ReferenceEquals(value, null) || (this.ValueIsUnityObject && (((UnityEngine.Object)(object)value) == null /*|| ((UnityEngine.Object)(object)value).SafeIsUnityNull()*/)))
                {
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (this.ValueIsUnityObject)
                        {
                            if (((UnityEngine.Object)(object)values[i]) != null /*|| ((UnityEngine.Object)(object)values[i]).SafeIsUnityNull()*/)
                            {
                                return PropertyValueState.ReferenceValueConflict;
                            }
                        }
                        else if (!object.ReferenceEquals(values[i], null))
                        {
                            return PropertyValueState.ReferenceValueConflict;
                        }
                    }

                    return PropertyValueState.NullReference;
                }
                else if (!this.ValueIsUnityObject && this.Property.Tree.ObjectIsReferenced(value, out referencePath) && referencePath != this.Property.Path)
                {
                    // Same property may be updated multiple times in same frame, sometimes - do not allow a property to be a reference to itself; that way lies madness
                    // This is the reason for the path inequality check

                    bool valueWasNull = false;
                    string otherReferencePath;

                    for (int i = 1; i < values.Length; i++)
                    {
                        TValue v = values[i];

                        if (object.ReferenceEquals(v, null))
                        {
                            valueWasNull = true;
                        }
                        else if (!this.Property.Tree.ObjectIsReferenced(v, out otherReferencePath) || otherReferencePath != referencePath)
                        {
                            return PropertyValueState.ReferencePathConflict;
                        }
                    }

                    if (valueWasNull)
                    {
                        return PropertyValueState.ReferenceValueConflict;
                    }
                    else
                    {
                        return PropertyValueState.Reference;
                    }
                }
                else
                {
                    var prop = this.Property;
                    var tree = prop.Tree;

                    Type type = (value as object).GetType();

                    bool isReferenceValueConflict = false;

                    tree.ForceRegisterObjectReference(value, prop);

                    for (int i = 1; i < values.Length; i++)
                    {
                        TValue v = values[i];

                        bool isNull = object.ReferenceEquals(null, v);

                        if (!isNull)
                        {
                            tree.ForceRegisterObjectReference(v, prop);
                        }

                        if (isNull || (v as object).GetType() != type)
                        {
                            // Continue looping; we want all value references force registered
                            isReferenceValueConflict = true;
                        }

                        if (this.ValueIsUnityObject && !object.ReferenceEquals(value, v))
                        {
                            isReferenceValueConflict = true;
                        }
                    }

                    if (isReferenceValueConflict)
                    {
                        return PropertyValueState.ReferenceValueConflict;
                    }
                }

                ICollectionResolver collectionResolver = this.Property.ChildResolver as ICollectionResolver;

                if (collectionResolver != null && collectionResolver.CheckHasLengthConflict())
                {
                    return PropertyValueState.CollectionLengthConflict;
                }

                return PropertyValueState.None;
            }
            else if (ValueIsMarkedAtomic)
            {
                TValue value = values[0];

                if (!ValueIsValueType && object.ReferenceEquals(value, null))
                {
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (!object.ReferenceEquals(values[i], null))
                        {
                            return PropertyValueState.ReferenceValueConflict;
                        }
                    }

                    return PropertyValueState.NullReference;
                }
                else
                {
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (!AtomHandler.Compare(value, values[i]))
                        {
                            return PropertyValueState.PrimitiveValueConflict;
                        }
                    }
                }

                return PropertyValueState.None;
            }
            else if (ValueIsPrimitive || ValueIsValueType)
            {
                TValue value = values[0];

                for (int i = 1; i < values.Length; i++)
                {
                    if (!EqualityComparer(value, values[i]))
                    {
                        return PropertyValueState.PrimitiveValueConflict;
                    }
                }

                return PropertyValueState.None;
            }
            else
            {
                // Value is a non-primitive value type
                return PropertyValueState.None;
            }
        }

        /// <summary>
        /// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
        /// </summary>
        /// <returns>
        /// True if any changes were made, otherwise, false.
        /// </returns>
        public sealed override bool ApplyChanges()
        {
            bool changed = false;
            var tree = this.Property.Tree;

            if (this.Values.AreDirty)
            {
                this.Property.RecordForUndo();
                changed = true;

                for (int i = 0; i < this.ValueCount; i++)
                {
                    //                                                     In this one case, the parent value is *supposed* to be null
                    if (object.ReferenceEquals(this.GetParent(i), null) && !(this.Property.Tree.IsStatic && (this.Property.ParentValueProperty == null || this.Property.ParentValueProperty.IsTreeRoot)))
                    {
                        Debug.LogError("Parent is null!");
                        continue;
                    }

                    var value = this.InternalValuesArray[i];
                    this.SetActualValueImplementation(i, value);
                }

                this.Values.MarkClean();

                for (int i = 0; i < this.ValueCount; i++)
                {
                    this.TriggerOnValueChanged(i);
                }

                // Force an update of our own property before we do stuff that might depend on properties that are children of it, such as registering prefab modifications
                this.Property.Update(true);

                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (this.SerializationBackend == SerializationBackend.Odin && tree.PrefabModificationHandler.HasPrefabs && tree.PrefabModificationHandler.TargetPrefabs[i] != null)
                    {
                        tree.PrefabModificationHandler.RegisterPrefabValueModification(this.Property, i);
                    }
                }
            }
            
            return changed;
        }

        /// <summary>
        /// Gets the parent value at the given index.
        /// </summary>
        private TParent GetParent(int index)
        {
            if (this.Property == this.Property.Tree.RootProperty)
            {
                return (TParent)(object)index;
            }
            else if (this.ParentValueProperty != null)
            {
                IPropertyValueEntry<TParent> parentValueEntry = (IPropertyValueEntry<TParent>)this.ParentValueProperty.ValueEntry;
                return parentValueEntry.Values[index];
            }
            else
            {
                return (TParent)this.Property.Tree.WeakTargets[index];
            }
        }

        protected override void SetActualValueImplementation(int index, TValue value)
        {
            TParent parent = this.GetParent(index);

            if (ParentIsValueType)
            {
                this.getterSetter.SetValue(ref parent, value);

                if (this.ParentValueProperty != null)
                {
                    ((IValueEntryActualValueSetter<TParent>)this.ParentValueProperty.ValueEntry).SetActualValue(index, parent);
                }
            }
            else
            {
                this.getterSetter.SetValue(ref parent, value);
            }
        }
    }
}
#endif