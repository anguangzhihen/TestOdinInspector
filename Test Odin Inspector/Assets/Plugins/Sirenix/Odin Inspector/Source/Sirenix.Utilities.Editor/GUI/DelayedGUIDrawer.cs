#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DelayedGUIDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using UnityEngine;

    public class DelayedGUIDrawer
    {
        private Rect areaRect;
        private Vector2 screenPos;
        private Material material;
        private RenderTexture prev;
        private RenderTexture target;

        public void Begin(float width, float height, bool drawGUI = false)
        {
            this.Begin(new Vector2(width, height), drawGUI);
        }

        public void Begin(Vector2 size, bool drawGUI = false)
        {
            if (Event.current.type != EventType.Layout)
            {
                var p = GUIUtility.ScreenToGUIPoint(this.screenPos);
                this.areaRect = new Rect(p, this.areaRect.size);
            }

            this.areaRect = new Rect(this.areaRect.position, size);

            GUIHelper.BeginIgnoreInput();
            GUILayout.BeginArea(this.areaRect, SirenixGUIStyles.None);

            if (Event.current.type == EventType.Repaint)
            {
                this.prev = RenderTexture.active;
                if (this.target != null)
                {
                    RenderTexture.ReleaseTemporary(this.target);
                }
                this.target = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
                RenderTexture.active = this.target;
                GL.Clear(false, true, new Color(0, 0, 0, 0));
            }
        }

        public void End()
        {
            if (Event.current.type == EventType.Repaint)
            {
                RenderTexture.active = this.prev;
            }

            GUILayout.EndArea();
            GUIHelper.EndIgnoreInput();
        }

        public void Draw(Vector2 position)
        {
            if (Event.current.type != EventType.Layout)
            {
                this.screenPos = GUIUtility.GUIToScreenPoint(position);
            }

            if (Event.current.type == EventType.Repaint)
            {
                if (this.material == null)
                {
                    this.material = new Material(Shader.Find("Unlit/Transparent"));
                }

                if (this.target != null)
                {
                    Graphics.Blit(this.target, RenderTexture.active, material);
                    RenderTexture.ReleaseTemporary(this.target);
                    this.target = null;
                }
            }
        }
    }
}
#endif