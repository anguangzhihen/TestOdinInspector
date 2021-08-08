#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FastMemberComparer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Collections.Generic;
    using System.Reflection;

    public class FastMemberComparer : IEqualityComparer<MemberInfo>
    {
        public static readonly FastMemberComparer Instance = new FastMemberComparer();

        public bool Equals(MemberInfo x, MemberInfo y)
        {
            if (object.ReferenceEquals(x, y)) return true; // Oft-used fast path over regular MemberInfo.Equals makes this much faster
            return x == y;
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
#endif