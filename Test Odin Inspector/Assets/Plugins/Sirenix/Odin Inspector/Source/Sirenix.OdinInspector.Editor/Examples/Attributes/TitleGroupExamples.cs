#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TitleGroupExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(TitleGroupAttribute))]
    internal class TitleGroupExamples
    {
        [TitleGroup("Ints")]
        public int SomeInt1;

        [TitleGroup("$SomeString1", "Optional subtitle")]
        public string SomeString1;

        [TitleGroup("Vectors", "Optional subtitle", alignment: TitleAlignments.Centered, horizontalLine: true, boldTitle: true, indent: false)]
        public Vector2 SomeVector1;

        [TitleGroup("Ints","Optional subtitle", alignment: TitleAlignments.Split)]
        public int SomeInt2;

        [TitleGroup("$SomeString1", "Optional subtitle")]
        public string SomeString2;

        [TitleGroup("Vectors")]
        public Vector2 SomeVector2 { get; set; }
        
        [TitleGroup("Ints/Buttons", indent: false)]
        private void IntButton() { }

        [TitleGroup("$SomeString1/Buttons", indent: false)]
        private void StringButton() { }

        [TitleGroup("Vectors")]
        private void VectorButton() { }
    }
}
#endif