#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SuppressInvalidAttributeErrorExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(SuppressInvalidAttributeErrorAttribute))]
    internal class SuppressInvalidAttributeErrorExample
    {
        [Range(0, 10)]
        public string InvalidAttributeError = "This field will have an error box for the Range attribute on a string field.";

        [Range(0, 10), SuppressInvalidAttributeError]
        public string SuppressedError = "The error has been suppressed on this field, and thus no error box will appear.";
    }
}
#endif