//-----------------------------------------------------------------------
// <copyright file="TwoWaySerializationBinder.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Binds types to strings during serialization, and strings to types during deserialization.
    /// </summary>
    public abstract class TwoWaySerializationBinder
    {
		/// <summary>
		/// Provides a default, catch-all <see cref="TwoWaySerializationBinder"/> implementation. This binder only includes assembly names, without versions and tokens, in order to increase compatibility.
		/// </summary>
		public static readonly TwoWaySerializationBinder Default = new DefaultSerializationBinder();
	
        /// <summary>
        /// Bind a type to a name.
        /// </summary>
        /// <param name="type">The type to bind.</param>
        /// <param name="debugContext">The debug context to log to.</param>
        /// <returns>The name that the type has been bound to.</returns>
        public abstract string BindToName(Type type, DebugContext debugContext = null);

        /// <summary>
        /// Binds a name to a type.
        /// </summary>
        /// <param name="typeName">The name of the type to bind.</param>
        /// <param name="debugContext">The debug context to log to.</param>
        /// <returns>The type that the name has been bound to, or null if the type could not be resolved.</returns>
        public abstract Type BindToType(string typeName, DebugContext debugContext = null);

        /// <summary>
        /// Determines whether the specified type name is mapped.
        /// </summary>
        public abstract bool ContainsType(string typeName);
    }
}