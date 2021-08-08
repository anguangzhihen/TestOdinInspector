#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SerializedPropertyUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public static class SerializedPropertyUtilities
    {
        private static Dictionary<Type, Delegate> PrimitiveValueGetters = new Dictionary<Type, Delegate>()
        {
            { typeof(int),              (Func<SerializedProperty, int>)             (p => p.intValue) },
            { typeof(bool),             (Func<SerializedProperty, bool>)            (p => p.boolValue) },
            { typeof(float),            (Func<SerializedProperty, float>)           (p => p.floatValue) },
            { typeof(string),           (Func<SerializedProperty, string>)          (p => p.stringValue) },
            { typeof(Color),            (Func<SerializedProperty, Color>)           (p => p.colorValue) },
            { typeof(LayerMask),        (Func<SerializedProperty, LayerMask>)       (p => p.intValue) },
            { typeof(Vector2),          (Func<SerializedProperty, Vector2>)         (p => p.vector2Value) },
            { typeof(Vector3),          (Func<SerializedProperty, Vector3>)         (p => p.vector3Value) },
            { typeof(Vector4),          (Func<SerializedProperty, Vector4>)         (p => p.vector4Value) },
            { typeof(Rect),             (Func<SerializedProperty, Rect>)            (p => p.rectValue) },
            { typeof(char),             (Func<SerializedProperty, char>)            (p => (char)p.intValue) },
            { typeof(AnimationCurve),   (Func<SerializedProperty, AnimationCurve>)  (p => p.animationCurveValue) },
            { typeof(Bounds),           (Func<SerializedProperty, Bounds>)          (p => p.boundsValue) },
            { typeof(Quaternion),       (Func<SerializedProperty, Quaternion>)      (p => p.quaternionValue) },
        };

        private static Dictionary<Type, Delegate> PrimitiveValueSetters = new Dictionary<Type, Delegate>()
        {
            { typeof(int),              (Action<SerializedProperty, int>)             ((p, v) => p.intValue = v) },
            { typeof(bool),             (Action<SerializedProperty, bool>)            ((p, v) => p.boolValue = v) },
            { typeof(float),            (Action<SerializedProperty, float>)           ((p, v) => p.floatValue = v) },
            { typeof(string),           (Action<SerializedProperty, string>)          ((p, v) => p.stringValue = v) },
            { typeof(Color),            (Action<SerializedProperty, Color>)           ((p, v) => p.colorValue = v) },
            { typeof(LayerMask),        (Action<SerializedProperty, LayerMask>)       ((p, v) => p.intValue = v) },
            { typeof(Vector2),          (Action<SerializedProperty, Vector2>)         ((p, v) => p.vector2Value = v) },
            { typeof(Vector3),          (Action<SerializedProperty, Vector3>)         ((p, v) => p.vector3Value = v) },
            { typeof(Vector4),          (Action<SerializedProperty, Vector4>)         ((p, v) => p.vector4Value = v) },
            { typeof(Rect),             (Action<SerializedProperty, Rect>)            ((p, v) => p.rectValue = v) },
            { typeof(char),             (Action<SerializedProperty, char>)            ((p, v) => p.intValue = v) },
            { typeof(AnimationCurve),   (Action<SerializedProperty, AnimationCurve>)  ((p, v) => p.animationCurveValue = v) },
            { typeof(Bounds),           (Action<SerializedProperty, Bounds>)          ((p, v) => p.boundsValue = v) },
            { typeof(Quaternion),       (Action<SerializedProperty, Quaternion>)      ((p, v) => p.quaternionValue = v) },
        };

        private static Dictionary<string, Type> UnityTypes;

        private static Type GetUnityTypeWithName(string name)
        {
            if (UnityTypes == null)
            {
                UnityTypes = new Dictionary<string, Type>();

                foreach (var type in AssemblyUtilities.GetTypes(AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes)
                                                      .Where(n => typeof(UnityEngine.Object).IsAssignableFrom(n)))
                {
                    if (UnityTypes.ContainsKey(type.Name))
                    {
                        UnityTypes[type.Name] = null;
                    }
                    else
                    {
                        UnityTypes[type.Name] = type;
                    }
                }
            }

            Type result;
            UnityTypes.TryGetValue(name, out result);
            return result;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static string GetProperTypeName(this SerializedProperty property)
        {
            if (property.type.StartsWith("PPtr<"))
            {
                return property.type.Substring(5).Trim('<', '>', '$');
            }

            return property.type;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool IsCompatibleWithType(this SerializedProperty property, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return property.type == type.Name;

                case SerializedPropertyType.Integer:
                    return type == typeof(int);

                case SerializedPropertyType.Boolean:
                    return type == typeof(bool);

                case SerializedPropertyType.Float:
                    return type == typeof(float);

                case SerializedPropertyType.String:
                    return type == typeof(string);

                case SerializedPropertyType.Color:
                    return type == typeof(Color);

                case SerializedPropertyType.ObjectReference:
                    {
                        if (!object.ReferenceEquals(property.objectReferenceValue, null))
                        {
                            return property.objectReferenceValue.GetType().IsAssignableFrom(type);
                        }

                        string typeName = property.GetProperTypeName();

                        if (typeName == "Prefab")
                        {
                            return type == typeof(GameObject);
                        }

                        var possibleType = GetUnityTypeWithName(typeName);

                        if (possibleType != null)
                        {
                            return possibleType.IsAssignableFrom(type);
                        }

                        return false;
                    }
                case SerializedPropertyType.LayerMask:
                    return type == typeof(LayerMask);

                case SerializedPropertyType.Enum:
                    {
                        if (!type.IsEnum) return false;

                        var enumNames = Enum.GetNames(type);
                        var propNames = property.enumNames;

                        if (enumNames.Length != propNames.Length) return false;

                        for (int i = 0; i < enumNames.Length; i++)
                        {
                            if (!string.Equals(enumNames[i], propNames[i].Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)) return false;
                        }

                        return true;
                    }

                case SerializedPropertyType.Vector2:
                    return type == typeof(Vector2);

                case SerializedPropertyType.Vector3:
                    return type == typeof(Vector3);

                case SerializedPropertyType.Vector4:
                    return type == typeof(Vector4);

                case SerializedPropertyType.Rect:
                    return type == typeof(Rect);

                case SerializedPropertyType.ArraySize:
                    return false;

                case SerializedPropertyType.Character:
                    return type == typeof(char);

                case SerializedPropertyType.AnimationCurve:
                    return type == typeof(AnimationCurve);

                case SerializedPropertyType.Bounds:
                    return type == typeof(Bounds);

                case SerializedPropertyType.Gradient:
                    return type == typeof(Gradient);

                case SerializedPropertyType.Quaternion:
                    return type == typeof(Quaternion);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Type GuessContainedType(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return null;

                case SerializedPropertyType.Integer:
                    return typeof(int);

                case SerializedPropertyType.Boolean:
                    return typeof(bool);

                case SerializedPropertyType.Float:
                    return typeof(float);

                case SerializedPropertyType.String:
                    return typeof(string);

                case SerializedPropertyType.Color:
                    return typeof(Color);

                case SerializedPropertyType.ObjectReference:
                    {
                        if (!object.ReferenceEquals(property.objectReferenceValue, null))
                        {
                            return property.objectReferenceValue.GetType();
                        }

                        string typeName = property.GetProperTypeName();

                        var possibles = AssemblyUtilities.GetTypes(AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes)
                                                         .Where(n => n.Name == typeName && typeof(UnityEngine.Object).IsAssignableFrom(n))
                                                         .ToList();

                        if (possibles.Count == 1)
                        {
                            return possibles[0];
                        }

                        return null;
                    }
                case SerializedPropertyType.LayerMask:
                    return typeof(LayerMask);

                case SerializedPropertyType.Enum:
                    return null;

                case SerializedPropertyType.Vector2:
                    return typeof(Vector2);

                case SerializedPropertyType.Vector3:
                    return typeof(Vector3);

                case SerializedPropertyType.Vector4:
                    return typeof(Vector4);

                case SerializedPropertyType.Rect:
                    return typeof(Rect);

                case SerializedPropertyType.ArraySize:
                    return null;

                case SerializedPropertyType.Character:
                    return typeof(char);

                case SerializedPropertyType.AnimationCurve:
                    return typeof(AnimationCurve);

                case SerializedPropertyType.Bounds:
                    return typeof(Bounds);

                case SerializedPropertyType.Gradient:
                    return typeof(Gradient);

                case SerializedPropertyType.Quaternion:
                    return typeof(Quaternion);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool CanSetGetValue(Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return true;
            }

            return PrimitiveValueGetters.ContainsKey(type);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Func<SerializedProperty, T> GetValueGetter<T>()
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                return p => (T)(object)p.objectReferenceValue;
            }

            Delegate result;
            PrimitiveValueGetters.TryGetValue(typeof(T), out result);
            return (Func<SerializedProperty, T>)result;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Action<SerializedProperty, T> GetValueSetter<T>()
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                return (p, v) => p.objectReferenceValue = (UnityEngine.Object)(object)v;
            }

            Delegate result;
            PrimitiveValueSetters.TryGetValue(typeof(T), out result);
            return (Action<SerializedProperty, T>)result;
        }
    }
}
#endif