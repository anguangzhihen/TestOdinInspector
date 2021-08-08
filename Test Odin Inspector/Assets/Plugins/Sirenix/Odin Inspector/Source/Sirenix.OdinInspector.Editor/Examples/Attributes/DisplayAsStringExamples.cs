#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisplayAsStringExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(DisplayAsStringAttribute))]
    internal class DisplayAsStringExamples
    {
        [InfoBox(
            "Instead of disabling values in the inspector in order to show some information or debug a value. " +
            "You can use DisplayAsString to show the value as text, instead of showing it in a disabled drawer")]
        [DisplayAsString]
        public Color SomeColor;

        [BoxGroup("SomeBox")]
        [HideLabel]
        [DisplayAsString]
        public string SomeText = "Lorem Ipsum";

		[InfoBox("The DisplayAsString attribute can also be configured to enable or disable overflowing to multiple lines.")]
		[HideLabel]
		[DisplayAsString]
		public string Overflow = "A very very very very very very very very very long string that has been configured to overflow.";

		[HideLabel]
		[DisplayAsString(false)]
		public string DisplayAllOfIt = "A very very very very very very very very long string that has been configured to not overflow.";
    }
}
#endif