#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DefaultMatchRules.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    public static class DefaultMatchRules
    {


        public static readonly TypeMatchRule ExactMatch = new TypeMatchRule(
            "Exact Match --> Type : Match[<Target>]",
            (info, targets) =>
            {
                if (info.MatchType.IsGenericTypeDefinition) return null;
                if (targets.Length != info.Targets.Length) return null;

                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != info.Targets[i]) return null;
                }

                return info.MatchType;
            });

        public static readonly TypeMatchRule GenericSingleTargetMatch = new TypeMatchRule(
            "Generic Single Target Match --> Type<T1 [, T2]> : Match<GenericType<T1 [, T2]>> [where T1 [, T2] : constraints]",
            (info, targets) =>
            {
                if (!info.MatchType.IsGenericTypeDefinition) return null;
                if (targets.Length != 1) return null;
                if (!info.Targets[0].IsGenericType || !targets[0].IsGenericType) return null;
                if (info.Targets[0].GetGenericTypeDefinition() != targets[0].GetGenericTypeDefinition()) return null;

                var matchArgs = info.MatchType.GetGenericArguments();
                var matchTargetArgs = info.Targets[0].GetGenericArguments();
                var targetArgs = targets[0].GetGenericArguments();

                if (matchArgs.Length != matchTargetArgs.Length || matchArgs.Length != targetArgs.Length) return null;

                if (!info.MatchType.AreGenericConstraintsSatisfiedBy(targetArgs)) return null;

                return info.MatchType.MakeGenericType(targetArgs);
            });

        public static readonly TypeMatchRule TargetsSatisfyGenericParameterConstraints = new TypeMatchRule(
            "Targets Satisfy Generic Parameter Constraints --> Type<T1 [, T2]> : Match<T1 [, T2]> [where T1 [, T2] : constraints]",
            (info, targets) =>
            {
                for (int i = 0; i < info.Targets.Length; i++)
                {
                    if (!info.Targets[i].IsGenericParameter) return null;
                }

                if (info.MatchType.IsGenericType && info.MatchType.AreGenericConstraintsSatisfiedBy(targets))
                {
                    return info.MatchType.MakeGenericType(targets);
                }

                return null;
            });

        public static readonly TypeMatchRule GenericParameterInference = new TypeMatchRule(
            "Generic Parameter Inference ---> Type<T1 [, T2] : Match<T1> [where T1 : constraints [, T2]]",
            (info, targets) =>
            {
                Type[] inferTargets;

                // Make sure we can apply generic parameter inference to the match info
                {
                    if (!info.MatchType.IsGenericType) return null;

                    int genericParameterTargetCount = 0;

                    for (int i = 0; i < info.Targets.Length; i++)
                    {
                        if (info.Targets[i].IsGenericParameter)
                        {
                            genericParameterTargetCount++;
                        }
                        else if (info.Targets[i] != targets[i])
                        {
                            // Everything but generic parameters must match exactly
                            return null;
                        }
                    }

                    if (genericParameterTargetCount == 0) return null;

                    if (genericParameterTargetCount != targets.Length)
                    {
                        inferTargets = new Type[genericParameterTargetCount];
                        int count = 0;
                        for (int i = 0; i < info.Targets.Length; i++)
                        {
                            if (info.Targets[i].IsGenericParameter)
                            {
                                inferTargets[count++] = targets[i];
                            }
                        }
                    }
                    else
                    {
                        inferTargets = targets;
                    }
                }

                Type[] inferredArgs;

                try
                {
                    if (info.MatchType.TryInferGenericParameters(out inferredArgs, inferTargets))
                    {
                        return info.MatchType.GetGenericTypeDefinition().MakeGenericType(inferredArgs);
                    }
                }
                catch (ArgumentException ex)
                {
                    UnityEngine.Debug.Log("WHoops");

                    if (info.MatchType.TryInferGenericParameters(out inferredArgs, inferTargets))
                    {
                        return info.MatchType.GetGenericTypeDefinition().MakeGenericType(inferredArgs);
                    }

                    throw ex;
                }

                return null;
            });

        public static readonly TypeMatchRule NestedInSameGenericType = new TypeMatchRule(
            "Nested In Same Generic Type ---> Type<T1, [, T2]>.NestedType : Type<T1, [, T2]>.Match<Target>",
            (info, targets) =>
            {
                if (targets.Length != 1) return null;

                var target = targets[0];

                if (!info.MatchType.IsNested || !info.Targets[0].IsNested || !target.IsNested) return null;

                if (!info.MatchType.DeclaringType.IsGenericType ||
                    !info.Targets[0].DeclaringType.IsGenericType ||
                    !target.DeclaringType.IsGenericType)
                {
                    return null;
                }

                if (info.MatchType.DeclaringType.GetGenericTypeDefinition() != info.Targets[0].DeclaringType.GetGenericTypeDefinition() ||
                    info.MatchType.DeclaringType.GetGenericTypeDefinition() != target.DeclaringType.GetGenericTypeDefinition()) return null;

                var args = target.GetGenericArguments();

                if (info.MatchType.AreGenericConstraintsSatisfiedBy(args))
                {
                    return info.MatchType.MakeGenericType(args);
                }

                return null;
            });
    }
}
#endif