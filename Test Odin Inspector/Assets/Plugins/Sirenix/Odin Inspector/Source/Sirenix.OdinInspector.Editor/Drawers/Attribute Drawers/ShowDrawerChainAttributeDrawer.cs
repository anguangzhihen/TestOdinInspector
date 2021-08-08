#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowDrawerChainAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Show drawer chain attribute drawer.
    /// </summary>
    [DrawerPriority(10000, 0, 0)]
    public class ShowDrawerChainAttributeDrawer : OdinAttributeDrawer<ShowDrawerChainAttribute>
    {
        private int drawnDepth;

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;

            var chain = property.GetActiveDrawerChain();
            var drawers = chain.BakedDrawerArray;

            SirenixEditorGUI.BeginToolbarBox("Drawers for property '" + this.Property.Path + "'");

            for (int i = 0; i < drawers.Length; i++)
            {
                bool highlight = drawers[i].GetType().Assembly != typeof(ShowDrawerChainAttributeDrawer).Assembly;

                if (highlight)
                {
                    GUIHelper.PushColor(Color.green);
                }

                if (i > this.drawnDepth)
                {
                    GUIHelper.PushColor(new Color(1, 1, 1, 0.5f));
                }

                EditorGUILayout.LabelField(i + ": " + drawers[i].GetType().GetNiceName() + (drawers[i].SkipWhenDrawing ? " (skipped)" : ""));
                var rect = GUILayoutUtility.GetLastRect();

                if (i > this.drawnDepth)
                {
                    GUIHelper.PopColor();
                }

                GUI.Label(rect, DrawerUtilities.GetDrawerPriority(drawers[i].GetType()).ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);

                if (highlight)
                {
                    GUIHelper.PopColor();
                }
            }
            SirenixEditorGUI.EndToolbarBox();

            this.CallNextDrawer(label);

            this.drawnDepth = chain.CurrentIndex;
        }
    }
}
#endif