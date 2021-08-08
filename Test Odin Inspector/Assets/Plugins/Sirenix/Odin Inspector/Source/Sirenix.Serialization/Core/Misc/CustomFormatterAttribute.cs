//-----------------------------------------------------------------------
// <copyright file="CustomFormatterAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Attribute indicating that a class which implements the <see cref="IFormatter{T}" /> interface somewhere in its hierarchy is a custom formatter for the type T.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    [Obsolete("Use a RegisterFormatterAttribute applied to the containing assembly instead.", true)]
    public class CustomFormatterAttribute : Attribute
    {
        /// <summary>
        /// The priority of the formatter. Of all the available custom formatters, the formatter with the highest priority is always chosen.
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFormatterAttribute"/> class with priority 0.
        /// </summary>
        public CustomFormatterAttribute()
        {
            this.Priority = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFormatterAttribute"/> class.
        /// </summary>
        /// <param name="priority">The priority of the formatter. Of all the available custom formatters, the formatter with the highest priority is always chosen.</param>
        public CustomFormatterAttribute(int priority = 0)
        {
            this.Priority = priority;
        }
    }
}