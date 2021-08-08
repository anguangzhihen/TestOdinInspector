//-----------------------------------------------------------------------
// <copyright file="LinqExtensions.cs" company="Sirenix IVS">
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
    /// Various LinQ extensions.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Calls an action on each item before yielding them.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="action">The action to call for each item.</param>
        public static IEnumerable<T> Examine<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Perform an action on each item.
        /// </summary>
		/// <param name="source">The source.</param>
		/// <param name="action">The action to perform.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }

            return source;
        }

        /// <summary>
        /// Perform an action on each item.
        /// </summary>
		/// <param name="source">The source.</param>
		/// <param name="action">The action to perform.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int counter = 0;

            foreach (var item in source)
            {
                action(item, counter++);
            }

            return source;
        }

        /// <summary>
        /// Convert each item in the collection.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="converter">Func to convert the items.</param>
        public static IEnumerable<T> Convert<T>(this IEnumerable source, Func<object, T> converter)
        {
            foreach (var item in source)
            {
                yield return converter(item);
            }
        }

        /// <summary>
        /// Convert a colletion to a HashSet.
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        /// <summary>
        /// Convert a colletion to a HashSet.
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(source, comparer);
        }

        /// <summary>
        /// Convert a collection to an immutable list.
        /// </summary>
		/// <param name="source">The collection.</param>
        public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source)
        {
            IList<T> iList = source as IList<T>;
            if (iList == null)
            {
                iList = source.ToArray();
            }
            return new ImmutableList<T>(iList);
        }

        /// <summary>
        /// Add an item to the beginning of a collection.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, Func<T> prepend)
        {
            yield return prepend();

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        ///	Add an item to the beginning of a collection.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, T prepend)
        {
            yield return prepend;

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, IEnumerable<T> prepend)
        {
            foreach (var item in prepend)
            {
                yield return item;
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, bool condition, Func<T> prepend)
        {
            if (condition)
            {
                yield return prepend();
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, bool condition, T prepend)
        {
            if (condition)
            {
                yield return prepend;
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, bool condition, IEnumerable<T> prepend)
        {
            if (condition)
            {
                foreach (var item in prepend)
                {
                    yield return item;
                }
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<bool> condition, Func<T> prepend)
        {
            if (condition())
            {
                yield return prepend();
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<bool> condition, T prepend)
        {
            if (condition())
            {
                yield return prepend;
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<bool> condition, IEnumerable<T> prepend)
        {
            if (condition())
            {
                foreach (var item in prepend)
                {
                    yield return item;
                }
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, bool> condition, Func<T> prepend)
        {
            if (condition(source))
            {
                yield return prepend();
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, bool> condition, T prepend)
        {
            if (condition(source))
            {
                yield return prepend;
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, bool> condition, IEnumerable<T> prepend)
        {
            if (condition(source))
            {
                foreach (var item in prepend)
                {
                    yield return item;
                }
            }

            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the end of a collection.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="append">Func to create the item to append.</param>
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, Func<T> append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            yield return append();
        }

        /// <summary>
        /// Add an item to the end of a collection.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="append">The item to append.</param>
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, T append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            yield return append;
        }

        /// <summary>
        /// Add a collection to the end of another collection.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="append">The collection to append.</param>
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, IEnumerable<T> append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            foreach (var item in append)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="append">Func to create the item to append.</param>
        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, Func<T> append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            if (condition)
            {
                yield return append();
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="append">The item to append.</param>
        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, T append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            if (condition)
            {
                yield return append;
            }
        }

        /// <summary>
        /// Add a collection to the end of another collection if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="append">The collection to append.</param>
        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, IEnumerable<T> append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            if (condition)
            {
                foreach (var item in append)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="append">Func to create the item to append.</param>
        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, Func<bool> condition, Func<T> append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            if (condition())
            {
                yield return append();
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="append">The item to append.</param>
        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, Func<bool> condition, T append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            if (condition())
            {
                yield return append;
            }
        }

        /// <summary>
        /// Add a collection to the end of another collection if a condition is met.
        /// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="append">The collection to append.</param>
        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, Func<bool> condition, IEnumerable<T> append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            if (condition())
            {
                foreach (var item in append)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns and casts only the items of type <typeparamref name="T"/>.
        /// </summary>
		/// <param name="source">The collection.</param>
        public static IEnumerable<T> FilterCast<T>(this IEnumerable source)
        {
            foreach (var obj in source)
            {
                if (obj is T)
                {
                    yield return (T)obj;
                }
            }
        }

        /// <summary>
        /// Adds a collection to a hashset.
        /// </summary>
		/// <param name="hashSet">The hashset.</param>
		/// <param name="range">The collection.</param>
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
        {
            foreach (var value in range)
            {
                hashSet.Add(value);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the list is either null or empty. Otherwise <c>false</c>.
        /// </summary>
		/// <param name="list">The list.</param>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Sets all items in the list to the given value.
        /// </summary>
		/// <param name="list">The list.</param>
		/// <param name="item">The value.</param>
        public static void Populate<T>(this IList<T> list, T item)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                list[i] = item;
            }
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the IList&lt;T&gt;.
        /// </summary>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            if (list is List<T>)
            {
                ((List<T>)list).AddRange(collection);
            }
            else
            {
                foreach (var item in collection)
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// Sorts an IList
        /// </summary>
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T>)
            {
                ((List<T>)list).Sort(comparison);
            }
            else
            {
                List<T> copy = new List<T>(list);
                copy.Sort(comparison);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = copy[i];
                }
            }
        }

        /// <summary>
        /// Sorts an IList
        /// </summary>
        public static void Sort<T>(this IList<T> list)
        {
            if (list is List<T>)
            {
                ((List<T>)list).Sort();
            }
            else
            {
                List<T> copy = new List<T>(list);
                copy.Sort();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = copy[i];
                }
            }
        }
    }
}