#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HashSetResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class HashSetResolver<TCollection, TElement> : BaseCollectionResolver<TCollection>
        where TCollection : HashSet<TElement>
    {
        private Dictionary<TCollection, List<TElement>> elementsArrays = new Dictionary<TCollection, List<TElement>>();
        private int lastUpdateId = -1;
        private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

        private HashSet<TCollection> seenHashset = new HashSet<TCollection>();
        private List<TCollection> toRemoveList = new List<TCollection>();

        public override Type ElementType { get { return typeof(TElement); } }

        public override int ChildNameToIndex(string name)
        {
            return CollectionResolverUtilities.DefaultChildNameToIndex(name);
        }

        public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
        {
            return false;
        }

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
                    getterSetter: new GetterSetter<TCollection, TElement>(
                        getter: (ref TCollection collection) => this.GetElement(collection, childIndex),
                        setter: (ref TCollection collection, TElement element) => this.SetElement(collection, element, childIndex)),
                    attributes: this.Property.Attributes.Where(attr => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true)).AppendWith(new DelayedAttribute()).AppendWith(new SuppressInvalidAttributeErrorAttribute()).ToArray());

                this.childInfos[childIndex] = result;
            }

            return result;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        private TElement GetElement(TCollection collection, int index)
        {
            this.EnsureUpdated();

            List<TElement> elements;

            if (this.elementsArrays.TryGetValue(collection, out elements))
            {
                return elements[index];
            }

            return default(TElement);
        }

        private void SetElement(TCollection collection, TElement element, int index)
        {
            var count = collection.Count;

            List<TElement> elements;

            if (!this.elementsArrays.TryGetValue(collection, out elements))
            {
                return;
            }

            // Refuse to set a value that already exists, since this is just the same as deleting a value, and is confusing
            if (elements.Contains(element)) return; 

            elements[index] = element;

            collection.Clear();
            collection.AddRange(elements);
            this.EnsureUpdated(true);
        }

        private void EnsureUpdated(bool force = false)
        {
            var treeId = this.Property.Tree.UpdateID;

            if (!force && this.lastUpdateId == treeId)
            {
                return;
            }

            this.seenHashset.Clear();
            this.toRemoveList.Clear();

            this.lastUpdateId = treeId;
            var count = this.ValueEntry.ValueCount;

            for (int i = 0; i < count; i++)
            {
                var collection = this.ValueEntry.Values[i];

                if (object.ReferenceEquals(collection, null)) continue;

                this.seenHashset.Add(collection);

                List<TElement> elements;

                if (!this.elementsArrays.TryGetValue(collection, out elements))
                {
                    elements = new List<TElement>(collection.Count);
                    this.elementsArrays[collection] = elements;
                }

                elements.Clear();
                elements.AddRange(collection);
                var comparer = DictionaryKeyUtility.KeyComparer<TElement>.Default;
                elements.Sort((IComparer<TElement>)comparer);
            }

            foreach (var col in this.elementsArrays.Keys)
            {
                if (!this.seenHashset.Contains(col)) this.toRemoveList.Add(col);
            }

            for (int i = 0; i < this.toRemoveList.Count; i++)
            {
                this.elementsArrays.Remove(this.toRemoveList[i]);
            }
        }

        protected override void Add(TCollection collection, object value)
        {
            collection.Add((TElement)value);
        }

        protected override void Clear(TCollection collection)
        {
            collection.Clear();
        }

        protected override bool CollectionIsReadOnly(TCollection collection)
        {
            return false;
        }

        protected override int GetChildCount(TCollection value)
        {
            return value.Count;
        }

        protected override void Remove(TCollection collection, object value)
        {
            collection.Remove((TElement)value);
        }
    }
}
#endif