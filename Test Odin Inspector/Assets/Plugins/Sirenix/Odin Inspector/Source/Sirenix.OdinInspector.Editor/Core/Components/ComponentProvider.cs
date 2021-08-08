#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ComponentProvider.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Validation;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public abstract class ComponentProvider
    {
        public abstract PropertyComponent CreateComponent(InspectorProperty property);
    }

    public abstract class PropertyComponent
    {
        public readonly InspectorProperty Property;

        public PropertyComponent(InspectorProperty property)
        {
            this.Property = property;
        }

        public virtual void Reset() { }
    }

    public sealed class ValidationComponentProvider : ComponentProvider
    {
        public IValidatorLocator ValidatorLocator;

        public ValidationComponentProvider()
        {
            this.ValidatorLocator = new DefaultValidatorLocator();
        }

        public ValidationComponentProvider(IValidatorLocator validatorLocator)
        {
            this.ValidatorLocator = validatorLocator;
        }

        public override PropertyComponent CreateComponent(InspectorProperty property)
        {
            return new ValidationComponent(property, this.ValidatorLocator);
        }
    }

    public sealed class ValidationComponent : PropertyComponent, IDisposable
    {
        public readonly IValidatorLocator ValidatorLocator;
        private IList<Validator> validators;

        public ValidationComponent(InspectorProperty property, IValidatorLocator validatorLocator) : base(property)
        {
            this.ValidatorLocator = validatorLocator;
        }

        public void Dispose()
        {
            if (this.validators != null)
            {
                for (int i = 0; i < this.validators.Count; i++)
                {
                    var disposable = this.validators[i] as IDisposable;

                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }   

                this.validators = null;
            }
        }

        public IList<Validator> GetValidators()
        {
            if (this.validators == null)
            {
                if (this.ValidatorLocator.PotentiallyHasValidatorsFor(this.Property))
                {
                    this.validators = this.ValidatorLocator.GetValidators(this.Property);
                }
                else this.validators = new Validator[0];
            }

            return this.validators;
        }

        public override void Reset()
        {
            this.validators = null;
        }

        public void ValidateProperty(ref List<ValidationResult> results)
        {
            if (results == null)
            {
                results = new List<ValidationResult>();
            }

            if (this.validators == null)
            {
                this.GetValidators();
            }

            for (int i = 0; i < this.validators.Count; i++)
            {
                var validator = this.validators[i];

                ValidationResult result = null;

                try
                {
                    validator.RunValidation(ref result);
                }
                catch (Exception ex)
                {
                    while (ex is TargetInvocationException)
                    {
                        ex = ex.InnerException;
                    }

                    result = new ValidationResult()
                    {
                        Message = "Exception was thrown during validation of property " + this.Property.NiceName + ": " + ex.ToString(),
                        ResultType = ValidationResultType.Error,
                        ResultValue = ex,
                        Setup = new ValidationSetup()
                        {
                            Member = this.Property.Info.GetMemberInfo(),
                            ParentInstance = this.Property.ParentValues[0],
                            Root = this.Property.SerializationRoot.ValueEntry.WeakValues[0],
                            Validator = validator,
                            Value = this.Property.ValueEntry == null ? null : this.Property.ValueEntry.WeakSmartValue
                        },
                        Path = this.Property.Path
                    };
                }

                if (result != null && result.ResultType != ValidationResultType.IgnoreResult)
                    results.Add(result);
            }
        }
    }
}
#endif