#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssemblyTypeFlags.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;

    /// <summary>
    /// AssemblyTypeFlags is a bitmask used to filter types and assemblies related to Unity.
    /// </summary>
    /// <seealso cref="Sirenix.Utilities.AssemblyUtilities"/>
    [Flags]
    public enum AssemblyTypeFlags
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
        /// UserEditorTypes includes all custom user scripts that are located in an editor folder but not in a plugins folder.
        /// </summary>
        UserEditorTypes = 1 << 3,

        /// <summary>
        /// PluginEditorTypes includes all editor types located in the plugins folder.
        /// </summary>
        PluginEditorTypes = 1 << 4,

        /// <summary>
        /// UnityEditorTypes includes all editor types that are not user editor types nor plugin editor types.
        /// </summary>
        UnityEditorTypes = 1 << 5,

        /// <summary>
        /// OtherTypes includes all other types that are not depended on UnityEngine or UnityEditor.
        /// </summary>
        OtherTypes = 1 << 6,

        /// <summary>
        /// CustomTypes includes includes all types manually added to the Unity project.
        /// This includes UserTypes, UserEditorTypes, PluginTypes and PluginEditorTypes.
        /// </summary>
        CustomTypes = UserTypes | UserEditorTypes | PluginTypes | PluginEditorTypes,

        /// <summary>
        /// GameTypes includes all assemblies that are likely to be included in builds.
        /// This includes UserTypes, PluginTypes, UnityTypes and OtherTypes.
        /// </summary>
        GameTypes = UserTypes | PluginTypes | UnityTypes | OtherTypes,

        /// <summary>
        /// EditorTypes includes UserEditorTypes, PluginEditorTypes and UnityEditorTypes.
        /// </summary>
        EditorTypes = UserEditorTypes | PluginEditorTypes | UnityEditorTypes,

        /// <summary>
        /// All includes UserTypes, PluginTypes, UnityTypes, UserEditorTypes, PluginEditorTypes, UnityEditorTypes and OtherTypes.
        /// </summary>
        All = UserTypes | PluginTypes | UnityTypes | UserEditorTypes | PluginEditorTypes | UnityEditorTypes | OtherTypes
    }
}
#endif