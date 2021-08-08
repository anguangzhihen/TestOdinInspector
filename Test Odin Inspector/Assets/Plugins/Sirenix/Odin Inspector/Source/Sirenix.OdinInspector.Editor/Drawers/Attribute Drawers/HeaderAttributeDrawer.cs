#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HeaderAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HeaderAttribute"/>.
    /// </summary>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="SpaceAttribute"/>
    [DrawerPriority(1, 0, 0)]
    public sealed class HeaderAttributeDrawer : OdinAttributeDrawer<HeaderAttribute>
    {
        private ValueResolver<string> textResolver;

        protected override void Initialize()
        {
            // Don't draw for collection elements
            if (this.Property.Parent != null && this.Property.Parent.ChildResolver is ICollectionResolver)
            {
                this.SkipWhenDrawing = true;
                return;
            }

            this.textResolver = ValueResolver.GetForString(this.Property, this.Attribute.header);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.Property != this.Property.Tree.GetRootProperty(0))
            {
                EditorGUILayout.Space();
            }

            if (this.textResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.textResolver.ErrorMessage);
            }
            else
            {
                EditorGUILayout.LabelField(this.textResolver.GetValue(), EditorStyles.boldLabel);
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif