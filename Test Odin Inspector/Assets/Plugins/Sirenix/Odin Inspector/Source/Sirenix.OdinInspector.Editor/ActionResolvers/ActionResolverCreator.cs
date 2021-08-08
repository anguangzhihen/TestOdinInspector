#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ActionResolverCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Reflection;
    using System.Text;
    using UnityEngine;

    public abstract class ActionResolverCreator
    {
        private struct ResolverAndPriority
        {
            public ActionResolverCreator ResolverCreator;
            public double Priority;
        }

        private static StringBuilder SB = new StringBuilder();
        private static ResolverAndPriority[] ActionResolverCreators = new ResolverAndPriority[8];
        protected static readonly ResolvedAction FailedResolveAction = (ref ActionResolverContext context, int selectionIndex) => { };

        static ActionResolverCreator()
        {
            var assemblies = ResolverUtilities.GetResolverAssemblies();

            for (int i = 0; i < assemblies.Count; i++)
            {
                var assembly = assemblies[i];
                var attrs = assembly.GetCustomAttributes(typeof(RegisterDefaultActionResolverAttribute), false);

                for (int j = 0; j < attrs.Length; j++)
                {
                    var attr = (RegisterDefaultActionResolverAttribute)attrs[j];

                    try
                    {
                        object instance = Activator.CreateInstance(attr.ResolverType);
                        Register((ActionResolverCreator)instance, attr.Order);
                    }
                    catch (Exception ex)
                    {
                        while (ex.InnerException != null && ex is TargetInvocationException)
                        {
                            ex = ex.InnerException;
                        }

                        Debug.LogException(new Exception("Failed to create instance of registered default resolver of type '" + attr.ResolverType.GetNiceFullName() + "'", ex));
                    }
                }
            }
        }

        public abstract ResolvedAction TryCreateAction(ref ActionResolverContext context);
        public abstract string GetPossibleMatchesString(ref ActionResolverContext context);
        
        public static void Register(ActionResolverCreator valueResolverCreator, double order = 0)
        {
            var array = ActionResolverCreators;

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

            ActionResolverCreators = array;
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

        public static ActionResolver GetResolver(InspectorProperty property, string resolvedString)
        {
            return GetResolver(property, resolvedString, null);
        }

        public static ActionResolver GetResolver(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
        {
            var resolver = new ActionResolver();

            resolver.Context.Property = property;
            resolver.Context.ResolvedString = resolvedString;
            resolver.Context.LogExceptions = true;

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

        private static string GetPossibleMatchesMessage(ref ActionResolverContext context)
        {
            SB.Length = 0;

            SB.AppendLine("Could not match the given string '" + context.ResolvedString + "' to any action that can be performed in the context of the type '" + context.Property.ParentType.GetNiceName() + "'. The following kinds of actions are possible:");
            SB.AppendLine();

            var array = ActionResolverCreators;

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

        private static void InitResolver(ActionResolver resolver)
        {
            var array = ActionResolverCreators;

            ResolvedAction action = null;

            for (int i = 0; i < array.Length; i++)
            {
                var resolverCreator = array[i].ResolverCreator;
                if (resolverCreator == null) break;

                try
                {
                    action = resolverCreator.TryCreateAction(ref resolver.Context);
                }
                catch (Exception ex)
                {
                    resolver.Context.ErrorMessage = "Resolver creator '" + resolverCreator.GetType().Name + "' failed with exception:\n\n" + ex.ToString();
                    resolver.Action = FailedResolveAction;
                    return;
                }

                if (action != null)
                    break;
            }

            if (action == null)
            {
                resolver.Context.ErrorMessage = GetPossibleMatchesMessage(ref resolver.Context);
                resolver.Action = FailedResolveAction;
            }
            else resolver.Action = action;
        }

        protected static ResolvedAction GetDelegateInvoker(Delegate @delegate, NamedValues argSetup)
        {
            object[] parameters = new object[argSetup.Count];

            return (ref ActionResolverContext context, int selectionIndex) =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    object value = context.NamedValues.GetValue(argSetup[i].Name);
                    parameters[i] = ConvertUtility.WeakConvert(value, argSetup[i].Type);
                }

                @delegate.DynamicInvoke(parameters);

                if (context.SyncRefParametersWithNamedValues)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        NamedValue value;
                        var argInfo = argSetup[i];

                        if (!context.NamedValues.TryGetValue(argInfo.Name, out value))
                        {
                            throw new Exception("Expected named value '" + argInfo.Name + "' was not present!");
                        }

                        context.NamedValues.Set(argInfo.Name, ConvertUtility.WeakConvert(parameters[i], value.Type));
                    }
                }
            };
        }

        protected static ResolvedAction GetMethodInvoker(MethodInfo method, NamedValues argSetup, bool parentIsValueType)
        {
            object[] parameters = new object[argSetup.Count];
            bool isStatic = method.IsStatic;

            return (ref ActionResolverContext context, int selectionIndex) =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    object value = context.NamedValues.GetValue(argSetup[i].Name);
                    parameters[i] = ConvertUtility.WeakConvert(value, argSetup[i].Type);
                }

                object instance = isStatic ? null : context.GetParentValue(selectionIndex);
                method.Invoke(instance, parameters);

                if (!isStatic && parentIsValueType)
                {
                    context.SetParentValue(selectionIndex, instance);
                }

                if (context.SyncRefParametersWithNamedValues)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        NamedValue value;
                        var argInfo = argSetup[i];

                        if (!context.NamedValues.TryGetValue(argInfo.Name, out value))
                        {
                            throw new Exception("Expected named value '" + argInfo.Name + "' was not present!");
                        }

                        context.NamedValues.Set(argInfo.Name, ConvertUtility.WeakConvert(parameters[i], value.Type));
                    }
                }
            };
        }

        protected static unsafe bool IsCompatibleMethod(MethodInfo method, ref NamedValues namedValues, ref NamedValues argSetup, out string errorMessage)
        {
            var parameters = method.GetParameters();
            var namedValueCount = namedValues.Count;

            if (parameters.Length > namedValueCount)
            {
                errorMessage = "Method '" + method.GetNiceName() + "' has too many parameters (" + parameters.Length + "). The following '" + namedValueCount + "' parameters are available: \n\n" + namedValues.GetValueOverviewString();
                return false;
            }

            bool* claimedNamedValues = stackalloc bool[namedValueCount];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterName = parameter.Name;
                var parameterType = parameter.ParameterType;

                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }

                bool foundMatch = false;

                // First look for named args
                for (int j = 0; j < namedValueCount; j++)
                {
                    if (*(claimedNamedValues + j)) continue; // Value has been claimed by another parameter

                    var value = namedValues[j];

                    if (value.Name == parameterName)
                    {
                        if (!ConvertUtility.CanConvert(value.Type, parameterType))
                        {
                            errorMessage = "Method '" + method.Name + "' has an invalid signature; the parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "' cannot be assigned from the available type '" + value.Type.GetNiceName() + "'. The following parameters are available: \n\n" + namedValues.GetValueOverviewString();
                            return false;
                        }

                        argSetup.Add(value.Name, parameterType, null);
                        *(claimedNamedValues + j) = true;
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                {
                    // Then try to match by exact type
                    for (int j = 0; j < namedValueCount; j++)
                    {
                        if (*(claimedNamedValues + j)) continue; // Value has been claimed by another parameter

                        var value = namedValues[j];

                        if (value.Type == parameterType)
                        {
                            foundMatch = true;
                            argSetup.Add(value.Name, parameterType, null);
                            *(claimedNamedValues + j) = true;
                            break;
                        }
                    }
                }

                if (!foundMatch && parameterType != typeof(string))
                {
                    // Then try to match by convertable type
                    // All things convert to strings (via ToString()), so we don't allow that; otherwise, every value matches on string parameters, and that's no good
                    for (int j = 0; j < namedValueCount; j++)
                    {
                        if (*(claimedNamedValues + j)) continue; // Value has been claimed by another parameter

                        var value = namedValues[j];

                        if (ConvertUtility.CanConvert(value.Type, parameterType))
                        {
                            foundMatch = true;
                            argSetup.Add(value.Name, parameterType, null);
                            *(claimedNamedValues + j) = true;
                            break;
                        }
                    }
                }

                if (!foundMatch)
                {
                    errorMessage = "Method '" + method.Name + "' has an invalid signature; no values could be assigned to the parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "'. The following parameter values are available: \n\n" + namedValues.GetValueOverviewString();
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }
    }
}
#endif