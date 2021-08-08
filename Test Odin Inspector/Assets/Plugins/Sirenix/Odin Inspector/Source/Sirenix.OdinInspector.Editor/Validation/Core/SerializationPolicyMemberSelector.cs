#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SerializationPolicyMemberSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class SerializationPolicyMemberSelector : IMemberSelector
    {
        public readonly ISerializationPolicy Policy;

        public SerializationPolicyMemberSelector(ISerializationPolicy policy)
        {
            this.Policy = policy;
        }

        public IList<MemberInfo> SelectMembers(Type type)
        {
            return FormatterUtilities.GetSerializableMembers(type, this.Policy);
        }
    }
}
#endif