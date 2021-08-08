#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnInspectorInitStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Sirenix.OdinInspector.Editor.ActionResolvers;

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.OnInspectorInitStateUpdater), 10000)]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    public sealed class OnInspectorInitStateUpdater : AttributeStateUpdater<OnInspectorInitAttribute>
    {
        protected override void Initialize()
        {
            var action = ActionResolver.Get(this.Property, this.Attribute.Action);
            action.DoActionForAllSelectionIndices();
            this.ErrorMessage = action.ErrorMessage;
        }
    }
}
#endif