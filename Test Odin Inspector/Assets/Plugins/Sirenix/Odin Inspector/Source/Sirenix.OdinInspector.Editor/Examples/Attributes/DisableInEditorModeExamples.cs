#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableInEditorModeExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(DisableInEditorModeAttribute))]
    internal class DisableInEditorModeExamples
    {
        [Title("Disabled in edit mode")]
        [DisableInEditorMode]
        public GameObject A;

        [DisableInEditorMode]
        public Material B;
    }
}
#endif