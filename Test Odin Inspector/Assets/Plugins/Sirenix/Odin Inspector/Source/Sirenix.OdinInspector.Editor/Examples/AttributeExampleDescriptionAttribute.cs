#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AttributeExampleDescriptionAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AttributeExampleDescriptionAttribute : Attribute
    {
        public string Description;

        public AttributeExampleDescriptionAttribute(string description)
        {
            this.Description = description;
        }
    }
}
#endif