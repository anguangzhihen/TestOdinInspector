#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BaseOrderedCollectionResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using UnityEngine.Assertions;

    public abstract class BaseOrderedCollectionResolver<TCollection> : BaseCollectionResolver<TCollection>, IOrderedCollectionResolver
    {
        public void QueueInsertAt(int index, object[] values)
        {
            Assert.IsNotNull(values);
            Assert.AreEqual(values.Length, this.Property.Tree.WeakTargets.Count);

            for (int i = 0; i < values.Length; i++)
            {
                this.QueueInsertAt(index, values[i], i);
            }
        }

        public void QueueInsertAt(int index, object value, int selectionIndex)
        {
            this.EnqueueChange(() => this.InsertAt((TCollection)this.Property.BaseValueEntry.WeakValues[selectionIndex], index, value), new CollectionChangeInfo() { ChangeType = CollectionChangeType.Insert, Value = value, Index = index, SelectionIndex = selectionIndex });
        }

        public void QueueRemoveAt(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            var count = this.Property.Tree.WeakTargets.Count;

            for (int i = 0; i < count; i++)
            {
                QueueRemoveAt(index, i);
            }
        }

        public void QueueRemoveAt(int index, int selectionIndex)
        {
            this.EnqueueChange(() => this.RemoveAt((TCollection)this.Property.BaseValueEntry.WeakValues[selectionIndex], index), new CollectionChangeInfo() { ChangeType = CollectionChangeType.RemoveIndex, Index = index, SelectionIndex = selectionIndex });
        }

        protected abstract void InsertAt(TCollection collection, int index, object value);

        protected abstract void RemoveAt(TCollection collection, int index);
    }
}
#endif