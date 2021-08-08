#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeSearchInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using System;

    public struct TypeSearchInfo
    {
        public Type MatchType;
        public Type[] Targets;
        public double Priority;
    }
}
#endif