#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorPaletteExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(ColorPaletteAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Linq", "System.Collections.Generic" })]
    internal class ColorPaletteExamples
    {
        [ColorPalette]
        public Color ColorOptions;

        [ColorPalette("Underwater")]
        public Color UnderwaterColor;

        [ColorPalette("My Palette")]
        public Color MyColor;

        public string DynamicPaletteName = "Clovers";

        // The ColorPalette attribute supports both 
        // member references and attribute expressions.
        [ColorPalette("$DynamicPaletteName")]
        public Color DynamicPaletteColor;

        [ColorPalette("Fall"), HideLabel]
        public Color WideColorPalette;

        [ColorPalette("Clovers")]
        public Color[] ColorArray;

        // ------------------------------------
        // Color palettes can be accessed and modified from code.
        // Note that the color palettes will NOT automatically be included in your builds.
        // But you can easily fetch all color palettes via the ColorPaletteManager 
        // and include them in your game like so:
        // ------------------------------------

        [FoldoutGroup("Color Palettes", expanded: false)]
        [ListDrawerSettings(IsReadOnly = true)]
        [PropertyOrder(9)]
        public List<ColorPalette> ColorPalettes;

        [Serializable]
        public class ColorPalette
        {
            [HideInInspector]
            public string Name;

            [LabelText("$Name")]
            [ListDrawerSettings(IsReadOnly = true, Expanded = false)]
            public Color[] Colors;
        }

#if UNITY_EDITOR
        [FoldoutGroup("Color Palettes"), Button(ButtonSizes.Large), GUIColor(0, 1, 0), PropertyOrder(8)]
        private void FetchColorPalettes()
        {
            this.ColorPalettes = Sirenix.OdinInspector.Editor.ColorPaletteManager.Instance.ColorPalettes
                .Select(x => new ColorPalette()
                {
                    Name = x.Name,
                    Colors = x.Colors.ToArray()
                })
                .ToList();
        }
#endif
    }
}
#endif