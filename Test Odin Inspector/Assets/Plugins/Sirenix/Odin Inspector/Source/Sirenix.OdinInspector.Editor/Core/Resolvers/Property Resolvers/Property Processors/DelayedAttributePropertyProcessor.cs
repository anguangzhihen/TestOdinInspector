#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DelayedAttributePropertyProcessor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEngine;
    using System.Collections.Generic;

    [ResolverPriority(-1000000)]
    public class DelayedAttributeProcessor<T> : OdinPropertyProcessor<T, DelayedAttribute>
        where T : struct
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            for (int i = 0; i < propertyInfos.Count; i++)
            {
                propertyInfos[i].GetEditableAttributesList().Add(new DelayedAttribute());
            }
        }
    }

    [ResolverPriority(-1000000)]
    public class DelayedPropertyAttributeProcessor<T> : OdinPropertyProcessor<T, DelayedPropertyAttribute>
        where T : struct
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            for (int i = 0; i < propertyInfos.Count; i++)
            {
                propertyInfos[i].GetEditableAttributesList().Add(new DelayedPropertyAttribute());
            }
        }
    }
}
#endif