//-----------------------------------------------------------------------
// <copyright file="UnityVersion.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization.Utilities
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Utility class indicating current Unity version.
    /// </summary>
#if UNITY_EDITOR

    [UnityEditor.InitializeOnLoad]
#endif
    internal static class UnityVersion
    {
        static UnityVersion()
        {
            string[] version = Application.unityVersion.Split('.');

            if (version.Length < 2)
            {
                Debug.LogError("Could not parse current Unity version '" + Application.unityVersion + "'; not enough version elements.");
                return;
            }

            if (int.TryParse(version[0], out Major) == false)
            {
                Debug.LogError("Could not parse major part '" + version[0] + "' of Unity version '" + Application.unityVersion + "'.");
            }

            if (int.TryParse(version[1], out Minor) == false)
            {
                Debug.LogError("Could not parse minor part '" + version[1] + "' of Unity version '" + Application.unityVersion + "'.");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureLoaded()
        {
            // This method ensures that this type has been initialized before any loading of objects occurs.
            // If this isn't done, the static constructor may be invoked at an illegal time that is not
            // allowed by Unity. During scene deserialization, off the main thread, is an example.
        }

        /// <summary>
        /// Tests current Unity version is equal or greater.
        /// </summary>
        /// <param name="major">Minimum major version.</param>
        /// <param name="minor">Minimum minor version.</param>
        /// <returns><c>true</c> if the current Unity version is greater. Otherwise <c>false</c>.</returns>
        public static bool IsVersionOrGreater(int major, int minor)
        {
            return UnityVersion.Major > major || (UnityVersion.Major == major && UnityVersion.Minor >= minor);
        }

        /// <summary>
        /// The current Unity version major.
        /// </summary>
        public static readonly int Major;

        /// <summary>
        /// The current Unity version minor.
        /// </summary>
        public static readonly int Minor;
    }
}