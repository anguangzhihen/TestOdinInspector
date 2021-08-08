#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MyInlineScriptableObject.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    public class MyInlineScriptableObject : ScriptableObject
    {
        [ShowInInlineEditors]
        public string ShownInInlineEditor;

        [HideInInlineEditors]
        public string HiddenInInlineEditor;
    }
}
#endif