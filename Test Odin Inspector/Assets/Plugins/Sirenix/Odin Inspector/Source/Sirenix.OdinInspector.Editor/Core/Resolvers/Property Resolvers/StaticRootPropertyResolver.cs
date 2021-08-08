#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StaticRootPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities;

    public class StaticInspectorSerializationBackend : SerializationBackend
    {
        public static readonly StaticInspectorSerializationBackend Default = new StaticInspectorSerializationBackend();

        public override bool SupportsGenerics { get { return true; } }

        public override bool SupportsPolymorphism { get { return true; } }

        public override bool SupportsCyclicReferences { get { return true; } }

        public override bool IsUnity { get { return false; } }

        public override bool CanSerializeMember(MemberInfo member) { return true; }

        public override bool CanSerializeType(Type type) { return true; }
    }

    [OdinDontRegister] // DefaultOdinPropertyResolverLocator handles putting this on static tree root properties
    public class StaticRootPropertyResolver<T> : BaseMemberPropertyResolver<T>
    {

        private Type targetType;
        private PropertyContext<bool> allowObsoleteMembers;

        protected override bool AllowNullValues { get { return true; } }

        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            this.targetType = this.ValueEntry.TypeOfValue;
            var members = targetType.GetAllMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var infos = new List<InspectorPropertyInfo>();

            this.allowObsoleteMembers = this.Property.Context.GetGlobal("ALLOW_OBSOLETE_STATIC_MEMBERS", false);

            foreach (var member in members.Where(Filter).OrderBy(Order))
            {
                var attributes = new List<Attribute>();
                InspectorPropertyInfoUtility.ProcessAttributes(this.Property, member, attributes);

                if (member is MethodInfo)
                {
                    if ((member as MethodInfo).IsGenericMethodDefinition)
                    {
                        // Let's not deal with this for now.
                        continue;
                    }

                    if (!attributes.HasAttribute<ButtonAttribute>() && !attributes.HasAttribute<OnInspectorGUIAttribute>())
                    {
                        attributes.Add(new ButtonAttribute(ButtonSizes.Medium));
                    }
                }

                var backend = member is MethodInfo ? SerializationBackend.None : StaticInspectorSerializationBackend.Default;
                var info = InspectorPropertyInfo.CreateForMember(member, true, backend, attributes);

                InspectorPropertyInfo previousPropertyWithName = null;
                int previousPropertyIndex = -1;

                for (int j = 0; j < infos.Count; j++)
                {
                    if (infos[j].PropertyName == info.PropertyName)
                    {
                        previousPropertyIndex = j;
                        previousPropertyWithName = infos[j];
                        break;
                    }
                }

                if (previousPropertyWithName != null)
                {
                    bool createAlias = true;

                    if (member.SignaturesAreEqual(previousPropertyWithName.GetMemberInfo()))
                    {
                        createAlias = false;
                        infos.RemoveAt(previousPropertyIndex);
                    }

                    if (createAlias)
                    {
                        var alias = InspectorPropertyInfoUtility.GetPrivateMemberAlias(previousPropertyWithName.GetMemberInfo(), previousPropertyWithName.TypeOfOwner.GetNiceName(), " -> ");
                        infos[previousPropertyIndex] = InspectorPropertyInfo.CreateForMember(alias, true, backend, attributes);
                    }
                }

                infos.Add(info);
            }

            return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(this.Property, targetType, infos, false);
        }
        
        private int Order(MemberInfo arg1)
        {
            if (arg1 is FieldInfo) return 1;
            if (arg1 is PropertyInfo) return 2;
            if (arg1 is MethodInfo) return 3;
            return 4;
        }

        private bool Filter(MemberInfo member)
        {
            if (member.DeclaringType == typeof(object) && targetType != typeof(object)) return false;
            if (!(member is FieldInfo || member is PropertyInfo || member is MethodInfo)) return false;
            if (member is FieldInfo && (member as FieldInfo).IsSpecialName) return false;
            if (member is MethodInfo && (member as MethodInfo).IsSpecialName) return false;
            if (member is PropertyInfo && (member as PropertyInfo).IsSpecialName) return false;
            if (member.IsDefined<CompilerGeneratedAttribute>()) return false;
            if (!allowObsoleteMembers.Value && member.IsDefined<ObsoleteAttribute>()) return false;

            return true;
        }
    }
}
#endif