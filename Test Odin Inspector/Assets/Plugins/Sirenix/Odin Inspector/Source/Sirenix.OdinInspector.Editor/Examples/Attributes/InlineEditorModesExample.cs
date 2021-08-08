#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InlineEditorModesExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(
        typeof(InlineEditorAttribute),
        Name = "Modes",
        Description = "The InlineEditor has various modes that can be used for different use cases.")]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class InlineEditorModesExample
    {
#if UNITY_EDITOR // ExampleTransform is an example type and only exists in the editor
        [Title("Boxed / Default")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public ExampleTransform Boxed;

        [Title("Foldout")]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public ExampleTransform Foldout;

        [Title("Hide ObjectField")]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public ExampleTransform CompletelyHidden;

        [Title("Show ObjectField if null")]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public ExampleTransform OnlyHiddenWhenNotNull;
#endif

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            Boxed = ExampleHelper.GetScriptableObject<ExampleTransform>("Boxed");
            Foldout = ExampleHelper.GetScriptableObject<ExampleTransform>("Foldout");
            CompletelyHidden = ExampleHelper.GetScriptableObject<ExampleTransform>("Completely Hidden");
            OnlyHiddenWhenNotNull = ExampleHelper.GetScriptableObject<ExampleTransform>("Only Hidden When Not Null");
        }

        [OnInspectorDispose]
        private void CleanupData()
        {
            if (Boxed != null) Object.DestroyImmediate(Boxed);
            if (Foldout != null) Object.DestroyImmediate(Foldout);
            if (CompletelyHidden != null) Object.DestroyImmediate(CompletelyHidden);
            if (OnlyHiddenWhenNotNull != null) Object.DestroyImmediate(OnlyHiddenWhenNotNull);
        }
#endif
    }
}
#endif