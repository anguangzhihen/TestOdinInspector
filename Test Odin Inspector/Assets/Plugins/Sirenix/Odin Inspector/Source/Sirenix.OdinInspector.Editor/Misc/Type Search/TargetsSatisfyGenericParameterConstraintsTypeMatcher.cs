#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TargetsSatisfyGenericParameterConstraintsTypeMatcher.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    public class TargetsSatisfyGenericParameterConstraintsTypeMatcher : TypeMatcher
    {
        private TypeSearchInfo info;
        private Type[] genericArgs;

        public override string Name { get { return "Targets Satisfy Generic Parameter Constraints --> Type<T1 [, T2]> : Match<T1 [, T2]> [where T1 [, T2] : constraints]"; } }

        public override Type Match(Type[] targets, ref bool stopMatching)
        {
            if (TypeExtensions.AreGenericConstraintsSatisfiedBy(this.genericArgs, targets))
            {
                return info.MatchType.MakeGenericType(targets);
            }

            return null;
        }

        public class Creator : TypeMatcherCreator
        {
            public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
            {
                matcher = null;

                if (!info.MatchType.IsGenericType) return false;

                for (int i = 0; i < info.Targets.Length; i++)
                    if (!info.Targets[i].IsGenericParameter) return false;

                matcher = new TargetsSatisfyGenericParameterConstraintsTypeMatcher()
                {
                    info = info,
                    genericArgs = info.MatchType.GetGenericArguments(),
                };

                return true;
            }
        }
    }
}
#endif