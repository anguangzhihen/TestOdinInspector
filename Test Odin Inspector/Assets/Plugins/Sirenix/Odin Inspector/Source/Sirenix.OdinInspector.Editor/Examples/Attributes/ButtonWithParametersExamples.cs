#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ButtonWithParametersExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(ButtonAttribute),
        Name = "Parameters Examples",
        Description = "You can also use the Button attribute on any method with parameters.\n" +
        "This will draw a form in the inspector that lets you fill out the parameters that gets passed to the method when the button is pressed.")]
    internal class ButtonWithParametersExamples
    {
        [Button]
        private void Default(float a, float b, GameObject c)
        {
        }

        [Button]
        private void Default(float t, float b, float[] c)
        {
        }

        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        private int FoldoutButton(int a = 2, int b = 2)
        {
            return a + b;
        }

        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        private void FoldoutButton(int a, int b, ref int result)
        {
            result = a + b;
        }

        [Button(ButtonStyle.Box)]
        private void Full(float a, float b, out float c)
        {
            c = a + b;
        }

        [Button(ButtonSizes.Large, ButtonStyle.Box)]
        private void Full(int a, float b, out float c)
        {
            c = a + b;
        }

        [Button(ButtonStyle.CompactBox, Expanded = true)]
        private void CompactExpanded(float a, float b, GameObject c)
        {
        }

        [Button(ButtonSizes.Medium, ButtonStyle.Box, Expanded = true)]
        private void FullExpanded(float a, float b)
        {
        }
    }
}
#endif