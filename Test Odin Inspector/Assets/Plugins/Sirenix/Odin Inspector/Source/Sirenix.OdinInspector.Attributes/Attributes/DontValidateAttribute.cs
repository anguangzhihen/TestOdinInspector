//-----------------------------------------------------------------------
// <copyright file="DontValidateAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Tells the validation system that this member should not be validated. It will not show validation messages in the inspector, and it will not be scanned by the project validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class DontValidateAttribute : Attribute
    {
    }
}