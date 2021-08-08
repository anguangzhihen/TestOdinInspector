#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RequiredValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.RequiredValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class RequiredValidator<T> : AttributeValidator<RequiredAttribute, T>
        where T : class
    {
        private ValueResolver<string> errorMessageGetter;

        protected override void Initialize()
        {
            if (this.Attribute.ErrorMessage != null)
            {
                this.errorMessageGetter = ValueResolver.GetForString(this.Property, this.Attribute.ErrorMessage);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            if (!this.IsValid(this.ValueEntry.SmartValue))
            {
                result.ResultType = this.Attribute.MessageType.ToValidationResultType();
                result.Message = this.errorMessageGetter != null ? this.errorMessageGetter.GetValue() : (this.Property.NiceName + " is required");
            }
        }

        private bool IsValid(T memberValue)
        {
            if (object.ReferenceEquals(memberValue, null))
                return false;

            if (memberValue is string && string.IsNullOrEmpty(memberValue as string))
                return false;

            if (memberValue is UnityEngine.Object && (memberValue as UnityEngine.Object) == null)
                return false;

            return true;
        }
    }
}
#endif