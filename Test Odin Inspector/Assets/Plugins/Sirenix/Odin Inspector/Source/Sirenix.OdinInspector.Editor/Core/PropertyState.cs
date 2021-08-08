#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorProperty.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using Serialization;
    using Sirenix.Utilities;
    using UnityEngine;

    /// <summary>
    /// <para>This is a class for creating, getting and modifying a property's various states. An instance of this class always comes attached to an InspectorProperty.</para>
    /// <para>See Odin's tutorials for more information about usage of the state system.</para>
    /// </summary>
    public sealed class PropertyState
    {
        private bool visible;
        private bool visibleLastLayout;

        private bool enabled;
        private bool enabledLastLayout;

        private LocalPersistentContext<bool> expanded;
        private bool expandedLastLayout;
        
        private InspectorProperty property;
        private int index;

        private Dictionary<string, CustomState> customStates;

        private class CustomState
        {
            public object Value;
            public object ValueLastLayout;
            public object DefaultValue;
            public Type Type;
            public ILocalPersistentContext PersistentValue;
        }

        public PropertyState(InspectorProperty property, int index)
        {
            this.property = property;
            this.index = index;

            if (this.property.ChildResolver is ICollectionResolver)
            {
                this.expandedLastLayout = GeneralDrawerConfig.Instance.ExpandFoldoutByDefault;
            }
            else
            {
                this.expandedLastLayout = GeneralDrawerConfig.Instance.OpenListsByDefault;
            }

            this.Reset();
            this.Update();
        }

        /// <summary>
        /// If set to true, all state changes for this property will be logged to the console.
        /// </summary>
        public bool LogChanges = false;

        /// <summary>
        /// Whether the property is visible in the inspector.
        /// </summary>
        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (this.visible == value) return;

                if (this.LogChanges)
                {
                    this.LogChange("Visible", this.visible, value);
                }

                this.visible = value;
                this.SendStateChangedNotifications("Visible");
            }
        }

        /// <summary>
        /// Whether the Visible state was true or not during the last layout event.
        /// </summary>
        public bool VisibleLastLayout { get { return this.visibleLastLayout; } }

        /// <summary>
        /// Whether the property is enabled in the inspector.
        /// </summary>
        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                if (this.enabled == value) return;

                if (this.LogChanges)
                {
                    this.LogChange("Enabled", this.enabled, value);
                }

                this.enabled = value;
                this.SendStateChangedNotifications("Enabled");
            }
        }

        /// <summary>
        /// Whether the Enabled state was true or not during the last layout event.
        /// </summary>
        public bool EnabledLastLayout { get { return this.enabledLastLayout; } }

        /// <summary>
        /// Whether the property is expanded in the inspector.
        /// </summary>
        public bool Expanded
        {
            get
            {
                if (this.expanded == null)
                {
                    this.expanded = this.GetPersistentContext<bool>("expanded", this.expandedLastLayout);
                }

                return this.expanded.Value;
            }
            set
            {
                if (this.expanded == null)
                {
                    this.expanded = this.GetPersistentContext<bool>("expanded", this.expandedLastLayout);
                }

                if (this.expanded.Value == value) return;

                if (this.LogChanges)
                {
                    this.LogChange("Expanded", this.expanded.Value, value);
                }

                this.expanded.Value = value;
                this.SendStateChangedNotifications("Expanded");
            }
        }

        /// <summary>
        /// Whether the Expanded state was true or not during the last layout event.
        /// </summary>
        public bool ExpandedLastLayout { get { return this.expandedLastLayout; } }
        
        /// <summary>
        /// Creates a custom state with a given name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="persistent"></param>
        /// <param name="defaultValue"></param>
        public void Create<T>(string key, bool persistent, T defaultValue)
        {
            if (this.customStates == null)
            {
                this.customStates = new Dictionary<string, CustomState>();
            }
            else if (this.customStates.ContainsKey(key))
            {
                throw new InvalidOperationException("The state '" + key + "' already exists on the property '" + this.property.Path + "'; can't create a new one with the same key.");
            }

            var state = new CustomState();

            state.Type = typeof(T);

            if (persistent)
            {
                state.PersistentValue = GetPersistentContext<T>(key, defaultValue);
                state.ValueLastLayout = state.PersistentValue.WeakValue;
            }
            else
            {
                state.Value = defaultValue;
                state.ValueLastLayout = state.Value;
                state.DefaultValue = defaultValue;
            }

            this.customStates.Add(key, state);
            this.SendStateChangedNotifications(key);
        }

        /// <summary>
        /// Determines whether a state with the given key exists.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the state exists, otherwise, false.</returns>
        public bool Exists(string key)
        {
            bool isPersistent;
            Type valueType;
            return this.Exists(key, out isPersistent, out valueType);
        }

        /// <summary>
        /// Determines whether a state with the given key exists.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="isPersistent">If the state exists, this out parameter will be true if the state is persistent.</param>
        /// <returns>True if the state exists, otherwise, false.</returns>
        public bool Exists(string key, out bool isPersistent)
        {
            Type valueType;
            return this.Exists(key, out isPersistent, out valueType);

        }

        /// <summary>
        /// Determines whether a state with the given key exists.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="valueType">If the state exists, this out parameter will contain the type of value that the state contains.</param>
        /// <returns>True if the state exists, otherwise, false.</returns>
        public bool Exists(string key, out Type valueType)
        {
            bool isPersistent;
            return this.Exists(key, out isPersistent, out valueType);
        }

        /// <summary>
        /// Determines whether a state with the given key exists.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="isPersistent">If the state exists, this out parameter will be true if the state is persistent.</param>
        /// <param name="valueType">If the state exists, this out parameter will contain the type of value that the state contains.</param>
        /// <returns>True if the state exists, otherwise, false.</returns>
        public bool Exists(string key, out bool isPersistent, out Type valueType)
        {
            switch (key)
            {
                case "Expanded":
                    isPersistent = true;
                    valueType = typeof(bool);
                    return true;

                case "Visible":
                    isPersistent = false;
                    valueType = typeof(bool);
                    return true;

                case "Enabled":
                    isPersistent = false;
                    valueType = typeof(bool);
                    return true;
            }

            CustomState state;

            if (this.customStates != null && this.customStates.TryGetValue(key, out state))
            {
                isPersistent = state.PersistentValue != null;
                valueType = state.Type;
                return true;
            }

            isPersistent = false;
            valueType = null;
            return false;
        }

        /// <summary>
        /// Gets the value of a given state as an instance of type T.
        /// </summary>
        /// <typeparam name="T">The type to get the state value as. An <see cref="InvalidOperationException"/> will be thrown if the state's value type cannot be assigned to T.</typeparam>
        /// <param name="key">The key of the state to get. An <see cref="InvalidOperationException"/> will be thrown if a state with the given key does not exist.</param>
        /// <returns>The value of the state.</returns>
        public T Get<T>(string key)
        {
            CustomState state;

            if (this.customStates != null && this.customStates.TryGetValue(key, out state))
            {
                try
                {
                    return (T)(state.PersistentValue != null ? state.PersistentValue.WeakValue : state.Value);
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException("Cannot get property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + state.Type.GetNiceName() + "'.");
                }
            }

            throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + this.property.Path + "'.");
        }

        /// <summary>
        /// Gets the value that a given state contained last layout as an instance of type T.
        /// </summary>
        /// <typeparam name="T">The type to get the state value as. An <see cref="InvalidOperationException"/> will be thrown if the state's value type cannot be assigned to T.</typeparam>
        /// <param name="key">The key of the state to get. An <see cref="InvalidOperationException"/> will be thrown if a state with the given key does not exist.</param>
        /// <returns>The value of the state during the last layout event.</returns>
        public T GetLastLayout<T>(string key)
        {
            CustomState state;

            if (this.customStates != null && this.customStates.TryGetValue(key, out state))
            {
                try
                {
                    return (T)state.ValueLastLayout;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException("Cannot get property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + state.Type.GetNiceName() + "'.");
                }
            }

            throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + this.property.Path + "'.");
        }

        /// <summary>
        /// Sets the value of a given state to a given value.
        /// </summary>
        /// <typeparam name="T">The type to set the state value as. An <see cref="InvalidOperationException"/> will be thrown if T cannot be assigned to the state's value type.</typeparam>
        /// <param name="key">The key of the state to set the value of. An <see cref="InvalidOperationException"/> will be thrown if a state with the given key does not exist.</param>
        /// <param name="value">The value to set.</param>
        public void Set<T>(string key, T value)
        {
            CustomState state;

            if (this.customStates != null && this.customStates.TryGetValue(key, out state))
            {
                if (typeof(T) != state.Type)
                {
                    throw new InvalidOperationException("Cannot set property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + state.Type.GetNiceName() + "'.");
                }

                T current = (T)(state.PersistentValue != null ? state.PersistentValue.WeakValue : state.Value);

                // This avoids constant boxing overhead if the state is set to the same value all the time
                if (!PropertyValueEntry<T>.EqualityComparer(current, value))
                {
                    if (this.LogChanges)
                    {
                        this.LogChange(key, current, value);
                    }

                    if (state.PersistentValue != null)
                    {
                        state.PersistentValue.WeakValue = value;
                    }
                    else
                    {
                        state.Value = value;
                    }

                    this.SendStateChangedNotifications(key);
                }
            }
            else
            {
                throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + this.property.Path + "'.");
            }
        }

        private LocalPersistentContext<T> GetPersistentContext<T>(string key, T defaultValue)
        {
            return PersistentContext.GetLocal(TwoWaySerializationBinder.Default.BindToName(this.property.Tree.TargetType).GetHashCode(), this.property.Path, this.index, key, defaultValue);
        }

        internal void Update()
        {
            if (Event.current == null || Event.current.type == EventType.Layout)
            {
                this.visibleLastLayout = this.visible;
                this.enabledLastLayout = this.enabled;

                if (this.expanded != null)
                {
                    this.expandedLastLayout = this.expanded.Value;
                }

                if (this.customStates != null)
                {
                    foreach (var state in this.customStates.GFValueIterator())
                    {
                        state.ValueLastLayout = state.PersistentValue != null ? state.PersistentValue.WeakValue : state.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Cleans the property state and prepares it for cached reuse of its containing PropertyTree. This will also reset the state.
        /// </summary>
        public void CleanForCachedReuse()
        {
            if (this.customStates != null)
                this.customStates.Clear();

            this.Reset();
        }

        /// <summary>
        /// Resets all states to their default values. Persistent states will be updated to their persistent cached value if one exists.
        /// </summary>
        public void Reset()
        {
            this.enabled = true;
            this.visible = true;

            if (this.expanded != null)
            {
                this.expanded.UpdateLocalValue();
            }

            if (this.customStates != null)
            {
                foreach (var state in this.customStates.GFValueIterator())
                {
                    if (state.PersistentValue != null)
                    {
                        state.PersistentValue.UpdateLocalValue();
                    }
                    else
                    {
                        state.Value = state.DefaultValue;
                    }
                }
            }
        }

        private void LogChange<T>(string state, T oldValue, T newValue)
        {
            var str = "Property '" + this.property.Path + "'s '" + state + "' state changed from '" + oldValue + "' to '" + newValue + "'";

            if (Event.current == null)
            {
                str += " while outside IMGUI context";
            }
            else
            {
                str += " during IMGUI event '" + Event.current.type + "'";
            }

            Debug.Log(str);
        }

        private void SendStateChangedNotifications(string state)
        {
            // Self state changed
            {
                var chain = this.property.GetActiveDrawerChain().BakedDrawerArray;

                for (int i = 0; i < chain.Length; i++)
                {
                    var notification = chain[i] as IOnSelfStateChangedNotification;

                    if (notification != null)
                    {
                        try
                        {
                            notification.OnSelfStateChanged(state);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                var stateUpdaters = this.property.StateUpdaters;

                for (int i = 0; i < stateUpdaters.Length; i++)
                {
                    var notification = stateUpdaters[i] as IOnSelfStateChangedNotification;

                    if (notification != null)
                    {
                        try
                        {
                            notification.OnSelfStateChanged(state);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

            var parent = this.property.Parent;

            if (parent == null && this.property != this.property.Tree.RootProperty)
            {
                parent = this.property.Tree.RootProperty;
            }

            if (parent != null)
            {
                var chain = parent.GetActiveDrawerChain().BakedDrawerArray;
                var index = this.property.Index;

                for (int i = 0; i < chain.Length; i++)
                {
                    var notification = chain[i] as IOnChildStateChangedNotification;

                    if (notification != null)
                    {
                        try
                        {
                            notification.OnChildStateChanged(index, state);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                var stateUpdaters = parent.StateUpdaters;

                for (int i = 0; i < stateUpdaters.Length; i++)
                {
                    var notification = stateUpdaters[i] as IOnChildStateChangedNotification;

                    if (notification != null)
                    {
                        try
                        {
                            notification.OnChildStateChanged(index, state);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                var current = parent;

                while (current != null)
                {
                    chain = current.GetActiveDrawerChain().BakedDrawerArray;

                    for (int i = 0; i < chain.Length; i++)
                    {
                        var notification = chain[i] as IRecursiveOnChildStateChangedNotification;

                        if (notification != null)
                        {
                            try
                            {
                                notification.OnChildStateChanged(this.property, state);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }

                    stateUpdaters = current.StateUpdaters;

                    for (int i = 0; i < stateUpdaters.Length; i++)
                    {
                        var notification = stateUpdaters[i] as IRecursiveOnChildStateChangedNotification;

                        if (notification != null)
                        {
                            try
                            {
                                notification.OnChildStateChanged(this.property, state);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }

                    var nextCurrent = current.Parent;

                    if (nextCurrent == null && current != current.Tree.RootProperty)
                    {
                        nextCurrent = current.Tree.RootProperty;
                    }

                    current = nextCurrent;
                }
            }
        }
    }
}
#endif