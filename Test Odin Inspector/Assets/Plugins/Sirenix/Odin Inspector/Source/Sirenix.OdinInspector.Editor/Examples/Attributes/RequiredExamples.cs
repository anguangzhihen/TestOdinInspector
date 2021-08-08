#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RequiredExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(RequiredAttribute), "Required displays an error when objects are missing.")]
    internal class RequiredExamples
    {
        [Required]
        public GameObject MyGameObject;

        [Required("Custom error message.")]
        public Rigidbody MyRigidbody;

		[InfoBox("Use $ to indicate a member string as message.")]
		[Required("$DynamicMessage")]
		public GameObject GameObject;

		public string DynamicMessage = "Dynamic error message";
    }
}
#endif