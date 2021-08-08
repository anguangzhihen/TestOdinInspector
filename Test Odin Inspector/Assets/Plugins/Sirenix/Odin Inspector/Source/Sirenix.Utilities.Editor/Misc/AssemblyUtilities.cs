#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssemblyUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// A utility class for finding types in various asssembly.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class AssemblyUtilities
    {
        private static string[] userAssemblyPrefixes = new string[]
        {
            "Assembly-CSharp",
            "Assembly-UnityScript",
            "Assembly-Boo",
            "Assembly-CSharp-Editor",
            "Assembly-UnityScript-Editor",
            "Assembly-Boo-Editor",
        };

        private static string[] pluginAssemblyPrefixes = new string[]
        {
            "Assembly-CSharp-firstpass",
            "Assembly-CSharp-Editor-firstpass",
            "Assembly-UnityScript-firstpass",
            "Assembly-UnityScript-Editor-firstpass",
            "Assembly-Boo-firstpass",
            "Assembly-Boo-Editor-firstpass",
        };

        private static readonly Dictionary<Assembly, bool> IsDynamicCache = new Dictionary<Assembly, bool>(ReferenceEqualityComparer<Assembly>.Default);
        private static readonly object IS_DYNAMIC_CACHE_LOCK = new object();
        private static readonly object ASSEMBLY_LOAD_QUEUE_LOCK = new object();
        private static readonly object ASSEMBLY_TYPE_FLAG_LOOKUP_LOCK = new object();

        private static readonly object INITIALIZATION_SETUP_LOCK = new object();
        private static readonly object ASSEMBLY_REGISTER_LOCK = new object();

        [NonSerialized]
        private static volatile Thread LoadThread = null;

        private static readonly List<Assembly> AssemblyLoadQueue = new List<Assembly>();
        private static bool AssemblyLoadQueueContainsItems = false;

        private static Assembly unityEngineAssembly;
#if UNITY_EDITOR
        private static Assembly unityEditorAssembly;
#endif
        private static DirectoryInfo projectFolderDirectory;
        private static DirectoryInfo scriptAssembliesDirectory;
        private static Dictionary<string, Type> stringTypeLookup;
        private static Dictionary<Assembly, AssemblyTypeFlags> assemblyTypeFlagLookup;
        private static List<Assembly> allAssemblies;
        private static ImmutableList<Assembly> allAssembliesImmutable;
        private static List<Assembly> userAssemblies;
        private static List<Assembly> userEditorAssemblies;
        private static List<Assembly> pluginAssemblies;
        private static List<Assembly> pluginEditorAssemblies;
        private static List<Assembly> unityAssemblies;
        private static List<Assembly> unityEditorAssemblies;
        private static List<Assembly> otherAssemblies;
        private static List<Type> userTypes;
        private static List<Type> userEditorTypes;
        private static List<Type> pluginTypes;
        private static List<Type> pluginEditorTypes;
        private static List<Type> unityTypes;
        private static List<Type> unityEditorTypes;
        private static List<Type> otherTypes;
        private static string dataPath;
        private static string scriptAssembliesPath;

        private static volatile bool finishedLoading = false;

        /// <summary>
        /// Initializes the <see cref="AssemblyUtilities"/> class.
        /// </summary>
        static AssemblyUtilities()
        {
            LoadThread = null;
            finishedLoading = false;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            bool didStartThreadSuccessfully = false;

            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                var assembly = args.LoadedAssembly;
                var isDynamic = assembly.GetType().FullName.EndsWith("AssemblyBuilder") || assembly.Location == null || assembly.Location == "";

                if (!isDynamic && !args.LoadedAssembly.ReflectionOnly)
                {
                    lock (ASSEMBLY_LOAD_QUEUE_LOCK)
                    {
                        AssemblyLoadQueue.Add(assembly);
                        AssemblyLoadQueueContainsItems = true;
                    }
                }
            };

            ThreadStart loader = () =>
            {
                try
                {
                    Load(assemblies);
                }
                finally
                {
                    finishedLoading = true;
                    //LoadThread = null;
                }
            };

            lock (INITIALIZATION_SETUP_LOCK)
            {
                try
                {
                    LoadThread = new Thread(loader);
                    LoadThread.Name = "Load thread";
                    LoadThread.Start();
                    didStartThreadSuccessfully = true;
                }
                catch (Exception ex)
                {
                    didStartThreadSuccessfully = false;
                    //LoadThread = null;
                    Debug.LogError("STARTING LOAD THREAD EXCEPTIONED OUT: " + ex.ToString());
                    // If this fails, we are probably on WebGL
                }
            }

            // If the threaded version didn't work, then try again in the current thread.
            if (!didStartThreadSuccessfully)
            {
                try
                {
                    Load(assemblies);
                }
                catch (Exception) { }
                finally
                {
                    finishedLoading = true;
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DoNothing()
        {
            // The static constructor has now been invoked!
        }

        private static void EnsureIsFullyInitialized()
        {
            if (Thread.CurrentThread == LoadThread)
            {
                // This should ensure that we can always safely call this from the initializer thread during initialization
                return;
            }

            //Thread loadThreadLocal;

            lock (INITIALIZATION_SETUP_LOCK)
            {
                // This is less of a lock and more of a strong memory barrier
                // that makes sure we're actually getting the property LoadThread
                // field value.
            }

            var local = LoadThread;

            local.Join();

            if (!finishedLoading)
            {
                // This can actually happen! Sometimes, on some hardware/some computers/some runtimes/some versions of Unity,
                // Thread.Join() will do nothing at all, and the joined thread will still be running and we will not have joined
                // it yet. Hence, we're adding this extra bit here, where we "manually join", so to speak.
                // 
                // This fallback saves us in those cases where Thread.Join() is broken.

                //var name = local.Name;
                //var state = local.ThreadState;
                
                //Debug.LogError("EnsureIsFullyInitialized() cruised right past a ThreadJoin without stopping, before loading is done! The joined thread's name was " + (local.Name ?? "NULL") + " and its state was " + state);

                while (!finishedLoading)
                {
                    Thread.Sleep(1);
                }
            }

            while (AssemblyLoadQueueContainsItems)
            {
                List<Assembly> newlyLoadedAssemblies;
                lock (ASSEMBLY_LOAD_QUEUE_LOCK)
                {
                    newlyLoadedAssemblies = AssemblyLoadQueue.ToList();
                    AssemblyLoadQueue.Clear();
                    AssemblyLoadQueueContainsItems = false;
                }

                // AssemblyLoadQueueContainsItems can be set to true while the following is happning:
                for (int i = 0; i < newlyLoadedAssemblies.Count; i++)
                {
                    RegisterAssembly(newlyLoadedAssemblies[i]);
                }
            }
        }

        [Obsolete("Reload is no longer supported.")]
        public static void Reload() { }

        /// <summary>
        /// Re-scans the entire AppDomain.
        /// </summary>
        private static void Load(Assembly[] assemblies)
        {
            dataPath = Environment.CurrentDirectory.Replace("\\", "//").Replace("//", "/").TrimEnd('/') + "/Assets";
            scriptAssembliesPath = Environment.CurrentDirectory.Replace("\\", "//").Replace("//", "/").TrimEnd('/') + "/Library/ScriptAssemblies";
            userAssemblies = new List<Assembly>();
            userEditorAssemblies = new List<Assembly>();
            pluginAssemblies = new List<Assembly>();
            pluginEditorAssemblies = new List<Assembly>();
            unityAssemblies = new List<Assembly>();
            unityEditorAssemblies = new List<Assembly>();
            otherAssemblies = new List<Assembly>();
            userTypes = new List<Type>(100);
            userEditorTypes = new List<Type>(100);
            pluginTypes = new List<Type>(100);
            pluginEditorTypes = new List<Type>(100);
            unityTypes = new List<Type>(100);
            unityEditorTypes = new List<Type>(100);
            otherTypes = new List<Type>(100);
            stringTypeLookup = new Dictionary<string, Type>(100);
            assemblyTypeFlagLookup = new Dictionary<Assembly, AssemblyTypeFlags>(100);
            unityEngineAssembly = typeof(Vector3).Assembly;

#if UNITY_EDITOR
            unityEditorAssembly = typeof(UnityEditor.EditorWindow).Assembly;
#endif

            projectFolderDirectory = new DirectoryInfo(dataPath);
            scriptAssembliesDirectory = new DirectoryInfo(scriptAssembliesPath);

            allAssemblies = new List<Assembly>();
            allAssembliesImmutable = new ImmutableList<Assembly>(allAssemblies);

            for (int i = 0; i < assemblies.Length; i++)
            {
                RegisterAssembly(assemblies[i]);
            }
        }

        private static void RegisterAssembly(Assembly assembly)
        {
            AssemblyTypeFlags assemblyFlag;

            lock (ASSEMBLY_REGISTER_LOCK)
            {
                if (allAssemblies.Contains(assembly)) return;
                allAssemblies.Add(assembly);
            }

            assemblyFlag = GetAssemblyTypeFlag(assembly);

            lock (ASSEMBLY_REGISTER_LOCK)
            {
                Type[] types = assembly.SafeGetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];
                    stringTypeLookup[type.FullName] = type;
                }

                if (assemblyFlag == AssemblyTypeFlags.UserTypes)
                {
                    userAssemblies.Add(assembly);
                    userTypes.AddRange(types);
                }
                else if (assemblyFlag == AssemblyTypeFlags.UserEditorTypes)
                {
                    userEditorAssemblies.Add(assembly);
                    userEditorTypes.AddRange(types);
                }
                else if (assemblyFlag == AssemblyTypeFlags.PluginTypes)
                {
                    pluginAssemblies.Add(assembly);
                    pluginTypes.AddRange(types);
                }
                else if (assemblyFlag == AssemblyTypeFlags.PluginEditorTypes)
                {
                    pluginEditorAssemblies.Add(assembly);
                    pluginEditorTypes.AddRange(types);
                }
                else if (assemblyFlag == AssemblyTypeFlags.UnityTypes)
                {
                    unityAssemblies.Add(assembly);
                    unityTypes.AddRange(types);
                }
                else if (assemblyFlag == AssemblyTypeFlags.UnityEditorTypes)
                {
                    unityEditorAssemblies.Add(assembly);
                    unityEditorTypes.AddRange(types);
                }
                else if (assemblyFlag == AssemblyTypeFlags.OtherTypes)
                {
                    otherAssemblies.Add(assembly);
                    otherTypes.AddRange(types);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="ImmutableList"/> of all assemblies in the current <see cref="System.AppDomain"/>.
        /// </summary>
        /// <returns>An <see cref="ImmutableList"/> of all assemblies in the current <see cref="AppDomain"/>.</returns>
        public static ImmutableList<Assembly> GetAllAssemblies()
        {
            var array = allAssembliesImmutable.ToArray();
            return new ImmutableList<Assembly>(array);
        }

        /// <summary>
        /// Gets the <see cref="AssemblyTypeFlags"/> for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The <see cref="AssemblyTypeFlags"/> for a given assembly.</returns>
        /// <exception cref="System.NullReferenceException"><paramref name="assembly"/> is null.</exception>
        public static AssemblyTypeFlags GetAssemblyTypeFlag(this Assembly assembly)
        {
            if (assembly == null) throw new NullReferenceException("assembly");

            EnsureIsFullyInitialized();

            lock (ASSEMBLY_TYPE_FLAG_LOOKUP_LOCK)
            {
                AssemblyTypeFlags result;

                if (assemblyTypeFlagLookup.TryGetValue(assembly, out result) == false)
                {
                    result = GetAssemblyTypeFlagNoLookup(assembly);

                    assemblyTypeFlagLookup[assembly] = result;
                }

                return result;
            }
        }

        private static AssemblyTypeFlags GetAssemblyTypeFlagNoLookup(Assembly assembly)
        {
            AssemblyTypeFlags result;
            string path = assembly.GetAssemblyDirectory();
            string name = assembly.FullName.ToLower(CultureInfo.InvariantCulture);

            bool isInScriptAssemblies = false;
            bool isInProject = false;

            if (path != null && Directory.Exists(path))
            {
                var pathDir = new DirectoryInfo(path);

                isInScriptAssemblies = pathDir.FullName == scriptAssembliesDirectory.FullName;
                isInProject = projectFolderDirectory.HasSubDirectory(pathDir);
            }

            bool isUserScriptAssembly = name.StartsWithAnyOf(userAssemblyPrefixes, StringComparison.InvariantCultureIgnoreCase);
            bool isPluginScriptAssembly = name.StartsWithAnyOf(pluginAssemblyPrefixes, StringComparison.InvariantCultureIgnoreCase);
            bool isGame = assembly.IsDependentOn(unityEngineAssembly);
            bool isPlugin = isPluginScriptAssembly || isInProject || (!isUserScriptAssembly && isInScriptAssemblies);

            // HACK: Odin and other assemblies, but easpecially Odin, needs to be registered as a plugin if it's installed as a package from the Unity PackageManager.
            // However there doesn't seemt to be any good way of figuring that out.

            // TODO: Find a good way of figuring if it's a plugin when located installed as a package.
            // Maybe it would be easier to figure out whether something was a Unity type, and then have plugin as fallback, instead of ther other way around, which
            // is how it works now.
            if (!isPlugin && name.StartsWith("sirenix."))
            {
                isPlugin = true;
            }

            bool isUser = !isPlugin && isUserScriptAssembly;

#if UNITY_EDITOR
            bool isEditor = isUser ? name.Contains("-editor") : assembly.IsDependentOn(unityEditorAssembly);

            if (isUser)
            {
                isEditor = name.Contains("-editor");
            }
            else
            {
                isEditor = assembly.IsDependentOn(unityEditorAssembly);
            }
#else
                bool isEditor = false;
#endif
            if (!isGame && !isEditor && !isPlugin && !isUser)
            {
                result = AssemblyTypeFlags.OtherTypes;
            }
            else if (isEditor && !isPlugin && !isUser)
            {
                result = AssemblyTypeFlags.UnityEditorTypes;
            }
            else if (isGame && !isEditor && !isPlugin && !isUser)
            {
                result = AssemblyTypeFlags.UnityTypes;
            }
            else if (isEditor && isPlugin && !isUser)
            {
                result = AssemblyTypeFlags.PluginEditorTypes;
            }
            else if (!isEditor && isPlugin && !isUser)
            {
                result = AssemblyTypeFlags.PluginTypes;
            }
            else if (isEditor && isUser)
            {
                result = AssemblyTypeFlags.UserEditorTypes;
            }
            else if (!isEditor && isUser)
            {
                result = AssemblyTypeFlags.UserTypes;
            }
            else
            {
                result = AssemblyTypeFlags.OtherTypes;
            }

            return result;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="fullName">The full name of the type without any assembly information.</param>
        public static Type GetTypeByCachedFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return null;
            }

            EnsureIsFullyInitialized();

            Type type;
            if (stringTypeLookup.TryGetValue(fullName, out type))
            {
                return type;
            }

            return null;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [Obsolete("This method was renamed. Use GetTypeByCachedFullName instead.")]
        public static Type GetType(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return null;
            }

            EnsureIsFullyInitialized();

            Type type;
            if (stringTypeLookup.TryGetValue(fullName, out type))
            {
                return type;
            }

            return null;
        }

        /// <summary>
        /// Determines whether an assembly is depended on another assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="otherAssembly">The other assembly.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="assembly"/> has a reference in <paramref name="otherAssembly"/> or <paramref name="assembly"/> is the same as <paramref name="otherAssembly"/>.
        /// </returns>
        /// <exception cref="System.NullReferenceException"><paramref name="assembly"/> is null.</exception>
        /// <exception cref="System.NullReferenceException"><paramref name="otherAssembly"/> is null.</exception>
        public static bool IsDependentOn(this Assembly assembly, Assembly otherAssembly)
        {
            if (assembly == null) throw new NullReferenceException("assembly");
            if (otherAssembly == null) throw new NullReferenceException("otherAssembly");

            if (assembly == otherAssembly)
            {
                return true;
            }

            var otherName = otherAssembly.GetName().ToString();
            var referencedAsssemblies = assembly.GetReferencedAssemblies();

            for (int i = 0; i < referencedAsssemblies.Length; i++)
            {
                if (otherName == referencedAsssemblies[i].ToString())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the assembly module is a of type <see cref="System.Reflection.Emit.ModuleBuilder"/>.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        ///   <c>true</c> if the specified assembly of type <see cref="System.Reflection.Emit.ModuleBuilder"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">assembly</exception>
        public static bool IsDynamic(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            bool result;

            lock (IS_DYNAMIC_CACHE_LOCK)
            {
                if (!IsDynamicCache.TryGetValue(assembly, out result))
                {
                    try
                    {
                        // Will cover both System.Reflection.Emit.AssemblyBuilder and System.Reflection.Emit.InternalAssemblyBuilder
                        result = assembly.GetType().FullName.EndsWith("AssemblyBuilder") || assembly.Location == null || assembly.Location == "";
                    }
                    catch
                    {
                        result = true;
                    }

                    IsDynamicCache.Add(assembly, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the full file path to a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The full file path to a given assembly, or <c>Null</c> if no file path was found.</returns>
        /// <exception cref="System.NullReferenceException"><paramref name="assembly"/> is Null.</exception>
        public static string GetAssemblyDirectory(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            var path = assembly.GetAssemblyFilePath();
            if (path == null)
            {
                return null;
            }

            try
            {
                return Path.GetDirectoryName(path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the full directory path to a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The full directory path in which a given assembly is located, or <c>Null</c> if no file path was found.</returns>
        public static string GetAssemblyFilePath(this Assembly assembly)
        {
            if (assembly == null) return null;
            if (assembly.IsDynamic()) return null;
            if (assembly.CodeBase == null) return null;

            var filePrefix = @"file:///";
            var path = assembly.CodeBase;

            if (path.StartsWith(filePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(filePrefix.Length);
                path = path.Replace('\\', '/');

                if (File.Exists(path))
                {
                    return path;
                }

                if (!Path.IsPathRooted(path))
                {
                    if (File.Exists("/" + path))
                    {
                        path = "/" + path;
                    }
                    else
                    {
                        path = Path.GetFullPath(path);
                    }
                }

                if (!File.Exists(path))
                {
                    try
                    {
                        path = assembly.Location;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    return path;
                }

                if (File.Exists(path))
                {
                    return path;
                }
            }

            if (File.Exists(assembly.Location))
            {
                return assembly.Location;
            }

            return null;
        }

        /// <summary>
        /// Get types from the current AppDomain with a specified <see cref="AssemblyTypeFlags"/> filter.
        /// </summary>
        /// <param name="assemblyTypeFlags">The <see cref="AssemblyTypeFlags"/> filters.</param>
        /// <returns>Types from the current AppDomain with the specified <see cref="AssemblyTypeFlags"/> filters.</returns>
        public static IEnumerable<Type> GetTypes(AssemblyTypeFlags assemblyTypeFlags)
        {
            EnsureIsFullyInitialized();

            bool includeUserTypes = (assemblyTypeFlags & AssemblyTypeFlags.UserTypes) == AssemblyTypeFlags.UserTypes;
            bool includeUserEditorTypes = (assemblyTypeFlags & AssemblyTypeFlags.UserEditorTypes) == AssemblyTypeFlags.UserEditorTypes;
            bool includePluginTypes = (assemblyTypeFlags & AssemblyTypeFlags.PluginTypes) == AssemblyTypeFlags.PluginTypes;
            bool includePluginEditorTypes = (assemblyTypeFlags & AssemblyTypeFlags.PluginEditorTypes) == AssemblyTypeFlags.PluginEditorTypes;
            bool includeUnityTypes = (assemblyTypeFlags & AssemblyTypeFlags.UnityTypes) == AssemblyTypeFlags.UnityTypes;
            bool includeUnityEditorTypes = (assemblyTypeFlags & AssemblyTypeFlags.UnityEditorTypes) == AssemblyTypeFlags.UnityEditorTypes;
            bool includeOtherTypes = (assemblyTypeFlags & AssemblyTypeFlags.OtherTypes) == AssemblyTypeFlags.OtherTypes;

            if (includeUserTypes) for (int i = 0; i < userTypes.Count; i++) yield return userTypes[i];
            if (includeUserEditorTypes) for (int i = 0; i < userEditorTypes.Count; i++) yield return userEditorTypes[i];
            if (includePluginTypes) for (int i = 0; i < pluginTypes.Count; i++) yield return pluginTypes[i];
            if (includePluginEditorTypes) for (int i = 0; i < pluginEditorTypes.Count; i++) yield return pluginEditorTypes[i];
            if (includeUnityTypes) for (int i = 0; i < unityTypes.Count; i++) yield return unityTypes[i];
            if (includeUnityEditorTypes) for (int i = 0; i < unityEditorTypes.Count; i++) yield return unityEditorTypes[i];
            if (includeOtherTypes) for (int i = 0; i < otherTypes.Count; i++) yield return otherTypes[i];

        }
        
        private static bool StartsWithAnyOf(this string str, IEnumerable<string> values, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            var iList = values as IList<string>;

            if (iList != null)
            {
                int count = iList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (str.StartsWith(iList[i], comparisonType))
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var value in values)
                {
                    if (str.StartsWith(value, comparisonType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
#endif