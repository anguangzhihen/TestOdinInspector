#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeInfoBoxPropertyProcessor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Collections.Generic;
    using Sirenix.OdinInspector;

    [ResolverPriority(-10)]
    public class TypeInfoBoxPropertyProcessor<T> : OdinPropertyProcessor<T, TypeInfoBoxAttribute>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
        {
            var attr = this.Property.GetAttribute<TypeInfoBoxAttribute>();
            memberInfos.AddDelegate("InjectedTypeInfoBox", () => { }, -100000, new InfoBoxAttribute(attr.Message), new OnInspectorGUIAttribute("@"));
        }
    }
}
#endif