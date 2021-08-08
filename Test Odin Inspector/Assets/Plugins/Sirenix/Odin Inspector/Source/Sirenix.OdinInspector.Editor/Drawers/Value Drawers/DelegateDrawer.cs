#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DelegateDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Delegate property drawer. This drawer is rather simplistic for now, and will receive significant upgrades in the future.
    /// </summary>
    [DrawerPriority(0.51, 0, 0)] // Just above the regular valueconflict/null value drawers, as we handle that here
    public class DelegateDrawer<T> : OdinValueDrawer<T> where T : class
    {
        private static MethodInfo invokeMethodField;
        private static bool gotInvokeMethod;

        private UnityEngine.Object contextObj;

        private static MethodInfo InvokeMethod
        {
            get
            {
                if (!gotInvokeMethod)
                {
                    invokeMethodField = typeof(T).GetMethod("Invoke");
                    gotInvokeMethod = true;
                }

                return invokeMethodField;
            }
        }

        /// <summary>
        /// See <see cref="OdinDrawer.CanDrawTypeFilter(Type)"/>.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return !type.IsAbstract && typeof(Delegate).IsAssignableFrom(type) && InvokeMethod != null;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            Delegate del = (Delegate)(object)entry.SmartValue;
            GUIContent content = GUIHelper.TempContent((string)null);
            bool conflict = false;
            bool targetConflict = false;

            bool isNull = false;
            bool anyMethodsNull = false;
            bool anyMethodsNotNull = false;

            for (int i = 0; i < entry.ValueCount; i++)
            {
                Delegate del2 = (Delegate)(object)entry.Values[i];

                if (del2 != null && del2.Method == null)
                {
                    anyMethodsNull = true;
                }
                else
                {
                    anyMethodsNotNull = true;
                }
            }

            if (entry.ValueState == PropertyValueState.NullReference || (anyMethodsNull && !anyMethodsNotNull))
            {
                conflict = true;
                isNull = true;
                content.text = "Null";
            }
            else if (entry.ValueState == PropertyValueState.ReferenceValueConflict || anyMethodsNull)
            {
                conflict = true;
                content.text = "Multiselection Value Conflict";
            }
            else
            {
                MethodInfo method = del.Method;
                object target = del.Target;

                for (int i = 1; i < entry.ValueCount; i++)
                {
                    var otherDel = (Delegate)(object)entry.Values[i];

                    if (otherDel.Method != method)
                    {
                        conflict = true;
                    }

                    if (otherDel.Target != target)
                    {
                        targetConflict = true;
                    }
                }

                if (conflict)
                {
                    content.text = "Multiselection Method Conflict";
                }
                else
                {
                    content.text = method.GetFullName();

                    if (method.IsStatic)
                    {
                        content.text = "static " + content.text;
                    }
                }
            }

            if (isNull)
            {
                content.text = typeof(T).GetNiceName();
            }

            var rect = EditorGUILayout.GetControlRect(label != null);
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            else
            {
                rect = EditorGUI.IndentedRect(rect);
            }

            var obj = del == null ? null : del.Target as UnityEngine.Object;
            if (obj == null)
            {
                obj = this.contextObj;
            }

            rect.width *= 0.5f;
            if (GUI.Button(rect, content, EditorStyles.popup))
            {
                Popup(entry, rect, obj);
            }

            bool previousMixedValue = EditorGUI.showMixedValue;
            {
                if (targetConflict)
                {
                    EditorGUI.showMixedValue = true;
                }
            }

            UnityEngine.Object newTarget;

            rect.x += rect.width;
            EditorGUI.BeginChangeCheck();
            //GUIHelper.PushColor(new Color(1, 1, 1, 0), true);
            newTarget = EditorGUI.ObjectField(rect, obj, typeof(UnityEngine.Object), true);
            //GUIHelper.PopColor();
            bool changed = EditorGUI.EndChangeCheck();

            if (del != null && Event.current.type == EventType.Repaint && obj != null)
            {
                MethodInfo method = del.Method;

                string labelName;
                if (targetConflict)
                {
                    labelName = "Target conflict";
                }
                else if (obj is Component)
                {
                    labelName = (obj as Component).gameObject.name;
                }
                else
                {
                    labelName = obj.GetType().GetNiceName();
                }

                GUIContent text = new GUIContent(labelName, AssetPreview.GetMiniThumbnail(obj));
                GUI.Label(rect, text, EditorStyles.objectField);
            }
            else if (obj == null)
            {
                GUIContent text = new GUIContent("None", AssetPreview.GetMiniThumbnail(obj));
                GUI.Label(rect, text, EditorStyles.objectField);
            }

            if (newTarget != obj && changed)
            {
                for (int i = 0; i < entry.ValueCount; i++)
                {
                    entry.Values[i] = null;
                }

                this.contextObj = newTarget;
            }

            if (targetConflict)
            {
                EditorGUI.showMixedValue = previousMixedValue;
            }
        }

        private void Popup(IPropertyValueEntry<T> entry, Rect rect, UnityEngine.Object target)
        {
            Type returnType = InvokeMethod.ReturnType;
            Type[] parameters = InvokeMethod.GetParameters().Select(n => n.ParameterType).ToArray();
            GenericMenu menu = new GenericMenu();

            if (target == null)
            {
                menu.AddDisabledItem(new GUIContent("No target selected"));
            }
            else
            {
                var targetGameObject = target as GameObject;
                var targetComponent = target as Component;

                if (targetGameObject == null && targetComponent != null)
                {
                    targetGameObject = targetComponent.gameObject;
                }

                if (targetGameObject != null)
                {
                    RegisterGameObject(menu, entry, "", targetGameObject, returnType, parameters);
                }
                else
                {
                    RegisterUnityObject(menu, entry, "", target, returnType, parameters);
                }

                if (menu.GetItemCount() == 0)
                {
                    menu.AddDisabledItem(new GUIContent("No suitable method found on target"));
                }
            }

            menu.DropDown(rect);
        }

        private void RegisterGameObject(GenericMenu menu, IPropertyValueEntry<T> entry, string path, GameObject go, Type returnType, Type[] parameters)
        {
            RegisterUnityObject(menu, entry, path + "/GameObject", go, returnType, parameters);

            foreach (var component in go.GetComponents<Component>())
            {
                RegisterUnityObject(menu, entry, path + "/" + component.GetType().GetNiceName(), component, returnType, parameters);
            }
        }

        private void RegisterUnityObject(GenericMenu menu, IPropertyValueEntry<T> entry, string path, UnityEngine.Object obj, Type returnType, Type[] parameters)
        {
            MethodInfo[] methods = obj.GetType()
                .GetAllMembers<MethodInfo>(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(n =>
                {
                    if (n.ReturnType != returnType)
                    {
                        return false;
                    }

                    var methodParams = n.GetParameters();

                    if (methodParams.Length != parameters.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < methodParams.Length; i++)
                    {
                        if (methodParams[i].ParameterType != parameters[i])
                        {
                            return false;
                        }
                    }

                    return true;
                })
                .ToArray();

            foreach (var method in methods)
            {
                string name = method.GetFullName();
                MethodInfo closureMethod = method; // For lambda capture

                if (method.DeclaringType != obj.GetType())
                {
                    name = method.DeclaringType.GetNiceName() + "/" + name;
                }

                if (method.IsStatic)
                {
                    name += " (static)";
                }

                GenericMenu.MenuFunction func = () =>
                {
                    entry.Property.Tree.DelayActionUntilRepaint(() =>
                    {
                        Delegate del;

                        if (closureMethod.IsStatic)
                        {
                            del = Delegate.CreateDelegate(typeof(T), null, closureMethod);
                        }
                        else
                        {
                            del = Delegate.CreateDelegate(typeof(T), obj, closureMethod);
                        }

                        for (int i = 0; i < entry.ValueCount; i++)
                        {
                            entry.Values[i] = (T)(object)del;
                        }

                        // Apply changes is called immediately after this is invoked in repaint, during EndDrawPropertyTree
                        //entry.ApplyChanges();
                        this.contextObj = null;
                    });
                };

                menu.AddItem(new GUIContent((path + "/" + name).TrimStart('/')), false, func);
            }
        }
    }
}
#endif