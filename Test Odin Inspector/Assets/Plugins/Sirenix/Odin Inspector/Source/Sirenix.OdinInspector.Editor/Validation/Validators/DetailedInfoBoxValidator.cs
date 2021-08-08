#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DetailedInfoBoxValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.DetailedInfoBoxValidator))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;
    using System.Reflection;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    [NoValidationInInspector]
    public class DetailedInfoBoxValidator : AttributeValidator<DetailedInfoBoxAttribute>
    {
        private ValueResolver<bool> showMessageGetter;
        private ValueResolver<string> messageGetter;
        private ValueResolver<string> detailsGetter;
        
        protected override void Initialize()
        {
            if (this.Attribute.VisibleIf != null)
            {
                this.showMessageGetter = ValueResolver.Get<bool>(this.Property, this.Attribute.VisibleIf, true);
                this.messageGetter = ValueResolver.GetForString(this.Property, this.Attribute.Message);
                this.detailsGetter = ValueResolver.GetForString(this.Property, this.Attribute.Details);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.showMessageGetter == null) return;

            if (this.showMessageGetter.HasError || this.messageGetter.HasError || this.showMessageGetter.HasError)
            {
                result.Message = ValueResolver.GetCombinedErrors(showMessageGetter, messageGetter, detailsGetter);
                result.ResultType = ValidationResultType.Error;
                return;
            }
            
            bool hasMessage = this.showMessageGetter.GetValue();

            if (hasMessage)
            {
                result.ResultType = this.Attribute.InfoMessageType.ToValidationResultType();
                result.Message = this.messageGetter.GetValue() + "\n\nDETAILS:\n\n" + this.detailsGetter.GetValue();
            }
        }
    }
}
#endif