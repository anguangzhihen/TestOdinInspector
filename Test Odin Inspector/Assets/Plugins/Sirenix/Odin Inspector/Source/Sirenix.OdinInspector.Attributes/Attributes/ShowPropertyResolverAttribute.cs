//-----------------------------------------------------------------------
// <copyright file="ShowPropertyResolverAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    /// ShowPropertyResolver shows the property resolver responsible for bringing the member into the property tree.
    /// This is useful in situations where you want to debug why a particular member that is normally not shown in the inspector suddenly is.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ShowPropertyResolver]
    ///		public int IndentedInt;
    ///	}
    /// </code>
    /// </example>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowPropertyResolverAttribute : Attribute
    {
    }
}