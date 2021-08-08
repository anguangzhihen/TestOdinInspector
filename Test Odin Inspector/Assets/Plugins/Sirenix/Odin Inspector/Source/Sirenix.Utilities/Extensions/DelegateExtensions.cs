//-----------------------------------------------------------------------
// <copyright file="DelegateExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Delegate method extensions.
    /// </summary>
    public static class DelegateExtensions
    {
        /// <summary>
        /// Memoizes the specified func - returns the memoized version
        /// </summary>
        public static Func<TResult> Memoize<TResult>(this Func<TResult> getValue)
        {
            TResult value = default(TResult);
            bool hasValue = false;
            return () =>
            {
                if (!hasValue)
                {
                    hasValue = true;
                    value = getValue();
                }

                return value;
            };
        }

        /// <summary>
        /// Memoizes the specified func - returns the memoized version
        /// </summary>
        public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> func)
        {
            var dic = new Dictionary<T, TResult>();
            return n =>
            {
                TResult result;
                if (!dic.TryGetValue(n, out result))
                {
                    result = func(n);
                    dic.Add(n, result);
                }

                return result;
            };
        }
    }
}