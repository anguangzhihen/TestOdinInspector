#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnCollectionChangedAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;
    using System;
    using Sirenix.OdinInspector.Editor.ActionResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="OnCollectionChangedAttribute"/>.
    /// </summary>
    /// <seealso cref="OnCollectionChangedAttribute"/>
    /// <seealso cref="OnValueChangedAttribute"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    /// <seealso cref="InfoBoxAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class OnCollectionChangedAttributeDrawer : OdinAttributeDrawer<OnCollectionChangedAttribute>, IDisposable
    {
        private static readonly NamedValue[] ActionArgs = new NamedValue[]
        {
            new NamedValue("info", typeof(CollectionChangeInfo))
        };

        private ActionResolver onBefore;
        private ActionResolver onAfter;
        private ICollectionResolver resolver;

        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ChildResolver is ICollectionResolver;
        }

        protected override void Initialize()
        {
            this.resolver = (ICollectionResolver)this.Property.ChildResolver;

            if (this.Attribute.Before != null)
            {
                this.onBefore = ActionResolver.Get(this.Property, this.Attribute.Before, ActionArgs);
                
                if (!this.onBefore.HasError)
                {
                    this.resolver.OnBeforeChange += this.OnBeforeChange;
                }
            }

            if (this.Attribute.After != null)
            {
                this.onAfter = ActionResolver.Get(this.Property, this.Attribute.After, ActionArgs);
                
                if (!this.onAfter.HasError)
                {
                    this.resolver.OnAfterChange += this.OnAfterChange;
                }
            }

            if ((this.onAfter == null || !this.onAfter.HasError)
                && (this.onBefore == null || !this.onBefore.HasError))
            {
                this.SkipWhenDrawing = true;
            }
        }

        private void OnBeforeChange(CollectionChangeInfo info)
        {
            this.onBefore.Context.NamedValues.Set("info", info);
            this.onBefore.DoAction(info.SelectionIndex);
        }

        private void OnAfterChange(CollectionChangeInfo info)
        {
            this.onAfter.Context.NamedValues.Set("info", info);
            this.onAfter.DoAction(info.SelectionIndex);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            ActionResolver.DrawErrors(this.onBefore, this.onAfter);
            this.CallNextDrawer(label);
        }

        public void Dispose()
        {
            if (this.onBefore != null)
            {
                this.resolver.OnBeforeChange -= this.OnBeforeChange;
            }

            if (this.onAfter != null)
            {
                this.resolver.OnAfterChange -= this.OnAfterChange;
            }
        }
    }
}
#endif