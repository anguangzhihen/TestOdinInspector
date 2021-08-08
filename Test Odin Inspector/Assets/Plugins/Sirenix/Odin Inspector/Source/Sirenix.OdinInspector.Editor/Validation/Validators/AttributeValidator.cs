#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AttributeValidator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal interface IAttributeValidator
    {
        void SetAttributeInstance(Attribute attribute);
    }

    internal interface IAttributeValueValidator
    {
        Type GetValueType();
        IPropertyValueEntry RefreshValueEntry();
        bool IsLegacy { get; }
    }

    public abstract class AttributeValidator<TAttribute> : Validator, IAttributeValidator
        where TAttribute : Attribute
    {
        private static readonly Dictionary<Type, bool> ValidatorIsLegacyMap = new Dictionary<Type, bool>(FastTypeComparer.Instance);

        private bool? isValueValidator_backing;

        private bool IsValueValidator
        {
            get
            {
                if (!this.isValueValidator_backing.HasValue)
                {
                    this.isValueValidator_backing = this is IAttributeValueValidator;
                }

                return this.isValueValidator_backing.Value;
            }
        }

        private bool? isLegacy_backing;

        private bool IsLegacy
        {
            get
            {
                if (!this.isLegacy_backing.HasValue)
                {
                    if (this.IsValueValidator)
                    {
                        this.isLegacy_backing = (this as IAttributeValueValidator).IsLegacy;
                    }
                    else
                    {
                        bool result;
                        var type = this.GetType();

                        if (!ValidatorIsLegacyMap.TryGetValue(type, out result))
                        {
                            var legacyMethod = type.GetMethod("Validate", Flags.InstancePrivateDeclaredOnly, null, new Type[] { typeof(object), typeof(object), typeof(MemberInfo), typeof(ValidationResult) }, null);
                            result = legacyMethod != null;
                            ValidatorIsLegacyMap.Add(type, result);
                        }

                        this.isLegacy_backing = result;
                    }
                }

                return this.isLegacy_backing.Value;
            }
        }

        public TAttribute Attribute { get; private set; }

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public sealed override bool CanValidateMembers()
        {
            return this.IsLegacy;
        }

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public sealed override bool CanValidateValues()
        {
            return false;
        }

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public sealed override bool CanValidateValue(Type type)
        {
            return false;
        }

        [Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
        public sealed override void Initialize(Type type)
        {
            throw new NotSupportedException("Attribute validators cannot validate values without members");
        }

        [Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
        public sealed override void RunValueValidation(object value, UnityEngine.Object root, ref ValidationResult result)
        {
            throw new NotSupportedException("Attribute validators cannot validate values without members");
        }

        [Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
        public sealed override void RunMemberValidation(object parentInstance, MemberInfo member, object memberValue, UnityEngine.Object root, ref ValidationResult result)
        {
            this.RunValidation(ref result);
        }

        public sealed override void RunValidation(ref ValidationResult result)
        {
            if (result == null)
                result = new ValidationResult();

            result.Setup = new ValidationSetup()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Kind = ValidationKind.Member,
#pragma warning restore CS0618 // Type or member is obsolete

                Validator = this,
                Member = this.Property.Info.GetMemberInfo(),
                ParentInstance = this.Property.ParentValues[0],
                Value = this.Property.ValueEntry == null ? null : this.Property.ValueEntry.WeakSmartValue,
                Root = this.Property.SerializationRoot.ValueEntry.WeakValues[0],
            };

            result.Path = this.Property.Path;

            if (result.Setup.Member == null)
            {
                var memberProp = this.Property.FindParent(p => p.Info.GetMemberInfo() != null, false);

                if (memberProp != null)
                    result.Setup.Member = memberProp.Info.GetMemberInfo();
            }

            bool isValueValidator = this.IsValueValidator;

            if (isValueValidator)
            {
                var entry = (this as IAttributeValueValidator).RefreshValueEntry();

                if (entry == null)
                {
                    result.ResultValue = null;
                    result.ResultType = ValidationResultType.Error;
                    result.Message = "Property " + this.Property.NiceName + " did not have validator " + this.GetType().GetNiceName() + "'s expected value entry of type '" + (this as IAttributeValueValidator).GetValueType().GetNiceName() + "' on it, but instead a value entry of type '" + this.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
                }
            }

            result.ResultValue = null;
            result.ResultType = ValidationResultType.Valid;
            result.Message = "";
            
            try
            {
                if (isValueValidator || !this.IsLegacy)
                {
                    this.Validate(result);
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    this.Validate(result.Setup.ParentInstance, result.Setup.Value, result.Setup.Member, result);
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
            catch (Exception ex)
            {
                while (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                result.ResultType = ValidationResultType.Error;
                result.Message = "An exception was thrown during validation: " + ex.ToString();
            }
        }

        protected virtual void Validate(ValidationResult result)
        {
            result.ResultType = ValidationResultType.Warning;
            result.Message = "Validation logic for " + this.GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
        }

        [Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
        protected virtual void Validate(object parentInstance, object memberValue, MemberInfo member, ValidationResult result)
        {
            this.Validate(result);
        }

        void IAttributeValidator.SetAttributeInstance(Attribute attribute)
        {
            this.Attribute = (TAttribute)attribute;
        }
    }

    public abstract class AttributeValidator<TAttribute, TValue> : AttributeValidator<TAttribute>, IAttributeValueValidator
        where TAttribute : Attribute
    {
        private static readonly Dictionary<Type, bool> ValidatorIsLegacyMap = new Dictionary<Type, bool>(FastTypeComparer.Instance);

        private bool? isLegacy_backing;

        private bool IsLegacy
        {
            get
            {
                if (!this.isLegacy_backing.HasValue)
                {
                    bool result;
                    var type = this.GetType();

                    if (!ValidatorIsLegacyMap.TryGetValue(type, out result))
                    {
                        var legacyMethod = type.GetMethod("Validate", Flags.InstancePrivateDeclaredOnly, null, new Type[] { typeof(object), typeof(TValue), typeof(MemberInfo), typeof(ValidationResult) }, null);
                        result = legacyMethod != null;
                        ValidatorIsLegacyMap.Add(type, result);
                    }

                    this.isLegacy_backing = result;
                }

                return this.isLegacy_backing.Value;
            }
        }

        public IPropertyValueEntry<TValue> ValueEntry { get; private set; }

        bool IAttributeValueValidator.IsLegacy { get { return this.IsLegacy; } }

        Type IAttributeValueValidator.GetValueType()
        {
            return typeof(TValue);
        }

        IPropertyValueEntry IAttributeValueValidator.RefreshValueEntry()
        {
            return (this.ValueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.IsLegacy)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                this.Validate(result.Setup.ParentInstance, result.Setup.Value, result.Setup.Member, result);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                result.ResultType = ValidationResultType.Warning;
                result.Message = "Validation logic for " + this.GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
            }
        }

        [Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property and this.ValueEntry for context and value information.", false)]
        protected sealed override void Validate(object parentInstance, object memberValue, MemberInfo member, ValidationResult result)
        {
            this.Validate(parentInstance, (TValue)memberValue, member, result);
        }

        [Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property and this.ValueEntry for context and value information.", false)]
        protected virtual void Validate(object parentInstance, TValue memberValue, MemberInfo member, ValidationResult result)
        {
            result.ResultType = ValidationResultType.Warning;
            result.Message = "Validation logic for " + this.GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
        }
    }
}
#endif