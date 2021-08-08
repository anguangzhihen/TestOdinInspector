#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnumToggleButtonsExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(EnumToggleButtonsAttribute))]
    internal class EnumToggleButtonsExamples
    {
        [Title("Default")]
        public SomeBitmaskEnum DefaultEnumBitmask;

        [Title("Standard Enum")]
        [EnumToggleButtons]
        public SomeEnum SomeEnumField;

        [EnumToggleButtons, HideLabel]
        public SomeEnum WideEnumField;

        [Title("Bitmask Enum")]
        [EnumToggleButtons]
        public SomeBitmaskEnum BitmaskEnumField;

        [EnumToggleButtons, HideLabel]
        public SomeBitmaskEnum EnumFieldWide;

        public enum SomeEnum
        {
            First, Second, Third, Fourth, AndSoOn
        }

        [System.Flags]
        public enum SomeBitmaskEnum
        {
            A = 1 << 1,
            B = 1 << 2,
            C = 1 << 3,
            All = A | B | C
        }
    }
}
#endif