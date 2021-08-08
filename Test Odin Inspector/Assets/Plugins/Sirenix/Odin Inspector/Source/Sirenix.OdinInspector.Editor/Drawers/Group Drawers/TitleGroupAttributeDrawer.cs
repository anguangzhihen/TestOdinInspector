#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TitleGroupAttributeDrawer.cs" company="Sirenix IVS">
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
    /// Draws properties marked with <see cref="TitleGroupAttribute"/>.
    /// </summary>
    /// <seealso cref="TitleGroupAttribute"/>
    /// <seealso cref="TitleAttribute"/>

    public sealed class TitleGroupAttributeDrawer : OdinGroupDrawer<TitleGroupAttribute>
    {
        public ValueResolver<string> TitleHelper;
        public ValueResolver<string> SubtitleHelper;

        protected override void Initialize()
        {
            this.TitleHelper = ValueResolver.GetForString(this.Property, this.Attribute.GroupName);
            this.SubtitleHelper = ValueResolver.GetForString(this.Property, this.Attribute.Subtitle);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var attribute = this.Attribute;

            if (property != property.Tree.GetRootProperty(0))
            {
                EditorGUILayout.Space();
            }

            SirenixEditorGUI.Title(this.TitleHelper.GetValue(), this.SubtitleHelper.GetValue(), (TextAlignment)(int)attribute.Alignment, attribute.HorizontalLine, attribute.BoldTitle);

            GUIHelper.PushIndentLevel(EditorGUI.indentLevel + (attribute.Indent ? 1 : 0));
            for (int i = 0; i < property.Children.Count; i++)
            {
                var child = property.Children[i];
                child.Draw(child.Label);
            }
            GUIHelper.PopIndentLevel();
        }
    }
}
#endif