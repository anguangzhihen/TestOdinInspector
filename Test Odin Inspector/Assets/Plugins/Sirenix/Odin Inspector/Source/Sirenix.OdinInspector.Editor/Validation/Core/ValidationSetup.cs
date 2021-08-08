#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidationSetup.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using System;
    using System.Reflection;

    public struct ValidationSetup
    {
        public Validator Validator;
        public MemberInfo Member;
        public object Value;
        public object ParentInstance;
        public object Root;

        [Obsolete("There is no longer any strict distinction between value and member validation, as validation is run on properties instead.", false)]
        public ValidationKind Kind;
    }
}
#endif