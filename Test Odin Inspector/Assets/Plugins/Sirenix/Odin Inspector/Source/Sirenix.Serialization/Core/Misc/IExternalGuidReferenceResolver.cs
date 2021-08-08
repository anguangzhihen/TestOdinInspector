//-----------------------------------------------------------------------
// <copyright file="IExternalGuidReferenceResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Resolves external guid references to reference objects during serialization and deserialization.
    /// </summary>
    public interface IExternalGuidReferenceResolver
    {
        /// <summary>
        /// Gets or sets the next resolver in the chain.
        /// </summary>
        /// <value>
        /// The next resolver in the chain.
        /// </value>
        IExternalGuidReferenceResolver NextResolver { get; set; }

        /// <summary>
        /// Tries to resolve a reference from a given Guid.
        /// </summary>
        /// <param name="guid">The Guid to resolve.</param>
        /// <param name="value">The resolved value.</param>
        /// <returns><c>true</c> if the value was resolved; otherwise, <c>false</c>.</returns>
        bool TryResolveReference(Guid guid, out object value);

        /// <summary>
        /// Determines whether this resolver can reference the specified value with a Guid.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="guid">The Guid which references the value.</param>
        /// <returns><c>true</c> if the value can be referenced; otherwise, <c>false</c>.</returns>
        bool CanReference(object value, out Guid guid);
    }
}