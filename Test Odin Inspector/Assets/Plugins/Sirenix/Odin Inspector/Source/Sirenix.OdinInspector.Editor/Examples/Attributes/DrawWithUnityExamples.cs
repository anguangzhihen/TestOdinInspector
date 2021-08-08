#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(DrawWithUnityAttribute))]
    internal class DrawWithUnityExamples
    {
        [InfoBox("If you ever experience trouble with one of Odin's attributes, there is a good chance that DrawWithUnity will come in handy; it will make Odin draw the value as Unity normally would.")]
        public GameObject ObjectDrawnWithOdin;

        [DrawWithUnity]
        public GameObject ObjectDrawnWithUnity;
    }
}
#endif