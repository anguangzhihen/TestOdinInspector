#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MinValueAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEngine;
    
    [DrawerPriority(0.3)]
    public sealed class MinValueAttributeDrawer<T> : OdinAttributeDrawer<MinValueAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> minValueGetter;
        
        public override bool CanDrawTypeFilter(Type type)
        {
            return IsNumber || IsVector;
        }

        protected override void Initialize()
        {
            this.minValueGetter = ValueResolver.Get<double>(this.Property, this.Attribute.Expression, this.Attribute.MinValue);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.minValueGetter.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.minValueGetter.ErrorMessage);
            }

            this.CallNextDrawer(label);

            if (this.minValueGetter.HasError)
            {
                return;
            }

            T value = this.ValueEntry.SmartValue;
            var min = this.minValueGetter.GetValue();

            if (!GenericNumberUtility.NumberIsInRange(value, min, double.MaxValue))
            {
                this.ValueEntry.SmartValue = GenericNumberUtility.Clamp(value, min, double.MaxValue);
            }
        }
    }
}
#endif