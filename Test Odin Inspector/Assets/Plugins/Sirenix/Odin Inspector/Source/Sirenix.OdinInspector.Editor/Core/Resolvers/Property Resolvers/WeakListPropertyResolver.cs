#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="WeakListPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    [ResolverPriority(-4.9)]
    public class WeakListPropertyResolver<TList> : BaseOrderedCollectionResolver<TList>, IMaySupportPrefabModifications
        where TList : IList
    {
        private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

        public bool MaySupportPrefabModifications { get { return true; } }

        public override Type ElementType { get { return typeof(object); } }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            if (childIndex < 0 || childIndex >= this.ChildCount)
            {
                throw new IndexOutOfRangeException();
            }

            InspectorPropertyInfo result;

            if (!this.childInfos.TryGetValue(childIndex, out result))
            {
                result = InspectorPropertyInfo.CreateValue(
                    name: CollectionResolverUtilities.DefaultIndexToChildName(childIndex),
                    order: childIndex,
                    serializationBackend: this.Property.BaseValueEntry.SerializationBackend,
                    getterSetter: new GetterSetter<TList, object>(
                        getter: (ref TList list) => list[childIndex],
                        setter: (ref TList list, object element) => list[childIndex] = element),
                attributes: this.Property.Attributes.Where(attr => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true)).ToArray());

                this.childInfos[childIndex] = result;
            }

            return result;
        }

        public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
        {
            return false;
        }

        public override int ChildNameToIndex(string name)
        {
            return CollectionResolverUtilities.DefaultChildNameToIndex(name);
        }

        protected override int GetChildCount(TList value)
        {
            return value.Count;
        }

        protected override void Add(TList collection, object value)
        {
            collection.Add(value);
        }

        protected override void InsertAt(TList collection, int index, object value)
        {
            collection.Insert(index, value);
        }

        protected override void Remove(TList collection, object value)
        {
            collection.Remove(value);
        }

        protected override void RemoveAt(TList collection, int index)
        {
            collection.RemoveAt(index);
        }

        protected override void Clear(TList collection)
        {
            collection.Clear();
        }

        protected override bool CollectionIsReadOnly(TList collection)
        {
            return collection.IsReadOnly;
        }
    }
}
#endif