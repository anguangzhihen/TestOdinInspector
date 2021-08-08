//-----------------------------------------------------------------------
// <copyright file="ImmutableHashSet.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Immutable hashset wraps another hashset, and allows for reading the inner hashset, without the ability to change it.
    /// </summary>
    [Serializable]
    public class ImmutableHashSet<T> : IEnumerable<T>
    {
        private readonly HashSet<T> hashSet;

        /// <summary>
        /// Creates an immutable hashset around another hashset.
        /// </summary>
        public ImmutableHashSet(HashSet<T> hashSet)
        {
            this.hashSet = hashSet;
        }

        /// <summary>
        /// Returns <c>true</c> if the item is contained in the list.
        /// </summary>
        /// <param name="item">The item's value.</param>
        public bool Contains(T item)
        {
            return this.hashSet.Contains(item);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return this.hashSet.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.hashSet.GetEnumerator();
        }
    }
}