#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="VerticalGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Drawer for the <see cref="VerticalGroupAttribute"/>
    /// </summary>
    /// <seealso cref="VerticalGroupAttribute"/>

    public class VerticalGroupAttributeDrawer : OdinGroupDrawer<VerticalGroupAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var attribute = this.Attribute;

            GUILayout.BeginVertical();

            if (attribute.PaddingTop != 0)
            {
                GUILayout.Space(attribute.PaddingTop);
            }

            for (int i = 0; i < property.Children.Count; i++)
            {
                var child = property.Children[i];
                child.Draw(child.Label);
            }

            if (attribute.PaddingBottom != 0)
            {
                GUILayout.Space(attribute.PaddingBottom);
            }

            GUILayout.EndVertical();
        }
    }
}
#endif