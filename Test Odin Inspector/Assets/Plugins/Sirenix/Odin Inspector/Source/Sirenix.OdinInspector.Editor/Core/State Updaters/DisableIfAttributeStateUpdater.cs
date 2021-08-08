#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableIfAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableIfAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Drawers;

    public class DisableIfAttributeStateUpdater : AttributeStateUpdater<DisableIfAttribute>
    {
        private IfAttributeHelper helper;

        protected override void Initialize()
        {
            this.helper = new IfAttributeHelper(this.Property, this.Attribute.Condition, true);
            this.ErrorMessage = this.helper.ErrorMessage;
        }

        public override void OnStateUpdate()
        {
            this.Property.State.Enabled = !this.helper.GetValue(this.Attribute.Value);
            this.ErrorMessage = this.helper.ErrorMessage;
        }
    }
}
#endif