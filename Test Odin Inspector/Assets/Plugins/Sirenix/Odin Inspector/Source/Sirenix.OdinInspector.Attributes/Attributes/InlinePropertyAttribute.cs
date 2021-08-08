//-----------------------------------------------------------------------
// <copyright file="InlinePropertyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// The Inline Property is used to place the contents of a type next to the label, instead of being rendered in a foldout.
    /// </summary>
    /// <example>
    /// <code>
    /// public class InlinePropertyExamples : MonoBehaviour
    /// {
    ///     public Vector3 Vector3;
    ///
    ///     public Vector3Int Vector3Int;
    ///
    ///     [InlineProperty(LabelWidth = 12)]  // It can be placed on classes as well as members
    ///     public Vector2Int Vector2Int;
    ///
    /// }
    ///
    /// [Serializable]
    /// [InlineProperty(LabelWidth = 12)] // It can be placed on classes as well as members
    /// public struct Vector3Int
    /// {
    ///     [HorizontalGroup]
    ///     public int X;
    ///
    ///     [HorizontalGroup]
    ///     public int Y;
    ///
    ///     [HorizontalGroup]
    ///     public int Z;
    /// }
    ///
    /// [Serializable]
    /// public struct Vector2Int
    /// {
    ///     [HorizontalGroup]
    ///     public int X;
    ///
    ///     [HorizontalGroup]
    ///     public int Y;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class InlinePropertyAttribute : Attribute
    {
        /// <summary>
        /// Specify a label width for all child properties.
        /// </summary>
        public int LabelWidth;
    }
}