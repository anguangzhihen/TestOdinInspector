#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorPalette.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A color palette.
    /// </summary>
    [Serializable]
    public class ColorPalette
    {
        [SerializeField, PropertyOrder(0)]
        private string name;

        [SerializeField]
        private bool showAlpha = false;

        [SerializeField, PropertyOrder(3)]
        [ListDrawerSettings(Expanded = true, DraggableItems = true, ShowPaging = false, ShowItemCount = true)]
        private List<Color> colors = new List<Color>();

        /// <summary>
        /// Name of the color palette.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// The colors.
        /// </summary>
        public List<Color> Colors
        {
            get { return this.colors; }
            set { this.colors = value; }
        }

        /// <summary>
        /// Whether to show the alpha channel.
        /// </summary>
        public bool ShowAlpha
        {
            get { return this.showAlpha; }
            set { this.showAlpha = value; }
        }
    }
}
#endif