#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SceneObjectsOnlyValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.SceneObjectsOnlyValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;

    public class SceneObjectsOnlyValidator<T> : AttributeValidator<SceneObjectsOnlyAttribute, T>
        where T : UnityEngine.Object
    {
        protected override void Validate(ValidationResult result)
        {
            var value = this.ValueEntry.SmartValue;

            if (value != null && AssetDatabase.Contains(value))
            {
                string name = value.name;
                var component = value as Component;
                if (component != null)
                {
                    name = "from " + component.gameObject.name;
                }

                result.ResultType = ValidationResultType.Error;
                result.Message = (value as object).GetType().GetNiceName() + " " + name + " cannot be an asset.";
            }
        }
    }
    
}
#endif