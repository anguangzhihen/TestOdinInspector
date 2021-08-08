#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeSearchResult.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using System;

    public struct TypeSearchResult
    {
        public TypeSearchInfo MatchedInfo;
        public Type MatchedType;
        public Type[] MatchedTargets;
        public TypeMatcher MatchedMatcher;
        public TypeMatchRule MatchedRule;
        public TypeSearchIndex MatchedIndex;
    }
}
#endif