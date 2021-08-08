#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DoesNotSupportPrefabModificationsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    internal sealed class DoesNotSupportPrefabModificationsAttribute : Attribute
    {
    }
}
#endif