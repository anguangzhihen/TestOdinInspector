#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InheritAttributeAttributesAttributeProcessor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Resolvers
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor;

    /// <summary>
    /// This attribute processor will take any attribute already applied to the property with the <see cref="IncludeMyAttributesAttribute"/> applied to,
    /// and take all attributes applied to the attribute (except any <see cref="AttributeUsageAttribute"/>) and add to them to the property.
    /// This allows for adding attributes to attributes in the property system.
    /// </summary>
    [ResolverPriority(-100000)]
    public class InheritAttributeAttributesAttributeProcessor : OdinAttributeProcessor
    {
        private static readonly Type TypeOf_OnInspectorInitAttribute = typeof(OnInspectorInitAttribute);
        private static readonly Type TypeOf_PropertyOrderAttribute = typeof(PropertyOrderAttribute);

        /// <summary>
        /// Looks for attributes in the attributes list with a <see cref="IncludeMyAttributesAttribute"/> applied, and adds the attribute from those attributes to the property.
        /// </summary>
        /// <param name="parentProperty">The parent of the member.</param>
        /// <param name="member">The member that is being processed.</param>
        /// <param name="attributes">The list of attributes currently applied to the property.</param>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            bool hasOnInspectorInit = false;
            bool hasPropertyOrder = false;

            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                var type = attributes[i].GetType();

                if (type.IsDefined(typeof(IncludeMyAttributesAttribute), false))
                {
                    var attrs = type.GetCustomAttributes(false);

                    for (int j = 0; j < attrs.Length; j++)
                    {
                        var attr = attrs[j];
                        if (attr is AttributeUsageAttribute) continue;
                        attributes.Add(attr as Attribute);
                    }
                }

                if (type == TypeOf_OnInspectorInitAttribute)
                {
                    hasOnInspectorInit = true;
                }
                else if (type == TypeOf_PropertyOrderAttribute)
                {
                    hasPropertyOrder = true;
                }
            }

            if (hasOnInspectorInit && !hasPropertyOrder && member is MethodInfo)
            {
                attributes.Add(new PropertyOrderAttribute(int.MinValue));
            }
        }
    }
}
#endif