//-----------------------------------------------------------------------
// <copyright file="InfoMessageType.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
    
namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// Type of info message box. This enum matches Unity's MessageType enum which could not be used since it is located in the UnityEditor assembly.
    /// </summary>
    public enum InfoMessageType
    {
		/// <summary>
		/// Generic message box with no type.
		/// </summary>
		None,

		/// <summary>
		/// Information message box.
		/// </summary>
		Info,

		/// <summary>
		/// Warning message box.
		/// </summary>
		Warning,

		/// <summary>
		/// Error message box.
		/// </summary>
		Error
	}
}