//-----------------------------------------------------------------------
// <copyright file="OdinSerializeAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Indicates that an instance field or auto-property should be serialized by Odin.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [JetBrains.Annotations.MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OdinSerializeAttribute : Attribute
    {
    }
}