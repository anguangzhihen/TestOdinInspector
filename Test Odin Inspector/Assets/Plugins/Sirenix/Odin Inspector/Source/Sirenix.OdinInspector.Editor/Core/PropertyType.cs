#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyType.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// Enumeration describing the different types of properties that exist.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// Property represents a value.
        /// </summary>
        Value = 0,

        /// <summary>
        /// Property represents a method.
        /// </summary>
        Method = 1,

        /// <summary>
        /// Property represents a named group of properties.
        /// </summary>
        Group = 2
    }
}
#endif