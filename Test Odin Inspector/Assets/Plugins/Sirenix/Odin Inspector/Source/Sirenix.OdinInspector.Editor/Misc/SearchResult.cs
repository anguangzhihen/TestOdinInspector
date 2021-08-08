#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SearchResult.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Collections.Generic;

    public class SearchResult
    {
        public InspectorProperty MatchedProperty;
        public List<SearchResult> ChildResults = new List<SearchResult>();
    }
}
#endif