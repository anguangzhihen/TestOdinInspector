#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DefaultOdinAttributeProcessorLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor.TypeSearch;
    using Sirenix.Utilities;

    /// <summary>
    /// Default implementation and the version that will be used when no other OdinAttributeProcessorLocator instance have been given to a PropertyTree.
    /// This implementation will find all AttributeProcessor definitions not marked with the <see cref="OdinDontRegisterAttribute"/>.
    /// </summary>
    public sealed class DefaultOdinAttributeProcessorLocator : OdinAttributeProcessorLocator
    {
        /// <summary>
        /// Singleton instance of the DefaultOdinAttributeProcessorLocator class.
        /// </summary>
        public static readonly DefaultOdinAttributeProcessorLocator Instance = new DefaultOdinAttributeProcessorLocator();

        /// <summary>
        /// Type search index used for matching <see cref="OdinAttributeProcessor"/> to properties.
        /// </summary>
        public static readonly TypeSearchIndex SearchIndex = new TypeSearchIndex();

        private static Dictionary<Type, OdinAttributeProcessor> ResolverInstanceMap = new Dictionary<Type, OdinAttributeProcessor>(FastTypeComparer.Instance);

        static DefaultOdinAttributeProcessorLocator()
        {
            if (UnityTypeCacheUtility.IsAvailable)
            {
                using (SimpleProfiler.Section("DefaultOdinAttributeProcessorLocator - TypeCache"))
                {
                    var types = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(OdinAttributeProcessor));

                    foreach (var type in types)
                    {
                        if (type.IsAbstract || type.IsDefined<OdinDontRegisterAttribute>())
                            continue;

                        SearchIndex.AddIndexedType(new TypeSearchInfo()
                        {
                            MatchType = type,
                            Priority = ResolverUtilities.GetResolverPriority(type),
                            Targets = type.ImplementsOpenGenericClass(typeof(OdinAttributeProcessor<>)) ? new Type[] { type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeProcessor<>))[0] } : Type.EmptyTypes
                        });
                    }
                }
            }
            else
            {
                using (SimpleProfiler.Section("DefaultOdinAttributeProcessorLocator - Reflection"))
                {
                    var assemblies = ResolverUtilities.GetResolverAssemblies();

                    for (int i = 0; i < assemblies.Count; i++)
                    {
                        var types = assemblies[i].SafeGetTypes();

                        for (int j = 0; j < types.Length; j++)
                        {
                            var type = types[j];

                            if (type.IsAbstract ||
                                !typeof(OdinAttributeProcessor).IsAssignableFrom(type) ||
                                type.IsDefined<OdinDontRegisterAttribute>())
                                continue;

                            SearchIndex.AddIndexedType(new TypeSearchInfo()
                            {
                                MatchType = type,
                                Priority = ResolverUtilities.GetResolverPriority(type),
                                Targets = type.ImplementsOpenGenericClass(typeof(OdinAttributeProcessor<>)) ? new Type[] { type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeProcessor<>))[0] } : Type.EmptyTypes
                            });
                        }
                    }
                }
            }
        }

        private static void IndexType(Type type)
        {

        }

        private List<TypeSearchResult[]> CachedMatchesList = new List<TypeSearchResult[]>();

        /// <summary>
        /// Gets a list of <see cref="OdinAttributeProcessor"/> to process attributes for the specified child member of the parent property.
        /// </summary>
        /// <param name="parentProperty">The parent of the member.</param>
        /// <param name="member">Child member of the parent property.</param>
        /// <returns>List of <see cref="OdinAttributeProcessor"/> to process attributes for the specified member.</returns>
        public override List<OdinAttributeProcessor> GetChildProcessors(InspectorProperty parentProperty, MemberInfo member)
        {
            CachedMatchesList.Clear();
            CachedMatchesList.Add(SearchIndex.GetMatches(Type.EmptyTypes));

            if (parentProperty.ValueEntry != null)
            {
                CachedMatchesList.Add(SearchIndex.GetMatches(parentProperty.ValueEntry.TypeOfValue));
            }

            var results = TypeSearchIndex.GetCachedMergedQueryResults(CachedMatchesList);

            List<OdinAttributeProcessor> processors = new List<OdinAttributeProcessor>(results.Length);

            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                var resolver = GetResolverInstance(result.MatchedType);

                if (resolver.CanProcessChildMemberAttributes(parentProperty, member))
                {
                    processors.Add(resolver);
                }
            }

            return processors;
        }

        /// <summary>
        /// Gets a list of <see cref="OdinAttributeProcessor"/> to process attributes for the specified property.
        /// </summary>
        /// <param name="property">The property to find attribute porcessors for.</param>
        /// <returns>List of <see cref="OdinAttributeProcessor"/> to process attributes for the speicied member.</returns>
        public override List<OdinAttributeProcessor> GetSelfProcessors(InspectorProperty property)
        {
            CachedMatchesList.Clear();
            CachedMatchesList.Add(SearchIndex.GetMatches(Type.EmptyTypes));

            if (property.ValueEntry != null)
            {
                CachedMatchesList.Add(SearchIndex.GetMatches(property.ValueEntry.TypeOfValue));
            }

            var results = TypeSearchIndex.GetCachedMergedQueryResults(CachedMatchesList);
            List<OdinAttributeProcessor> processors = new List<OdinAttributeProcessor>(results.Length);

            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                var resolver = GetResolverInstance(result.MatchedType);

                if (resolver.CanProcessSelfAttributes(property))
                {
                    processors.Add(resolver);
                }
            }

            return processors;
        }

        private static OdinAttributeProcessor GetResolverInstance(Type type)
        {
            OdinAttributeProcessor result;

            if (!ResolverInstanceMap.TryGetValue(type, out result))
            {
                result = (OdinAttributeProcessor)Activator.CreateInstance(type);
                ResolverInstanceMap.Add(type, result);
            }

            return result;
        }
    }
}
#endif