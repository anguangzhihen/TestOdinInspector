#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIClipInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Reflection;
    using Utilities;
    using UnityEngine;

    /// <summary>
    /// Emitted wrapper for the internal "UnityEngine.GUIClip" class.
    /// </summary>
    public static class GUIClipInfo
    {
        private static Type GUIClipType = typeof(GUI).Assembly.GetType("UnityEngine.GUIClip");
        private static Func<bool> enabledGetter = DeepReflection.CreateValueGetter<bool>(GUIClipType, "enabled");
        private static Func<Rect> topMostRectGetter = DeepReflection.CreateValueGetter<Rect>(GUIClipType, "topmostRect");
        private static Func<Rect> visibleRectGetter = DeepReflection.CreateValueGetter<Rect>(GUIClipType, "visibleRect");

        private static Func<Vector2, Vector2> unclipVectorGetter = (Func<Vector2, Vector2>)Delegate.CreateDelegate(
            typeof(Func<Vector2, Vector2>),
            GUIClipType.GetMethod("Unclip", new Type[] { typeof(Vector2) }));

        private static Func<Rect, Rect> unclipRectGetter = (Func<Rect, Rect>)Delegate.CreateDelegate(
            typeof(Func<Rect, Rect>), 
            GUIClipType.GetMethod("Unclip", new Type[] { typeof(Rect) }));

        private static Func<Rect> getTopRectGetter = (Func<Rect>)Delegate.CreateDelegate(
            typeof(Func<Rect>),
            GUIClipType.GetMethod("GetTopRect", Flags.StaticAnyVisibility, null, Type.EmptyTypes, null));

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool Enabled { get { return enabledGetter(); } }

        /// <summary>
        /// Gets the top most clipped rect.
        /// </summary>
        public static Rect TopMostRect { get { return topMostRectGetter(); } }

        /// <summary>
        /// Gets the visible rect.
        /// </summary>
        public static Rect VisibleRect { get { return visibleRectGetter(); } }

        /// <summary>
        /// Gets the top rect.
        /// </summary>
        public static Rect TopRect { get { return getTopRectGetter(); } }

        /// <summary>
        /// Unclips the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns></returns>
        public static Vector2 Unclip(Vector2 pos)
        {
            return unclipVectorGetter(pos);
        }

        /// <summary>
        /// Unclips the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns></returns>
        public static Rect Unclip(Rect rect)
        {
            return unclipRectGetter(rect);
        }
    }
}
#endif