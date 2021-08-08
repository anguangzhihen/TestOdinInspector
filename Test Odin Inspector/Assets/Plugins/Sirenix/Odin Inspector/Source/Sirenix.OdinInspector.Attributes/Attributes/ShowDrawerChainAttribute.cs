//-----------------------------------------------------------------------
// <copyright file="ShowDrawerChainAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    /// ShowDrawerChain lists all prepend, append and value drawers being used in the inspector.
    /// This is great in situations where you want to debug, and want to know which drawers might be involved in drawing the property.
    /// </para>
    /// <para>Your own custom drawers are highlighted with a green label.</para>
    /// <para>Drawers, that have not been called during the draw chain, will be greyed out in the inspector to make it clear which drawers have had an effect on the properties.</para>
    /// </summary>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ShowDrawerChain]
    ///		public int IndentedInt;
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowDrawerChainAttribute : Attribute
    {
    }
}