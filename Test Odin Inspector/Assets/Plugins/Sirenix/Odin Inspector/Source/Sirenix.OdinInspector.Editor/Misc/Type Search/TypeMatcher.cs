#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeMatcher.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using System;

    public abstract class TypeMatcher
    {
        public abstract string Name { get; }
        public abstract Type Match(Type[] targets, ref bool stopMatching);
    }
}
#endif