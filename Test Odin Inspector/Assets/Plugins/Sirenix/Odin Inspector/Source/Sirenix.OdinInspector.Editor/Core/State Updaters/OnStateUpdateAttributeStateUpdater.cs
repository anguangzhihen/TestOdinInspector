#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnStateUpdateAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.OnStateUpdateAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ActionResolvers;

    public sealed class OnStateUpdateAttributeStateUpdater : AttributeStateUpdater<OnStateUpdateAttribute>
    {
        private ActionResolver action;

        protected override void Initialize()
        {
            this.action = ActionResolver.Get(this.Property, this.Attribute.Action);
            this.ErrorMessage = this.action.ErrorMessage;
        }

        public override void OnStateUpdate()
        {
            this.action.DoActionForAllSelectionIndices();
            this.ErrorMessage = this.action.ErrorMessage;
        }
    }
}
#endif