#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StrongListPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [ResolverPriority(-1)]
    public class StrongListPropertyResolver<TList, TElement> : BaseOrderedCollectionResolver<TList>, IMaySupportPrefabModifications
        where TList : IList<TElement>
    {
        private static bool IsArray = typeof(TList).IsArray;

        private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();
        private List<Attribute> childAttrs;

        public bool MaySupportPrefabModifications { get { return true; } }

        public override Type ElementType { get { return typeof(TElement); } }

        protected override void Initialize()
        {
            base.Initialize();

            var propAttrs = this.Property.Attributes;
            List<Attribute> attrs = new List<Attribute>(propAttrs.Count);

            for (int i = 0; i < propAttrs.Count; i++)
            {
                var attr = propAttrs[i];
                if (attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true)) continue;
                attrs.Add(attr);
            }

            this.childAttrs = attrs;
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
                    getterSetter: new GetterSetter<TList, TElement>(
                        getter: (ref TList list) => list[childIndex],
                        setter: (ref TList list, TElement element) => list[childIndex] = element),
                    attributes: this.childAttrs);

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
            if (IsArray)
            {
                TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithAddedElement((TElement[])(object)collection, (TElement)value);
                this.ReplaceArray(collection, newArray);
            }
            else
            {
                collection.Add((TElement)value);
            }
        }

        protected override void InsertAt(TList collection, int index, object value)
        {
            if (IsArray)
            {
                TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithInsertedElement((TElement[])(object)collection, index, (TElement)value);
                this.ReplaceArray(collection, newArray);
            }
            else
            {
                collection.Insert(index, (TElement)value);
            }
        }

        protected override void Remove(TList collection, object value)
        {
            if (IsArray)
            {
                int index = collection.IndexOf((TElement)value);

                if (index >= 0)
                {
                    TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, index);
                    this.ReplaceArray(collection, newArray);
                }
            }
            else
            {
                collection.Remove((TElement)value);
            }
        }

        protected override void RemoveAt(TList collection, int index)
        {
            if (IsArray)
            {
                TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, index);
                this.ReplaceArray(collection, newArray);
            }
            else
            {
                collection.RemoveAt(index);
            }
        }

        protected override void Clear(TList collection)
        {
            if (IsArray)
            {
                this.ReplaceArray(collection, (TList)(object)new TElement[0]);
            }
            else
            {
                collection.Clear();
            }
        }

        protected override bool CollectionIsReadOnly(TList collection)
        {
            // An array's strongly typed ICollection<T>.IsReadOnly FOR SOME FUCKING RETARDED REASON faultily returns true.
            // IsReadOnly is always supposed to be false on arrays (!!!!), so we enforce that manually BECAUSE IT MAKES SENSE,
            // AND YOU CAN FUCKING EDIT IT COME ON .NET.
            if (IsArray) return false;
            return collection.IsReadOnly;
        }

        private void ReplaceArray(TList oldArray, TList newArray)
        {
            if (!this.Property.ValueEntry.SerializationBackend.SupportsCyclicReferences)
            {
                for (int i = 0; i < this.ValueEntry.ValueCount; i++)
                {
                    if (object.ReferenceEquals(this.ValueEntry.Values[i], oldArray))
                    {
                        this.ValueEntry.Values[i] = newArray;
                        (this.ValueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
                    }
                }
            }
            else
            {
                foreach (var prop in this.Property.Tree.EnumerateTree(true))
                {
                    if (prop.Info.PropertyType == PropertyType.Value && !prop.Info.TypeOfValue.IsValueType)
                    {
                        var valueEntry = prop.ValueEntry;

                        for (int i = 0; i < valueEntry.ValueCount; i++)
                        {
                            object obj = valueEntry.WeakValues[i];

                            if (object.ReferenceEquals(oldArray, obj))
                            {
                                valueEntry.WeakValues[i] = newArray;
                                (valueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif