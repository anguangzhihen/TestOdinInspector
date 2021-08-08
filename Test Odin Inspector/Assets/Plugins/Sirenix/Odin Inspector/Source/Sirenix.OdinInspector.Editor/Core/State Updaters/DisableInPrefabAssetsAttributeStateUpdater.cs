#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableInPrefabAssetsAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableInPrefabAssetsAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEditor;

    public sealed class DisableInPrefabAssetsAttributeStateUpdater : AttributeStateUpdater<DisableInPrefabAssetsAttribute>
    {
        private bool disable;

        protected override void Initialize()
        {
            var unityObjectTarget = this.Property.Tree.WeakTargets[0] as UnityEngine.Object;

            if (unityObjectTarget != null)
            {
                var type = PrefabUtility.GetPrefabType(unityObjectTarget);
                this.disable = type == PrefabType.Prefab || type == PrefabType.ModelPrefab;
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