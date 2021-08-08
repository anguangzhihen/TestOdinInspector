#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUICallType.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// Specifies hows any given drawer should drawer the property.
    /// Changing this behavior, also changes which methods should be overridden in the drawer.
    /// </summary>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinGroupDrawer{TGroupAttribute}"/>

    [Obsolete("Removed support GUICallType.Rect and DrawPropertyRect as it didn't really do much. You can get the same behaviour by overriding DrawPropertyLayout and calling GUILayoutUtility.GetRect or EditorGUILayout.GetControlRect.", true)]
    public enum GUICallType
    {
        /// <summary>
        /// GUILayout enabled the use of GUILayout, EditorGUILayout and <see cref="Utilities.Editor.SirenixEditorGUI"/>
        /// </summary>
        GUILayout,

        /// <summary>
        /// Draws the property using Unity's GUI, and EditorGUI.
        /// </summary>
        Rect
    }
}
#endif