#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInNonPrefabsAttributeStateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.HideInNonPrefabsAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEditor;

    public sealed class HideInNonPrefabsAttributeStateUpdater : AttributeStateUpdater<HideInNonPrefabsAttribute>
    {
        private bool hide;

        protected override void Initialize()
        {
            var unityObjectTarget = this.Property.Tree.WeakTargets[0] as UnityEngine.Object;

            if (unityObjectTarget != null)
            {
                var type = PrefabUtility.GetPrefabType(unityObjectTarget);
                this.hide =
                    type == PrefabType.None ||
                    type == PrefabType.MissingPrefabInstance ||
                    type == PrefabType.DisconnectedModelPrefabInstance ||
                    type == PrefabType.DisconnectedPrefabInstance;
            }
        }

        public override void OnStateUpdate()
        {
            this.Property.State.Visible = !this.hide;
        }
    }
}
#endif