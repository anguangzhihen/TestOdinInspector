#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityPropertyHandlerUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using System;
    using System.Collections.Generic;

    internal static class UnityPropertyHandlerUtility
    {
        private const string ScriptAttributeUtilityName = "UnityEditor.ScriptAttributeUtility";
        private const string PropertyHandlerCacheName = "UnityEditor.PropertyHandlerCache";
        private const string PropertyHandlerName = "UnityEditor.PropertyHandler";

        private const string ScriptAttributeUtility_GetHandlerName = "GetHandler";
        private const string ScriptAttributeUtility_PropertyHandlerCacheName = "propertyHandlerCache";
        private const string PropertyHandlerCache_SetHandlerName = "SetHandler";
        private const string PropertyHandler_OnGUIName = "OnGUI";
        private const string PropertyHandler_GetHeightName = "GetHeight";
        private const string PropertyHandler_PropertyDrawerName = "m_PropertyDrawer";
        private const string PropertyHandler_AddMenuItemsName = "AddMenuItems";

        private const string PropertyHandler_PropertyDrawersName_2021_1 = "m_PropertyDrawers";
        //private const string PropertyHandler_DecoratorDrawersName_2021_1 = "m_PropertyDrawers";

        // Different name so as not to potentially collide with other delegates
        private delegate void FiveArgAction<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        private static readonly Func<SerializedProperty, object> ScriptAttributeUtility_GetHandler_Func;
        private static readonly Func<object> ScriptAttributeUtility_GetPropertyHandlerCache;
        private static readonly Action<object, SerializedProperty, object> PropertyHandlerCache_SetHandler;
        private static readonly Func<object> PropertyHandler_Create;
        private static readonly FiveArgAction<object, Rect, SerializedProperty, GUIContent, bool> PropertyHandler_OnGUI;
        private static readonly Func<object, SerializedProperty, GUIContent, bool, float> PropertyHandler_GetHeight;
        private static readonly Action<object, PropertyDrawer> PropertyHandler_SetPropertyDrawer;
        private static readonly Action<object, SerializedProperty, GenericMenu> PropertyHandler_AddMenuItems_Func;

        private static readonly Type ScriptAttributeUtility = typeof(Editor).Assembly.GetType(ScriptAttributeUtilityName);
        private static readonly Type PropertyHandlerCache = typeof(Editor).Assembly.GetType(PropertyHandlerCacheName);
        private static readonly Type PropertyHandler = typeof(Editor).Assembly.GetType(PropertyHandlerName);

        static UnityPropertyHandlerUtility()
        {
            try
            {
                if (ScriptAttributeUtility == null)
                {
                    CouldNotFindTypeError(ScriptAttributeUtilityName);
                    return;
                }

                if (PropertyHandlerCache == null)
                {
                    CouldNotFindTypeError(PropertyHandlerCacheName);
                    return;
                }

                if (PropertyHandler == null)
                {
                    CouldNotFindTypeError(PropertyHandlerName);
                    return;
                }

                var propertyHandlerCacheProperty = ScriptAttributeUtility.GetProperty(ScriptAttributeUtility_PropertyHandlerCacheName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var getHandler = ScriptAttributeUtility.GetMethod(ScriptAttributeUtility_GetHandlerName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(SerializedProperty) }, null);
                var setHandlerMethod = PropertyHandlerCache.GetMethod(PropertyHandlerCache_SetHandlerName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var onGUIMethod = PropertyHandler.GetMethod(PropertyHandler_OnGUIName, BindingFlags.Public | BindingFlags.Instance, null, new Type[4] { typeof(Rect), typeof(SerializedProperty), typeof(GUIContent), typeof(bool) }, null);
                var getHeightMethod = PropertyHandler.GetMethod(PropertyHandler_GetHeightName, BindingFlags.Public | BindingFlags.Instance, null, new Type[3] { typeof(SerializedProperty), typeof(GUIContent), typeof(bool) }, null);
                var addMenuItems = PropertyHandler.GetMethod(PropertyHandler_AddMenuItemsName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(SerializedProperty), typeof(GenericMenu) }, null);

                FieldInfo drawerField = PropertyHandler.GetField(PropertyHandler_PropertyDrawerName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo drawersField = null;
                //FieldInfo decoratorsField = null;

                if (drawerField == null)
                {
                    drawersField = PropertyHandler.GetField(PropertyHandler_PropertyDrawersName_2021_1, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    //decoratorsField = PropertyHandler.GetField(PropertyHandler_DecoratorDrawersName_2021_1, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }

                if (propertyHandlerCacheProperty == null)
                {
                    CouldNotFindMemberError(ScriptAttributeUtility, ScriptAttributeUtility_PropertyHandlerCacheName);
                    return;
                }

                if (getHandler == null)
                {
                    CouldNotFindMemberError(ScriptAttributeUtility, ScriptAttributeUtility_GetHandlerName);
                    return;
                }

                if (setHandlerMethod == null)
                {
                    CouldNotFindMemberError(PropertyHandlerCache, PropertyHandlerCache_SetHandlerName);
                    return;
                }

                if (onGUIMethod == null)
                {
                    CouldNotFindMemberError(PropertyHandler, PropertyHandler_OnGUIName);
                    return;
                }

                if (getHeightMethod == null)
                {
                    CouldNotFindMemberError(PropertyHandler, PropertyHandler_GetHeightName);
                    return;
                }

                if (drawerField == null)
                {
                    if (UnityVersion.IsVersionOrGreater(2021, 1))
                    {
                        if (drawersField == null)
                        {
                            CouldNotFindMemberError(PropertyHandler, PropertyHandler_PropertyDrawersName_2021_1);
                            return;
                        }

                        //if (decoratorsField == null)
                        //{
                        //    CouldNotFindMemberError(PropertyHandler, PropertyHandler_DecoratorDrawersName_2021_1);
                        //    return;
                        //}
                    }
                    else
                    {
                        CouldNotFindMemberError(PropertyHandler, PropertyHandler_PropertyDrawerName);
                        return;
                    }
                }

                if (addMenuItems == null)
                {
                    CouldNotFindMemberError(PropertyHandler, PropertyHandler_AddMenuItemsName);
                    return;
                }

                ScriptAttributeUtility_GetPropertyHandlerCache = () => propertyHandlerCacheProperty.GetValue(null, null);
                ScriptAttributeUtility_GetHandler_Func = (property) => getHandler.Invoke(null, new object[] { property });
                PropertyHandlerCache_SetHandler = (instance, property, handler) => setHandlerMethod.Invoke(instance, new object[] { property, handler });
                PropertyHandler_Create = () => Activator.CreateInstance(PropertyHandler);
                PropertyHandler_OnGUI = (instance, rect, property, label, includeChildren) => onGUIMethod.Invoke(instance, new object[] { rect, property, label, includeChildren });
                PropertyHandler_GetHeight = (instance, property, label, includeChildren) => (float)getHeightMethod.Invoke(instance, new object[] { property, label, includeChildren });
                PropertyHandler_AddMenuItems_Func = (handler, property, menu) => addMenuItems.Invoke(handler, new object[] { property, menu });

                if (drawerField == null)
                {
                    PropertyHandler_SetPropertyDrawer = (instance, drawer) =>
                    {
                        var drawers = (List<PropertyDrawer>)drawersField.GetValue(instance);

                        if (drawers == null)
                        {
                            drawers = new List<PropertyDrawer>();
                            drawersField.SetValue(instance, drawers);
                        }

                        drawers.Add(drawer);
                    };
                }
                else
                {
                    PropertyHandler_SetPropertyDrawer = (instance, drawer) => drawerField.SetValue(instance, drawer);
                }


                IsAvailable = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(new Exception("UnityPropertyHandlerUtility initialization failed with an exception; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.", ex));
            }
        }

        private static void CouldNotFindTypeError(string typeName)
        {
            Debug.LogError("Could not find the internal Unity type '" + typeName + "'; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.");
        }

        private static void CouldNotFindMemberError(Type type, string memberName)
        {
            Debug.LogError("Could not find the member '" + memberName + "' on internal Unity type '" + type.GetNiceFullName() + "'; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.");
        }

        public static bool IsAvailable { get; private set; }

        public static void PropertyHandlerOnGUI(object handler, Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (!IsAvailable)
            {
                return;
            }

            try
            {
                var cache = ScriptAttributeUtility_GetPropertyHandlerCache();
                PropertyHandlerCache_SetHandler(cache, property, handler);
                PropertyHandler_OnGUI(handler, rect, property, label, includeChildren);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.IsExitGUIException())
                {
                    throw ex.AsExitGUIException();
                }

                throw ex;
            }
        }

        public static float PropertyHandlerGetHeight(object handler, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (!IsAvailable)
            {
                return 0f;
            }

            try
            {
                var cache = ScriptAttributeUtility_GetPropertyHandlerCache();
                PropertyHandlerCache_SetHandler(cache, property, handler);
                return PropertyHandler_GetHeight(handler, property, label, includeChildren);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.IsExitGUIException())
                {
                    throw ex.AsExitGUIException();
                }

                throw ex;
            }
        }

        public static object ScriptAttributeUtility_GetHandler(SerializedProperty property)
        {
            if (!IsAvailable)
            {
                return null;
            }

            return ScriptAttributeUtility_GetHandler_Func(property);
        }

        public static void PropertyHandler_AddMenuItems(object handler, SerializedProperty property, GenericMenu menu)
        {
            if (!IsAvailable)
            {
                return;
            }

            PropertyHandler_AddMenuItems_Func(handler, property, menu);
        }

        public static object CreatePropertyHandler(PropertyDrawer drawer)
        {
            if (!IsAvailable)
            {
                return null;
            }

            object handler = PropertyHandler_Create();
            PropertyHandler_SetPropertyDrawer(handler, drawer);
            return handler;
        }
    }
}
#endif