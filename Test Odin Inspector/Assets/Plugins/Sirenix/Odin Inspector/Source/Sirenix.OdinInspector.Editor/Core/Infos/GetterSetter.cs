#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GetterSetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    internal static class GetterSetterCaches<TOwner>
    {
        public static readonly DoubleLookupDictionary<MemberInfo, Type, Delegate> Getters = new DoubleLookupDictionary<MemberInfo, Type, Delegate>(FastMemberComparer.Instance, FastTypeComparer.Instance);
        public static readonly DoubleLookupDictionary<MemberInfo, Type, Delegate> Setters = new DoubleLookupDictionary<MemberInfo, Type, Delegate>(FastMemberComparer.Instance, FastTypeComparer.Instance);
    }

    /// <summary>
    /// Responsible for getting and setting values on properties.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="Sirenix.OdinInspector.Editor.IValueGetterSetter{TOwner, TValue}" />
    public class GetterSetter<TOwner, TValue> : IValueGetterSetter<TOwner, TValue>
    {
        private ValueGetter<TOwner, TValue> getter;
        private ValueSetter<TOwner, TValue> setter;
        private Func<TValue> staticGetter;
        private Action<TValue> staticSetter;

        /// <summary>
        /// Whether the value is readonly.
        /// </summary>
        public bool IsReadonly { get { return this.setter == null && this.staticSetter == null; } }

        /// <summary>
        /// Gets the type of the owner.
        /// </summary>
        public Type OwnerType { get { return typeof(TOwner); } }

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        public Type ValueType { get { return typeof(TValue); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetterSetter{TOwner, TValue}"/> class.
        /// </summary>
        /// <param name="memberInfo">The field member to represent.</param>
        /// <param name="isReadOnly">if set to <c>true</c> [is readonly].</param>
        public GetterSetter(MemberInfo memberInfo, bool isReadOnly)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            if (memberInfo.IsStatic())
            {
                this.staticGetter = GetCachedStaticGetter(memberInfo);

                if (!isReadOnly)
                {
                    this.staticSetter = GetCachedStaticSetter(memberInfo);
                }
            }
            else
            {
                this.getter = GetCachedGetter(memberInfo);

                if (!isReadOnly)
                {
                    this.setter = GetCachedSetter(memberInfo);
                }
            }
        }

        private static ValueGetter<TOwner, TValue> GetCachedGetter(MemberInfo member)
        {
            Delegate del;

            if (GetterSetterCaches<TOwner>.Getters.TryGetInnerValue(member, typeof(TValue), out del))
            {
                return (ValueGetter<TOwner, TValue>)del;
            }
            else
            {
                ValueGetter<TOwner, TValue> result;

                var fieldInfo = member as FieldInfo;
                var propertyInfo = member as PropertyInfo;

                if (fieldInfo != null)
                {
                    result = EmitUtilities.CreateInstanceFieldGetter<TOwner, TValue>(fieldInfo);
                }
                else if (propertyInfo != null)
                {
                    result = EmitUtilities.CreateInstancePropertyGetter<TOwner, TValue>(propertyInfo);
                }
                else
                {
                    throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
                }

                GetterSetterCaches<TOwner>.Getters.AddInner(member, typeof(TValue), result);
                return result;
            }
        }

        private static ValueSetter<TOwner, TValue> GetCachedSetter(MemberInfo member)
        {
            Delegate del;

            if (GetterSetterCaches<TOwner>.Setters.TryGetInnerValue(member, typeof(TValue), out del))
            {
                return (ValueSetter<TOwner, TValue>)del;
            }
            else
            {
                ValueSetter<TOwner, TValue> result = null;

                var fieldInfo = member as FieldInfo;
                var propertyInfo = member as PropertyInfo;

                if (fieldInfo != null)
                {
                    if (!fieldInfo.IsLiteral)
                    {
                        result = EmitUtilities.CreateInstanceFieldSetter<TOwner, TValue>(fieldInfo);
                    }
                }
                else if (propertyInfo != null)
                {
                    if (propertyInfo.CanWrite)
                    {
                        result = EmitUtilities.CreateInstancePropertySetter<TOwner, TValue>(propertyInfo);
                    }
                }
                else
                {
                    throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
                }

                GetterSetterCaches<TOwner>.Setters.AddInner(member, typeof(TValue), result);
                return result;
            }
        }

        private static Func<TValue> GetCachedStaticGetter(MemberInfo member)
        {
            Delegate del;

            if (GetterSetterCaches<TOwner>.Getters.TryGetInnerValue(member, typeof(TValue), out del))
            {
                return (Func<TValue>)del;
            }
            else
            {
                Func<TValue> result;

                var fieldInfo = member as FieldInfo;
                var propertyInfo = member as PropertyInfo;

                if (fieldInfo != null)
                {
                    result = EmitUtilities.CreateStaticFieldGetter<TValue>(fieldInfo);
                }
                else if (propertyInfo != null)
                {
                    result = EmitUtilities.CreateStaticPropertyGetter<TValue>(propertyInfo);
                }
                else
                {
                    throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
                }

                GetterSetterCaches<TOwner>.Getters.AddInner(member, typeof(TValue), result);
                return result;
            }
        }

        private static Action<TValue> GetCachedStaticSetter(MemberInfo member)
        {
            Delegate del;

            if (GetterSetterCaches<TOwner>.Setters.TryGetInnerValue(member, typeof(TValue), out del))
            {
                return (Action<TValue>)del;
            }
            else
            {
                Action<TValue> result = null;

                var fieldInfo = member as FieldInfo;
                var propertyInfo = member as PropertyInfo;

                if (fieldInfo != null)
                {
                    if (!fieldInfo.IsLiteral)
                    {
                        result = EmitUtilities.CreateStaticFieldSetter<TValue>(fieldInfo);
                    }
                }
                else if (propertyInfo != null)
                {
                    if (propertyInfo.CanWrite)
                    {
                        result = EmitUtilities.CreateStaticPropertySetter<TValue>(propertyInfo);
                    }
                }
                else
                {
                    throw new ArgumentException("Cannot create a GetterSetter for a member of type + " + member.GetType().Name + "!");
                }

                GetterSetterCaches<TOwner>.Setters.AddInner(member, typeof(TValue), result);
                return result;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetterSetter{TOwner, TValue}"/> class.
        /// </summary>
        /// <param name="getter">The getter.</param>
        /// <param name="setter">The setter.</param>
        /// <exception cref="ArgumentNullException">getter</exception>
        public GetterSetter(ValueGetter<TOwner, TValue> getter, ValueSetter<TOwner, TValue> setter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException("getter");
            }

            this.getter = getter;
            this.setter = setter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetterSetter{TOwner, TValue}"/> class.
        /// </summary>
        /// <param name="getter">The getter.</param>
        /// <param name="setter">The setter.</param>
        /// <exception cref="ArgumentNullException">getter</exception>
        public GetterSetter(Func<TValue> getter, Action<TValue> setter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException("getter");
            }

            this.getter = (ref TOwner owner) => getter();

            if (setter != null)
            {
                this.setter = (ref TOwner owner, TValue value) => setter(value);
            }
        }

        /// <summary>
        /// Gets the value from a given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The found value.</returns>
        /// <exception cref="System.ArgumentNullException">owner is null</exception>
        public TValue GetValue(ref TOwner owner)
        {
            return
                this.getter != null ? this.getter(ref owner) :
                this.staticGetter();
        }

        /// <summary>
        /// Gets the value from a given weakly typed owner.
        /// </summary>
        /// <param name="owner">The weakly typed owner.</param>
        /// <returns>The found value.</returns>
        public object GetValue(object owner)
        {
            TOwner castOwner = (TOwner)owner;
            return this.GetValue(ref castOwner);
        }

        /// <summary>
        /// Sets the weakly typed value on a given weakly typed owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        public void SetValue(ref TOwner owner, TValue value)
        {
            if (this.IsReadonly)
            {
                Debug.LogError("Tried to set a value on a readonly getter setter!");
                return;
            }

            if (this.setter != null)
            {
                this.setter(ref owner, value);
            }
            else if (this.staticSetter != null)
            {
                this.staticSetter(value);
            }
            else
            {
                Debug.Log("WTF TOR!?");
            }
        }

        /// <summary>
        /// Sets the value on a given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        public void SetValue(object owner, object value)
        {
            TOwner castOwner = (TOwner)owner;
            this.SetValue(ref castOwner, (TValue)value);
        }
    }
}
#endif