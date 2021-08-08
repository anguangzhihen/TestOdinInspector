#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OmitFromPrefabModificationPathsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class OmitFromPrefabModificationPathsAttribute : Attribute
    {
    }
}
#endif