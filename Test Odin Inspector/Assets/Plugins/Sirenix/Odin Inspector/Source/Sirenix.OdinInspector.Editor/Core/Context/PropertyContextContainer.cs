#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyContextContainer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;
    using Utilities;

    /// <summary>
    /// <para>Contains a context for an <see cref="InspectorProperty"/>, which offers the ability to address persistent values by key across several editor GUI frames.</para>
    /// <para>Use this in drawers to store contextual editor-only values such as the state of a foldout.</para>
    /// </summary>
    public sealed class PropertyContextContainer
    {
        private Dictionary<string, object> globalContexts;
        private InspectorProperty property;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContextContainer"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <exception cref="System.ArgumentNullException">property</exception>
        public PropertyContextContainer(InspectorProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            this.property = property;
        }

        private bool TryGetGlobalConfig<T>(string key, out PropertyContext<T> context, out bool isNew)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            context = null;
            object value;

            if (this.globalContexts == null)
            {
                this.globalContexts = new Dictionary<string, object>();
            }

            var contexts = this.globalContexts;

            if (contexts.TryGetValue(key, out value))
            {
                isNew = false;
                context = value as PropertyContext<T>;

                if (context == null)
                {
                    throw new InvalidOperationException("Tried to get global property of type " + typeof(T).GetNiceName() + " with key " + key + " on property at path " + this.property.Path + ", but a global property of a different type (" + value.GetType().GetArgumentsOfInheritedOpenGenericClass(typeof(PropertyContext<>))[0].GetNiceName() + ") already existed with the same key.");
                }
            }
            else
            {
                isNew = true;
                context = PropertyContext<T>.Create();
                contexts[key] = context;
            }

            return true;
        }

        /// <summary>
        /// <para>Gets a global context value for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>The found context.</returns>
        public PropertyContext<T> GetGlobal<T>(string key, Func<T> getDefaultValue)
        {
            PropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalConfig(key, out result, out isNew) && isNew)
            {
                result.Value = getDefaultValue();
            }

            return result;
        }

        /// <summary>
        /// <para>Gets a global context value for a given key, using a given default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>The found context.</returns>
        public PropertyContext<T> GetGlobal<T>(string key, T defaultValue)
        {
            PropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalConfig(key, out result, out isNew) && isNew)
            {
                result.Value = defaultValue;
            }

            return result;
        }

        /// <summary>
        /// <para>Gets a global context value for a given key, and creates a new instance of <see cref="T"/> as a default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>The found context.</returns>
        public PropertyContext<T> GetGlobal<T>(string key) where T : new()
        {
            PropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalConfig(key, out result, out isNew) && isNew)
            {
                result.Value = new T();
            }

            return result;
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance, string key, Func<TValue> getDefaultValue)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue, TDrawer>(string key, Func<TValue> getDefaultValue) where TDrawer : OdinDrawer
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(Type drawerType, string key, Func<TValue> getDefaultValue)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance, string key, TValue defaultValue)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue, TDrawer>(string key, TValue defaultValue) where TDrawer : OdinDrawer
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(Type drawerType, string key, TValue defaultValue)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance, string key) where TValue : new()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance) where TValue : new()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance, TValue defaultValue)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue, TDrawer>(string key) where TValue : new() where TDrawer : OdinDrawer
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public PropertyContext<TValue> Get<TValue>(Type drawerType, string key) where TValue : new()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public bool Get<TValue>(OdinDrawer drawerInstance, string key, out TValue context)
            where TValue : class, new()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public bool Get<TValue>(OdinDrawer drawerInstance, string key, out PropertyContext<TValue> context)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public bool Get<TValue>(Type drawerType, string key, out PropertyContext<TValue> context)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Local property contexts are obsolete. Use local drawer fields instead.
        /// </summary>
        [Obsolete("Use local fields in the drawer instead.", true)]
        public bool Get<TValue, TDrawer>(string key, out PropertyContext<TValue> context)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a <see cref="GlobalPersistentContext{T}"/> object and creates a <see cref="LocalPersistentContext{T}"/> object for it.
        /// </summary>
        /// <typeparam name="TValue">The type of the value of the context.</typeparam>
        /// <param name="drawer">The instance of the drawer.</param>
        /// <param name="key">The key for the context.</param>
        /// <param name="defaultValue">The default value for the context.</param>
        public LocalPersistentContext<TValue> GetPersistent<TValue>(OdinDrawer drawer, string key, TValue defaultValue)
        {
            return LocalPersistentContext<TValue>.Create(PersistentContext.Get(
                TwoWaySerializationBinder.Default.BindToName(drawer.GetType()).GetHashCode(),
                TwoWaySerializationBinder.Default.BindToName(this.property.Tree.TargetType).GetHashCode(),
                this.property.Path.GetHashCode(),
                new DrawerStateSignature(this.property.RecursiveDrawDepth, InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth, this.property.DrawerChainIndex).GetHashCode(),
                key,
                defaultValue));
        }

        /// <summary>
        /// Gets a <see cref="GlobalPersistentContext{T}"/> object and creates a <see cref="LocalPersistentContext{T}"/> object for it.
        /// Returns <c>true</c> when the <see cref="GlobalPersistentContext{T}"/> is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value of the context.</typeparam>
        /// <param name="drawer">The instance of the drawer.</param>
        /// <param name="key">The key for the context.</param>
        /// <param name="context">The <see cref="LocalPersistentContext{T}"/> object.</param>
        /// <returns>Returns <c>true</c> when the <see cref="GlobalPersistentContext{T}"/> is first created. Otherwise <c>false</c>.</returns>
        public bool GetPersistent<TValue>(OdinDrawer drawer, string key, out LocalPersistentContext<TValue> context)
        {
            GlobalPersistentContext<TValue> global;
            var isNew = PersistentContext.Get(
                TwoWaySerializationBinder.Default.BindToName(drawer.GetType()).GetHashCode(),
                TwoWaySerializationBinder.Default.BindToName(this.property.Tree.TargetType).GetHashCode(),
                this.property.Path.GetHashCode(),
                new DrawerStateSignature(this.property.RecursiveDrawDepth, InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth, this.property.DrawerChainIndex).GetHashCode(),
                key,
                out global);

            context = LocalPersistentContext<TValue>.Create(global);
            return isNew;
        }

        /// <summary>
        /// Swaps context values with a given <see cref="PropertyContextContainer"/>.
        /// </summary>
        /// <param name="otherContext">The context to swap with.</param>
        public void SwapContext(PropertyContextContainer otherContext)
        {
            // Swap global configs
            {
                var temp = otherContext.globalContexts;
                otherContext.globalContexts = this.globalContexts;
                this.globalContexts = temp;
            }
        }

        [Serializable]
        private struct DrawerStateSignature : IEquatable<DrawerStateSignature>
        {
            public int RecursiveDrawDepth;
            public int CurrentInlineEditorDrawDepth;
            public int DrawerChainIndex;

            public DrawerStateSignature(int recursiveDrawDepth, int currentInlineEditorDrawDepth, int drawerChainIndex)
            {
                this.RecursiveDrawDepth = recursiveDrawDepth;
                this.CurrentInlineEditorDrawDepth = currentInlineEditorDrawDepth;
                this.DrawerChainIndex = drawerChainIndex;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + this.RecursiveDrawDepth;
                    hash = hash * 31 + this.CurrentInlineEditorDrawDepth;
                    hash = hash * 31 + this.DrawerChainIndex;
                    return hash;
                }
            }

            public override bool Equals(object obj)
            {
                return obj is DrawerStateSignature && this.Equals((DrawerStateSignature)obj);
            }

            public bool Equals(DrawerStateSignature other)
            {
                return this.RecursiveDrawDepth == other.RecursiveDrawDepth
                    && this.CurrentInlineEditorDrawDepth == other.CurrentInlineEditorDrawDepth
                    && this.DrawerChainIndex == other.DrawerChainIndex;
            }
        }
    }
}
#endif