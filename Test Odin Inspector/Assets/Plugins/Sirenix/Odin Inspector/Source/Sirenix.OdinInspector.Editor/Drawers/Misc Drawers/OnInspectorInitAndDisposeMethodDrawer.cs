#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnInspectorInitAndDisposeMethodDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;

    public class OnInspectorInitAndDisposeMethodDrawer : MethodDrawer
    {
        protected override bool CanDrawMethodProperty(InspectorProperty property)
        {
            var attrs = property.Attributes;

            return attrs.HasAttribute<OnInspectorDisposeAttribute>()
                || attrs.HasAttribute<OnInspectorInitAttribute>();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Draw nothing
        }
    }
}
#endif