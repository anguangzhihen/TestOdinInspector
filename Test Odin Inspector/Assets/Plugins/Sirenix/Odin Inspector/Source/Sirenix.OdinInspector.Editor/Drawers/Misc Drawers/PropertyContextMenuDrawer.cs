#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyContextMenuDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Serialization;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;
    using System;
    using System.Collections;

    public static class PropertyContextMenuDrawer
    {
        public static class GenericMenuUtility
        {
            private static FieldInfo GenericMenu_menuItems_Field;

            public static readonly bool Available;

            static GenericMenuUtility()
            {
                GenericMenu_menuItems_Field = typeof(GenericMenu).GetField("menuItems", Flags.InstanceAnyVisibility);
            }

            public static ArrayList GetMenuItems(GenericMenu genericMenu)
            {
                throw new NotImplementedException();
            }
        }

        private static MethodInfo EditorGUI_FillPropertyContextMenu = typeof(EditorGUI).GetMethod("FillPropertyContextMenu", Flags.StaticAnyVisibility, null, new Type[] { typeof(SerializedProperty), typeof(SerializedProperty), typeof(GenericMenu) }, null);

        /// <summary>
        /// Adds the right click area.
        /// </summary>
        public static void AddRightClickArea(InspectorProperty property, Rect rect)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive);
            AddRightClickArea(property, rect, id);
        }

        /// <summary>
        /// Adds the right click area.
        /// </summary>
        public static void AddRightClickArea(InspectorProperty property, Rect rect, int id)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
            {
                // This check should be unnecessary now because the PropertyContextMenuDrawer should now be skipped.
                // Do not eat events or change hot ID if the context menu is disabled.
                //var disableAttr = property.GetAttribute<DisableContextMenuAttribute>();
                //if (disableAttr != null)
                //{
                //    if (property.ChildResolver is ICollectionResolver)
                //    {
                //        if (disableAttr.DisableForCollectionElements)
                //        {
                //            return;
                //        }
                //    }
                //    else if (disableAttr.DisableForMember)
                //    {
                //        return;
                //    }
                //}

                GUIUtility.hotControl = id;
                Event.current.Use();
                GUIHelper.RequestRepaint();
            }

            if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition) && id == GUIUtility.hotControl)
            {
                GUIHelper.RemoveFocusControl();
                Event.current.Use();

                var menu = new GenericMenu();
                GUIHelper.RemoveFocusControl();
                PopulateGenericMenu(property, menu);
                property.PopulateGenericMenu(menu);
                if (menu.GetItemCount() == 0)
                {
                    menu = null;
                }
                else { menu.ShowAsContext(); }
            }

            if (GUIUtility.hotControl == id && Event.current.type == EventType.Repaint)
            {
                rect.width = 3;
                rect.x -= 4;
                SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.HighlightedTextColor);
            }
        }

        public static GenericMenu FillUnityContextMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var unityProperty = property.Tree.GetUnityPropertyForPath(property.UnityPropertyPath);

            if (unityProperty == null)
            {
                return genericMenu ?? new GenericMenu();
            }

            return FillUnityContextMenu(unityProperty, genericMenu);
        }

        public static GenericMenu FillUnityContextMenu(SerializedProperty property, GenericMenu genericMenu = null)
        {
            if (EditorGUI_FillPropertyContextMenu != null)
            {
                return (GenericMenu)EditorGUI_FillPropertyContextMenu.Invoke(null, new object[] { property, null, genericMenu });
            }
            else
            {
                var handler = UnityPropertyHandlerUtility.ScriptAttributeUtility_GetHandler(property);

                if (handler != null)
                {
                    genericMenu = genericMenu ?? new GenericMenu();
                    UnityPropertyHandlerUtility.PropertyHandler_AddMenuItems(handler, property, genericMenu);
                }
            }

            return genericMenu ?? new GenericMenu();
        }

        private static void PopulateChangedFromPrefabContext(InspectorProperty property, GenericMenu genericMenu)
        {
            if (!property.Tree.PrefabModificationHandler.HasPrefabs) return;

            var entry = property.ValueEntry;

            if (entry != null)
            {
                InspectorProperty prefabProperty = null;

                if (property.Tree.PrefabModificationHandler.PrefabPropertyTree != null)
                {
                    prefabProperty = property.Tree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(property.Path);
                }

                bool active = prefabProperty != null;

                int moddedChildren = property.Children.Recurse().Count(c => c.ValueEntry != null && c.ValueEntry.ValueChangedFromPrefab);

                bool showApplyToPrefab = false;

                if (entry.ValueChangedFromPrefab || moddedChildren > 0)
                {
                    if (active)
                    {
                        genericMenu.AddItem(new GUIContent("Revert to prefab value" + (moddedChildren > 0 ? " (" + moddedChildren + " child modifications to revert)" : "")), false, () =>
                        {
                            property.RecordForUndo("Revert to prefab value");

                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.Value);
                            }

                            if (property.Tree.UnitySerializedObject != null)
                            {
                                property.Tree.UnitySerializedObject.Update();
                            }
                        });

                        showApplyToPrefab = true;
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Revert to prefab value (Does not exist on prefab)"));
                    }
                }

                if (entry.ListLengthChangedFromPrefab)
                {
                    if (active)
                    {
                        genericMenu.AddItem(new GUIContent("Revert to prefab list length"), false, () =>
                        {
                            property.RecordForUndo("Revert to prefab list length");

                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.ListLength);
                            }

                            property.Children.Update();

                            if (property.Tree.UnitySerializedObject != null)
                            {
                                property.Tree.UnitySerializedObject.Update();
                            }
                        });

                        showApplyToPrefab = true;
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Revert to prefab list length (Does not exist on prefab)"));
                    }
                }

                if (entry.DictionaryChangedFromPrefab)
                {
                    if (active)
                    {
                        genericMenu.AddItem(new GUIContent("Revert dictionary changes to prefab value"), false, () =>
                        {
                            property.RecordForUndo("Revert to prefab dictionary");

                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.Dictionary);
                            }

                            property.Children.Update();

                            if (property.Tree.UnitySerializedObject != null)
                            {
                                property.Tree.UnitySerializedObject.Update();
                            }
                        });

                        showApplyToPrefab = true;
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Revert dictionary changes to prefab value (Does not exist on prefab)"));
                    }
                }

                if (showApplyToPrefab)
                {
                    var applyText = "Apply value to prefab '" + (prefabProperty.Tree.WeakTargets[0] as UnityEngine.Object).name + "'";

                    genericMenu.AddItem(new GUIContent(applyText), false, () =>
                    {
                        bool overrideApplied = false;

                        var undoObjs = prefabProperty.SerializationRoot.ValueEntry.WeakValues.Cast<UnityEngine.Object>().AppendWith(
                                       property.SerializationRoot.ValueEntry.WeakValues.Cast<UnityEngine.Object>()).ToArray();

                        Undo.RecordObjects(undoObjs, applyText);

                        if (OdinPrefabSerializationEditorUtility.HasApplyPropertyOverride && property.ValueEntry.SerializationBackend.IsUnity && property.Tree.UnitySerializedObject != null)
                        {
                            SerializedProperty instanceProp = property.Tree.GetUnitySerializedObjectNoUpdate().FindProperty(property.UnityPropertyPath);

                            if (instanceProp != null)
                            {
                                var handler = property.Tree.PrefabModificationHandler;
                                for (int i = 0; i < handler.TargetPrefabs.Count; i++)
                                {
                                    var prefabPath = AssetDatabase.GetAssetPath(handler.TargetPrefabs[i]);
                                    OdinPrefabSerializationEditorUtility.ApplyPropertyOverride(instanceProp, prefabPath);
                                }
                                overrideApplied = true;
                            }
                        }

                        if (!overrideApplied)
                        {
                            var prefabEntry = prefabProperty.ValueEntry;

                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                var value = entry.WeakValues[i];
                                var copy = SerializationUtility.CreateCopy(value);
                                prefabEntry.WeakValues[i] = copy;
                            }

                            prefabEntry.ApplyChanges();

                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                if (entry.ValueChangedFromPrefab)
                                {
                                    property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.Value);
                                }

                                if (entry.ListLengthChangedFromPrefab)
                                {
                                    property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.ListLength);
                                }

                                if (entry.DictionaryChangedFromPrefab)
                                {
                                    property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.Dictionary);
                                }
                            }

                        }

                        if (property.Tree.UnitySerializedObject != null)
                        {
                            property.Tree.UnitySerializedObject.Update();
                        }

                        Undo.FlushUndoRecordObjects();
                    });
                }
            }
        }

        private class IndexPopupWindow
        {
            [HideInInspector]
            public string IntLabel;

            [HideInInspector]
            public int MaxCount;

            [LabelText("$IntLabel")]
            [MinValue(0)]
            [MaxValue("$MaxCount")]
            public int Value;

            [HideInInspector]
            public Action MoveAction;

            [HideInInspector]
            public Action CloseWindowAction;

            [HorizontalGroup("Buttons"), Button]
            public void Move()
            {
                this.MoveAction();
                this.CloseWindowAction();
            }

            [HorizontalGroup("Buttons"), Button]
            public void Cancel()
            {
                this.CloseWindowAction();
            }

            [OnInspectorGUI, PropertyOrder(-1)]
            private void DetectEnter()
            {
                if (Event.current.OnKeyUp(KeyCode.Return))
                {
                    this.Move();
                    GUIUtility.ExitGUI();
                }
            }
        }

        private static void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (Event.current.shift)
            {
                FillUnityContextMenu(property, genericMenu);
            }

            PopulateChangedFromPrefabContext(property, genericMenu);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }

            if (property.Parent != null && property.Parent.ChildResolver is IOrderedCollectionResolver)
            {
                var parentResolver = property.Parent.ChildResolver as IOrderedCollectionResolver;
                var parentListDrawerSettings = property.Parent.GetAttribute<ListDrawerSettingsAttribute>();

                bool listDrawerSettingsIsReadOnly = parentListDrawerSettings != null && parentListDrawerSettings.IsReadOnly;

                if (listDrawerSettingsIsReadOnly || parentResolver.IsReadOnly)
                {
                    genericMenu.AddDisabledItem(new GUIContent("Move element to top"));
                    genericMenu.AddDisabledItem(new GUIContent("Move element to bottom"));
                    genericMenu.AddDisabledItem(new GUIContent("Move element to index"));
                    genericMenu.AddDisabledItem(new GUIContent("Duplicate element"));
                    genericMenu.AddDisabledItem(new GUIContent("Insert pasted element"));
                    genericMenu.AddDisabledItem(new GUIContent("Insert new element"));
                    genericMenu.AddDisabledItem(new GUIContent("Delete element"));
                }
                else
                {
                    genericMenu.AddItem(new GUIContent("Move element to top"), false, () =>
                    {
                        var values = new object[property.ValueEntry.WeakValues.Count];

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = property.ValueEntry.WeakValues[i];
                        }

                        parentResolver.QueueRemoveAt(property.Index);
                        parentResolver.QueueInsertAt(0, values);
                    });

                    genericMenu.AddItem(new GUIContent("Move element to bottom"), false, () =>
                    {
                        var values = new object[property.ValueEntry.WeakValues.Count];

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = property.ValueEntry.WeakValues[i];
                        }

                        parentResolver.QueueRemoveAt(property.Index);
                        parentResolver.QueueAdd(values);
                    });

                    genericMenu.AddItem(new GUIContent("Move element to index"), false, () =>
                    {
                        var popup = new IndexPopupWindow();

                        popup.IntLabel = "Index";
                        popup.Value = property.Index;
                        popup.MaxCount = property.Parent.Children.Count - 1;
                        popup.MoveAction = () =>
                        {
                            var index = popup.Value;
                            var currentIndex = property.Index;

                            bool isAdd = false;

                            if (index < 0) index = 0;
                            if (index > property.Parent.Children.Count) isAdd = true;

                            var values = new object[property.ValueEntry.WeakValues.Count];

                            for (int i = 0; i < values.Length; i++)
                            {
                                values[i] = property.ValueEntry.WeakValues[i];
                            }

                            parentResolver.QueueRemoveAt(property.Index);

                            if (isAdd)
                            {
                                parentResolver.QueueAdd(values);
                            }
                            else
                            {
                                parentResolver.QueueInsertAt(index, values);
                            }
                        };

                        property.Tree.DelayActionUntilRepaint(() =>
                        {
                            var window = OdinEditorWindow.InspectObjectInDropDown(popup, 120);
                            popup.CloseWindowAction = () => EditorApplication.delayCall += window.Close;
                        });
                    });

                    genericMenu.AddItem(new GUIContent("Duplicate element"), false, () =>
                    {
                        var values = new object[property.ValueEntry.WeakValues.Count];

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = SerializationUtility.CreateCopy(property.ValueEntry.WeakValues[i]);
                        }

                        if (property.Index + 1 >= property.Parent.Children.Count)
                        {
                            parentResolver.QueueAdd(values);
                        }
                        else
                        {
                            parentResolver.QueueInsertAt(property.Index + 1, values);
                        }
                    });

                    if (Clipboard.CanPaste(parentResolver.ElementType))
                    {
                        genericMenu.AddItem(new GUIContent("Insert pasted element"), false, () =>
                        {
                            var value = Clipboard.Paste();
                            var values = new object[property.ValueEntry.WeakValues.Count];

                            values[0] = value;

                            for (int i = 1; i < values.Length; i++)
                            {
                                values[i] = SerializationUtility.CreateCopy(property.ValueEntry.WeakValues[i]);
                            }

                            parentResolver.QueueInsertAt(property.Index, values);
                        });
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Insert pasted element"));
                    }

                    IHackyListDrawerInteractions parentListDrawer = null;

                    foreach (var drawer in property.Parent.GetActiveDrawerChain().BakedDrawerArray)
                    {
                        parentListDrawer = drawer as IHackyListDrawerInteractions;
                        if (parentListDrawer != null) break;
                    }

                    if (parentListDrawer == null || !parentListDrawer.CanCreateValuesToAdd)
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Insert new element"));
                    }
                    else
                    {
                        genericMenu.AddItem(new GUIContent("Insert new element"), false, () =>
                        {
                            property.Tree.DelayActionUntilRepaint(() =>
                            {
                                parentListDrawer.CreateValuesToAdd((object[] values) =>
                                {
                                    parentResolver.QueueInsertAt(property.Index, values);
                                }, new Rect(Event.current.mousePosition, Vector2.one));
                            });
                        });
                    }

                    genericMenu.AddItem(new GUIContent("Delete element"), false, () =>
                    {
                        property.Tree.DelayActionUntilRepaint(() =>
                        {
                            parentResolver.QueueRemoveAt(property.Index);
                        });
                    });
                }
            }

            var objs = property.ValueEntry.WeakValues.FilterCast<object>().Where(x => x != null).ToArray();
            var valueToCopy = (objs == null || objs.Length == 0) ? null : (objs.Length == 1 ? objs[0] : objs);
            bool isUnityObject = property.ValueEntry.BaseValueType.InheritsFrom(typeof(UnityEngine.Object));
            bool hasValue = valueToCopy != null;
            bool canPaste = Clipboard.CanPaste(property.ValueEntry.BaseValueType);
            bool isEditable = property.ValueEntry.IsEditable;
            bool isNullable =
                (property.ValueEntry.BaseValueType.IsClass || property.ValueEntry.BaseValueType.IsInterface) &&
                !property.Info.TypeOfValue.IsValueType &&
                (property.ValueEntry.SerializationBackend.SupportsPolymorphism || isUnityObject);

            //if (canPaste && property.ValueEntry.SerializationBackend != SerializationBackend.Unity && Clipboard.CurrentCopyMode == CopyModes.CopyReference)
            //{
            //    canPaste = false;
            //}

            if (canPaste && isEditable)
            {
                genericMenu.AddItem(new GUIContent("Paste"), false, () =>
                {
                    property.Tree.DelayActionUntilRepaint(() =>
                    {
                        for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                        {
                            property.ValueEntry.WeakValues[i] = Clipboard.Paste();
                        }
                        // Apply happens after the action is invoked in repaint
                        //property.ValueEntry.ApplyChanges();
                        GUIHelper.RequestRepaint();
                    });
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Paste"));
            }

            if (hasValue)
            {
                if (isUnityObject)
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.CopyReference));
                }
                else if (property.ValueEntry.TypeOfValue.IsNullableType() == false)
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.CopyReference));
                }
                else if (!property.ValueEntry.SerializationBackend.SupportsPolymorphism)
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.DeepCopy));
                }
                else
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.DeepCopy));
                    genericMenu.AddItem(new GUIContent("Copy Special/Deep Copy (default)"), false, () => Clipboard.Copy(valueToCopy, CopyModes.DeepCopy));
                    genericMenu.AddItem(new GUIContent("Copy Special/Shallow Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.ShallowCopy));
                    genericMenu.AddItem(new GUIContent("Copy Special/Copy Reference"), false, () => Clipboard.Copy(valueToCopy, CopyModes.CopyReference));
                }
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Copy"));
            }

            if (isNullable)
            {
                genericMenu.AddSeparator("");

                if (hasValue && isEditable)
                {
                    genericMenu.AddItem(new GUIContent("Set To Null"), false, () =>
                    {
                        property.Tree.DelayActionUntilRepaint(() =>
                        {
                            for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                            {
                                property.ValueEntry.WeakValues[i] = null;
                            }
                            // Apply happens after the action is invoked in repaint
                            //property.ValueEntry.ApplyChanges();
                            GUIHelper.RequestRepaint();
                        });
                    });
                }
                else
                {
                    genericMenu.AddDisabledItem(new GUIContent("Set To Null"));
                }
            }
        }
    }

    /// <summary>
    /// Opens a context menu for any given property on right click. The context menu is populated by all relevant drawers that implements <see cref="IDefinesGenericMenuItems"/>.
    /// </summary>
    /// <seealso cref="IDefinesGenericMenuItems"/>
    [DrawerPriority(95, 0, 0)]
    public sealed class PropertyContextMenuDrawer<T> : OdinValueDrawer<T>
    {
        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return !property.IsTreeRoot;
        }

        /// <summary>
        /// Initializes the drawer.
        /// </summary>
        protected override void Initialize()
        {
            var disableAttr = this.Property.GetAttribute<DisableContextMenuAttribute>();

            if (disableAttr != null && disableAttr.DisableForMember)
            {
                this.SkipWhenDrawing = true;
            }
            else if (this.Property.Parent != null && this.Property.Parent.ChildResolver is ICollectionResolver)
            {
                disableAttr = this.Property.Parent.GetAttribute<DisableContextMenuAttribute>();
                this.SkipWhenDrawing = disableAttr != null && disableAttr.DisableForCollectionElements;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.CallNextDrawer(label);

            var id = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.Layout)
            {
                return;
            }

            Rect rect;

            if (this.Property.Parent != null && this.Property.Parent.ChildResolver is ICollectionResolver)
            {
                rect = GUIHelper.GetCurrentLayoutRect();
            }
            else
            {
                rect = this.Property.LastDrawnValueRect;
            }

            GUIHelper.PushGUIEnabled(true);
            PropertyContextMenuDrawer.AddRightClickArea(this.Property, rect, id);
            GUIHelper.PopGUIEnabled();
        }
    }
}
#endif