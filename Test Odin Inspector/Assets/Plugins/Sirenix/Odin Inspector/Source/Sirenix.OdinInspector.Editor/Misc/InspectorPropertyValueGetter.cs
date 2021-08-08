#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorPropertyValueGetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    ///	Helper class to get values from InspectorProperties. This class is deprecated and fully replaced by <see cref="Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolver" />.
    /// </summary>
    [Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.",
#if SIRENIX_INTERNAL
        true
#else
        false
#endif
        )]
    public class InspectorPropertyValueGetter<TReturnType>
    {
        private ValueResolver<TReturnType> backingResolver;

        public static readonly bool IsValueType = typeof(TReturnType).IsValueType;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage { get { return this.backingResolver.ErrorMessage; } }

        /// <summary>
        /// Gets the referenced member information.
        /// </summary>
        [Obsolete("A member is no longer guaranteed.", true)]
        public MemberInfo MemberInfo { get { throw new NotSupportedException("How have you even called this?? Just stop!"); } }

        [Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.",
#if SIRENIX_INTERNAL
        true
#else
        false
#endif
        )]
        public InspectorPropertyValueGetter(InspectorProperty property, string memberName, bool allowInstanceMember = true, bool allowStaticMember = true)
        {
            this.backingResolver = ValueResolver.Get<TReturnType>(property, memberName);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        [Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.",
#if SIRENIX_INTERNAL
        true
#else
        false
#endif
        )]
        public TReturnType GetValue()
        {
            return this.backingResolver.GetValue();
        }

        /// <summary>
        /// Gets all values from all targets.
        /// </summary>
        [Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.",
#if SIRENIX_INTERNAL
        true
#else
        false
#endif
        )]
        public IEnumerable<TReturnType> GetValues()
        {
            var count = this.backingResolver.Context.Property.Tree.WeakTargets.Count;

            for (int i = 0; i < count; i++)
            {
                yield return this.backingResolver.GetValue(i);
            }
        }
    }
}
#endif