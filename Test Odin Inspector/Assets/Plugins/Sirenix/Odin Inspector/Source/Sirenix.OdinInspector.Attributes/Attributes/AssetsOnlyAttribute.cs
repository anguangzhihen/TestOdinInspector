//-----------------------------------------------------------------------
// <copyright file="AssetsOnlyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>AssetsOnly is used on object properties, and restricts the property to project assets, and not scene objects.</para>
    /// <para>Use this when you want to ensure an object is from the project, and not from the scene.</para>
    /// </summary>
    /// <example>
	/// <para>The following example shows a component with a game object property, that must be a prefab from the project, and not a scene object.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
	/// {
	///		[AssetsOnly]
	///		public GameObject MyPrefab;
	/// }
    /// </code>
    /// </example>
	/// <seealso cref="SceneObjectsOnlyAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class AssetsOnlyAttribute : Attribute
    {
    }
}