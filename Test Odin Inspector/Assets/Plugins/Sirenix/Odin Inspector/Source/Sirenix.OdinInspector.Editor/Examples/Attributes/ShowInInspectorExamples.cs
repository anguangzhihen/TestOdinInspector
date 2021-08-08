#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowInInspectorExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(ShowInInspectorAttribute),
        "ShowInInspector is used to display properties that otherwise wouldn't be shown in the inspector, such as non-serialized fields or properties.")]
    internal class ShowInInspectorExamples
    {
#pragma warning disable // These fields are in fact being used; let's not have bothersome warnings.
        [ShowInInspector]
        private int myPrivateInt;

        [ShowInInspector]
        public int MyPropertyInt { get; set; }

        [ShowInInspector]
        public int ReadOnlyProperty
        {
            get { return this.myPrivateInt; }
        }

        [ShowInInspector]
        public static bool StaticProperty { get; set; }
    }
}
#endif