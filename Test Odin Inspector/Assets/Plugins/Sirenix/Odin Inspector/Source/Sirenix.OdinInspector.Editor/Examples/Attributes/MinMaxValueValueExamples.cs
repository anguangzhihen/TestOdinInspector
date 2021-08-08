#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MinMaxValueValueExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(MinValueAttribute))]
    [AttributeExample(typeof(MaxValueAttribute))]
    internal class MinMaxValueValueExamples
    {
        // Ints
        [Title("Int")]
        [MinValue(0)]
        public int IntMinValue0;

        [MaxValue(0)]
        public int IntMaxValue0;

        // Floats
        [Title("Float")]
        [MinValue(0)]
        public float FloatMinValue0;

        [MaxValue(0)]
        public float FloatMaxValue0;

        // Vectors
        [Title("Vectors")]
        [MinValue(0)]
        public Vector3 Vector3MinValue0;

        [MaxValue(0)]
        public Vector3 Vector3MaxValue0;
    }
}
#endif