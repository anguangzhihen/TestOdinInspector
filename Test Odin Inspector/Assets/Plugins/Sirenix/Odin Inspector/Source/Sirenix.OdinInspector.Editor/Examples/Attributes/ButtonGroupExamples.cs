#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ButtonGroupExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(ButtonGroupAttribute))]
    internal class ButtonGroupExamples
    {
        [ButtonGroup]
        private void A()
        {
        }

        [ButtonGroup]
        private void B()
        {
        }

        [ButtonGroup]
        private void C()
        {
        }

        [ButtonGroup]
        private void D()
        {
        }

        [Button(ButtonSizes.Large)]
        [ButtonGroup("My Button Group")]
        private void E()
        {
        }

        [GUIColor(0, 1, 0)]
        [ButtonGroup("My Button Group")]
        private void F()
        {
        }
    }
}
#endif