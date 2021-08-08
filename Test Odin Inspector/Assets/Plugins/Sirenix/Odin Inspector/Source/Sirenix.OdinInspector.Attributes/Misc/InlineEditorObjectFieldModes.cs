//-----------------------------------------------------------------------
// <copyright file="InlineEditorObjectFieldModes.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// How the InlineEditor attribute drawer should draw the object field.
    /// </summary>
    public enum InlineEditorObjectFieldModes
    {
        /// <summary>
        /// Draws the object field in a box.
        /// </summary>
        Boxed,

        /// <summary>
        /// Draws the object field with a foldout.
        /// </summary>
        Foldout,

        /// <summary>
        /// Hides the object field unless it's null.
        /// </summary>
        Hidden,

        /// <summary>
        /// Hidden the object field also when the object is null.
        /// </summary>
        CompletelyHidden,
    }
}