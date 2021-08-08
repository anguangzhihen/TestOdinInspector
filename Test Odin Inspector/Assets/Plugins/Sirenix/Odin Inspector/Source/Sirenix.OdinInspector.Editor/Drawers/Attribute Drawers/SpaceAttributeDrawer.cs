#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SpaceAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="SpaceAttribute"/>.
    /// </summary>
    /// <seealso cref="SpaceAttribute"/>
    [DrawerPriority(2, 0, 0)]
    public sealed class SpaceAttributeDrawer : OdinAttributeDrawer<SpaceAttribute>
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
            if (this.drawSpace)
            {
                var attribute = this.Attribute;

                if (attribute.height == 0)
                {
                    EditorGUILayout.Space();
                }
                else
                {
                    GUILayout.Space(attribute.height);
                }
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif