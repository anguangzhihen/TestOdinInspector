#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AsyncProgressBar.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using UnityEditor;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public static class AsyncProgressBar
    {
        private static readonly Type AsyncProgressBarType = typeof(Editor).Assembly.GetType("UnityEditor.AsyncProgressBar");
        private static readonly Func<float> ProgressGetter = DeepReflection.CreateValueGetter<float>(AsyncProgressBarType, "progress");
        private static readonly Func<string> ProgressInfoGetter = DeepReflection.CreateValueGetter<string>(AsyncProgressBarType, "progressInfo");
        private static readonly Func<bool> IsShowingGetter = DeepReflection.CreateValueGetter<bool>(AsyncProgressBarType, "isShowing");

        private static readonly Action<string, float> DisplayCaller = (Action<string, float>)Delegate.CreateDelegate(
            typeof(Action<string, float>),
            AsyncProgressBarType.GetMethod("Display", Flags.AllMembers)
        );

        private static readonly Action ClearCaller = (Action)Delegate.CreateDelegate(
            typeof(Action),
            AsyncProgressBarType.GetMethod("Clear", Flags.AllMembers)
        );

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static float Progress { get { return ProgressGetter(); } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static string ProgressInfo { get { return ProgressInfoGetter(); } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool IsShowing { get { return IsShowingGetter(); } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static void Display(string progressInfo, float progress)
        {
            DisplayCaller(progressInfo, progress);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static void Clear()
        {
            ClearCaller();
        }
    }
}
#endif