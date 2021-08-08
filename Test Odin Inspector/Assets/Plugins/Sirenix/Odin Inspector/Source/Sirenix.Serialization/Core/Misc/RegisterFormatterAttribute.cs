//-----------------------------------------------------------------------
// <copyright file="RegisterFormatterAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterFormatterAttribute : Attribute
    {
        public Type FormatterType { get; private set; }
        public int Priority { get; private set; }

        public RegisterFormatterAttribute(Type formatterType, int priority = 0)
        {
            this.FormatterType = formatterType;
            this.Priority = priority;
        }
    }
}