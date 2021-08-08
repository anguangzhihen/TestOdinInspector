//-----------------------------------------------------------------------
// <copyright file="DeepReflection.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if NET_STANDARD_2_0
#error Odin Inspector is incapable of compiling source code against the .NET Standard 2.0 API surface. You can change the API Compatibility Level in the Player settings.
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE) && !ENABLE_IL2CPP
#define CAN_EMIT
#endif

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

#if CAN_EMIT

    using System.Reflection.Emit;

#endif

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public static class DeepReflection
    {
        private enum PathStepType
        {
            Member,
            WeakListElement,
            StrongListElement,
            ArrayElement
        }

        private struct PathStep
        {
            public readonly PathStepType StepType;
            public readonly MemberInfo Member;
            public readonly int ElementIndex;
            public readonly Type ElementType;
            public readonly MethodInfo StrongListGetItemMethod;

            public PathStep(MemberInfo member)
            {
                this.StepType = PathStepType.Member;
                this.Member = member;
                this.ElementIndex = -1;
                this.ElementType = null;
                this.StrongListGetItemMethod = null;
            }

            public PathStep(int elementIndex)
            {
                this.StepType = PathStepType.WeakListElement;
                this.Member = null;
                this.ElementIndex = elementIndex;
                this.ElementType = null;
                this.StrongListGetItemMethod = null;
            }

            public PathStep(int elementIndex, Type strongListElementType, bool isArray)
            {
                this.StepType = isArray ? PathStepType.ArrayElement : PathStepType.StrongListElement;
                this.Member = null;
                this.ElementIndex = elementIndex;
                this.ElementType = strongListElementType;
                this.StrongListGetItemMethod = typeof(IList<>).MakeGenericType(strongListElementType).GetMethod("get_Item");
            }
        }

        private static MethodInfo WeakListGetItem = typeof(IList).GetMethod("get_Item");
        private static MethodInfo WeakListSetItem = typeof(IList).GetMethod("set_Item");

#pragma warning disable 0414
        private static MethodInfo CreateWeakAliasForInstanceGetDelegate1MethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForInstanceGetDelegate1", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo CreateWeakAliasForInstanceGetDelegate2MethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForInstanceGetDelegate2", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo CreateWeakAliasForStaticGetDelegateMethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForStaticGetDelegate", BindingFlags.NonPublic | BindingFlags.Static);

        private static MethodInfo CreateWeakAliasForInstanceSetDelegate1MethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForInstanceSetDelegate1", BindingFlags.NonPublic | BindingFlags.Static);
        //private static MethodInfo CreateWeakAliasForInstanceSetDelegate2MethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForInstanceSetDelegate2", BindingFlags.NonPublic | BindingFlags.Static);
        //private static MethodInfo CreateWeakAliasForStaticSetDelegateMethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForStaticSetDelegate", BindingFlags.NonPublic | BindingFlags.Static);
#pragma warning restore 0414

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Func<object> CreateWeakStaticValueGetter(Type rootType, Type resultType, string path, bool allowEmit = true)
        {
            if (rootType == null)
            {
                throw new ArgumentNullException("rootType");
            }

            bool rootIsStatic;
            var memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);

            if (rootIsStatic == false)
            {
                throw new ArgumentException("Given path root is not static.");
            }

#if !CAN_EMIT
            return CreateSlowDeepStaticValueGetterDelegate(memberPath);
#else
            if (!allowEmit)
            {
                return CreateSlowDeepStaticValueGetterDelegate(memberPath);
            }

            Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);

            MethodInfo weakAliasCreator = CreateWeakAliasForStaticGetDelegateMethodInfo.MakeGenericMethod(resultType);
            Func<object> result = (Func<object>)weakAliasCreator.Invoke(null, new object[] { emittedDelegate });

            return result;
#endif
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Func<object, object> CreateWeakInstanceValueGetter(Type rootType, Type resultType, string path, bool allowEmit = true)
        {
            if (rootType == null)
            {
                throw new ArgumentNullException("rootType");
            }

            bool rootIsStatic;
            var memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);

            if (rootIsStatic)
            {
                throw new ArgumentException("Given path root is static.");
            }

#if !CAN_EMIT
            return CreateSlowDeepInstanceValueGetterDelegate(memberPath);
