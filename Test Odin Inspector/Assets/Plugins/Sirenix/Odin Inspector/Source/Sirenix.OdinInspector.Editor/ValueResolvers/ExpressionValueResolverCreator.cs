#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ExpressionValueResolverCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
[assembly: Sirenix.OdinInspector.Editor.ValueResolvers.RegisterDefaultValueResolverCreator(typeof(Sirenix.OdinInspector.Editor.ValueResolvers.ExpressionValueResolverCreator), -5)]

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor.Expressions;
    using System;

    public class ExpressionValueResolverCreator : ValueResolverCreator
    {
        public override string GetPossibleMatchesString(ref ValueResolverContext context)
        {
#if ODIN_LIMITED_VERSION
            return null;
#else
            return "C# Expressions: \"@expression\"";
#endif
        }

        public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
        {
            if (string.IsNullOrEmpty(context.ResolvedString) || context.ResolvedString.Length < 2 || context.ResolvedString[0] != '@') return null;

#if ODIN_LIMITED_VERSION
            context.ErrorMessage = "Attribute expressions are not available in this version of Odin.";
            return GetFailedResolveFunc<TResult>();
#else
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
                return GetFailedResolverFunc<TResult>();
            }

            var returnType = method.Method.ReturnType;

            if (returnType == typeof(void))
            {
                context.ErrorMessage = "Expression cannot evaluate to 'void'; it must evaluate to a value that can be assigned or converted to required type '" + typeof(TResult).GetNiceName() + "'";
                return GetFailedResolverFunc<TResult>();
            }
            else if (!ConvertUtility.CanConvert(returnType, typeof(TResult)))
            {
                context.ErrorMessage = "Cannot convert expression result type '" + method.Method.ReturnType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
                return GetFailedResolverFunc<TResult>();
            }

            var parameterValues = new object[parameterCount + (isStatic ? 0 : 1)];
            return GetExpressionLambda<TResult>(method, isStatic, context.ParentType.IsValueType, parameterValues);
#endif
        }

        private static ValueResolverFunc<TResult> GetExpressionLambda<TResult>(Delegate method, bool isStatic, bool parentIsValueType, object[] parameterValues)
        {
            return (ref ValueResolverContext context, int selectionIndex) =>
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

                var result = ConvertUtility.Convert<TResult>(method.DynamicInvoke(parameterValues));

                if (!isStatic && parentIsValueType)
                {
                    context.SetParentValue(selectionIndex, array[0]);
                }

                return result;
            };
        }
    }
}
#endif