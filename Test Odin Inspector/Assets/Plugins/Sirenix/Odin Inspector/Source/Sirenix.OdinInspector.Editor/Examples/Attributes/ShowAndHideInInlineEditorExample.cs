#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowAndHideInInlineEditorExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using UnityEngine;

    [AttributeExample(typeof(ShowInInlineEditorsAttribute))]
    [AttributeExample(typeof(HideInInlineEditorsAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class ShowAndHideInInlineEditorExample
    {
#if UNITY_EDITOR // MyInlineScriptableObject is an example type and only exists in the editor
        [InfoBox("Click the pen icon to open a new inspector window for the InlineObject too see the differences these attributes make.")]
        [InlineEditor(Expanded = true)]
        public MyInlineScriptableObject InlineObject;
#endif

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            InlineObject = ExampleHelper.GetScriptableObject<MyInlineScriptableObject>("Inline Object");
        }

        [OnInspectorDispose]
        private void CleanupData()
        {
            if (InlineObject != null) Object.DestroyImmediate(InlineObject);
        }
#endif
    }
}
#endif