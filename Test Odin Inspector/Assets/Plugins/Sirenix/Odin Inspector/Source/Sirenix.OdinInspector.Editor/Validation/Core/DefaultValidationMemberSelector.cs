#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DefaultValidationMemberSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class DefaultValidationMemberSelector : IMemberSelector
    {
        public static readonly DefaultValidationMemberSelector Instance = new DefaultValidationMemberSelector();

        private static Dictionary<Type, List<MemberInfo>> ResultCache = new Dictionary<Type, List<MemberInfo>>(FastTypeComparer.Instance);

        private static readonly object LOCK = new object();

        public IList<MemberInfo> SelectMembers(Type type)
        {
            List<MemberInfo> result;

            lock (LOCK)
            {
                if (!ResultCache.TryGetValue(type, out result))
                {
                    result = ScanForMembers(type);
                    ResultCache[type] = result;
                }
            }

            return result;
        }

        private static List<MemberInfo> ScanForMembers(Type type)
        {
            List<MemberInfo> result = new List<MemberInfo>();

            foreach (var member in type.GetAllMembers(Flags.AllMembers))
            {
                if (member.DeclaringType == typeof(UnityEngine.Object))
                    continue;

                if (member is FieldInfo)
                {
                    var field = member as FieldInfo;

                    if (field.IsStatic && !field.IsDefined<ShowInInspectorAttribute>())
                        continue;

                    result.Add(member);
                }
                else if (member is PropertyInfo)
                {
                    var prop = member as PropertyInfo;

                    if (prop.IsStatic() && !prop.IsDefined<ShowInInspectorAttribute>())
                        continue;

                    result.Add(prop);
                }
            }

            return result;
        }
    }
}
#endif