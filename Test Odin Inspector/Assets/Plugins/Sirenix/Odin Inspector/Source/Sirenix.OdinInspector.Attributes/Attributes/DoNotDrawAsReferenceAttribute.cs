//-----------------------------------------------------------------------
// <copyright file="DoNotDrawAsReferenceAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Indicates that the member should not be drawn as a value reference, if it becomes a reference to another value in the tree. Beware, and use with care! This may lead to infinite draw loops!
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class DoNotDrawAsReferenceAttribute : Attribute
    {
    }
}