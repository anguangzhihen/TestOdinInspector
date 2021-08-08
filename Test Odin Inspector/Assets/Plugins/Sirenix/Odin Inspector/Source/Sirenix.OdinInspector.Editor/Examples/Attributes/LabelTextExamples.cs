#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="LabelTextExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(LabelTextAttribute), "Specify a different label text for your properties.")]
    internal class LabelTextExamples
    {
        [LabelText("1")]
        public int MyInt1 = 1;

        [LabelText("2")]
        public int MyInt2 = 12;

        [LabelText("3")]
        public int MyInt3 = 123;

		[InfoBox("Use $ to refer to a member string.")]
		[LabelText("$MyInt3")]
		public string LabelText = "The label is taken from the number 3 above";

		[InfoBox("Use @ to execute an expression.")]
        [LabelText("@DateTime.Now.ToString(\"HH:mm:ss\")")]
        public string DateTimeLabel;
    }
}
#endif