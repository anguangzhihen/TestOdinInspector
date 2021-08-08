#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInEditorModeAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.HideInEditorModeAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEngine;

    public sealed class HideInEditorModeAttributeStateUpdater : AttributeStateUpdater<HideInEditorModeAttribute>
    {
        public override void OnStateUpdate()
        {
            this.Property.State.Visible = Application.isPlaying;
        }
    }
}
#endif