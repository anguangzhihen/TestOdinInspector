#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableInEditorModeAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableInEditorModeAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEngine;

    public class DisableInEditorModeAttributeStateUpdater : AttributeStateUpdater<DisableInEditorModeAttribute>
    {
        public override void OnStateUpdate()
        {
            // Only disable, never enable
            if (this.Property.State.Enabled && !Application.isPlaying)
            {
                this.Property.State.Enabled = false;
            }
        }
    }
}
#endif