#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ChangingEditorToolExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExampleDescription("Example of using EnumPaging together with OnValueChanged.")]
    [AttributeExample(typeof(EnumPagingAttribute), Order = 10)]
    [AttributeExample(typeof(OnValueChangedAttribute), Order = 10)]
    internal class ChangingEditorToolExample
    {
#if UNITY_EDITOR // UnityEditor.Tool is an editor-only type, so this example will not work in a build
        [EnumPaging, OnValueChanged("SetCurrentTool")]
        [InfoBox("Changing this property will change the current selected tool in the Unity editor.")]
        public UnityEditor.Tool sceneTool;

        private void SetCurrentTool()
        {
            UnityEditor.Tools.current = this.sceneTool;
        }
#endif
    }
}
#endif