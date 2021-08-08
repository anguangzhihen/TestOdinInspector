#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ButtonsInBoxesExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(BoxGroupAttribute), Order = 10)]
    [AttributeExample(typeof(FoldoutGroupAttribute), Order = 10)]
    [AttributeExample(typeof(HorizontalGroupAttribute), Order = 10)]
    internal class ButtonsInBoxesExample
    {
        [Button(ButtonSizes.Large)]
        [FoldoutGroup("Buttons in Boxes")]
        [HorizontalGroup("Buttons in Boxes/Horizontal")]
        [BoxGroup("Buttons in Boxes/Horizontal/One")]
        public void Button1() { }

        [Button(ButtonSizes.Large)]
        [BoxGroup("Buttons in Boxes/Horizontal/Two")]
        public void Button2() { }

        [Button]
        [HorizontalGroup("Buttons in Boxes/Horizontal", Width = 60)]
        [BoxGroup("Buttons in Boxes/Horizontal/Double")]
        public void Accept() { }

        [Button]
        [BoxGroup("Buttons in Boxes/Horizontal/Double")]
        public void Cancel() { }
    }
}
#endif