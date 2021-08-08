#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MultipleStackedBoxesExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(BoxGroupAttribute), Order = 10)]
    [AttributeExample(typeof(HorizontalGroupAttribute), Order = 10)]
    [AttributeExample(typeof(TitleGroupAttribute), Order = 10)]
    [AttributeExample(typeof(VerticalGroupAttribute), Order = 10)]
    internal class MultipleStackedBoxesExample
    {
        [TitleGroup("Multiple Stacked Boxes")]
        [HorizontalGroup("Multiple Stacked Boxes/Split")]
        [VerticalGroup("Multiple Stacked Boxes/Split/Left")]
        [BoxGroup("Multiple Stacked Boxes/Split/Left/Box A")]
        public int BoxA;

        [BoxGroup("Multiple Stacked Boxes/Split/Left/Box B")]
        public int BoxB;

        [VerticalGroup("Multiple Stacked Boxes/Split/Right")]
        [BoxGroup("Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxC, BoxD, BoxE;
    }
}
#endif