#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EmptyPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    [OdinDontRegister]
    public class EmptyPropertyResolver : OdinPropertyResolver
    {
        public override int ChildNameToIndex(string name)
        {
            return -1;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            throw new NotSupportedException();
        }

        protected override int CalculateChildCount()
        {
            return 0;
        }

        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            return property != property.Tree.RootProperty;
        }
    }
}
#endif