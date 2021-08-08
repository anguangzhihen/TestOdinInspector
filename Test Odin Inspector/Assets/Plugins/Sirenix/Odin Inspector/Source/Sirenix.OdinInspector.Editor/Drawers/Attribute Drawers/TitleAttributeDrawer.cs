#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TitleAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="TitleAttribute"/>.
    /// </summary>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="TitleGroupAttribute"/>
    [DrawerPriority(1, 0, 0)]
    public sealed class TitleAttributeDrawer : OdinAttributeDrawer<TitleAttribute>
    {
        private ValueResolver<string> titleResolver;
        private ValueResolver<string> subtitleResolver;

        protected override void Initialize()
        {
            this.titleResolver = ValueResolver.GetForString(this.Property, this.Attribute.Title);
            this.subtitleResolver = ValueResolver.GetForString(this.Property, this.Attribute.Subtitle);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Don't draw added emtpy space for the first property.
            if (this.Property != this.Property.Tree.GetRootProperty(0))
            {
                EditorGUILayout.Space();
            }

            bool valid = true;

            if (this.titleResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.titleResolver.ErrorMessage);
                valid = false;
            }

            if (this.subtitleResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.subtitleResolver.ErrorMessage);
                valid = false;
            }

            if (valid)
            {
                SirenixEditorGUI.Title(
                    this.titleResolver.GetValue(),
                    this.subtitleResolver.GetValue(),
                    (TextAlignment)this.Attribute.TitleAlignment,
                    this.Attribute.HorizontalLine,
                    this.Attribute.Bold);
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif