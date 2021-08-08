#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MenuTreeBrowser.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [HideReferenceObjectPicker, HideLabel]
    public abstract class MenuTreeBrowser
    {
        private OdinMenuTree tree;
        private ResizableColumn[] columns = new ResizableColumn[]
        {
            ResizableColumn.FlexibleColumn(280, 80),
            ResizableColumn.DynamicColumn()
        };

        public ResizableColumn MenuColumn
        {
            get { return this.columns[0]; }
        }

        /// <summary>
        /// The content padding
        /// </summary>
        [HideInInspector]
        public Vector2 ContentPadding = new Vector4(10, 0);

        /// <summary>
        /// Draws the menu tree.
        /// </summary>
        [OnInspectorGUI, PropertyOrder(-1)]
        protected virtual void DrawMenuTree()
        {
            var rect = GUIHelper.GetCurrentLayoutRect();
            GUITableUtilities.ResizeColumns(rect, this.columns);
            rect = EditorGUILayout.BeginVertical(GUILayoutOptions.Width(this.MenuColumn.ColWidth));
            EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
            this.tree = this.tree ?? this.BuildMenuTree();
            if (this.tree != null)
            {
                this.tree.Config.AutoHandleKeyboardNavigation = true;
                this.tree.DrawMenuTree();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            GUILayout.Space(this.ContentPadding.x);
            SirenixEditorGUI.DrawBorders(rect, 1);
            GUILayout.BeginVertical();
            GUILayout.Space(this.ContentPadding.y);
        }

        /// <summary>
        /// Gets the value selected value.
        /// </summary>
        [EnableGUI, HideLabel, ShowInInspector, DisableContextMenu, VerticalGroup, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden), SuppressInvalidAttributeError]
        public object Value
        {
            get
            {
                if (this.tree == null) return null;
                var selected = this.tree.Selection.FirstOrDefault();
                if (selected == null) return null;
                return selected.Value;
            }
        }

        [OnInspectorGUI, PropertyOrder(-1000)]
        private void BeginDrawEditor()
        {
            GUILayout.BeginHorizontal();
        }

        [OnInspectorGUI, PropertyOrder(+1000)]
        private void EndDrawEditor()
        {
            GUILayout.Space(this.ContentPadding.y);
            GUILayout.EndVertical();
            GUILayout.Space(this.ContentPadding.x);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Invokes BuildMenuTree.
        /// </summary>
        public void ForceRebuildMenuTree()
        {
            this.tree = this.BuildMenuTree();
        }

        public abstract OdinMenuTree BuildMenuTree();
    }
}
#endif