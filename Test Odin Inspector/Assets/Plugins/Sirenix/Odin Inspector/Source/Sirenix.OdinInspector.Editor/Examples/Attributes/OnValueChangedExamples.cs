#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnValueChangedExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(OnValueChangedAttribute), "OnValueChanged is used here to create a material for a shader, when the shader is changed.")]
    internal class OnValueChangedExamples
    {
        [OnValueChanged("CreateMaterial")]
        public Shader Shader;

        [ReadOnly, InlineEditor(InlineEditorModes.LargePreview)]
        public Material Material;

        private void CreateMaterial()
        {
            if (this.Material != null)
            {
                Material.DestroyImmediate(this.Material);
            }

            if (this.Shader != null)
            {
                this.Material = new Material(this.Shader);
            }
        }
    }
}
#endif