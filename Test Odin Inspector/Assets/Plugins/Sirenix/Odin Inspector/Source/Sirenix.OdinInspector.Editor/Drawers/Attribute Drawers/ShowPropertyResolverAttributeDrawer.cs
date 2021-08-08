#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowPropertyResolverAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Drawer for the ShowPropertyResolver attribute.
    /// </summary>
    /// <seealso cref="ShowPropertyResolverAttribute" />
    [DrawerPriority(10000, 0, 0)]
    public class ShowPropertyResolverAttributeDrawer : OdinAttributeDrawer<ShowPropertyResolverAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var name = property.ChildResolver != null ? property.ChildResolver.GetType().GetNiceName() : "None";

            SirenixEditorGUI.BeginToolbarBox(name);
            this.CallNextDrawer(label);
            SirenixEditorGUI.EndToolbarBox();
        }
    }
}
#endif