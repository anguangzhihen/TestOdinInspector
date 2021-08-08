#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorDefaultEditors.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// InspectorDefaultEditors is a bitmask used to tell <see cref="InspectorConfig"/> which types should have an Odin Editor generated.
    /// </summary>
    /// <seealso cref="InspectorConfig"/>
    [Flags]
    public enum InspectorDefaultEditors
    {
        /// <summary>
        /// Excludes all types.
        /// </summary>
        None = 0,

        /// <summary>
        /// UserTypes includes all custom user scripts that are not located in an editor or plugin folder.
        /// </summary>
        UserTypes = 1 << 0,

        /// <summary>
        /// PluginTypes includes all types located in the plugins folder and are not located in an editor folder.
        /// </summary>
        PluginTypes = 1 << 1,

        /// <summary>
        /// UnityTypes includes all types depended on UnityEngine and from UnityEngine, except editor, plugin and user types.
        /// </summary>
        UnityTypes = 1 << 2,

        /// <summary>
        /// OtherTypes include all other types that are not depended on UnityEngine or UnityEditor.
        /// </summary>
        OtherTypes = 1 << 3
    }
}
#endif