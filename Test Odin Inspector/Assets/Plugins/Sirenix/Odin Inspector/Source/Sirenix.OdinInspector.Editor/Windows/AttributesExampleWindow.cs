#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AttributesExampleWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using Sirenix.OdinInspector.Editor.Examples;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    public class AttributesExampleWindow : OdinMenuEditorWindow
    {
        private OdinAttributeExampleItem example;
        private Vector2 scrollPosition;

        public static void OpenWindow()
        {
            OpenWindow(null);
        }

        public static void OpenWindow(Type attributeType)
        {
            bool isNew = Resources.FindObjectsOfTypeAll<AttributesExampleWindow>().Length == 0;

            var w = GetWindow<AttributesExampleWindow>();

            if (isNew)
            {
                w.MenuWidth = 250;
                w.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(850f, 700f);
            }

            if (attributeType != null)
            {
                w.ForceMenuTreeRebuild();

                var item = w.MenuTree.EnumerateTree().FirstOrDefault(x => x.Value == attributeType);
                if (item != null)
                {
                    w.MenuTree.Selection.Clear();
                    w.MenuTree.Selection.Add(item);
                }
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;
            tree.Selection.SelectionChanged += this.SelectionChanged;
            tree.Config.DrawSearchToolbar = true;
            tree.Config.DefaultMenuStyle.Height = 22;

            AttributeExampleUtilities.BuildMenuTree(tree);

            return tree;
        }

        private void SelectionChanged(SelectionChangedType obj)
        {
            if (this.example != null)
            {
                this.example.OnDeselected();
                this.example = null;
            }

            var attr = this.MenuTree.Selection.Select(i => i.Value).FilterCast<Type>().FirstOrDefault();
            if (attr != null)
            {
                this.example = AttributeExampleUtilities.GetExample(attr);
            }
        }

        protected override void DrawEditors()
        {
            GUILayout.BeginArea(new Rect(4, 0, Mathf.Max(300, this.position.width - this.MenuWidth - 4), this.position.height));
            this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, GUILayoutOptions.ExpandWidth(false));
            GUILayout.Space(4);

            if (this.example != null)
            {
                this.example.Draw();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void OnDisable()
        {
            if (this.example != null)
            {
                this.example.OnDeselected();
                this.example = null;
            }
        }

        protected override void OnDestroy()
        {
            if (this.example != null)
            {
                this.example.OnDeselected();
                this.example = null;
            }
        }
    }
}
#endif