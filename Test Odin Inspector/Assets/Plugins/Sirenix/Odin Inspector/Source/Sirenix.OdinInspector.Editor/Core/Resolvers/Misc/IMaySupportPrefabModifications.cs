#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IMaySupportPrefabModifications.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Note: this interface may be temporary, and may eventually be substituted for a public-facing way of extending the prefab modification system.
    /// <para/>
    /// For now, it only exists to denote which internally defined resolvers support prefab modifications being set.
    /// </summary>
    internal interface IMaySupportPrefabModifications
    {
        bool MaySupportPrefabModifications { get; }
    }
}
#endif