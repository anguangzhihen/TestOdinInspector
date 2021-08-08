//-----------------------------------------------------------------------
// <copyright file="DontApplyToListElementsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
	
	/// <summary>
	/// <para>DontApplyToListElements is used on other attributes, and indicates that those attributes should be applied only to the list, and not to the elements of the list.</para>
	/// <para>Use this on attributes that should only work on a list or array property as a whole, and not on each element of the list.</para>
    /// </summary>
	/// <example>
	/// <para>The following example shows how DontApplyToListElements is used on <see cref="ShowIfAttribute"/>.</para>
    /// <code>
    ///	[DontApplyToListElements]
    ///	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    ///	public sealed class VisibleIfAttribute : Attribute
    ///	{
    ///	    public string MemberName { get; private set; }
	///	
    ///	    public VisibleIfAttribute(string memberName)
    ///	    {
    ///	        this.MemberName = memberName;
    ///	    }
    ///	}
	/// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DontApplyToListElementsAttribute : Attribute
    {
    }
}