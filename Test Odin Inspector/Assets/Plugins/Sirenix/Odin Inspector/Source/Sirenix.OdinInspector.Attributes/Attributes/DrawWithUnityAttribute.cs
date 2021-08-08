//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>DrawWithUnity can be applied to a field or property to make Odin draw it using Unity's old drawing system. Use it if you want to selectively disable Odin drawing for a particular member.</para>
    /// </summary>
    /// <remarks>
    /// <para>Note that this attribute does not mean "disable Odin completely for this property"; it is visual only in nature, and in fact represents an Odin drawer which calls into Unity's old property drawing system. As Odin is still ultimately responsible for arranging the drawing of the property, and since other attributes exist with a higher priority than this attribute, and it is not guaranteed that Unity will draw the property if another attribute is present to override this one.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class DrawWithUnityAttribute : Attribute
    {
    }
}