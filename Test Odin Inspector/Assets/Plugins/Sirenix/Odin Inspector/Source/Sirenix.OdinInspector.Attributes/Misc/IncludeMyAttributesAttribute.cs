//-----------------------------------------------------------------------
// <copyright file="IncludeMyAttributesAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// When this attribute is added is added to another attribute, then attributes from that attribute
    /// will also be added to the property in the attribute processing step.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IncludeMyAttributesAttribute : Attribute
    {
    }
}