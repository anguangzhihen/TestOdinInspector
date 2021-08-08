//-----------------------------------------------------------------------
// <copyright file="IndentAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
	/// <para>Indent is used on any property and moves the property's label to the right.</para>
	/// <para>Use this to clearly organize properties in the inspector.</para>
    /// </summary>
	/// <example>
	/// <para>The following example shows how a property is indented by Indent.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[Indent]
	///		public int IndentedInt;
	///	}
	/// </code>
    /// </example>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class IndentAttribute : Attribute
    {
        /// <summary>
        ///	Indicates how much a property should be indented.
        /// </summary>
        public int IndentLevel;

        /// <summary>
        /// Indents a property in the inspector.
        /// </summary>
        /// <param name="indentLevel">How much a property should be indented.</param>
        public IndentAttribute(int indentLevel = 1)
        {
            this.IndentLevel = indentLevel;
        }
    }
}