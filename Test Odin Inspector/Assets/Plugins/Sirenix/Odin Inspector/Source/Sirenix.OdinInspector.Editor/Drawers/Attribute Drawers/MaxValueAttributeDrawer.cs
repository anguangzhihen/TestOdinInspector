#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MaxValueAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeDrawer<T> : OdinAttributeDrawer<MaxValueAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> maxValueGetter;

        public override bool CanDrawTypeFilter(Type type)
        {
            return IsNumber || IsVector;
        }

        protected override void Initialize()
        {
            this.maxValueGetter = ValueResolver.Get<double>(this.Property, this.Attribute.Expression, this.Attribute.MaxValue);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.maxValueGetter.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.maxValueGetter.ErrorMessage);
            }

            this.CallNextDrawer(label);

            if (this.maxValueGetter.HasError)
            {
                return;
            }

            T value = this.ValueEntry.SmartValue;
            var max = this.maxValueGetter.GetValue();

            if (!GenericNumberUtility.NumberIsInRange(value, double.MinValue, max))
            {
                this.ValueEntry.SmartValue = GenericNumberUtility.Clamp(value, double.MinValue, max);
            }
        }
    }
}
#endif