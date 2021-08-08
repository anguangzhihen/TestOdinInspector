#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeSearchIndex.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class TypeSearchIndex
    {
        private class ProcessedTypeSearchInfo
        {
            public TypeSearchInfo Info;
            public List<TypeMatcher> Matchers;
        }

        public string MatchedTypeLogName = "matched type";

        public List<TypeMatchIndexingRule> IndexingRules = new List<TypeMatchIndexingRule>();
        public List<TypeMatchRule> MatchRules = new List<TypeMatchRule>();
        public Action<string, TypeSearchInfo> LogInvalidTypeInfo = (message, info) => Debug.LogError(message);

        private readonly List<ProcessedTypeSearchInfo> indexedTypes = new List<ProcessedTypeSearchInfo>();
        private readonly Type[] CachedTargetArray1 = new Type[1];
        private readonly Type[] CachedTargetArray2 = new Type[2];
        
        /// <summary>
        /// To safely change anything in the type cache, you must be holding this lock.
        /// </summary>
        public readonly object LOCK = new object();

        public List<TypeMatcherCreator> TypeMatcherCreators = new List<TypeMatcherCreator>();

        private class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
        {
            public bool Equals(Type[] x, Type[] y)
            {
                if (x == y) return true;
                if (x == null || y == null) return false;
                if (x.Length != y.Length) return false;

                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i]) return false;
                }

                return true;
            }

            public int GetHashCode(Type[] obj)
            {
                if (obj == null) return 0;
                const int prime = 137;
                int result = 1;
                unchecked
                {
                    for (int i = 0; i < obj.Length; i++)
                    {
                        var type = obj[i];
                        var typeHash = type == null ? 1 : type.GetHashCode();
                        result = prime * result + (typeHash ^ (typeHash >> 16));
                    }
                }
                return result;
            }
        }

        private Dictionary<Type[], TypeSearchResult[]> resultCache = new Dictionary<Type[], TypeSearchResult[]>(new TypeArrayEqualityComparer());

        public TypeSearchIndex(bool addDefaultValidationRules = true, bool addDefaultMatchRules = true)
        {
            if (addDefaultValidationRules)
            {
                this.AddDefaultIndexingRules();
            }

            if (addDefaultMatchRules)
            {
                this.AddDefaultMatchRules();
                this.AddDefaultMatchCreators();
            }
        }

        private struct QueryResult
        {
            public int CurrentIndex;
            public double CurrentPriority;
            public TypeSearchResult[] Result;

            public QueryResult(TypeSearchResult[] result)
            {
                this.Result = result;
                this.CurrentIndex = 0;
                this.CurrentPriority = result[0].MatchedInfo.Priority;
            }
        }

        private static readonly List<QueryResult> CachedQueryResultList = new List<QueryResult>();

        private class MergeSignatureComparer : IEqualityComparer<MergeSignature>
        {
            public bool Equals(MergeSignature x, MergeSignature y)
            {
                if (x.Hash != y.Hash) return false;
                if (object.ReferenceEquals(x.Results, y.Results)) return true;

                var count = x.ResultCount;
                if (count != y.ResultCount) return false;

                for (int i = 0; i < count; i++)
                {
                    if (!object.ReferenceEquals(x.Results[i], y.Results[i])) return false;
                }

                return true;
            }

            public int GetHashCode(MergeSignature obj)
            {
                return obj.Hash;
            }
        }

        private struct MergeSignature
        {
            public int Hash;
            public IList<TypeSearchResult[]> Results;
            public int ResultCount;

            public MergeSignature(IList<TypeSearchResult[]> results, int count)
            {
                this.Results = results;
                this.ResultCount = count;

                const int prime = 137;
                int result = 1;
                unchecked
                {
                    for (int i = 0; i < count; i++)
                    {
                        var hash = results[i].GetHashCode();
                        result = prime * result + (hash ^ (hash >> 16));
                    }
                }
                this.Hash = result;
            }

            public override int GetHashCode()
            {
                return this.Hash;
            }
        }

        private static readonly object STATIC_LOCK = new object();
        private static readonly Dictionary<MergeSignature, TypeSearchResult[]> KnownMergeSignatures = new Dictionary<MergeSignature, TypeSearchResult[]>(new MergeSignatureComparer());
        private static readonly List<TypeSearchResult> CachedFastMergeList = new List<TypeSearchResult>();
        private static readonly TypeSearchResult[] EmptyResultArray = new TypeSearchResult[0];

        public static List<List<Type[]>> GetAllCachedMergeSignatures(TypeSearchIndex index)
        {
            lock (STATIC_LOCK)
            {
                List<List<Type[]>> result = new List<List<Type[]>>();

                foreach (var mergeSignature in KnownMergeSignatures.Keys)
                {
                    if (!IsMergeSignatureForIndex(mergeSignature, index)) continue;

                    List<Type[]> signatureList = new List<Type[]>();

                    for (int i = 0; i < mergeSignature.ResultCount; i++)
                    {
                        TypeSearchResult[] searchResult = mergeSignature.Results[i];
                        // One result in a match

                        if (searchResult.Length == 0)
                        {
                            signatureList.Add(Type.EmptyTypes);
                        }
                        else
                        {
                            signatureList.Add(searchResult[0].MatchedTargets);
                        }
                    }

                    result.Add(signatureList);
                }

                return result;
            }
        }

        private static bool IsMergeSignatureForIndex(MergeSignature signature, TypeSearchIndex index)
        {
            for (int i = 0; i < signature.ResultCount; i++)
            {
                TypeSearchResult[] resultSet = signature.Results[i];

                if (resultSet.Length > 0)
                {
                    return resultSet[0].MatchedIndex == index;
                }
            }

            return false;
        }

        public static TypeSearchResult[] GetCachedMergedQueryResults(TypeSearchResult[][] results, int resultsCount)
        {
            if (resultsCount == 0)
            {
                return EmptyResultArray;
            }

            if (resultsCount == 1)
            {
                return results[0];
            }

            TypeSearchResult[] fastResultArray;

            lock (STATIC_LOCK)
            {
                // The following merge signature-based caching logic results in a roughly 2-3x speedup over doing the actual merge, once the actual merge has been done once
                var mergeSignature = new MergeSignature(results, resultsCount);

                if (KnownMergeSignatures.TryGetValue(mergeSignature, out fastResultArray))
                {
                    return fastResultArray;
                }

                // None of our fast paths worked, so we have to do the actual merging now

                var mergeIntoList = CachedFastMergeList;
                mergeIntoList.Clear();

                var queries = CachedQueryResultList;
                queries.Clear();

                for (int i = 0; i < resultsCount; i++)
                {
                    if (results[i].Length == 0)
                    {
                        continue;
                    }

                    queries.Add(new QueryResult(results[i]));
                }

                // The below loop is about unifying all the queries (which are individually cached and sorted
                // by the TypeSearchIndex) so that their results are combined in a proper sorted order.
                //
                // The sorted results are temporarily stored in the finalResults list, so that we don't have
                // to yield out of this loop while it's running and so introduce unknown performance hits or
                // cache misses.

                var queriesCount = queries.Count;

                while (true)
                {
                    // Find the next drawer with the highest priority
                    double highestPriority = double.MinValue;
                    int highestIndex = -1;

                    for (int i = 0; i < queriesCount; i++)
                    {
                        var query = queries[i];
                        if (query.CurrentIndex >= query.Result.Length) continue;
                        if (query.CurrentPriority > highestPriority)
                        {
                            highestPriority = query.CurrentPriority;
                            highestIndex = i;
                        }
                    }

                    // If there was no drawer available at all, then we are done.
                    if (highestIndex == -1)
                        break;

                    var highest = queries[highestIndex];
                    mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
                    highest.CurrentIndex++;

                    if (highest.CurrentIndex < highest.Result.Length)
                    {
                        highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
                    }

                    queries[highestIndex] = highest;
                }

                mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);

                var arr = mergeIntoList.ToArray();
                KnownMergeSignatures.Add(mergeSignature, arr);
                return arr;
            }
        }

        public static TypeSearchResult[] GetCachedMergedQueryResults(List<TypeSearchResult[]> results)
        {
            if (results.Count == 0)
            {
                return EmptyResultArray;
            }

            if (results.Count == 1)
            {
                return results[0];
            }

            TypeSearchResult[] fastResultArray;

            lock (STATIC_LOCK)
            {
                // The following merge signature-based caching logic results in a roughly 2-3x speedup over doing the actual merge, once the actual merge has been done once
                var mergeSignature = new MergeSignature(results, results.Count);

                if (KnownMergeSignatures.TryGetValue(mergeSignature, out fastResultArray))
                {
                    return fastResultArray;
                }

                // None of our fast paths worked, so we have to do the actual merging now

                var mergeIntoList = CachedFastMergeList;
                mergeIntoList.Clear();

                var queries = CachedQueryResultList;
                queries.Clear();

                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].Length == 0)
                    {
                        continue;
                    }

                    queries.Add(new QueryResult(results[i]));
                }

                // The below loop is about unifying all the queries (which are individually cached and sorted
                // by the TypeSearchIndex) so that their results are combined in a proper sorted order.
                //
                // The sorted results are temporarily stored in the finalResults list, so that we don't have
                // to yield out of this loop while it's running and so introduce unknown performance hits or
                // cache misses.

                var queriesCount = queries.Count;

                while (true)
                {
                    // Find the next drawer with the highest priority
                    double highestPriority = double.MinValue;
                    int highestIndex = -1;

                    for (int i = 0; i < queriesCount; i++)
                    {
                        var query = queries[i];
                        if (query.CurrentIndex >= query.Result.Length) continue;
                        if (query.CurrentPriority > highestPriority)
                        {
                            highestPriority = query.CurrentPriority;
                            highestIndex = i;
                        }
                    }

                    // If there was no drawer available at all, then we are done.
                    if (highestIndex == -1)
                        break;

                    var highest = queries[highestIndex];
                    mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
                    highest.CurrentIndex++;

                    if (highest.CurrentIndex < highest.Result.Length)
                    {
                        highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
                    }

                    queries[highestIndex] = highest;
                }

                mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);

                var arr = mergeIntoList.ToArray();
                KnownMergeSignatures.Add(mergeSignature, arr);
                return arr;
            }
        }

        public static void MergeQueryResultsIntoList(List<TypeSearchResult[]> results, List<TypeSearchResult> mergeIntoList)
        {
            mergeIntoList.Clear();

            if (results.Count == 0)
            {
                return;
            }

            if (results.Count == 1)
            {
                var arr = results[0];

                for (int i = 0; i < arr.Length; i++)
                {
                    mergeIntoList.Add(arr[i]);
                }

                return;
            }

            lock (STATIC_LOCK)
            {
                TypeSearchResult[] fastResultArray;

                // The following merge signature-based caching logic results in a roughly 2-3x speedup over doing the actual merge, once the actual merge has been done once
                var mergeSignature = new MergeSignature(results, results.Count);

                if (KnownMergeSignatures.TryGetValue(mergeSignature, out fastResultArray))
                {
                    for (int i = 0; i < fastResultArray.Length; i++)
                    {
                        mergeIntoList.Add(fastResultArray[i]);
                    }

                    return;
                }

                // None of our fast paths worked, so we have to do the actual merging now
                var queries = CachedQueryResultList;
                queries.Clear();

                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].Length == 0)
                    {
                        continue;
                    }

                    queries.Add(new QueryResult(results[i]));
                }

                // The below loop is about unifying all the queries (which are individually cached and sorted
                // by the TypeSearchIndex) so that their results are combined in a proper sorted order.
                //
                // The sorted results are temporarily stored in the finalResults list, so that we don't have
                // to yield out of this loop while it's running and so introduce unknown performance hits or
                // cache misses.

                var queriesCount = queries.Count;

                while (true)
                {
                    // Find the next drawer with the highest priority
                    double highestPriority = double.MinValue;
                    int highestIndex = -1;

                    for (int i = 0; i < queriesCount; i++)
                    {
                        var query = queries[i];
                        if (query.CurrentIndex >= query.Result.Length) continue;
                        if (query.CurrentPriority > highestPriority)
                        {
                            highestPriority = query.CurrentPriority;
                            highestIndex = i;
                        }
                    }

                    // If there was no drawer available at all, then we are done.
                    if (highestIndex == -1)
                        break;

                    var highest = queries[highestIndex];
                    mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
                    highest.CurrentIndex++;

                    if (highest.CurrentIndex < highest.Result.Length)
                    {
                        highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
                    }

                    queries[highestIndex] = highest;
                }

                mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);
                KnownMergeSignatures.Add(mergeSignature, mergeIntoList.ToArray());
            }
        }

        public List<Type[]> GetAllCachedTargets()
        {
            var result = new List<Type[]>();

            lock (this.LOCK)
            {
                foreach (var key in this.resultCache.Keys)
                {
                    result.Add(key);
                }
            }

            return result;
        }

        public void AddIndexedType(TypeSearchInfo typeToIndex)
        {
            lock (this.LOCK)
            {
                var indexedType = this.ProcessInfo(typeToIndex);

                if (indexedType != null)
                {
                    InsertIndexedTypeSorted(this.indexedTypes, indexedType);
                }

                if (this.resultCache.Count > 0)
                {
                    this.resultCache.Clear();
                }
            }
        }

        public void AddIndexedTypeUnsorted(TypeSearchInfo typeToIndex)
        {
            lock (this.LOCK)
            {
                var indexedType = this.ProcessInfo(typeToIndex);

                if (indexedType != null)
                {
                    this.indexedTypes.Add(indexedType);
                }

                if (this.resultCache.Count > 0)
                {
                    this.resultCache.Clear();
                }
            }
        }

        public void AddIndexedTypes(List<TypeSearchInfo> typesToIndex)
        {
            lock (this.LOCK)
            {
                for (int i = 0; i < typesToIndex.Count; i++)
                {
                    var indexedType = this.ProcessInfo(typesToIndex[i]);

                    if (indexedType != null)
                    {
                        InsertIndexedTypeSorted(this.indexedTypes, indexedType);
                    }
                }

                if (this.resultCache.Count > 0)
                {
                    this.resultCache.Clear();
                }
            }
        }

        public void ClearResultCache()
        {
            lock (this.LOCK)
            {
                this.resultCache.Clear();
            }
        }

        public TypeSearchResult[] GetMatches(Type target)
        {
            if (target == null) throw new ArgumentNullException("target");

            TypeSearchResult[] result;

            lock (this.LOCK)
            {
                var array = CachedTargetArray1;
                array[0] = target;

                if (!this.resultCache.TryGetValue(array, out result))
                {
                    var targets = new Type[] { target };

                    result = this.FindAllMatches(targets);
                    this.resultCache[targets] = result;
                }
            }

            return result;
        }

        public TypeSearchResult[] GetMatches(Type target1, Type target2)
        {
            if (target1 == null) throw new ArgumentNullException("target1");
            if (target2 == null) throw new ArgumentNullException("target2");

            TypeSearchResult[] result;

            lock (this.LOCK)
            {
                var array = CachedTargetArray2;
                array[0] = target1;
                array[1] = target2;

                if (!this.resultCache.TryGetValue(array, out result))
                {
                    var targets = new Type[] { target1, target2 };

                    result = this.FindAllMatches(targets);
                    this.resultCache[targets] = result;
                }
            }

            return result;
        }

        public TypeSearchResult[] GetMatches(params Type[] targets)
        {
            if (targets == null) throw new ArgumentNullException("targets");

            TypeSearchResult[] result;

            lock (this.LOCK)
            {
                if (!this.resultCache.TryGetValue(targets, out result))
                {
                    result = this.FindAllMatches(targets);
                    this.resultCache[targets] = result;
                }
            }

            return result;
        }

        private TypeSearchResult[] FindAllMatches(Type[] targets)
        {
            // We look through and match on indexedTypes linearly,
            // and indexedTypes is already sorted properly - therefore
            // we do not need to sort or order anything here.

            List<TypeSearchResult> sortedMatches = new List<TypeSearchResult>();

            for (int i = 0; i < this.indexedTypes.Count; i++)
            {
                var info = this.indexedTypes[i];

                if (targets.Length != info.Info.Targets.Length) continue;

                for (int j = 0; j < info.Matchers.Count; j++)
                {
                    bool stopMatchingForInfo = false;
                    var matcher = info.Matchers[j];
                    var match = matcher.Match(targets, ref stopMatchingForInfo);

                    if (match != null)
                    {
                        sortedMatches.Add(new TypeSearchResult()
                        {
                            MatchedInfo = info.Info,
                            MatchedMatcher = matcher,
                            MatchedType = match,
                            MatchedTargets = targets,
                            MatchedIndex = this,
                        });
                        break;
                    }

                    if (stopMatchingForInfo)
                    {
                        break;
                    }
                }

                for (int j = 0; j < this.MatchRules.Count; j++)
                {
                    var rule = this.MatchRules[j];
                    bool stopMatchingForInfo = false;
                    var match = rule.Match(info.Info, targets, ref stopMatchingForInfo);

                    if (match != null)
                    {
                        sortedMatches.Add(new TypeSearchResult()
                        {
                            MatchedInfo = info.Info,
                            MatchedRule = rule,
                            MatchedType = match,
                            MatchedTargets = targets,
                            MatchedIndex = this,
                        });
                        break;
                    }

                    if (stopMatchingForInfo)
                    {
                        break;
                    }
                }
            }

            return sortedMatches.ToArray();
        }

        private static void InsertIndexedTypeSorted(List<ProcessedTypeSearchInfo> indexedTypes, ProcessedTypeSearchInfo typeInfo)
        {
            //var matchPriority = typeInfo.Info.Priority;

            //for (int i = 0; i < indexedTypes.Count; i++)
            //{
            //    if (matchPriority > indexedTypes[i].Info.Priority)
            //    {
            //        indexedTypes.Insert(i, typeInfo);
            //        return;
            //    }
            //}

            // indexedTypes.Add(typeInfo);

            // Binary search for insertion point as the list is already sorted

            var priority = typeInfo.Info.Priority;
            int left = 0, right = indexedTypes.Count - 1;

            int current = 0; 
            int compare = 0;

            while (left <= right)
            {
                current = (left + right) / 2;

                var middle = indexedTypes[current];

                compare = priority < middle.Info.Priority ? -1 : (priority > middle.Info.Priority ? 1 : 0);

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
                var count = indexedTypes.Count;

                while (current + 1 < count)
                {
                    var next = indexedTypes[current + 1];

                    if (priority > next.Info.Priority)
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

            indexedTypes.Insert(current, typeInfo);
        }

        private ProcessedTypeSearchInfo ProcessInfo(TypeSearchInfo info)
        {
            var originalInfo = info;
            if (info.Targets == null) info.Targets = Type.EmptyTypes;

            for (int i = 0; i < info.Targets.Length; i++)
            {
                if (info.Targets[i] == null)
                {
                    throw new ArgumentNullException("Target at index " + i + " in info for match type " + info.MatchType.GetNiceFullName() + " is null.");
                }
            }

            for (int i = 0; i < this.IndexingRules.Count; i++)
            {
                var rule = this.IndexingRules[i];
                string errorMessage = null;

                if (!rule.Process(ref info, ref errorMessage))
                {
                    if (this.LogInvalidTypeInfo != null)
                    {
                        if (errorMessage == null)
                        {
                            this.LogInvalidTypeInfo("Invalid " + this.MatchedTypeLogName + " declaration '" + originalInfo.MatchType.GetNiceFullName() + "'! Rule '" + rule.Name.Replace("{name}", this.MatchedTypeLogName) + "' failed.", originalInfo);
                        }
                        else
                        {
                            errorMessage = errorMessage.Replace("{name}", this.MatchedTypeLogName);
                            this.LogInvalidTypeInfo("Invalid " + this.MatchedTypeLogName + " declaration '" + originalInfo.MatchType.GetNiceFullName() + "'! Rule '" + rule.Name.Replace("{name}", this.MatchedTypeLogName) + "' failed with message: " + errorMessage, originalInfo);
                        }
                    }

                    return null;
                }
            }

            var processedInfo = new ProcessedTypeSearchInfo();

            processedInfo.Info = info;
            processedInfo.Matchers = new List<TypeMatcher>(this.TypeMatcherCreators.Count);

            for (int i = 0; i < this.TypeMatcherCreators.Count; i++)
            {
                TypeMatcher matcher;

                if (this.TypeMatcherCreators[i].TryCreateMatcher(info, out matcher))
                {
                    processedInfo.Matchers.Add(matcher);
                }
            }

            return processedInfo;
        }

        public void AddDefaultMatchRules()
        {
            //this.MatchRules.Add(DefaultMatchRules.ExactMatch);
            //this.MatchRules.Add(DefaultMatchRules.GenericSingleTargetMatch);
            //this.MatchRules.Add(DefaultMatchRules.TargetsSatisfyGenericParameterConstraints);
            //this.MatchRules.Add(DefaultMatchRules.GenericParameterInference); 
            //this.MatchRules.Add(DefaultMatchRules.NestedInSameGenericType);
        }

        public void AddDefaultMatchCreators()
        {
            lock (this.LOCK)
            {
                this.TypeMatcherCreators.Add(new ExactTypeMatcher.Creator());
                this.TypeMatcherCreators.Add(new GenericSingleTargetTypeMatcher.Creator());
                this.TypeMatcherCreators.Add(new TargetsSatisfyGenericParameterConstraintsTypeMatcher.Creator());
                this.TypeMatcherCreators.Add(new GenericParameterInferenceTypeMatcher.Creator());
                this.TypeMatcherCreators.Add(new NestedInSameGenericTypeTypeMatcher.Creator());
            }
        }

        public void AddDefaultIndexingRules()
        {
            lock (this.LOCK)
            {
                this.IndexingRules.Add(DefaultIndexingRules.MustBeAbleToInstantiateType);
                this.IndexingRules.Add(DefaultIndexingRules.NoAbstractOrInterfaceTargets);
                this.IndexingRules.Add(DefaultIndexingRules.GenericMatchTypeValidation);
                this.IndexingRules.Add(DefaultIndexingRules.GenericDefinitionSanityCheck);
            }
        }
    }
}
#endif