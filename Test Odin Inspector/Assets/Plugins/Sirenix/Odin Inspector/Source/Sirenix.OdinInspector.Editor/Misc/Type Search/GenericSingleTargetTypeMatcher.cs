#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GenericSingleTargetTypeMatcher.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    public class GenericSingleTargetTypeMatcher : TypeMatcher
    {
        private TypeSearchInfo info;
        private Type[] matchArgs;
        private Type[] matchTargetArgs;
        private Type targetGenericTypeDefinition;

        public override string Name { get { return "Generic Single Target Match --> Type<T1 [, T2]> : Match<GenericType<T1 [, T2]>> [where T1 [, T2] : constraints]"; } }

        public override Type Match(Type[] targets, ref bool stopMatching)
        {
            if (targets.Length != 1) return null;
            if (!targets[0].IsGenericType) return null;
            if (this.targetGenericTypeDefinition != targets[0].GetGenericTypeDefinition()) return null;
            
            var targetArgs = targets[0].GetGenericArguments();

            if (this.matchArgs.Length != this.matchTargetArgs.Length || matchArgs.Length != targetArgs.Length) return null;

            if (!info.MatchType.AreGenericConstraintsSatisfiedBy(targetArgs)) return null;

            return info.MatchType.MakeGenericType(targetArgs);
        }

        public class Creator : TypeMatcherCreator
        {
            public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
            {
                matcher = null;

                if (!info.MatchType.IsGenericTypeDefinition) return false;
                if (!info.Targets[0].IsGenericType) return false;

                matcher = new GenericSingleTargetTypeMatcher()
                {
                    info = info,
                    matchArgs = info.MatchType.GetGenericArguments(),
                    matchTargetArgs = info.Targets[0].GetGenericArguments(),
                    targetGenericTypeDefinition = info.Targets[0].GetGenericTypeDefinition(),
                };

                return true;
            }
        }
    }
}
#endif