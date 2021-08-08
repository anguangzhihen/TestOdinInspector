#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ExpressionUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    using System;

    /// <summary>
    /// Utility for parsing and emitting expression delegates.
    /// </summary>
    public static class ExpressionUtility
    {
#if !ODIN_LIMITED_VERSION
        private readonly static Tokenizer Tokenizer  = new Tokenizer();
        private readonly static ASTParser Parser     = new ASTParser(Tokenizer);
        private readonly static ASTEmitter Emitter   = new ASTEmitter();
        private readonly static EmitContext Context  = new EmitContext();
#endif

        public static string GetASTPrettyPrint(string expression)
        {
#if !ODIN_LIMITED_VERSION
            Tokenizer.SetExpressionString(expression);
            var ast = Parser.Parse();
            return ast.ToPrettyPrint();
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        
        public static bool TryParseTypeNameAsCSharpIdentifier(string typeString, out Type type)
        {
#if !ODIN_LIMITED_VERSION
            try
            {
                Tokenizer.SetExpressionString(typeString);
                var ast = Parser.Parse();

                Context.IsStatic = true;
                Context.Type = typeof(object);
                Context.ReturnType = null;
                Context.Parameters = null;
                Context.ParameterNames = null;

                Emitter.Context = Context;
                Emitter.Visit(ast);

                type = ast.NodeValue as Type;
                return type != null;
            }
            catch (Exception)
            {
                type = null;
                return false;
            }
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.IsStatic = isStatic;
            Context.Type = contextType;
            Context.ReturnType = null;
            Context.Parameters = null;
            Context.ParameterNames = null;
            return ParseExpression(expression, Context, null, out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="parameters">The parameters of the expression delegate.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, Type[] parameters, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.IsStatic = isStatic;
            Context.Type = contextType;
            Context.ReturnType = null;
            Context.Parameters = parameters;
            Context.ParameterNames = null;
            return ParseExpression(expression, Context, null, out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="parameters">The parameters of the expression delegate.</param>
        /// <param name="parameterNames">The names of the expression's parameters, for use with the named parameter syntax.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, Type[] parameters, string[] parameterNames, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.IsStatic = isStatic;
            Context.Type = contextType;
            Context.ReturnType = null;
            Context.Parameters = parameters;
            Context.ParameterNames = parameterNames;
            return ParseExpression(expression, Context, null, out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="context">The emit context.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, EmitContext context, out string errorMessage, bool richTextError = true)
        {
            return ParseExpression(expression, context, null, out errorMessage, richTextError);
        }

        /// <summary>Parses an expression and tries to emit a delegate of the specified type.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="context">The emit context.</param>
        /// <param name="delegateType">The type of the delegate to emit.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, EmitContext context, Type delegateType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            errorMessage = null;
            try
            {
                Tokenizer.SetExpressionString(expression);
                return Emitter.EmitMethod("$Expression(" + expression + ")_" + Guid.NewGuid().ToString(), Parser.Parse(), context, delegateType);
            }
            catch (SyntaxException ex)
            {
                errorMessage = ex.GetNiceErrorMessage(expression, richTextError);
                return null;
            }
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<TResult> ParseFunc<TResult>(string expression, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = true;
            Context.Parameters = Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionFunc<TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, TResult> ParseFunc<T1, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1) }
                : Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, TResult> ParseFunc<T1, T2, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2) }
                : new Type[] { typeof(T2) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, TResult> ParseFunc<T1, T2, T3, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3) }
                : new Type[] { typeof(T2), typeof(T3) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, TResult> ParseFunc<T1, T2, T3, T4, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, TResult> ParseFunc<T1, T2, T3, T4, T5, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction ParseAction(string expression, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = true;
            Context.Parameters = Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionAction)ParseExpression(expression, Context, typeof(ExpressionAction), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1> ParseAction<T1>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1) }
                : Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionAction<T1>)ParseExpression(expression, Context, typeof(ExpressionAction<T1>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2> ParseAction<T1, T2>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2) }
                : new Type[] { typeof(T2) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3> ParseAction<T1, T2, T3>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3) }
                : new Type[] { typeof(T2), typeof(T3) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4> ParseAction<T1, T2, T3, T4>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5> ParseAction<T1, T2, T3, T4, T5>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6> ParseAction<T1, T2, T3, T4, T5, T6>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7> ParseAction<T1, T2, T3, T4, T5, T6, T7>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8> ParseAction<T1, T2, T3, T4, T5, T6, T7, T8>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> ParseAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
    }
}
#endif