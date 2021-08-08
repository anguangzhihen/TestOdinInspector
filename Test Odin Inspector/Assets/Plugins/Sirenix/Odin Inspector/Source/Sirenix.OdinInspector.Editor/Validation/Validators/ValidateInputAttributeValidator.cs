#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidateInputAttributeValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.ValidateInputAttributeValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class ValidateInputAttributeValidator<T> : AttributeValidator<ValidateInputAttribute, T>
    {
        private static readonly NamedValue[] customValidationArgs = new NamedValue[]
        {
            new NamedValue("value", typeof(T)),
            new NamedValue("message", typeof(string)),
            new NamedValue("messageType", typeof(InfoMessageType?)),
        };

        public override RevalidationCriteria RevalidationCriteria
        {
            get
            {
                if (this.Attribute.ContinuousValidationCheck)
                    return RevalidationCriteria.Always;

                if (this.Attribute.IncludeChildren)
                    return RevalidationCriteria.OnValueChangeOrChildValueChange;

                return RevalidationCriteria.OnValueChange;
            }
        }

        private ValidationResultType defaultResultType;
        private ValueResolver<string> defaultValidationMessageGetter;
        private ValueResolver<bool> validationChecker;

        protected override void Initialize()
        {
            this.defaultResultType = this.Attribute.MessageType.ToValidationResultType();
            this.defaultValidationMessageGetter = ValueResolver.Get<string>(this.Property, this.Attribute.DefaultMessage, this.Attribute.DefaultMessage ?? "Value is invalid for '" + this.Property.NiceName + "'");
            this.validationChecker = ValueResolver.Get<bool>(this.Property, this.Attribute.Condition, customValidationArgs);
            this.validationChecker.Context.SyncRefParametersWithNamedValues = true;
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.defaultValidationMessageGetter.HasError || this.validationChecker.HasError)
            {
                result.Message = ValueResolver.GetCombinedErrors(this.validationChecker, this.defaultValidationMessageGetter);
                result.ResultType = ValidationResultType.Error;
                return;
            }

            this.validationChecker.Context.NamedValues.Set("value", this.ValueEntry.SmartValue);
            this.validationChecker.Context.NamedValues.Set("message", null);
            this.validationChecker.Context.NamedValues.Set("messageType", null);

            var isValid = this.validationChecker.GetValue();

            if (!isValid)
            {
                var messageParam = (string)this.validationChecker.Context.NamedValues.GetValue("message");
                var resultTypeParam = (InfoMessageType?)this.validationChecker.Context.NamedValues.GetValue("messageType");

                result.Message = messageParam ?? this.defaultValidationMessageGetter.GetValue();
                result.ResultType = resultTypeParam.HasValue ? resultTypeParam.Value.ToValidationResultType() : this.defaultResultType;
            }
        }
    }
}
#endif