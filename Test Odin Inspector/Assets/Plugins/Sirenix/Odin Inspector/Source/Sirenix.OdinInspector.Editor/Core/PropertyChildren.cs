#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Represents the children of an <see cref="InspectorProperty"/>.
    /// </summary>
    public sealed class PropertyChildren : IEnumerable<InspectorProperty>
    {
        private Dictionary<int, InspectorPropertyInfo> infosByIndex = new Dictionary<int, InspectorPropertyInfo>();
        private Dictionary<int, InspectorProperty> childrenByIndex = new Dictionary<int, InspectorProperty>();
        private Dictionary<int, string> pathsByIndex = new Dictionary<int, string>();
        private bool allowChildren;

        private OdinPropertyResolver resolver;
        private IRefreshableResolver refreshableResolver;
        private IPathRedirector pathRedirector;
        private IHasSpecialPropertyPaths hasSpecialPropertyPaths;

        /// <summary>
        /// The <see cref="InspectorProperty"/> that this instance handles children for.
        /// </summary>
        private InspectorProperty property;

        /// <summary>
        /// Gets a child by index. This is an alias for <see cref="Get(int)" />.
        /// </summary>
        /// <param name="index">The index of the child to get.</param>
        /// <returns>The child at the given index.</returns>
        public InspectorProperty this[int index]
        {
            get { return this.Get(index); }
        }

        /// <summary>
        /// Gets a child by name. This is an alias for <see cref="Get(string)" />.
        /// </summary>
        /// <param name="name">The name of the child to get.</param>
        /// <returns>The child, if a child was found; otherwise, null.</returns>
        public InspectorProperty this[string name]
        {
            get { return this.Get(name); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChildren"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        /// <exception cref="System.ArgumentNullException">property is null</exception>
        internal PropertyChildren(InspectorProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            this.property = property;
            this.resolver = this.property.ChildResolver;
            this.refreshableResolver = this.resolver as IRefreshableResolver;
            this.pathRedirector = this.resolver as IPathRedirector;
            this.hasSpecialPropertyPaths = this.resolver as IHasSpecialPropertyPaths;
        }

        /// <summary>
        /// The number of children on the property.
        /// </summary>
        public int Count
        {
            get
            {
                return this.allowChildren ? this.property.ChildResolver.ChildCount : 0;
            }
        }

        internal void ClearAndDisposeChildren()
        {
            foreach (var child in this.childrenByIndex.Values)
            {
                try
                {
                    child.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            this.infosByIndex.Clear();
            this.childrenByIndex.Clear();
            this.pathsByIndex.Clear();

            this.property.Tree.ClearPathCaches();
        }

        /// <summary>
        /// Updates this instance of <see cref="PropertyChildren"/>.
        /// </summary>
        public void Update()
        {
            bool wasAllowingChildren = this.allowChildren;

            this.allowChildren = true;

            if (this.property != this.property.Tree.RootProperty &&
                this.property.ValueEntry != null &&
                (this.property.ValueEntry.ValueState == PropertyValueState.Reference
                || this.property.ValueEntry.ValueState == PropertyValueState.NullReference
                || this.property.ValueEntry.ValueState == PropertyValueState.ReferencePathConflict
                || this.property.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict))
            {
                this.allowChildren = false;

                if (wasAllowingChildren)
                {
                    this.ClearAndDisposeChildren();
                }
            }

            if (this.allowChildren)
            {
                this.property.ChildResolver.ForceUpdateChildCount();
            }
        }

        /// <summary>
        /// Gets a child by name.
        /// </summary>
        /// <param name="name">The name of the child to get.</param>
        /// <returns>The child, if a child was found; otherwise, null.</returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public InspectorProperty Get(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (!this.allowChildren || this.Count == 0) return null;

            int index = this.resolver.ChildNameToIndex(name);

            if (index >= 0 && index < this.Count)
            {
                return this.Get(index);
            }

            if (this.pathRedirector != null)
            {
                InspectorProperty result;

                if (this.pathRedirector.TryGetRedirectedProperty(name, out result))
                {
                    result.Update();
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a child by index.
        /// </summary>
        /// <param name="index">The index of the child to get.</param>
        /// <returns>
        /// The child at the given index.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">The given index was out of range.</exception>
        public InspectorProperty Get(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }

            InspectorProperty result;

            if (!this.childrenByIndex.TryGetValue(index, out result) || this.NeedsRefresh(index))
            {
                // A property already exists and must be refreshed, so it must be disposed immediately
                if (result != null)
                {
                    result.Dispose();
                    this.childrenByIndex.Remove(index);
                }

                // The order of operations here is very important. Calling result.Update() can cause all sorts of things to happen.
                // Including trying to get this very same child, resulting in an infinite loop because it hasn't
                // been set yet, so a new child will be created, ad infinitum.

                // Setting the child value, then updating, makes sure this sort of thing can never happen.

                result = InspectorProperty.Create(this.property.Tree, this.property, this.GetInfo(index), index, false);
                this.childrenByIndex[index] = result;
                this.property.Tree.NotifyPropertyCreated(result);
            }

            result.Update();
            return result;
        }

        /// <summary>
        /// Gets the path of the child at a given index.
        /// </summary>
        /// <param name="index">The index to get the path of.</param>
        /// <returns>The path of the child at the given index.</returns>
        /// <exception cref="System.IndexOutOfRangeException">The given index was out of range.</exception>
        public string GetPath(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }

            string result;

            if (!this.pathsByIndex.TryGetValue(index, out result) || this.NeedsRefresh(index))
            {
                if (this.hasSpecialPropertyPaths != null)
                {
                    result = this.hasSpecialPropertyPaths.GetSpecialChildPath(index);
                }
                else if (this.property.IsTreeRoot)
                {
                    result = this.GetInfo(index).PropertyName;
                }
                else
                {
                    result = this.property.Path + "." + this.GetInfo(index).PropertyName;
                }

                this.pathsByIndex[index] = result;
            }

            return result;
        }

        /// <summary>
        /// Returns an IEnumerable that recursively yields all children of the property, depth first.
        /// </summary>
        public IEnumerable<InspectorProperty> Recurse()
        {
            for (int i = 0; i < this.Count; i++)
            {
                var child = this[i];

                yield return child;

                foreach (var subChild in child.Children.Recurse())
                {
                    yield return subChild;
                }
            }
        }

        /// <summary>
        /// Gets the property's already created children. If the child count is less than or equal to 10000, children are returned in order. If the count is larger than 10000, they are returned in no particular order.
        /// </summary>
        internal ExistingChildEnumerator GetExistingChildren()
        {
            if (this.childrenByIndex == null || this.childrenByIndex.Count == 0)
            {
                return ExistingChildEnumerator.Empty;
            }

            if (this.Count <= 10000)
            {
                return new ExistingChildEnumerator(this);
            }
            else
            {
                return new ExistingChildEnumerator(this.childrenByIndex.GFIterator());
            }
            //var count = this.Count;

            //if (count <= 10000)
            //{
            //    for (int i = 0; i < count; i++)
            //    {
            //        InspectorProperty child;

            //        if (this.childrenByIndex.TryGetValue(i, out child))
            //        {
            //            yield return child;
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (var child in this.childrenByIndex.Values)
            //    {
            //        yield return child;
            //    }
            //}
        }

        internal struct ExistingChildEnumerator
        {
            private PropertyChildren children;
            private InspectorProperty current;
            private int index;
            private int count;
            private GarbageFreeIterators.DictionaryIterator<int, InspectorProperty> dictionaryIterator;
            private bool isEmpty;

            public static readonly ExistingChildEnumerator Empty;

            static ExistingChildEnumerator()
            {
                Empty = new ExistingChildEnumerator()
                {
                    isEmpty = true
                };
            }

            public ExistingChildEnumerator(PropertyChildren children)
            {
                this.children = children;
                this.current = null;
                this.index = -1;
                this.count = children.Count;
                this.dictionaryIterator = default(GarbageFreeIterators.DictionaryIterator<int, InspectorProperty>);
                this.isEmpty = false;
            }

            public ExistingChildEnumerator(GarbageFreeIterators.DictionaryIterator<int, InspectorProperty> dictionaryIterator) : this()
            {
                this.dictionaryIterator = dictionaryIterator;
            }

            public InspectorProperty Current
            {
                get
                {
                    if (this.isEmpty) return null;

                    return this.children == null ? this.dictionaryIterator.Current.Value : this.current; 
                }
            }

            public void Dispose()
            {
                if (this.isEmpty) return;
                if (this.children == null) this.dictionaryIterator.Dispose();
            }

            public ExistingChildEnumerator GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                if (this.isEmpty) return false;

                if (this.children == null) return this.dictionaryIterator.MoveNext();

                if (this.index >= this.count)
                    return false;

                while (true)
                {
                    this.index++;

                    if (this.index >= this.count)
                    {
                        this.current = null;
                        return false;
                    }

                    if (this.children.childrenByIndex.TryGetValue(this.index, out this.current))
                    {
                        return true;
                    }
                }
            }

            public void Reset()
            {
                this.index = -1;
                this.current = null;
            }
        }

        private InspectorPropertyInfo GetInfo(int index)
        {
            InspectorPropertyInfo info;

            if (!this.infosByIndex.TryGetValue(index, out info) || (this.refreshableResolver != null && this.refreshableResolver.ChildPropertyRequiresRefresh(index, info)))
            {
                info = this.resolver.GetChildInfo(index);
                this.infosByIndex[index] = info;
            }

            return info;
        }

        private bool NeedsRefresh(int index)
        {
            InspectorPropertyInfo info;
            return !this.infosByIndex.TryGetValue(index, out info) || (this.refreshableResolver != null && this.refreshableResolver.ChildPropertyRequiresRefresh(index, info));
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<InspectorProperty> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }
    }
}
#endif