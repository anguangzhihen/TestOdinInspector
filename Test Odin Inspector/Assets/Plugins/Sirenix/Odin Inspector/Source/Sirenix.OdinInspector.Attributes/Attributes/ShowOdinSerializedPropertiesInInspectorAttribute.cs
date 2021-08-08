//-----------------------------------------------------------------------
// <copyright file="ShowOdinSerializedPropertiesInInspectorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Marks a type as being specially serialized. Odin uses this attribute to check whether it should include non-Unity-serialized members in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowOdinSerializedPropertiesInInspectorAttribute : Attribute
    {
    }
}