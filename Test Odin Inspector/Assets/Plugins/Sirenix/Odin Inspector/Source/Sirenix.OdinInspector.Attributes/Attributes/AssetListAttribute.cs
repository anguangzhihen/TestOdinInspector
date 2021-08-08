//-----------------------------------------------------------------------
// <copyright file="AssetListAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>AssetLists is used on lists and arrays and single elements of unity types, and replaces the default list drawer with a list of all possible assets with the specified filter.</para>
    /// <para>Use this to both filter and include or exclude assets from a list or an array, without navigating the project window.</para>
    /// </summary>
    /// <remarks>
    /// <para>Asset lists works on all asset types such as materials, scriptable objects, prefabs, custom components, audio, textures etc, and does also show inherited types.</para>
    /// </remarks>
    /// <example>
    /// <para>The following example will display an asset list of all prefabs located in the project window.</para>
    /// <code>
    /// public class AssetListExamples : MonoBehaviour
    /// {
    ///     [InfoBox("The AssetList attribute work on both lists of UnityEngine.Object types and UnityEngine.Object types, but have different behaviour.")]
    ///     [AssetList]
    ///     [InlineEditor(InlineEditorModes.LargePreview)]
    ///     public GameObject Prefab;
    /// 
    ///     [AssetList]
    ///     public List&lt;PlaceableObject&gt; PlaceableObjects;
    /// 
    ///     [AssetList(Path = "Plugins/Sirenix/")]
    ///     [InlineEditor(InlineEditorModes.LargePreview)]
    ///     public UnityEngine.Object Object;
    /// 
    ///     [AssetList(AutoPopulate = true)]
    ///     public List&lt;PlaceableObject&gt; PlaceableObjectsAutoPopulated;
    /// 
    ///     [AssetList(LayerNames = "MyLayerName")]
    ///     public GameObject[] AllPrefabsWithLayerName;
    /// 
    ///     [AssetList(AssetNamePrefix = "Rock")]
    ///     public List&lt;GameObject&gt; PrefabsStartingWithRock;
    /// 
    ///     [AssetList(Path = "/Plugins/Sirenix/")]
    ///     public List&lt;GameObject&gt; AllPrefabsLocatedInFolder;
    /// 
    ///     [AssetList(Tags = "MyTagA, MyTabB", Path = "/Plugins/Sirenix/")]
    ///     public List&lt;GameObject&gt; GameObjectsWithTag;
    /// 
    ///     [AssetList(Path = "/Plugins/Sirenix/")]
    ///     public List&lt;Material&gt; AllMaterialsInSirenix;
    /// 
    ///     [AssetList(Path = "/Plugins/Sirenix/")]
    ///     public List&lt;ScriptableObject&gt; AllScriptableObjects;
    /// 
    ///     [InfoBox("Use a method as a custom filter for the asset list.")]
    ///     [AssetList(CustomFilterMethod = "HasRigidbodyComponent")]
    ///     public List&lt;GameObject&gt; MyRigidbodyPrefabs;
    /// 
    ///     private bool HasRigidbodyComponent(GameObject obj)
    ///     {
    ///         return obj.GetComponent&lt;Rigidbody&gt;() != null;
    ///     }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class AssetListAttribute : Attribute
    {
        /// <summary>
        /// <para>If <c>true</c>, all assets found and displayed by the asset list, will automatically be added to the list when inspected.</para>
        /// </summary>
        public bool AutoPopulate;

        /// <summary>
		/// <para>Comma separated list of tags to filter the asset list.</para>
        /// </summary>
        public string Tags;

        /// <summary>
        /// <para>Filter the asset list to only include assets with a specified layer.</para>
        /// </summary>
        public string LayerNames;

        /// <summary>
        /// <para>Filter the asset list to only include assets which name begins with.</para>
        /// </summary>
        public string AssetNamePrefix;

        /// <summary>
        /// <para>Filter the asset list to only include assets which is located at the specified path.</para>
        /// </summary>
        public string Path;

        /// <summary>
        /// <para>Filter the asset list to only include assets for which the given filter method returns true.</para>
        /// </summary>
        public string CustomFilterMethod;

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="AssetListAttribute"/> class.</para>
        /// </summary>
        public AssetListAttribute()
        {
            this.AutoPopulate = false;
            this.Tags = null;
            this.LayerNames = null;
            this.AssetNamePrefix = null;
            this.CustomFilterMethod = null;
        }
    }
}