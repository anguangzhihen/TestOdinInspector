#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidationKind.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;

    [Obsolete("There is no longer any strict distinction between value and member validation.", false)]
    public enum ValidationKind
    {
        Value,
        Member
    }
}
#endif