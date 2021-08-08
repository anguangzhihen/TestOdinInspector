#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValueValidator.cs" company="Sirenix IVS">
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

    public abstract class ValueValidator<TValue> : Validator, DefaultValidatorLocator.IValueValidator_InternalTemporaryHack
    {
        // TODO: Remove this grossness the moment type matching has been fixed to not match value validators to attributes and vice versa
        Type DefaultValidatorLocator.IValueValidator_InternalTemporaryHack.ValidatedType { get { return typeof(TValue); } }

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
                        var legacyMethod = type.GetMethod("Validate", Flags.InstancePrivateDeclaredOnly, null, new Type[] { typeof(TValue), typeof(ValidationResult) }, null);
                        result = legacyMethod != null;
                        ValidatorIsLegacyMap.Add(type, result);
                    }

                    this.isLegacy_backing = result;
                }

                return this.isLegacy_backing.Value;
            }
        }

        public IPropertyValueEntry<TValue> ValueEntry { get; private set; }

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public sealed override bool CanValidateValues()
        {
            return this.IsLegacy;
        }

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public sealed override bool CanValidateMembers()
        {
            return false;
        }

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public sealed override bool CanValidateMember(MemberInfo member, Type memberValueType)
        {
            return false;
        }

        [Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
        public sealed override void Initialize(MemberInfo member, Type memberValueType)
        {
            throw new NotSupportedException("Value validators cannot validate members");
        }

        [Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
        public sealed override void RunMemberValidation(object parentInstance, MemberInfo member, object memberValue, UnityEngine.Object root, ref ValidationResult result)
        {
            throw new NotSupportedException("Value validators cannot validate members");
        }

        [Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
        public sealed override void RunValueValidation(object value, UnityEngine.Object root, ref ValidationResult result)
        {
            this.RunValidation(ref result);
        }

        [Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property and this.ValueEntry for context and value information.", false)]
        protected virtual void Validate(TValue value, ValidationResult result)
        {
            this.Validate(result);
        }

        protected virtual void Validate(ValidationResult result)
        {
            result.ResultType = ValidationResultType.Warning;
            result.Message = "Validation logic for " + this.GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
        }

        public sealed override void RunValidation(ref ValidationResult result)
        {
            if (result == null)
                result = new ValidationResult();

            result.Setup = new ValidationSetup()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Kind = ValidationKind.Value,
#pragma warning restore CS0618 // Type or member is obsolete
                Validator = this,
                Value = this.Property.ValueEntry.WeakSmartValue,
                Root = this.Property.SerializationRoot.ValueEntry.WeakValues[0] as UnityEngine.Object,
            };

            result.Path = this.Property.Path;

            this.ValueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;

            if (this.ValueEntry == null)
            {
                result.ResultValue = null;
                result.ResultType = ValidationResultType.Error;
                result.Message = "Property " + this.Property.NiceName + " did not have validator " + this.GetType().GetNiceName() + "'s expected value entry of type '" + typeof(TValue).GetNiceName() + "' on it, but instead a value entry of type '" + this.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
                return;
            }

            result.ResultValue = null;
            result.ResultType = ValidationResultType.Valid;
            result.Message = "";

            try
            {
                if (this.IsLegacy)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    //UnityEngine.Debug.LogError(this.GetType().GetNiceName() + " is still legacy!");
                    this.Validate((TValue)this.Property.ValueEntry.WeakSmartValue, result);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
                    //UnityEngine.Debug.Log(this.GetType().GetNiceName() + " is no longer legacy!");
                    this.Validate(result);
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
    }
}
#endif