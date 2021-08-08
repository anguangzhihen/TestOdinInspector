#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MemberReferenceValueResolverCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
[assembly: Sirenix.OdinInspector.Editor.ValueResolvers.RegisterDefaultValueResolverCreator(typeof(Sirenix.OdinInspector.Editor.ValueResolvers.MemberReferenceValueResolverCreator), -10)]

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Reflection;

    public class MemberReferenceValueResolverCreator : BaseMemberValueResolverCreator
    {
        public override string GetPossibleMatchesString(ref ValueResolverContext context)
        {
            if (context.ResultType == typeof(string))
            {
                return "Member References: \"$MemberName\"";
            }
            else
            {
                return "Member References: \"MemberName\"";
            }
        }

        public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
        {
            if (string.IsNullOrEmpty(context.ResolvedString) || context.ResolvedString.Length < 2) return null;

            bool mustBeMember;
            string memberName;
            
            if (context.ResolvedString[0] == '$')
            {
                mustBeMember = true;
                memberName = context.ResolvedString.Substring(1);
            }
            else
            {
                if (typeof(TResult) == typeof(string) && context.HasFallbackValue) return null;

                mustBeMember = false;
                memberName = context.ResolvedString;
            }

            // Optimization; if it is not a valid identifier there is no need to bother looking for it
            if (!TypeExtensions.IsValidIdentifier(memberName))
            {
                if (mustBeMember)
                {
                    context.ErrorMessage = "'" + memberName + "' is not a valid C# member identifier.";
                    return GetFailedResolverFunc<TResult>();
                }
                else return null;
            }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            bool isStatic = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;

            if (!isStatic)
            {
                flags |= BindingFlags.Instance;
            }

            var contextType = context.ParentType;
            string errorMessage;
            NamedValues argSetup = default(NamedValues);

            // Try a field
            MemberInfo member = contextType.GetField(memberName, flags);

            // Then a property
            if (member == null)
            {
                member = contextType.GetProperty(memberName, flags);
            }

            // Then a method
            if (member == null)
            {
                member = GetCompatibleMethod(contextType, memberName, flags, ref context.NamedValues, ref argSetup, out errorMessage);

                if (errorMessage != null)
                {
                    context.ErrorMessage = errorMessage;
                    return GetFailedResolverFunc<TResult>();
                }
            }

            if (member == null && !isStatic)
            {
                // We can go looking in base classes now

                Type current = contextType.BaseType;
                var newFlags = flags;

                newFlags &= ~BindingFlags.FlattenHierarchy;
                newFlags |= BindingFlags.DeclaredOnly;

                do
                {
                    // Try a field
                    member = current.GetField(memberName, newFlags);

                    // Then a property
                    if (member == null)
                    {
                        member = current.GetProperty(memberName, newFlags);
                    }

                    // Then a method
                    if (member == null)
                    {
                        member = GetCompatibleMethod(current, memberName, flags, ref context.NamedValues, ref argSetup, out errorMessage);

                        if (errorMessage != null)
                        {
                            context.ErrorMessage = errorMessage;
                            return GetFailedResolverFunc<TResult>();
                        }
                    }

                    if (member == null) current = current.BaseType;
                    else break;
                }
                while (current != null);
            }

            if (member != null)
            {
                var containedType = member.GetReturnType();

                if (containedType == typeof(void) || !ConvertUtility.CanConvert(containedType, typeof(TResult)))
                {
                    if (member is MethodInfo)
                    {
                        if (containedType == typeof(void))
                        {
                            context.ErrorMessage = "Method " + member.Name + " cannot return void; it must return a value that can be assigned or converted to the type '" + typeof(TResult).GetNiceName() + "'";
                        }
                        else
                        {
                            context.ErrorMessage = "Cannot convert method " + member.Name + "'s return type '" + containedType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
                        }
                    }
                    else
                    {
                        context.ErrorMessage = "Cannot convert member " + member.Name + "'s contained type '" + containedType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
                    }

                    return GetFailedResolverFunc<TResult>();
                }

                if (member is FieldInfo)
                {
                    return GetFieldGetter<TResult>(member as FieldInfo);
                }
                else if (member is PropertyInfo)
                {
                    return GetPropertyGetter<TResult>(member as PropertyInfo, context.Property.ParentType.IsValueType);
                }
                else
                {
                    return GetMethodGetter<TResult>(member as MethodInfo, argSetup, context.Property.ParentType.IsValueType);
                }
            }
            
            if (mustBeMember)
            {
                context.ErrorMessage = "Could not find a field, property or method with the name '" + memberName + "' on the type '" + contextType.GetNiceName() + "'.";
                return GetFailedResolverFunc<TResult>();
            }

            return null;
        }
    }
}
#endif