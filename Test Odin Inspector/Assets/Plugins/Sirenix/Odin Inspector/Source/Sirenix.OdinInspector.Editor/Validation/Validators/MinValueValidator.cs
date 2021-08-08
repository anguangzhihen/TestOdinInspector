#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MinValueValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.MinValueValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class MinValueValidator<T> : AttributeValidator<MinValueAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> minValueGetter;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            return IsNumber || IsVector;
        }

        protected override void Initialize()
        {
            this.minValueGetter = ValueResolver.Get<double>(this.Property, this.Attribute.Expression, this.Attribute.MinValue);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.minValueGetter.HasError)
            {
                result.Message = this.minValueGetter.ErrorMessage;
                result.ResultType = ValidationResultType.Error;
                return;
            }

            var min = this.minValueGetter.GetValue();

            if (!GenericNumberUtility.NumberIsInRange(this.ValueEntry.SmartValue, min, double.MaxValue))
            {
                result.Message = "Number is smaller than " + min + ".";
                result.ResultType = ValidationResultType.Error;
            }
        }
    }
}
#endif