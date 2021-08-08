#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidationPathStep.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System.Reflection;

    public struct ValidationPathStep
    {
        public string StepString;
        public object Value;
        public MemberInfo Member;
    }
}
#endif