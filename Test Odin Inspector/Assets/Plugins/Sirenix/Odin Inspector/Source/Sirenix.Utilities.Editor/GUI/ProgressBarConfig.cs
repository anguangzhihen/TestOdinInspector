#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ProgressBarConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Configuration for progress bar fields.
    /// </summary>
    public struct ProgressBarConfig
    {
        /// <summary>
        /// The height of the progress bar field. Default 12 pixel.
        /// </summary>
        public int Height;

        /// <summary>
        /// The foreground color of the progress bar field.
        /// </summary>
        public Color ForegroundColor;

        /// <summary>
        /// The background color of the progress bar field.
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// If <c>true</c> the progress bar field will draw a label ontop to show the current value.
        /// </summary>
        public bool DrawValueLabel;

        /// <summary>
        /// Alignment of the progress bar field overlay.
        /// </summary>
        public TextAlignment ValueLabelAlignment;

        /// <summary>
        /// Default configuration.
        /// </summary>
        public static ProgressBarConfig Default
        {
            get
            {
                Color foregroundColor = new Color(0.24f, 0.387f, 0.783f, 1f);
                Color backgroundColor = new Color(0.651f, 0.651f, 0.651f, 1f);

#if UNITY_EDITOR
                if (UnityEditor.EditorGUIUtility.isProSkin)
                {
                    foregroundColor = new Color(0.28f, 0.659f, 0.978f, 1f);
                    backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);
                }
#endif

                return new ProgressBarConfig(
                        12,
                        foregroundColor,
                        backgroundColor,
                        false,
                        TextAlignment.Center);
            }
        }

        /// <summary>
        /// Creates a copy of the configuration.
        /// </summary>
        /// <param name="config">The configuration to copy.</param>
        public ProgressBarConfig(ProgressBarConfig config)
        {
            this.Height = config.Height;
            this.ForegroundColor = config.ForegroundColor;
            this.BackgroundColor = config.BackgroundColor;
            this.DrawValueLabel = config.DrawValueLabel;
            this.ValueLabelAlignment = config.ValueLabelAlignment;
        }

        /// <summary>
        /// Creates a progress bar configuration.
        /// </summary>
        /// <param name="height">The height of the progress bar.</param>
        /// <param name="foregroundColor">The foreground color of the progress bar.</param>
        /// <param name="backgroundColor">The background color of the progress bar.</param>
        /// <param name="textOverlay">If <c>true</c> there will be drawn a overlay on top of the field.</param>
        /// <param name="overlayAlignment">The alignment of the text overlay.</param>
        public ProgressBarConfig(int height, Color foregroundColor, Color backgroundColor, bool textOverlay, TextAlignment overlayAlignment)
        {
            this.Height = height;
            this.ForegroundColor = foregroundColor;
            this.BackgroundColor = backgroundColor;
            this.DrawValueLabel = textOverlay;
            this.ValueLabelAlignment = overlayAlignment;
        }
    }
}
#endif