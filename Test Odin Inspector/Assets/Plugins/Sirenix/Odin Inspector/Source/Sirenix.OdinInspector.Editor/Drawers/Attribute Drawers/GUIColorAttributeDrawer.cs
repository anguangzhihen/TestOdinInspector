#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIColorAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="GUIColorAttribute"/>.
    /// This drawer sets the current GUI color, before calling the next drawer in the chain.
    /// </summary>
    /// <seealso cref="GUIColorAttribute"/>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="ColorPaletteAttribute"/>
    [DrawerPriority(0.5, 0, 0)]
    public sealed class GUIColorAttributeDrawer : OdinAttributeDrawer<GUIColorAttribute>
    {
        internal static Color CurrentOuterColor = Color.white;

        private ValueResolver<Color> colorResolver;

        protected override void Initialize()
        {
            this.colorResolver = ValueResolver.Get<Color>(this.Property, this.Attribute.GetColor, this.Attribute.Color);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.colorResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.colorResolver.ErrorMessage);
                this.CallNextDrawer(label);
            }
            else
            {
                GUIHelper.PushColor(this.colorResolver.GetValue());
                this.CallNextDrawer(label);
                GUIHelper.PopColor();
            }
        }
    }
}
#endif