#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NamedValueGetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    public delegate object NamedValueGetter(ref ActionResolverContext context, int selectionIndex);
}
#endif