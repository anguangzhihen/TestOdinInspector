//-----------------------------------------------------------------------
// <copyright file="RegisterFormatterLocatorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterFormatterLocatorAttribute : Attribute
    {
        public Type FormatterLocatorType { get; private set; }
        public int Priority { get; private set; }

        public RegisterFormatterLocatorAttribute(Type formatterLocatorType, int priority = 0)
        {
            this.FormatterLocatorType = formatterLocatorType;
            this.Priority = priority;
        }
    }
}