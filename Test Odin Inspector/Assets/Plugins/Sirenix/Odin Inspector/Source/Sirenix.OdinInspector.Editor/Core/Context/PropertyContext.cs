#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyContext.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using Sirenix.Utilities;

    /// <summary>
    /// <para>A contextual value attached to an <see cref="InspectorProperty"/>, mapped to a key, contained in a <see cref="PropertyContextContainer"/>.</para>
    /// </summary>
    public sealed class PropertyContext<T>
    {
        /// <summary>
        /// The contained value.
        /// </summary>
        public T Value;

        private PropertyContext()
        {
        }

        /// <summary>
        /// Creates a new PropertyContext.
        /// </summary>
        public static PropertyContext<T> Create()
        {
            return new PropertyContext<T>();
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="PropertyContext{T}"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator T(PropertyContext<T> context)
        {
            if (context == null)
            {
                return default(T);
            }
            else
            {
                return context.Value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance, of the format "<see cref="PropertyContext{T}"/>: Value.ToString()".
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.GetType().GetNiceName() + ": " + this.Value;
        }
    }
}
#endif