#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideReferenceObjectPickerExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(HideReferenceObjectPickerAttribute),
        "When the object picker is hidden, you can right click and set the instance to null, in order to set a new value.\n\n" +
        "If you don't want this behavior, you can use DisableContextMenu attribute to ensure people can't change the value.")]
    internal class HideReferenceObjectPickerExamples
    {
        [Title("Hidden Object Pickers")]
        [HideReferenceObjectPicker]
        public MyCustomReferenceType OdinSerializedProperty1 = new MyCustomReferenceType();

        [HideReferenceObjectPicker]
        public MyCustomReferenceType OdinSerializedProperty2 = new MyCustomReferenceType();

        [Title("Shown Object Pickers")]
        public MyCustomReferenceType OdinSerializedProperty3 = new MyCustomReferenceType();

        public MyCustomReferenceType OdinSerializedProperty4 = new MyCustomReferenceType();

        // Protip: You can also put the HideInInspector attribute on the class definition itself to hide it globally for all members.
        // [HideReferenceObjectPicker]
        public class MyCustomReferenceType
        {
            public int A;
            public int B;
            public int C;
        }
    }
}
#endif