#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInInspectorAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.HideInInspectorAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEngine;

    public sealed class HideInInspectorAttributeStateUpdater : AttributeStateUpdater<HideInInspector>
    {
        private bool showInInspectorAttribute;
        private PropertyContext<bool> isInReference;

        protected override void Initialize()
        {
            this.showInInspectorAttribute = this.Property.Attributes.HasAttribute<ShowInInspectorAttribute>();
        }

        public override void OnStateUpdate()
        {
            // The ShowInInspector attribute should always overrule the HideInInspector attribute.
            // Also draw if we are a collection element.
            if (this.showInInspectorAttribute || (this.Property.Parent != null && this.Property.Parent.ChildResolver is ICollectionResolver))
            {
                this.Property.State.Visible = true;
                return;
            }

            // Draw if we are in a reference
            if (this.isInReference == null)
            {
                this.isInReference = this.Property.Context.GetGlobal("is_in_reference", false);
            }

            if (this.isInReference.Value)
            {
                this.Property.State.Visible = true;
                return;
            }

            this.Property.State.Visible = false;
        }
    }
}
#endif