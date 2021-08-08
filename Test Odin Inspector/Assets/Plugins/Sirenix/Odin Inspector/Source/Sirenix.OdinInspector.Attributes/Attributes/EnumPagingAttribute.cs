//-----------------------------------------------------------------------
// <copyright file="EnumPagingAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>Draws an enum selector in the inspector with next and previous buttons to let you cycle through the available values for the enum property.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// public enum MyEnum
    /// {
    ///     One,
    ///     Two,
    ///     Three,
    /// }
    /// 
    /// public class MyMonoBehaviour : MonoBehaviour
    /// {
    ///     [EnumPaging]
    ///     public MyEnum Value;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class EnumPagingAttribute : Attribute
    {
    }
}