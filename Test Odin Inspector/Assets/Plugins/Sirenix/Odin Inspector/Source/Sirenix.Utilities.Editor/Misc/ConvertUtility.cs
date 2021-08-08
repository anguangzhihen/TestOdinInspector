#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ConvertUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    public static class ConvertUtility
    {
        private static readonly DoubleLookupDictionary<Type, Type, object> StrongCastLookup = new DoubleLookupDictionary<Type, Type, object>(FastTypeComparer.Instance, FastTypeComparer.Instance);
        private static readonly DoubleLookupDictionary<Type, Type, Func<object, object>> WeakCastLookup = new DoubleLookupDictionary<Type, Type, Func<object, object>>(FastTypeComparer.Instance, FastTypeComparer.Instance);

        public static bool CanConvert<TFrom, TTo>()
        {
            return CanConvert(typeof(TFrom), typeof(TTo));
        }

        public static bool CanConvert(Type from, Type to)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            if (from == to)
            {
                return true;
            }

            if (to == typeof(string))
            {
                return true;
            }

            if (from.IsCastableTo(to))
            {
                return true;
            }

            if (GenericNumberUtility.IsNumber(from) && GenericNumberUtility.IsNumber(to))
            {
                return true;
            }

            return GetCastDelegate(from, to) != null;
        }

        public static bool TryConvert<TFrom, TTo>(TFrom value, out TTo result)
        {
            if (value is TTo)
            {
                result = (TTo)(object)value;
                return true;
            }

            if (typeof(TTo) == typeof(string))
            {
                result = value != null ? (TTo)(object)value.ToString() : default(TTo);
                return true;
            }

            if (GenericNumberUtility.IsNumber(typeof(TFrom)) && GenericNumberUtility.IsNumber(typeof(TTo)))
            {
                result = GenericNumberUtility.ConvertNumber<TTo>(value);
                return true;
            }

            var cast = GetCastDelegate<TFrom, TTo>();

            if (cast == null)
            {
                result = default(TTo);
                return false;
            }

            result = cast(value);
            return true;
        }

        public static TTo Convert<TFrom, TTo>(TFrom value)
        {
            if (value is TTo)
            {
                return (TTo)(object)value;
            }

            if (typeof(TTo) == typeof(string))
            {
                return value != null ? (TTo)(object)value.ToString() : default(TTo);
            }

            if (GenericNumberUtility.IsNumber(typeof(TFrom)) && GenericNumberUtility.IsNumber(typeof(TTo)))
            {
                return GenericNumberUtility.ConvertNumber<TTo>(value);
            }

            var cast = GetCastDelegate<TFrom, TTo>();

            if (cast == null)
            {
                throw new InvalidCastException();
            }

            return cast(value);
        }

        public static object WeakConvert(object value, Type to)
        {
            if (value == null)
            {
                if (to.IsValueType)
                {
                    return Activator.CreateInstance(to);
                }

                return null;
            }

            var typeOfValue = value.GetType();

            if (to.IsAssignableFrom(typeOfValue))
            {
                return value;
            }

            if (to == typeof(string))
            {
                return value.ToString();
            }

            if (GenericNumberUtility.IsNumber(typeOfValue) && GenericNumberUtility.IsNumber(to))
            {
                return GenericNumberUtility.ConvertNumberWeak(value, to);
            }

            var cast = GetCastDelegate(typeOfValue, to);

            if (cast == null)
            {
                throw new InvalidCastException();
            }

            return cast(value);
        }

        public static T Convert<T>(object value)
        {
            if (value is T)
            {
                return (T)value;
            }

            if (value == null)
            {
                return default(T);
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)value.ToString();
            }

            var typeOfValue = value.GetType();

            if (GenericNumberUtility.IsNumber(typeOfValue) && GenericNumberUtility.IsNumber(typeof(T)))
            {
                return GenericNumberUtility.ConvertNumber<T>(value);
            }

            var cast = GetCastDelegate(typeOfValue, typeof(T));

            if (cast == null)
            {
                throw new InvalidCastException();
            }

            return (T)cast(value);
        }

        public static bool TryConvert<T>(object value, out T result)
        {
            if (value is T)
            {
                result = (T)value;
                return true;
            }

            if (value == null)
            {
                result = default(T);
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)value.ToString();
                return true;
            }

            var typeOfValue = value.GetType();

            if (GenericNumberUtility.IsNumber(typeOfValue) && GenericNumberUtility.IsNumber(typeof(T)))
            {
                result = GenericNumberUtility.ConvertNumber<T>(value);
                return true;
            }

            var cast = GetCastDelegate(typeOfValue, typeof(T));

            if (cast == null)
            {
                result = default(T);
                return false;
            }

            result = (T)cast(value);
            return true;
        }

        private static Func<object, object> GetCastDelegate(Type from, Type to)
        {
            Func<object, object> castDelegate;
            if (!WeakCastLookup.TryGetInnerValue(from, to, out castDelegate))
            {
                castDelegate = TypeExtensions.GetCastMethodDelegate(from, to);
                WeakCastLookup.AddInner(from, to, castDelegate);
            }
            return castDelegate;
        }

        private static Func<TFrom, TTo> GetCastDelegate<TFrom, TTo>()
        {
            object del;
            Func<TFrom, TTo> castDelegate;
            if (!StrongCastLookup.TryGetInnerValue(typeof(TFrom), typeof(TTo), out del))
            {
                castDelegate = TypeExtensions.GetCastMethodDelegate<TFrom, TTo>();
                StrongCastLookup.AddInner(typeof(TFrom), typeof(TTo), castDelegate);
            }
            else
            {
                castDelegate = (Func<TFrom, TTo>)del;
            }

            return castDelegate;
        }
    }
}
#endif