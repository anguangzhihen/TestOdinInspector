#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SirenixEditorFields.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Field drawing functions for various types.
    /// </summary>
    public static class SirenixEditorFields
    {
        // If you add code to my baby, please follow this guideline:
        /* Overload guidelines:
		 * Method ( Rect rect, GUIContent label, <Value>						)
		 * Method ( Rect rect, string label, <Value>							)
		 * Method ( Rect rect, <Value>											)
		 * Method ( GUIContent label, <Value>, params GUILayoutOption[] options	)
		 * Method ( string label, <Value>, params GUILayoutOption[] options		)
		 * Method ( <Value>, params GUILayoutOption[] options					)
		 */

        private const int DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT = 30;
        private static readonly int slideKnobWidth = 14;
        private static readonly Color delayedActiveColor = Color.yellow;
        private static Vector4 vectorNormalBuffer;
        private static float vectorLengthBuffer;
        private static int localHotControl;
        private static int delayedIntBuffer;
        private static long delayedLongBuffer;
        private static float delayedFloatBuffer;
        private static double delayedDoubleBuffer;
        private static string delayedTextBuffer;
        private static GUIStyle progressBarTextOverlayStyle = null;
        private static GUIStyle minMaxSliderStyle = null;
        private static GUIStyle sliderBackground = null;
        private static GUIStyle minMaxFloatingLabelStyle = null;
        private static List<int> layerNumbers = new List<int>();
        private static bool? responsiveVectorComponentFields;
        private static bool currentEnumControlHasValue = false;
        private static int currentEnumControlID = 0;
        private static Enum selectedEnumValue;

        /// <summary>
        /// The width of the X, Y and Z labels in structs.
        /// </summary>
        public static readonly int SingleLetterStructLabelWidth = 13;

        /// <summary>
        /// When <c>true</c> the component labels, for vector fields, will be hidden when the field is too narrow.
        /// </summary>
        public static bool ResponsiveVectorComponentFields
        {
            get
            {
                if (!responsiveVectorComponentFields.HasValue)
                {
                    responsiveVectorComponentFields = EditorPrefs.GetBool("SirenixEditorFields.ResponsiveVectorComponentFields", true);
                }

                return responsiveVectorComponentFields.Value;
            }
            set
            {
                responsiveVectorComponentFields = value;
                EditorPrefs.SetBool("SirenixEditorFields.ResponsiveVectorComponentFields", value);
            }
        }

        /// <summary>
        /// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        public static UnityEngine.Object UnityObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects)
        {
            UnityEngine.Object originalValue = value;
            bool originalValueWasFakeNull = value == null && !object.ReferenceEquals(value, null);

            // This could be added to also support dragging on object fields.
            // value = DragAndDropUtilities.DragAndDropZone(rect, value, objectType, true, true) as UnityEngine.Object;

            var penRect = rect;
            penRect.x += penRect.width - 38;
            penRect.width = 20;
            SirenixEditorGUI.BeginDrawOpenInspector(penRect, value, SirenixEditorGUI.IndentLabelRect(rect, label != null));

            allowSceneObjects = allowSceneObjects && !typeof(ScriptableObject).IsAssignableFrom(objectType);

            value = label == null ?
                EditorGUI.ObjectField(rect, value == null ? null : value, objectType, allowSceneObjects) :
                EditorGUI.ObjectField(rect, label, value == null ? null : value, objectType, allowSceneObjects);

            SirenixEditorGUI.EndDrawOpenInspector(penRect, value);

            if (originalValueWasFakeNull && object.ReferenceEquals(value, null))
            {
                value = originalValue;
            }

            return value;
        }

        /// <summary>
        /// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        public static UnityEngine.Object UnityObjectField(Rect rect, string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects)
        {
            return UnityObjectField(rect, GUIHelper.TempContent(label), value, objectType, allowSceneObjects);
        }

        /// <summary>
        /// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        public static UnityEngine.Object UnityObjectField(Rect rect, UnityEngine.Object value, Type objectType, bool allowSceneObjects)
        {
            return UnityObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects);
        }

        /// <summary>
        /// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="options">Layout options.</param>
        public static UnityEngine.Object UnityObjectField(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, options);
            return UnityObjectField(rect, label, value, objectType, allowSceneObjects);
        }

        /// <summary>
        /// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="options">Layout options.</param>
        public static UnityEngine.Object UnityObjectField(string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, options);
            return UnityObjectField(rect, GUIHelper.TempContent(label), value, objectType, allowSceneObjects);
        }

        /// <summary>
        /// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
        /// </summary>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="options">Layout options.</param>
        public static UnityEngine.Object UnityObjectField(UnityEngine.Object value, Type objectType, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, options);
            return UnityObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects);
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        public static TElement PreviewObjectField<TElement>(Rect rect, TElement value, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
        {
            // TODO: Add overloads
            var id = DragAndDropUtilities.GetDragAndDropId(rect);
            DragAndDropUtilities.DrawDropZone(rect, value, null, id);
            
            if (!dragOnly)
            {
                value = DragAndDropUtilities.DropZone(rect, value, id);
                value = DragAndDropUtilities.ObjectPickerZone(rect, value, allowSceneObjects, id);
            }

            value = DragAndDropUtilities.DragZone(rect, value, allowMove, allowSwap, id);

            return value;
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        public static UnityEngine.Object UnityPreviewObjectField(Rect rect, UnityEngine.Object value, UnityEngine.Texture texture, Type type, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
        {
            var id = DragAndDropUtilities.GetDragAndDropId(rect);

            DragAndDropUtilities.DrawDropZone(rect, texture, null, id);

            if (!dragOnly)
            {
                value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
                value = DragAndDropUtilities.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object;
            }

            value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = id;
                GUIUtility.hotControl = id;
            }

            return value;
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        public static UnityEngine.Object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type type, ObjectFieldAlignment alignment, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
        {
            var id = DragAndDropUtilities.GetDragAndDropId(rect);

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, id, label);
            }

            if (alignment == ObjectFieldAlignment.Left)
            {
                rect = rect.AlignLeft(rect.height);
            }
            else if (alignment == ObjectFieldAlignment.Center)
            {
                rect = rect.AlignCenter(rect.height);
            }
            else
            {
                rect = rect.AlignRight(rect.height);
            }

            DragAndDropUtilities.DrawDropZone(rect, value, null, id);

            if (!dragOnly)
            {
                value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
                value = DragAndDropUtilities.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object;
            }

            value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = id;
                GUIUtility.hotControl = id;
            }

            return value;
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        public static UnityEngine.Object UnityPreviewObjectField(Rect rect, UnityEngine.Object value, Type type, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
        {
            return UnityPreviewObjectField(rect, null, value, type, ObjectFieldAlignment.Right, dragOnly, allowMove, allowSwap, allowSceneObjects);
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="alignment">How the square object field should be aligned.</param>
        public static UnityEngine.Object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
        {
            return SirenixEditorFields.UnityPreviewObjectField(rect, label, value, objectType, alignment, false, true, true, allowSceneObjects);
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="alignment">How the square object field should be aligned.</param>
        public static UnityEngine.Object UnityPreviewObjectField(Rect rect, string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
        {
            return UnityPreviewObjectField(rect, GUIHelper.TempContent(label), value, objectType, allowSceneObjects, alignment);
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="alignment">How the square object field should be aligned.</param>
        public static UnityEngine.Object UnityPreviewObjectField(Rect rect, UnityEngine.Object value, Type objectType, bool allowSceneObjects, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
        {
            return UnityPreviewObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects, alignment);
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="height">The height or size of the square object field.</param>
        /// <param name="alignment">How the square object field should be aligned.</param>
        public static UnityEngine.Object UnityPreviewObjectField(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, float height = DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, height);
            return UnityPreviewObjectField(rect, label, value, objectType, allowSceneObjects, alignment);
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="height">The height or size of the square object field.</param>
        /// <param name="alignment">How the square object field should be aligned.</param>
        public static UnityEngine.Object UnityPreviewObjectField(string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, float height = DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, height);
            return UnityPreviewObjectField(rect, GUIHelper.TempContent(label), value, objectType, allowSceneObjects, alignment);
        }

        /// <summary>
        /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
        /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
        /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
        /// </summary>
        /// <param name="value">The Unity object.</param>
        /// <param name="objectType">The Unity object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="height">The height or size of the square object field.</param>
        /// <param name="alignment">How the square object field should be aligned.</param>
        public static UnityEngine.Object UnityPreviewObjectField(UnityEngine.Object value, Type objectType, bool allowSceneObjects, float height = DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            return UnityPreviewObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects, alignment);
        }

        /// <summary>
        /// Draws a polymorphic ObjectField.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="options">Layout options.</param>
        public static object PolymorphicObjectField(GUIContent label, object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            Rect rect;
            bool hasKeyboardFocus;
            int id;
            SirenixEditorGUI.GetFeatureRichControlRect(label, out id, out hasKeyboardFocus, out rect);

            return PolymorphicObjectField(rect, value, type, allowSceneObjects, hasKeyboardFocus, id);
        }

        /// <summary>
        /// Draws a polymorphic ObjectField.
        /// </summary>
        public static object PolymorphicObjectField(Rect rect, GUIContent label, object value, Type type, bool allowSceneObjects)
        {
            var totalRect = rect;
            var valueRect = rect;
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);

            if (label == null)
            {
                valueRect = EditorGUI.IndentedRect(valueRect);
            }
            else
            {
                totalRect.xMin += EditorGUI.indentLevel * 15f;
                valueRect = EditorGUI.PrefixLabel(valueRect, controlId, label);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && totalRect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = controlId;
            }

            var hasKeyboardFocus = GUIUtility.keyboardControl == controlId && GUIHelper.CurrentWindow == EditorWindow.focusedWindow;
            return PolymorphicObjectField(rect, value, type, allowSceneObjects, hasKeyboardFocus, controlId);
        }

        /// <summary>
        /// Draws a polymorphic ObjectField.
        /// </summary>
        public static object PolymorphicObjectField(Rect rect, object value, Type type, bool allowSceneObjects, bool hasKeyboardFocus, int id)
        {
            var e = Event.current.type;

            var dropId = DragAndDropUtilities.GetDragAndDropId(rect);
            var penRect = rect;
            var uObj = value as UnityEngine.Object;

            if (uObj)
            {
                penRect.x += penRect.width - 38;
                penRect.width = 20;

                SirenixEditorGUI.BeginDrawOpenInspector(penRect, uObj, rect);
            }

            if (e == EventType.Repaint)
            {
                GUIContent title;
                if (EditorGUI.showMixedValue)
                {
                    title = new GUIContent("   " + "— Conflict (" + type.GetNiceName() + ")");
                }
                else if (value == null)
                {
                    title = new GUIContent("   " + "Null (" + type.GetNiceName() + ")");
                }
                else if (uObj)
                {
                    string baseType = value.GetType() == type ? "" : " : " + type.GetNiceName();
                    title = new GUIContent("   " + uObj.name + " (" + value.GetType().GetNiceName() + baseType + ")");
                }
                else
                {
                    string baseType = value.GetType() == type ? "" : " : " + type.GetNiceName();
                    title = new GUIContent("   " + value.GetType().GetNiceName() + baseType);
                }

                EditorStyles.objectField.Draw(rect, title, id, DragAndDropUtilities.HoveringAcceptedDropZone == dropId);

                if (uObj)
                {
                    var thumbnail = GUIHelper.GetAssetThumbnail(uObj, value.GetType(), true);

                    if (thumbnail != null)
                    {
                        GUI.DrawTexture(rect.AlignLeft(rect.height * 0.75f).SetHeight(rect.height * 0.75f).AddX(3).AddY(1.5f), thumbnail);
                    }
                }
                else
                {
                    if (UnityVersion.IsVersionOrGreater(2019, 3))
                    {
                        EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height - 3).AlignCenterY(rect.height - 3).AddY(1));
                    }
                    else
                    {
                        EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height));
                    }
                }
            }

            if (uObj)
            {
                SirenixEditorGUI.EndDrawOpenInspector(penRect, uObj);
            }

            //// Handle Unity dragging manually for now
            //if ((e == EventType.DragUpdated || e == EventType.DragPerform) && rect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length == 1)
            //{
            //    UnityEngine.Object obj = DragAndDrop.objectReferences[0];

            //    bool accept = false;

            //    if (type.IsAssignableFrom(obj.GetType()))
            //    {
            //        accept = true;
            //    }
            //    else if (obj is GameObject && (type.InheritsFrom(typeof(Component)) || type.IsInterface))
            //    {
            //        obj = (obj as GameObject).GetComponent(type);

            //        if (obj != null)
            //        {
            //            accept = true;
            //        }
            //    }

            //    if (accept)
            //    {
            //        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            //        Event.current.Use();

            //        if (e == EventType.DragPerform)
            //        {
            //            DragAndDrop.AcceptDrag();
            //            GUI.changed = true;
            //            return obj;
            //        }
            //    }
            //}

            var objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + GUIUtility.GetControlID(FocusType.Passive), type);

            value = DragAndDropUtilities.DropZone(rect, value, type, true, dropId);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) || hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
            {
                var forceOpenPicker = rect.AlignRight(16).Contains(Event.current.mousePosition);

                if (!forceOpenPicker && uObj)
                {
                    if (Event.current.clickCount == 1)
                    {
                        EditorGUIUtility.PingObject(uObj);
                    }
                    else if (Event.current.clickCount == 2)
                    {
                        AssetDatabase.OpenAsset(uObj);
                    }
                }
                else
                {
                    objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
                }

                Event.current.Use();
            }

            if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
            {
                GUI.changed = true;
                return objectPicker.ClaimObject();
            }

            return value;
        }

        /// <summary>
        /// Draws a polymorphic ObjectField.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="options">Layout options.</param>
        public static object PolymorphicObjectField(string label, object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            return PolymorphicObjectField(GUIHelper.TempContent(label), value, type, allowSceneObjects);
        }

        /// <summary>
        /// Draws a polymorphic ObjectField.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The object type. This supports inheritance.</param>
        /// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
        /// <param name="options">Layout options.</param>
        public static object PolymorphicObjectField(object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            return PolymorphicObjectField((GUIContent)null, value, type, allowSceneObjects);
        }

        /// <summary>
        /// Draws a field for a layer mask.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="layerMask">The layer mask to draw.</param>
        public static LayerMask LayerMaskField(Rect rect, GUIContent label, LayerMask layerMask)
        {
            // TODO: Make this less ugly

            var layers = InternalEditorUtility.layers;

            layerNumbers.Clear();

            for (int i = 0; i < layers.Length; i++)
            {
                layerNumbers.Add(LayerMask.NameToLayer(layers[i]));
            }

            int maskWithoutEmpty = 0;

            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) != 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }

            maskWithoutEmpty = label == null ? EditorGUI.MaskField(rect, maskWithoutEmpty, layers)
                                             : EditorGUI.MaskField(rect, label, maskWithoutEmpty, layers);

            int mask = 0;

            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }
            }

            layerMask.value = mask;

            return layerMask;
        }

        /// <summary>
        /// Draws a field for a layer mask.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="layerMask">The layer mask to draw.</param>
        public static LayerMask LayerMaskField(Rect rect, string label, LayerMask layerMask)
        {
            return LayerMaskField(rect, GUIHelper.TempContent(label), layerMask);
        }

        /// <summary>
        /// Draws a field for a layer mask.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="layerMask">The layer mask to draw.</param>
        public static LayerMask LayerMaskField(Rect rect, LayerMask layerMask)
        {
            return LayerMaskField(rect, (GUIContent)null, layerMask);
        }

        /// <summary>
        /// Draws a field for a layer mask.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="layerMask">The layer mask to draw.</param>
        /// <param name="options">Layout options.</param>
        public static LayerMask LayerMaskField(GUIContent label, LayerMask layerMask, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, options);
            return LayerMaskField(rect, label, layerMask);
        }

        /// <summary>
        /// Draws a field for a layer mask.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="layerMask">The layer mask to draw.</param>
        /// <param name="options">Layout options.</param>
        public static LayerMask LayerMaskField(string label, LayerMask layerMask, params GUILayoutOption[] options)
        {
            return LayerMaskField(GUIHelper.TempContent(label), layerMask, options);
        }

        /// <summary>
        /// Draws a field for a layer mask.
        /// </summary>
        /// <param name="layerMask">The layer mask to draw.</param>
        /// <param name="options">Layout options.</param>
        public static LayerMask LayerMaskField(LayerMask layerMask, params GUILayoutOption[] options)
        {
            return LayerMaskField((GUIContent)null, layerMask, options);
        }

        /// <summary>
        /// Draws a Guid field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Guid GuidField(Rect rect, GUIContent label, Guid value)
        {
            return GuidField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a Guid field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Guid GuidField(Rect rect, Guid value) { return GuidField(rect, null, value, null); }

        /// <summary>
        /// Draws a Guid field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Guid GuidField(GUIContent label, Guid value)
        {
            return GuidField(label, value, (GUIStyle)null, (GUILayoutOption[])null);
        }

        /// <summary>
        /// Draws a Guid field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Guid GuidField(GUIContent label, Guid value, params GUILayoutOption[] options)
        {
            return GuidField(label, value, null, options);
        }

        /// <summary>
        /// Draws a Guid field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Guid GuidField(GUIContent label, Guid value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.textField, options);
            return SirenixEditorFields.GuidField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a Guid field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Guid GuidField(Rect rect, GUIContent label, Guid value, GUIStyle style)
        {
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label, style ?? EditorStyles.label);
            }

            string text = value.ToString("D");

            EditorGUI.BeginChangeCheck();

            string newText = EditorGUI.DelayedTextField(rect.SubXMax(75), text, style ?? EditorStyles.textField);

            if (EditorGUI.EndChangeCheck() || newText != text)
            {
                text = newText;

                try
                {
                    value = new Guid(text);
                    GUI.changed = true;
                }
                catch
                {
                    // Ignore
                }
            }

            if (GUI.Button(rect.SetXMin(rect.xMax - 70), GUIHelper.TempContent("New GUID")))
            {
                value = Guid.NewGuid();
                GUI.changed = true;
            }

            return value;
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(Rect rect, GUIContent label, int value, GUIStyle style)
        {
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.Layout)
            {
                value = label != null ?
                    EditorGUI.IntField(rect, label, value, style ?? EditorStyles.numberField) :
                    EditorGUI.IntField(rect, value, style ?? EditorStyles.numberField);
                return value;
            }

            Rect slideRect = rect.AlignRight(slideKnobWidth);
            value = SirenixEditorGUI.SlideRectInt(slideRect, control, value);

            value = label != null ?
                EditorGUI.IntField(rect, label, value, style ?? EditorStyles.numberField) :
                EditorGUI.IntField(rect, value, style ?? EditorStyles.numberField);

            DrawSlideKnob(rect, control);

            return value;
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(Rect rect, GUIContent label, int value)
        {
            return SirenixEditorFields.IntField(rect, label, value, null);
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(Rect rect, string label, int value)
        {
            return SirenixEditorFields.IntField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(Rect rect, int value)
        {
            return SirenixEditorFields.IntField(rect, null, value, null);
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return SirenixEditorFields.IntField(rect, label, value, style);
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(GUIContent label, int value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.IntField(label, value, null, options);
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(string label, int value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.IntField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws an int field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int IntField(int value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.IntField(null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(Rect rect, GUIContent label, int value, GUIStyle style)
        {
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            if (OnLocalControlRelease(rect, control))
            {
                GUI.changed = true;
                value = delayedIntBuffer;
            }

            // Value buffer
            int buffer = value;
            if (localHotControl == control)
            {
                GUIHelper.PushColor(delayedActiveColor);
                buffer = delayedIntBuffer;
            }

            EditorGUI.BeginChangeCheck();
            buffer = IntField(rect, label, buffer, style);

            if (localHotControl == control)
            {
                GUIHelper.PopColor();
            }

            if (EditorGUI.EndChangeCheck())
            {
                GUI.changed = false;
                localHotControl = control;
                delayedIntBuffer = buffer;
            }

            return value;
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(Rect rect, GUIContent label, int value)
        {
            return SirenixEditorFields.DelayedIntField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(Rect rect, string label, int value)
        {
            return SirenixEditorFields.DelayedIntField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(Rect rect, int value)
        {
            return SirenixEditorFields.DelayedIntField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return SirenixEditorFields.DelayedIntField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(GUIContent label, int value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.DelayedIntField(label, value, null, options);
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(string label, int value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.DelayedIntField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed int field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int DelayedIntField(int value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.DelayedIntField(null, value, null, options);
        }

        /// <summary>
        /// Draws a range field for ints.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(Rect rect, GUIContent label, int value, int min, int max, GUIStyle style)
        {
            return label != null ?
                (int)EditorGUI.Slider(rect, label, value, (min < max ? min : max), (max > min ? max : min)) :
                (int)EditorGUI.Slider(rect, value, (min < max ? min : max), (max > min ? max : min));
        }

        /// <summary>
        /// Draws a range field for ints.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(Rect rect, GUIContent label, int value, int min, int max)
        {
            return RangeIntField(rect, label, value, min, max, null);
        }

        /// <summary>
        /// Draws a range field for ints.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(Rect rect, string label, int value, int min, int max)
        {
            return RangeIntField(rect, label != null ? GUIHelper.TempContent(label) : null, value, min, max, null);
        }

        /// <summary>
        /// Draws a range field for ints.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(Rect rect, int value, int min, int max)
        {
            return RangeIntField(rect, null, value, min, max, null);
        }

        /// <summary>
        /// Drwas a range field for ints.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(GUIContent label, int value, int min, int max, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return SirenixEditorFields.RangeIntField(rect, label, value, min, max, style);
        }

        /// <summary>
        /// Draws a range field for ints.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(GUIContent label, int value, int min, int max, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.RangeIntField(label, value, min, max, null, options);
        }

        /// <summary>
        /// Draws a range field for ints.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(string label, int value, int min, int max, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.RangeIntField(label != null ? GUIHelper.TempContent(label) : null, value, min, max, null, options);
        }

        /// <summary>
        /// Draws a range field for ints.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int RangeIntField(int value, int min, int max, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.RangeIntField(null, value, min, max, null, options);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="valueLabel">Optional text for label to be drawn ontop of the progress bar. This value is only used if the DrawValueLabel option is enabled in the ProgressBarConfig.</param>
        public static double ProgressBarField(Rect rect, GUIContent label, double value, double minValue, double maxValue, ProgressBarConfig config, string valueLabel)
        {
            int controlId;
            bool focus;

            // Draw the label.
            rect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out controlId, out focus);

            if (Event.current.type == EventType.Layout)
            {
                return value;
            }

            // Center the progress bar to the field.
            rect = rect.AlignCenterY(config.Height);

            // Handle inputs.
            if (GUI.enabled)
            {
                bool changed = false;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) ||
                    GUIUtility.hotControl == controlId && (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag))
                {
                    // Update value based on mouse position.
                    GUIUtility.hotControl = controlId;
                    value = minValue + Math.Abs(maxValue - minValue) * Mathf.Clamp01((Event.current.mousePosition.x - rect.xMin) / rect.width) * (minValue <= maxValue ? 1.0 : -1.0);

                    changed = true;
                }
                else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow)
                {
                    value += 1 * (minValue < maxValue ? 1 : -1);
                    changed = true;
                }
                else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow)
                {
                    value -= 1 * (minValue < maxValue ? 1 : -1);
                    changed = true;
                }
                else if (GUIUtility.hotControl == controlId && Event.current.rawType == EventType.MouseUp)
                {
                    // Release hot control.
                    GUIUtility.hotControl = 0;
                }

                if (changed)
                {
                    GUI.changed = true;

                    // Clamp the value
                    var low = Math.Min(minValue, maxValue);
                    var high = Math.Max(minValue, maxValue);
                    value = value <= low ? low : value >= high ? high : value;

                    GUIHelper.RequestRepaint();
                    Event.current.Use();
                }
            }

            // Draw the progress bar.
            if (Event.current.type == EventType.Repaint)
            {
                GUIHelper.PushGUIEnabled(true);

                // Draw borders
                SirenixEditorGUI.DrawSolidRect(
                    rect,
                    SirenixGUIStyles.BorderColor,
                    false);

                // Draw progress bar background.
                SirenixEditorGUI.DrawSolidRect(
                    rect.Padding(1),
                    config.BackgroundColor,
                    false);

                float progress = maxValue != minValue ? MathUtilities.LinearStep((float)minValue, (float)maxValue, (float)value) : 1f;

                // Draw progress bar foreground.
                Rect foregroundRect = rect
                    .Padding(1) // Offset with 1 pixel on all sides.
                    .AlignLeft((rect.width - 2) * progress); // Compensate for 1 pixel padding on both sides, and then shrink the width of the rect to the value.
                SirenixEditorGUI.DrawSolidRect(
                    foregroundRect,
                    config.ForegroundColor,
                    false);

                // Draw a value label, if enabled.
                if (config.DrawValueLabel || valueLabel != null)
                {
                    // Create default label.
                    if (valueLabel == null)
                    {
                        valueLabel = value.ToString(Math.Abs(maxValue - minValue) <= 1f ? "0.###" : "0.#");
                        if (minValue == 0.0)
                        {
                            valueLabel += " / " + maxValue.ToString("0.#");
                        }
                    }

                    GUIContent overlayContent = GUIHelper.TempContent(valueLabel);
                    ProgressBarOverlayLabel(rect, overlayContent, progress, config);
                }

                GUIHelper.PopGUIEnabled();
            }

            return value;
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        public static double ProgressBarField(Rect rect, GUIContent label, double value, double minValue, double maxValue, ProgressBarConfig config)
        {
            return ProgressBarField(rect, label, value, minValue, maxValue, config, null);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        public static double ProgressBarField(Rect rect, string label, double value, double minValue, double maxValue, ProgressBarConfig config)
        {
            return ProgressBarField(rect, label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        public static double ProgressBarField(Rect rect, double value, double minValue, double maxValue, ProgressBarConfig config)
        {
            return ProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, config);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        public static double ProgressBarField(Rect rect, GUIContent label, double value, double minValue, double maxValue)
        {
            return ProgressBarField(rect, label, value, minValue, maxValue, ProgressBarConfig.Default);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        public static double ProgressBarField(Rect rect, string label, double value, double minValue, double maxValue)
        {
            return ProgressBarField(rect, label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        public static double ProgressBarField(Rect rect, double value, double minValue, double maxValue)
        {
            return ProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="options">Layout options.</param>
        public static double ProgressBarField(GUIContent label, double value, double minValue, double maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, config.Height < EditorGUIUtility.singleLineHeight ? EditorGUIUtility.singleLineHeight : config.Height, options);
            return ProgressBarField(rect, label, value, minValue, maxValue, config);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="options">Layout options.</param>
        public static double ProgressBarField(string label, double value, double minValue, double maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
        {
            return ProgressBarField(label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config, options);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="options">Layout options.</param>
        public static double ProgressBarField(double value, double minValue, double maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
        {
            return ProgressBarField((GUIContent)null, value, minValue, maxValue, config, options);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="options">Layout options.</param>
        public static double ProgressBarField(GUIContent label, double value, double minValue, double maxValue, params GUILayoutOption[] options)
        {
            return ProgressBarField(label, value, minValue, maxValue, ProgressBarConfig.Default, options);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="options">Layout options.</param>
        public static double ProgressBarField(string label, double value, double minValue, double maxValue, params GUILayoutOption[] options)
        {
            return ProgressBarField(label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default, options);
        }

        /// <summary>
        /// Draws a colored progress bar field.
        /// </summary>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="options">Layout options.</param>
        public static double ProgressBarField(double value, double minValue, double maxValue, params GUILayoutOption[] options)
        {
            return ProgressBarField((GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default, options);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="valueLabel">Optional text for label to be drawn ontop of the progress bar. This value is only used if the DrawValueLabel option is enabled in the ProgressBarConfig.</param>
        public static long SegmentedProgressBarField(Rect rect, GUIContent label, long value, long minValue, long maxValue, ProgressBarConfig config, string valueLabel)
        {
            bool focus;
            int controlId;

            // Draw the label.
            rect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out controlId, out focus);

            // Center the progress bar to the field.
            rect = rect.AlignCenterY(config.Height);

            // Handle inputs.
            if (GUI.enabled)
            {
                bool changed = false;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) ||
                    GUIUtility.hotControl == controlId && (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag))
                {
                    // Update value based on mouse position.
                    GUIUtility.hotControl = controlId;

                    float tileSize = rect.width / Mathf.Abs(maxValue - minValue);

                    if (Event.current.mousePosition.x < rect.xMin + tileSize * 0.1f) // Allow the user to set value to the min value, by clicking the very first bit of the field.
                    {
                        value = minValue;
                    }
                    else
                    {
                        value = minValue + (long)((Mathf.Min(Event.current.mousePosition.x - rect.xMin, rect.width) / tileSize) + 1) * (minValue <= maxValue ? 1 : -1);
                    }

                    changed = true;
                }
                else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow)
                {
                    value += 1 * (minValue < maxValue ? 1 : -1);
                    changed = true;
                }
                else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow)
                {
                    value -= 1 * (minValue < maxValue ? 1 : -1);
                    changed = true;
                }
                else if (GUIUtility.hotControl == controlId && Event.current.rawType == EventType.MouseUp)
                {
                    // Release hot control.
                    GUIUtility.hotControl = 0;
                }

                if (changed)
                {
                    // Clamp the value
                    var low = Math.Min(minValue, maxValue);
                    var high = Math.Max(minValue, maxValue);
                    value = value <= low ? low : value >= high ? high : value;

                    GUI.changed = true;
                    GUIHelper.RequestRepaint();
                    Event.current.Use();
                }
            }

            // Draw the progress bar.
            if (Event.current.type == EventType.Repaint)
            {
                GUIHelper.PushGUIEnabled(true);

                var range = Math.Abs(maxValue - minValue);

                if (range > 0)
                {
                    var low = Math.Min(minValue, maxValue);
                    var high = Mathf.Max(minValue, maxValue);
                    var displayValue = value <= low ? low : value >= high ? high : value;

                    // Draw the progress tiles.
                    for (long i = 0; i < range; i++)
                    {
                        var tile = rect.Expand(0.5f, 0).Split((int)i, (int)Mathf.Abs(maxValue - minValue));

                        SirenixEditorGUI.DrawSolidRect(tile.Padding(0.5f, 0f), SirenixGUIStyles.BorderColor, false);

                        if ((minValue <= maxValue) && minValue + i < displayValue || minValue - i > displayValue)
                        {
                            // Draw a filled tile.
                            SirenixEditorGUI.DrawSolidRect(tile.Padding(1.5f, 1), config.ForegroundColor, false);
                        }
                        else
                        {
                            // Draw an empty tile.
                            SirenixEditorGUI.DrawSolidRect(tile.Padding(1.5f, 1), config.BackgroundColor, false);
                        }
                    }
                }
                else
                {
                    // Draw single filled tile.
                    SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor, false);
                    SirenixEditorGUI.DrawSolidRect(rect.Padding(1), config.ForegroundColor, false);
                }

                // If enabled, then draw the overlay on top of the progress bar.
                if (config.DrawValueLabel || valueLabel != null)
                {
                    if (valueLabel == null)
                    {
                        valueLabel = value.ToString();
                        if (minValue == 0)
                        {
                            valueLabel += " / " + maxValue.ToString();
                        }
                    }

                    ProgressBarOverlayLabel(rect, GUIHelper.TempContent(valueLabel), range > 0 ? MathUtilities.LinearStep(minValue, maxValue, value) : 1f, config);
                }

                GUIHelper.PopGUIEnabled();
            }

            return value;
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        public static long SegmentedProgressBarField(Rect rect, GUIContent label, long value, long minValue, long maxValue, ProgressBarConfig config)
        {
            return SegmentedProgressBarField(rect, label, value, minValue, maxValue, config, null);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        public static long SegmentedProgressBarField(Rect rect, string label, long value, long minValue, long maxValue, ProgressBarConfig config)
        {
            return SegmentedProgressBarField(rect, label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        public static long SegmentedProgressBarField(Rect rect, long value, long minValue, long maxValue, ProgressBarConfig config)
        {
            return SegmentedProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, config);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        public static long SegmentedProgressBarField(Rect rect, GUIContent label, long value, long minValue, long maxValue)
        {
            return SegmentedProgressBarField(rect, label, value, minValue, maxValue, ProgressBarConfig.Default);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        public static long SegmentedProgressBarField(Rect rect, string label, long value, long minValue, long maxValue)
        {
            return SegmentedProgressBarField(rect, label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        public static long SegmentedProgressBarField(Rect rect, long value, long minValue, long maxValue)
        {
            return SegmentedProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="options">Layout options.</param>
        public static long SegmentedProgressBarField(GUIContent label, long value, long minValue, long maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, config.Height < EditorGUIUtility.singleLineHeight ? EditorGUIUtility.singleLineHeight : config.Height, options);
            return SegmentedProgressBarField(rect, label, value, minValue, maxValue, config);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="options">Layout options.</param>
        public static long SegmentedProgressBarField(string label, long value, long minValue, long maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
        {
            return SegmentedProgressBarField(label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config, options);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="config">The configuration for the progress bar field.</param>
        /// <param name="options">Layout options.</param>
        public static long SegmentedProgressBarField(long value, long minValue, long maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
        {
            return SegmentedProgressBarField((GUIContent)null, value, minValue, maxValue, config, options);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="options">Layout options.</param>
        public static long SegmentedProgressBarField(GUIContent label, long value, long minValue, long maxValue, params GUILayoutOption[] options)
        {
            return SegmentedProgressBarField(label, value, minValue, maxValue, ProgressBarConfig.Default, options);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="label">The label to use, or null if no label should be used.</param>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="options">Layout options.</param>
        public static long SegmentedProgressBarField(string label, long value, long minValue, long maxValue, params GUILayoutOption[] options)
        {
            return SegmentedProgressBarField(label != null ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default, options);
        }

        /// <summary>
        /// Draws a colored segmented progress bar field.
        /// </summary>
        /// <param name="value">The current value of the progress bar.</param>
        /// <param name="minValue">The left hand side value of the progress bar.</param>
        /// <param name="maxValue">The right hand side value of the progress bar.</param>
        /// <param name="options">Layout options.</param>
        public static long SegmentedProgressBarField(long value, long minValue, long maxValue, params GUILayoutOption[] options)
        {
            return SegmentedProgressBarField((GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default, options);
        }

        /// <summary>
        /// Draws an overlay on top of a progress bar field.
        /// </summary>
        /// <param name="rect">The rect used to draw the progress bar field with. (Minus the Rect for the prefix label, if any.)</param>
        /// <param name="label">The label to draw ontop of the progress bar field.</param>
        /// <param name="progress">The relative value of the progress bar, from 0 to 1.</param>
        /// <param name="config">The configuration used to draw the progress bar field.</param>
        public static void ProgressBarOverlayLabel(Rect rect, GUIContent label, float progress, ProgressBarConfig config)
        {
            if (progressBarTextOverlayStyle == null)
            {
                progressBarTextOverlayStyle = new GUIStyle(SirenixGUIStyles.LeftAlignedGreyMiniLabel)
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    contentOffset = Vector2.zero,
                };
            }

            if (Event.current.type == EventType.Repaint)
            {
                // Draw the text overlay
                Rect foregroundRect = rect.Padding(1).AlignLeft((rect.width - 2) * Mathf.Clamp01(progress));

                Vector2 size = progressBarTextOverlayStyle.CalcSize(label);
                Rect overlayRect = rect.HorizontalPadding(4, 0).AlignCenterY(size.y);

                switch (config.ValueLabelAlignment)
                {
                    default:
                    case TextAlignment.Center:
                        overlayRect = overlayRect.AlignCenterX(size.x);
                        break;

                    case TextAlignment.Left:
                        overlayRect = overlayRect.AlignLeft(size.x);
                        break;

                    case TextAlignment.Right:
                        overlayRect = overlayRect.AlignRight(size.x + 2);
                        break;
                }

                // Draw the text in background color.
                if (foregroundRect.xMax < overlayRect.xMax)
                {
                    float offset = overlayRect.xMin - Mathf.Max(foregroundRect.xMax, overlayRect.xMin);

                    progressBarTextOverlayStyle.normal.textColor = config.ForegroundColor;
                    progressBarTextOverlayStyle.contentOffset = new Vector2(offset, 0f);

                    GUI.Label(
                        overlayRect.AddX(Mathf.Abs(offset)),
                        label,
                        progressBarTextOverlayStyle);
                }

                // Draw the text in foreground color.
                if (foregroundRect.Overlaps(overlayRect))
                {
                    progressBarTextOverlayStyle.normal.textColor = config.BackgroundColor;
                    progressBarTextOverlayStyle.contentOffset = Vector2.zero;
                    overlayRect = overlayRect.SetXMax(Mathf.Min(foregroundRect.xMax, overlayRect.xMax));

                    GUI.Label(
                        overlayRect,
                        label,
                        progressBarTextOverlayStyle);
                }
            }
        }

        /// <summary>
        /// Draws an overlay on top of a progress bar field.
        /// </summary>
        /// <param name="rect">The rect used to draw the progress bar field with. (Minus the Rect for the prefix label, if any.)</param>
        /// <param name="label">The label to draw ontop of the progress bar field.</param>
        /// <param name="progress">The relative value of the progress bar, from 0 to 1.</param>
        /// <param name="config">The configuration used to draw the progress bar field.</param>
        private static void ProgressBarOverlayLabel(Rect rect, string label, float progress, ProgressBarConfig config)
        {
            ProgressBarOverlayLabel(rect, GUIHelper.TempContent(label), progress, config);
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(Rect rect, GUIContent label, long value, GUIStyle style)
        {
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            Rect slideRect = rect.AlignRight(slideKnobWidth);

            value = SirenixEditorGUI.SlideRectLong(slideRect, control, value);

            value = label != null ?
                EditorGUI.LongField(rect, label, value, style ?? EditorStyles.numberField) :
                EditorGUI.LongField(rect, value, style ?? EditorStyles.numberField);

            DrawSlideKnob(rect, control);

            return value;
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(Rect rect, GUIContent label, long value)
        {
            return SirenixEditorFields.LongField(rect, label, value, null);
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(Rect rect, string label, long value)
        {
            return SirenixEditorFields.LongField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(Rect rect, long value)
        {
            return SirenixEditorFields.LongField(rect, null, value, null);
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(GUIContent label, long value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return SirenixEditorFields.LongField(rect, label, value, style);
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(GUIContent label, long value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.LongField(label, value, null, options);
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(string label, long value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.LongField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws an long field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long LongField(long value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.LongField(null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(Rect rect, GUIContent label, long value, GUIStyle style)
        {
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            if (OnLocalControlRelease(rect, control))
            {
                value = delayedLongBuffer;
            }

            // Value buffer
            long buffer = value;

            if (localHotControl == control)
            {
                GUIHelper.PushColor(delayedActiveColor);
                buffer = delayedLongBuffer;
            }

            EditorGUI.BeginChangeCheck();
            buffer = LongField(rect, label, buffer, style);

            if (localHotControl == control)
            {
                GUIHelper.PopColor();
            }

            if (EditorGUI.EndChangeCheck())
            {
                localHotControl = control;
                delayedLongBuffer = buffer;
            }

            return value;
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(Rect rect, GUIContent label, long value)
        {
            return SirenixEditorFields.DelayedLongField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(Rect rect, string label, long value)
        {
            return SirenixEditorFields.DelayedLongField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(Rect rect, long value)
        {
            return SirenixEditorFields.DelayedLongField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(GUIContent label, long value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return SirenixEditorFields.DelayedLongField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(GUIContent label, long value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.DelayedLongField(label, value, null, options);
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(string label, long value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.DelayedLongField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed long field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static long DelayedLongField(long value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.DelayedLongField(null, value, null, options);
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(Rect rect, GUIContent label, float value, GUIStyle style)
        {
            Rect slideRect = rect.AlignRight(slideKnobWidth);
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);

            value = SirenixEditorGUI.SlideRect(slideRect, control, value);

            value = label != null ?
                EditorGUI.FloatField(rect, label, value, style ?? EditorStyles.numberField) :
                EditorGUI.FloatField(rect, value, style ?? EditorStyles.numberField);

            DrawSlideKnob(rect, control);

            return value;
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(Rect rect, GUIContent label, float value)
        {
            return FloatField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(Rect rect, string label, float value)
        {
            return FloatField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(Rect rect, float value)
        {
            return FloatField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return FloatField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(GUIContent label, float value, params GUILayoutOption[] options)
        {
            return FloatField(label, value, null, options);
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(string label, float value, params GUILayoutOption[] options)
        {
            return FloatField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a float field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float FloatField(float value, params GUILayoutOption[] options)
        {
            return FloatField(null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(Rect rect, GUIContent label, float value, GUIStyle style)
        {
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            if (OnLocalControlRelease(rect, control))
            {
                value = delayedFloatBuffer;
                GUI.changed = true;
            }

            // Value buffer
            float buffer = value;
            if (localHotControl == control)
            {
                GUIHelper.PushColor(delayedActiveColor);
                buffer = delayedFloatBuffer;
            }

            EditorGUI.BeginChangeCheck();
            buffer = FloatField(rect, label, buffer, style);

            if (localHotControl == control)
            {
                GUIHelper.PopColor();
            }

            if (EditorGUI.EndChangeCheck())
            {
                localHotControl = control;
                delayedFloatBuffer = buffer;
                GUI.changed = false;
            }

            return value;
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(Rect rect, GUIContent label, float value)
        {
            return DelayedFloatField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(Rect rect, string label, float value)
        {
            return DelayedFloatField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(Rect rect, float value)
        {
            return DelayedFloatField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return DelayedFloatField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(GUIContent label, float value, params GUILayoutOption[] options)
        {
            return DelayedFloatField(label, value, null, options);
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(string label, float value, params GUILayoutOption[] options)
        {
            return DelayedFloatField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed float field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float DelayedFloatField(float value, params GUILayoutOption[] options)
        {
            return DelayedFloatField(null, value, null, options);
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(Rect rect, GUIContent label, float value, float min, float max, GUIStyle style)
        {
            return label != null ?
                EditorGUI.Slider(rect, label, value, (min < max ? min : max), (max > min ? max : min)) :
                EditorGUI.Slider(rect, value, (min < max ? min : max), (max > min ? max : min));
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(Rect rect, GUIContent label, float value, float min, float max)
        {
            return RangeFloatField(rect, label, value, min, max, null);
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(Rect rect, string label, float value, float min, float max)
        {
            return RangeFloatField(rect, label != null ? GUIHelper.TempContent(label) : null, value, min, max, null);
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(Rect rect, float value, float min, float max)
        {
            return RangeFloatField(rect, null, value, min, max, null);
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(GUIContent label, float value, float min, float max, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return RangeFloatField(rect, label, value, min, max, style);
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(GUIContent label, float value, float min, float max, params GUILayoutOption[] options)
        {
            return RangeFloatField(label, value, min, max, null, options);
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(string label, float value, float min, float max, params GUILayoutOption[] options)
        {
            return RangeFloatField(label != null ? GUIHelper.TempContent(label) : null, value, min, max, null, options);
        }

        /// <summary>
        /// Draws a range field for floats.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static float RangeFloatField(float value, float min, float max, params GUILayoutOption[] options)
        {
            return RangeFloatField(null, value, min, max, null, options);
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(Rect rect, GUIContent label, double value, GUIStyle style)
        {
            Rect slideRect = rect.AlignRight(slideKnobWidth);
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            value = SirenixEditorGUI.SlideRectDouble(slideRect, control, value);

            value = label != null ?
                EditorGUI.DoubleField(rect, label, value, style ?? EditorStyles.numberField) :
                EditorGUI.DoubleField(rect, value, style ?? EditorStyles.numberField);

            DrawSlideKnob(rect, control);

            return value;
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(Rect rect, GUIContent label, double value)
        {
            return DoubleField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(Rect rect, string label, double value)
        {
            return DoubleField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(Rect rect, double value)
        {
            return DoubleField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(GUIContent label, double value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return DoubleField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(GUIContent label, double value, params GUILayoutOption[] options)
        {
            return DoubleField(label, value, null, options);
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(string label, double value, params GUILayoutOption[] options)
        {
            return DoubleField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a double field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DoubleField(double value, params GUILayoutOption[] options)
        {
            return DoubleField(null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(Rect rect, GUIContent label, double value, GUIStyle style)
        {
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            if (OnLocalControlRelease(rect, control))
            {
                value = delayedDoubleBuffer;
                GUI.changed = true;
            }

            // Value buffer
            double buffer = value;
            if (localHotControl == control)
            {
                GUIHelper.PushColor(delayedActiveColor);
                buffer = delayedDoubleBuffer;
            }

            EditorGUI.BeginChangeCheck();
            buffer = DoubleField(rect, label, buffer, style);
            if (localHotControl == control)
            {
                GUIHelper.PopColor();
            }
            if (EditorGUI.EndChangeCheck())
            {
                localHotControl = control;
                delayedDoubleBuffer = buffer;
                GUI.changed = false;
            }

            return value;
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(Rect rect, GUIContent label, double value)
        {
            return DelayedDoubleField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(Rect rect, string label, double value)
        {
            return DelayedDoubleField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(Rect rect, double value)
        {
            return DelayedDoubleField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(GUIContent label, double value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return DelayedDoubleField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(GUIContent label, double value, params GUILayoutOption[] options)
        {
            return DelayedDoubleField(label, value, null, options);
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(string label, double value, params GUILayoutOption[] options)
        {
            return DelayedDoubleField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed double field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static double DelayedDoubleField(double value, params GUILayoutOption[] options)
        {
            return DelayedDoubleField(null, value, null, options);
        }

        // @Todo
        // DoubleRange

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(Rect rect, GUIContent label, decimal value, GUIStyle style)
        {
            double d = (double)value;

            EditorGUI.BeginChangeCheck();

            // Slide label
            if (label != null)
            {
                Rect labelRect = rect.SetWidth(GUIHelper.BetterLabelWidth);
                rect = EditorGUI.PrefixLabel(rect, label);
                d = SirenixEditorGUI.SlideRectDouble(labelRect, EditorGUIUtility.GetControlID(FocusType.Passive), d);
            }

            // Slide knob
            Rect slideRect = rect.AlignRight(slideKnobWidth);
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            d = SirenixEditorGUI.SlideRectDouble(slideRect, control, d);

            if (EditorGUI.EndChangeCheck())
            {
                value = (decimal)d;
            }

            // Field
            string s = value.ToString(CultureInfo.InvariantCulture);
            s = DelayedTextField(rect, s);

            decimal dec;
            if (GUI.changed && decimal.TryParse(s, out dec))
            {
                value = dec;
            }

            DrawSlideKnob(rect, control);

            return value;
        }

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(Rect rect, GUIContent label, decimal value)
        {
            return DecimalField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(Rect rect, string label, decimal value)
        {
            return DecimalField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(Rect rect, decimal value)
        {
            return DecimalField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(GUIContent label, decimal value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return DecimalField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(GUIContent label, decimal value, params GUILayoutOption[] options)
        {
            return DecimalField(label, value, null, options);
        }

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(string label, decimal value, params GUILayoutOption[] options)
        {
            return DecimalField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a decimal field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static decimal DecimalField(decimal value, params GUILayoutOption[] options)
        {
            return DecimalField(null, value, null, options);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(Rect rect, GUIContent label, string value, GUIStyle style)
        {
            return label != null ?
                EditorGUI.TextField(rect, label, value, style ?? EditorStyles.textField) :
                EditorGUI.TextField(rect, value, style ?? EditorStyles.textField);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(Rect rect, GUIContent label, string value)
        {
            return TextField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(Rect rect, string label, string value)
        {
            return TextField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(Rect rect, string value)
        {
            return TextField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(GUIContent label, string value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return TextField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(GUIContent label, string value, params GUILayoutOption[] options)
        {
            return TextField(label, value, null, options);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(string label, string value, params GUILayoutOption[] options)
        {
            return TextField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a text field for strings.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string TextField(string value, params GUILayoutOption[] options)
        {
            return TextField(null, value, null, options);
        }

        // @Todo
        // Textbox

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(Rect rect, GUIContent label, string value, GUIStyle style)
        {
            int control = EditorGUIUtility.GetControlID(FocusType.Passive);
            if (OnLocalControlRelease(rect, control))
            {
                GUI.changed = true;
                value = delayedTextBuffer;
            }

            // Value buffer
            string buffer = value;
            if (localHotControl == control)
            {   
                GUIHelper.PushColor(delayedActiveColor);
                buffer = delayedTextBuffer;
            }

            EditorGUI.BeginChangeCheck();
            buffer = TextField(rect, label, buffer, style);

            if (localHotControl == control)
            {
                GUIHelper.PopColor();
            }

            if (EditorGUI.EndChangeCheck())
            {
                GUI.changed = false;
                localHotControl = control;
                delayedTextBuffer = buffer;
            }

            return value;

            //int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);

            //string text = value;
            //if (controlID == localHotControl)
            //{
            //    GUIHelper.PushColor(delayedActiveColor);
            //    text = delayedTextBuffer;
            //}

            //if (label != null)
            //{
            //    rect = EditorGUI.PrefixLabel(rect, label);
            //}

            //bool cancelEvent = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape;
            //bool confirmEvent = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;

            //EditorGUI.BeginChangeCheck();
            //text = EditorGUI.TextField(rect, text);

            //if (controlID == localHotControl)
            //{
            //    GUIHelper.PopColor();
            //}

            //if (EditorGUI.EndChangeCheck())
            //{
            //    localHotControl = controlID;
            //    delayedTextBuffer = text;
            //}

            //if (controlID == localHotControl && confirmEvent)
            //{
            //    localHotControl = 0;
            //    GUI.changed = true;
            //    Event.current.Use();
            //    return text;
            //}
            //else if (controlID == localHotControl && cancelEvent)
            //{
            //    localHotControl = 0;
            //    Event.current.Use();
            //    return value;
            //}
            //else
            //{
            //    return value;
            //}
        }

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(Rect rect, GUIContent label, string value)
        {
            return DelayedTextField(rect, label, value, null);
        }

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(Rect rect, string label, string value)
        {
            return DelayedTextField(rect, label != null ? GUIHelper.TempContent(label) : null, value, null);
        }

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(Rect rect, string value)
        {
            return DelayedTextField(rect, null, value, null);
        }

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(GUIContent label, string value, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.textField, options);
            return DelayedTextField(rect, label, value, style);
        }

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(GUIContent label, string value, params GUILayoutOption[] options)
        {
            return DelayedTextField(label, value, null, options);
        }

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(string label, string value, params GUILayoutOption[] options)
        {
            return DelayedTextField(label != null ? GUIHelper.TempContent(label) : null, value, null, options);
        }

        /// <summary>
        /// Draws a delayed text field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static string DelayedTextField(string value, params GUILayoutOption[] options)
        {
            return DelayedTextField(null, value, null, options);
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a file.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <returns>A path to a file.</returns>
        public static string FilePathField(Rect rect, GUIContent label, string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes)
        {
            bool needsProcessing = false;

            // Highlight path in Unity when field is clicked.
            GUIHelper.PushGUIEnabled(true);
            if (label != null && path.IsNullOrWhitespace() == false && rect.AlignLeft(GUIHelper.BetterLabelWidth).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.clickCount >= 2)
            {
                // Create a path relative to the unity project.
                string highlightPath = GetRelativePath(path, Directory.GetParent(Application.dataPath).FullName);

                if (highlightPath.IsNullOrWhitespace() == false)
                {
                    // Load the asset at the path.
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(highlightPath.Replace('\\', '/'));

                    // Then highlight it.
                    if (obj != null)
                    {
                        EditorGUIUtility.PingObject(obj);
                    }
                }

                Event.current.Use();
            }
            GUIHelper.PopGUIEnabled();

            // Drop zone for dragging files onto the field and using their path.
            {
                var droppedObject = DragAndDropUtilities.DropZone<UnityEngine.Object>(rect, null, false);
                if (droppedObject != null)
                {
                    // Get the path from the dropped object.
                    string pathBuffer = AssetDatabase.GetAssetPath(droppedObject);

                    // Test for extensions
                    bool accept = true;

                    if ((File.GetAttributes(pathBuffer) & FileAttributes.Directory) != 0)
                    {
                        // Don't accept folders.
                        accept = false;
                    }
                    else if (extensions.IsNullOrWhitespace() == false)
                    {
                        // Test if the dropped file has the correct extension.
                        string e = Path.GetExtension(pathBuffer).Trim('.');
                        if (e.IsNullOrWhitespace())
                        {
                            accept = false;
                        }
                        else if (extensions.Split(',', ';').Select(i => i.Trim(' ', '.', '*')).DefaultIfEmpty(e).Any(i => i.Equals(e, StringComparison.CurrentCultureIgnoreCase)) == false)
                        {
                            accept = false;
                        }
                    }

                    // The drop is acceptable.
                    if (accept)
                    {
                        path = pathBuffer;
                        needsProcessing = true;
                    }
                }
            }

            // Text field for the path.
            {
                EditorGUI.BeginChangeCheck();
                string pathBuffer = SirenixEditorFields.TextField(rect.AlignLeft(rect.width - 18), label, path);
                if (EditorGUI.EndChangeCheck())
                {
                    // Don't mark the path for processing here to allow users to input whatever they want. Only enforce forward/backslashing.
                    path = useBackslashes ? pathBuffer.Replace('/', '\\') : pathBuffer.Replace('\\', '/');
                }
            }

            // Can the field be edited?
            bool isEnabled = GUI.enabled;

            // The button should always be clickable, even if the field is disabled.
            if (Event.current.type != EventType.Repaint)
            {
                GUI.enabled = true;
            }

            // Open file panel to select file from explorer window.
            if (SirenixEditorGUI.IconButton(rect.AlignRight(18f).SetHeight(18f).SubY(1).AddX(1), EditorIcons.Folder))
            {
                // Create a path that Unity's file panel will open correctly.
                string directory = GetOpenExplorerPath(path, parentPath);

                if (isEnabled)
                {
                    // Open the file panel
                    string pathBuffer = EditorUtility.OpenFilePanel("Select File", directory, GetFilePanelExtensions(extensions));

                    if (pathBuffer.IsNullOrWhitespace() == false)
                    {
                        path = pathBuffer;
                        needsProcessing = true;
                    }
                }
                else
                {
                    // Open explorer for the directory, instead of the file panel.
                    System.Diagnostics.Process.Start(directory);
                }
            }

            // Reset GUI enabled.
            if (Event.current.type != EventType.Repaint)
            {
                GUI.enabled = isEnabled;
            }

            // Process the path to be relative to the parent path, absolute, and/or use backslashes.
            if (path.IsNullOrWhitespace() == false && needsProcessing)
            {
                // To make it simple, start by getting the absolute path.
                path = Path.GetFullPath(path);

                // Then make it relative if required.
                if (absolutePath == false)
                {
                    path = GetRelativePath(path, parentPath.IsNullOrWhitespace() ? Directory.GetParent(Application.dataPath).FullName : parentPath);
                }

                // Enforce use of forward or back slashes.
                path = useBackslashes ? path.Replace('/', '\\') : path.Replace('\\', '/');

                GUI.changed = true;
            }

            return path;
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a file.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <returns>A path to a file.</returns>
        public static string FilePathField(Rect rect, string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes)
        {
            return FilePathField(rect, null, path, parentPath, extensions, absolutePath, useBackslashes);
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a file.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>A path to a file.</returns>
        public static string FilePathField(GUIContent label, string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(options);
            return FilePathField(rect, label, path, parentPath, extensions, absolutePath, useBackslashes);
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a file.
        /// </summary>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>A path to a file.</returns>
        public static string FilePathField(string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
        {
            return FilePathField(null, path, parentPath, extensions, absolutePath, useBackslashes, options);
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a folder.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <returns>A path to a folder.</returns>
        public static string FolderPathField(Rect rect, GUIContent label, string path, string parentPath, bool absolutePath, bool useBackslashes)
        {
            bool needsProcessing = false;

            // Highlight path in Unity when field is clicked.
            GUIHelper.PushGUIEnabled(true);
            if (label != null && rect.AlignLeft(GUIHelper.BetterLabelWidth).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.clickCount >= 2)
            {
                // Create a path relative to the unity project.
                string highlightPath = GetRelativePath(path, Directory.GetParent(Application.dataPath).FullName);

                if (highlightPath.IsNullOrWhitespace() == false)
                {
                    // Load the asset at the path.
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(highlightPath.Replace('\\', '/'));

                    // Then highlight it.
                    if (obj != null)
                    {
                        EditorGUIUtility.PingObject(obj);
                    }
                }

                Event.current.Use();
            }
            GUIHelper.PopGUIEnabled();

            // Drop zone for dragging files onto the field and using their path.
            {
                var droppedObject = DragAndDropUtilities.DropZone<UnityEngine.Object>(rect, null, false);
                if (droppedObject != null)
                {
                    // Get the path from the dropped object.
                    string pathBuffer = AssetDatabase.GetAssetPath(droppedObject);

                    if ((File.GetAttributes(pathBuffer) & FileAttributes.Directory) == 0)
                    {
                        // Take the parent directory from the file path.
                        pathBuffer = Path.GetDirectoryName(pathBuffer);
                    }

                    path = pathBuffer;
                    needsProcessing = true;
                }
            }

            // Text field for the path.
            {
                EditorGUI.BeginChangeCheck();
                string pathBuffer = SirenixEditorFields.TextField(rect.AlignLeft(rect.width - 18), label, path);
                if (EditorGUI.EndChangeCheck())
                {
                    // Don't mark the path for processing here to allow users to input whatever they want. Only enforce forward/backslashing.
                    path = useBackslashes ? pathBuffer.Replace('/', '\\') : pathBuffer.Replace('\\', '/');
                }
            }

            // Can the field be edited?
            bool isEnabled = GUI.enabled;

            // The button should always be clickable, even if the field is disabled.
            if (Event.current.type != EventType.Repaint)
            {
                GUI.enabled = true;
            }

            // Open folder panel to select file from explorer window.
            if (SirenixEditorGUI.IconButton(rect.AlignRight(18f).SetHeight(18f).SubY(1).AddX(1), EditorIcons.Folder))
            {
                // Create a path that Unity's file panel will open correctly.
                string directory = GetOpenExplorerPath(path, parentPath);

                // Open the file panel
                if (isEnabled)
                {
                    string pathBuffer = EditorUtility.OpenFolderPanel("Select File", directory, "");

                    if (pathBuffer.IsNullOrWhitespace() == false)
                    {
                        path = pathBuffer;
                        needsProcessing = true;
                    }
                }
                else
                {
                    // Open explorer for the directory, instead of the folder panel.
                    System.Diagnostics.Process.Start(directory);
                }
            }

            // Reset GUI enabled.
            if (Event.current.type != EventType.Repaint)
            {
                GUI.enabled = isEnabled;
            }

            // Process the path to be relative to the parent path, absolute, and/or use backslashes.
            if (path.IsNullOrWhitespace() == false && needsProcessing)
            {
                // To make it simple, start by getting the absolute path.
                path = Path.GetFullPath(path);

                // Then make it relative if required.
                if (absolutePath == false)
                {
                    path = GetRelativePath(path, parentPath.IsNullOrWhitespace() ? Directory.GetParent(Application.dataPath).FullName : parentPath);
                }

                // Enforce use of forward or back slashes.
                path = useBackslashes ? path.Replace('/', '\\') : path.Replace('\\', '/');

                GUI.changed = true;
            }

            return path;
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a folder.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <returns>A path to a folder.</returns>
        public static string FolderPathField(Rect rect, string path, string parentPath, bool absolutePath, bool useBackslashes)
        {
            return FolderPathField(rect, null, path, parentPath, absolutePath, useBackslashes);
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a folder.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>A path to a folder.</returns>
        public static string FolderPathField(GUIContent label, string path, string parentPath, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(options);
            return FolderPathField(rect, label, path, parentPath, absolutePath, useBackslashes);
        }

        /// <summary>
        /// Draws a field that lets the user select a path to a folder.
        /// </summary>
        /// <param name="path">The current value.</param>
        /// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
        /// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
        /// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>A path to a folder.</returns>
        public static string FolderPathField(string path, string parentPath, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
        {
            return FolderPathField(null, path, parentPath, absolutePath, useBackslashes, options);
        }

        private static string GetRelativePath(string path, string parentPath)
        {
            if (parentPath.IsNullOrWhitespace())
            {
                // Nothing to make the path realtive to.
                return path;
            }
            if (path.IsNullOrWhitespace())
            {
                // Nothing to make relative.
                return null;
            }

            // Ensure parent path is rooted.
            parentPath = Path.GetFullPath(parentPath);

            // Ensure path is rooted.
            if (Path.IsPathRooted(path) == false)
            {
                // Parent path is already rooted.
                path = Path.Combine(parentPath, path);

                // Enforce forward slashes.
                path = path.Replace('\\', '/');
            }

            return PathUtilities.MakeRelative(parentPath, path);
        }

        private static string GetOpenExplorerPath(string path, string parentPath)
        {
            // Test if a path is specified.
            if (path.IsNullOrWhitespace())
            {
                // No path specified. Try uisng the parent path instead if any is specified.
                return parentPath.IsNullOrWhitespace() ? string.Empty : GetOpenExplorerPath(parentPath, null);
            }

            // Combine the path with the parent path.
            if (parentPath.IsNullOrWhitespace() == false && Path.IsPathRooted(path) == false)
            {
                path = Path.Combine(parentPath, path);
            }

            // Get Convert to absolute path.
            if (Path.IsPathRooted(path) == false)
            {
                path = Path.GetFullPath(path);
            }

            // Strip the file name from the path.
            if (File.Exists(path) && File.GetAttributes(path) != FileAttributes.Directory)
            {
                path = Path.GetDirectoryName(path);
            }

            // Convert to forward slashes to keep it simple for Unity.
            path = path.Replace('/', '\\').TrimEnd('/');

            // Find the best path.
            string result = FindFirstExistingPath(path);

            if (result.IsNullOrWhitespace() && parentPath.IsNullOrWhitespace() == false)
            {
                // No existing path found. Try using the path path instead. // @todo: is this actually necessary?
                return GetOpenExplorerPath(parentPath, null);
            }
            else
            {
                // No path found, and no parent path to try instead.
                return result;
            }
        }

        private static string FindFirstExistingPath(string path)
        {
            if (path.IsNullOrWhitespace())
            {
                // Couldn't find an existing path.
                return string.Empty;
            }
            else if (Directory.Exists(path))
            {
                // Use this path.
                return path.Replace('\\', '/').Trim('/');
            }
            else
            {
                // Go one level up.
                var parent = Directory.GetParent(path);
                if (parent == null)
                {
                    return string.Empty;
                }

                return FindFirstExistingPath(parent.ToString());
            }
        }

        private static string GetFilePanelExtensions(string extensions)
        {
            if (extensions.IsNullOrWhitespace())
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            var e = extensions.Split(',', ';').Select(i => i.Trim(' ', '.', '*')).Where(i => !i.IsNullOrWhitespace()).GetEnumerator();

            while (e.MoveNext())
            {
                if (builder.Length > 0)
                {
                    builder.Append(";*.");
                }
                builder.Append(e.Current);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Draws a prefix label for a vector field, that implements label dragging.
        /// </summary>
        public static Vector4 VectorPrefixSlideRect(Rect rect, Vector4 value)
        {
            int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.Layout)
            {
                return value;
            }

            // Working values
            Vector4 normal = value.sqrMagnitude > 0f ? value.normalized : Vector4.one;
            float length = value.magnitude;

            if (GUIUtility.hotControl == controlID)
            {
                normal = vectorNormalBuffer;
                length = vectorLengthBuffer;
            }
            else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                vectorNormalBuffer = normal;
                vectorLengthBuffer = length;
            }

            // Sliding rect
            EditorGUI.BeginChangeCheck();
            length = SirenixEditorGUI.SlideRect(rect, controlID, length);
            if (EditorGUI.EndChangeCheck())
            {
                vectorLengthBuffer = length;
                value = normal * length;
                value.x = (float)Math.Round(value.x, 2);
                value.y = (float)Math.Round(value.y, 2);
                value.z = (float)Math.Round(value.z, 2);
                value.w = (float)Math.Round(value.w, 2);
            }

            return value;
        }

        /// <summary>
        /// Draws a prefix label for a vector field, that implements label dragging.
        /// </summary>
        /// <param name="totalRect">The position and total size of the field.</param>
        /// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
        /// <param name="value">The value for the vector field.</param>
        /// <returns>The vector scaled by label dragging.</returns>
        public static Vector4 VectorPrefixLabel(ref Rect totalRect, GUIContent label, Vector4 value)
        {
            if (label == null) { return value; }

            // Contorl ID for slide label.
            int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);

            // Draw label and create label rect.
            Rect labelRect = new Rect(totalRect.x, totalRect.y, totalRect.width, totalRect.height);
            totalRect = EditorGUI.PrefixLabel(totalRect, label);
            labelRect.width -= totalRect.width;

            // Working values
            Vector4 normal = value.sqrMagnitude > 0f ? value.normalized : Vector4.one;
            float length = value.magnitude;

            if (GUIUtility.hotControl == controlID)
            {
                normal = vectorNormalBuffer;
                length = vectorLengthBuffer;
            }
            else if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                vectorNormalBuffer = normal;
                vectorLengthBuffer = length;
            }

            // Sliding rect
            EditorGUI.BeginChangeCheck();
            length = SirenixEditorGUI.SlideRect(labelRect, controlID, length);
            if (EditorGUI.EndChangeCheck())
            {
                vectorLengthBuffer = length;
                value = normal * length;
                value.x = (float)Math.Round(value.x, 2);
                value.y = (float)Math.Round(value.y, 2);
                value.z = (float)Math.Round(value.z, 2);
                value.w = (float)Math.Round(value.w, 2);
            }

            return value;
        }

        /// <summary>
        /// Draws a prefix label for a vector field, that implements label dragging.
        /// </summary>
        /// <param name="totalRect">The position and total size of the field.</param>
        /// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
        /// <param name="value">The value for the vector field.</param>
        /// <returns>The vector scaled by label dragging.</returns>
        public static Vector4 VectorPrefixLabel(ref Rect totalRect, string label, Vector4 value)
        {
            return VectorPrefixLabel(ref totalRect, GUIHelper.TempContent(label), value);
        }

        /// <summary>
        /// Draws a prefix label for a vector field, that implements label dragging.
        /// </summary>
        /// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
        /// <param name="value">The value for the vector field.</param>
        /// <returns>The vector scaled by label dragging.</returns>
        public static Vector4 VectorPrefixLabel(GUIContent label, Vector4 value)
        {
            if (label == null) { return value; }

            // Contorl ID for slide label.
            int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);

            // Draw label and create label rect.
            EditorGUILayout.PrefixLabel(label);
            Rect labelRect = GUILayoutUtility.GetLastRect();

            // Working values
            Vector4 normal = value.sqrMagnitude > 0f ? value.normalized : Vector4.one;
            float length = value.magnitude;

            if (GUIUtility.hotControl == controlID)
            {
                normal = vectorNormalBuffer;
                length = vectorLengthBuffer;
            }
            else if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                vectorNormalBuffer = normal;
                vectorLengthBuffer = length;
            }

            // Sliding rect
            EditorGUI.BeginChangeCheck();
            length = SirenixEditorGUI.SlideRect(labelRect, controlID, length);
            if (EditorGUI.EndChangeCheck())
            {
                vectorLengthBuffer = length;
                value = normal * length;
                value.x = (float)Math.Round(value.x, 2);
                value.y = (float)Math.Round(value.y, 2);
                value.z = (float)Math.Round(value.z, 2);
                value.w = (float)Math.Round(value.w, 2);
            }

            return value;
        }

        /// <summary>
        /// Draws a prefix label for a vector field, that implements label dragging.
        /// </summary>
        /// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
        /// <param name="value">The value for the vector field.</param>
        /// <returns>The vector scaled by label dragging.</returns>
        public static Vector4 VectorPrefixLabel(string label, Vector4 value)
        {
            return VectorPrefixLabel(GUIHelper.TempContent(label), value);
        }

        /// <summary>
        /// Draws a Vector2 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector2 Vector2Field(Rect rect, GUIContent label, Vector2 value)
        {
            value = (Vector2)VectorPrefixLabel(ref rect, label, (Vector4)value);

            bool showLabels = !(ResponsiveVectorComponentFields && rect.width < 185);

            GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
            GUIHelper.PushIndentLevel(0);
            value.x = FloatField(rect.Split(0, 3).HorizontalPadding(0, 2), showLabels ? "X" : null, value.x);
            value.y = FloatField(rect.Split(1, 3).HorizontalPadding(0, 2), showLabels ? "Y" : null, value.y);
            GUIHelper.PopIndentLevel();
            GUIHelper.PopLabelWidth();

            return value;
        }

        /// <summary>
        /// Draws a Vector2 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector2 Vector2Field(Rect rect, string label, Vector2 value)
        {
            return SirenixEditorFields.Vector2Field(rect, label != null ? GUIHelper.TempContent(label) : null, value);
        }

        /// <summary>
        /// Draws a Vector2 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector2 Vector2Field(Rect rect, Vector2 value)
        {
            return SirenixEditorFields.Vector2Field(rect, (GUIContent)null, value);
        }

        /// <summary>
        /// Draws a Vector2 field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector2 Vector2Field(GUIContent label, Vector2 value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
            return SirenixEditorFields.Vector2Field(rect, label, value);
        }

        /// <summary>
        /// Draws a Vector2 field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector2 Vector2Field(string label, Vector2 value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.Vector2Field(label != null ? GUIHelper.TempContent(label) : null, value, options);
        }

        /// <summary>
        /// Draws a Vector2 field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector2 Vector2Field(Vector2 value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.Vector2Field((GUIContent)null, value, options);
        }

        /// <summary>
        /// Draws a Vector3 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector3 Vector3Field(Rect rect, GUIContent label, Vector3 value)
        {
            // Sliding label
            value = (Vector3)VectorPrefixLabel(ref rect, label, (Vector3)value);

            bool showLabels = !(ResponsiveVectorComponentFields && rect.width < 185);

            // Field
            GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
            GUIHelper.PushIndentLevel(0);
            value.x = FloatField(rect.Split(0, 3).HorizontalPadding(0, 2), showLabels ? "X" : null, value.x);
            value.y = FloatField(rect.Split(1, 3).HorizontalPadding(0, 1), showLabels ? "Y" : null, value.y);
            value.z = FloatField(rect.Split(2, 3).HorizontalPadding(1, 0), showLabels ? "Z" : null, value.z);
            GUIHelper.PopIndentLevel();
            GUIHelper.PopLabelWidth();

            return value;
        }

        /// <summary>
        /// Draws a Vector3 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector3 Vector3Field(Rect rect, string label, Vector3 value)
        {
            return SirenixEditorFields.Vector3Field(rect, label != null ? GUIHelper.TempContent(label) : null, value);
        }

        /// <summary>
        /// Draws a Vector3 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector3 Vector3Field(Rect rect, Vector3 value)
        {
            return SirenixEditorFields.Vector3Field(rect, (GUIContent)null, value);
        }

        /// <summary>
        /// Draws a Vector3 field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector3 Vector3Field(GUIContent label, Vector3 value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
            return SirenixEditorFields.Vector3Field(rect, label, value);
        }

        /// <summary>
        /// Draws a Vector3 field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector3 Vector3Field(string label, Vector3 value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.Vector3Field(label != null ? GUIHelper.TempContent(label) : null, value, options);
        }

        /// <summary>
        /// Draws a Vector3 field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector3 Vector3Field(Vector3 value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.Vector3Field((GUIContent)null, value, options);
        }

        /// <summary>
        /// Draws a Vector4 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector4 Vector4Field(Rect rect, GUIContent label, Vector4 value)
        {
            value = VectorPrefixLabel(ref rect, label, value);

            bool showLabels = !(ResponsiveVectorComponentFields && rect.width < 185);

            // Field
            GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
            GUIHelper.PushIndentLevel(0);
            value.x = FloatField(rect.Split(0, 4).HorizontalPadding(0, 2), showLabels ? "X" : null, value.x);
            value.y = FloatField(rect.Split(1, 4).HorizontalPadding(0, 2), showLabels ? "Y" : null, value.y);
            value.z = FloatField(rect.Split(2, 4).HorizontalPadding(0, 2), showLabels ? "Z" : null, value.z);
            value.w = FloatField(rect.Split(3, 4).HorizontalPadding(0, 2), showLabels ? "W" : null, value.w);
            GUIHelper.PopIndentLevel();
            GUIHelper.PopLabelWidth();

            return value;
        }

        /// <summary>
        /// Draws a Vector4 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector4 Vector4Field(Rect rect, string label, Vector4 value)
        {
            return SirenixEditorFields.Vector4Field(rect, label != null ? GUIHelper.TempContent(label) : null, value);
        }

        /// <summary>
        /// Draws a Vector4 field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector4 Vector4Field(Rect rect, Vector4 value)
        {
            return SirenixEditorFields.Vector4Field(rect, (GUIContent)null, value);
        }

        /// <summary>
        /// Draws a Vector4 field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector4 Vector4Field(GUIContent label, Vector4 value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
            return SirenixEditorFields.Vector4Field(rect, label, value);
        }

        /// <summary>
        /// Draws a Vector4 field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector4 Vector4Field(string label, Vector4 value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.Vector4Field(label != null ? GUIHelper.TempContent(label) : null, value, options);
        }

        /// <summary>
        /// Draws a Vector4 field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Vector4 Vector4Field(Vector4 value, params GUILayoutOption[] options)
        {
            return SirenixEditorFields.Vector4Field((GUIContent)null, value, options);
        }

        /// <summary>
        /// Draws a Color field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value"></param>
        /// <returns>Value assigned to the field.</returns>
        public static Color ColorField(Rect rect, GUIContent label, Color value)
        {
            return label != null ?
                EditorGUI.ColorField(rect, label, value) :
                EditorGUI.ColorField(rect, value);
        }

        /// <summary>
        /// Draws a Color field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value"></param>
        /// <returns>Value assigned to the field.</returns>
        public static Color ColorField(Rect rect, string label, Color value)
        {
            return ColorField(rect, label != null ? GUIHelper.TempContent(label) : (GUIContent)null, value);
        }

        /// <summary>
        /// Draws a Color field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value"></param>
        /// <returns>Value assigned to the field.</returns>
        public static Color ColorField(Rect rect, Color value)
        {
            return ColorField(rect, (GUIContent)null, value);
        }

        /// <summary>
        /// Draws a Color field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value"></param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Color ColorField(GUIContent label, Color value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, options);
            return ColorField(rect, label, value);
        }

        /// <summary>
        /// Draws a Color field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value"></param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Color ColorField(string label, Color value, params GUILayoutOption[] options)
        {
            return ColorField(label != null ? GUIHelper.TempContent(label) : null, value, options);
        }

        /// <summary>
        /// Draws a Color field.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Color ColorField(Color value, params GUILayoutOption[] options)
        {
            return ColorField((GUIContent)null, value, options);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="limits">The min and max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
        public static Vector2 MinMaxSlider(Rect rect, GUIContent label, Vector2 value, Vector2 limits, bool showFields = false)
        {
            // Initialize styles
            if (minMaxSliderStyle == null)
            {
                minMaxSliderStyle = (GUIStyle)"MinMaxHorizontalSliderThumb";
            }
            if (sliderBackground == null)
            {
                sliderBackground = GUI.skin.horizontalSlider;
            }

            // Label
            //rect = GUIHelper.IndentRect(rect);
            Rect totalRect = rect;
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            // Slide rects
            int fieldWidth = showFields ? Mathf.RoundToInt(rect.width * 0.3f * 0.5f) : 0;
            Rect fieldRect = rect.AddX(fieldWidth).SetWidth(rect.width - fieldWidth * 2 - 11).HorizontalPadding(4);
            Rect controlRect = fieldRect.SetXMax(Mathf.RoundToInt(fieldRect.x + fieldRect.width * MathUtilities.LinearStep(limits.x, limits.y, value.y) + 11))
                .AddXMin(Mathf.RoundToInt(fieldRect.width * MathUtilities.LinearStep(limits.x, limits.y, value.x)));

            // Fields
            if (showFields)
            {
                GUIHelper.PushIndentLevel(0);
                EditorGUI.BeginChangeCheck();
                float newX = SirenixEditorFields.FloatField(rect.AlignLeft(fieldWidth), value.x);
                if (EditorGUI.EndChangeCheck())
                {
                    value.x = newX;
                    value.x = Mathf.Clamp(value.x, limits.x, limits.y);
                    GUI.changed = true;
                }
                EditorGUI.BeginChangeCheck();
                float newY = SirenixEditorFields.FloatField(rect.AlignRight(fieldWidth), value.y);
                if (EditorGUI.EndChangeCheck())
                {
                    value.y = newY;
                    value.y = Mathf.Clamp(value.y, limits.x, limits.y);
                    GUI.changed = true;
                }
                GUIHelper.PopIndentLevel();
            }

            // Slider controls
            if (Event.current.IsHovering(fieldRect))
            {
                GUIHelper.RequestRepaint();
            }
            if (Event.current.OnMouseDown(fieldRect.SetWidth(fieldRect.width + 11), 0, true))
            {
                GUIUtility.hotControl = controlId;
                localHotControl = (int)
                    (Event.current.control ? MinMaxSliderLocalControl.Bar :
                    Event.current.mousePosition.x <= controlRect.xMin ? MinMaxSliderLocalControl.Min :
                    Event.current.mousePosition.x >= controlRect.xMax ? MinMaxSliderLocalControl.Max :
                    Mathf.Abs(controlRect.xMin - Event.current.mousePosition.x) < Mathf.Abs(controlRect.xMax - Event.current.mousePosition.x) ? MinMaxSliderLocalControl.Min : MinMaxSliderLocalControl.Max);

                // Update value.
                if (localHotControl == (int)MinMaxSliderLocalControl.Min)
                {
                    value.x = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
                    value.x = Mathf.Min(value.x, value.y);
                }
                else if (localHotControl == (int)MinMaxSliderLocalControl.Max)
                {
                    value.y = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
                    value.y = Mathf.Max(value.x, value.y);
                }

                GUI.changed = true;
            }
            else if (GUIUtility.hotControl == controlId)
            {
                if (Event.current.rawType == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                else if (Event.current.OnMouseMoveDrag(true))
                {
                    if (localHotControl == (int)MinMaxSliderLocalControl.Min)
                    {
                        value.x = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
                        value.x = Mathf.Min(value.x, value.y);
                    }
                    else if (localHotControl == (int)MinMaxSliderLocalControl.Max)
                    {
                        value.y = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
                        value.y = Mathf.Max(value.x, value.y);
                    }
                    else
                    {
                        controlRect.x = Mathf.Clamp(controlRect.x + Event.current.delta.x, fieldRect.x, fieldRect.xMax + 11 - controlRect.width);
                        value.x = Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.x, fieldRect.xMax, controlRect.x));
                        value.y = Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.x, fieldRect.xMax, controlRect.xMax - 11));
                    }

                    GUIHelper.RequestRepaint();
                    GUI.changed = true;
                }
            }

            if (Event.current.OnRepaint())
            {
                EditorGUIUtility.AddCursorRect(controlRect, Event.current.control || GUIUtility.hotControl == controlId && localHotControl == (int)MinMaxSliderLocalControl.Bar ? MouseCursor.Link : MouseCursor.SlideArrow);

                if (UnityVersion.IsVersionOrGreater(2019, 3))
                {
                    controlRect = controlRect.AddY(3.5f);
                }

                sliderBackground.Draw(fieldRect.SetWidth(fieldRect.width + 11).AddY(-1), GUIContent.none, 0);

                if (!EditorGUI.showMixedValue)
                {
                    minMaxSliderStyle.Draw(controlRect.MinWidth(11), GUIContent.none, controlId);
                }

                if (!EditorGUI.showMixedValue && (Event.current.IsHovering(totalRect) || GUIUtility.hotControl == controlId))
                {
                    Rect floatRect = fieldRect.SetWidth(fieldRect.width + 11);

                    GUIContent xLabel = new GUIContent(MathUtilities.DiscardLeastSignificantDecimal(value.x).ToString());
                    GUIContent yLabel = new GUIContent(MathUtilities.DiscardLeastSignificantDecimal(value.y).ToString());
                    GUIContent minLabel = new GUIContent(limits.x.ToString());
                    GUIContent maxLabel = new GUIContent(limits.y.ToString());

                    if (minMaxFloatingLabelStyle == null)
                    {
                        minMaxFloatingLabelStyle = new GUIStyle((GUIStyle)"ProfilerBadge")
                        {
                            font = EditorStyles.miniButton.font,
                            fontStyle = EditorStyles.miniButton.fontStyle,
                            fontSize = EditorStyles.miniButton.fontSize,
                            alignment = TextAnchor.MiddleCenter,
                        };
                    }

                    var size = minMaxFloatingLabelStyle.CalcSize(xLabel);
                    var xRect = floatRect.SetSize(size).SetCenterX(controlRect.xMin).AddY(-size.y).Expand(4, 0);

                    size = minMaxFloatingLabelStyle.CalcSize(yLabel);
                    var yRect = floatRect.SetSize(size).SetCenterX(controlRect.xMax).AddY(-size.y).Expand(4, 0);

                    size = minMaxFloatingLabelStyle.CalcSize(minLabel);
                    var minRect = floatRect.SetSize(size).SetCenterX(fieldRect.xMin).AddY(-size.y).Expand(4, 0);

                    size = minMaxFloatingLabelStyle.CalcSize(maxLabel);
                    var maxRect = floatRect.AlignRight(size.x).SetHeight(size.y).AddY(-size.y).Expand(4, 0);

                    // Overlapping
                    if (xRect.xMax + 4 > yRect.xMin)
                    {
                        float d = xRect.xMax + 4 - yRect.xMin;
                        xRect.x -= Mathf.RoundToInt(d * 0.5f);
                        yRect.x += Mathf.RoundToInt(d * 0.5f);
                    }

                    if (minRect.xMax + 4 < xRect.xMin)
                    {
                        minMaxFloatingLabelStyle.Draw(minRect, minLabel, -1);
                    }
                    if (maxRect.xMin - 4 > yRect.xMax)
                    {
                        minMaxFloatingLabelStyle.Draw(maxRect, maxLabel, -1);
                    }

                    minMaxFloatingLabelStyle.Draw(xRect, xLabel, -1);
                    minMaxFloatingLabelStyle.Draw(yRect, yLabel, -1);
                }
            }

            return value;
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="limits">The min and max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
        public static Vector2 MinMaxSlider(Rect rect, string label, Vector2 value, Vector2 limits, bool showFields = false)
        {
            return MinMaxSlider(rect, GUIHelper.TempContent(label), value, limits, showFields);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <param name="limits">The min and max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
        public static Vector2 MinMaxSlider(Rect rect, Vector2 value, Vector2 limits, bool showFields)
        {
            return MinMaxSlider(rect, (GUIContent)null, value, limits, showFields);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="limits">The min and max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
        public static Vector2 MinMaxSlider(GUIContent label, Vector2 value, Vector2 limits, bool showFields = false, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, options);
            return MinMaxSlider(rect, label, value, limits, showFields);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="limits">The min and max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
        public static Vector2 MinMaxSlider(string label, Vector2 value, Vector2 limits, bool showFields = false, params GUILayoutOption[] options)
        {
            return MinMaxSlider(GUIHelper.TempContent(label), value, limits, showFields, options);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="limits">The min and max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
        public static Vector2 MinMaxSlider(Vector2 value, Vector2 limits, bool showFields, params GUILayoutOption[] options)
        {
            return MinMaxSlider((GUIContent)null, value, limits, showFields, options);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="minValue">Current min value.</param>
        /// <param name="maxValue">Current max value.</param>
        /// <param name="minLimit">The min limit for the value.</param>
        /// <param name="maxLimit">The max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        public static void MinMaxSlider(Rect rect, GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false)
        {
            Vector2 value = new Vector2(minValue, maxValue);
            Vector2 limits = new Vector2(minLimit, maxLimit);
            value = MinMaxSlider(rect, label, value, limits, showFields);
            minValue = value.x;
            maxValue = value.y;
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="minValue">Current min value.</param>
        /// <param name="maxValue">Current max value.</param>
        /// <param name="minLimit">The min limit for the value.</param>
        /// <param name="maxLimit">The max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        public static void MinMaxSlider(Rect rect, string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false)
        {
            MinMaxSlider(rect, GUIHelper.TempContent(label), ref minValue, ref maxValue, minLimit, maxLimit, showFields);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="minValue">Current min value.</param>
        /// <param name="maxValue">Current max value.</param>
        /// <param name="minLimit">The min limit for the value.</param>
        /// <param name="maxLimit">The max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        public static void MinMaxSlider(Rect rect, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields)
        {
            MinMaxSlider(rect, (GUIContent)null, ref minValue, ref maxValue, minLimit, maxLimit, showFields);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="minValue">Current min value.</param>
        /// <param name="maxValue">Current max value.</param>
        /// <param name="minLimit">The min limit for the value.</param>
        /// <param name="maxLimit">The max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <param name="options">Layout options.</param>
        public static void MinMaxSlider(GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, options);
            MinMaxSlider(rect, label, ref minValue, ref maxValue, minLimit, maxLimit, showFields);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="minValue">Current min value.</param>
        /// <param name="maxValue">Current max value.</param>
        /// <param name="minLimit">The min limit for the value.</param>
        /// <param name="maxLimit">The max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <param name="options">Layout options.</param>
        public static void MinMaxSlider(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false, params GUILayoutOption[] options)
        {
            MinMaxSlider(GUIHelper.TempContent(label), ref minValue, ref maxValue, minLimit, maxLimit, showFields, options);
        }

        /// <summary>
        /// Draws a slider for setting two values between a min and a max limit.
        /// </summary>
        /// <param name="minValue">Current min value.</param>
        /// <param name="maxValue">Current max value.</param>
        /// <param name="minLimit">The min limit for the value.</param>
        /// <param name="maxLimit">The max limit for the value.</param>
        /// <param name="showFields">Show fields for min and max value.</param>
        /// <param name="options">Layout options.</param>
        public static void MinMaxSlider(ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields, params GUILayoutOption[] options)
        {
            MinMaxSlider((GUIContent)null, ref minValue, ref maxValue, minLimit, maxLimit, showFields, options);
        }

        /// <summary>
        /// Draws a rotation field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="mode">Draw mode for rotation field.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion RotationField(Rect rect, GUIContent label, Quaternion value, QuaternionDrawMode mode)
        {
            switch (mode)
            {
                case QuaternionDrawMode.Eulers:
                    return EulerField(rect, label, value);

                case QuaternionDrawMode.AngleAxis:
                    return AngleAxisField(rect, label, value);

                case QuaternionDrawMode.Raw:
                    return QuaternionField(rect, label, value);

                default:
                    throw new NotImplementedException("Unknown draw mode: " + mode.ToString());
            }
        }

        /// <summary>
        /// Draws a rotation field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="mode">Draw mode for rotation field.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion RotationField(Rect rect, string label, Quaternion value, QuaternionDrawMode mode)
        {
            return RotationField(rect, label != null ? GUIHelper.TempContent(label) : null, value, mode);
        }

        /// <summary>
        /// Draws a rotation field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <param name="mode">Draw mode for rotation field.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion RotationField(Rect rect, Quaternion value, QuaternionDrawMode mode)
        {
            return RotationField(rect, (GUIContent)null, value, mode);
        }

        /// <summary>
        /// Draws a rotation field for a quaternion.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="mode">Draw mode for rotation field.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion RotationField(GUIContent label, Quaternion value, QuaternionDrawMode mode, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
            return RotationField(rect, label, value, mode);
        }

        /// <summary>
        /// Draws a rotation field for a quaternion.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="mode">Draw mode for rotation field.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion RotationField(string label, Quaternion value, QuaternionDrawMode mode, params GUILayoutOption[] options)
        {
            return RotationField(label != null ? GUIHelper.TempContent(label) : null, value, mode, options);
        }

        /// <summary>
        /// Draws a rotation field for a quaternion.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="mode">Draw mode for rotation field.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion RotationField(Quaternion value, QuaternionDrawMode mode, params GUILayoutOption[] options)
        {
            return RotationField((GUIContent)null, value, mode, options);
        }

        /// <summary>
        /// Draws an euler field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion EulerField(Rect rect, GUIContent label, Quaternion value)
        {
            // Start of field ID.
            int beginID = EditorGUIUtility.GetControlID(FocusType.Passive);

            // Draw the label, if any.
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            // Update buffer.
            var context = GUIHelper.GetTemporaryContext<QuaternionContextBuffer>("SirenixEditorFields.EulerField_ID:" + beginID, "QuaternionFieldBuffer").Value;
            if (localHotControl != beginID || Event.current.type == EventType.Repaint && !context.IsUsed)
            {
                if (localHotControl == beginID)
                {
                    localHotControl = 0;
                }
                context.Set(value, QuaternionDrawMode.Eulers);
            }

            // Draw field.
            EditorGUI.BeginChangeCheck();
            context.Eulers = Vector3Field(rect, context.Eulers);
            if (EditorGUI.EndChangeCheck())
            {
                localHotControl = beginID;
                value = Quaternion.Euler(context.Eulers);
                GUI.changed = true;
            }

            // End of field ID.
            int endID = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.Repaint)
            {
                // Reset buffer IsUsed value at end of frame.
                context.IsUsed = false;
            }
            else
            {
                // Update context IsUsed value.
                context.IsUsed =
                    // Don't override IsUsed, if the context is already in use.
                    context.IsUsed ||
                    // Current GUI control is between begin and end ID.
                    GUIUtility.hotControl > beginID && GUIUtility.hotControl < endID ||
                    GUIUtility.keyboardControl > beginID && GUIUtility.keyboardControl < endID;
            }

            return value;
        }

        /// <summary>
        /// Draws an euler field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion EulerField(Rect rect, string label, Quaternion value)
        {
            return EulerField(rect, label != null ? GUIHelper.TempContent(label) : null, value);
        }

        /// <summary>
        /// Draws an euler field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion EulerField(Rect rect, Quaternion value)
        {
            return EulerField(rect, (GUIContent)null, value);
        }

        /// <summary>
        /// Draws an euler field for a quaternion.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion EulerField(GUIContent label, Quaternion value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
            return EulerField(rect, label, value);
        }

        /// <summary>
        /// Draws an euler field for a quaternion.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion EulerField(string label, Quaternion value, params GUILayoutOption[] options)
        {
            return EulerField(label != null ? GUIHelper.TempContent(label) : null, value, options);
        }

        /// <summary>
        /// Draws an euler field for a quaternion.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion EulerField(Quaternion value, params GUILayoutOption[] options)
        {
            return EulerField((GUIContent)null, value, options);
        }

        /// <summary>
        /// Draws an angle axis field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion AngleAxisField(Rect rect, GUIContent label, Quaternion value)
        {
            // Start of field ID.
            int beginID = GUIUtility.GetControlID(FocusType.Passive);

            // Draw the label, if any.
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            // Update buffer
            var context = GUIHelper.GetTemporaryContext<QuaternionContextBuffer>("SirenixEditorFields.AngleAxisField_ID:" + beginID, "QuaternionFieldBuffer").Value;
            if (localHotControl != beginID || Event.current.type == EventType.Repaint && !context.IsUsed)
            {
                if (localHotControl == beginID)
                {
                    localHotControl = 0;
                }
                context.Set(value, QuaternionDrawMode.AngleAxis);
            }

            // Rects
            Rect axisRect = rect.SetWidth(rect.width * 0.65f);
            Rect angleRect = rect.AlignRight(rect.width - axisRect.width);

            // Field
            bool showLabels = !(ResponsiveVectorComponentFields && rect.width < 185);

            // Draw field.
            EditorGUI.BeginChangeCheck();

            GUIHelper.PushIndentLevel(0);
            GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
            Vector3 axis = context.Axis;
            axis.x = FloatField(axisRect.Split(0, 3), showLabels ? "X" : null, axis.x);
            axis.y = FloatField(axisRect.Split(1, 3), showLabels ? "Y" : null, axis.y);
            axis.z = FloatField(axisRect.Split(2, 3), showLabels ? "Z" : null, axis.z);
            context.Axis = axis;
            GUIHelper.PopLabelWidth();

            GUIHelper.PushLabelWidth(38f);
            context.Angle = FloatField(angleRect, showLabels ? "Angle" : null, context.Angle);
            GUIHelper.PopLabelWidth();
            GUIHelper.PopIndentLevel();

            if (EditorGUI.EndChangeCheck())
            {
                localHotControl = beginID;
                value = Quaternion.AngleAxis(MathUtilities.Wrap(context.Angle, 0f, 360f), context.Axis.normalized);

                GUI.changed = true;
            }

            // End of field ID.
            int endID = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.Repaint)
            {
                // Reset buffer IsUsed value at end of frame.
                context.IsUsed = false;
            }
            else
            {
                // Update context IsUsed value.
                context.IsUsed =
                    // Don't override IsUsed, if the context is already in use.
                    context.IsUsed ||
                    // Current GUI control is between begin and end ID.
                    GUIUtility.hotControl > beginID && GUIUtility.hotControl < endID ||
                    GUIUtility.keyboardControl > beginID && GUIUtility.keyboardControl < endID;
            }

            return value;
        }

        /// <summary>
        /// Draws an angle axis field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion AngleAxisField(Rect rect, string label, Quaternion value)
        {
            return AngleAxisField(rect, label != null ? GUIHelper.TempContent(label) : null, value);
        }

        /// <summary>
        /// Draws an angle axis field for a quaternion.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion AngleAxisField(Rect rect, Quaternion value)
        {
            return AngleAxisField(rect, (GUIContent)null, value);
        }

        /// <summary>
        /// Draws an angle axis field for a quaternion.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion AngleAxisField(GUIContent label, Quaternion value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
            return AngleAxisField(rect, label, value);
        }

        /// <summary>
        /// Draws an angle axis field for a quaternion.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion AngleAxisField(string label, Quaternion value, params GUILayoutOption[] options)
        {
            return AngleAxisField(label != null ? GUIHelper.TempContent(label) : null, value, options);
        }

        /// <summary>
        /// Draws an angle axis field for a quaternion.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion AngleAxisField(Quaternion value, params GUILayoutOption[] options)
        {
            return AngleAxisField((GUIContent)null, value, options);
        }

        /// <summary>
        /// Draws a quaternion field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion QuaternionField(Rect rect, GUIContent label, Quaternion value)
        {
            // Start of field ID.
            int beginID = EditorGUIUtility.GetControlID(FocusType.Passive);

            // Draw the label, if any.
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            // Update buffer
            var context = GUIHelper.GetTemporaryContext<QuaternionContextBuffer>("SirenixEditorFields.QuaternionField_ID:" + beginID, "QuaternionFieldBuffer").Value;
            if (localHotControl != beginID || Event.current.type == EventType.Repaint && !context.IsUsed)
            {
                if (localHotControl == beginID)
                {
                    localHotControl = 0;
                }
                context.Set(value, QuaternionDrawMode.Raw);
            }

            // Draw field.
            EditorGUI.BeginChangeCheck();
            GUIHelper.PushIndentLevel(0);
            context.Raw = Vector4Field(rect, context.Raw);
            GUIHelper.PopIndentLevel();
            if (EditorGUI.EndChangeCheck())
            {
                localHotControl = beginID;
                value.x = context.Raw.x;
                value.y = context.Raw.y;
                value.z = context.Raw.z;
                value.w = context.Raw.w;
                GUI.changed = true;
            }

            // End of field ID.
            int endID = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.Repaint)
            {
                // Reset buffer IsUsed value at end of frame.
                context.IsUsed = false;
            }
            else
            {
                // Update context IsUsed value.
                context.IsUsed =
                    // Don't override IsUsed, if the context is already in use.
                    context.IsUsed ||
                    // Current GUI control is between begin and end ID.
                    GUIUtility.hotControl > beginID && GUIUtility.hotControl < endID ||
                    GUIUtility.keyboardControl > beginID && GUIUtility.keyboardControl < endID;
            }

            return value;
        }

        /// <summary>
        /// Draws a quaternion field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion QuaternionField(Rect rect, string label, Quaternion value)
        {
            return QuaternionField(rect, label != null ? GUIHelper.TempContent(label) : null, value);
        }

        /// <summary>
        /// Draws a quaternion field.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="value">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion QuaternionField(Rect rect, Quaternion value)
        {
            return QuaternionField(rect, (GUIContent)null, value);
        }

        /// <summary>
        /// Draws a quaternion field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion QuaternionField(GUIContent label, Quaternion value, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
            return QuaternionField(rect, label, value);
        }

        /// <summary>
        /// Draws a quaternion field.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion QuaternionField(string label, Quaternion value, params GUILayoutOption[] options)
        {
            return QuaternionField(label != null ? GUIHelper.TempContent(label) : null, value, options);
        }

        /// <summary>
        /// Draws a quaternion field.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Quaternion QuaternionField(Quaternion value, params GUILayoutOption[] options)
        {
            return QuaternionField((GUIContent)null, value, options);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(Rect rect, GUIContent label, int selected, string[] itemNames, GUIStyle style)
        {
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            return EditorGUI.Popup(rect, selected, itemNames, style ?? EditorStyles.popup);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(Rect rect, GUIContent label, int selected, string[] itemNames)
        {
            return Dropdown(rect, label, selected, itemNames, null);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(Rect rect, string label, int selected, string[] itemNames)
        {
            return Dropdown(rect, label != null ? GUIHelper.TempContent(label) : null, selected, itemNames, null);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(Rect rect, int selected, string[] itemNames)
        {
            return Dropdown(rect, null, selected, itemNames, null);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(GUIContent label, int selected, string[] itemNames, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return Dropdown(rect, label, selected, itemNames, style);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(GUIContent label, int selected, string[] itemNames, params GUILayoutOption[] options)
        {
            return Dropdown(label, selected, itemNames, null, options);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(string label, int selected, string[] itemNames, params GUILayoutOption[] options)
        {
            return Dropdown(label != null ? GUIHelper.TempContent(label) : null, selected, itemNames, null, options);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <param name="selected">Current value.</param>
        /// <param name="itemNames">Names of selectable items.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static int Dropdown(int selected, string[] itemNames, params GUILayoutOption[] options)
        {
            return Dropdown(null, selected, itemNames, null, options);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items">Selectable items.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(Rect rect, GUIContent label, T selected, IList<T> items)
        {
            var controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, controlID, label);
            }

            string display = null;

            if (EditorGUI.showMixedValue)
            {
                display = SirenixEditorGUI.MixedValueDashChar;
            }
            else
            {
                display = selected == null ? "Null" : selected.ToString();
            }

            if (GUI.Button(rect, display, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < items.Count; i++)
                {
                    int localI = i;
                    bool isSelected = EqualityComparer<T>.Default.Equals(selected, items[i]);

                    menu.AddItem(new GUIContent(items[i] == null ? "Null" : (items[i] + "")), isSelected, () =>
                    {
                        PopupSelector<T>.CurrentSelectingPopupControlID = controlID;
                        PopupSelector<T>.SelectFunc = () => items[localI];
                    });
                }
                menu.DropDown(rect);
            }

            if (PopupSelector<T>.CurrentSelectingPopupControlID == controlID && PopupSelector<T>.SelectFunc != null)
            {
                selected = PopupSelector<T>.SelectFunc();
                PopupSelector<T>.CurrentSelectingPopupControlID = -1;
                PopupSelector<T>.SelectFunc = null;
                GUI.changed = true;
            }

            return selected;
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items">Selectable items.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(GUIContent label, T selected, IList<T> items)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField);
            return Dropdown(rect, label, selected, items);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items">Selectable items.</param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(Rect rect, GUIContent label, T selected, T[] items, string[] itemNames, GUIStyle style)
        {
            int index = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (selected.Equals(items[i]))
                {
                    index = i;
                    break;
                }
            }

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            index = EditorGUI.Popup(rect, index, itemNames, style ?? EditorStyles.popup);
            return items[index];
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items"></param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(Rect rect, GUIContent label, T selected, T[] items, string[] itemNames)
        {
            return Dropdown<T>(rect, label, selected, items, itemNames, null);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items"></param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(Rect rect, string label, T selected, T[] items, string[] itemNames)
        {
            return Dropdown<T>(rect, label != null ? GUIHelper.TempContent(label) : null, selected, items, itemNames, null);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items"></param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(Rect rect, T selected, T[] items, string[] itemNames)
        {
            return Dropdown<T>(rect, null, selected, items, itemNames, null);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items"></param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(GUIContent label, T selected, T[] items, string[] itemNames, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return Dropdown<T>(rect, label, selected, items, itemNames, style);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items"></param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(GUIContent label, T selected, T[] items, string[] itemNames, params GUILayoutOption[] options)
        {
            return Dropdown<T>(label, selected, items, itemNames, null, options);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="items"></param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(string label, T selected, T[] items, string[] itemNames, params GUILayoutOption[] options)
        {
            return Dropdown<T>(label != null ? GUIHelper.TempContent(label) : null, selected, items, itemNames, null, options);
        }

        /// <summary>
        /// Draws a generic dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selected">Current value.</param>
        /// <param name="items"></param>
        /// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static T Dropdown<T>(T selected, T[] items, string[] itemNames, params GUILayoutOption[] options)
        {
            return Dropdown<T>(null, selected, items, itemNames, null, options);
        }

        // Enum dropdown implementation for non-flag enums.
        private static Enum EnumDropdownImplementation(Rect buttonPosition, string display, int controlID, Type type, Enum selected, GUIStyle style)
        {
            if (GUI.Button(buttonPosition, display, style))
            {
                string[] names = Enum.GetNames(type);
                Array valuesArray = Enum.GetValues(type);
                currentEnumControlID = controlID;
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < names.Length; i++)
                {
                    int localI = i;
                    menu.AddItem(new GUIContent(names[i]), selected.Equals(valuesArray.GetValue(i)), () =>
                    {
                        currentEnumControlHasValue = true;
                        selectedEnumValue = (Enum)(object)valuesArray.GetValue(localI);
                    });
                }

                menu.DropDown(buttonPosition);
            }

            if (currentEnumControlHasValue && controlID == currentEnumControlID)
            {
                currentEnumControlHasValue = false;
                if (selected != selectedEnumValue)
                {
                    GUI.changed = true;
                    selected = selectedEnumValue;
                }
                selectedEnumValue = null;
            }

            return selected;
        }

        // Enum dropdown implementation for flag enums.
        private static Enum EnumFlagDropdownImplementation(Rect buttonPosition, string display, int controlID, Type type, Enum selected, GUIStyle style)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            bool signed = underlyingType == typeof(sbyte) || underlyingType == typeof(int) || underlyingType == typeof(short) || underlyingType == typeof(long);

            selected = GetCurrentMaskValue(controlID, type, selected, signed);

            if (string.IsNullOrEmpty(display) || display == "0")
            {
                display = "None";
            }
            else if (display.Contains(","))
            {
                var size = style.CalcSize(new GUIContent(display));

                if (size.x > buttonPosition.width)
                {
                    display = "Mixed (" + (display.Count(n => n == ',') + 1) + ")...";
                }
            }

            if (GUI.Button(buttonPosition, display, style))
            {
                string[] names = Enum.GetNames(type);
                Array valuesArray = Enum.GetValues(type);
                GenericMenu menu = new GenericMenu();

                MaskMenu.CurrentEnumControlID = controlID;
                MaskMenu.EnumChanged = false;

                if (signed)
                {
                    long selectedValue = Convert.ToInt64(selected, CultureInfo.InvariantCulture);
                    var values = valuesArray.FilterCast<object>().Select(n => Convert.ToInt64(n, CultureInfo.InvariantCulture)).ToList();
                    var noneIndex = values.IndexOf(0);
                    var allIndex = values.FindIndex(n => n != 0 && values.All(m => (m & n) == n));
                    long allValue = 0L;
                    for (int i = 0; i < values.Count; i++)
                    {
                        allValue |= values[i];
                    }

                    if (values.Count >= 16)
                    {
                        if (allIndex == -1)
                        {
                            menu.AddItem(new GUIContent("All"), selectedValue == allValue, EnumMaskSetValueDelegateSigned, allValue);
                            menu.AddItem(new GUIContent("None"), selectedValue == 0, EnumMaskSetValueDelegateSigned, (long)0);
                        }

                        if (allIndex == -1 || noneIndex == -1)
                        {
                            menu.AddSeparator("");
                        }
                    }

                    for (int i = 0; i < names.Length; i++)
                    {
                        long value = values[i];
                        bool hasFlag;

                        if (value == 0)
                        {
                            hasFlag = selectedValue == 0;
                        }
                        else
                        {
                            hasFlag = (value & selectedValue) == value;
                        }

                        menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(names[i])), hasFlag, EnumMaskSetValueDelegateSigned, value);
                    }

                    if (values.Count < 16)
                    {
                        if (allIndex == -1 || noneIndex == -1)
                        {
                            menu.AddSeparator("");
                        }

                        if (allIndex == -1)
                        {
                            menu.AddItem(new GUIContent("All"), selectedValue == allValue, EnumMaskSetValueDelegateSigned, allValue);
                            menu.AddItem(new GUIContent("None"), selectedValue == 0, EnumMaskSetValueDelegateSigned, (long)0);
                        }
                    }
                }
                else
                {
                    ulong selectedValue = Convert.ToUInt64(selected, CultureInfo.InvariantCulture);
                    var values = valuesArray.FilterCast<object>().Select(n => Convert.ToUInt64(n, CultureInfo.InvariantCulture)).ToList();
                    var noneIndex = values.IndexOf(0);
                    var allIndex = values.FindIndex(n => n != 0 && values.All(m => (m & n) == n));
                    ulong allValue = 0ul;
                    for (int i = 0; i < values.Count; i++)
                    {
                        allValue |= values[i];
                    }

                    if (values.Count >= 16)
                    {
                        if (allIndex == -1)
                        {
                            menu.AddItem(new GUIContent("All"), selectedValue == allValue, EnumMaskSetValueDelegateUnsigned, allValue);
                            menu.AddItem(new GUIContent("None"), selectedValue == 0, EnumMaskSetValueDelegateUnsigned, (ulong)0);
                        }

                        if (allIndex == -1 || noneIndex == -1)
                        {
                            menu.AddSeparator("");
                        }
                    }

                    for (int i = 0; i < names.Length; i++)
                    {
                        ulong value = values[i];
                        bool hasFlag;

                        if (value == 0)
                        {
                            hasFlag = selectedValue == 0;
                        }
                        else
                        {
                            hasFlag = (value & selectedValue) == value;
                        }

                        menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(names[i])), hasFlag, EnumMaskSetValueDelegateUnsigned, value);
                    }

                    if (values.Count < 16)
                    {
                        if (allIndex == -1 || noneIndex == -1)
                        {
                            menu.AddSeparator("");
                        }

                        if (allIndex == -1)
                        {
                            menu.AddItem(new GUIContent("All"), selectedValue == allValue, EnumMaskSetValueDelegateUnsigned, allValue);
                            menu.AddItem(new GUIContent("None"), selectedValue == 0, EnumMaskSetValueDelegateUnsigned, (ulong)0);
                        }
                    }
                }

                menu.DropDown(buttonPosition);
            }

            return selected;
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(Rect rect, GUIContent label, Enum selected, GUIStyle style)
        {
            var type = selected.GetType();
            var controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);
            var display = EditorGUI.showMixedValue ? SirenixEditorGUI.MixedValueDashChar : selected.ToString();
            var buttonPosition = label == null ? rect : EditorGUI.PrefixLabel(rect, controlID, label, EditorStyles.label);
            style = style ?? EditorStyles.popup;

            if (label == null)
            {
                buttonPosition = EditorGUI.IndentedRect(buttonPosition);
            }

            if (type.IsDefined<FlagsAttribute>())
            {
                return EnumFlagDropdownImplementation(buttonPosition, display, controlID, type, selected, style);
            }
            else
            {
                return EnumDropdownImplementation(buttonPosition, display, controlID, type, selected, style);
            }
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(Rect rect, GUIContent label, Enum selected)
        {
            return EnumDropdown(rect, label, selected, null);
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(Rect rect, string label, Enum selected)
        {
            return EnumDropdown(rect, label != null ? GUIHelper.TempContent(label) : null, selected, null);
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="selected">Current value.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(Rect rect, Enum selected)
        {
            return EnumDropdown(rect, null, selected, null);
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return EnumDropdown(rect, label, selected, style);
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(GUIContent label, Enum selected, params GUILayoutOption[] options)
        {
            return EnumDropdown(label, selected, null, options);
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(string label, Enum selected, params GUILayoutOption[] options)
        {
            return EnumDropdown(label != null ? GUIHelper.TempContent(label) : null, selected, null, options);
        }

        /// <summary>
        /// Draws a dropdown for an enum or an enum mask.
        /// </summary>
        /// <param name="selected">Current value.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        public static Enum EnumDropdown(Enum selected, params GUILayoutOption[] options)
        {
            return EnumDropdown(null, selected, null, options);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="items">Avaible items in the dropdown.</param>
        /// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
        /// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
        public static bool Dropdown<T>(Rect rect, GUIContent label, IList<int> selected, IList<T> items, bool multiSelection)
        {
            var controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, controlID, label);
            }

            string display = null;

            if (EditorGUI.showMixedValue)
            {
                display = SirenixEditorGUI.MixedValueDashChar;
            }
            else
            {
                for (int i = 0; i < selected.Count; i++)
                {
                    var item = items[selected[i]];
                    string name = item == null ? "Null" : item.ToString();
                    if (display == null)
                    {
                        display = name;
                    }
                    else
                    {
                        display = name + ", " + display;
                    }
                }
            }
            display = display ?? "None";

            if (GUI.Button(rect, display, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < items.Count; i++)
                {
                    int localI = i;
                    bool isSelected = selected.Contains(i);
                    string numSelected = "";
                    if (isSelected)
                    {
                        int selectedCount = selected.Count(x => x == i);
                        if (selectedCount > 1)
                        {
                            numSelected = " (" + selectedCount + ")";
                        }
                    }
                    menu.AddItem(new GUIContent(items[i] + numSelected), isSelected, () =>
                    {
                        PopupSelector.CurrentSelectingPopupControlID = controlID;
                        PopupSelector.SelectAction = () =>
                        {
                            if (multiSelection)
                            {
                                if (isSelected)
                                {
                                    for (int j = selected.Count - 1; j >= 0; j--)
                                    {
                                        if (selected[j] == localI)
                                        {
                                            selected.RemoveAt(j);
                                        }
                                    }
                                }
                                else
                                {
                                    selected.Add(localI);
                                }
                            }
                            else
                            {
                                selected.Clear();
                                selected.Add(localI);
                            }
                        };
                    });
                }
                menu.DropDown(rect);
            }

            if (PopupSelector.CurrentSelectingPopupControlID == controlID && PopupSelector.SelectAction != null)
            {
                PopupSelector.SelectAction();
                PopupSelector.CurrentSelectingPopupControlID = -1;
                PopupSelector.SelectAction = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="items">Avaible items in the dropdown.</param>
        /// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
        /// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
        public static bool Dropdown<T>(Rect rect, string label, IList<int> selected, IList<T> items, bool multiSelection)
        {
            return Dropdown<T>(rect, label != null ? GUIHelper.TempContent(label) : null, selected, items, multiSelection);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="items">Avaible items in the dropdown.</param>
        /// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
        /// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
        public static bool Dropdown<T>(Rect rect, IList<int> selected, IList<T> items, bool multiSelection)
        {
            return Dropdown<T>(rect, (GUIContent)null, selected, items, multiSelection);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="items">Avaible items in the dropdown.</param>
        /// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
        /// <param name="options">Layout options.</param>
        /// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
        public static bool Dropdown<T>(GUIContent label, IList<int> selected, IList<T> items, bool multiSelection, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.popup, options);
            return Dropdown<T>(rect, label, selected, items, multiSelection);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="items">Avaible items in the dropdown.</param>
        /// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
        /// <param name="options">Layout options.</param>
        /// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
        public static bool Dropdown<T>(string label, IList<int> selected, IList<T> items, bool multiSelection, params GUILayoutOption[] options)
        {
            return Dropdown<T>(label != null ? GUIHelper.TempContent(label) : null, selected, items, multiSelection, options);
        }

        /// <summary>
        /// Draws a dropdown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selected">Current selection.</param>
        /// <param name="items">Avaible items in the dropdown.</param>
        /// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
        /// <param name="options">Layout options.</param>
        /// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
        public static bool Dropdown<T>(IList<int> selected, IList<T> items, bool multiSelection, params GUILayoutOption[] options)
        {
            return Dropdown<T>((GUIContent)null, selected, items, multiSelection, options);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(Rect rect, GUIContent label, Enum selected, GUIStyle style)
        {
            return EnumDropdown(rect, label, selected, style);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(Rect rect, GUIContent label, Enum selected)
        {
            return EnumMaskDropdown(rect, label, selected, null);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(Rect rect, string label, Enum selected)
        {
            return EnumMaskDropdown(rect, label != null ? GUIHelper.TempContent(label) : null, selected, null);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="rect">Position and size of the field.</param>
        /// <param name="selected">Current selection.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(Rect rect, Enum selected)
        {
            return EnumMaskDropdown(rect, null, selected, null);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
            return EnumMaskDropdown(rect, label, selected, style);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(GUIContent label, Enum selected, params GUILayoutOption[] options)
        {
            return EnumMaskDropdown(label, selected, null, options);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
        /// <param name="selected">Current selection.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(string label, Enum selected, params GUILayoutOption[] options)
        {
            return EnumMaskDropdown(label != null ? GUIHelper.TempContent(label) : null, selected, null, options);
        }

        /// <summary>
        /// Draws a dropdown field for enum masks.
        /// </summary>
        /// <param name="selected">Current selection.</param>
        /// <param name="options">Layout options.</param>
        /// <returns>Value assigned to the field.</returns>
        [Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead")]
        public static Enum EnumMaskDropdown(Enum selected, params GUILayoutOption[] options)
        {
            return EnumMaskDropdown(null, selected, null, options);
        }

        private static bool OnLocalControlRelease(Rect rect, int controlID)
        {
            if (localHotControl != 0 && localHotControl == controlID && (
                (Event.current.rawType == EventType.MouseUp) ||
                (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) ||
                (Event.current.rawType == EventType.MouseDown && Event.current.button == 1) ||
                (Event.current.rawType == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))))
            {
                localHotControl = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void DrawSlideKnob(Rect rect, int id)
        {
            var e = Event.current.type;
            if (e == EventType.Layout)
            {
                return;
            }

            var show = GUIUtility.hotControl == id || rect.Contains(Event.current.mousePosition);

            if (show && Event.current.type == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint();
            }

            if (Event.current.type == EventType.Repaint && show)
            {
                var slideKnob = rect.AlignRight(slideKnobWidth - 2);
                slideKnob.y = slideKnob.center.y;
                slideKnob.y -= 9;
                slideKnob.x -= 2;
                slideKnob.height = 18;
                var s = slideKnobWidth / 2;
                GUIHelper.PushColor(GUIUtility.hotControl == id || slideKnob.Contains(Event.current.mousePosition) ? Color.white : new Color(1f, 1f, 1f, 0.35f));
                GUI.DrawTexture(slideKnob.AlignLeft(s), EditorIcons.TriangleLeft.Active);
                GUI.DrawTexture(slideKnob.AlignRight(s), EditorIcons.TriangleRight.Active);
                //slideKnobStyle.Draw(rect.AddX(-3), GUIContent.none, id);
                GUIHelper.PopColor();
            }
        }

        private static void EnumMaskSetValueDelegateSigned(object value)
        {
            MaskMenu.EnumChanged = true;
            MaskMenu.ChangedMaskValueSigned = (long)value;
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent(MaskMenu.MASK_MENU_CHANGED_EVENT_NAME));
        }

        private static void EnumMaskSetValueDelegateUnsigned(object value)
        {
            MaskMenu.EnumChanged = true;
            MaskMenu.ChangedMaskValueUnsigned = (ulong)value;
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent(MaskMenu.MASK_MENU_CHANGED_EVENT_NAME));
        }

        private static Enum GetCurrentMaskValue(int controlId, Type enumType, Enum selected, bool signed)
        {
            var current = Event.current;

            if (current.type == EventType.ExecuteCommand && current.commandName == MaskMenu.MASK_MENU_CHANGED_EVENT_NAME && controlId == MaskMenu.CurrentEnumControlID && MaskMenu.EnumChanged)
            {
                if (signed)
                {
                    long value = Convert.ToInt64(selected, CultureInfo.InvariantCulture);

                    if (MaskMenu.ChangedMaskValueSigned == 0)
                    {
                        value = 0;
                    }
                    else if ((MaskMenu.ChangedMaskValueSigned & value) == MaskMenu.ChangedMaskValueSigned)
                    {
                        // Remove flag
                        value = value & ~MaskMenu.ChangedMaskValueSigned;
                    }
                    else
                    {
                        // Add flag
                        value |= MaskMenu.ChangedMaskValueSigned;
                    }

                    selected = (Enum)Enum.ToObject(enumType, value);
                }
                else
                {
                    ulong value = Convert.ToUInt64(selected, CultureInfo.InvariantCulture);

                    if (MaskMenu.ChangedMaskValueUnsigned == 0)
                    {
                        value = 0;
                    }
                    else if ((MaskMenu.ChangedMaskValueUnsigned & value) == MaskMenu.ChangedMaskValueUnsigned)
                    {
                        // Remove flag
                        value = value & ~MaskMenu.ChangedMaskValueUnsigned;
                    }
                    else
                    {
                        // Add flag
                        value |= MaskMenu.ChangedMaskValueUnsigned;
                    }

                    selected = (Enum)Enum.ToObject(enumType, value);
                }

                GUI.changed = true;
                current.Use();
            }

            return selected;
        }

        private enum MinMaxSliderLocalControl { Min = 1, Max = 2, Bar = 3 };

        private class QuaternionContextBuffer
        {
            public bool IsUsed = false;

            public QuaternionDrawMode DrawMode = (QuaternionDrawMode)(-1);
            private Vector4 buffer;

            public Vector3 Eulers
            {
                get { return (Vector3)this.buffer; }
                set { this.buffer = (Vector4)value; }
            }

            public Vector3 Axis
            {
                get { return (Vector3)this.buffer; }
                set { this.buffer.Set(value.x, value.y, value.z, this.buffer.w); }
            }

            public float Angle
            {
                get { return this.buffer.w; }
                set { this.buffer.w = value; }
            }

            public Vector4 Raw
            {
                get { return this.buffer; }
                set { this.buffer = value; }
            }

            public void Set(Quaternion quaternion, QuaternionDrawMode mode)
            {
                this.DrawMode = mode;

                switch (mode)
                {
                    case QuaternionDrawMode.Eulers:
                        this.buffer = quaternion.eulerAngles;
                        //Vector3 euler = quaternion.eulerAngles;
                        //this.buffer = new Vector4((float)Math.Round(euler.x, 3), (float)Math.Round(euler.y, 3), (float)Math.Round(euler.z, 3));
                        break;

                    case QuaternionDrawMode.AngleAxis:
                        float angle;
                        Vector3 axis;
                        quaternion.ToAngleAxis(out angle, out axis);
                        this.buffer = new Vector4(axis.x, axis.y, axis.z, angle);
                        //this.buffer = new Vector4((float)Math.Round(axis.x, 3), (float)Math.Round(axis.y, 3), (float)Math.Round(axis.z, 3), (float)Math.Round(angle, 3));
                        break;

                    case QuaternionDrawMode.Raw:
                        this.buffer = new Vector4(
                            quaternion.x,
                            quaternion.y,
                            quaternion.z,
                            quaternion.w);
                        break;
                }
            }
        }

        private static class PopupSelector
        {
            public static int CurrentSelectingPopupControlID;
            public static Action SelectAction;
        }

        private static class PopupSelector<T>
        {
            public static int CurrentSelectingPopupControlID;
            public static Func<T> SelectFunc;
        }

        private static class MaskMenu
        {
            public const string MASK_MENU_CHANGED_EVENT_NAME = "SirenixMaskMenuChanged";

            public static long ChangedMaskValueSigned { get; set; }

            public static ulong ChangedMaskValueUnsigned { get; set; }

            public static int CurrentEnumControlID { get; set; }

            public static bool EnumChanged { get; set; }
        }
    }
}
#endif