#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="VerticalGroupExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(VerticalGroupAttribute),
        "VerticalGroup, similar to HorizontalGroup, groups properties together vertically in the inspector.\n" +
        "By itself it doesn't do much, but combined with other groups, like HorizontalGroup, it can be very useful. It can also be used in TableLists to create columns.")]
    internal class VerticalGroupExamples
	{
		[HorizontalGroup("Split")]
		[VerticalGroup("Split/Left")]
		public InfoMessageType First;

		[VerticalGroup("Split/Left")]
		public InfoMessageType Second;

		[HideLabel]
		[VerticalGroup("Split/Right")]
		public int A;

		[HideLabel]
		[VerticalGroup("Split/Right")]
		public int B;
	}
}
#endif