#else
            if (!allowEmit)
            {
                return CreateSlowDeepInstanceValueGetterDelegate(memberPath);
            }

            Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);

            MethodInfo weakAliasCreator = CreateWeakAliasForInstanceGetDelegate1MethodInfo.MakeGenericMethod(rootType, resultType);
            Func<object, object> result = (Func<object, object>)weakAliasCreator.Invoke(null, new object[] { emittedDelegate });

            return result;
#endif
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Action<object, object> CreateWeakInstanceValueSetter(Type rootType, Type argType, string path, bool allowEmit = true)
        {
            if (rootType == null)
            {
                throw new ArgumentNullException("rootType");
            }

            bool rootIsStatic;
            var memberPath = GetMemberPath(rootType, ref argType, path, out rootIsStatic, isSet: true);

            if (rootIsStatic)
            {
                throw new ArgumentException("Given path root is static.");
            }

            allowEmit = false; // TODO: (Tor) Implement emit for deep reflection value setters

#if !CAN_EMIT
            return CreateSlowDeepInstanceValueSetterDelegate(memberPath);
#else
            if (!allowEmit)
            {
                return CreateSlowDeepInstanceValueSetterDelegate(memberPath);
            }

            Delegate emittedDelegate = null;//CreateEmittedDeepValueGetterDelegate(path, rootType, argType, memberPath, rootIsStatic);
            MethodInfo weakAliasCreator = CreateWeakAliasForInstanceSetDelegate1MethodInfo.MakeGenericMethod(rootType, argType);
            Action<object, object> result = (Action<object, object>)weakAliasCreator.Invoke(null, new object[] { emittedDelegate });

            return result;
#endif
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Func<object, TResult> CreateWeakInstanceValueGetter<TResult>(Type rootType, string path, bool allowEmit = true)
        {
            if (rootType == null)
            {
                throw new ArgumentNullException("rootType");
            }

            Type resultType = typeof(TResult);
            bool rootIsStatic;
            var memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);

            if (rootIsStatic)
            {
                throw new ArgumentException("Given path root is static.");
            }

#if !CAN_EMIT
            var del = CreateSlowDeepInstanceValueGetterDelegate(memberPath);
            return (obj) => (TResult)del(obj);
#else
            if (!allowEmit)
            {
                var del = CreateSlowDeepInstanceValueGetterDelegate(memberPath);
                return (obj) => (TResult)del(obj);
            }

            Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);

            MethodInfo weakAliasCreator = CreateWeakAliasForInstanceGetDelegate2MethodInfo.MakeGenericMethod(rootType, resultType);
            Func<object, TResult> result = (Func<object, TResult>)weakAliasCreator.Invoke(null, new object[] { emittedDelegate });

            return result;
#endif
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Func<TResult> CreateValueGetter<TResult>(Type rootType, string path, bool allowEmit = true)
        {
            if (rootType == null)
            {
                throw new ArgumentNullException("rootType");
            }

            var resultType = typeof(TResult);
            bool rootIsStatic;
            var memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);

            if (rootIsStatic == false)
            {
                throw new ArgumentException("Given path root is not static; use the generic overload with a target type.");
            }

#if !CAN_EMIT
            var slowDelegate = CreateSlowDeepStaticValueGetterDelegate(memberPath);
            return () => (TResult)slowDelegate();
#else
            if (!allowEmit)
            {
                var slowDelegate = CreateSlowDeepStaticValueGetterDelegate(memberPath);
                return () => (TResult)slowDelegate();
            }

            Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);
            return (Func<TResult>)emittedDelegate;
#endif
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Func<TTarget, TResult> CreateValueGetter<TTarget, TResult>(string path, bool allowEmit = true)
        {
            var resultType = typeof(TResult);
            bool rootIsStatic;
            var memberPath = GetMemberPath(typeof(TTarget), ref resultType, path, out rootIsStatic, isSet: false);

            if (rootIsStatic)
            {
                throw new ArgumentException("Given path root is static; use the generic overload without a target type.");
            }

#if !CAN_EMIT
            var slowDelegate = CreateSlowDeepInstanceValueGetterDelegate(memberPath);
            return (target) => (TResult)slowDelegate(target);
#else
            if (!allowEmit)
            {
                var slowDelegate = CreateSlowDeepInstanceValueGetterDelegate(memberPath);
                return (target) => (TResult)slowDelegate(target);
            }

            Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, typeof(TTarget), resultType, memberPath, rootIsStatic);
            return (Func<TTarget, TResult>)emittedDelegate;
