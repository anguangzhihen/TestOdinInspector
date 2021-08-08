#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RegisterStateUpdaterAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class RegisterStateUpdaterAttribute : Attribute
    {
        public readonly Type Type;
        public readonly double Priority;

        public RegisterStateUpdaterAttribute(Type type, double priority = 0)
        {
            this.Type = type;
            this.Priority = priority;
        }
    }
}
#endif