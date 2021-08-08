#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InlineEditorExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(InlineEditorAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class InlineEditorExamples
    {
#if UNITY_EDITOR // ExampleTransform is an example type and only exists in the editor
        [InlineEditor]
        public ExampleTransform InlineComponent;
#endif

        [InlineEditor(InlineEditorModes.FullEditor)]
        public Material FullInlineEditor;

        [InlineEditor(InlineEditorModes.GUIAndHeader)]
        public Material InlineMaterial;

        [InlineEditor(InlineEditorModes.SmallPreview)]
        public Material[] InlineMaterialList;

        [InlineEditor(InlineEditorModes.LargePreview)]
        public Mesh InlineMeshPreview;

        // You can also use the InlineEditor attribute directly on a class definition itself!
        //[InlineEditor]
        //public class ExampleTransform : ScriptableObject
        //{
        //    public Vector3 Position;
        //    public Quaternion Rotation;
        //    public Vector3 Scale = Vector3.one;
        //}

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            InlineComponent = ExampleHelper.GetScriptableObject<ExampleTransform>("Inline Component");
            FullInlineEditor = ExampleHelper.GetMaterial();
            InlineMaterial = ExampleHelper.GetMaterial();
            InlineMaterialList = new Material[]
            {
                ExampleHelper.GetMaterial(),
                ExampleHelper.GetMaterial(),
                ExampleHelper.GetMaterial(),
            };
            InlineMeshPreview = ExampleHelper.GetMesh();
        }

        [OnInspectorDispose]
        private void CleanupData()
        {
            if (InlineComponent != null) Object.DestroyImmediate(InlineComponent);
        }
#endif
    }
}
#endif