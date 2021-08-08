#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RegisterDefaultActionResolverAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterDefaultActionResolverAttribute : Attribute
    {
        public Type ResolverType;
        public double Order;

        public RegisterDefaultActionResolverAttribute(Type resolverType, double order)
        {
            this.ResolverType = resolverType;
            this.Order = order;
        }
    }
}
#endif