#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeDefinitionAttributeProcessor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Sirenix.Utilities;

    /// <summary>
    /// Find attributes attached to the type definition of a property and adds to them to attribute list.
    /// </summary>
    [ResolverPriority(1000)]
    public class TypeDefinitionAttributeProcessor : OdinAttributeProcessor
    {
        private static readonly Dictionary<Type, bool> HadResultCache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

        /// <summary>
        /// This attribute processor can only process for properties.
        /// </summary>
        /// <param name="parentProperty">The parent of the specified member.</param>
        /// <param name="member">The member to process.</param>
        /// <returns><c>false</c>.</returns>
        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// This attribute processor can only process for properties with an attached value entry.
        /// </summary>
        /// <param name="property">The property to process.</param>
        /// <returns><c>true</c> if the specified property has a value entry. Otherwise <c>false</c>.</returns>
        public override bool CanProcessSelfAttributes(InspectorProperty property)
        {
            var entry = property.ValueEntry;

            if (entry == null)
                return false;

            if (FastTypeComparer.Instance.Equals(entry.TypeOfValue, entry.BaseValueType))
            {
                bool result;

                if (HadResultCache.TryGetValue(entry.BaseValueType, out result))
                    return result;
            }

            return true;
        }

        /// <summary>
        /// Finds all attributes attached to the type and base types of the specified property value and adds them to the attribute list.
        /// </summary>
        /// <param name="property">The property to process.</param>
        /// <param name="attributes">The list of attributes for the property.</param>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            var current = property.ValueEntry.TypeOfValue;

            bool isBaseType = FastTypeComparer.Instance.Equals(current, property.ValueEntry.BaseValueType);

            if (isBaseType)
            {
                bool wasInCache = false;
                bool hadResult;

                var original = current;

                if (HadResultCache.TryGetValue(original, out hadResult))
                {
                    if (!hadResult) return;
                    wasInCache = true;
                }

                while (true)
                {
                    if (current == null) break;
                    var flag = AssemblyUtilities.GetAssemblyTypeFlag(current.Assembly);
                    if (flag == AssemblyTypeFlags.OtherTypes) break;
                    if (flag == AssemblyTypeFlags.UnityTypes) break;

                    if (current.IsDefined(typeof(Attribute), false))
                    {
                        hadResult = true;
                        attributes.AddRange(current.GetAttributes(false));
                    }

                    current = current.BaseType;
                }

                if (!wasInCache)
                {
                    HadResultCache.Add(original, hadResult);
                }
            }
            else
            {
                while (true)
                {
                    if (current == null) break;
                    var flag = AssemblyUtilities.GetAssemblyTypeFlag(current.Assembly);
                    if (flag == AssemblyTypeFlags.OtherTypes) break;
                    if (flag == AssemblyTypeFlags.UnityTypes) break;

                    if (current.IsDefined(typeof(Attribute), false))
                    {
                        attributes.AddRange(current.GetAttributes(false));
                    }

                    current = current.BaseType;
                }

                current = property.ValueEntry.BaseValueType;

                if (current.IsInterface)
                {
                    while (true)
                    {
                        if (current == null) break;
                        var flag = AssemblyUtilities.GetAssemblyTypeFlag(current.Assembly);
                        if (flag == AssemblyTypeFlags.OtherTypes) break;
                        if (flag == AssemblyTypeFlags.UnityTypes) break;

                        if (current.IsDefined(typeof(Attribute), false))
                        {
                            attributes.AddRange(current.GetAttributes(false));
                        }

                        current = current.BaseType;
                    }
                }
            }
        }
    }


    [ResolverPriority(1000)]
    public class TypeDefinitionGroupAttributeProcessor : OdinAttributeProcessor
    {
        private static readonly Dictionary<Type, bool> HadResultCache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            var current = member.GetReturnType();

            if (current == null)
                return false;

            bool result;

            if (HadResultCache.TryGetValue(current, out result))
                return result;

            return true;
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            var original = member.GetReturnType();
            var current = original;

            if (current == null) return;

            bool hadResult;
            bool wasInCache = false;

            if (HadResultCache.TryGetValue(original, out hadResult))
            {
                // We can return early; there will be no result
                if (!hadResult) return;
                wasInCache = true;
            }

            while (true)
            {
                if (current == null) break;
                var flag = AssemblyUtilities.GetAssemblyTypeFlag(current.Assembly);
                if (flag == AssemblyTypeFlags.OtherTypes) break;
                if (flag == AssemblyTypeFlags.UnityTypes) break;

                if (current.IsDefined(typeof(PropertyGroupAttribute), false))
                {
                    hadResult = true;
                    var attrs = current.GetCustomAttributes(typeof(PropertyGroupAttribute), false);

                    for (int i = 0; i < attrs.Length; i++)
                    {
                        attributes.Add(attrs[i] as Attribute);
                    }
                }

                current = current.BaseType;
            }

            if (!wasInCache)
            {
                HadResultCache.Add(original, hadResult);
            }
        }
    }
}
#endif