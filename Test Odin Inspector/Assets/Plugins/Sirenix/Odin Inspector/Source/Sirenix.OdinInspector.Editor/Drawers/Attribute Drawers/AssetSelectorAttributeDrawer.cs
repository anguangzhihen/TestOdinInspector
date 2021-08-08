#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetSelectorAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [DrawerPriority(0, 0, 2002)]
    public sealed class AssetSelectorAttributeDrawer : OdinAttributeDrawer<AssetSelectorAttribute>
    {
        private GUIContent label;
        private bool isList;
        private bool isListElement;
        private Func<IEnumerable<ValueDropdownItem>> getValues;
        private Func<IEnumerable<object>> getSelection;
        private Type elementOrBaseType;
        private bool isString;
        private IEnumerable<object> result;
        private bool enableMultiSelect;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.isList = this.Property.ChildResolver as IOrderedCollectionResolver != null;
            this.isListElement = this.Property.Info.GetMemberInfo() == null;
            this.getSelection = () => this.Property.ValueEntry.WeakValues.Cast<object>();
            this.elementOrBaseType = this.isList ?
                (this.Property.ChildResolver as IOrderedCollectionResolver).ElementType :
                this.Property.ValueEntry.BaseValueType;
            this.isString = this.elementOrBaseType == typeof(string);

            this.getValues = () =>
            {
                var filter = this.Attribute.Filter ?? "";

                if (string.IsNullOrEmpty(filter) && !typeof(Component).IsAssignableFrom(this.elementOrBaseType) && !this.elementOrBaseType.IsInterface)
                {
                    filter = "t:" + this.elementOrBaseType.Name;
                }

                var assetGuids = AssetDatabase.FindAssets(filter, this.Attribute.SearchInFolders ?? new string[0]);

                return assetGuids
                    .Select(x => AssetDatabase.GUIDToAssetPath(x))
                    .Distinct()
                    .SelectMany(x =>
                    {
                        IEnumerable<UnityEngine.Object> objs;
                        if (x.EndsWith(".unity", StringComparison.InvariantCultureIgnoreCase))
                        {
                            objs = Enumerable.Repeat(AssetDatabase.LoadAssetAtPath(x, typeof(UnityEngine.Object)), 1);
                        }
                        else
                        {
                            objs = AssetDatabase.LoadAllAssetsAtPath(x);
                        }

                        return objs.Where(obj => obj != null && this.elementOrBaseType.IsAssignableFrom(obj.GetType()))
                            .Select(obj => new { o = obj, p = x });
                    })
                    .Select(x => new ValueDropdownItem()
                    {
                        Text = x.p + (AssetDatabase.IsMainAsset(x.o) ? "" : "/" + x.o.name),
                        Value = this.isString ? (object)x.p : x.o
                    });
            };
        }

        private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query)
        {
            return query.Select(x =>
            {
                if (x is ValueDropdownItem)
                {
                    return (ValueDropdownItem)x;
                }

                if (x is IValueDropdownItem)
                {
                    var ix = x as IValueDropdownItem;
                    return new ValueDropdownItem(ix.GetText(), ix.GetValue());
                }

                return new ValueDropdownItem(null, x);
            });
        }

        /// <summary>
        /// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.label = label;

            if (this.Property.ValueEntry == null)
            {
                this.CallNextDrawer(label);
                return;
            }

            if (this.isList)
            {
                if (this.Attribute.DisableListAddButtonBehaviour)
                {
                    this.CallNextDrawer(label);
                }
                else
                {
                    CollectionDrawerStaticInfo.NextCustomAddFunction = this.OpenSelector;
                    this.CallNextDrawer(label);
                    if (this.result != null)
                    {
                        this.AddResult(this.result);
                        this.result = null;
                    }
                }
            }
            else
            {
                if (this.Attribute.DrawDropdownForListElements || !this.isListElement)
                {
                    this.DrawDropdown();
                }
                else
                {
                    this.CallNextDrawer(label);
                }
            }
        }

        private void AddResult(IEnumerable<object> query)
        {
            if (this.isList)
            {
                var changer = this.Property.ChildResolver as IOrderedCollectionResolver;

                if (this.enableMultiSelect)
                {
                    changer.QueueClear();
                }

                foreach (var item in query)
                {
                    object[] arr = new object[this.Property.ParentValues.Count];

                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = item;
                    }

                    changer.QueueAdd(arr);
                }
            }
            else
            {
                var first = query.FirstOrDefault();
                for (int i = 0; i < this.Property.ValueEntry.WeakValues.Count; i++)
                {
                    this.Property.ValueEntry.WeakValues[i] = first;
                }
            }
        }

        private void DrawDropdown()
        {
            IEnumerable<object> newResult = null;
            if (!this.isList)
            {
                GUILayout.BeginHorizontal();
                {
                    var width = 15f;
                    if (this.label != null)
                    {
                        width += GUIHelper.BetterLabelWidth;
                    }

                    newResult = GenericSelector<object>.DrawSelectorDropdown(this.label, GUIContent.none, this.ShowSelector, GUIStyle.none, GUILayoutOptions.Width(width));
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
            else
            {
                string valueName = this.GetCurrentValueName();
                newResult = GenericSelector<object>.DrawSelectorDropdown(this.label, valueName, this.ShowSelector);
            }

            if (newResult != null && newResult.Any())
            {
                this.AddResult(newResult);
            }
        }

        private void OpenSelector()
        {
            var rect = new Rect(Event.current.mousePosition, Vector2.zero);
            var selector = this.ShowSelector(rect);
            selector.SelectionConfirmed += x => this.result = x;
        }

        private OdinSelector<object> ShowSelector(Rect rect)
        {
            var selector = this.CreateSelector();

            rect.x = (int)rect.x;
            rect.y = (int)rect.y;
            rect.width = (int)rect.width;
            rect.height = (int)rect.height;

            if (!this.isList)
            {
                rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
            }

            selector.ShowInPopup(rect, new Vector2(this.Attribute.DropdownWidth, this.Attribute.DropdownHeight));
            return selector;
        }

        private GenericSelector<object> CreateSelector()
        {
            // TODO: Attribute is now cached, could that become a problem here?
            this.Attribute.IsUniqueList = this.Attribute.IsUniqueList || this.Attribute.ExcludeExistingValuesInList;
            var query = this.getValues() ?? Enumerable.Empty<ValueDropdownItem>();

            var isEmpty = query.Any() == false;

            if (!isEmpty)
            {
                if (this.isList && this.Attribute.ExcludeExistingValuesInList || (this.isListElement && this.Attribute.IsUniqueList))
                {
                    var list = query.ToList();
                    var listProperty = this.Property.FindParent(x => (x.ChildResolver as IOrderedCollectionResolver) != null, true);
                    var comparer = new IValueDropdownEqualityComparer(false);

                    listProperty.ValueEntry.WeakValues.Cast<IEnumerable>()
                        .SelectMany(x => x.Cast<object>())
                        .ForEach(x =>
                        {
                            list.RemoveAll(c => comparer.Equals(c, x));
                        });

                    query = list;
                }
            }

            var selector = new GenericSelector<object>(this.Attribute.DropdownTitle, false, query.Select(x => new GenericSelectorItem<object>(x.Text, x.Value)));

            this.enableMultiSelect = this.isList && this.Attribute.IsUniqueList && !this.Attribute.ExcludeExistingValuesInList;

            if (this.Attribute.FlattenTreeView)
            {
                selector.FlattenedTree = true;
            }

            if (this.isList && !this.Attribute.ExcludeExistingValuesInList && this.Attribute.IsUniqueList)
            {
                selector.CheckboxToggle = true;
            }
            else if (!this.enableMultiSelect)
            {
                selector.EnableSingleClickToSelect();
            }

            if (this.isList && this.enableMultiSelect)
            {
                selector.SelectionTree.Selection.SupportsMultiSelect = true;
                selector.DrawConfirmSelectionButton = true;
            }

            selector.SelectionTree.Config.DrawSearchToolbar = true;

            var selection = Enumerable.Empty<object>();

            if (!this.isList)
            {
                selection = this.getSelection();
            }
            else if (this.enableMultiSelect)
            {
                selection = this.getSelection().SelectMany(x => (x as IEnumerable).Cast<object>());
            }

            selector.SetSelection(selection);
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);

            if (this.Attribute.ExpandAllMenuItems)
            {
                selector.SelectionTree.EnumerateTree(x => x.Toggled = true);
            }

            return selector;
        }

        private string GetCurrentValueName()
        {
            if (!EditorGUI.showMixedValue)
            {
                var weakValue = this.Property.ValueEntry.WeakSmartValue;
                return weakValue + "";
            }
            else
            {
                return SirenixEditorGUI.MixedValueDashChar;
            }
        }
    }
}
#endif