#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityTypeCacheUtility.cs" company="Sirenix IVS">
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

    public static class UnityTypeCacheUtility
    {
        public static readonly bool IsAvailable;
        private static readonly MethodInfo UnityEditor_TypeCache_GetTypesDerivedFrom_Method;

        static UnityTypeCacheUtility()
        {
            var typeCacheType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TypeCache");

            if (typeCacheType != null)
            {
                UnityEditor_TypeCache_GetTypesDerivedFrom_Method = typeCacheType.GetMethod("GetTypesDerivedFrom", Flags.StaticPublic, null, new Type[] { typeof(Type) }, null);

                if (UnityEditor_TypeCache_GetTypesDerivedFrom_Method != null)
                {
                    IsAvailable = true;
                }
            }
        }

        public static IList<Type> GetTypesDerivedFrom(Type type)
        {
            if (!IsAvailable) throw new NotSupportedException();
            return (IList<Type>)UnityEditor_TypeCache_GetTypesDerivedFrom_Method.Invoke(null, new object[] { type });
        }
    }
}
#endif