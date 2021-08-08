#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SuffixLabelExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

	using UnityEngine;

    [AttributeExample(typeof(SuffixLabelAttribute),
        "The SuffixLabel attribute draws a label at the end of a property. " +
        "It's useful for conveying intend about a property.")]
    internal class SuffixLabelExamples
	{
		[SuffixLabel("Prefab")]
		public GameObject GameObject;

		[Space(15)]
		[InfoBox(
            "Using the Overlay property, the suffix label will be drawn on top of the property instead of behind it.\n" +
			"Use this for a neat inline look.")]
		[SuffixLabel("ms", Overlay = true)]
		public float Speed;

		[SuffixLabel("radians", Overlay = true)]
		public float Angle;

		[Space(15)]
		[InfoBox("The Suffix attribute also supports referencing a member string field, property, or method by using $.")]
		[SuffixLabel("$Suffix", Overlay = true)]
		public string Suffix = "Dynamic suffix label";

        [InfoBox("The Suffix attribute also supports expressions by using @.")]
        [SuffixLabel("@DateTime.Now.ToString(\"HH:mm:ss\")", true)]
        public string Expression;
	}
}
#endif