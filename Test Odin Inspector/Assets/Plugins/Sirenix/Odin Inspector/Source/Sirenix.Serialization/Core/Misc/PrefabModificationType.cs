//-----------------------------------------------------------------------
// <copyright file="PrefabModificationType.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Types of prefab modification that can be applied.
    /// </summary>
    public enum PrefabModificationType
    {
        /// <summary>
        /// A value has been changed at a given path.
        /// </summary>
        Value,

        /// <summary>
        /// A list length has been changed at a given path.
        /// </summary>
        ListLength,

        /// <summary>
        /// A dictionary has been changed at a given path.
        /// </summary>
        Dictionary
    }
}