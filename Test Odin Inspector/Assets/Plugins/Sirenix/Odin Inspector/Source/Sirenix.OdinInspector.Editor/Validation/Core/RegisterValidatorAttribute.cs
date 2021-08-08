#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RegisterValidatorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;

    /// <summary>
    /// Apply this to an assembly to register validators for the validation system.
    /// This enables locating of all relevant validator types very quickly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterValidatorAttribute : Attribute
    {
        public readonly Type ValidatorType;
        public int Priority;

        public RegisterValidatorAttribute(Type validatorType)
        {
            this.ValidatorType = validatorType;
        }
    }
}
#endif