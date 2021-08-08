//-----------------------------------------------------------------------
// <copyright file="TypeFilterAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class TypeFilterAttribute : Attribute
    {

        /// <summary>
        /// Name of any field, property or method member that implements IList. E.g. arrays or Lists. Obsolete; use the FilterGetter member instead.
        /// </summary>
        [Obsolete("Use the FilterGetter member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string MemberName { get { return this.FilterGetter; } set { this.FilterGetter = value; } }

        /// <summary>
        /// A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.
        /// </summary>
        public string FilterGetter;

        /// <summary>
        /// Gets or sets the title for the dropdown. Null by default.
        /// </summary>
        public string DropdownTitle;

        /// <summary>
        /// Creates a dropdown menu for a property.
        /// </summary>
        /// <param name="filterGetter">A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.</param>
        public TypeFilterAttribute(string filterGetter)
        {
            this.FilterGetter = filterGetter;
        }
    }
}