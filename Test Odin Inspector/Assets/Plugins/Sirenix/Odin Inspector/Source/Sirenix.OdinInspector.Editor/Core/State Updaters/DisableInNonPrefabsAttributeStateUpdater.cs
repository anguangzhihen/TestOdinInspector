#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableInNonPrefabsAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableInNonPrefabsAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEditor;

    public sealed class DisableInNonPrefabsAttributeStateUpdater : AttributeStateUpdater<DisableInNonPrefabsAttribute>
    {
        private bool disable;

        protected override void Initialize()
        {
            var unityObjectTarget = this.Property.Tree.WeakTargets[0] as UnityEngine.Object;

            if (unityObjectTarget != null)
            {
                var type = PrefabUtility.GetPrefabType(unityObjectTarget);
                this.disable =
                    type == PrefabType.None ||
                    type == PrefabType.MissingPrefabInstance ||
                    type == PrefabType.DisconnectedModelPrefabInstance ||
                    type == PrefabType.DisconnectedPrefabInstance;
            }
        }

        public override void OnStateUpdate()
        {
            // Only disable, never enable
            if (this.Property.State.Enabled && this.disable)
            {
                this.Property.State.Enabled = false;
            }
        }
    }
}
#endif