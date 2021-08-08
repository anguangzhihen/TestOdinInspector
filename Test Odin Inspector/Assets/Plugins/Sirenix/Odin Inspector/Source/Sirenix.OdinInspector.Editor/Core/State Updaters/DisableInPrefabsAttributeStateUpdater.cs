#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableInPrefabsAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableInPrefabsAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEditor;

    public sealed class DisableInPrefabsAttributeStateUpdater : AttributeStateUpdater<DisableInPrefabsAttribute>
    {
        private bool disable;

        protected override void Initialize()
        {
            var unityObjectTarget = this.Property.Tree.WeakTargets[0] as UnityEngine.Object;

            if (unityObjectTarget != null)
            {
                var type = PrefabUtility.GetPrefabType(unityObjectTarget);
                this.disable =
                    type == PrefabType.ModelPrefabInstance ||
                    type == PrefabType.PrefabInstance ||
                    type == PrefabType.Prefab ||
                    type == PrefabType.ModelPrefab;
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