//-----------------------------------------------------------------------
// <copyright file="ButtonStyle.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// Button style for methods with parameters.
    /// </summary>
    public enum ButtonStyle
    {
        /// <summary>
        /// Draws a foldout box around the parameters of the method with the button on the box header itself.
        /// This is the default style of a method with parameters.
        /// </summary>
        CompactBox,

        /// <summary>
        /// Draws a button with a foldout to expose the parameters of the method.
        /// </summary>
        FoldoutButton,

        /// <summary>
        /// Draws a foldout box around the parameters of the method with the button at the bottom of the box.
        /// </summary>
        Box,
    }
}