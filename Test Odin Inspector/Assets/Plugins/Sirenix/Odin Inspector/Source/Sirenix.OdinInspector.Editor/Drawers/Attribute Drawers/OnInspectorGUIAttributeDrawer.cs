#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnInspectorGUIAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.OdinInspector.Editor.ActionResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="OnInspectorGUIAttribute"/>.
    /// Calls the method, the attribute is either attached to, or the method that has been specified in the attribute, to allow for custom GUI drawing.
    /// </summary>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    /// <seealso cref="OnValueChangedAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    /// <seealso cref="DrawWithUnityAttribute"/>
    ///	<seealso cref="InlineEditorAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class OnInspectorGUIAttributeDrawer : OdinAttributeDrawer<OnInspectorGUIAttribute>
    {
        private ActionResolver propertyMethod;
        private ActionResolver prependGUI;
        private ActionResolver appendGUI;

        protected override void Initialize()
        {
            if (this.Property.Info.PropertyType == PropertyType.Method)
            {
                this.propertyMethod = ActionResolver.Get(this.Property, null);
            }
            else
            {
                if (this.Attribute.Prepend != null)
                {
                    this.prependGUI = ActionResolver.Get(this.Property, this.Attribute.Prepend);
                }

                if (this.Attribute.Append != null)
                {
                    this.appendGUI = ActionResolver.Get(this.Property, this.Attribute.Append);
                }
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.Property.Info.PropertyType == PropertyType.Method)
            {
                if (this.propertyMethod.HasError)
                    this.propertyMethod.DrawError();
                else
                    this.propertyMethod.DoAction();
            }
            else
            {
                // Draw errors, if any
                ActionResolver.DrawErrors(this.prependGUI, this.appendGUI);

                // Draw the actual GUI
                if (this.prependGUI != null && !this.prependGUI.HasError) this.prependGUI.DoAction();
                this.CallNextDrawer(label);
                if (this.appendGUI != null && !this.appendGUI.HasError) this.appendGUI.DoAction();
            }
        }
    }
}
#endif