#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AtomHandlerLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using Sirenix.Utilities;
    using System.Collections.Generic;
    using UnityEngine;

    public static class AtomHandlerLocator
    {
        private static readonly Dictionary<Type, Type> AtomHandlerTypes = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, IAtomHandler> AtomHandlers = new Dictionary<Type, IAtomHandler>();

        static AtomHandlerLocator()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];

                if (!assembly.SafeIsDefined(typeof(AtomContainerAttribute), false)) continue;

                var types = assembly.SafeGetTypes();

                for (int j = 0; j < types.Length; j++)
                {
                    var handler = types[j];

                    if (!typeof(IAtomHandler).IsAssignableFrom(handler)) continue;
                    if (handler.IsAbstract) continue;
                    if (!handler.IsDefined(typeof(AtomHandlerAttribute), false)) continue;
                    if (handler.GetConstructor(Type.EmptyTypes) == null) continue;

                    var args = handler.GetArgumentsOfInheritedOpenGenericInterface(typeof(IAtomHandler<>));

                    if (args == null) continue;

                    var atomicType = args[0];

                    if (atomicType.IsAbstract)
                    {
                        Debug.LogError("The type '" + atomicType.GetNiceName() + "' cannot be marked atomic, as it is abstract.");
                        continue;
                    }

                    AtomHandlerTypes.Add(atomicType, handler);
                }
            }
        }

        public static bool IsMarkedAtomic(this Type type)
        {
            return AtomHandlerTypes.ContainsKey(type);
        }

        public static IAtomHandler GetAtomHandler(Type type)
        {
            if (!AtomHandlerTypes.ContainsKey(type))
            {
                return null;
            }

            IAtomHandler result;

            if (!AtomHandlers.TryGetValue(type, out result))
            {
                result = (IAtomHandler)Activator.CreateInstance(AtomHandlerTypes[type]);
                AtomHandlers[type] = result;
            }

            return result;
        }

        public static IAtomHandler<T> GetAtomHandler<T>()
        {
            return (IAtomHandler<T>)GetAtomHandler(typeof(T));
        }
    }
}
#endif