#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FixUnityAssemblyVersionResolution.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.IO;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// <para>
    /// If you mark any of Unity's assemblies with the [AssemblyVersion] attribute to get a rolling assembly
    /// version that changes sometimes (or all the time), Odin's hardcoded assembly references to user types
    /// will break.
    /// </para>
    /// <para>
    /// To fix this case, and all other cases of references to wrongly versioned Unity types not being resolved,
    /// we overload the app domain's type resolution and resolve Unity user assemblies properly regardless of
    /// version.
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    internal static class FixUnityAssemblyVersionResolution
    {
        static FixUnityAssemblyVersionResolution()
        {
            Fix();
        }

        private static void Fix()
        {
            var unityAssemblyPrefixes = new string[]
            {
                        "Assembly-CSharp-Editor-firstpass",
                        "Assembly-CSharp-firstpass",
                        "Assembly-CSharp-Editor",
                        "Assembly-CSharp",
                        "Assembly-UnityScript-Editor-firstpass",
                        "Assembly-UnityScript-firstpass",
                        "Assembly-UnityScript-Editor",
                        "Assembly-UnityScript",
                        "Assembly-Boo-Editor-firstpass",
                        "Assembly-Boo-firstpass",
                        "Assembly-Boo-Editor",
                        "Assembly-Boo",
            };

            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
            {
                string name = args.Name;

                for (int i = 0; i < unityAssemblyPrefixes.Length; i++)
                {
                    var prefix = unityAssemblyPrefixes[i];

                    if (name.StartsWith(prefix))
                    {
                        // Remove versioning and other information from name, then resolve basic assembly name
                        var index = name.IndexOf(',');

                        if (index >= 0)
                        {
                            name = name.Substring(0, index);
                        }

                        try
                        {
                            return Assembly.Load(name);
                        }
                        catch (FileNotFoundException)
                        {
                            return null;
                        }
                        catch (ReflectionTypeLoadException)
                        {
                            return null;
                        }
                        catch (TypeLoadException)
                        {
                            return null;
                        }
                    }
                }

                return null;
            };
        }
    }
}
#endif