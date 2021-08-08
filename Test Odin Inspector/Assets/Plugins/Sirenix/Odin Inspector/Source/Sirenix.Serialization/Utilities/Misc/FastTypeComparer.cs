//-----------------------------------------------------------------------
// <copyright file="FastTypeComparer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization.Utilities
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Compares types by reference before comparing them using the default type equality operator.
    /// This can constitute a *significant* speedup when used as the comparer for dictionaries.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEqualityComparer{System.Type}" />
    public class FastTypeComparer : IEqualityComparer<Type>
    {
        public static readonly FastTypeComparer Instance = new FastTypeComparer();

        public bool Equals(Type x, Type y)
        {
            if (object.ReferenceEquals(x, y)) return true; // Oft-used fast path over regular Type.Equals makes this much faster
            return x == y;
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }
}