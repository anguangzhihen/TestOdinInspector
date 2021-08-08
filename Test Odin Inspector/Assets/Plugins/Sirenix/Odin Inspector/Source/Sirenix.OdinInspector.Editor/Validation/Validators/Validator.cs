#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Validator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;
    using System.Reflection;

    public abstract class Validator
    {
        public InspectorProperty Property { get; private set; }

        public virtual RevalidationCriteria RevalidationCriteria { get { return RevalidationCriteria.Always; } }

        public void Initialize(InspectorProperty property)
        {
            this.Property = property;

            this.Initialize();
            
            // Backwards compatibility

#pragma warning disable CS0618 // Type or member is obsolete
            if (this.CanValidateMembers())
            {
                if (property.Info.HasBackingMembers)
                {
                    if (property.ValueEntry == null)
                        throw new NoBackwardsCompatibilityForLegacyValidatorException();

                    this.Initialize(property.Info.GetMemberInfo(), property.ValueEntry.TypeOfValue);
                }
                else
                {
                    var memberProp = property.FindParent(p => p.Info.GetMemberInfo() != null, true);

                    if (memberProp != null)
                    {
                        if (memberProp.ValueEntry == null)
                            throw new NoBackwardsCompatibilityForLegacyValidatorException();

                        this.Initialize(memberProp.Info.GetMemberInfo(), memberProp.ValueEntry.TypeOfValue);
                    }
                    else
                    {
                        throw new NoBackwardsCompatibilityForLegacyValidatorException();
                    }
                }
            }
            
            if (this.CanValidateValues())
            {
                if (property.ValueEntry == null)
                    throw new NoBackwardsCompatibilityForLegacyValidatorException();

                this.Initialize(property.ValueEntry.TypeOfValue);
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public virtual bool CanValidateProperty(InspectorProperty property)
        {
            return true;
        }

        protected virtual void Initialize()
        {
        }
        
        public virtual void RunValidation(ref ValidationResult result)
        {
        }

        #region Backwards compatibility

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public virtual bool CanValidateValues()
        {
            return false;
        }

        [Obsolete("Use CanValidateProperty(InspectorProperty property) instead. There is no longer a distinction between value and member validators; instead properties are validated.", false)]
        public virtual bool CanValidateValue(Type type)
        {
            return true;
        }

        [Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
        public virtual bool CanValidateMembers()
        {
            return false;
        }

        [Obsolete("Use CanValidateProperty(InspectorProperty property) instead. There is no longer a distinction between value and member validators; instead properties are validated.", false)]
        public virtual bool CanValidateMember(MemberInfo member, Type memberValueType)
        {
            return true;
        }

        [Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
        public virtual void RunValueValidation(object value, UnityEngine.Object root, ref ValidationResult result)
        {
        }

        [Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
        public virtual void RunMemberValidation(object parentInstance, MemberInfo member, object memberValue, UnityEngine.Object root, ref ValidationResult result)
        {
        }

        [Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
        public virtual void Initialize(Type type)
        {
        }

        [Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
        public virtual void Initialize(MemberInfo member, Type memberValueType)
        {
        }

        #endregion Backwards compatibility
    }
}
#endif