#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CustomValueDrawerAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using Utilities.Editor;
    using UnityEngine;
    using System.Collections;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="ValidateInputAttribute"/>.
    /// </summary>
    /// <seealso cref="ValidateInputAttribute"/>

    [DrawerPriority(0, 0, double.MaxValue)]
    public class CustomValueDrawerAttributeDrawer<T> : OdinAttributeDrawer<CustomValueDrawerAttribute, T>
    {
        private ValueResolver customDrawer;
        private static readonly NamedValue[] customDrawerArgs = new NamedValue[2]
        {
            new NamedValue("label", typeof(GUIContent)),
            new NamedValue("callNextDrawer", typeof(Func<GUIContent, bool>))
        };
        
        public override bool CanDrawTypeFilter(Type type)
        {
            return !typeof(IList).IsAssignableFrom(type);
        }

        protected override void Initialize()
        {
            this.customDrawer = ValueResolver.Get(this.ValueEntry.BaseValueType, this.Property, this.Attribute.Action, customDrawerArgs);

            if (!this.customDrawer.HasError)
            {
                this.customDrawer.Context.NamedValues.Set("callNextDrawer", (Func<GUIContent, bool>)this.CallNextDrawer);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.customDrawer.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.customDrawer.ErrorMessage);
                this.CallNextDrawer(label);
            }
            else
            {
                this.customDrawer.Context.NamedValues.Set("label", label);
                this.ValueEntry.SmartValue = (T)this.customDrawer.GetWeakValue();
            }
        }
    }
}
#endif