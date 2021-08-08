//-----------------------------------------------------------------------
// <copyright file="PersistentAssemblyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

	using System;
    
	/// <summary>
    /// Indicates a persistent assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PersistentAssemblyAttribute : Attribute
    {
    }
}