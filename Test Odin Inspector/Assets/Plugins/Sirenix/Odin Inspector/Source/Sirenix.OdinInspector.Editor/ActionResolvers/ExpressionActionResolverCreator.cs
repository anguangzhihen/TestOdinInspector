#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ExpressionActionResolverCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.ActionResolvers.RegisterDefaultActionResolver(typeof(Sirenix.OdinInspector.Editor.ActionResolvers.ExpressionActionResolverCreator), -10)]

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    using Sirenix.Utilities.Editor.Expressions;
    using System;

    public class ExpressionActionResolverCreator : ActionResolverCreator
    {
        private static readonly ResolvedAction EmptyExpression = (ref ActionResolverContext context, int selectionIndex) => { };

        public override string GetPossibleMatchesString(ref ActionResolverContext context)
        {
#if ODIN_LIMITED_VERSION
            return null;
#else
            return "C# Expressions: \"@expression\"";
#endif
        }

        public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
        {
            if (string.IsNullOrEmpty(context.ResolvedString) || context.ResolvedString[0] != '@') return null;

#if ODIN_LIMITED_VERSION
            context.ErrorMessage = "Attribute expressions are not available in this version of Odin.";
            return GetFailedResolveFunc<TResult>();
#else
            if (context.ResolvedString.Length == 1)
            {
                return EmptyExpression;
            }

            var expression = context.ResolvedString.Substring(1);

            var parameterCount = context.NamedValues.Count;

            bool isStatic = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;

            var parameterNames = new string[parameterCount];
            var parameterTypes = new Type[parameterCount];

            for (int i = 0; i < parameterCount; i++)
            {
                var value = context.NamedValues[i];

                parameterNames[i] = value.Name;
                parameterTypes[i] = value.Type;
            }

            string compileError;
            var method = ExpressionUtility.ParseExpression(expression, isStatic, context.ParentType, parameterTypes, parameterNames, out compileError, true);

            if (compileError != null)
            {
                context.ErrorMessage = compileError;
                return FailedResolveAction;
            }

            var parameterValues = new object[parameterCount + (isStatic ? 0 : 1)];
            return GetExpressionLambda(method, isStatic, context.ParentType.IsValueType, parameterValues);
#endif
        }

        private static ResolvedAction GetExpressionLambda(Delegate method, bool isStatic, bool parentIsValueType, object[] parameterValues)
        {
            return (ref ActionResolverContext context, int selectionIndex) =>
            {
                int offset = 0;

                var array = parameterValues;

                if (!isStatic)
                {
                    array[0] = context.GetParentValue(selectionIndex);
                    offset = 1;
                }

                for (int i = offset; i < array.Length; i++)
                {
                    array[i] = context.NamedValues[i - offset].CurrentValue;
                }

                method.DynamicInvoke(parameterValues);

                if (!isStatic && parentIsValueType)
                {
                    context.SetParentValue(selectionIndex, array[0]);
                }
            };
        }
    }
}
#endif