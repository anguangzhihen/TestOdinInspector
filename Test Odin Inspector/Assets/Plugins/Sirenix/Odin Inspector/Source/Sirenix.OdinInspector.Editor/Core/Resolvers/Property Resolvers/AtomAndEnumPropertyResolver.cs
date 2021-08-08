#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AtomAndEnumPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    [ResolverPriority(1000)]
    public class AtomAndEnumPropertyResolver<TValue> : OdinPropertyResolver<TValue>, IMaySupportPrefabModifications
    {
        public bool MaySupportPrefabModifications { get { return true; } }

        public override int ChildNameToIndex(string name)
        {
            return -1;
        }

        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            var type = property.ValueEntry.TypeOfValue;
            return type.IsEnum || AtomHandlerLocator.IsMarkedAtomic(type);
        }

        protected override int GetChildCount(TValue value)
        {
            return 0;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            throw new System.NotSupportedException();
        }
    }
}
#endif