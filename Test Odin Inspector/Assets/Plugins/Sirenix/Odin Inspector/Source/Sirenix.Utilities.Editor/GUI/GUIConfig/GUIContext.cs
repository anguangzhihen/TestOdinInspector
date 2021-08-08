#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIContext.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// This class is due to undergo refactoring.
    /// </summary>
    public class GUIContext<T> : IControlContext
    {
        internal bool HasValue = false;

        /// <summary>
        /// The value.
        /// </summary>
        public T Value;

        /// <summary>
        /// Performs an implicit conversion from <see cref="GUIContext{T}"/> to <see cref="T"/>.
        /// </summary>
        public static implicit operator T(GUIContext<T> context)
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

        int IControlContext.LastRenderedFrameId { get; set; }
    }
}
#endif