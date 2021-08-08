#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SceneAndAssetsOnlyExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(AssetsOnlyAttribute))]
    [AttributeExample(typeof(SceneObjectsOnlyAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
    internal class SceneAndAssetsOnlyExamples
    {
        [Title("Assets only")]
        [AssetsOnly]
        public List<GameObject> OnlyPrefabs;

        [AssetsOnly]
        public GameObject SomePrefab;

        [AssetsOnly]
        public Material MaterialAsset;

        [AssetsOnly]
        public MeshRenderer SomeMeshRendererOnPrefab;

        [Title("Scene Objects only")]
        [SceneObjectsOnly]
        public List<GameObject> OnlySceneObjects;

        [SceneObjectsOnly]
        public GameObject SomeSceneObject;

        [SceneObjectsOnly]
        public MeshRenderer SomeMeshRenderer;
    }
}
#endif