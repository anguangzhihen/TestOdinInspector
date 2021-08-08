#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InlineButtonExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(InlineButtonAttribute))]
    internal class InlineButtonExamples
    {
        // Inline Buttons:
        [InlineButton("A")]
        public int InlineButton;

        [InlineButton("A")]
        [InlineButton("B", "Custom Button Name")]
        public int ChainedButtons;

        private void A()
        {
            Debug.Log("A");
        }

        private void B()
        {
            Debug.Log("B");
        }
    }
}
#endif