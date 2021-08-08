#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyValueCollectionAlias.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Sirenix.Utilities;
    using UnityEngine.Assertions;

    /// <summary>
    /// <para>Represents an alias for a strongly typed collection of values for a <see cref="PropertyValueEntry{T}" /> - one value per selected inspector target.</para>
    /// <para>This class ensures that polymorphism works in the inspector, and can be strongly typed in applicable cases.</para>
    /// </summary>
    /// <typeparam name="TActualValue">The type of the aliased collection.</typeparam>
    /// <typeparam name="TValue">The polymorphic type of this collection, which is assignable to <see cref="TActualValue"/>.</typeparam>
    /// <seealso cref="Sirenix.OdinInspector.Editor.PropertyValueCollection" />
    /// <seealso cref="Sirenix.OdinInspector.Editor.IPropertyValueCollection{TValue}" />
    public sealed class PropertyValueCollectionAlias<TActualValue, TValue> : PropertyValueCollection, IPropertyValueCollection<TValue> where TValue : TActualValue
    {
        private static readonly bool IsMarkedAtomic = typeof(TValue).IsMarkedAtomic();
        private static readonly IAtomHandler<TValue> AtomHandler = IsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null;
        private static readonly bool IsValueType = typeof(TValue).IsValueType;

        private IPropertyValueCollection<TActualValue> aliased;
        private OriginalValuesAlias originalValuesAlias;
        private TValue[] atomicValues;
        private TValue[] originalAtomicValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueCollectionAlias{TActualValue, TValue}"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="aliasedCollection">The aliased collection.</param>
        /// <param name="atomArray">Not yet documented.</param>
        /// <param name="originalAtomArray">Not yet documented.</param>
        /// <exception cref="System.ArgumentException">aliasedCollection</exception>
        public PropertyValueCollectionAlias(InspectorProperty property, IPropertyValueCollection<TActualValue> aliasedCollection, TValue[] atomArray, TValue[] originalAtomArray)
            : base(property)
        {
            Assert.IsNotNull(aliasedCollection);

            if (IsMarkedAtomic)
            {
                Assert.IsNotNull(atomArray);
                Assert.IsNotNull(originalAtomArray);
            }

            this.aliased = aliasedCollection;
            this.originalValuesAlias = new OriginalValuesAlias(this.aliased.Original);
            this.atomicValues = atomArray;
            this.originalAtomicValues = originalAtomArray;
        }

        /// <summary>
        /// Whether the values have been changed since <see cref="MarkClean" /> was last called.
        /// </summary>
        public override bool AreDirty { get { return this.aliased.AreDirty; } }

        /// <summary>
        /// The number of values in the collection.
        /// </summary>
        public override int Count { get { return this.aliased.Count; } }

        /// <summary>
        /// Gets a value indicating whether this instance is synchronized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
        /// </value>
        protected override bool IsSynchronized { get { return ((ICollection)this.aliased).IsSynchronized; } }

        /// <summary>
        /// Gets the synchronization root object.
        /// </summary>
        /// <value>
        /// The synchronization root object.
        /// </value>
        protected override object SyncRoot { get { return ((ICollection)this.aliased).SyncRoot; } }

        int ICollection<TValue>.Count { get { return this.aliased.Count; } }

        bool ICollection<TValue>.IsReadOnly { get { return false; } }

        /// <summary>
        /// The original values of the (loosely typed) value collection, such as they were immediately after the last <see cref="PropertyValueEntry.Update" /> call.
        /// </summary>
        protected override IImmutableList WeakOriginal
        {
            get
            {
                return this.aliased.Original;
            }
        }

        /// <summary>
        /// The original values of the value collection, such as they were immediately after the last <see cref="PropertyValueEntry.Update" /> call.
        /// </summary>
        public IImmutableList<TValue> Original
        {
            get
            {
                return this.originalValuesAlias;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public new TValue this[int index]
        {
            get
            {
                try
                {
                    return (TValue)this.aliased[index];
                }
                catch
                {
                    // If this happens the property is probably will probably change drawers in the next frame and correct itself.
                    return default(TValue);
                }
            }
            set
            {
                if (IsMarkedAtomic)
                {
                    if (AtomHandler.Compare(value, this.atomicValues[index]) == false)
                    {
                        AtomHandler.Copy(ref value, ref this.atomicValues[index]);

                        if (IsValueType)
                        {
                            this.aliased.ForceSetValue(index, value);
                        }

                        this.aliased.ForceMarkDirty();
                    }
                }
                else
                {
                    this.aliased[index] = value;
                }
            }
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator GetEnumerator()
        {
            return this.aliased.GetEnumerator();
        }

        /// <summary>
        /// Marks the value collection as being clean again. This is typically called at the end of the current GUI frame, during <see cref="PropertyValueEntry.ApplyChanges" />.
        /// </summary>
        public override void MarkClean()
        {
            this.aliased.MarkClean();
        }

        /// <summary>
        /// Reverts the value collection to its origin values (found in <see cref="Original" />) from the last <see cref="PropertyValueEntry.Update" /> call, and marks the value collection as being clean again.
        /// </summary>
        public override void RevertUnappliedValues()
        {
            this.aliased.RevertUnappliedValues();

            if (IsMarkedAtomic)
            {
                for (int i = 0; i < this.aliased.Count; i++)
                {
                    var value = (TValue)this.aliased[i];

                    AtomHandler.Copy(ref this.originalAtomicValues[i], ref this.atomicValues[i]);
                    AtomHandler.Copy(ref this.originalAtomicValues[i], ref value);

                    if (IsValueType)
                    {
                        this.aliased.ForceSetValue(i, value);
                    }
                }

                this.aliased.MarkClean();
            }
        }

        /// <summary>
        /// Determines whether the collection contains the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        protected override bool Contains(object value)
        {
            return ((IList)this.aliased).Contains(value);
        }

        /// <summary>
        /// Gets the index of the given value, or -1 if the value was not found.
        /// </summary>
        /// <param name="value">The value to get the index of.</param>
        /// <returns>
        /// The index of the given value, or -1 if the value was not found.
        /// </returns>
        protected override int IndexOf(object value)
        {
            return ((IList)this.aliased).IndexOf(value);
        }

        /// <summary>
        /// Copies the collection to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="index">The index to copy from.</param>
        protected override void CopyTo(Array array, int index)
        {
            ((IList)this.aliased).CopyTo(array, index);
        }

        /// <summary>
        /// Gets the weakly typed value at the given index.
        /// </summary>
        /// <param name="index">The index of the value to get.</param>
        /// <returns>
        /// The weakly typed value at the given index
        /// </returns>
        protected override object GetWeakValue(int index)
        {
            return this.aliased[index];
        }

        /// <summary>
        /// Sets the weakly typed value at the given index.
        /// </summary>
        /// <param name="index">The index to set the value of.</param>
        /// <param name="value">The value to set.</param>
        protected override void SetWeakValue(int index, object value)
        {
            this.aliased[index] = (TActualValue)value;
        }

        int IList<TValue>.IndexOf(TValue item)
        {
            throw new NotImplementedException();
        }

        void IList<TValue>.Insert(int index, TValue item)
        {
            throw new NotImplementedException();
        }

        void IList<TValue>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection<TValue>.Add(TValue item)
        {
            throw new NotImplementedException();
        }

        void ICollection<TValue>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<TValue>.Contains(TValue item)
        {
            throw new NotImplementedException();
        }

        void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<TValue>.Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <para>Force sets the value, ignoring whether it is editable or not.</para>
        /// <para>Note that this will fail on list element value entries where <see cref="M:Sirenix.OdinInspector.Editor.IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
        /// </summary>
        /// <param name="index">The selection index of the value.</param>
        /// <param name="value">The value to be set.</param>
        public void ForceSetValue(int index, TValue value)
        {
            this.aliased.ForceSetValue(index, value);
        }

        /// <summary>
        /// <para>Force sets the value, ignoring whether it is editable or not.</para>
        /// <para>Note that this will fail on list element value entries where <see cref="IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
        /// </summary>
        /// <param name="index">The selection index of the value.</param>
        /// <param name="value">The value to be set.</param>
        public override void ForceSetValue(int index, object value)
        {
            this.aliased.ForceSetValue(index, (TActualValue)value);
        }

        /// <summary>
        /// Marks the value collection as being dirty, regardless of any value changes.
        /// </summary>
        public override void ForceMarkDirty()
        {
            this.aliased.ForceMarkDirty();
        }

        private class OriginalValuesAlias : IImmutableList<TValue>
        {
            private IImmutableList<TActualValue> aliased;

            public OriginalValuesAlias(IImmutableList<TActualValue> aliased)
            {
                this.aliased = aliased;
            }

            TValue IImmutableList<TValue>.this[int index]
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            object IList.this[int index]
            {
                get { return this.aliased[index]; }
                set
                {
                    throw new NotSupportedException();
                }
            }

            TValue IList<TValue>.this[int index]
            {
                get { return (TValue)this.aliased[index]; }
                set
                {
                    throw new NotSupportedException();
                }
            }

            bool IList.IsFixedSize { get { return this.aliased.IsFixedSize; } }

            bool IList.IsReadOnly { get { return (this.aliased as IList).IsReadOnly; } }

            bool ICollection<TValue>.IsReadOnly { get { return (this.aliased as IList).IsReadOnly; } }

            int ICollection.Count { get { return (this.aliased as IList).Count; } }

            int ICollection<TValue>.Count { get { return (this.aliased as IList).Count; } }

            bool ICollection.IsSynchronized { get { return this.aliased.IsSynchronized; } }

            object ICollection.SyncRoot { get { return this.aliased.SyncRoot; } }

            int IList.Add(object value)
            {
                return this.aliased.Add(value);
            }

            void ICollection<TValue>.Add(TValue item)
            {
                this.aliased.Add(item);
            }

            void IList.Clear()
            {
                (this.aliased as IList).Clear();
            }

            void ICollection<TValue>.Clear()
            {
                (this.aliased as IList).Clear();
            }

            bool IList.Contains(object value)
            {
                return (this.aliased as IList).Contains(value);
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return (this.aliased as ICollection<TValue>).Contains(item);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                this.aliased.CopyTo(array, index);
            }

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
            {
                this.aliased.CopyTo(array, arrayIndex);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return (this.aliased as IEnumerable<TValue>).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (this.aliased as IEnumerable).GetEnumerator();
            }

            int IList.IndexOf(object value)
            {
                return this.aliased.IndexOf(value);
            }

            int IList<TValue>.IndexOf(TValue item)
            {
                return this.aliased.IndexOf(item);
            }

            void IList.Insert(int index, object value)
            {
                this.aliased.Insert(index, value);
            }

            void IList<TValue>.Insert(int index, TValue item)
            {
                this.aliased.Insert(index, item);
            }

            void IList.Remove(object value)
            {
                this.aliased.Remove(value);
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                return this.aliased.Remove(item);
            }

            void IList.RemoveAt(int index)
            {
                (this.aliased as IList).RemoveAt(index);
            }

            void IList<TValue>.RemoveAt(int index)
            {
                (this.aliased as IList<TValue>).RemoveAt(index);
            }
        }
    }
}
#endif