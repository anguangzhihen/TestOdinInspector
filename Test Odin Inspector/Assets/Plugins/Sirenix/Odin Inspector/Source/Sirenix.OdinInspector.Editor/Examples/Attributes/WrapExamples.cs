#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="WrapExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(WrapAttribute))]
    internal class WrapExamples
    {
        [Wrap(0f, 100f)]
        public int IntWrapFrom0To100;
        
        [Wrap(0f, 100f)]
        public float FloatWrapFrom0To100;
        
        [Wrap(0f, 100f)]
        public Vector3 Vector3WrapFrom0To100;

        [Wrap(0f, 360)]
        public float AngleWrap;

        [Wrap(0f, Mathf.PI * 2)]
        public float RadianWrap;
    }
}
#endif