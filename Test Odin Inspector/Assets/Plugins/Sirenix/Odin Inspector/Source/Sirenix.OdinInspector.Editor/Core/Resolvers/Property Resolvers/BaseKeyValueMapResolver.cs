#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BaseKeyValueMapResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEngine.Assertions;

    public abstract class BaseKeyValueMapResolver<TMap> : BaseCollectionResolver<TMap>, IKeyValueMapResolver
    {
        public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
        {
            return this.GetChildInfo(index) != info;
        }

        public void QueueRemoveKey(object[] keys)
        {
            Assert.IsNotNull(keys);
            Assert.AreEqual(keys.Length, this.Property.Tree.WeakTargets.Count);

            for (int i = 0; i < keys.Length; i++)
            {
                this.QueueRemoveKey(keys[i], i);
            }
        }

        public void QueueRemoveKey(object key, int selectionIndex)
        {
            this.EnqueueChange(() => this.RemoveKey((TMap)this.Property.BaseValueEntry.WeakValues[selectionIndex], key), new CollectionChangeInfo() { ChangeType = CollectionChangeType.RemoveKey, Key = key, SelectionIndex = selectionIndex });
        }

        public void QueueSet(object[] keys, object[] values)
        {
            Assert.IsNotNull(keys);
            Assert.IsNotNull(values);

            Assert.AreEqual(keys.Length, this.Property.Tree.WeakTargets.Count);
            Assert.AreEqual(values.Length, this.Property.Tree.WeakTargets.Count);

            for (int i = 0; i < values.Length; i++)
            {
                this.QueueSet(keys[i], values[i], i);
            }
        }

        public void QueueSet(object key, object value, int selectionIndex)
        {
            this.EnqueueChange(() => this.Set((TMap)this.Property.BaseValueEntry.WeakValues[selectionIndex], key, value), new CollectionChangeInfo() { ChangeType = CollectionChangeType.SetKey, Key = key, Value = value, SelectionIndex = selectionIndex });
        }

        protected abstract void Set(TMap map, object key, object value);

        protected abstract void RemoveKey(TMap map, object key);

        public abstract object GetKey(int selectionIndex, int childIndex);
    }
}
#endif