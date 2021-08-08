#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityEditorEventUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor // This used to be placed in the Sirenix.OdinInspector.Editor assembly, but had to be moved to the utilities.
{
#pragma warning disable

    using UnityEditor;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using Sirenix.Utilities;
    using System.Linq;

    /// <summary>
    /// Sometimes, an idiot overrides a delay action subscription to <see cref="EditorApplication.delayCall"/>,
    /// which can be done because the people at Unity didn't know what events were once upon a time.
    /// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
    /// </summary>
    [InitializeOnLoad]
    public static class UnityEditorEventUtility
    {
        private static readonly MemberInfo EditorApplication_delayCall_Member = typeof(EditorApplication).GetMember("delayCall", Flags.StaticAnyVisibility).FirstOrDefault();

        private static readonly object actionsToSubscribe_LOCK = new object();
        private static readonly List<Action> actionsToSubscribe = new List<Action>();

        private static readonly EventInfo onProjectChangedEvent = typeof(EditorApplication).GetEvent("projectChanged");

        public static readonly bool HasOnProjectChanged = onProjectChangedEvent != null;

        static UnityEditorEventUtility()
        {
            EditorApplication.update += OnUpdate;
        }

        public static event Action OnProjectChanged
        {
            add
            {
                if (onProjectChangedEvent != null)
                {
                    onProjectChangedEvent.AddEventHandler(null, value);
                }
                else throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
            }
            remove
            {
                if (onProjectChangedEvent != null)
                {
                    onProjectChangedEvent.RemoveEventHandler(null, value);
                }
                else throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
            }
        }

        /// <summary>
        /// Sometimes, an idiot overrides a delay action subscription to <see cref="EditorApplication.delayCall"/>,
        /// which can be done because the people at Unity didn't know what events were once upon a time.
        /// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
        /// <para />
        /// As opposed to <see cref="DelayAction"/>, this method is safe to call from any thread, and will
        /// delay the actual subscription to a safe time.
        /// </summary>
        public static void DelayActionThreadSafe(Action action)
        {
            lock (actionsToSubscribe_LOCK)
            {
                actionsToSubscribe.Add(action);
            }
        }


        /// <summary>
        /// Sometimes, an idiot overrides a delay action subscription to <see cref="EditorApplication.delayCall"/>,
        /// which can be done because the people at Unity didn't know what events were once upon a time.
        /// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
        /// </summary>
        public static void DelayAction(Action action)
        {
            bool executed = false;

            Action execute = null;

            EditorApplication.ProjectWindowItemCallback projectWindowItemOnGUI = (_, __) => execute();
            EditorApplication.HierarchyWindowItemCallback hierarchyWindowItemOnGUI = (_, __) => execute();
            EditorApplication.CallbackFunction update = () => execute();
            Action delayCall = () => execute();

            execute = () =>
            {
                if (!executed)
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        executed = true;

                        EditorApplication.projectWindowItemOnGUI -= projectWindowItemOnGUI;
                        EditorApplication.hierarchyWindowItemOnGUI -= hierarchyWindowItemOnGUI;
                        EditorApplication.update -= update;
                        EditorApplication_delayCall -= delayCall;
                    }
                }
            };

            EditorApplication.projectWindowItemOnGUI += projectWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += hierarchyWindowItemOnGUI;
            EditorApplication.update += update;
            EditorApplication_delayCall += delayCall;
        }

        private static void OnUpdate()
        {
            lock (actionsToSubscribe_LOCK)
            {
                if (actionsToSubscribe.Count > 0)
                {
                    for (int i = 0; i < actionsToSubscribe.Count; i++)
                    {
                        DelayAction(actionsToSubscribe[i]);
                    }

                    actionsToSubscribe.Clear();
                }
            }
        }

        /// <summary>
        /// In 2020.1, Unity changed EditorApplication.delayCall from a field to an event, meaning 
        /// we now have to use reflection to access it consistently across all versions of Unity.
        /// </summary>
        public static event Action EditorApplication_delayCall
        {
            add
            {
                if (EditorApplication_delayCall_Member == null) throw new InvalidOperationException("EditorApplication.delayCall field or event could not be found. Odin will be broken.");

                if (EditorApplication_delayCall_Member is FieldInfo)
                {
                    EditorApplication.CallbackFunction val = (EditorApplication.CallbackFunction)(EditorApplication_delayCall_Member as FieldInfo).GetValue(null);
                    val += value.ConvertDelegate<EditorApplication.CallbackFunction>();
                    (EditorApplication_delayCall_Member as FieldInfo).SetValue(null, val);
                }
                else if (EditorApplication_delayCall_Member is EventInfo)
                {
                    (EditorApplication_delayCall_Member as EventInfo).AddEventHandler(null, value);
                }
                else
                {
                    if (EditorApplication_delayCall_Member == null) throw new InvalidOperationException("EditorApplication.delayCall was not a field or an event. Odin will be broken.");
                }
            }
            remove
            {
                if (EditorApplication_delayCall_Member == null) throw new InvalidOperationException("EditorApplication.delayCall field or event could not be found. Odin will be broken.");

                if (EditorApplication_delayCall_Member is FieldInfo)
                {
                    EditorApplication.CallbackFunction val = (EditorApplication.CallbackFunction)(EditorApplication_delayCall_Member as FieldInfo).GetValue(null);
                    val -= value.ConvertDelegate<EditorApplication.CallbackFunction>();
                    (EditorApplication_delayCall_Member as FieldInfo).SetValue(null, val);
                }
                else if (EditorApplication_delayCall_Member is EventInfo)
                {
                    (EditorApplication_delayCall_Member as EventInfo).RemoveEventHandler(null, value);
                }
                else
                {
                    if (EditorApplication_delayCall_Member == null) throw new InvalidOperationException("EditorApplication.delayCall was not a field or an event. Odin will be broken.");
                }
            }
        }

        private static T ConvertDelegate<T>(this Delegate src)
        {
            if (src == null || src.GetType() == typeof(T))
                return (T)(object)src;

            if (src.GetInvocationList().Count() == 1)
            {
                return (T)(object)Delegate.CreateDelegate(typeof(T), src.Target, src.Method);
            }
            else
            {
                return (T)(object)src.GetInvocationList().Aggregate<Delegate, Delegate>(null, (current, d) => Delegate.Combine(current, (Delegate)(object)ConvertDelegate<T>(d)));
            }
        }
    }
}
#endif