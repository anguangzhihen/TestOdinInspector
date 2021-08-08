#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GroupVisibilityStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.GroupVisibilityStateUpdater<>))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public sealed class GroupVisibilityStateUpdater<TAttr> : AttributeStateUpdater<TAttr>, IOnChildStateChangedNotification where TAttr : PropertyGroupAttribute
    {
        private ValueResolver<bool> visibleIfResolver;
        private IfAttributeHelper visibleIfHelper;
        private object helperValue;

        private bool negateVisibleIf = false;

        public override bool CanUpdateProperty(InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Group;
        }

        protected override void Initialize()
        {
            this.Property.AnimateVisibility = this.Attribute.AnimateVisibility;

            if (this.Attribute is ShowIfGroupAttribute)
            {
                this.visibleIfHelper = new IfAttributeHelper(this.Property, (this.Attribute as ShowIfGroupAttribute).Condition, true);
                this.ErrorMessage = this.visibleIfHelper.ErrorMessage;
                this.helperValue = (this.Attribute as ShowIfGroupAttribute).Value;
            }
            else if (this.Attribute is HideIfGroupAttribute)
            {
                this.visibleIfHelper = new IfAttributeHelper(this.Property, (this.Attribute as HideIfGroupAttribute).Condition, false);
                this.ErrorMessage = this.visibleIfHelper.ErrorMessage;
                this.helperValue = (this.Attribute as HideIfGroupAttribute).Value;
                this.negateVisibleIf = true;
            }
            else
            {
                this.visibleIfResolver = ValueResolver.Get<bool>(this.Property, this.Attribute.VisibleIf, true);
                this.ErrorMessage = this.visibleIfResolver.ErrorMessage;
            }

            this.UpdateVisibility();
        }

        public override void OnStateUpdate()
        {
            if (this.visibleIfResolver != null || this.visibleIfHelper != null)
            {
                // The group also controls its own visibility
                this.UpdateVisibility();
            }
        }

        public void OnChildStateChanged(int childIndex, string state)
        {
            if (state == "Visible")
            {
                this.UpdateVisibility();
            }
        }

        public void UpdateVisibility()
        {
            if (this.visibleIfResolver != null || this.visibleIfHelper != null)
            {
                bool resolvedValue;
                
                if (this.visibleIfResolver != null)
                {
                    resolvedValue = this.visibleIfResolver.GetValue();
                    this.ErrorMessage = this.visibleIfResolver.ErrorMessage;
                }
                else
                {
                    resolvedValue = this.visibleIfHelper.GetValue(this.helperValue);
                    this.ErrorMessage = this.visibleIfHelper.ErrorMessage;
                }

                if (this.negateVisibleIf)
                {
                    resolvedValue = !resolvedValue;
                }

                this.Property.State.Visible = resolvedValue;

                if (this.Attribute.HideWhenChildrenAreInvisible && this.Property.State.Visible)
                {
                    this.Property.State.Visible &= this.AreAnyChildrenVisible();
                }
            }
            else if (this.Attribute.HideWhenChildrenAreInvisible)
            {
                this.Property.State.Visible = this.AreAnyChildrenVisible();
            }
        }

        public bool AreAnyChildrenVisible()
        {
            bool anyChildrenVisible = false;

            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                if (this.Property.Children[i].State.Visible)
                {
                    anyChildrenVisible = true;
                    break;
                }
            }

            return anyChildrenVisible;
        }
    }
}
#endif