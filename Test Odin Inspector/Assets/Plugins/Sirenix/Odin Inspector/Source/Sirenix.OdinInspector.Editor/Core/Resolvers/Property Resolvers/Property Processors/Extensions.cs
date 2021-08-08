#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Utilities;

    public static class ProcessedMemberPropertyResolverExtensions
    {
        #region AddValue overloads

        public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter)
        {
            AddValue(infos, name, getter, setter, 0, SerializationBackend.None, null);
        }

        public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter, params Attribute[] attributes)
        {
            AddValue(infos, name, getter, setter, 0, SerializationBackend.None, attributes);
        }

        public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter, float order = 0, SerializationBackend backend = null)
        {
            AddValue(infos, name, getter, setter, 0, SerializationBackend.None, null);
        }

        public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name, ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter, float order = 0, SerializationBackend backend = null, params Attribute[] attributes)
        {
            infos.Add(InspectorPropertyInfo.CreateValue(name, order, backend, new GetterSetter<TOwner, TValue>(getter, setter), attributes));
        }

        public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter)
        {
            AddValue<TValue>(infos, name, getter, setter, 0, SerializationBackend.None, null);
        }

        public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter, params Attribute[] attributes)
        {
            AddValue<TValue>(infos, name, getter, setter, 0, SerializationBackend.None, attributes);
        }

        public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter, float order = 0, SerializationBackend backend = null)
        {
            AddValue<TValue>(infos, name, getter, setter, 0, SerializationBackend.None, null);
        }

        public static void AddValue<TValue>(this IList<InspectorPropertyInfo> infos, string name, Func<TValue> getter, Action<TValue> setter, float order = 0, SerializationBackend backend = null, params Attribute[] attributes)
        {
            infos.Add(InspectorPropertyInfo.CreateValue(name, order, backend, new GetterSetter<object, TValue>(getter, setter), attributes));
        }

        #endregion AddValue overloads

        #region AddDelegate overloads (whew!)

        public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Action @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1>(this IList<InspectorPropertyInfo> infos, string name, Action<T1> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1, T2>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1, T2, T3>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1, T2, T3, T4>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3, T4> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<TResult> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, TResult> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1, T2, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, TResult> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1, T2, T3, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, TResult> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate<T1, T2, T3, T4, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, T4, TResult> @delegate)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, null);
        }

        public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Action @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1>(this IList<InspectorPropertyInfo> infos, string name, Action<T1> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1, T2>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1, T2, T3>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1, T2, T3, T4>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3, T4> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<TResult> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, TResult> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1, T2, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, TResult> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1, T2, T3, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, TResult> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate<T1, T2, T3, T4, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, T4, TResult> @delegate, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, 0, attributes);
        }

        public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Action @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1>(this IList<InspectorPropertyInfo> infos, string name, Action<T1> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1, T2>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1, T2, T3>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1, T2, T3, T4>(this IList<InspectorPropertyInfo> infos, string name, Action<T1, T2, T3, T4> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<TResult> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, TResult> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1, T2, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, TResult> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1, T2, T3, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, TResult> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate<T1, T2, T3, T4, TResult>(this IList<InspectorPropertyInfo> infos, string name, Func<T1, T2, T3, T4, TResult> @delegate, float order = 0, params Attribute[] attributes)
        {
            AddDelegate(infos, name, (Delegate)@delegate, order, attributes);
        }

        public static void AddDelegate(this IList<InspectorPropertyInfo> infos, string name, Delegate @delegate, float order = 0, params Attribute[] attributes)
        {
            infos.Add(InspectorPropertyInfo.CreateForDelegate(name, order, typeof(object), (Delegate)@delegate, attributes));
        }

        #endregion AddDelegate overloads (whew!)

        #region AddMember
        public static void AddMember(this IList<InspectorPropertyInfo> infos, MemberInfo member)
        {
            AddMember(infos, member, true, SerializationBackend.None, null);
        }

        public static void AddMember(this IList<InspectorPropertyInfo> infos, MemberInfo member, params Attribute[] attributes)
        {
            AddMember(infos, member, true, SerializationBackend.None, attributes);
        }

        public static void AddMember(this IList<InspectorPropertyInfo> infos, MemberInfo member, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
        {
            infos.Add(InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, attributes ?? new Attribute[0]));
        }

        public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, MemberInfo member)
        {
            AddProcessedMember(infos, parentProperty, member, true, SerializationBackend.None, null);
        }

        public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, MemberInfo member, params Attribute[] attributes)
        {
            AddProcessedMember(infos, parentProperty, member, true, SerializationBackend.None, attributes);
        }

        public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, MemberInfo member, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
        {
            var list = new List<Attribute>();
            if (attributes != null) list.AddRange(attributes);
            InspectorPropertyInfoUtility.ProcessAttributes(parentProperty, member, list);
            infos.Add(InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, list));
        }

        public static Type ProcessingOwnerType { get; set; }

        public static void AddMember(this IList<InspectorPropertyInfo> infos, string name)
        {
            AddMember(infos, name, true, SerializationBackend.None, null);
        }

        public static void AddMember(this IList<InspectorPropertyInfo> infos, string name, params Attribute[] attributes)
        {
            AddMember(infos, name, true, SerializationBackend.None, attributes);
        }

        public static void AddMember(this IList<InspectorPropertyInfo> infos, string name, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
        {
            var members = ProcessingOwnerType.GetMember(name, MemberTypes.Field | MemberTypes.Method | MemberTypes.Property, Flags.AllMembers);

            if (members.Length == 0 || members.Length > 1)
            {
                throw new ArgumentException("Could not find precisely 1 member on type '" + ProcessingOwnerType.GetNiceName() + "' with name '" + name + "'; found " + members.Length + " members.");
            }

            AddMember(infos, members[0], true, SerializationBackend.None, attributes ?? new Attribute[0]);
        }

        public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, string name)
        {
            AddProcessedMember(infos, parentProperty, name, true, SerializationBackend.None, null);
        }

        public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, string name, params Attribute[] attributes)
        {
            AddProcessedMember(infos, parentProperty, name, true, SerializationBackend.None, attributes);
        }

        public static void AddProcessedMember(this IList<InspectorPropertyInfo> infos, InspectorProperty parentProperty, string name, bool allowEditable = true, SerializationBackend backend = null, params Attribute[] attributes)
        {
            var type = ProcessingOwnerType;

            type = (parentProperty.ValueEntry != null) ? parentProperty.ValueEntry.TypeOfValue : ProcessingOwnerType;

            var members = type.GetMember(name, MemberTypes.Field | MemberTypes.Method | MemberTypes.Property, Flags.AllMembers);

            if (members.Length == 0 || members.Length > 1)
            {
                throw new ArgumentException("Could not find precisely 1 member on type '" + type.GetNiceName() + "' with name '" + name + "'; found " + members.Length + " members.");
            }

            var list = new List<Attribute>();
            if (attributes != null) list.AddRange(attributes);
            InspectorPropertyInfoUtility.ProcessAttributes(parentProperty, members[0], list);
            infos.Add(InspectorPropertyInfo.CreateForMember(members[0], allowEditable, backend, list));
        }

        #endregion AddMember

        public static bool Remove(this IList<InspectorPropertyInfo> infos, string name)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].PropertyName == name)
                {
                    infos.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public static InspectorPropertyInfo Find(this IList<InspectorPropertyInfo> infos, string name)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].PropertyName == name)
                {
                    return infos[i];
                }
            }

            return null;
        }
    }
}
#endif