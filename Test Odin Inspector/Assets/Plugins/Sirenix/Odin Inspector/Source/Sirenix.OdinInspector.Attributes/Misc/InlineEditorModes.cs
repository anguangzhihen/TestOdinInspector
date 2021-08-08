//-----------------------------------------------------------------------
// <copyright file="InlineEditorModes.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// Editor modes for <see cref="InlineEditorAttribute" />
    /// </summary>
    /// <seealso cref="InlineEditorAttribute" />
    public enum InlineEditorModes
    {
        /// <summary>
        /// Draws only the editor GUI
        /// </summary>
        GUIOnly = 0,

        /// <summary>
        /// Draws the editor GUI and the editor header.
        /// </summary>
        GUIAndHeader = 1,

        /// <summary>
        /// Draws the editor GUI to the left, and a small editor preview to the right.
        /// </summary>
        GUIAndPreview = 2,

        /// <summary>
        /// Draws a small editor preview without any GUI.
        /// </summary>
        SmallPreview = 3,

        /// <summary>
        /// Draws a large editor preview without any GUI.
        /// </summary>
        LargePreview = 4,

        /// <summary>
        /// Draws the editor header and GUI to the left, and a small editor preview to the right.
        /// </summary>
        FullEditor = 5
    }
}