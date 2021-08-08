#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BigTabGroupExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(ResponsiveButtonGroupAttribute), Order = 10)]
    [AttributeExample(typeof(TabGroupAttribute), Order = 10)]
    internal class BigTabGroupExample
    {
        [TitleGroup("Tabs")]
        [HorizontalGroup("Tabs/Split", Width = 0.5f)]
        [TabGroup("Tabs/Split/Parameters", "A")]
        public string NameA, NameB, NameC;

        [TabGroup("Tabs/Split/Parameters", "B")]
        public int ValueA, ValueB, ValueC;

        [TabGroup("Tabs/Split/Buttons", "Responsive")]
        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Hello() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void World() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void And() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Such() { }

        [Button]
        [TabGroup("Tabs/Split/Buttons", "More Tabs")]
        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        public void SubButtonA() { }

        [Button]
        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        public void SubButtonB() { }

        [Button(ButtonSizes.Gigantic)]
        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "B")]
        public void SubButtonC() { }
    }
}
#endif