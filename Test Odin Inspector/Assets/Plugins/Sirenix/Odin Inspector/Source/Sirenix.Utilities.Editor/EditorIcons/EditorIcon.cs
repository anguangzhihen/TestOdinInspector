#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EditorIcon.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Icon for using in editor GUI.
    /// </summary>
    public abstract class EditorIcon
    {
        private static Material blurWhenDownscalingMaterial;

        private GUIContent inactiveGUIContent;
        private GUIContent highlightedGUIContent;
        private GUIContent activeGUIContent;

        /// <summary>
        /// Gets the raw input icon texture.
        /// </summary>
        public abstract Texture2D Raw { get; }

        /// <summary>
        /// Gets the icon's highlighted texture.
        /// </summary>
        public abstract Texture Highlighted { get; }

        /// <summary>
        /// Gets the icon's active texture.
        /// </summary>
        public abstract Texture Active { get; }

        /// <summary>
        /// Gets the icon's inactive texture.
        /// </summary>
        public abstract Texture Inactive { get; }

        /// <summary>
        /// Gets a GUIContent object with the active texture.
        /// </summary>
        public GUIContent ActiveGUIContent
        {
            get
            {
                if (this.activeGUIContent == null || this.activeGUIContent.image == null)
                {
                    this.activeGUIContent = new GUIContent(this.Inactive);
                }
                return this.activeGUIContent;
            }
        }

        /// <summary>
        /// Gets a GUIContent object with the inactive texture.
        /// </summary>
        public GUIContent InactiveGUIContent
        {
            get
            {
                if (this.inactiveGUIContent == null || this.inactiveGUIContent.image == null)
                {
                    this.inactiveGUIContent = new GUIContent(this.Inactive);
                }
                return this.inactiveGUIContent;
            }
        }

        /// <summary>
        /// Gets a GUIContent object with the highlighted texture.
        /// </summary>
        public GUIContent HighlightedGUIContent
        {
            get
            {
                if (this.highlightedGUIContent == null || this.highlightedGUIContent.image == null)
                {
                    this.highlightedGUIContent = new GUIContent(this.Inactive);
                }
                return this.highlightedGUIContent;
            }
        }

        /// <summary>
        /// Draws the icon in a square rect, with a custom shader that makes the icon look better when down-scaled.
        /// This also handles mouseover effects, and linier color spacing.
        /// </summary>
        public void Draw(Rect rect)
        {
            if (Event.current.type != EventType.Repaint) return;

            Texture iconTex;
            if (!GUI.enabled)
            {
                iconTex = this.Inactive;
            }
            else if (rect.Contains(Event.current.mousePosition))
            {
                GUIHelper.RequestRepaint();
                iconTex = this.Highlighted;
            }
            else
            {
                iconTex = this.Active;
            }

            this.Draw(rect, iconTex);
        }


        /// <summary>
        /// Draws the icon in a square rect, with a custom shader that makes the icon look better when down-scaled.
        /// This also handles mouseover effects, and linier color spacing.
        /// </summary>
        //[Obsolete("Draw the texture like you normally would instead.")]
        public void Draw(Rect rect, float drawSize)
        {
            if (Event.current.type != EventType.Repaint) return;

            Texture iconTex;
            if (!GUI.enabled)
            {
                iconTex = this.Inactive;
            }
            else if (rect.Contains(Event.current.mousePosition))
            {
                GUIHelper.RequestRepaint();
                iconTex = this.Highlighted;
            }
            else
            {
                iconTex = this.Active;
            }

            rect = rect.AlignCenter(drawSize, drawSize);
            this.Draw(rect, iconTex);
        }

        /// <summary>
        /// Draws the icon in a square rect, with a custom shader that makes the icon look better when down-scaled.
        /// This also handles mouseover effects, and linier color spacing.
        /// </summary>
        public void Draw(Rect rect, Texture texture)
        {
            if (Event.current.type != EventType.Repaint) return;

            rect.x = (int)rect.x;
            rect.y = (int)rect.y;
            rect.width = (int)rect.width;
            rect.height = (int)rect.height;

            GUI.DrawTexture(rect, texture);

            //if (!GUIClipInfo.VisibleRect.Contains(rect.center))
            //{
            //    return;
            //}

            //if (blurWhenDownscalingMaterial == null || blurWhenDownscalingMaterial.shader == null)
            //{
            //    blurWhenDownscalingMaterial = new Material(ShaderUtil.CreateShaderAsset(blurWhenDownscalingShader));
            //}


            //// The smaller the image, the bigger the texel size.
            //float texelSize = Mathf.Pow(Mathf.Max((1f - rect.width/this.Active.width) * 1.866f, 0), 1.46f) / this.Active.width;
            //blurWhenDownscalingMaterial.SetFloat("_TexelSize", texelSize);
            //blurWhenDownscalingMaterial.SetColor("_TintColor", GUI.color);
            //Graphics.DrawTexture(rect, texture, blurWhenDownscalingMaterial);
        }
    }
}
#endif