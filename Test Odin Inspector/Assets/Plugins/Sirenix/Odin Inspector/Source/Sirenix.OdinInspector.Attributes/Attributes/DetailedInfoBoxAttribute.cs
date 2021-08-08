//-----------------------------------------------------------------------
// <copyright file="DetailedInfoBoxAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>DetailedInfoBox is used on any property, and displays a message box that can be expanded to show more details.</para>
    /// <para>Use this to convey a message to a user, and give them the option to see more details.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how DetailedInfoBox is used on a field.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[DetailedInfoBox("This is a message", "Here is some more details about that message")]
    ///		public int MyInt;
    ///	}
    /// </code>
    /// </example>
    /// <seealso cref="InfoBoxAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [DontApplyToListElements]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class DetailedInfoBoxAttribute : Attribute
    {
        /// <summary>
        /// The message for the message box.
        /// </summary>
        public string Message;

        /// <summary>
        /// The hideable details of the message box.
        /// </summary>
        public string Details;

        /// <summary>
        /// Type of the message box.
        /// </summary>
        public InfoMessageType InfoMessageType;

        /// <summary>
        /// Optional name of a member to hide or show the message box.
        /// </summary>
        public string VisibleIf;

        /// <summary>
        /// Displays a message box with hideable details.
        /// </summary>
        /// <param name="message">The message for the message box.</param>
        /// <param name="details">The hideable details of the message box.</param>
        /// <param name="infoMessageType">Type of the message box.</param>
        /// <param name="visibleIf">Optional name of a member to hide or show the message box.</param>
        public DetailedInfoBoxAttribute(string message, string details, InfoMessageType infoMessageType = InfoMessageType.Info, string visibleIf = null)
        {
            this.Message = message;
            this.Details = details;
            this.InfoMessageType = infoMessageType;
            this.VisibleIf = visibleIf;
        }
    }
}