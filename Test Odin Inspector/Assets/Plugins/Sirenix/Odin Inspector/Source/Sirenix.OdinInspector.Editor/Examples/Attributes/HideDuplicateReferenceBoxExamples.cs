#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideDuplicateReferenceBoxExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(HideDuplicateReferenceBoxAttribute), "Indicates that Odin should hide the reference box, if this property would otherwise be drawn as a reference to another property, due to duplicate reference values being encountered."), ShowOdinSerializedPropertiesInInspector]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.Utilities.Editor" })]
    internal partial class HideDuplicateReferenceBoxExamples
    {
        [PropertyOrder(1)]
        public ReferenceTypeClass firstObject;

        [PropertyOrder(3)]
        public ReferenceTypeClass withReferenceBox;

        [PropertyOrder(5)]
        [HideDuplicateReferenceBox]
        public ReferenceTypeClass withoutReferenceBox;

        [OnInspectorInit]
        public void CreateData()
        {
            this.firstObject = new ReferenceTypeClass();
            this.withReferenceBox = this.firstObject;
            this.withoutReferenceBox = this.firstObject;
            this.firstObject.recursiveReference = this.firstObject;
        }

        public class ReferenceTypeClass
        {
            [HideDuplicateReferenceBox]
            public ReferenceTypeClass recursiveReference;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
            [OnInspectorGUI, PropertyOrder(-1)]
            private void MessageBox()
            {
                SirenixEditorGUI.WarningMessageBox("Recursively drawn references will always show the reference box regardless, to prevent infinite depth draw loops.");
            }
#endif
        }

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorGUI, PropertyOrder(0)]
        private void MessageBox1()
        {
            SirenixEditorGUI.Title("The first reference will always be drawn normally", null, TextAlignment.Left, true);
        }

        [OnInspectorGUI, PropertyOrder(2)]
        private void MessageBox2()
        {
            GUILayout.Space(20);
            SirenixEditorGUI.Title("All subsequent references will be wrapped in a reference box", null, TextAlignment.Left, true);
        }

        [OnInspectorGUI, PropertyOrder(4)]
        private void MessageBox3()
        {
            GUILayout.Space(20);
            SirenixEditorGUI.Title("With the [HideDuplicateReferenceBox] attribute, this box is hidden", null, TextAlignment.Left, true);
        }
#endif
    }
}
#endif