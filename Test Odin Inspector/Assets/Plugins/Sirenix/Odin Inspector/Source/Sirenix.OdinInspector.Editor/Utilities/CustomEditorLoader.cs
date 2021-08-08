#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CustomEditorLoader.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEditor;

    [InitializeOnLoad]
    internal static class CustomEditorLoader
    {
        static CustomEditorLoader()
        {
            if (InspectorConfig.HasInstanceLoaded)
            {
                InspectorConfig.Instance.UpdateOdinEditors();
            }
            else
            {
                UnityEditorEventUtility.DelayAction(() => InspectorConfig.Instance.UpdateOdinEditors());
            }
        }
    }
}
#endif