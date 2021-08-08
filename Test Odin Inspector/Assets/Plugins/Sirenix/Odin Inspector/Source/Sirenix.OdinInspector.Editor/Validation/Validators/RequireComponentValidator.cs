#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RequireComponentValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.RequireComponentValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using UnityEngine;

    public class RequireComponentValidator<T> : AttributeValidator<RequireComponent, T>
        where T : Component
    {
        public override bool CanValidateProperty(InspectorProperty property)
        {
            return property == property.Tree.RootProperty;
        }

        protected override void Validate(ValidationResult result)
        {
            var value = this.ValueEntry.SmartValue;

            bool ignore = false;

            if ((value as UnityEngine.Object) == null)
                ignore = true;

            if (ignore)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }

            if (Attribute.m_Type0 != null && typeof(UnityEngine.Component).IsAssignableFrom(Attribute.m_Type0))
            {
                if (value.gameObject.GetComponent(Attribute.m_Type0) == null)
                {
                    result.Message = "GameObject is missing required component of type '" + Attribute.m_Type0.GetNiceName() + "'";
                    result.ResultType = ValidationResultType.Error;
                }
            }

            if (Attribute.m_Type1 != null && typeof(UnityEngine.Component).IsAssignableFrom(Attribute.m_Type1))
            {
                if (value.gameObject.GetComponent(Attribute.m_Type1) == null)
                {
                    result.Message += "\n\nGameObject is missing required component of type '" + Attribute.m_Type1.GetNiceName() + "'";
                    result.ResultType = ValidationResultType.Error;
                }
            }

            if (Attribute.m_Type2 != null && typeof(UnityEngine.Component).IsAssignableFrom(Attribute.m_Type2))
            {
                if (value.gameObject.GetComponent(Attribute.m_Type2) == null)
                {
                    result.Message += "\n\nGameObject is missing required component of type '" + Attribute.m_Type2.GetNiceName() + "'";
                    result.ResultType = ValidationResultType.Error;
                }
            }
        }
    }
}
#endif