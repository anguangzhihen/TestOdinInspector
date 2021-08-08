#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidatorExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;

    public static class ValidatorExtensions
    {
        public static ValidationResultType ToValidationResultType(this InfoMessageType messageType)
        {
            if (messageType == InfoMessageType.Error)
                return ValidationResultType.Error;
            else if (messageType == InfoMessageType.Warning)
                return ValidationResultType.Warning;
            else return ValidationResultType.Valid;
        }
    }
}
#endif