//-----------------------------------------------------------------------
// <copyright file="UnityExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Reflection;

    /// <summary>
    /// Extends various Unity classes.
    /// </summary>
    public static class UnityExtensions
    {
        private static readonly ValueGetter<UnityEngine.Object, IntPtr> UnityObjectCachedPtrFieldGetter;

        static UnityExtensions()
        {
            var field = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                UnityObjectCachedPtrFieldGetter = EmitUtilities.CreateInstanceFieldGetter<UnityEngine.Object, IntPtr>(field);
            }
        }

        /// <summary>
        /// Determines whether a Unity object is null or "fake null",
        /// without ever calling Unity's own equality operators.
        /// This method is useful for checking if a Unity object is
        /// null, destroyed or missing at times when it is not allowed
        /// to call Unity's own equality operators, for example when
        /// not running on the main thread.
        /// </summary>
        /// <param name="obj">The Unity object to check.</param>
        /// <returns>True if the object is null, missing or destroyed; otherwise false.</returns>
        public static bool SafeIsUnityNull(this UnityEngine.Object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return true;
            }

            if (UnityObjectCachedPtrFieldGetter == null)
            {
                throw new NotSupportedException("Could not find the field 'm_CachedPtr' in the class UnityEngine.Object; cannot perform a special null check.");
            }

            IntPtr ptr = UnityObjectCachedPtrFieldGetter(ref obj);
            return ptr == IntPtr.Zero;
        }
    }
}