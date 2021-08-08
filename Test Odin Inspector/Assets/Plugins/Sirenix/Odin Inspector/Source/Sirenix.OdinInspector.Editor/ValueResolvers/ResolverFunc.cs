#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ResolverFunc.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    using System;

    public delegate TResult ValueResolverFunc<TResult>(ref ValueResolverContext context, int selectionIndex);
}
#endif