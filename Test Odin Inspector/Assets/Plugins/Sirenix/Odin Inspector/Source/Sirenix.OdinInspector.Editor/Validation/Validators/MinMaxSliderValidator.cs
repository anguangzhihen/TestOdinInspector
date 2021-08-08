#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MinMaxSliderValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.MinMaxSliderValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using UnityEngine;

    public class MinMaxSliderValidator<T> : AttributeValidator<MinMaxSliderAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> minValueGetter;
        private ValueResolver<double> maxValueGetter;
        private ValueResolver<Vector2> rangeGetter;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            return IsNumber || IsVector;
        }

        protected override void Initialize()
        {
            if (this.Attribute.MinMaxValueGetter != null)
            {
                this.rangeGetter = ValueResolver.Get<Vector2>(this.Property, this.Attribute.MinMaxValueGetter, new Vector2(this.Attribute.MinValue, this.Attribute.MaxValue));
            }
            else
            {
                this.minValueGetter = ValueResolver.Get<double>(this.Property, this.Attribute.MinValueGetter, this.Attribute.MinValue);
                this.maxValueGetter = ValueResolver.Get<double>(this.Property, this.Attribute.MaxValueGetter, this.Attribute.MaxValue);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            double min, max;

            if (this.rangeGetter != null)
            {
                if (this.rangeGetter.HasError)
                {
                    result.Message = this.rangeGetter.ErrorMessage;
                    result.ResultType = ValidationResultType.Error;
                    return;
                }
                else
                {
                    var range = this.rangeGetter.GetValue();
                    min = range.x;
                    max = range.y;
                }
            }
            else
            {
                if (this.minValueGetter.HasError || this.maxValueGetter.HasError)
                {
                    result.Message = ValueResolver.GetCombinedErrors(this.minValueGetter, this.maxValueGetter);
                    result.ResultType = ValidationResultType.Error;
                    return;
                }

                min = this.minValueGetter.GetValue();
                max = this.maxValueGetter.GetValue();
            }

            if (!GenericNumberUtility.NumberIsInRange(this.ValueEntry.SmartValue, min, max))
            {
                result.Message = "Number is not in range.";
                result.ResultType = ValidationResultType.Error;
            }
        }
    }
}
#endif