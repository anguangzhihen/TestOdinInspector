#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ExactTypeMatcher.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using System;

    public class ExactTypeMatcher : TypeMatcher
    {
        private TypeSearchInfo info;

        public override string Name { get { return "Exact Match --> Type : Match[<Target>]"; } }

        public override Type Match(Type[] targets, ref bool stopMatching)
        {
            if (targets.Length != this.info.Targets.Length) return null;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != this.info.Targets[i]) return null;
            }

            return this.info.MatchType;
        }

        public class Creator : TypeMatcherCreator
        {
            public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
            {
                matcher = null;

                if (info.MatchType.IsGenericTypeDefinition) return false;

                matcher = new ExactTypeMatcher()
                {
                    info = info
                };

                return true;
            }
        }
    }
}
#endif