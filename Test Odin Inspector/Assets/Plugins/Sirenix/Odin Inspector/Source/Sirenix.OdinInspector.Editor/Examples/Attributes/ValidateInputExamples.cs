#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidateInputExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using UnityEngine;

    [AttributeExample(typeof(ValidateInputAttribute), 
        "ValidateInput is used to display error boxes in case of invalid values.\nIn this case the GameObject must have a MeshRenderer component.")]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class ValidateInputExamples
    {
#if UNITY_EDITOR // MyScriptyScriptableObject is an example type and only exists in the editor
        [HideLabel]
        [Title("Default message", "You can just provide a default message that is always used")]
        [ValidateInput("MustBeNull", "This field should be null.")]
        public MyScriptyScriptableObject DefaultMessage;
#endif

        [Space(12), HideLabel]
        [Title("Dynamic message", "Or the validation method can dynamically provide a custom message")]
        [ValidateInput("HasMeshRendererDynamicMessage", "Prefab must have a MeshRenderer component")]
        public GameObject DynamicMessage;

        [Space(12), HideLabel]
        [Title("Dynamic message type", "The validation method can also control the type of the message")]
        [ValidateInput("HasMeshRendererDynamicMessageAndType", "Prefab must have a MeshRenderer component")]
        public GameObject DynamicMessageAndType;

        [Space(8), HideLabel]
        [InfoBox("Change GameObject value to update message type", InfoMessageType.None)]
        public InfoMessageType MessageType;

        [Space(12), HideLabel]
        [Title("Dynamic default message", "Use $ to indicate a member string as default message")]
        [ValidateInput("AlwaysFalse", "$Message", InfoMessageType.Warning)]
        public string Message = "Dynamic ValidateInput message";

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        private bool AlwaysFalse(string value)
        {
            return false;
        }

        private bool MustBeNull(MyScriptyScriptableObject scripty)
        {
            return scripty == null;
        }

        private bool HasMeshRendererDefaultMessage(GameObject gameObject)
        {
            if (gameObject == null) return true;

            return gameObject.GetComponentInChildren<MeshRenderer>() != null;
        }

        private bool HasMeshRendererDynamicMessage(GameObject gameObject, ref string errorMessage)
        {
            if (gameObject == null) return true;

            if (gameObject.GetComponentInChildren<MeshRenderer>() == null)
            {
                // If errorMessage is left as null, the default error message from the attribute will be used
                errorMessage = "\"" + gameObject.name + "\" must have a MeshRenderer component";

                return false;
            }

            return true;
        }

        private bool HasMeshRendererDynamicMessageAndType(GameObject gameObject, ref string errorMessage, ref InfoMessageType? messageType)
        {
            if (gameObject == null) return true;

            if (gameObject.GetComponentInChildren<MeshRenderer>() == null)
            {
                // If errorMessage is left as null, the default error message from the attribute will be used
                errorMessage = "\"" + gameObject.name + "\" should have a MeshRenderer component";

                // If messageType is left as null, the default message type from the attribute will be used
                messageType = this.MessageType;

                return false;
            }

            return true;
        }
#endif
    }
}
#endif