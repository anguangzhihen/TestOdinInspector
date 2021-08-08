#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeMatchRule.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using System;

    public class TypeMatchRule
    {
        public delegate Type TypeMatchRuleDelegate1(TypeSearchInfo info, Type[] targets);

        public delegate Type TypeMatchRuleDelegate2(TypeSearchInfo info, Type[] targets, ref bool stopMatchingForInfo);

        public readonly string Name;

        private TypeMatchRuleDelegate1 rule1;
        private TypeMatchRuleDelegate2 rule2;

        public TypeMatchRule(string name, TypeMatchRuleDelegate1 rule)
        {
            this.Name = name;
            this.rule1 = rule;
        }

        public TypeMatchRule(string name, TypeMatchRuleDelegate2 rule)
        {
            this.Name = name;
            this.rule2 = rule;
        }

        public Type Match(TypeSearchInfo matchInfo, Type[] targets, ref bool stopMatchingForInfo)
        {
            if (this.rule1 != null)
            {
                return this.rule1(matchInfo, targets);
            }
            else
            {
                return this.rule2(matchInfo, targets, ref stopMatchingForInfo);
            }
        }

        public override string ToString()
        {
            return "TypeMatchRule: " + this.Name;
        }
    }
}
#endif