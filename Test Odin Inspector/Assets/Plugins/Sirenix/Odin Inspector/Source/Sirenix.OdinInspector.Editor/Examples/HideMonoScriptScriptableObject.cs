#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideMonoScriptScriptableObject.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [HideMonoScript]
    public class HideMonoScriptScriptableObject : ScriptableObject
    {
        public string Value;
    }
}
#endif