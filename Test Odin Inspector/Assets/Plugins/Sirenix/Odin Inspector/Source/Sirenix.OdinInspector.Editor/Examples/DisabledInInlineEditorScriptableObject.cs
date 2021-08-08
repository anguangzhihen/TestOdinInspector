#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisabledInInlineEditorScriptableObject.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    public class DisabledInInlineEditorScriptableObject : ScriptableObject
    {
        public string AlwaysEnabled;

        [DisableInInlineEditors]
        public string DisabledInInlineEditor;
    }
}
#endif