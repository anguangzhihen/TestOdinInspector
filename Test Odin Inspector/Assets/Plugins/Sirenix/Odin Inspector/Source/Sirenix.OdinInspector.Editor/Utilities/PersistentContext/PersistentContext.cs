#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PersistentContext.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Provides context objects that still persist when Unity reloads or is restarted.
    /// </summary>
    public static class PersistentContext
    {
        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified key.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static GlobalPersistentContext<TValue> Get<TKey1, TValue>(TKey1 alphaKey, TValue defaultValue)
        {
            bool isNew;
            GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), 0, 0, 0, 0, out isNew);

            if (isNew)
            {
                context.Value = defaultValue;
            }

            return context;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TValue>(TKey1 alphaKey, TKey2 betaKey, TValue defaultValue)
        {
            bool isNew;
            GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), 0, 0, 0, out isNew);

            if (isNew)
            {
                context.Value = defaultValue;
            }

            return context;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TKey3, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TValue defaultValue)
        {
            bool isNew;
            GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), gammaKey.GetHashCode(), 0, 0, out isNew);

            if (isNew)
            {
                context.Value = defaultValue;
            }

            return context;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TKey3, TKey4, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TValue defaultValue)
        {
            bool isNew;
            GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), gammaKey.GetHashCode(), deltaKey.GetHashCode(), 0, out isNew);

            if (isNew)
            {
                context.Value = defaultValue;
            }

            return context;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TKey5">The type of the fifth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="epsilonKey">The fifth key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TKey5 epsilonKey, TValue defaultValue)
        {
            bool isNew;
            GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), gammaKey.GetHashCode(), deltaKey.GetHashCode(), epsilonKey.GetHashCode(), out isNew);

            if (isNew)
            {
                context.Value = defaultValue;
            }

            return context;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified key.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool Get<TKey1, TValue>(TKey1 alphaKey, out GlobalPersistentContext<TValue> context)
        {
            bool isNew;
            context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), 0, 0, 0, 0, out isNew);

            return isNew;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool Get<TKey1, TKey2, TValue>(TKey1 alphaKey, TKey2 betaKey, out GlobalPersistentContext<TValue> context)
        {
            bool isNew;
            context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), 0, 0, 0, out isNew);

            return isNew;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool Get<TKey1, TKey2, TKey3, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, out GlobalPersistentContext<TValue> context)
        {
            bool isNew;
            context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), gammaKey.GetHashCode(), 0, 0, out isNew);

            return isNew;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool Get<TKey1, TKey2, TKey3, TKey4, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, out GlobalPersistentContext<TValue> context)
        {
            bool isNew;
            context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), gammaKey.GetHashCode(), deltaKey.GetHashCode(), 0, out isNew);

            return isNew;
        }

        /// <summary>
        /// Gets a GlobalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TKey5">The type of the fifth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="epsilonKey">The fifth key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool Get<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TKey5 epsilonKey, out GlobalPersistentContext<TValue> context)
        {
            bool isNew;
            context = PersistentContextCache.Instance.GetContext<TValue>(alphaKey.GetHashCode(), betaKey.GetHashCode(), gammaKey.GetHashCode(), deltaKey.GetHashCode(), epsilonKey.GetHashCode(), out isNew);

            return isNew;
        }

        /// <summary>
		/// Gets a LocalPersistentContext object for the specified key.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="defaultValue">The default value, used for when the context object is first created.</param>
		public static LocalPersistentContext<TValue> GetLocal<TKey1, TValue>(TKey1 alphaKey, TValue defaultValue)
        {
            return LocalPersistentContext<TValue>.Create(Get<TKey1, TValue>(alphaKey, defaultValue));
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static LocalPersistentContext<TValue> GetLocal<TKey1, TKey2, TValue>(TKey1 alphaKey, TKey2 betaKey, TValue defaultValue)
        {
            return LocalPersistentContext<TValue>.Create(Get<TKey1, TKey2, TValue>(alphaKey, betaKey, defaultValue));
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static LocalPersistentContext<TValue> GetLocal<TKey1, TKey2, TKey3, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TValue defaultValue)
        {
            return LocalPersistentContext<TValue>.Create(Get<TKey1, TKey2, TKey3, TValue>(alphaKey, betaKey, gammaKey, defaultValue));
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static LocalPersistentContext<TValue> GetLocal<TKey1, TKey2, TKey3, TKey4, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TValue defaultValue)
        {
            return LocalPersistentContext<TValue>.Create(Get<TKey1, TKey2, TKey3, TKey4, TValue>(alphaKey, betaKey, gammaKey, deltaKey, defaultValue));
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TKey5">The type of the fifth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="epsilonKey">The fifth key.</param>
        /// <param name="defaultValue">The default value, used for when the context object is first created.</param>
        public static LocalPersistentContext<TValue> GetLocal<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TKey5 epsilonKey, TValue defaultValue)
        {
            return LocalPersistentContext<TValue>.Create(Get<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(alphaKey, betaKey, gammaKey, deltaKey, epsilonKey, defaultValue));
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified key.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool GetLocal<TKey1, TValue>(TKey1 alphaKey, out LocalPersistentContext<TValue> context)
        {
            GlobalPersistentContext<TValue> global;
            bool isNew = Get<TKey1, TValue>(alphaKey, out global);
            context = LocalPersistentContext<TValue>.Create(global);

            return isNew;
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool GetLocal<TKey1, TKey2, TValue>(TKey1 alphaKey, TKey2 betaKey, out LocalPersistentContext<TValue> context)
        {
            GlobalPersistentContext<TValue> global;
            bool isNew = Get(alphaKey, betaKey, out global);
            context = LocalPersistentContext<TValue>.Create(global);

            return isNew;
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool GetLocal<TKey1, TKey2, TKey3, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, out LocalPersistentContext<TValue> context)
        {
            GlobalPersistentContext<TValue> global;
            bool isNew = Get(alphaKey, betaKey, gammaKey, out global);
            context = LocalPersistentContext<TValue>.Create(global);

            return isNew;
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool GetLocal<TKey1, TKey2, TKey3, TKey4, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, out LocalPersistentContext<TValue> context)
        {
            GlobalPersistentContext<TValue> global;
            bool isNew = Get(alphaKey, betaKey, gammaKey, deltaKey, out global);
            context = LocalPersistentContext<TValue>.Create(global);

            return isNew;
        }

        /// <summary>
        /// Gets a LocalPersistentContext object for the specified keys.
        /// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TKey1">The type of the first key.</typeparam>
        /// <typeparam name="TKey2">The type of the second key.</typeparam>
        /// <typeparam name="TKey3">The type of the third key.</typeparam>
        /// <typeparam name="TKey4">The type of the fourth key.</typeparam>
        /// <typeparam name="TKey5">The type of the fifth key.</typeparam>
        /// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
        /// <param name="alphaKey">The first key.</param>
        /// <param name="betaKey">The second key.</param>
        /// <param name="gammaKey">The third key.</param>
        /// <param name="deltaKey">The fourth key.</param>
        /// <param name="epsilonKey">The fifth key.</param>
        /// <param name="context">The persistent context object.</param>
        /// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
        public static bool GetLocal<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TKey5 epsilonKey, out LocalPersistentContext<TValue> context)
        {
            GlobalPersistentContext<TValue> global;
            bool isNew = Get(alphaKey, betaKey, gammaKey, deltaKey, epsilonKey, out global);
            context = LocalPersistentContext<TValue>.Create(global);

            return isNew;
        }
    }
}
#endif