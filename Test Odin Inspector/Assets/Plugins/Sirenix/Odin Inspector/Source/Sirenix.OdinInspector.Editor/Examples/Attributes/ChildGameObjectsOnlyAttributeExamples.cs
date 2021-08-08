#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ChildGameObjectsOnlyAttributeExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(ChildGameObjectsOnlyAttribute), "The ChildGameObjectsOnly attribute can be used on Components and GameObject fields and will prepend a small button next to the object-field that will search through all child gameobjects for assignable objects and present them in a dropdown for the user to choose from.")]
    internal class ChildGameObjectsOnlyAttributeExamples
    {
        [ChildGameObjectsOnly]
        public Transform ChildOrSelfTransform;

        [ChildGameObjectsOnly]
        public GameObject ChildGameObject;

        [ChildGameObjectsOnly(IncludeSelf = false)]
        public Light[] Lights;
    }
}
#endif