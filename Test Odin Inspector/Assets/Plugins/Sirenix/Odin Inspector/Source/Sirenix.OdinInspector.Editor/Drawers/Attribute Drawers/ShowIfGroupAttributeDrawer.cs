#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowIfGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;

    public class ShowIfGroupAttributeDrawer : OdinGroupDrawer<ShowIfGroupAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                this.Property.Children[i].Draw();
            }
        }
    }
}
#endif