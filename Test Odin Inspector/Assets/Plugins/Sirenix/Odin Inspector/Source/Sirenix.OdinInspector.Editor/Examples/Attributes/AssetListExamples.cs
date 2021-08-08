#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetListExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(AssetListAttribute), "The AssetList attribute work on both lists of UnityEngine.Object types and UnityEngine.Object types, but have different behaviour.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
    internal class AssetListExamples
    {
        [AssetList]
        [PreviewField(70, ObjectFieldAlignment.Center)]
        public Texture2D SingleObject;

        [AssetList(Path = "/Plugins/Sirenix/")]
        public List<ScriptableObject> AssetList;

        [FoldoutGroup("Filtered Odin ScriptableObjects", expanded: false)]
        [AssetList(Path = "Plugins/Sirenix/")]
        public ScriptableObject Object;

        [AssetList(AutoPopulate = true, Path = "Plugins/Sirenix/")]
        [FoldoutGroup("Filtered Odin ScriptableObjects", expanded: false)]
        public List<ScriptableObject> AutoPopulatedWhenInspected;

        [AssetList(LayerNames = "MyLayerName")]
        [FoldoutGroup("Filtered AssetLists examples")]
        public GameObject[] AllPrefabsWithLayerName;

        [AssetList(AssetNamePrefix = "Rock")]
        [FoldoutGroup("Filtered AssetLists examples")]
        public List<GameObject> PrefabsStartingWithRock;

        [FoldoutGroup("Filtered AssetLists examples")]
        [AssetList(Tags = "MyTagA, MyTabB", Path = "/Plugins/Sirenix/")]
        public List<GameObject> GameObjectsWithTag;

        [FoldoutGroup("Filtered AssetLists examples")]
        [AssetList(CustomFilterMethod = "HasRigidbodyComponent")]
        public List<GameObject> MyRigidbodyPrefabs;

        private bool HasRigidbodyComponent(GameObject obj)
        {
            return obj.GetComponent<Rigidbody>() != null;
        }
    }
}
#endif