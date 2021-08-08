#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NoBackwardsCompatibilityForLegacyValidatorException.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;

    internal class NoBackwardsCompatibilityForLegacyValidatorException : Exception
    {
    }
}
#endif