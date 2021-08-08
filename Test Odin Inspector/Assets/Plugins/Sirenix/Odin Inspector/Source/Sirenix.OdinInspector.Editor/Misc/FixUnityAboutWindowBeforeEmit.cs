#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FixUnityAboutWindowBeforeEmit.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// <para>This class fixes Unity's about window, by invoking "UnityEditor.VisualStudioIntegration.UnityVSSupport.GetAboutWindowLabel" before any dynamic assemblies have been defined.</para>
    /// <para>This is because dynamic assemblies in the current AppDomain break that method, and Unity's about window depends on it.</para>
    /// </summary>
    [InitializeOnLoad]
    internal static class FixUnityAboutWindowBeforeEmit
    {
        static FixUnityAboutWindowBeforeEmit()
        {
            Fix();
        }

        private static bool isFixed = false;

        public static void Fix()
        {
            if (!isFixed)
            {
                isFixed = true;

                Type unityVSSupport = typeof(Editor).Assembly.GetType("UnityEditor.VisualStudioIntegration.UnityVSSupport");

                if (unityVSSupport != null)
                {
                    MethodInfo getAboutWindowLabel = unityVSSupport.GetMethod("GetAboutWindowLabel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    if (getAboutWindowLabel != null)
                    {
                        try
                        {
                            getAboutWindowLabel.Invoke(null, null);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
#endif