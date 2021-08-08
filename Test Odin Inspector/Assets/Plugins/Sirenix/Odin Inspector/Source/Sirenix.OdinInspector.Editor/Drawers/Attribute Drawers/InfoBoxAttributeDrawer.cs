#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InfoBoxAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;
    using UnityEditor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="InfoBoxAttribute"/>.
    /// Draws an info box above the property. Error and warning info boxes can be tracked by Odin Scene Validator.
    /// </summary>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="DetailedInfoBoxAttribute"/>
    /// <seealso cref="RequiredAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    [DrawerPriority(0, 10001, 0)]
    public sealed class InfoBoxAttributeDrawer : OdinAttributeDrawer<InfoBoxAttribute>
    {
        private bool drawMessageBox;
        private ValueResolver<bool> visibleIfResolver;
        private ValueResolver<string> messageResolver;
        private MessageType messageType;

        protected override void Initialize()
        {
            this.visibleIfResolver = ValueResolver.Get<bool>(this.Property, this.Attribute.VisibleIf, true);
            this.messageResolver = ValueResolver.GetForString(this.Property, this.Attribute.Message);

            this.drawMessageBox = this.visibleIfResolver.GetValue();

            switch (this.Attribute.InfoMessageType)
            {
                default:
                case InfoMessageType.None:
                    this.messageType = MessageType.None;
                    break;
                case InfoMessageType.Info:
                    this.messageType = MessageType.Info;
                    break;
                case InfoMessageType.Warning:
                    this.messageType = MessageType.Warning;
                    break;
                case InfoMessageType.Error:
                    this.messageType = MessageType.Error;
                    break;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            bool valid = true;

            if (this.visibleIfResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.visibleIfResolver.ErrorMessage);
                valid = false;
            }

            if (this.messageResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.messageResolver.ErrorMessage);
                valid = false;
            }

            if (!valid)
            {
                this.CallNextDrawer(label);
                return;
            }

            if (this.Attribute.GUIAlwaysEnabled)
            {
                GUIHelper.PushGUIEnabled(true);
            }

            if (Event.current.type == EventType.Layout)
            {
                this.drawMessageBox = this.visibleIfResolver.GetValue();
            }

            if (this.drawMessageBox)
            {
                SirenixEditorGUI.MessageBox(this.messageResolver.GetValue(), this.messageType);
            }

            if (this.Attribute.GUIAlwaysEnabled)
            {
                GUIHelper.PopGUIEnabled();
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif