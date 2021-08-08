//-----------------------------------------------------------------------
// <copyright file="TitleAlignments.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// Title alignment enum used by various attributes.
    /// </summary>
    /// <seealso cref="TitleGroupAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    public enum TitleAlignments
    {
        /// <summary>
        /// Title and subtitle left aligned.
        /// </summary>
        Left,

        /// <summary>
        /// Title and subtitle centered aligned.
        /// </summary>
        Centered,

        /// <summary>
        /// Title and subtitle right aligned.
        /// </summary>
        Right,

        /// <summary>
        /// Title on the left, subtitle on the right.
        /// </summary>
        Split,
    }
}