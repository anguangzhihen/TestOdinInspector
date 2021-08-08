#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FoldoutGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws all properties grouped together with the <see cref="FoldoutGroupAttribute"/>
    /// </summary>
    /// <seealso cref="FoldoutGroupAttribute"/>
    public class FoldoutGroupAttributeDrawer : OdinGroupDrawer<FoldoutGroupAttribute>
    {
        private ValueResolver<string> titleGetter;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.titleGetter = ValueResolver.GetForString(this.Property, this.Attribute.GroupName);

            if (this.Attribute.HasDefinedExpanded)
            {
                this.Property.State.Expanded = this.Attribute.Expanded;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var attribute = this.Attribute;

            if (this.titleGetter.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.titleGetter.ErrorMessage);
            }

            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            var content = GUIHelper.TempContent(this.titleGetter.HasError ? property.Label.text : this.titleGetter.GetValue());
            this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, content);
            SirenixEditorGUI.EndBoxHeader();

            if (SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded))
            {
                for (int i = 0; i < property.Children.Count; i++)
                {
                    var child = property.Children[i];
                    child.Draw(child.Label);
                }
            }
            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }
    }
}
#endif