#endif
        }

        private static Func<object, object> CreateWeakAliasForInstanceGetDelegate1<TTarget, TResult>(Func<TTarget, TResult> func)
        {
            return (obj) => func((TTarget)obj);
        }

        private static Func<object, TResult> CreateWeakAliasForInstanceGetDelegate2<TTarget, TResult>(Func<TTarget, TResult> func)
        {
            return (obj) => func((TTarget)obj);
        }

        private static Func<object> CreateWeakAliasForStaticGetDelegate<TResult>(Func<TResult> func)
        {
            return () => func();
        }

        private static Action<object, object> CreateWeakAliasForInstanceSetDelegate1<TTarget, TArg1>(Action<TTarget, TArg1> func)
        {
            return (obj, arg) => func((TTarget)obj, (TArg1)arg);
        }

        private static Action<object, TArg1> CreateWeakAliasForInstanceSetDelegate2<TTarget, TArg1>(Action<TTarget, TArg1> func)
        {
            return (obj, arg) => func((TTarget)obj, (TArg1)arg);
        }

        private static Action<object> CreateWeakAliasForStaticSetDelegate<TArg1>(Action<TArg1> func)
        {
            return (arg) => func((TArg1)arg);
        }

        private static Delegate CreateEmittedDeepValueGetterDelegate(string path, Type rootType, Type resultType, List<PathStep> memberPath, bool rootIsStatic)
        {
#if !CAN_EMIT
            throw new NotSupportedException("Emitting is not supported on the current platform.");
#else

            DynamicMethod getterMethod;

            if (rootIsStatic)
            {
                getterMethod = new DynamicMethod(rootType.FullName + "_getter<" + path + ">", resultType, new Type[0], true);
            }
            else
            {
                getterMethod = new DynamicMethod(rootType.FullName + "_getter<" + path + ">", resultType, new Type[] { rootType }, true);
            }

            ILGenerator il = getterMethod.GetILGenerator();

            if (rootIsStatic == false)
            {
                // Load root instance from method argument
                il.Emit(OpCodes.Ldarg_0);
            }

            for (int i = 0; i < memberPath.Count; i++)
            {
                var step = memberPath[i];

                switch (step.StepType)
                {
                    case PathStepType.Member:
                        {
                            MemberInfo member = step.Member;

                            // Field
                            {
                                FieldInfo field = member as FieldInfo;

                                if (field != null)
                                {
                                    if (field.IsLiteral)
                                    {
                                        EmitConstant(il, field.GetRawConstantValue());
                                    }
                                    else if (field.IsStatic)
                                    {
                                        il.Emit(OpCodes.Ldsfld, field);
                                    }
                                    else
                                    {
                                        il.Emit(OpCodes.Ldfld, field);
                                    }
                                }
                            }

                            // Property
                            {
                                PropertyInfo property = member as PropertyInfo;

                                if (property != null)
                                {
                                    var getMethod = property.GetGetMethod(true);

                                    if (getMethod.IsStatic)
                                    {
                                        il.Emit(OpCodes.Call, getMethod);
                                    }
                                    else if (getMethod.DeclaringType.IsValueType)
                                    {
                                        var localAddr = il.DeclareLocal(getMethod.DeclaringType);

                                        il.Emit(OpCodes.Stloc, localAddr);
                                        il.Emit(OpCodes.Ldloca, localAddr);
                                        il.Emit(OpCodes.Call, getMethod);
                                    }
                                    else
                                    {
                                        il.Emit(OpCodes.Callvirt, getMethod);
                                    }
                                }
                            }

                            // Method
                            {
                                MethodInfo method = member as MethodInfo;

                                if (method != null)
                                {
                                    if (method.IsStatic)
                                    {
                                        il.Emit(OpCodes.Call, method);
                                    }
                                    else if (method.DeclaringType.IsValueType)
                                    {
                                        var localAddr = il.DeclareLocal(method.DeclaringType);

                                        il.Emit(OpCodes.Stloc, localAddr);
                                        il.Emit(OpCodes.Ldloca, localAddr);
                                        il.Emit(OpCodes.Call, method);
                                    }
                                    else
                                    {
                                        il.Emit(OpCodes.Callvirt, method);
                                    }
                                }
                            }

                            Type returnType = member.GetReturnType();
                            if ((resultType == typeof(object) || returnType.IsInterface) && returnType.IsValueType)
                            {
                                il.Emit(OpCodes.Box, returnType);
                            }
                        }
                        break;

                    case PathStepType.ArrayElement:
                        {
                            il.Emit(OpCodes.Ldc_I4, step.ElementIndex);
                            il.Emit(OpCodes.Ldelem, step.ElementType);
                        }
                        break;

                    case PathStepType.WeakListElement:
                        {
                            il.Emit(OpCodes.Ldc_I4, step.ElementIndex);
                            il.Emit(OpCodes.Callvirt, WeakListGetItem);
                        }
                        break;

                    case PathStepType.StrongListElement:
                        {
                            Type strongListType = typeof(IList<>).MakeGenericType(step.ElementType);
                            MethodInfo getItemMethod = strongListType.GetMethod("get_Item");

                            il.Emit(OpCodes.Ldc_I4, step.ElementIndex);
                            il.Emit(OpCodes.Callvirt, getItemMethod);
                        }
                        break;

                    default:
                        break;
                }
            }

            il.Emit(OpCodes.Ret);

            if (rootIsStatic)
            {
                return getterMethod.CreateDelegate(typeof(Func<>).MakeGenericType(resultType));
            }
            else
            {
                return getterMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(rootType, resultType));
            }
