#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ResolvedAction.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    public delegate void ResolvedAction(ref ActionResolverContext context, int selectionIndex);
}
#endif