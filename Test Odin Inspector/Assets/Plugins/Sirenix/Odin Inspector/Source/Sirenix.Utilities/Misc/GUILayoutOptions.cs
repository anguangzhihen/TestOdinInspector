//-----------------------------------------------------------------------
// <copyright file="GUILayoutOptions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// <para>
    /// GUILayoutOptions is a handy utility that provides cached GUILayoutOpion arrays based on the wanted parameters.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    /// Most GUILayout and EditorGUILayout methods takes an optional "params GUILayoutOption[]" parameter.
    /// Each time you call this, an array is allocated generating garbage.
    /// </para>
    /// <code>
    /// // Generates garbage:
    /// GUILayout.Label(label, GUILayout.Label(label, GUILayout.Width(20), GUILayout.ExpandHeight(), GUILayout.MaxWidth(300)));
    ///
    /// // Does not generate garbage:
    /// GUILayout.Label(label, GUILayout.Label(label, GUILayoutOptions.Width(20).ExpandHeight().MaxWidth(300)));
    /// </code>
    /// </example>
    public static class GUILayoutOptions
    {
        private static int CurrentCacheIndex = 0;
        private static readonly GUILayoutOptionsInstance[] GUILayoutOptionsInstanceCache;
        private static readonly Dictionary<GUILayoutOptionsInstance, GUILayoutOption[]> GUILayoutOptionsCache = new Dictionary<GUILayoutOptionsInstance, GUILayoutOption[]>();

        /// <summary>
        /// An EmptyGUIOption[] array with a length of 0.
        /// </summary>
        public static readonly GUILayoutOption[] EmptyGUIOptions = new GUILayoutOption[0];

        static GUILayoutOptions()
        {
            GUILayoutOptionsInstanceCache = new GUILayoutOptionsInstance[30];
            GUILayoutOptionsInstanceCache[0] = new GUILayoutOptionsInstance();
            for (int i = 1; i < 30; i++)
            {
                GUILayoutOptionsInstanceCache[i] = new GUILayoutOptionsInstance();
                GUILayoutOptionsInstanceCache[i].Parent = GUILayoutOptionsInstanceCache[i - 1];
            }
        }

        internal enum GUILayoutOptionType
        {
            Width,
            Height,
            MinWidth,
            MaxHeight,
            MaxWidth,
            MinHeight,
            ExpandHeight,
            ExpandWidth,
        }

        /// <summary>
        /// A GUILayoutOptions instance with an implicit operator to be converted to a GUILayoutOption[] array.
        /// </summary>
        /// <seealso cref="GUILayoutOptions"/>
        public sealed class GUILayoutOptionsInstance : IEquatable<GUILayoutOptionsInstance>
        {
            private float value;

            internal GUILayoutOptionsInstance Parent;

            internal GUILayoutOptionType GUILayoutOptionType;

            private GUILayoutOption[] GetCachedOptions()
            {
                GUILayoutOption[] value;
                if (GUILayoutOptionsCache.TryGetValue(this, out value) == false)
                {
                    value = GUILayoutOptionsCache[this.Clone()] = this.CreateOptionsArary();
                }
                return value;
            }

            /// <summary>
            /// Gets or creates the cached GUILayoutOption array based on the layout options specified.
            /// </summary>
            public static implicit operator GUILayoutOption[] (GUILayoutOptionsInstance options)
            {
                return options.GetCachedOptions();
            }

            private GUILayoutOption[] CreateOptionsArary()
            {
                List<GUILayoutOption> options = new List<GUILayoutOption>();

                var curr = this;
                while (curr != null)
                {
                    switch (curr.GUILayoutOptionType)
                    {
                        case GUILayoutOptionType.Width:
                            options.Add(GUILayout.Width(curr.value));
                            break;

                        case GUILayoutOptionType.Height:
                            options.Add(GUILayout.Height(curr.value));
                            break;

                        case GUILayoutOptionType.MaxHeight:
                            options.Add(GUILayout.MaxHeight(curr.value));
                            break;

                        case GUILayoutOptionType.MaxWidth:
                            options.Add(GUILayout.MaxWidth(curr.value));
                            break;

                        case GUILayoutOptionType.MinHeight:
                            options.Add(GUILayout.MinHeight(curr.value));
                            break;

                        case GUILayoutOptionType.MinWidth:
                            options.Add(GUILayout.MinWidth(curr.value));
                            break;

                        case GUILayoutOptionType.ExpandHeight:
                            options.Add(GUILayout.ExpandHeight(curr.value > 0.2f));
                            break;

                        case GUILayoutOptionType.ExpandWidth:
                            options.Add(GUILayout.ExpandWidth(curr.value > 0.2f));
                            break;
                    }
                    curr = curr.Parent;
                }
                return options.ToArray();
            }

            private GUILayoutOptionsInstance Clone()
            {
                GUILayoutOptionsInstance result = null;
                result = new GUILayoutOptionsInstance() { value = this.value, GUILayoutOptionType = this.GUILayoutOptionType };
                var currResult = result;
                var curr = this.Parent;
                while (curr != null)
                {
                    currResult.Parent = new GUILayoutOptionsInstance() { value = curr.value, GUILayoutOptionType = curr.GUILayoutOptionType };
                    curr = curr.Parent;
                    currResult = currResult.Parent;
                }
                return result;
            }

            internal GUILayoutOptionsInstance()
            {
            }

            /// <summary>
            /// Option passed to a control to give it an absolute width.
            /// </summary>
            public GUILayoutOptionsInstance Width(float width)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.Width, width);
                return instance;
            }

            /// <summary>
            /// Option passed to a control to give it an absolute height.
            /// </summary>
            public GUILayoutOptionsInstance Height(float height)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.Height, height);
                return instance;
            }

            /// <summary>
            /// Option passed to a control to specify a maximum height.
            /// </summary>
            public GUILayoutOptionsInstance MaxHeight(float height)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.MaxHeight, height);
                return instance;
            }

            /// <summary>
            /// Option passed to a control to specify a maximum width.
            /// </summary>
            public GUILayoutOptionsInstance MaxWidth(float width)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.MaxWidth, width);
                return instance;
            }

            /// <summary>
            /// Option passed to a control to specify a minimum height.
            /// </summary>
            public GUILayoutOptionsInstance MinHeight(float height)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.MinHeight, height);
                return instance;
            }

            /// <summary>
            /// Option passed to a control to specify a minimum width.
            /// </summary>
            public GUILayoutOptionsInstance MinWidth(float width)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.MinWidth, width);
                return instance;
            }

            /// <summary>
            /// Option passed to a control to allow or disallow vertical expansion.
            /// </summary>
            public GUILayoutOptionsInstance ExpandHeight(bool expand = true)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.ExpandHeight, expand);
                return instance;
            }

            /// <summary>
            /// Option passed to a control to allow or disallow horizontal expansion.
            /// </summary>
            public GUILayoutOptionsInstance ExpandWidth(bool expand = true)
            {
                var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
                instance.SetValue(GUILayoutOptionType.ExpandWidth, expand);
                return instance;
            }

            internal void SetValue(GUILayoutOptionType type, float value)
            {
                this.GUILayoutOptionType = type;
                this.value = value;
            }

            internal void SetValue(GUILayoutOptionType type, bool value)
            {
                this.GUILayoutOptionType = type;
                this.value = value ? 1 : 0;
            }

            /// <summary>
            /// Determines whether the instance is equals another instance.
            /// </summary>
            public bool Equals(GUILayoutOptionsInstance other)
            {
                var currA = this;
                var currB = other;
                while (currA != null && currB != null)
                {
                    if (currA.GUILayoutOptionType != currB.GUILayoutOptionType || currA.value != currB.value)
                    {
                        return false;
                    }

                    currA = currA.Parent;
                    currB = currB.Parent;
                }

                if (currB != null || currA != null)
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            public override int GetHashCode()
            {
                unchecked
                {
                    int count = 0;
                    int hash = 17;
                    var curr = this;
                    while (curr != null)
                    {
                        hash = hash * 29 + this.GUILayoutOptionType.GetHashCode() + this.value.GetHashCode() * 17 + count++;
                        curr = curr.Parent;
                    }
                    return hash;
                }
            }
        }

        /// <summary>
        /// Option passed to a control to give it an absolute width.
        /// </summary>
        public static GUILayoutOptionsInstance Width(float width)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.Width, width);
            return instance;
        }

        /// <summary>
        /// Option passed to a control to give it an absolute height.
        /// </summary>
        public static GUILayoutOptionsInstance Height(float height)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.Height, height);
            return instance;
        }

        /// <summary>
        /// Option passed to a control to specify a maximum height.
        /// </summary>
        public static GUILayoutOptionsInstance MaxHeight(float height)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.MaxHeight, height);
            return instance;
        }

        /// <summary>
        /// Option passed to a control to specify a maximum width.
        /// </summary>
        public static GUILayoutOptionsInstance MaxWidth(float width)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.MaxWidth, width);
            return instance;
        }

        /// <summary>
        /// Option passed to a control to specify a minimum width.
        /// </summary>
        public static GUILayoutOptionsInstance MinWidth(float width)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.MinWidth, width);
            return instance;
        }

        /// <summary>
        /// Option passed to a control to specify a minimum height.
        /// </summary>
        public static GUILayoutOptionsInstance MinHeight(float height)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.MinHeight, height);
            return instance;
        }

        /// <summary>
        /// Option passed to a control to allow or disallow vertical expansion.
        /// </summary>
        public static GUILayoutOptionsInstance ExpandHeight(bool expand = true)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.ExpandHeight, expand);
            return instance;
        }

        /// <summary>
        /// Option passed to a control to allow or disallow horizontal expansion.
        /// </summary>
        public static GUILayoutOptionsInstance ExpandWidth(bool expand = true)
        {
            CurrentCacheIndex = 0;
            var instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
            instance.SetValue(GUILayoutOptionType.ExpandWidth, expand);
            return instance;
        }
    }
}