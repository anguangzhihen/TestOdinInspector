#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ChildGameObjectsOnlyValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.ChildGameObjectsOnlyValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using UnityEngine;

    [NoValidationInInspector]
    public class ChildGameObjectsOnlyValidator<T> : AttributeValidator<ChildGameObjectsOnlyAttribute, T>
        where T : UnityEngine.Object
    {
        protected override void Validate(ValidationResult result)
        {
            GameObject ownerGo = result.Setup.Root as GameObject;

            if (ownerGo == null)
            {
                Component component = result.Setup.Root as Component;

                if (component != null)
                {
                    ownerGo = component.gameObject;
                }
            }

            GameObject valueGo = this.ValueEntry.SmartValue as GameObject;

            if (valueGo == null)
            {
                Component component = this.ValueEntry.SmartValue as Component;

                if (component != null)
                {
                    valueGo = component.gameObject;
                }
            }

            // Attribute doesn't apply in this context, as we're not on a GameObject
            // or are not dealing with the right kind of value
            if (ownerGo == null || valueGo == null)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }

            if (this.Attribute.IncludeSelf && ownerGo == valueGo)
            {
                result.ResultType = ValidationResultType.Valid;
                return;
            }
            
            Transform current = valueGo.transform;

            while (true)
            {
                current = current.parent;

                if (current == null)
                    break;

                if (current.gameObject == ownerGo)
                {
                    result.ResultType = ValidationResultType.Valid;
                    return;
                }
            }

            result.ResultType = ValidationResultType.Error;
            result.Message = valueGo.name + " must be a child of " + ownerGo.name;
        }
    }
    
}
#endif