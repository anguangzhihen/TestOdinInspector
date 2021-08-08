//-----------------------------------------------------------------------
// <copyright file="DisableInInlineEditorsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Disables a property if it is drawn within an <see cref="InlineEditorAttribute"/>.
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class DisableInInlineEditorsAttribute : Attribute
    {
    }
}