#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ChildGameObjectsOnlyAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using System.Linq;
    using Utilities;
    using Utilities.Editor;
    using UnityEngine;

    public class ChildGameObjectsOnlyAttributeDrawer<T> : OdinAttributeDrawer<ChildGameObjectsOnlyAttribute, T>
        where T : class
    {
        private bool isValidValues;
        private bool rootIsComponent;
        private int rootCount;
        private bool isList;

        protected override void Initialize()
        {
            var root = this.GetRoot(0);
            this.rootIsComponent = root is Component;
            this.rootCount = this.Property.SerializationRoot.BaseValueEntry.WeakValues.Count;
            this.Property.ValueEntry.OnValueChanged += x => this.ValidateCurrentValue();
            this.isList = (this.Property.ChildResolver is ICollectionResolver);

            if (this.rootIsComponent)
            {
                this.ValidateCurrentValue();
            }
        }

        private Transform GetRoot(int index)
        {
            // If the user should have the ability to provide an alternative 
            // root component via the attribute, then support for that can be added here.
            var parentValues = this.Property.SerializationRoot.BaseValueEntry.WeakValues;
            var root = parentValues[index] as Component;
            if (root)
            {
                return root.transform;
            }
            return null;
        }

        private void ValidateCurrentValue()
        {
            var entry = this.ValueEntry;

            this.isValidValues = true;
            if (entry.SmartValue as UnityEngine.Object)
            {
                for (int i = 0; i < this.rootCount; i++)
                {
                    var root = this.GetRoot(i);
                    var uObj = this.ValueEntry.Values[i] as UnityEngine.Object;
                    if (!uObj)
                    {
                        continue;
                    }

                    var component = uObj as Component;
                    var go = uObj as GameObject;
                    if (go) component = go.transform;
                    if (!component)
                    {
                        this.isValidValues = false;
                        return;
                    }

                    var transform = component.transform;
                    if (!this.Attribute.IncludeSelf && transform == root)
                    {
                        this.isValidValues = false;
                        return;
                    }

                    if (IsRootOf(root, transform) == false)
                    {
                        this.isValidValues = false;
                        return;
                    }
                }
            }
        }

        private string GetGameObjectPath(Transform root, Transform child)
        {
            if (root == child)
            {
                return root.name;
            }

            var path = "";
            var curr = child;

            while (curr)
            {
                if (!this.Attribute.IncludeSelf && curr == root)
                {
                    return path.Trim('/');
                }

                path = curr.name + "/" + path;

                if (this.Attribute.IncludeSelf && curr == root)
                {
                    return path.Trim('/');
                }


                curr = curr.parent;
            }

            return null;
        }

        private static bool IsRootOf(Transform root, Transform child)
        {
            var curr = child;
            while (curr)
            {
                if (curr == root)
                {
                    return true;
                }
                curr = curr.parent;
            }

            return false;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!this.rootIsComponent)
            {
                // Serialization root is not a Component, so it has no children.
                // Should we display a warning or an error here?
                this.CallNextDrawer(label);
                return;
            }

            if (this.rootCount > 1)
            {
                // TODO: Add support for multi-selection to the child-selector dropdown.
                this.CallNextDrawer(label);
                return;
            }

            if (this.isList)
            {
                var prev = CollectionDrawerStaticInfo.NextCustomAddFunction;
                CollectionDrawerStaticInfo.NextCustomAddFunction = ListAddButton;
                this.CallNextDrawer(label);
                CollectionDrawerStaticInfo.NextCustomAddFunction = prev;
                return;
            }

            // If we want it to validate the value each frame.
            // if (Event.current.type == EventType.Layout)
            // {
            //     this.ValidateCurrentValue();
            // }

            if (!this.isValidValues)
            {
                SirenixEditorGUI.ErrorMessageBox("The object must be a child of the selected GameObject.");
            }

            GUILayout.BeginHorizontal();
            {
                var width = 15f;
                if (label != null)
                {
                    width += GUIHelper.BetterLabelWidth;
                }

                var newResult = GenericSelector<UnityEngine.Object>.DrawSelectorDropdown(label, GUIContent.none, this.ShowSelector, GUIStyle.none, GUILayoutOptions.Width(width));
                if (newResult != null && newResult.Any())
                {
                    this.ValueEntry.SmartValue = newResult.FirstOrDefault() as T;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    var btnRect = GUILayoutUtility.GetLastRect().AlignRight(15);
                    btnRect.y += 4;
                    SirenixGUIStyles.PaneOptions.Draw(btnRect, GUIContent.none, 0);
                }

                GUILayout.BeginVertical();
                this.CallNextDrawer(null);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void ListAddButton()
        {
            var selector = this.ShowSelector(new Rect());
            selector.SelectionConfirmed += (x) =>
            {
                var resolver = this.Property.ChildResolver as ICollectionResolver;
                resolver.QueueAdd(new object[] { x.FirstOrDefault() });
            };
        }

        private OdinSelector<UnityEngine.Object> ShowSelector(Rect rect)
        {
            var selector = this.CreateSelector();
            if (rect == new Rect())
            {
                rect = new Rect(Event.current.mousePosition, Vector2.zero);
                rect.x = (int)rect.x;
                rect.y = (int)rect.y;
                rect.width = (int)rect.width;
                rect.height = (int)rect.height;
                if (!this.isList)
                {
                    rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
                }
                selector.ShowInPopup(rect, new Vector2(0, 0));
            }
            else
            {
                rect.x = (int)rect.x;
                rect.y = (int)rect.y;
                rect.width = (int)rect.width;
                rect.height = (int)rect.height;
                rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
                selector.ShowInPopup(rect);
            }

            return selector;
        }

        private GenericSelector<UnityEngine.Object> CreateSelector()
        {
            var t = isList ? (this.Property.ChildResolver as ICollectionResolver).ElementType : typeof(T);
            var isGo = t == typeof(GameObject);
            var root = this.GetRoot(0);

            var children = root.GetComponentsInChildren(isGo ? typeof(Transform) : t)
                .Where(x => this.Attribute.IncludeSelf || x.transform != root)
                .OfType<UnityEngine.Object>();

            if (isGo)
            {
                children = children.OfType<Component>().Select(x => x.gameObject).OfType<UnityEngine.Object>();
            }

            Func<UnityEngine.Object, string> getName = x =>
            {
                var c = x as Component;
                var o = x as GameObject;
                var transform = c ? c.transform : o.transform;
                return this.GetGameObjectPath(root, transform);
            };

            var selector = new GenericSelector<UnityEngine.Object>(
                null, false, getName, children.Where(x => x.GetType().InheritsFrom(t)));
            selector.SelectionTree.Config.DrawSearchToolbar = true;
            selector.SetSelection(this.ValueEntry.SmartValue as UnityEngine.Object);
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);
            selector.SelectionTree.EnumerateTree().Where(x => x.Icon == null).ForEach(x => x.Icon = EditorIcons.UnityGameObjectIcon);
            selector.SelectionTree.EnumerateTree().ForEach(x => x.Toggled = true);
            selector.EnableSingleClickToSelect();

            return selector;
        }
    }
}
#endif