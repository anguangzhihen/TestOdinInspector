#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="LabelTextAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using UnityEditor;

    /// <summary>
    /// Draws properties marked with <see cref="LabelTextAttribute"/>.
    /// Creates a new GUIContent, with the provided label text, before calling further down in the drawer chain.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="GUIColorAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class LabelTextAttributeDrawer : OdinAttributeDrawer<LabelTextAttribute>
    {
        //private static readonly IValueResolver<string> TextResolver = ValueResolverUtility.CreateResolver<string>()
        //    .TryMemberOrExpression();

        //private IValueProvider<string> textProvider;

        private ValueResolver<string> textProvider;
        private GUIContent overrideLabel;

        protected override void Initialize()
        {
            //var context = ValueResolverUtility.CreateContext(this);
            //this.textProvider = TextResolver.Resolve(context, this.Attribute.Text, this.Attribute.Text);

            this.textProvider = ValueResolver.GetForString(this.Property, this.Attribute.Text);
            this.overrideLabel = new GUIContent();
        }

        /// <summary>
        /// Draws the attribute.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.textProvider.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.textProvider.ErrorMessage);
                this.CallNextDrawer(label);
                return;
            }

            var str = this.textProvider.GetValue();
            GUIContent useLabel;

            if (str == null)
            {
                useLabel = null;
            }
            else
            {
                if (this.Attribute.NicifyText)
                {
                    str = ObjectNames.NicifyVariableName(str);
                }

                this.overrideLabel.text = str;
                useLabel = this.overrideLabel;
            }

            this.CallNextDrawer(useLabel);
        }
    }
}
#endif