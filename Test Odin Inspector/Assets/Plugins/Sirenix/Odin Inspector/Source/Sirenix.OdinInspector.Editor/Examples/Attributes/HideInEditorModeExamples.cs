#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInEditorModeExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(HideInEditorModeAttribute))]
    internal class HideInEditorModeExamples
    {
        [Title("Hidden in editor mode")]
        [HideInEditorMode]
        public int C;

        [HideInEditorMode]
        public int D;
    }
}
#endif