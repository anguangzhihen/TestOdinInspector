#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SuffixLabelAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="SuffixLabelAttribute"/>.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="PropertyTooltipAttribute"/>
    /// <seealso cref="InlineButtonAttribute"/>
    /// <seealso cref="CustomValueDrawerAttribute"/>
    [AllowGUIEnabledForReadonly]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class SuffixLabelAttributeDrawer : OdinAttributeDrawer<SuffixLabelAttribute>
    {
        private ValueResolver<string> labelResolver;

        protected override void Initialize()
        {
            this.labelResolver = ValueResolver.GetForString(this.Property, this.Attribute.Label);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.labelResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.labelResolver.ErrorMessage);
            }

            if (this.Attribute.Overlay)
            {
                this.CallNextDrawer(label);
                GUIHelper.PushGUIEnabled(true);
                GUI.Label(GUILayoutUtility.GetLastRect().HorizontalPadding(0, 8), this.labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                GUIHelper.PopGUIEnabled();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                this.CallNextDrawer(label);
                GUILayout.EndVertical();
                GUIHelper.PushGUIEnabled(true);
                GUILayout.Label(this.labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel, GUILayoutOptions.ExpandWidth(false));
                GUIHelper.PopGUIEnabled();
                GUILayout.EndHorizontal();
            }
        }
    }
}
#endif