#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NestedInSameGenericTypeTypeMatcher.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    public class NestedInSameGenericTypeTypeMatcher : TypeMatcher
    {
        private TypeSearchInfo info;
        private Type matchTypeGenericDefinition;

        public override string Name { get { return "Nested In Same Generic Type ---> Type<T1, [, T2]>.NestedType : Type<T1, [, T2]>.Match<Target>"; } }

        public override Type Match(Type[] targets, ref bool stopMatching)
        {
            if (targets.Length != 1) return null;

            var target = targets[0];

            if (!target.IsNested) return null;

            if (!target.DeclaringType.IsGenericType)
            {
                return null;
            }

            if (matchTypeGenericDefinition != target.DeclaringType.GetGenericTypeDefinition()) return null;

            var args = target.GetGenericArguments();

            if (info.MatchType.AreGenericConstraintsSatisfiedBy(args))
            {
                return info.MatchType.MakeGenericType(args);
            }

            return null;
        }

        public class Creator : TypeMatcherCreator
        {
            public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
            {
                matcher = null;

                if (info.Targets.Length == 0 || !info.MatchType.IsNested || !info.Targets[0].IsNested) return false;

                if (!info.MatchType.DeclaringType.IsGenericType ||
                    !info.Targets[0].DeclaringType.IsGenericType)
                {
                    return false;
                }

                var matchTypeGenericDefinition = info.MatchType.DeclaringType.GetGenericTypeDefinition();

                if (matchTypeGenericDefinition != info.Targets[0].DeclaringType.GetGenericTypeDefinition()) return false;

                matcher = new NestedInSameGenericTypeTypeMatcher()
                {
                    info = info,
                    matchTypeGenericDefinition = matchTypeGenericDefinition,
                };

                return true;
            }
        }
    }
}
#endif