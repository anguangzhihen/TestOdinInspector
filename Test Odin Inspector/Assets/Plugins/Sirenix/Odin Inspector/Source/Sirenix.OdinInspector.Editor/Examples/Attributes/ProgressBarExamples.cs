#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ProgressBarExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(ProgressBarAttribute),
        "The ProgressBar attribute draws a horizontal colored bar, which can also be clicked to change the value." +
        "\n\nIt can be used to show how full an inventory might be, or to make a visual indicator for a healthbar. " +
        "It can even be used to make fighting game style health bars, that stack multiple layers of health.")]
    internal sealed class ProgressBarExamples
    {
        [ProgressBar(0, 100)]
        public int ProgressBar = 50;

        [HideLabel]
        [ProgressBar(-100, 100, r: 1, g: 1, b: 1, Height = 30)]
        public short BigColoredProgressBar = 50;

        [ProgressBar(0, 10, 0, 1, 0, Segmented = true)]
        public int SegmentedColoredBar = 5;

        [ProgressBar(0, 100, ColorGetter = "GetHealthBarColor")]
        public float DynamicHealthBarColor = 50;

        // The min and max properties also support attribute expressions with the $ symbol.
        [BoxGroup("Dynamic Range")]
        [ProgressBar("Min", "Max")]
        public float DynamicProgressBar = 50;

        [BoxGroup("Dynamic Range")]
        public float Min;

        [BoxGroup("Dynamic Range")]
        public float Max = 100;

        [Range(0, 300)]
        [BoxGroup("Stacked Health"), HideLabel]
        public float StackedHealth = 150;

        [HideLabel, ShowInInspector]
        [ProgressBar(0, 100, ColorGetter = "GetStackedHealthColor", BackgroundColorGetter = "GetStackHealthBackgroundColor", DrawValueLabel = false)]
        [BoxGroup("Stacked Health")]
        private float StackedHealthProgressBar
        {
            get { return this.StackedHealth % 100.01f; }
        }

        private Color GetHealthBarColor(float value)
        {
            return Color.Lerp(Color.red, Color.green, Mathf.Pow(value / 100f, 2));
        }

        private Color GetStackedHealthColor()
        {
            return
                this.StackedHealth > 200 ? Color.white :
                this.StackedHealth > 100 ? Color.green :
                Color.red;
        }

        private Color GetStackHealthBackgroundColor()
        {
            return
                this.StackedHealth > 200 ? Color.green :
                this.StackedHealth > 100 ? Color.red :
                new Color(0.16f, 0.16f, 0.16f, 1f);
        }
    }
}
#endif