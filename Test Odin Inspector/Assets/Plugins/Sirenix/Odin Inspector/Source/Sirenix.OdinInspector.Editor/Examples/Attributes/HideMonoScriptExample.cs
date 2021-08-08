#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideMonoScriptExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using UnityEngine;

    [AttributeExample(
        typeof(HideMonoScriptAttribute),
        Description = "The HideMonoScript attribute lets you hide the script reference at the top of the inspector of Unity objects." +
        "You can use this to reduce some of the clutter in your inspector.\n\n" +
        "You can also enable this behaviour globally from the general options in Tools > Odin Inspector > Preferences > General.")]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class HideMonoScriptExample
    {
#if UNITY_EDITOR // HideMonoScriptScriptableObject and ShowMonoScriptScriptableObject are example types and only exist in the editor
        [InfoBox("Click the pencil icon to open new inspector for these fields.")]
        public HideMonoScriptScriptableObject Hidden;

        // The script will also be hidden for the ShowMonoScript object if MonoScripts are hidden globally.
        public ShowMonoScriptScriptableObject Shown;
#endif

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            Hidden = ExampleHelper.GetScriptableObject<HideMonoScriptScriptableObject>("Hidden");
            Shown = ExampleHelper.GetScriptableObject<ShowMonoScriptScriptableObject>("Shown");
        }

        [OnInspectorDispose]
        private void CleanupData()
        {
            if (Hidden != null) Object.DestroyImmediate(Hidden);
            if (Shown != null) Object.DestroyImmediate(Shown);
        }
#endif
    }
}
#endif