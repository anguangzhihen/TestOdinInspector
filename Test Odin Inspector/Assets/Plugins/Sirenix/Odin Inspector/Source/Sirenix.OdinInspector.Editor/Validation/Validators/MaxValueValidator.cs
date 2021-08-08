#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MaxValueValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.MaxValueValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class MaxValueValidator<T> : AttributeValidator<MaxValueAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> maxValueGetter;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            return IsNumber || IsVector;
        }

        protected override void Initialize()
        {
            this.maxValueGetter = ValueResolver.Get<double>(this.Property, this.Attribute.Expression, this.Attribute.MaxValue);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.maxValueGetter.HasError)
            {
                result.Message = this.maxValueGetter.ErrorMessage;
                result.ResultType = ValidationResultType.Error;
                return;
            }

            var max = this.maxValueGetter.GetValue();

            if (!GenericNumberUtility.NumberIsInRange(this.ValueEntry.SmartValue, double.MinValue, max))
            {
                result.Message = "Number is larger than " + max + ".";
                result.ResultType = ValidationResultType.Error;
            }
        }
    }
}
#endif