#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValueResolverCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using UnityEngine;

    public abstract class ValueResolverCreator
    {
        private struct ResolverAndPriority
        {
            public ValueResolverCreator ResolverCreator;
            public double Priority;
        }

        private static StringBuilder SB = new StringBuilder();
        private static ResolverAndPriority[] ValueResolverCreators = new ResolverAndPriority[8];
        private static readonly Dictionary<Type, Delegate> FailedResolveFuncs = new Dictionary<Type, Delegate>(FastTypeComparer.Instance);
        private static readonly Dictionary<Type, Delegate> FallbackResolveFuncs = new Dictionary<Type, Delegate>(FastTypeComparer.Instance);
        private static readonly Dictionary<Type, MethodInfo> WeaklyTypedGetResolverMethods = new Dictionary<Type, MethodInfo>(FastTypeComparer.Instance);
        private static readonly MethodInfo GetResolverMethodInfo = typeof(ValueResolverCreator).GetMethod("GetResolver", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(InspectorProperty), typeof(string), typeof(object), typeof(bool), typeof(NamedValue[]) }, null);
        private static readonly object[] GetResolverMethodParameters = new object[5];

        static ValueResolverCreator()
        {
            var assemblies = ResolverUtilities.GetResolverAssemblies();

            for (int i = 0; i < assemblies.Count; i++)
            {
                var assembly = assemblies[i];
                var attrs = assembly.GetCustomAttributes(typeof(RegisterDefaultValueResolverCreatorAttribute), false);

                for (int j = 0; j < attrs.Length; j++)
                {
                    var attr = (RegisterDefaultValueResolverCreatorAttribute)attrs[j];

                    try
                    {
                        object instance = Activator.CreateInstance(attr.ResolverCreatorType);
                        Register((ValueResolverCreator)instance, attr.Order);
                    }
                    catch (Exception ex)
                    {
                        while (ex.InnerException != null && ex is TargetInvocationException)
                        {
                            ex = ex.InnerException;
                        }

                        Debug.LogException(new Exception("Failed to create instance of registered default resolver of type '" + attr.ResolverCreatorType.GetNiceFullName() + "'", ex));
                    }
                }
            }
        }

        public abstract ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context);
        public abstract string GetPossibleMatchesString(ref ValueResolverContext context);

        //private static TResult FallbackResolveResult<TResult>(ref ValueResolverContext context, int selectionIndex)
        //{
        //    return (TResult)context.FallbackValue;
        //}

        private static TResult FailedResolveResult<TResult>(ref ValueResolverContext context, int selectionIndex)
        {
            if (context.HasFallbackValue)
            {
                return (TResult)context.FallbackValue;
            }

            return default(TResult);
        }

        public static void Register(ValueResolverCreator valueResolverCreator, double order = 0)
        {
            var array = ValueResolverCreators;

            bool added = false;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].ResolverCreator == null)
                {
                    array[i].ResolverCreator = valueResolverCreator;
                    array[i].Priority = order;
                    added = true;
                    break;
                }
                else if (order > array[i].Priority)
                {
                    ShiftUp(ref array, i);
                    array[i].ResolverCreator = valueResolverCreator;
                    array[i].Priority = order;
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                int index = array.Length;
                Expand(ref array);
                array[index].ResolverCreator = valueResolverCreator;
                array[index].Priority = order;
            }

            ValueResolverCreators = array;
        }

        private static void Expand(ref ResolverAndPriority[] array)
        {
            var newArray = new ResolverAndPriority[array.Length * 2];
            Array.Copy(array, newArray, array.Length);
            array = newArray;
        }

        private static void ShiftUp(ref ResolverAndPriority[] array, int index)
        {
            int arrayEnd = array.Length - 1;

            if (array[arrayEnd].ResolverCreator != null) Expand(ref array);

            for (int i = arrayEnd; i >= index; i--)
            {
                if (i + 1 >= array.Length) continue;
                array[i + 1].ResolverCreator = array[i].ResolverCreator;
                array[i + 1].Priority = array[i].Priority;
            }
        }

        public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString)
        {
            return GetResolver(resultType, property, resolvedString, null, false, null);
        }

        public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
        {
            return GetResolver(resultType, property, resolvedString, null, false, namedArgs);
        }

        public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue)
        {
            return GetResolver(resultType, property, resolvedString, fallbackValue, true, null);
        }

        public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue, params NamedValue[] namedArgs)
        {
            return GetResolver(resultType, property, resolvedString, fallbackValue, true, namedArgs);
        }

        private static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue, bool hasFallback, params NamedValue[] namedArgs)
        {
            MethodInfo method;

            if (!WeaklyTypedGetResolverMethods.TryGetValue(resultType, out method))
            {
                method = GetResolverMethodInfo.MakeGenericMethod(resultType);
                WeaklyTypedGetResolverMethods.Add(resultType, method);
            }

            GetResolverMethodParameters[0] = property;
            GetResolverMethodParameters[1] = resolvedString;
            GetResolverMethodParameters[2] = fallbackValue;
            GetResolverMethodParameters[3] = hasFallback;
            GetResolverMethodParameters[4] = namedArgs;

            return (ValueResolver)method.Invoke(null, GetResolverMethodParameters);
        }


        public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString)
        {
            return GetResolver<TResult>(property, resolvedString, null, false, null);
        }

        public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
        {
            return GetResolver<TResult>(property, resolvedString, null, false, namedArgs);
        }

        public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue)
        {
            return GetResolver<TResult>(property, resolvedString, fallbackValue, true, null);
        }

        public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue, params NamedValue[] namedArgs)
        {
            return GetResolver<TResult>(property, resolvedString, fallbackValue, true, namedArgs);
        }

        private static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, object fallbackValue, bool hasFallback, params NamedValue[] namedArgs)
        {
            var resolver = new ValueResolver<TResult>();

            resolver.Context.Property = property;
            resolver.Context.ResolvedString = resolvedString;
            resolver.Context.ResultType = typeof(TResult);
            resolver.Context.LogExceptions = true;

            if (hasFallback)
            {
                resolver.Context.FallbackValue = fallbackValue;
                resolver.Context.HasFallbackValue = true;
            }

            if (namedArgs != null)
            {
                for (int i = 0; i < namedArgs.Length; i++)
                {
                    resolver.Context.NamedValues.Add(namedArgs[i]);
                }
            }

            resolver.Context.AddDefaultContextValues();

            InitResolver(resolver);

            return resolver;
        }

        private static string GetPossibleMatchesMessage(ref ValueResolverContext context)
        {
            SB.Length = 0;

            SB.AppendLine("Could not match the given string '" + context.ResolvedString + "' to any possible value resolution in the context of the type '" + context.ParentType.GetNiceName() + "'. The following kinds of value resolutions are possible:");
            SB.AppendLine();

            var array = ValueResolverCreators;

            for (int i = 0; i < array.Length; i++)
            {
                var resolver = array[i].ResolverCreator;
                if (resolver == null) break;
                var matchLine = resolver.GetPossibleMatchesString(ref context);
                if (matchLine == null) continue;
                SB.AppendLine(matchLine);
            }

            SB.AppendLine();
            SB.AppendLine("And the following named values are available:");
            SB.AppendLine();
            SB.Append(context.NamedValues.GetValueOverviewString());

            return SB.ToString();
        }

        private static void InitResolver<TResult>(ValueResolver<TResult> resolver)
        {
            var hasFallback = resolver.Context.HasFallbackValue;
            var array = ValueResolverCreators;

            ValueResolverFunc<TResult> func = null;

            for (int i = 0; i < array.Length; i++)
            {
                var resolverCreator = array[i].ResolverCreator;
                if (resolverCreator == null) break;

                try
                { 
                    func = resolverCreator.TryCreateResolverFunc<TResult>(ref resolver.Context);
                }
                catch (Exception ex)
                {
                    InitFailedResolve(resolver, false);

                    resolver.Context.ErrorMessage = "Resolver creator '" + resolverCreator.GetType().Name + "' failed with exception:\n\n" + ex.ToString();
                    return;
                }

                if (func != null)
                    break;
            }
            
            if (func == null)
            {
                if (hasFallback)
                {
                    InitFailedResolve(resolver, false);
                }
                else
                {
                    InitFailedResolve(resolver, true);
                }
            }
            else resolver.Func = func;
        }

        private static void InitFailedResolve<TResult>(ValueResolver<TResult> resolver, bool withError)
        {
            if (withError)
            {
                resolver.Context.ErrorMessage = GetPossibleMatchesMessage(ref resolver.Context);
            }

            Delegate failedResolveFunc;

            if (!FailedResolveFuncs.TryGetValue(typeof(TResult), out failedResolveFunc))
            {
                failedResolveFunc = (ValueResolverFunc<TResult>)FailedResolveResult<TResult>;
                FailedResolveFuncs.Add(typeof(TResult), failedResolveFunc);
            }

            resolver.Func = (ValueResolverFunc<TResult>)failedResolveFunc;
        }

        //private static void InitFallback<TResult>(ValueResolver<TResult> resolver, bool withError)
        //{
        //    Delegate fallbackResolveFunc;

        //    if (!FallbackResolveFuncs.TryGetValue(typeof(TResult), out fallbackResolveFunc))
        //    {
        //        fallbackResolveFunc = (ValueResolverFunc<TResult>)FallbackResolveResult<TResult>;
        //        FallbackResolveFuncs.Add(typeof(TResult), fallbackResolveFunc);
        //    }

        //    if (withError)
        //    {
        //        // Fail, but deliver fallback value
        //        resolver.Context.ErrorMessage = GetPossibleMatchesMessage(ref resolver.Context);
        //    }

        //    resolver.Func = (ValueResolverFunc<TResult>)fallbackResolveFunc;
        //}

        protected static ValueResolverFunc<TResult> GetFailedResolverFunc<TResult>()
        {
            Delegate failedResolveFunc;

            if (!FailedResolveFuncs.TryGetValue(typeof(TResult), out failedResolveFunc))
            {
                failedResolveFunc = (ValueResolverFunc<TResult>)FailedResolveResult<TResult>;
                FailedResolveFuncs.Add(typeof(TResult), failedResolveFunc);
            }

            return (ValueResolverFunc<TResult>)failedResolveFunc;
        }
    }
}
#endif