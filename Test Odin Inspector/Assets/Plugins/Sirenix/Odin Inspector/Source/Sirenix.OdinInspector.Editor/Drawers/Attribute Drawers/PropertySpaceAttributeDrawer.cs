#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertySpaceAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Draws a space for properties marked with the PropertySpace attribute.
    /// </summary>
    [DrawerPriority(2, 0, 0)]
    public sealed class PropertySpaceAttributeDrawer : OdinAttributeDrawer<PropertySpaceAttribute>
    {
        private bool drawSpace;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Property.Parent == null)
            {
                this.drawSpace = true;
            }
            else if (this.Property.Parent.ChildResolver is ICollectionResolver)
            {
                this.drawSpace = false;
            }
            else
            {
                this.drawSpace = true;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.drawSpace && this.Attribute.SpaceBefore != 0f)
            {
                GUILayout.Space(this.Attribute.SpaceBefore);
            }

            this.CallNextDrawer(label);

            if (this.Attribute.SpaceAfter != 0f)
            {
                GUILayout.Space(this.Attribute.SpaceAfter);
            }
        }
    }
}
#endif