#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MultiLinePropertyExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(MultilineAttribute))]
    [AttributeExample(typeof(MultiLinePropertyAttribute))]
    internal class MultiLinePropertyExamples
    {
        [Multiline(10)]
        public string UnityMultilineField = "";

        [Title("Wide Multiline Text Field", bold: false)]
        [HideLabel]
        [MultiLineProperty(10)]
        public string WideMultilineTextField = "";

        [InfoBox("Odin supports properties, but Unity's own Multiline attribute only works on fields.")]
        [ShowInInspector]
        [MultiLineProperty(10)]
        public string OdinMultilineProperty { get; set; }
    }
}
#endif