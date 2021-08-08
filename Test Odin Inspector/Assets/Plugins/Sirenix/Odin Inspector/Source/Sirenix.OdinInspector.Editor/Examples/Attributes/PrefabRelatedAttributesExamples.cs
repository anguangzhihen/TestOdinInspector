#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PrefabRelatedAttributesExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(HideInPrefabAssetsAttribute))]
    [AttributeExample(typeof(HideInPrefabInstancesAttribute))]
    [AttributeExample(typeof(HideInPrefabsAttribute))]
    [AttributeExample(typeof(HideInNonPrefabsAttribute))]
    [AttributeExample(typeof(DisableInPrefabAssetsAttribute))]
    [AttributeExample(typeof(DisableInPrefabInstancesAttribute))]
    [AttributeExample(typeof(DisableInPrefabsAttribute))]
    [AttributeExample(typeof(DisableInNonPrefabsAttribute))]
    internal class PrefabRelatedAttributesExamples
    {
        [InfoBox("These attributes will only have an effect when inspecting a GameObject's component.")]
        [HideInPrefabAssets]
        public GameObject HiddenInPrefabAssets;

        [HideInPrefabInstances]
        public GameObject HiddenInPrefabInstances;

        [HideInPrefabs]
        public GameObject HiddenInPrefabs;

        [HideInNonPrefabs]
        public GameObject HiddenInNonPrefabs;

        [DisableInPrefabAssets]
        public GameObject DisabledInPrefabAssets;

        [DisableInPrefabInstances]
        public GameObject DisabledInPrefabInstances;

        [DisableInPrefabs]
        public GameObject DisabledInPrefabs;

        [DisableInNonPrefabs]
        public GameObject DisabledInNonPrefabs;
    }
}
#endif