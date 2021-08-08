#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MemberDefinedAttributeProcessor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Sirenix.Utilities;

    /// <summary>
    /// Finds all attributes attached to the specified member and adds to them to attribute list.
    /// </summary>
    [ResolverPriority(100000)]
    public class MemberDefinedAttributeProcessor : OdinAttributeProcessor
    {
        private static readonly Dictionary<MemberInfo, List<Attribute>> MemberAttributesCache = new Dictionary<MemberInfo, List<Attribute>>(FastMemberComparer.Instance);
        
        /// <summary>
        /// This attribute processor can only process for members.
        /// </summary>
        /// <param name="property">The property to process.</param>
        /// <returns><c>false</c>.</returns>
        public override bool CanProcessSelfAttributes(InspectorProperty property)
        {
            return false;
        }

        /// <summary>
        /// Finds all attributes attached to the specified member and adds to them to the attributes list.
        /// </summary>
        /// <param name="parentProperty">The parent property of the specified member.</param>
        /// <param name="member">The member to process attributes for.</param>
        /// <param name="attributes">The current attributes applied to the property.</param>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            List<Attribute> attrs;

            if (!MemberAttributesCache.TryGetValue(member, out attrs))
            {
                attrs = member.GetAttributes<Attribute>(true).ToList();
                MemberAttributesCache.Add(member, attrs);
            }

            attributes.AddRange(attrs);
        }
    }
}
#endif