#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyOrderExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(PropertyOrderAttribute))]
    internal class PropertyOrderExamples
    {
		[PropertyOrder(1)]
		public int Second;

		[InfoBox("PropertyOrder is used to change the order of properties in the inspector.")]
		[PropertyOrder(-1)]
		public int First;
	}
}
#endif