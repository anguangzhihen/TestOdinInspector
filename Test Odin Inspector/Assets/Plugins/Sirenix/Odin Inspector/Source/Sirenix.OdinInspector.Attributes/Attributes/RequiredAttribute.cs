//-----------------------------------------------------------------------
// <copyright file="RequiredAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>Required is used on any object property, and draws a message in the inspector if the property is missing.</para>
    /// <para>Use this to clearly mark fields as necessary to the object.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows different uses of the Required attribute.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[Required]
    ///		public GameObject MyPrefab;
    ///
    ///		[Required(InfoMessageType.Warning)]
    ///		public Texture2D MyTexture;
    ///
    ///		[Required("MyMesh is nessessary for this component.")]
    ///		public Mesh MyMesh;
    ///
    ///		[Required("MyTransform might be important.", InfoMessageType.Info)]
    ///		public Transform MyTransform;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class RequiredAttribute : Attribute
    {
        /// <summary>
        /// The message of the info box.
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// The type of the info box.
        /// </summary>
        public InfoMessageType MessageType;

        /// <summary>
        /// Adds an error box to the inspector, if the property is missing.
        /// </summary>
        public RequiredAttribute()
        {
            this.MessageType = InfoMessageType.Error;
        }

        /// <summary>
        /// Adds an info box to the inspector, if the property is missing.
        /// </summary>
		/// <param name="errorMessage">The message to display in the error box.</param>
		/// <param name="messageType">The type of info box to draw.</param>
        public RequiredAttribute(string errorMessage, InfoMessageType messageType)
        {
            this.ErrorMessage = errorMessage;
            this.MessageType = messageType;
        }

        /// <summary>
        /// Adds an error box to the inspector, if the property is missing.
        /// </summary>
		/// <param name="errorMessage">The message to display in the error box.</param>
        public RequiredAttribute(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
            this.MessageType = InfoMessageType.Error;
        }

        /// <summary>
        /// Adds an info box to the inspector, if the property is missing.
        /// </summary>
		/// <param name="messageType">The type of info box to draw.</param>
        public RequiredAttribute(InfoMessageType messageType)
        {
            this.MessageType = messageType;
        }
    }
}