#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValueDropdownAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="ValueDropdownAttribute"/>.
    /// </summary>
    /// <seealso cref="ValueDropdownAttribute"/>
    /// <seealso cref="ValueDropdownItem{T}"/>
    /// <summary>
    /// Draws the property.
    /// </summary>
    [DrawerPriority(0, 0, 2002)]
    public sealed class ValueDropdownAttributeDrawer : OdinAttributeDrawer<ValueDropdownAttribute>
    {
        private string error;
        private GUIContent label;
        private bool isList;
        private bool isListElement;
        private Func<IEnumerable<ValueDropdownItem>> getValues;
        private Func<IEnumerable<object>> getSelection;
        private IEnumerable<object> result;
        private bool enableMultiSelect;
        private Dictionary<object, string> nameLookup;
        private ValueResolver<object> rawGetter;
        private LocalPersistentContext<bool> isToggled;
        //private object rawPrevGettedValue = null;
        //private int rawPrevGettedValueCount = 0;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.rawGetter = ValueResolver.Get<object>(this.Property, this.Attribute.ValuesGetter);
            this.isToggled = this.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);

            this.error = this.rawGetter.ErrorMessage;
            this.isList = this.Property.ChildResolver is ICollectionResolver;
            this.isListElement = this.Property.Parent != null && this.Property.Parent.ChildResolver is ICollectionResolver;
            this.getSelection = () => this.Property.ValueEntry.WeakValues.Cast<object>();
            this.getValues = () =>
            {
                var value = this.rawGetter.GetValue();

                return value == null ? null : (value as IEnumerable)
                    .Cast<object>()
                    .Where(x => x != null)
                    .Select(x =>
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
            };

            this.ReloadDropdownCollections();
        }

        private void ReloadDropdownCollections()
        {
            if (this.error != null)
            {
                return;
            }

            object first = null;
            var value = this.rawGetter.GetValue();
            if (value != null)
            {
                first = (value as IEnumerable).Cast<object>().FirstOrDefault();
            }

            var isNamedValueDropdownItems = first is IValueDropdownItem;

            if (isNamedValueDropdownItems)
            {
                var vals = this.getValues();
                this.nameLookup = new Dictionary<object, string>(new IValueDropdownEqualityComparer(false));
                foreach (var item in vals)
                {
                    nameLookup[item] = item.Text;
                }
            }
            else
            {
                this.nameLookup = null;
            }
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

            if (this.error != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.error);
                this.CallNextDrawer(label);
            }
            else if (this.isList)
            {
                if (this.Attribute.DisableListAddButtonBehaviour)
                {
                    this.CallNextDrawer(label);
                }
                else
                {
                    var oldSelector = CollectionDrawerStaticInfo.NextCustomAddFunction;
                    CollectionDrawerStaticInfo.NextCustomAddFunction = this.OpenSelector;
                    this.CallNextDrawer(label);
                    if (this.result != null)
                    {
                        this.AddResult(this.result);
                        this.result = null;
                    }
                    CollectionDrawerStaticInfo.NextCustomAddFunction = oldSelector;
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
                var changer = this.Property.ChildResolver as ICollectionResolver;

                if (this.enableMultiSelect)
                {
                    changer.QueueClear();
                }

                foreach (var item in query)
                {
                    object[] arr = new object[this.Property.ParentValues.Count];

                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = SerializationUtility.CreateCopy(item);
                    }

                    changer.QueueAdd(arr);
                }
            }
            else
            {
                var first = query.FirstOrDefault();
                for (int i = 0; i < this.Property.ValueEntry.WeakValues.Count; i++)
                {
                    this.Property.ValueEntry.WeakValues[i] = SerializationUtility.CreateCopy(first);
                }
            }
        }

        private void DrawDropdown()
        {
            IEnumerable<object> newResult = null;

            //if (this.Attribute.InlineSelector)
            //{
            //    bool recreateBecauseOfListChange = false;

            //    if (Event.current.type == EventType.Layout)
            //    {
            //        var _newCol = this.rawGetter.GetValue();
            //        if (_newCol != this.rawPrevGettedValue)
            //        {
            //            this.ReloadDropdownCollections();
            //            recreateBecauseOfListChange = true;
            //        }

            //        var iList = _newCol as IList;
            //        if (iList != null)
            //        {
            //            if (iList.Count != this.rawPrevGettedValueCount)
            //            {
            //                this.ReloadDropdownCollections();
            //                recreateBecauseOfListChange = true;
            //            }

            //            this.rawPrevGettedValueCount = iList.Count;
            //        }

            //        this.rawPrevGettedValue = _newCol;
            //    }

            //    if (this.inlineSelector == null || recreateBecauseOfListChange)
            //    {
            //        this.inlineSelector = this.CreateSelector();
            //        this.inlineSelector.SelectionChanged += (x) =>
            //        {
            //            this.nextResult = x;
            //        };
            //    }

            //    this.inlineSelector.OnInspectorGUI();

            //    if (this.nextResult != null)
            //    {
            //        newResult = this.nextResult;
            //        this.nextResult = null;
            //    }
            //}
            //else if (this.Attribute.AppendNextDrawer && !this.isList)
            if (this.Attribute.AppendNextDrawer && !this.isList)
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
                    bool disable = this.Attribute.DisableGUIInAppendedDrawer;
                    if (disable) GUIHelper.PushGUIEnabled(false);
                    this.CallNextDrawer(null);
                    if (disable) GUIHelper.PopGUIEnabled();
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                string valueName = GetCurrentValueName();

                if (this.Attribute.HideChildProperties == false && this.Property.Children.Count > 0)
                {
                    Rect valRect;
                    this.isToggled.Value = SirenixEditorGUI.Foldout(this.isToggled.Value, this.label, out valRect);
                    newResult = GenericSelector<object>.DrawSelectorDropdown(valRect, valueName, this.ShowSelector);

                    if (SirenixEditorGUI.BeginFadeGroup(this, this.isToggled.Value))
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < this.Property.Children.Count; i++)
                        {
                            var child = this.Property.Children[i];
                            child.Draw(child.Label);
                        }
                        EditorGUI.indentLevel--;
                    }
                    SirenixEditorGUI.EndFadeGroup();
                }
                else
                {
                    newResult = GenericSelector<object>.DrawSelectorDropdown(this.label, valueName, this.ShowSelector);
                }
            }

            if (newResult != null && newResult.Any())
            {
                this.AddResult(newResult);
            }
        }

        private void OpenSelector()
        {
            this.ReloadDropdownCollections();
            var rect = new Rect(Event.current.mousePosition, Vector2.zero);
            var selector = ShowSelector(rect);
            selector.SelectionConfirmed += x => result = x;
        }

        private OdinSelector<object> ShowSelector(Rect rect)
        {
            var selector = CreateSelector();

            rect.x = (int)rect.x;
            rect.y = (int)rect.y;
            rect.width = (int)rect.width;
            rect.height = (int)rect.height;

            if (this.Attribute.AppendNextDrawer && !this.isList)
            {
                rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
            }

            selector.ShowInPopup(rect, new Vector2(this.Attribute.DropdownWidth, this.Attribute.DropdownHeight));
            return selector;
        }

        private GenericSelector<object> CreateSelector()
        {
            // TODO: Attribute is now cached, could that become a problem here?
            this.Attribute.IsUniqueList = (this.Property.ChildResolver is IOrderedCollectionResolver) == false || (this.Attribute.IsUniqueList || this.Attribute.ExcludeExistingValuesInList);
            var query = this.getValues() ?? Enumerable.Empty<ValueDropdownItem>();

            var isEmpty = query.Any() == false;

            if (!isEmpty)
            {
                if (this.isList && this.Attribute.ExcludeExistingValuesInList || (this.isListElement && this.Attribute.IsUniqueList))
                {
                    var list = query.ToList();
                    var listProperty = this.Property.FindParent(x => (x.ChildResolver as ICollectionResolver) != null, true);
                    var comparer = new IValueDropdownEqualityComparer(false);

                    listProperty.ValueEntry.WeakValues.Cast<IEnumerable>()
                        .SelectMany(x => x.Cast<object>())
                        .ForEach(x =>
                        {
                            list.RemoveAll(c => comparer.Equals(c, x));
                        });

                    query = list;
                }

                // Update item names in the look up table in case the collection has changed.
                if (this.nameLookup != null)
                {
                    foreach (var item in query)
                    {
                        if (item.Value != null)
                        {
                            this.nameLookup[item.Value] = item.Text;
                        }
                    }
                }
            }

            var enableSearch = this.Attribute.NumberOfItemsBeforeEnablingSearch == 0 || (query != null && query.Take(this.Attribute.NumberOfItemsBeforeEnablingSearch).Count() == this.Attribute.NumberOfItemsBeforeEnablingSearch);

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
            else if (this.Attribute.DoubleClickToConfirm == false && !enableMultiSelect)
            {
                selector.EnableSingleClickToSelect();
            }

            if (this.isList && enableMultiSelect)
            {
                selector.SelectionTree.Selection.SupportsMultiSelect = true;
                selector.DrawConfirmSelectionButton = true;
            }

            selector.SelectionTree.Config.DrawSearchToolbar = enableSearch;

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

            if (this.Attribute.SortDropdownItems)
            {
                selector.SelectionTree.SortMenuItemsByName();
            }

            return selector;
        }

        private string GetCurrentValueName()
        {
            if (!EditorGUI.showMixedValue)
            {
                var weakValue = this.Property.ValueEntry.WeakSmartValue;

                string name = null;
                if (this.nameLookup != null && weakValue != null)
                {
                    this.nameLookup.TryGetValue(weakValue, out name);
                }

                return new GenericSelectorItem<object>(name, weakValue).GetNiceName();
            }
            else
            {
                return SirenixEditorGUI.MixedValueDashChar;
            }
        }
    }

    internal class IValueDropdownEqualityComparer : IEqualityComparer<object>
    {
        private bool isTypeLookup;

        public IValueDropdownEqualityComparer(bool isTypeLookup)
        {
            this.isTypeLookup = isTypeLookup;
        }

        public new bool Equals(object x, object y)
        {
            if (x is ValueDropdownItem)
            {
                x = ((ValueDropdownItem)x).Value;
            }

            if (y is ValueDropdownItem)
            {
                y = ((ValueDropdownItem)y).Value;
            }

            if (EqualityComparer<object>.Default.Equals(x, y))
            {
                return true;
            }

            if ((x == null) != (y == null))
            {
                return false;
            }

            if (this.isTypeLookup)
            {
                var tx = x as Type ?? x.GetType();
                var ty = y as Type ?? y.GetType();

                if (tx == ty)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return -1;
            }

            if (obj is ValueDropdownItem)
            {
                obj = ((ValueDropdownItem)obj).Value;
            }

            if (obj == null)
            {
                return -1;
            }

            if (this.isTypeLookup)
            {
                var t = obj as Type ?? obj.GetType();
                return t.GetHashCode();
            }
            else
            {
                return obj.GetHashCode();
            }
        }
    }
}
#endif