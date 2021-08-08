//-----------------------------------------------------------------------
// <copyright file="TypeInfoBoxAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>The TypeInfoBox attribute adds an info box to the very top of a type in the inspector.</para>
    /// <para>Use this to add an info box to the top of a class in the inspector, without having to use neither the PropertyOrder nor the OnInspectorGUI attribute.</para>
    /// </summary>
    /// <example>
    /// <para>The following example demonstrates the use of the TypeInfoBox attribute.</para>
    /// <code>
    /// [TypeInfoBox("This is my component and it is mine.")]
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     // Class implementation.
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="DetailedInfoBoxAttribute"/>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class TypeInfoBoxAttribute : Attribute
    {
        /// <summary>
        /// The message to display in the info box.
        /// </summary>
        public string Message;

        /// <summary>
        /// Draws an info box at the top of a type in the inspector.
        /// </summary>
        /// <param name="message">The message to display in the info box.</param>
        public TypeInfoBoxAttribute(string message)
        {
            this.Message = message;
        }
    }
}