#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ButtonGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws all properties grouped together with the <see cref="ButtonGroupAttribute"/>
    /// </summary>
    /// <seealso cref="ButtonGroupAttribute"/>

    public class ButtonGroupAttributeDrawer : OdinGroupDrawer<ButtonGroupAttribute>
    {
        private float buttonHeight;

        protected override void Initialize()
        {
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                var button = this.Property.Children[i].GetAttribute<ButtonAttribute>();
                if (button != null && button.ButtonHeight > 0)
                {
                    this.buttonHeight = button.ButtonHeight;
                }
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;

            SirenixEditorGUI.BeginIndentedHorizontal();

            for (int i = 0; i < property.Children.Count; i++)
            {
                var style = (GUIStyle)null;

                if (property.Children.Count != 1)
                {
                    if (i == 0)
                    {
                        style = SirenixGUIStyles.ButtonLeft;
                    }
                    else if (i == property.Children.Count - 1)
                    {
                        style = SirenixGUIStyles.ButtonRight;
                    }
                    else
                    {
                        style = SirenixGUIStyles.ButtonMid;
                    }
                }

                var child = property.Children[i];

                child.Context.GetGlobal("ButtonHeight", this.buttonHeight).Value = this.buttonHeight;
                child.Context.GetGlobal("ButtonStyle", style).Value = style;
                DefaultMethodDrawer.DontDrawMethodParameters = true;
                child.Draw(child.Label);
                DefaultMethodDrawer.DontDrawMethodParameters = false;
            }

            SirenixEditorGUI.EndIndentedHorizontal();
        }
    }
}
#endif