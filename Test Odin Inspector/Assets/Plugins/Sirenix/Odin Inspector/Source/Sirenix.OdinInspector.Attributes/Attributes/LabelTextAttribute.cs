//-----------------------------------------------------------------------
// <copyright file="LabelTextAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>LabelText is used to change the labels of properties.</para>
    /// <para>Use this if you want a different label than the name of the property.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how LabelText is applied to a few property fields.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
    /// {
    ///		[LabelText("1")]
    ///		public int MyInt1;
    ///
    ///		[LabelText("2")]
    ///		public int MyInt2;
    ///
    ///		[LabelText("3")]
    ///		public int MyInt3;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TitleAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class LabelTextAttribute : Attribute
    {
        /// <summary>
        /// The new text of the label.
        /// </summary>
        public string Text;

        /// <summary>
        /// Whether the label text should be nicified before it is displayed, IE, "m_someField" becomes "Some Field".
        /// If the label text is resolved via a member reference, an expression, or the like, then the evaluated result 
        /// of that member reference or expression will be nicified.
        /// </summary>
        public bool NicifyText;

        /// <summary>
        /// Give a property a custom label.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        public LabelTextAttribute(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Give a property a custom label.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        /// <param name="nicifyText">Whether to nicify the label text.</param>
        public LabelTextAttribute(string text, bool nicifyText)
        {
            this.Text = text;
            this.NicifyText = nicifyText;
        }
    }
}