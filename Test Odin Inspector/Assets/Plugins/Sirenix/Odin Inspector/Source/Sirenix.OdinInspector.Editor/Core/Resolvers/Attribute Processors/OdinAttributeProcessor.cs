#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinAttributeProcessor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Sirenix.Utilities.Editor;

    /// <summary>
    /// Attribute processor that can add, change and remove attributes from a property.
    /// </summary>
    public abstract class OdinAttributeProcessor : IHideObjectMembers
    {
        /// <summary>
        /// Instanciates an OdinAttributeProcessor instance of the specified type.
        /// </summary>
        /// <param name="processorType">The type of processor to instanciate. The type must inherit from <see cref="OdinAttributeProcessor"/>.</param>
        /// <returns>A new instance of the specified type.</returns>
        public static OdinAttributeProcessor Create(Type processorType)
        {
            if (processorType == null)
            {
                throw new ArgumentNullException("resolverType");
            }

            if (!typeof(OdinAttributeProcessor).IsAssignableFrom(processorType))
            {
                throw new ArgumentException("Type is not a AttributeResolver");
            }

            return (OdinAttributeProcessor)Activator.CreateInstance(processorType);
        }

        /// <summary>
        /// Checks if the processor can process attributes for the specified member.
        /// </summary>
        /// <param name="parentProperty">The parent property of the member.</param>
        /// <param name="member">The member to be processed.</param>
        /// <returns><c>true</c> if the processor can process for the specified member. Otherwise <c>false</c>.</returns>
        public virtual bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            return true;
        }

        /// <summary>
        /// Checks if the processor can process attributes for the specified property.
        /// </summary>
        /// <param name="property">The property to process.</param>
        /// <returns><c>true</c> if the processor can process attributes for the specified property. Otherwise <c>false</c>.</returns>
        public virtual bool CanProcessSelfAttributes(InspectorProperty property)
        {
            return true;
        }

        /// <summary>
        /// Processes attributes for the specified member.
        /// </summary>
        /// <param name="parentProperty">The parent property of the specified member.</param>
        /// <param name="member">The member to process attributes for.</param>
        /// <param name="attributes">The current attributes applied to the property.</param>
        public virtual void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
        }

        /// <summary>
        /// Processes attributes for the specified property.
        /// </summary>
        /// <param name="property">The property to process attributes for.</param>
        /// <param name="attributes">The current attributes applied to the property.</param>
        public virtual void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
        }
    }

    /// <summary>
    /// Attribute processor that can add, change and remove attributes from a property.
    /// </summary>
    public abstract class OdinAttributeProcessor<T> : OdinAttributeProcessor
    {
    }
}
#endif