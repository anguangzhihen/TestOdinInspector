#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuStyle.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using System.Globalization;
    using UnityEngine;

    /// <summary>
    /// <para>The style settings used by <see cref="OdinMenuItem"/>.</para>
    /// <para>
    /// A nice trick to style your menu is to add the tree.DefaultMenuStyle to the tree itself,
    /// and style it live. Once you are happy, you can hit the Copy CSharp Snippet button,
    /// remove the style from the menu tree, and paste the style directly into your code.
    /// </para>
    /// </summary>
    /// <seealso cref="OdinMenuTree" />
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuStyle
    {
        private GUIStyle defaultLabelStyle;
        private GUIStyle selectedLabelStyle;

        /// <summary>
        /// Gets or sets the default selected style.
        /// </summary>
        public GUIStyle DefaultLabelStyle
        {
            get
            {
                if (this.defaultLabelStyle == null)
                {
                    this.defaultLabelStyle = SirenixGUIStyles.Label;
                }

                return this.defaultLabelStyle;
            }
            set { this.defaultLabelStyle = value; }
        }

        /// <summary>
        /// Gets or sets the selected label style.
        /// </summary>
        public GUIStyle SelectedLabelStyle
        {
            get
            {
                if (this.selectedLabelStyle == null)
                {
                    this.selectedLabelStyle = SirenixGUIStyles.WhiteLabel;
                }

                return this.selectedLabelStyle;
            }
            set { this.selectedLabelStyle = value; }
        }

        /// <summary>
        /// The height of the menu item.
        /// </summary>
        [BoxGroup("General")]
        public int Height = 30;

        /// <summary>
        /// The global offset of the menu item content
        /// </summary>
        [BoxGroup("General")]
        public float Offset = 16;

        /// <summary>
        /// The vertical offset of the menu item label
        /// </summary>
        [BoxGroup("General")]
        public float LabelVerticalOffset = 0;

        /// <summary>
        /// The number of pixels to indent per level indent level.
        /// </summary>
        [BoxGroup("General")]
        public float IndentAmount = 15;

        /// <summary>
        /// The size of the icon.
        /// </summary>
        [BoxGroup("Icons")]
        public float IconSize = 16;

        /// <summary>
        /// The size of the icon.
        /// </summary>
        [BoxGroup("Icons")]
        public float IconOffset = 0;

        /// <summary>
        /// The transparency of icons when the menu item is not selected.
        /// </summary>
        [BoxGroup("Icons"), Range(0, 1)]
        public float NotSelectedIconAlpha = 0.85f;

        /// <summary>
        /// The padding between the icon and other content.
        /// </summary>
        [BoxGroup("Icons")]
        public float IconPadding = 3;

        /// <summary>
        /// Whether to draw the a foldout triangle for menu items with children.
        /// </summary>
        [BoxGroup("Triangle")]
        public bool DrawFoldoutTriangle = true;

        /// <summary>
        /// The size of the foldout triangle icon.
        /// </summary>
        [BoxGroup("Triangle")]
        public float TriangleSize = 17;

        /// <summary>
        /// The padding between the foldout triangle icon and other content.
        /// </summary>
        [BoxGroup("Triangle")]
        public float TrianglePadding = 8;

        /// <summary>
        /// Whether or not to align the triangle left or right of the content.
        /// If right, then the icon is pushed all the way to the right at a fixed position ignoring the indent level.
        /// </summary>
        [BoxGroup("Triangle")]
        public bool AlignTriangleLeft = false;

        /// <summary>
        /// Whether to draw borders between menu items.
        /// </summary>
        [BoxGroup("Borders")]
        public bool Borders = true;

        /// <summary>
        /// The horizontal border padding.
        /// </summary>
        [BoxGroup("Borders"), EnableIf("Borders")]
        public float BorderPadding = 13;

        /// <summary>
        /// The border alpha.
        /// </summary>
        [BoxGroup("Borders"), EnableIf("Borders"), Range(0, 1)]
        public float BorderAlpha = 0.5f;

        /// <summary>
        /// The background color for when a menu item is selected.
        /// </summary>
        [BoxGroup("Colors")]
        public Color SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f);

        /// <summary>
        /// The background color for when a menu item is selected.
        /// </summary>
        [BoxGroup("Colors")]
        public Color SelectedInactiveColorDarkSkin = new Color(0.838f, 0.838f, 0.838f, 0.134f);

        /// <summary>
        /// The background color for when a menu item is selected.
        /// </summary>
        [BoxGroup("Colors")]
        public Color SelectedColorLightSkin = new Color(0.243f, 0.49f, 0.9f, 1.000f);

        /// <summary>
        /// The background color for when a menu item is selected.
        /// </summary>
        [BoxGroup("Colors")]
        public Color SelectedInactiveColorLightSkin = new Color(0.5f, 0.5f, 0.5f, 1.000f);

        /// <summary>
        /// Sets the height of the menu item.
        /// </summary>
        public OdinMenuStyle SetHeight(int value)
        {
            this.Height = value;
            return this;
        }

        /// <summary>
        /// Sets the global offset of the menu item content
        /// </summary>
        public OdinMenuStyle SetOffset(float value)
        {
            this.Offset = value;
            return this;
        }

        /// <summary>
        /// Sets the number of pixels to indent per level indent level.
        /// </summary>
        public OdinMenuStyle SetIndentAmount(float value)
        {
            this.IndentAmount = value;
            return this;
        }

        /// <summary>
        /// Sets the size of the icon.
        /// </summary>
        public OdinMenuStyle SetIconSize(float value)
        {
            this.IconSize = value;
            return this;
        }

        /// <summary>
        /// Sets the size of the icon.
        /// </summary>
        public OdinMenuStyle SetIconOffset(float value)
        {
            this.IconOffset = value;
            return this;
        }

        /// <summary>
        /// Sets the transparency of icons when the menu item is not selected.
        /// </summary>
        public OdinMenuStyle SetNotSelectedIconAlpha(float value)
        {
            this.NotSelectedIconAlpha = value;
            return this;
        }

        /// <summary>
        /// Sets the padding between the icon and other content.
        /// </summary>
        public OdinMenuStyle SetIconPadding(float value)
        {
            this.IconPadding = value;
            return this;
        }

        /// <summary>
        /// Sets whether to draw the a foldout triangle for menu items with children.
        /// </summary>
        public OdinMenuStyle SetDrawFoldoutTriangle(bool value)
        {
            this.DrawFoldoutTriangle = value;
            return this;
        }

        /// <summary>
        /// Sets the size of the foldout triangle icon.
        /// </summary>
        public OdinMenuStyle SetTriangleSize(float value)
        {
            this.TriangleSize = value;
            return this;
        }

        /// <summary>
        /// Sets the padding between the foldout triangle icon and other content.
        /// </summary>
        public OdinMenuStyle SetTrianglePadding(float value)
        {
            this.TrianglePadding = value;
            return this;
        }

        /// <summary>
        /// Sets whether or not to align the triangle left or right of the content.
        /// If right, then the icon is pushed all the way to the right at a fixed position ignoring the indent level.
        /// </summary>
        public OdinMenuStyle SetAlignTriangleLeft(bool value)
        {
            this.AlignTriangleLeft = value;
            return this;
        }

        /// <summary>
        /// Sets whether to draw borders between menu items.
        /// </summary>
        public OdinMenuStyle SetBorders(bool value)
        {
            this.Borders = value;
            return this;
        }

        /// <summary>
        /// Sets the border alpha.
        /// </summary>
        public OdinMenuStyle SetBorderPadding(float value)
        {
            this.BorderPadding = value;
            return this;
        }

        /// <summary>
        /// Sets the border alpha.
        /// </summary>
        public OdinMenuStyle SetBorderAlpha(float value)
        {
            this.BorderAlpha = value;
            return this;
        }

        /// <summary>
        /// Sets the background color for when a menu item is selected.
        /// </summary>
        public OdinMenuStyle SetSelectedColorDarkSkin(Color value)
        {
            this.SelectedColorDarkSkin = value;
            return this;
        }

        /// <summary>
        /// Sets the background color for when a menu item is selected.
        /// </summary>
        public OdinMenuStyle SetSelectedColorLightSkin(Color value)
        {
            this.SelectedColorLightSkin = value;
            return this;
        }

        /// <summary>
        /// Creates and returns an instance of a menu style that makes it look like Unity's project window.
        /// </summary>
        public static OdinMenuStyle TreeViewStyle
        {
            get
            {
                return new OdinMenuStyle()
                {
                    BorderPadding = 0f,
                    AlignTriangleLeft = true,
                    TriangleSize = 16,
                    TrianglePadding = 0,
                    Offset = 20,
                    Height = 23,
                    IconPadding = 0,
                    BorderAlpha = 0.323f
                };
            }
        }

        public OdinMenuStyle Clone()
        {
            return new OdinMenuStyle()
            {
                Height = this.Height,
                Offset = this.Offset,
                IndentAmount = this.IndentAmount,
                IconSize = this.IconSize,
                IconOffset = this.IconOffset,
                NotSelectedIconAlpha = this.NotSelectedIconAlpha,
                IconPadding = this.IconPadding,
                TriangleSize = this.TriangleSize,
                TrianglePadding = this.TrianglePadding,
                AlignTriangleLeft = this.AlignTriangleLeft,
                Borders = this.Borders,
                BorderPadding = this.BorderPadding,
                BorderAlpha = this.BorderAlpha,
                SelectedColorDarkSkin = this.SelectedColorDarkSkin,
                SelectedColorLightSkin = this.SelectedColorLightSkin
            };
        }

        [Button("Copy C# Snippet", ButtonSizes.Large)]
        private void CopyCSharpSnippet()
        {
            Clipboard.Copy(@"new OdinMenuStyle()
{
    Height = " + this.Height + @",
    Offset = " + this.Offset.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    IndentAmount = " + this.IndentAmount.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    IconSize = " + this.IconSize.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    IconOffset = " + this.IconOffset.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    NotSelectedIconAlpha = " + this.NotSelectedIconAlpha.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    IconPadding = " + this.IconPadding.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    TriangleSize = " + this.TriangleSize.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    TrianglePadding = " + this.TrianglePadding.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    AlignTriangleLeft = " + this.AlignTriangleLeft.ToString().ToLower() + @",
    Borders = " + this.Borders.ToString().ToLower() + @",
    BorderPadding = " + this.BorderPadding.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    BorderAlpha = " + this.BorderAlpha.ToString("F2", CultureInfo.InvariantCulture) + @"f,
    SelectedColorDarkSkin = new Color(" +
        this.SelectedColorDarkSkin.r.ToString("F3", CultureInfo.InvariantCulture) + @"f, " +
        this.SelectedColorDarkSkin.g.ToString("F3", CultureInfo.InvariantCulture) + @"f, " +
        this.SelectedColorDarkSkin.b.ToString("F3", CultureInfo.InvariantCulture) + @"f, " +
        this.SelectedColorDarkSkin.a.ToString("F3", CultureInfo.InvariantCulture) + @"f),
    SelectedColorLightSkin = new Color(" +
        this.SelectedColorLightSkin.r.ToString("F3", CultureInfo.InvariantCulture) + @"f, " +
        this.SelectedColorLightSkin.g.ToString("F3", CultureInfo.InvariantCulture) + @"f, " +
        this.SelectedColorLightSkin.b.ToString("F3", CultureInfo.InvariantCulture) + @"f, " +
        this.SelectedColorLightSkin.a.ToString("F3", CultureInfo.InvariantCulture) + @"f)" +
    "};");
        }
    }
}
#endif