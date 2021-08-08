#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NullableReferenceDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Utilities;
    using System.Collections;
    using Sirenix.Serialization;
    using System;

    /// <summary>
    /// Draws all nullable reference types, with an object field.
    /// </summary>
    [AllowGUIEnabledForReadonly]
    [DrawerPriority(0, 0, 2000)]
    public sealed class NullableReferenceDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
    {
        private bool shouldDrawReferencePicker;
        private bool drawUnityObject;
        private bool allowSceneObjects;
        private OdinDrawer[] bakedDrawerArray;
        private InlinePropertyAttribute inlinePropertyAttr;
        private bool drawChildren;

        private PropertySearchFilter searchFilter;
        private string searchFieldControlName;

        protected override void Initialize()
        {
            var searchAttr = this.Property.GetAttribute<SearchableAttribute>();

            if (searchAttr != null)
            {
                this.searchFilter = new PropertySearchFilter(this.Property, searchAttr);
                this.searchFieldControlName = "PropertyTreeSearchField_" + Guid.NewGuid().ToString();
            }

            this.drawUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.TypeOfValue);
            this.allowSceneObjects = this.Property.GetAttribute<AssetsOnlyAttribute>() == null;
            this.bakedDrawerArray = this.Property.GetActiveDrawerChain().BakedDrawerArray;
            this.inlinePropertyAttr = this.Property.Attributes.GetAttribute<InlinePropertyAttribute>();
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            if (Event.current.type == EventType.Layout)
            {
                this.shouldDrawReferencePicker = ShouldDrawReferenceObjectPicker(this.ValueEntry);

                if (this.Property.Children.Count > 0)
                {
                    this.drawChildren = true;
                }
                else if (this.ValueEntry.ValueState != PropertyValueState.None)
                {
                    this.drawChildren = false;
                }
                else
                {
                    // Weird case: This prevents a foldout from being drawn that expands nothing.
                    // If we're the second last drawer, then the next drawer is most likely 
                    // the composite drawer. And since we don't have any children in this
                    // else statement, we don't have anything else to draw.
                    this.drawChildren = this.bakedDrawerArray[this.bakedDrawerArray.Length - 2] != this;
                }
            }

            if (entry.ValueState == PropertyValueState.NullReference)
            {
                if (this.drawUnityObject)
                {
                    this.CallNextDrawer(label);
                }
                else
                {
                    if (!entry.SerializationBackend.SupportsPolymorphism && entry.IsEditable)
                    {
                        SirenixEditorGUI.ErrorMessageBox("Unity-backed value is null. This should already be fixed by the FixUnityNullDrawer! It is likely that this type has been incorrectly guessed by Odin to be serialized by Unity when it is actually not. Please create an issue on Odin's issue tracker stating how to reproduce this error message.");
                    }

                    this.DrawField(label);
                }
            }
            else
            {
                if (this.shouldDrawReferencePicker)
                {
                    this.DrawField(label);
                }
                else
                {
                    this.CallNextDrawer(label);
                }
            }

            var objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
            if (objectPicker.IsReadyToClaim)
            {
                var obj = objectPicker.ClaimObject();
                entry.Property.Tree.DelayActionUntilRepaint(() =>
                {
                    entry.WeakValues[0] = obj;
                    for (int j = 1; j < entry.ValueCount; j++)
                    {
                        entry.WeakValues[j] = SerializationUtility.CreateCopy(obj);
                    }
                });
            }
        }

        private void DrawField(GUIContent label)
        {
            if (this.inlinePropertyAttr != null)
            {
                var pushLabelWidth = this.inlinePropertyAttr.LabelWidth > 0;
                if (label == null)
                {
                    if (pushLabelWidth) GUIHelper.PushLabelWidth(this.inlinePropertyAttr.LabelWidth);
                    this.DrawInlinePropertyReferencePicker();
                    this.CallNextDrawer(null);
                    if (pushLabelWidth) GUIHelper.PopLabelWidth();
                }
                else
                {
                    SirenixEditorGUI.BeginVerticalPropertyLayout(label);
                    this.DrawInlinePropertyReferencePicker();
                    if (pushLabelWidth) GUIHelper.PushLabelWidth(this.inlinePropertyAttr.LabelWidth);
                    for (int i = 0; i < this.Property.Children.Count; i++)
                    {
                        var child = this.Property.Children[i];
                        child.Draw(child.Label);
                    }
                    if (pushLabelWidth) GUIHelper.PopLabelWidth();
                    SirenixEditorGUI.EndVerticalPropertyLayout();
                }
            }
            else
            {
                Rect valueRect;
                bool hasKeyboardFocus;
                int id;
                var rect = SirenixEditorGUI.GetFeatureRichControlRect(null, out id, out hasKeyboardFocus, out valueRect);

                if (label != null)
                {
                    rect.width = GUIHelper.BetterLabelWidth;
                    valueRect.xMin = rect.xMax;

                    this.DrawSearchFilter(rect);

                    if (this.drawChildren)
                    {
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(rect, this.Property.State.Expanded, label);
                    }
                    else
                    {
                        rect = EditorGUI.IndentedRect(rect);
                        GUI.Label(rect, label);
                    }
                }
                else if (this.drawChildren)
                {
                    if (EditorGUIUtility.hierarchyMode)
                    {
                        rect.width = 18;
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(rect, this.Property.State.Expanded, GUIContent.none);
                    }
                    else
                    {
                        rect.width = 18;
                        valueRect.xMin = rect.xMax;
                        var preev = EditorGUIUtility.hierarchyMode;
                        EditorGUIUtility.hierarchyMode = false;
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(rect, this.Property.State.Expanded, GUIContent.none);
                        EditorGUIUtility.hierarchyMode = preev;
                    }
                }
                
                EditorGUI.BeginChangeCheck();
                var prev = EditorGUI.showMixedValue;
                if (this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
                {
                    EditorGUI.showMixedValue = true;
                }
                var newValue = SirenixEditorFields.PolymorphicObjectField(valueRect, this.ValueEntry.WeakSmartValue, this.ValueEntry.BaseValueType, this.allowSceneObjects, hasKeyboardFocus, id);
                EditorGUI.showMixedValue = prev;

                if (EditorGUI.EndChangeCheck())
                {
                    this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
                    {
                        this.ValueEntry.WeakValues[0] = newValue;
                        for (int j = 1; j < this.ValueEntry.ValueCount; j++)
                        {
                            this.ValueEntry.WeakValues[j] = SerializationUtility.CreateCopy(newValue);
                        }
                    });
                }

                if (this.drawChildren)
                {
                    var toggle = this.ValueEntry.ValueState == PropertyValueState.NullReference ? false : this.Property.State.Expanded;
                    if (SirenixEditorGUI.BeginFadeGroup(this, toggle))
                    {
                        EditorGUI.indentLevel++;

                        if (this.searchFilter != null && this.searchFilter.HasSearchResults)
                        {
                            this.searchFilter.DrawSearchResults();
                        }
                        else
                        {
                            this.CallNextDrawer(null);
                        }

                        EditorGUI.indentLevel--;
                    }
                    SirenixEditorGUI.EndFadeGroup();
                }
            }
        }

        private void DrawSearchFilter(Rect labelRect)
        {
            if (this.searchFilter != null)
            {
                Rect searchRect = new Rect(
                    labelRect.xMin + labelRect.width * 0.5f,
                    labelRect.y + 1,
                    (labelRect.width * 0.5f) - 5f,
                    labelRect.height);

                var newTerm = SirenixEditorGUI.SearchField(searchRect, this.searchFilter.SearchTerm, false, this.searchFieldControlName);

                if (newTerm != this.searchFilter.SearchTerm)
                {
                    this.searchFilter.SearchTerm = newTerm;
                    this.Property.Tree.DelayActionUntilRepaint(() =>
                    {
                        if (!string.IsNullOrEmpty(newTerm))
                        {
                            this.Property.State.Expanded = true;
                        }

                        this.searchFilter.UpdateSearch();
                        GUIHelper.RequestRepaint();
                    });
                }
            }
        }

        private void DrawInlinePropertyReferencePicker()
        {
            EditorGUI.BeginChangeCheck();
            var prev = EditorGUI.showMixedValue;
            if (this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
            {
                EditorGUI.showMixedValue = true;
            }
            var newValue = SirenixEditorFields.PolymorphicObjectField(this.ValueEntry.WeakSmartValue, this.ValueEntry.BaseValueType, this.allowSceneObjects);
            EditorGUI.showMixedValue = prev;

            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
                {
                    this.ValueEntry.WeakValues[0] = newValue;
                    for (int j = 1; j < this.ValueEntry.ValueCount; j++)
                    {
                        this.ValueEntry.WeakValues[j] = SerializationUtility.CreateCopy(newValue);
                    }
                });
            }
        }

        private static bool ShouldDrawReferenceObjectPicker(IPropertyValueEntry<T> entry)
        {
            return entry.SerializationBackend.SupportsPolymorphism
                && !entry.BaseValueType.IsValueType
                && entry.BaseValueType != typeof(string)
                && !(entry.Property.ChildResolver is ICollectionResolver)
                && !entry.BaseValueType.IsArray
                && entry.IsEditable
                && (!(typeof(UnityEngine.Object).IsAssignableFrom(entry.TypeOfValue) && !entry.BaseValueType.IsInterface))
                && !entry.BaseValueType.InheritsFrom(typeof(System.Collections.IDictionary))
                && !(entry.WeakSmartValue as UnityEngine.Object)
                && entry.Property.GetAttribute<HideReferenceObjectPickerAttribute>() == null;
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var entry = property.ValueEntry as IPropertyValueEntry<T>;
            bool isChangeable = property.ValueEntry.SerializationBackend.SupportsPolymorphism
                && !entry.BaseValueType.IsValueType
                && entry.BaseValueType != typeof(string);

            if (isChangeable)
            {
                if (entry.IsEditable)
                {
                    var objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
                    var rect = entry.Property.LastDrawnValueRect;
                    rect.position = GUIUtility.GUIToScreenPoint(rect.position);
                    rect.height = 20;
                    genericMenu.AddItem(new GUIContent("Change Type"), false, () =>
                    {
                        objectPicker.ShowObjectPicker(entry.WeakSmartValue, false, rect);
                    });
                }
                else
                {
                    genericMenu.AddDisabledItem(new GUIContent("Change Type"));
                }
            }
        }

        /// <summary>
        /// Returns a value that indicates if this drawer can be used for the given property.
        /// </summary>
        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            if (property.IsTreeRoot) return false;
            var type = property.ValueEntry.BaseValueType;
            return (type.IsClass || type.IsInterface) && type != typeof(string) && !typeof(UnityEngine.Object).IsAssignableFrom(type);
        }
    }
}
#endif