#endif
        }

        private static Func<object> CreateSlowDeepStaticValueGetterDelegate(List<PathStep> memberPath)
        {
            return (Func<object>)(() =>
            {
                object currentInstance = null;

                for (int i = 0; i < memberPath.Count; i++)
                {
                    currentInstance = SlowGetMemberValue(memberPath[i], currentInstance);
                }

                return currentInstance;
            });
        }

        private static Func<object, object> CreateSlowDeepInstanceValueGetterDelegate(List<PathStep> memberPath)
        {
            return (Func<object, object>)((object instance) =>
            {
                object currentInstance = instance;

                for (int i = 0; i < memberPath.Count; i++)
                {
                    currentInstance = SlowGetMemberValue(memberPath[i], currentInstance);
                }

                return currentInstance;
            });
        }

        private static Action<object, object> CreateSlowDeepInstanceValueSetterDelegate(List<PathStep> memberPath)
        {
            return (Action<object, object>)((object instance, object arg) =>
            {
                object currentInstance = instance;
                int count = memberPath.Count - 1;

                for (int i = 0; i < count; i++)
                {
                    currentInstance = SlowGetMemberValue(memberPath[i], currentInstance);
                }

                SlowSetMemberValue(memberPath[memberPath.Count - 1], currentInstance, arg);
            });
        }

        private static object SlowGetMemberValue(PathStep step, object instance)
        {
            switch (step.StepType)
            {
                case PathStepType.Member:
                    {
                        FieldInfo field = step.Member as FieldInfo;
                        if (field != null)
                        {
                            if (field.IsLiteral)
                                return field.GetRawConstantValue();
                            return field.GetValue(instance);
                        }

                        PropertyInfo prop = step.Member as PropertyInfo;
                        if (prop != null)
                        {
                            return prop.GetValue(instance, null);
                        }

                        MethodInfo method = step.Member as MethodInfo;
                        if (method != null)
                        {
                            return method.Invoke(instance, null);
                        }

                        throw new NotSupportedException(step.Member.GetType().GetNiceName());
                    }

                case PathStepType.WeakListElement:
                    return WeakListGetItem.Invoke(instance, new object[] { step.ElementIndex });

                case PathStepType.ArrayElement:
                    return (instance as Array).GetValue(step.ElementIndex);

                case PathStepType.StrongListElement:
                    return step.StrongListGetItemMethod.Invoke(instance, new object[] { step.ElementIndex });

                default:
                    throw new NotImplementedException(step.StepType.ToString());
            }
        }

        private static void SlowSetMemberValue(PathStep step, object instance, object value)
        {
            switch (step.StepType)
            {
                case PathStepType.Member:
                    {
                        FieldInfo field = step.Member as FieldInfo;
                        if (field != null)
                        {
                            field.SetValue(instance, value);
                            break;
                        }

                        PropertyInfo prop = step.Member as PropertyInfo;
                        if (prop != null)
                        {
                            prop.SetValue(instance, value, null);
                            break;
                        }

                        throw new NotSupportedException(step.Member.GetType().GetNiceName());
                    }

                case PathStepType.WeakListElement:
                    WeakListSetItem.Invoke(instance, new object[] { step.ElementIndex, value });
                    break;

                case PathStepType.ArrayElement:
                    (instance as Array).SetValue(value, step.ElementIndex);
                    break;

                case PathStepType.StrongListElement:
                    var setItemMethod = typeof(IList<>).MakeGenericType(step.ElementType).GetMethod("set_Item");
                    setItemMethod.Invoke(instance, new object[] { step.ElementIndex, value });
                    break;

                default:
                    throw new NotImplementedException(step.StepType.ToString());
            }
        }

        private static List<PathStep> GetMemberPath(Type rootType, ref Type resultType, string path, out bool rootIsStatic, bool isSet)
        {
            if (path.IsNullOrWhitespace())
            {
                throw new ArgumentException("Invalid path; is null or whitespace.");
            }

            rootIsStatic = false;
            List<PathStep> result = new List<PathStep>();
            string[] steps = path.Split('.');

            Type currentType = rootType;

            for (int i = 0; i < steps.Length; i++)
            {
                string step = steps[i];

                bool expectMethod = false;

                if (step.StartsWith("[", StringComparison.InvariantCulture) && step.EndsWith("]", StringComparison.InvariantCulture))
                {
                    int index;
                    string indexStr = step.Substring(1, step.Length - 2);

                    if (!int.TryParse(indexStr, out index))
                    {
                        throw new ArgumentException("Couldn't parse an index from the path step '" + step + "'.");
                    }

                    // We need to check the current type to see if we can treat it as a list

                    if (currentType.IsArray)
                    {
                        Type elementType = currentType.GetElementType();
                        result.Add(new PathStep(index, elementType, true));
                        currentType = elementType;
                    }
                    else if (currentType.ImplementsOpenGenericInterface(typeof(IList<>)))
                    {
                        Type elementType = currentType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];
                        result.Add(new PathStep(index, elementType, false));
                        currentType = elementType;
                    }
                    else if (typeof(IList).IsAssignableFrom(currentType))
                    {
                        result.Add(new PathStep(index));
                        currentType = typeof(object);
                    }
                    else
                    {
                        throw new ArgumentException("Cannot get elements by index from the type '" + currentType.Name + "'.");
                    }

                    continue;
                }

                if (step.EndsWith("()", StringComparison.InvariantCulture))
                {
                    expectMethod = true;
                    step = step.Substring(0, step.Length - 2);
                }

                var member = GetStepMember(currentType, step, expectMethod);

                if (member.IsStatic())
                {
                    if (currentType == rootType)
                    {
                        rootIsStatic = true;
                    }
                    else
                    {
                        throw new ArgumentException("The non-root member '" + step + "' is static; use that member as the path root instead.");
                    }
                }

                currentType = member.GetReturnType();

                if (expectMethod && (currentType == null || currentType == typeof(void)))
                {
                    throw new ArgumentException("The method '" + member.Name + "' has no return type and cannot be part of a deep reflection path.");
                }

                result.Add(new PathStep(member));
            }

            if (resultType == null)
            {
                resultType = currentType;
            }
            // Objects can always be assigned to everything else
            else if (currentType != typeof(object) && resultType.IsAssignableFrom(currentType) == false)
            {
                throw new ArgumentException("Last member '" + result[result.Count - 1].Member.Name + "' of path '" + path + "' contains type '" + currentType.AssemblyQualifiedName + "', which is not assignable to expected type '" + resultType.AssemblyQualifiedName + "'.");
            }

            return result;
        }

        private static MemberInfo GetStepMember(Type owningType, string name, bool expectMethod)
        {
            MemberInfo result = null;
            MemberInfo[] possibleMembers = owningType.GetAllMembers(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToArray();
            int stepMethodParameterCount = int.MaxValue;

            for (int j = 0; j < possibleMembers.Length; j++)
            {
                MemberInfo member = possibleMembers[j];

                if (expectMethod)
                {
                    MethodInfo method = member as MethodInfo;

                    if (method != null)
                    {
                        int parameterCount = method.GetParameters().Length;

                        if (result == null || parameterCount < stepMethodParameterCount)
                        {
                            result = method;
                            stepMethodParameterCount = parameterCount;
                        }
                    }
                }
                else
                {
                    if (member is MethodInfo)
                    {
                        throw new ArgumentException("Found method member for name '" + name + "', but expected a field or property.");
                    }

                    result = member;
                    break;
                }
            }

            if (result == null)
            {
                throw new ArgumentException("Could not find expected " + (expectMethod ? "method" : "field or property") + " '" + name + "' on type '" + owningType.GetNiceName() + "' while parsing reflection path.");
            }

            if (expectMethod && stepMethodParameterCount > 0)
            {
                throw new NotSupportedException("Method '" + result.GetNiceName() + "' has " + stepMethodParameterCount + " parameters, but method parameters are currently not supported.");
            }

            if ((result is FieldInfo || result is PropertyInfo || result is MethodInfo) == false)
            {
                throw new NotSupportedException("Members of type " + result.GetType().GetNiceName() + " are not support; only fields, properties and methods are supported.");
            }

            return result;
        }

#if CAN_EMIT
        private static void EmitConstant(ILGenerator il, object constant, Type type = null)
        {
            unchecked
            {
                if (constant == null)
                {
                    il.Emit(OpCodes.Ldnull);
                    return;
                }

                if (type == null)
                {
                    type = constant.GetType();
                }

                if (type == typeof(int)
                    || type == typeof(byte)
                    || type == typeof(sbyte)
                    || type == typeof(short)
                    || type == typeof(ushort))
                {
                    il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(constant));
                }
                else if (type == typeof(uint))
                {
                    il.Emit(OpCodes.Ldc_I4, (int)(uint)constant);
                }
                else if (type == typeof(long))
                {
                    il.Emit(OpCodes.Ldc_I8, (long)constant);
                }
                else if (type == typeof(ulong))
                {
                    il.Emit(OpCodes.Ldc_I8, (long)(ulong)constant);
                }
                else if (type == typeof(float))
                {
                    il.Emit(OpCodes.Ldc_R4, (float)constant);
                }
                else if (type == typeof(double))
                {
                    il.Emit(OpCodes.Ldc_R8, (double)constant);
                }
                else if (type == typeof(string))
                {
                    il.Emit(OpCodes.Ldstr, (string)constant);
                }
                else if (type == typeof(char))
                {
                    il.Emit(OpCodes.Ldc_I4, (int)(char)constant);
                }
                else if (type == typeof(decimal))
                {
                    var bits = decimal.GetBits((decimal)constant);
                    var constructor = typeof(decimal).GetConstructor(new Type[] { typeof(int[]) });

                    var arrLocal = il.DeclareLocal(typeof(int[]));

                    il.Emit(OpCodes.Ldc_I4, bits.Length);                   // (...), bits_length
                    il.Emit(OpCodes.Newarr, typeof(int));                   // (...), bits_array
                    il.Emit(OpCodes.Stloc, arrLocal);                       // (...)

                    for (int i = 0; i < bits.Length; i++)
                    {
                        il.Emit(OpCodes.Ldloc, arrLocal);                   // (...), bits_array
                        il.Emit(OpCodes.Ldc_I4, i);                         // (...), bits_array, i
                        il.Emit(OpCodes.Ldc_I4, bits[i]);                   // (...), bits_array, i, bits[i]
                        il.Emit(OpCodes.Stelem_I4);                         // (...), 
                    }

                    il.Emit(OpCodes.Ldloc, arrLocal);                       // (...), bits_array
                    il.Emit(OpCodes.Newobj, constructor);                     // (...), decimal
                }
                else if (type == typeof(bool))
                {
                    il.Emit((bool)constant ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                }
                else if (type.IsEnum)
                {
                    EmitConstant(il, constant, Enum.GetUnderlyingType(type));
                }
                else
                {
                    throw new NotSupportedException("Type " + type.GetNiceFullName() + " is not supported as a constant.");
                }
            }
        }
#endif
    }
}