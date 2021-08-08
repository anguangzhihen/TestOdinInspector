#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MultiDimensionalArrayPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections;

    [ResolverPriority(-1)]
    public sealed class MultiDimensionalArrayPropertyResolver<T> : OdinPropertyResolver<T> where T : IList
    {
        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            var type = property.ValueEntry.TypeOfValue;
            return type.IsArray && type.GetArrayRank() > 1;
        }

        public override int ChildNameToIndex(string name)
        {
            return -1;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            throw new NotSupportedException();
        }

        protected override int GetChildCount(T value)
        {
            return 0;
        }
    }
}
#endif