//-----------------------------------------------------------------------
// <copyright file="SearchFilterOptions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Options for filtering search.
    /// </summary>
    [Flags]
    public enum SearchFilterOptions
    {
        PropertyName = 1 << 0,
        PropertyNiceName = 1 << 1,
        TypeOfValue = 1 << 2,
        ValueToString = 1 << 3,
        ISearchFilterableInterface = 1 << 4,
        All = ~0
    }
}