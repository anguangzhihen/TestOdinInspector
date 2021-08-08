#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInPlayModeExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(HideInPlayModeAttribute))]
    internal class HideInPlayModeExamples
    {
        [Title("Hidden in play mode")]
        [HideInPlayMode]
        public int A;

        [HideInPlayMode]
        public int B;
    }
}
#endif