#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="VisibleStateExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

	[AttributeExample(typeof(OnStateUpdateAttribute), "The following example shows how OnStateUpdate can be used to control the visible state of a property.")]
	internal class VisibleStateExample
	{
		[OnStateUpdate("@$property.State.Visible = ToggleMyInt")]
		public int MyInt;
		
		public bool ToggleMyInt;
	}
}
#endif