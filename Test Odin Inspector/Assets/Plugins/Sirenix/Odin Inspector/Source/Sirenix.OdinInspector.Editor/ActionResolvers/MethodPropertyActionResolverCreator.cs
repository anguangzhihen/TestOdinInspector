#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MethodPropertyActionResolverCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Sirenix.Utilities;
using System.Reflection;

[assembly: Sirenix.OdinInspector.Editor.ActionResolvers.RegisterDefaultActionResolver(typeof(Sirenix.OdinInspector.Editor.ActionResolvers.MethodPropertyActionResolverCreator), 20)]

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    public class MethodPropertyActionResolverCreator : ActionResolverCreator
    {
        public override string GetPossibleMatchesString(ref ActionResolverContext context)
        {
            return null;
        }

        public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
        {
            var prop = context.Property;

            if (string.IsNullOrEmpty(context.ResolvedString) && prop.Info.PropertyType == PropertyType.Method)
            {
                MethodInfo method = (prop.Info.GetMemberInfo() as MethodInfo) ?? prop.Info.GetMethodDelegate().Method;

                if (method.IsGenericMethodDefinition)
                {
                    context.ErrorMessage = "Cannot invoke a generic method definition such as '" + method.GetNiceName() + "'.";
                    return FailedResolveAction;
                }

                NamedValues argSetup = default(NamedValues);

                if (IsCompatibleMethod(method, ref context.NamedValues, ref argSetup, out context.ErrorMessage))
                {
                    if (prop.Info.GetMethodDelegate() != null)
                    {
                        return GetDelegateInvoker(prop.Info.GetMethodDelegate(), argSetup);
                    }
                    else
                    {
                        return GetMethodInvoker(method, argSetup, prop.ParentType.IsValueType);
                    }
                }
                else if (context.ErrorMessage != null)
                {
                    return FailedResolveAction;
                }
            }

            return null;
        }
    }
}
#endif