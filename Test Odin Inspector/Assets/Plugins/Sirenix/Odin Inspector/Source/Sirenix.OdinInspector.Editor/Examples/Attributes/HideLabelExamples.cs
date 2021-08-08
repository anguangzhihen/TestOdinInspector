#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideLabelExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(HideLabelAttribute))]
    internal class HideLabelExamples
    {
        [Title("Wide Colors")]
        [HideLabel]
        [ColorPalette("Fall")]
        public Color WideColor1;

        [HideLabel]
        [ColorPalette("Fall")]
        public Color WideColor2;

        [Title("Wide Vector")]
        [HideLabel]
        public Vector3 WideVector1;

        [HideLabel]
        public Vector4 WideVector2;

        [Title("Wide String")]
        [HideLabel]
        public string WideString;

        [Title("Wide Multiline Text Field")]
        [HideLabel]
        [MultiLineProperty]
        public string WideMultilineTextField = "";
    }
}
#endif