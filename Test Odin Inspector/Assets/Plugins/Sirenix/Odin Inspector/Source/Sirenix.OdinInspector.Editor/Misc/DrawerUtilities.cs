#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.OdinInspector.Editor.TypeSearch;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;

    public static class DrawerUtilities
    {
        private static class Null { }

        public static readonly TypeSearchIndex SearchIndex = new TypeSearchIndex() { MatchedTypeLogName = "drawer" };

        private static List<DrawerAndPriority> AllDrawerTypes;

        private static readonly FieldInfo CustomPropertyDrawerTypeField = typeof(CustomPropertyDrawer).GetField("m_Type", Flags.InstanceAnyVisibility);
        private static readonly FieldInfo CustomPropertyDrawerUseForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", Flags.InstanceAnyVisibility);
        private static readonly bool SupportsUnityDrawers = DrawerUtilities.CustomPropertyDrawerTypeField != null && DrawerUtilities.CustomPropertyDrawerUseForChildrenField != null;

        private static readonly Dictionary<Type, DrawerPriority> DrawerTypePriorityLookup = new Dictionary<Type, DrawerPriority>(FastTypeComparer.Instance);
        private static readonly Dictionary<Type, OdinDrawer> UninitializedDrawers = new Dictionary<Type, OdinDrawer>(FastTypeComparer.Instance);

        private static TypeSearchResult[][] CachedQueryResultArray = new TypeSearchResult[16][];

        private static readonly Type AbstractTypeUnityPropertyDrawer_TArg2 = typeof(AbstractTypeUnityPropertyDrawer<,,>).GetGenericArguments()[2];
        private static readonly Type UnityPropertyAttributeDrawer_TArg1 = typeof(UnityPropertyAttributeDrawer<,,>).GetGenericArguments()[1];
        private static readonly Type UnityDecoratorAttributeDrawer_TArg1 = typeof(UnityDecoratorAttributeDrawer<,,>).GetGenericArguments()[1];

        private static readonly TypeMatchRule InvalidAttributeRule = new TypeMatchRule(
            "Invalid Attribute Notification Dummy Rule (This is never matched against, but only serves to be a result rule for invalid attribute type search results)",
            (info, target) => null);

        /// <summary>
        /// Odin has its own implementations for these attribute drawers; never use Unity's.
        /// </summary>
        private static readonly HashSet<string> ExcludeUnityDrawers = new HashSet<string>()
        {
            "HeaderDrawer",
            "DelayedDrawer",
            "MultilineDrawer",
            "RangeDrawer",
            "SpaceDrawer",
            "TextAreaDrawer",
            "ColorUsageDrawer"
        };

        private struct DrawerAndPriority
        {
            public Type Drawer;
            public DrawerPriority Priority;
            public string Name;
        }

        static DrawerUtilities()
        {
            // This method should be a lot faster than it used to be, now.
            // It has been refactored and all Linq removed.

            using (SimpleProfiler.Section("DrawerUtilities"))
            {
                if (!SupportsUnityDrawers)
                {
                    Debug.LogWarning("Could not find internal fields 'm_Type' and/or 'm_UseForChildren' in type CustomPropertyDrawer in this version of Unity; support for legacy Unity PropertyDrawers and DecoratorDrawers have been disabled in Odin's inspector. Please report this on Odin's issue tracker.");
                }

                // 
                // Find and process all drawer types, adding them in a sorted order to the AllDrawerTypes list
                // 

                if (UnityTypeCacheUtility.IsAvailable)
                {
                    //using (SimpleProfiler.Profile("DrawerUtilities type query with TypeCache"))
                    {
                        IList<Type> odinTypes = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(OdinDrawer));
                        IList<Type> unityTypes = SupportsUnityDrawers ? UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(GUIDrawer)) : null;

                        int allDrawerCountGuess = odinTypes.Count;

                        if (unityTypes != null)
                        {
                            allDrawerCountGuess += unityTypes.Count;

                            // Some Unity types declare multiple "variants" - we want to have some space for them so we hopefully still don't have to expand the list
                            allDrawerCountGuess += 50;
                        }

                        AllDrawerTypes = new List<DrawerAndPriority>(allDrawerCountGuess);

                        foreach (var type in odinTypes)
                        {
                            if (type.IsAbstract) continue;
                            ProcessDrawerType(type, true, false);
                        }

                        if (SupportsUnityDrawers)
                        {
                            foreach (var type in unityTypes)
                            {
                                if (type.IsAbstract) continue;
                                ProcessDrawerType(type, false, true);
                            }
                        }
                    }
                }
                else
                {
                    //using (SimpleProfiler.Section("DrawerUtilities type query with Reflection"))
                    {
                        AllDrawerTypes = new List<DrawerAndPriority>(200);
                        List<Type> types = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes | AssemblyTypeFlags.UnityEditorTypes).ToList();

                        for (int i = 0; i < types.Count; i++)
                        {
                            var type = types[i];

                            if (type.IsAbstract || !type.IsClass) continue;

                            bool isOdin = typeof(OdinDrawer).IsAssignableFrom(type);
                            bool isUnity = !isOdin && typeof(GUIDrawer).IsAssignableFrom(type);

                            ProcessDrawerType(type, isOdin, isUnity);
                        }
                    }
                }

                // Unity drawers have a peculiar method of generic target selection,
                // where you pass in the generic type definition that you wish to draw.
                //
                // We need to support this, for Unity's own legacy drawers, so we add
                // a custom rule that implements it.
                DrawerUtilities.SearchIndex.MatchRules.Add(new TypeSearch.TypeMatchRule(
                    "Unity Drawer Generic Target Matcher",
                    (info, targets) =>
                    {
                        if (targets.Length != 1) return null;
                        if (!info.Targets[0].IsGenericTypeDefinition) return null;

                        var baseDef = info.MatchType.GetGenericTypeDefinition();

                        bool abstractUnityValueDrawer = baseDef == typeof(AbstractTypeUnityPropertyDrawer<,,>);
                        bool plainUnityValueDrawer = baseDef == typeof(UnityPropertyDrawer<,>);

                        if (!(abstractUnityValueDrawer || plainUnityValueDrawer)) return null;

                        if (abstractUnityValueDrawer)
                        {
                            if (targets[0].ImplementsOpenGenericType(info.Targets[0]))
                            {
                                var args = info.MatchType.GetGenericArguments();
                                return info.MatchType.GetGenericTypeDefinition().MakeGenericType(args[0], targets[0], targets[0]);
                            }
                        }
                        else
                        {
                            if (!targets[0].IsGenericType) return null;

                            if (targets[0].GetGenericTypeDefinition() == info.Targets[0])
                            {
                                var args = info.MatchType.GetGenericArguments();
                                return info.MatchType.GetGenericTypeDefinition().MakeGenericType(args[0], targets[0]);
                            }
                        }

                        return null;
                    })
                );

                // 
                // Now we add all found drawers to the TypeSearchIndex
                // 

                for (int i = 0; i < DrawerUtilities.AllDrawerTypes.Count; i++)
                {
                    var type = DrawerUtilities.AllDrawerTypes[i].Drawer;

                    var info = new TypeSearchInfo()
                    {
                        MatchType = type,
                        Priority = DrawerUtilities.AllDrawerTypes.Count - i,
                        Targets = null
                    };

                    Type[] args;

                    if ((args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinValueDrawer<>))) != null)
                    {
                        info.Targets = args;
                    }
                    else if (type.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<>)))
                    {
                        if ((args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<,>))) != null)
                        {
                            info.Targets = args;
                            InvalidAttributeTargetUtility.RegisterValidAttributeTarget(info.Targets[0], info.Targets[1]);
                        }
                        else
                        {
                            info.Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<>));
                        }
                    }
                    else if ((args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinGroupDrawer<>))) != null)
                    {
                        info.Targets = args;
                    }
                    else if (!type.IsFullyConstructedGenericType())
                    {
                        info.Targets = type.GetGenericArguments();
                    }

                    info.Targets = info.Targets ?? Type.EmptyTypes;

                    // We've already sorted the drawers in the correct order, so the index doesn't 
                    //  need to insert them in sorted order, that's just a waste of work.
                    SearchIndex.AddIndexedTypeUnsorted(info);
                }

                //
                // Invoke static constructors of any classes that need it before drawing starts
                // 
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var attr in assembly.SafeGetCustomAttributes(typeof(StaticInitializeBeforeDrawingAttribute), false))
                    {
                        var castAttr = attr as StaticInitializeBeforeDrawingAttribute;
                        if (castAttr.Types == null) continue;

                        foreach (var type in castAttr.Types)
                        {
                            if (type == null) continue;
                            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                        }
                    }
                }

                // 
                // Do async TypeSearchIndex warmup based on types matched in previous domain load
                //
                if (GeneralDrawerConfig.Instance.PrecomputeTypeMatching)
                {
                    new Thread(WarmUpTypeSearchIndexIfCacheExists).Start();
                }

                AppDomain.CurrentDomain.DomainUnload += SaveSearchIndexCache;
            }
        }

        private static readonly string MatchCachePath = "Temp/Odin_TypeSearchIndex_MatchCache_Drawers.txt";
        //private static readonly string MergeCachePath = "Temp/Odin_TypeSearchIndex_MergeCache_Drawers.txt";

        private static void SaveSearchIndexCache(object sender, EventArgs e)
        {
            if (!GeneralDrawerConfig.HasInstanceLoaded || !GeneralDrawerConfig.Instance.PrecomputeTypeMatching) return;

            try
            {
                var binder = TwoWaySerializationBinder.Default;

                {
                    //var watch = System.Diagnostics.Stopwatch.StartNew();
                    List<Type[]> matches = SearchIndex.GetAllCachedTargets();
                    //Debug.Log("Writing search index match cache for '" + matches.Count + "' matches...");

                    using (var stream = new FileStream(MatchCachePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(stream))
                    {
                        foreach (var targets in matches)
                        {
                            for (int i = 0; i < targets.Length; i++)
                            {
                                if (i != 0) writer.Write("|");
                                writer.Write(binder.BindToName(targets[i]));
                            }

                            writer.WriteLine();
                        }
                    }

                    //watch.Stop();
                    //Debug.Log("Finished writing search index match cache for '" + matches.Count + "' matches in " + watch.Elapsed.TotalMilliseconds + " ms...");
                }

                //{
                //    Debug.Log("Getting merge signatures for writing...");
                //    var watch = System.Diagnostics.Stopwatch.StartNew();
                //    var mergeSignatures = TypeSearchIndex.GetAllCachedMergeSignatures(SearchIndex);
                //    Debug.Log("Writing search index merge cache for '" + mergeSignatures.Count + "' merge signatures...");

                //    using (var stream = new FileStream(MergeCachePath, FileMode.Create, FileAccess.Write, FileShare.None))
                //    using (var writer = new StreamWriter(stream))
                //    {
                //        for (int i1 = 0; i1 < mergeSignatures.Count; i1++)
                //        {
                //            List<Type[]> mergeSignature = mergeSignatures[i1];

                //            for (int i2 = 0; i2 < mergeSignature.Count; i2++)
                //            {
                //                Type[] targetSet = mergeSignature[i2];
                //                if (i2 != 0) writer.Write("§");

                //                for (int i3 = 0; i3 < targetSet.Length; i3++)
                //                {
                //                    if (i3 != 0) writer.Write("|");
                //                    writer.Write(binder.BindToName(targetSet[i3]));
                //                }
                //            }

                //            writer.WriteLine();
                //        }
                //    }

                //    watch.Stop();
                //    Debug.Log("Finished writing search index merge cache for '" + mergeSignatures.Count + "' merge signatures in " + watch.Elapsed.TotalMilliseconds + " ms...");
                //}
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void WarmUpTypeSearchIndexIfCacheExists()
        {
            try
            {
                var binder = TwoWaySerializationBinder.Default;
            
                if (File.Exists(MatchCachePath))
                {
                    //Debug.Log("Loading search index match cache data...");

                    //var watch = System.Diagnostics.Stopwatch.StartNew();
                    var lines = File.ReadAllLines(MatchCachePath);

                    //Debug.Log("Warming up search index matches for '" + lines.Length + "' matches...");

                    List<Type> types = new List<Type>();
                    //int actuallyDidItCount = 0;

                    foreach (var line in lines)
                    {
                        var typeNames = line.Split('|');
                        types.Clear();

                        bool validLine = true;

                        foreach (var typeName in typeNames)
                        {
                            var type = binder.BindToType(typeName);

                            if (type == null)
                            {
                                validLine = false;
                                break;
                            }

                            types.Add(type);
                        }

                        if (!validLine || types.Count == 0) continue;
                        else if (types.Count == 1) SearchIndex.GetMatches(types[0]);
                        else if (types.Count == 2) SearchIndex.GetMatches(types[0], types[1]);
                        else SearchIndex.GetMatches(types.ToArray());
                        //actuallyDidItCount++;
                    }

                    //watch.Stop();
                    //Debug.Log("Finished warming up " + actuallyDidItCount + " search index matches in " + watch.Elapsed.TotalMilliseconds + " ms");
                }

                //if (File.Exists(MergeCachePath))
                //{
                //    Debug.Log("Loading search index merge cache data...");

                //    var watch = System.Diagnostics.Stopwatch.StartNew();
                //    var lines = File.ReadAllLines(MergeCachePath);

                //    Debug.Log("Warming up search index merges for '" + lines.Length + "' merge signatures...");
                //    List<Type> types = new List<Type>();
                //    List<TypeSearchResult[]> results = new List<TypeSearchResult[]>();

                //    int actuallyDidItCount = 0;

                //    foreach (var mergeSignature in lines)
                //    {
                //        results.Clear();

                //        foreach (var targetSet in mergeSignature.Split('§'))
                //        {
                //            if (string.IsNullOrEmpty(targetSet))
                //            {
                //                results.Add(SearchIndex.GetMatches(Type.EmptyTypes));
                //                continue;
                //            }

                //            types.Clear();

                //            foreach (var target in targetSet.Split('|'))
                //            {
                //                var type = binder.BindToType(target);

                //                if (type == null)
                //                {
                //                    goto NEXT_MERGE_SIGNATURE;
                //                }

                //                types.Add(type);
                //            }

                //            if (types.Count == 0) results.Add(SearchIndex.GetMatches(Type.EmptyTypes));
                //            else if (types.Count == 1) results.Add(SearchIndex.GetMatches(types[0]));
                //            else if (types.Count == 1) results.Add(SearchIndex.GetMatches(types[0], types[1]));
                //            else results.Add(SearchIndex.GetMatches(types.ToArray()));
                //        }

                //        TypeSearchIndex.GetCachedMergedQueryResults(results);
                //        actuallyDidItCount++;
                    
                //    NEXT_MERGE_SIGNATURE:;
                //    }

                //    watch.Stop();
                //    Debug.Log("Finished warming up " + actuallyDidItCount + " search index merges in " + watch.Elapsed.TotalMilliseconds + " ms");
                //}
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static bool FastStartsWith(string str, string startsWith)
        {
            if (startsWith.Length > str.Length) return false;

            for (int i = 0; i < startsWith.Length; i++)
            {
                if (str[i] != startsWith[i]) return false;
            }

            return true;
        }

        private static void InsertSortedIntoAllDrawerTypes(Type drawer)
        {
            var name = drawer.Name;
            var priority = GetDrawerPriority(drawer);

            // Binary search for insertion point as the list is already sorted

            int left = 0, right = AllDrawerTypes.Count - 1;

            int current = 0;
            int compare = 0;

            while (left <= right)
            {
                current = (left + right) / 2;

                var middleDrawer = AllDrawerTypes[current];

                compare = priority.CompareTo(middleDrawer.Priority);

                if (compare == 0)
                {
                    compare = -name.CompareTo(middleDrawer.Name);
                }

                if (compare < 0)
                {
                    left = current + 1;
                }
                else if (compare > 0)
                {
                    right = current - 1;
                }
                else break;
            }

            if (compare == 0)
            {
                var count = AllDrawerTypes.Count;

                while (current + 1 < count)
                {
                    var next = AllDrawerTypes[current + 1];

                    if (priority > next.Priority)
                    {
                        current++;
                        break;
                    }

                    current++;
                }
            }
            else if (compare < 0)
            {
                current++;
            }

            AllDrawerTypes.Insert(current, new DrawerAndPriority()
            {
                Drawer = drawer,
                Priority = priority,
                Name = name,
            });
        }

        private static void ProcessDrawerType(Type type, bool isOdin, bool isUnity)
        {
            if (!isOdin && !isUnity) return;
            if (isUnity && !SupportsUnityDrawers) return;

            if (type.IsDefined(typeof(OdinDontRegisterAttribute), false)) return;

            var ns = type.Namespace;

            if (ns != null && FastStartsWith(ns, "Unity") && ExcludeUnityDrawers.Contains(type.Name))
                return;

            if (isOdin)
            {
                InsertSortedIntoAllDrawerTypes(type);
            }
            else
            {
                // It's a Unity legacy drawer that we must creater a wrapper Odin drawer type for

                if (type.IsGenericTypeDefinition) return;
                if (type.GetConstructor(Type.EmptyTypes) == null) return;

                bool isPropertyDrawer = typeof(PropertyDrawer).IsAssignableFrom(type);
                bool isDecoratorDrawer = !isPropertyDrawer && typeof(DecoratorDrawer).IsAssignableFrom(type);

                if (!isPropertyDrawer && !isDecoratorDrawer) return;

                object[] customPropertyDrawerAttributes = type.GetCustomAttributes(typeof(CustomPropertyDrawer), false);

                for (int j = 0; j < customPropertyDrawerAttributes.Length; j++)
                {
                    object attribute = customPropertyDrawerAttributes[j];

                    Type drawnType = CustomPropertyDrawerTypeField.GetValue(attribute) as Type;

                    if (drawnType == null) continue;

                    bool isPropertyAttribute = typeof(PropertyAttribute).IsAssignableFrom(drawnType);

                    if (isDecoratorDrawer && !isPropertyAttribute) continue;

                    bool useForChildren = (bool)CustomPropertyDrawerUseForChildrenField.GetValue(attribute);

                    Type wrapper;

                    if (isPropertyDrawer)
                    {
                        if (isPropertyAttribute)
                        {
                            if (useForChildren || drawnType.IsAbstract)
                            {
                                wrapper = typeof(UnityPropertyAttributeDrawer<,,>).MakeGenericType(type, UnityPropertyAttributeDrawer_TArg1, drawnType);
                            }
                            else
                            {
                                wrapper = typeof(UnityPropertyAttributeDrawer<,,>).MakeGenericType(type, drawnType, typeof(PropertyAttribute));
                            }
                        }
                        else
                        {
                            if (useForChildren || drawnType.IsAbstract)
                            {
                                wrapper = typeof(AbstractTypeUnityPropertyDrawer<,,>).MakeGenericType(type, drawnType, AbstractTypeUnityPropertyDrawer_TArg2);
                            }
                            else
                            {
                                wrapper = typeof(UnityPropertyDrawer<,>).MakeGenericType(type, drawnType);
                            }
                        }
                    }
                    else
                    {
                        if (useForChildren || drawnType.IsAbstract)
                        {
                            wrapper = typeof(UnityDecoratorAttributeDrawer<,,>).MakeGenericType(type, UnityDecoratorAttributeDrawer_TArg1, drawnType);
                        }
                        else
                        {
                            wrapper = typeof(UnityDecoratorAttributeDrawer<,,>).MakeGenericType(type, drawnType, typeof(PropertyAttribute));
                        }
                    }

                    InsertSortedIntoAllDrawerTypes(wrapper);
                }
            }
        }

        public static void GetDefaultPropertyDrawers(InspectorProperty property, ref TypeSearchResult[] resultArray, ref int resultCount)
        {
            resultCount = 0;
            int queryCount = 0;

            // First we make and gather lots of small queries, which are essentially instant, as they are
            //  all cached by the search index.
            {
                // Empty query (for all drawers with no type constraints at all)
                CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(Type.EmptyTypes);

                // Value query
                if (property.ValueEntry != null)
                {
                    CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(property.ValueEntry.TypeOfValue);
                }

                int maxNeededSize = 2 + property.Attributes.Count * 3;

                while (CachedQueryResultArray.Length <= maxNeededSize) ExpandArray(ref CachedQueryResultArray);

                // Attribute queries
                for (int i = 0; i < property.Attributes.Count; i++)
                {
                    var attr = property.Attributes[i].GetType();
                    CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr);

                    // Attribute and value query
                    if (property.ValueEntry != null)
                    {
                        CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr, property.ValueEntry.TypeOfValue);

                        if (InvalidAttributeTargetUtility.ShowInvalidAttributeErrorFor(property, attr))
                        {
                            CachedQueryResultArray[queryCount++] = GetInvalidAttributeTypeSearchResult(attr);
                        }
                    }
                }
            }

            var finalResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedQueryResultArray, queryCount);

            // Build up the final result list, filtering invalid drawer types away
            //  as we go.
            for (int i = 0; i < finalResults.Length; i++)
            {
                var result = finalResults[i];

                if (DrawerTypeCanDrawProperty(result.MatchedType, property))
                {
                    if (resultCount == resultArray.Length) ExpandArray(ref resultArray);
                    resultArray[resultCount++] = finalResults[i];
                }
            }
        }

        private static void ExpandArray<T>(ref T[] array)
        {
            var newArray = new T[array.Length * 2];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
            array = newArray;
        }

        private static readonly Dictionary<Type, TypeSearchResult[]> InvalidAttributeTypeSearchResults = new Dictionary<Type, TypeSearchResult[]>(FastTypeComparer.Instance);

        private static TypeSearchResult[] GetInvalidAttributeTypeSearchResult(Type attr)
        {
            TypeSearchResult[] result;

            if (!InvalidAttributeTypeSearchResults.TryGetValue(attr, out result))
            {
                result = new TypeSearchResult[]
                {
                    new TypeSearchResult()
                    {
                        MatchedInfo = new TypeSearchInfo()
                        {
                            MatchType = typeof(InvalidAttributeNotificationDrawer<>),
                            Priority = double.MaxValue,
                            Targets = Type.EmptyTypes
                        },
                        MatchedRule = InvalidAttributeRule,
                        MatchedTargets = Type.EmptyTypes,
                        MatchedType = typeof(InvalidAttributeNotificationDrawer<>).MakeGenericType(attr)
                    }
                };

                InvalidAttributeTypeSearchResults.Add(attr, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the priority of a given drawer type.
        /// </summary>
        public static DrawerPriority GetDrawerPriority(Type drawerType)
        {
            DrawerPriority result;

            if (!DrawerTypePriorityLookup.TryGetValue(drawerType, out result))
            {
                result = CalculateDrawerPriority(drawerType);
                DrawerTypePriorityLookup[drawerType] = result;
            }

            return result;
        }

        private static DrawerPriority CalculateDrawerPriority(Type drawerType)
        {
            DrawerPriority priority = DrawerPriority.AutoPriority;

            DrawerPriority adjustment = default(DrawerPriority);

            // Find a DrawerPriorityAttribute if there is one anywhere and use the priority from that
            {
                DrawerPriorityAttribute priorityAttribute = null;

                if (DrawerIsUnityAlias(drawerType))
                {
                    // Special case for Unity property alias drawers; 
                    // We should check if their assigned Unity drawer type
                    // itself declares a DrawerPriorityAttribute.

                    var innerDrawer = drawerType.GetGenericArguments()[0];

                    if (innerDrawer.IsDefined(typeof(DrawerPriorityAttribute), false))
                    {
                        priorityAttribute = innerDrawer.GetCustomAttribute<DrawerPriorityAttribute>(false);
                    }

                    if (priorityAttribute == null)
                    {
                        var flag = innerDrawer.Assembly.GetAssemblyTypeFlag();

                        bool isUnityDrawer = (flag & (AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes)) != 0;

                        // Unity-declared legacy property drawers have a priority penalty
                        //  compared to user-declared legacy property drawers.
                        if (isUnityDrawer)
                        {
                            adjustment.Value -= 0.1;
                        }
                    }
                }

                if (priorityAttribute == null && drawerType.IsDefined(typeof(DrawerPriorityAttribute), false))
                {
                    priorityAttribute = drawerType.GetCustomAttribute<DrawerPriorityAttribute>(false);
                }

                if (priorityAttribute != null)
                {
                    priority = priorityAttribute.Priority;
                }
            }

            // Figure out the drawer's actual priority if it's auto priority
            if (priority == DrawerPriority.AutoPriority)
            {
                if (drawerType.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<>)))
                {
                    priority = DrawerPriority.AttributePriority;
                }
                else
                {
                    priority = DrawerPriority.ValuePriority;
                }

                // All Odin drawers without explicit priorities default to a slightly lower priority, so
                // that user-defined default-priority drawers always override default-priority Odin drawers.
                if (drawerType.Assembly == typeof(OdinEditor).Assembly)
                {
                    priority.Value -= 0.001;
                }
            }

            priority += adjustment;

            return priority;
        }

        private static bool DrawerIsUnityAlias(Type drawerType)
        {
            if (!drawerType.IsGenericType || drawerType.IsGenericTypeDefinition)
                return false;

            var definition = drawerType.GetGenericTypeDefinition();

            return definition == typeof(UnityPropertyDrawer<,>)
                || definition == typeof(UnityPropertyAttributeDrawer<,,>)
                || definition == typeof(UnityDecoratorAttributeDrawer<,,>)
                || definition == typeof(AbstractTypeUnityPropertyDrawer<,,>);
        }

        public static bool DrawerTypeCanDrawProperty(Type drawerType, InspectorProperty property)
        {
            var drawer = GetCachedUninitializedDrawer(drawerType);
            return drawer.CanDrawProperty(property);
        }

        public static OdinDrawer GetCachedUninitializedDrawer(Type drawerType)
        {
            OdinDrawer result;
            if (!UninitializedDrawers.TryGetValue(drawerType, out result))
            {
                result = (OdinDrawer)FormatterServices.GetUninitializedObject(drawerType);
                UninitializedDrawers[drawerType] = result;
            }
            return result;
        }

        public static bool HasAttributeDrawer(Type attributeType)
        {
            return AllDrawerTypes
                .Select(d => d.Drawer.GetBaseClasses(false)
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(OdinAttributeDrawer<>)))
                .Where(d => d != null)
                .Any(d => d.GetGenericArguments()[0] == attributeType);
        }

        public static class InvalidAttributeTargetUtility
        {
            private static readonly Dictionary<Type, List<Type>> ConcreteAttributeTargets = new Dictionary<Type, List<Type>>(FastTypeComparer.Instance);
            private static readonly Dictionary<Type, List<Type>> GenericParameterAttributeTargets = new Dictionary<Type, List<Type>>(FastTypeComparer.Instance);

            // Attribute, value, result
            private static readonly DoubleLookupDictionary<Type, Type, bool> ShowErrorCache = new DoubleLookupDictionary<Type, Type, bool>(FastTypeComparer.Instance, FastTypeComparer.Instance);

            private static readonly List<Type> EmptyList = new List<Type>();

            public static void RegisterValidAttributeTarget(Type attribute, Type target)
            {
                List<Type> list;

                if (attribute.IsGenericParameter)
                {
                    if (!GenericParameterAttributeTargets.TryGetValue(attribute, out list))
                    {
                        list = new List<Type>();
                        GenericParameterAttributeTargets[attribute] = list;
                    }
                }
                else
                {
                    if (!ConcreteAttributeTargets.TryGetValue(attribute, out list))
                    {
                        list = new List<Type>();
                        ConcreteAttributeTargets[attribute] = list;
                    }
                }

                list.Add(target);
            }

            public static List<Type> GetValidTargets(Type attribute)
            {
                List<Type> result;
                if (!ConcreteAttributeTargets.TryGetValue(attribute, out result) && !GenericParameterAttributeTargets.TryGetValue(attribute, out result))
                {
                    bool foundAnyValids = false;

                    foreach (var entry in GenericParameterAttributeTargets)
                    {
                        var param = entry.Key;
                        var targets = entry.Value;

                        if (param.GenericParameterIsFulfilledBy(attribute))
                        {
                            result = targets.ToList();
                            ConcreteAttributeTargets[attribute] = result;
                            foundAnyValids = true;
                        }
                    }

                    if (!foundAnyValids)
                    {
                        ConcreteAttributeTargets[attribute] = null;
                    }
                }
                return result ?? EmptyList;
            }

            public static bool ShowInvalidAttributeErrorFor(InspectorProperty property, Type attribute)
            {
                if (property.ValueEntry == null) return false;
                if (property.ValueEntry.BaseValueType == typeof(object)) return false;
                if (property.Parent != null && property.Parent.ChildResolver is ICollectionResolver) return false;
                if (property.GetAttribute<SuppressInvalidAttributeErrorAttribute>() != null) return false;
                if (property.Info.TypeOfValue.IsInterface) return false;

                var collectionResolver = property.ChildResolver as ICollectionResolver;
                if (collectionResolver != null)
                {
                    if (collectionResolver.ElementType == typeof(object)) return false;
                    if (collectionResolver.ElementType.IsInterface) return false;

                    return ShowInvalidAttributeErrorFor(attribute, property.ValueEntry.BaseValueType)
                        && ShowInvalidAttributeErrorFor(attribute, collectionResolver.ElementType);
                }

                return ShowInvalidAttributeErrorFor(attribute, property.ValueEntry.BaseValueType);
            }

            public static bool ShowInvalidAttributeErrorFor(Type attribute, Type value)
            {
                bool result;
                if (!ShowErrorCache.TryGetInnerValue(attribute, value, out result))
                {
                    result = CalculateShowInvalidAttributeErrorFor(attribute, value);
                    ShowErrorCache[attribute][value] = result;
                }
                return result;
            }

            private static bool CalculateShowInvalidAttributeErrorFor(Type attribute, Type value)
            {
                if (attribute == typeof(DelayedAttribute) || attribute == typeof(DelayedPropertyAttribute))
                {
                    return false;
                }

                var validTargets = GetValidTargets(attribute);

                if (validTargets.Count == 0) return false;
                if (value == typeof(object)) return false;

                for (int i = 0; i < validTargets.Count; i++)
                {
                    var valid = validTargets[i];

                    if (valid == value)
                    {
                        return false;
                    }
                    else if (valid.IsGenericParameter)
                    {
                        if (valid.GenericParameterIsFulfilledBy(value))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}
#endif