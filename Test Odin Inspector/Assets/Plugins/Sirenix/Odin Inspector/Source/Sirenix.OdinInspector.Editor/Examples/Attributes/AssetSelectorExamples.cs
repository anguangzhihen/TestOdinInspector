#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetSelectorExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(AssetSelectorAttribute), "The AssetSelector attribute prepends a small button next to the object field that will present the user with a dropdown of assets to select from which can be customized from the attribute.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
    internal class AssetSelectorExamples
    {
        [AssetSelector]
        public Material AnyAllMaterials;

        [AssetSelector]
        public Material[] ListOfAllMaterials;

        [AssetSelector(FlattenTreeView = true)]
        public PhysicMaterial NoTreeView;

        [AssetSelector(Paths = "Assets/MyScriptableObjects")]
        public ScriptableObject ScriptableObjectsFromFolder;

        [AssetSelector(Paths = "Assets/MyScriptableObjects|Assets/Other/MyScriptableObjects")]
        public Material ScriptableObjectsFromMultipleFolders;

        [AssetSelector(Filter = "name t:type l:label")]
        public UnityEngine.Object AssetDatabaseSearchFilters;

        [Title("Other Minor Features")]

        [AssetSelector(DisableListAddButtonBehaviour = true)]
        public List<GameObject> DisableListAddButtonBehaviour;

        [AssetSelector(DrawDropdownForListElements = false)]
        public List<GameObject> DisableListElementBehaviour;

        [AssetSelector(ExcludeExistingValuesInList = false)]
        public List<GameObject> ExcludeExistingValuesInList;

        [AssetSelector(IsUniqueList = false)]
        public List<GameObject> DisableUniqueListBehaviour;

        [AssetSelector(ExpandAllMenuItems = true)]
        public List<GameObject> ExpandAllMenuItems;

        [AssetSelector(DropdownTitle = "Custom Dropdown Title")]
        public List<GameObject> CustomDropdownTitle;
    }
}
#endif