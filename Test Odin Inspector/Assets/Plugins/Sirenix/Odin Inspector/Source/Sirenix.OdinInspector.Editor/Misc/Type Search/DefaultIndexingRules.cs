#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DefaultIndexingRules.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Linq;

    public static class DefaultIndexingRules
    {
        public static readonly TypeMatchIndexingRule MustBeAbleToInstantiateType = new TypeMatchIndexingRule(
            "Must be able to instantiate {name}",
            (ref TypeSearchInfo info, ref string error) =>
            {
                if (info.MatchType.IsAbstract)
                {
                    error = "Type is an abstract";
                    return false;
                }
                else if (info.MatchType.IsInterface)
                {
                    error = "Type is an interface";
                    return false;
                }
                else if (info.MatchType.GetConstructor(Type.EmptyTypes) == null)
                {
                    error = "Has no public parameterless constructor";
                    return false;
                }

                return true;
            });

        public static readonly TypeMatchIndexingRule NoAbstractOrInterfaceTargets = new TypeMatchIndexingRule(
            "No abstract or interface targets",
            (ref TypeSearchInfo info, ref string error) =>
            {
                for (int i = 0; i < info.Targets.Length; i++)
                {
                    if (info.Targets[i].IsGenericParameter) continue;

                    if (info.Targets[i].IsInterface)
                    {
                        error = "You cannot use an interface '" + info.Targets[i].GetNiceName() + "' as a {name} target. Use a generic {name} with a constraint for that interface instead.";
                        return false;
                    }
                    else if (info.Targets[i].IsAbstract)
                    {
                        error = "You cannot use an abstract type '" + info.Targets[i].GetNiceName() + "' as a {name} target. Use a generic {name} with a constraint for that abstract class instead.";
                        return false;
                    }
                }

                return true;
            });

        public static readonly TypeMatchIndexingRule GenericMatchTypeValidation = new TypeMatchIndexingRule(
            "Generic {name} validation",
            (ref TypeSearchInfo info, ref string error) =>
            {
                if (!info.MatchType.IsGenericTypeDefinition) return true;
                if (info.Targets.Length != 1) return true;
                if (info.Targets[0].IsGenericParameter) return true;

                var targetType = info.Targets[0];

                if (info.MatchType.IsNested && info.MatchType.DeclaringType.IsGenericType)
                {
                    var parentArgs = info.MatchType.DeclaringType.GetGenericArguments();
                    var drawerArgs = info.MatchType.GetGenericArguments();
                    var valueArgs = targetType.GetGenericArguments();

                    bool valid = parentArgs.Length == drawerArgs.Length && parentArgs.Length == valueArgs.Length;

                    // The length comparison should actually be good enough to check the condition
                    // We just compare the names out of due diligence
                    if (valid)
                    {
                        for (int i = 0; i < parentArgs.Length; i++)
                        {
                            if (parentArgs[i].Name != drawerArgs[i].Name || parentArgs[i].Name != valueArgs[i].Name)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    if (!valid)
                    {
                        error = "You cannot declare {name}s nested inside generic types unless the following conditions are true: 1) the nested {name} itself is not generic, 2) the nested {name} must target a type that is nested within the same type as the nested {name}, 3) the target type must not be generic.";
                        return false;
                    }
                }
                else if (targetType.IsGenericType && targetType.GenericArgumentsContainsTypes((info.MatchType.GetGenericArguments().Where(n => n.IsGenericParameter).ToArray())))
                {
                    return true;
                }
                else
                {
                    error = "You cannot declare a generic {name} without passing a generic parameter as the target, or a generic type definition containing all the {name}'s generic parameters as the target. You passed '" + targetType.GetNiceName() + "'.";
                    return false;
                }

                if (targetType.IsNested && targetType.DeclaringType.IsGenericType)
                {
                    var parentArgs = targetType.DeclaringType.GetGenericArguments();
                    var drawerArgs = targetType.GetGenericArguments();
                    var valueArgs = info.MatchType.GetGenericArguments();

                    bool valid = parentArgs.Length == drawerArgs.Length && parentArgs.Length == valueArgs.Length;

                    // The length comparison should actually be good enough to check the condition
                    // We just compare the names out of due diligence
                    if (valid)
                    {
                        for (int i = 0; i < parentArgs.Length; i++)
                        {
                            if (parentArgs[i].Name != drawerArgs[i].Name || parentArgs[i].Name != valueArgs[i].Name)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    if (!valid)
                    {
                        error = "You cannot declare {name}s nested inside generic types unless the following conditions are true: 1) the nested {name} itself is not generic, 2) the nested {name} must target a type that is nested within the same type as the nested {name}, 3) the target type must not be generic.";
                        return false;
                    }
                }
                else if (info.MatchType.IsGenericType && targetType.GenericArgumentsContainsTypes(targetType.GetGenericArguments().Where(n => n.IsGenericParameter).ToArray()))
                {
                    return true;
                }
                else
                {
                    error = "You cannot declare a generic {name} without passing either a generic parameter or a generic type definition containing all the {name}'s generic parameters as the target. You passed '" + targetType.GetNiceName() + "'.";
                    return false;
                }

                return true;
            });

        public static readonly TypeMatchIndexingRule GenericDefinitionSanityCheck = new TypeMatchIndexingRule(
            "Generic {name} definition sanity check",
            (ref TypeSearchInfo info, ref string error) =>
            {
                if (!info.MatchType.IsGenericTypeDefinition) return true;

                {
                    //bool hasGenericParameterArguments; // TODO: This was unused. Is that intentional?

                    for (int i = 0; i < info.Targets.Length; i++)
                    {
                        if (info.Targets[i].IsGenericParameter)
                        {
                            //hasGenericParameterArguments = true;
                            break;
                        }
                    }
                }

                return true;
            });
    }
}
#endif