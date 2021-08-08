#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinPropertyProcessorLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.TypeSearch;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public static class OdinPropertyProcessorLocator
    {
        private static readonly Dictionary<Type, OdinPropertyProcessor> EmptyInstances = new Dictionary<Type, OdinPropertyProcessor>(FastTypeComparer.Instance);
        public static readonly TypeSearchIndex SearchIndex = new TypeSearchIndex() { MatchedTypeLogName = "member property processor" };
        private static readonly List<TypeSearchResult[]> CachedQueryList = new List<TypeSearchResult[]>();

        static OdinPropertyProcessorLocator()
        {
            if (UnityTypeCacheUtility.IsAvailable)
            {
                using (SimpleProfiler.Section("OdinPropertyProcessorLocator Type Cache"))
                {
                    var types = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(OdinPropertyProcessor));

                    foreach (var type in types)
                    {
                        if (type.IsAbstract || type.IsDefined<OdinDontRegisterAttribute>(false))
                            continue;

                        IndexType(type);
                    }
                }
            }
            else
            {
                using (SimpleProfiler.Section("OdinPropertyProcessorLocator Reflection"))
                {
                    var assemblies = ResolverUtilities.GetResolverAssemblies();

                    for (int i = 0; i < assemblies.Count; i++)
                    {
                        var types = assemblies[i].SafeGetTypes();

                        for (int j = 0; j < types.Length; j++)
                        {
                            var type = types[j];

                            if (type.IsAbstract ||
                                !typeof(OdinPropertyProcessor).IsAssignableFrom(type) ||
                                type.IsDefined<OdinDontRegisterAttribute>(false))
                                continue;

                            IndexType(type);
                        }
                    }
                }
            }
        }

        private static void IndexType(Type type)
        {
            if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<>)))
            {
                if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<,>)))
                {
                    // Value/attribute targeted resolver
                    SearchIndex.AddIndexedType(new TypeSearchInfo()
                    {
                        MatchType = type,
                        Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<,>)),
                        Priority = ResolverUtilities.GetResolverPriority(type)
                    });
                }
                else
                {
                    // Value targeted resolver
                    SearchIndex.AddIndexedType(new TypeSearchInfo()
                    {
                        MatchType = type,
                        Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<>)),
                        Priority = ResolverUtilities.GetResolverPriority(type)
                    });
                }
            }
            else
            {
                // No target constraints resolver (only CanResolveForProperty)
                SearchIndex.AddIndexedType(new TypeSearchInfo()
                {
                    MatchType = type,
                    Targets = Type.EmptyTypes,
                    Priority = ResolverUtilities.GetResolverPriority(type)
                });
            }
        }

        public static List<OdinPropertyProcessor> GetMemberProcessors(InspectorProperty property)
        {
            var queries = CachedQueryList;
            queries.Clear();

            //var results = CachedResultList;
            //results.Clear();

            queries.Add(SearchIndex.GetMatches(Type.EmptyTypes));

            if (property.ValueEntry != null)
            {
                var valueType = property.ValueEntry.TypeOfValue;

                queries.Add(SearchIndex.GetMatches(valueType));

                for (int i = 0; i < property.Attributes.Count; i++)
                {
                    queries.Add(SearchIndex.GetMatches(valueType, property.Attributes[i].GetType()));
                }
            }

            var results = TypeSearchIndex.GetCachedMergedQueryResults(queries);

            List<OdinPropertyProcessor> processors = new List<OdinPropertyProcessor>();

            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                if (GetEmptyInstance(result.MatchedType).CanProcessForProperty(property))
                {
                    processors.Add(OdinPropertyProcessor.Create(result.MatchedType, property));
                }
            }

            return processors;
        }

        private static OdinPropertyProcessor GetEmptyInstance(Type type)
        {
            OdinPropertyProcessor result;
            if (!EmptyInstances.TryGetValue(type, out result))
            {
                result = (OdinPropertyProcessor)FormatterServices.GetUninitializedObject(type);
                EmptyInstances[type] = result;
            }
            return result;
        }
    }
}
#endif