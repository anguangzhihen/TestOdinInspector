//-----------------------------------------------------------------------
// <copyright file="GUIColorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
    using UnityEngine;

    /// <summary>
    /// <para>GUIColor is used on any property and changes the GUI color used to draw the property.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how GUIColor is used on a properties to create a rainbow effect.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[GUIColor(1f, 0f, 0f)]
    ///		public int A;
    ///	
    ///		[GUIColor(1f, 0.5f, 0f, 0.2f)]
    ///		public int B;
	///	
	///		[GUIColor("GetColor")]
	///		public int C;
	///		
	///		private Color GetColor() { return this.A == 0 ? Color.red : Color.white; }
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class GUIColorAttribute : Attribute
    {
        /// <summary>
        /// The GUI color of the property.
        /// </summary>
        public Color Color;

        /// <summary>
        /// The name of a local field, member or property that returns a Color. Both static and instance methods are supported.
        /// </summary>
        public string GetColor;

        /// <summary>
        /// Sets the GUI color for the property.
        /// </summary>
        /// <param name="r">The red channel.</param>
        /// <param name="g">The green channel.</param>
        /// <param name="b">The blue channel.</param>
        /// <param name="a">The alpha channel.</param>
        public GUIColorAttribute(float r, float g, float b, float a = 1f)
        {
            this.Color = new Color(r, g, b, a);
        }

        /// <summary>
        /// Sets the GUI color for the property.
        /// </summary>
        /// <param name="getColor">Specify the name of a local field, member or property that returns a Color.</param>
        public GUIColorAttribute(string getColor)
        {
            this.GetColor = getColor;
        }
    }
}