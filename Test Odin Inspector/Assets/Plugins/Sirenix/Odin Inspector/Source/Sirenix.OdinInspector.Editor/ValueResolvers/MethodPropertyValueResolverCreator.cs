#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MethodPropertyActionResolverCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Sirenix.Utilities;
using System.Reflection;

[assembly: Sirenix.OdinInspector.Editor.ValueResolvers.RegisterDefaultValueResolverCreator(typeof(Sirenix.OdinInspector.Editor.ValueResolvers.MethodPropertyValueResolverCreator), 20)]

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    public class MethodPropertyValueResolverCreator : BaseMemberValueResolverCreator
    {
        public override string GetPossibleMatchesString(ref ValueResolverContext context)
        {
            return null;
        }

        public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
        {
            var prop = context.Property;

            if (string.IsNullOrEmpty(context.ResolvedString) && prop.Info.PropertyType == PropertyType.Method)
            {
                MethodInfo method = (prop.Info.GetMemberInfo() as MethodInfo) ?? prop.Info.GetMethodDelegate().Method;

                if (method.IsGenericMethodDefinition)
                {
                    context.ErrorMessage = "Cannot invoke a generic method definition such as '" + method.GetNiceName() + "'.";
                    return GetFailedResolverFunc<TResult>();
                }

                var containedType = method.ReturnType;

                if (containedType == typeof(void) || !ConvertUtility.CanConvert(containedType, typeof(TResult)))
                {
                    return null;
                }

                NamedValues argSetup = default(NamedValues);

                if (IsCompatibleMethod(method, ref context.NamedValues, ref argSetup, out context.ErrorMessage))
                {
                    if (prop.Info.GetMethodDelegate() != null)
                    {
                        return GetDelegateGetter<TResult>(prop.Info.GetMethodDelegate(), argSetup);
                    }
                    else
                    {
                        return GetMethodGetter<TResult>(method, argSetup, prop.ParentType.IsValueType);
                    }
                }
                else if (context.ErrorMessage != null)
                {
                    return GetFailedResolverFunc<TResult>();
                }
            }

            return null;
        }
    }
}
#endif