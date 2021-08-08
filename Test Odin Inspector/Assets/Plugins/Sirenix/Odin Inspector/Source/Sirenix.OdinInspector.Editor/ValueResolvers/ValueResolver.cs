#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValueResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using System;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// <para>A ValueResolver resolves a string to a value of a given type, given an InspectorProperty instance to use as context. Call <see cref="ValueResolver.Get{TResult}(InspectorProperty, string)"/> to get an instance of a ValueResolver.</para>
    /// <para>Value resolvers are a globally extendable system that can be hooked into and modified or changed by creating and registering a <see cref="ValueResolverCreator"/>.</para>
    /// <para>See Odin's tutorials for details and examples of how to use ValueResolvers.</para>
    /// </summary>
    public abstract class ValueResolver
    {
        private static readonly StringBuilder SB = new StringBuilder();

        /// <summary>
        /// The context of this ValueResolver, containing all of its configurations and values it needs to function. For performance and simplicity reasons, this is a single very large struct that is passed around by ref to anything that needs it.
        /// </summary>
        public ValueResolverContext Context;

        /// <summary>
        /// The current error message that the resolver has, or null if there is no error message. This is a shortcut for writing "resolver.Context.ErrorMessage".
        /// </summary>
        public string ErrorMessage { get { return this.Context.ErrorMessage; } }

        /// <summary>
        /// Whether there is an error message at the moment. This is a shortcut for writing "resolver.Context.ErrorMessage != null".
        /// </summary>
        public bool HasError { get { return this.Context.ErrorMessage != null; } }

        /// <summary>
        /// The type of value that this resolver instance is supposed to get.
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        /// Gets a value from the value resolver in a weakly typed manner.
        /// </summary>
        /// <param name="selectionIndex">The selection index at which to get the value, in the case of multi-selection. Defaults to 0.</param>
        /// <returns>The value that was gotten.</returns>
        public abstract object GetWeakValue(int selectionIndex = 0);

        /// <summary>
        /// Draws an error message box if there is an error, and does nothing if there is no error.
        /// </summary>
        public void DrawError()
        {
            if (this.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.ErrorMessage);
            }
        }

        /// <summary>
        /// Creates a new value resolver instance in a weakly typed fashion, though the result is the same as using a strongly typed generic overload.
        /// This is useful when you don't know at compile time which type you want to resolve.
        /// </summary>
        /// <param name="resultType">The type of value that the new resolver should resolve.</param>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        public static ValueResolver Get(Type resultType, InspectorProperty property, string resolvedString)
        {
            return ValueResolverCreator.GetResolver(resultType, property, resolvedString);
        }

        /// <summary>
        /// Creates a new value resolver instance in a weakly typed fashion, though the result is the same as using a strongly typed generic overload.
        /// This is useful when you don't know at compile time which type you want to resolve.
        /// </summary>
        /// <param name="resultType">The type of value that the new resolver should resolve.</param>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        /// <param name="namedArgs">The extra named args that this resolver has access to. Passing in a named arg that already exists will silently override the pre-existing named arg.</param>
        public static ValueResolver Get(Type resultType, InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
        {
            return ValueResolverCreator.GetResolver(resultType, property, resolvedString, namedArgs);
        }

        /// <summary>
        /// Creates a new value resolver instance.
        /// </summary>
        /// <typeparam name="TResult">The type of value that the new resolver should resolve.</typeparam>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        public static ValueResolver<TResult> Get<TResult>(InspectorProperty property, string resolvedString)
        {
            return ValueResolverCreator.GetResolver<TResult>(property, resolvedString);
        }

        /// <summary>
        /// Creates a new value resolver instance.
        /// </summary>
        /// <typeparam name="TResult">The type of value that the new resolver should resolve.</typeparam>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        /// <param name="namedArgs">The extra named args that this resolver has access to. Passing in a named arg that already exists will silently override the pre-existing named arg.</param>
        public static ValueResolver<TResult> Get<TResult>(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
        {
            return ValueResolverCreator.GetResolver<TResult>(property, resolvedString, namedArgs);
        }

        /// <summary>
        /// Creates a new value resolver instance in a weakly typed fashion, though the result is the same as using a strongly typed generic overload.
        /// This is useful when you don't know at compile time which type you want to resolve.
        /// </summary>
        /// <param name="resultType">The type of value that the new resolver should resolve.</param>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        /// <param name="fallbackValue">The value that the resolver should return if the string cannot be resolved to anything, or if there is an error in creating a resolver, or if resolution itself throws an exception.</param>
        public static ValueResolver Get(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue)
        {
            return ValueResolverCreator.GetResolver(resultType, property, resolvedString, fallbackValue, null);
        }

        /// <summary>
        /// Creates a new value resolver instance in a weakly typed fashion, though the result is the same as using a strongly typed generic overload.
        /// This is useful when you don't know at compile time which type you want to resolve.
        /// </summary>
        /// <param name="resultType">The type of value that the new resolver should resolve.</param>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        /// <param name="fallbackValue">The value that the resolver should return if the string cannot be resolved to anything, or if there is an error in creating a resolver, or if resolution itself throws an exception.</param>
        /// <param name="namedArgs">The extra named args that this resolver has access to. Passing in a named arg that already exists will silently override the pre-existing named arg.</param>
        public static ValueResolver Get(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue, params NamedValue[] namedArgs)
        {
            return ValueResolverCreator.GetResolver(resultType, property, resolvedString, fallbackValue, namedArgs);
        }

        /// <summary>
        /// <para>Creates a new value resolver instance meant to resolve a string value in particular. This is a shorthand for creating a string resolver that has the resolved string as a fallback value.</para>
        /// <para>This special case will get you the behaviour where, if you pass in a string that is not meant to be resolved in a special way, the value resolver will just pass you that string back as the result value.</para>
        /// </summary>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        public static ValueResolver<string> GetForString(InspectorProperty property, string resolvedString)
        {
            return ValueResolverCreator.GetResolver<string>(property, resolvedString, resolvedString);
        }

        /// <summary>
        /// <para>Creates a new value resolver instance meant to resolve a string value in particular. This is a shorthand for creating a string resolver that has the resolved string as a fallback value.</para>
        /// <para>This special case will get you the behaviour where, if you pass in a string that is not meant to be resolved in a special way, the value resolver will just pass you that string back as the result value.</para>
        /// </summary>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        /// <param name="namedArgs">The extra named args that this resolver has access to. Passing in a named arg that already exists will silently override the pre-existing named arg.</param>
        public static ValueResolver<string> GetForString(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
        {
            return ValueResolverCreator.GetResolver<string>(property, resolvedString, resolvedString, namedArgs);
        }

        /// <summary>
        /// Creates a new value resolver instance.
        /// </summary>
        /// <typeparam name="TResult">The type of value that the new resolver should resolve.</typeparam>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        /// <param name="fallbackValue">The value that the resolver should return if the string cannot be resolved to anything, or if there is an error in creating a resolver, or if resolution itself throws an exception.</param>
        public static ValueResolver<TResult> Get<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue)
        {
            return ValueResolverCreator.GetResolver<TResult>(property, resolvedString, fallbackValue);
        }

        /// <summary>
        /// Creates a new value resolver instance.
        /// </summary>
        /// <typeparam name="TResult">The type of value that the new resolver should resolve.</typeparam>
        /// <param name="property">The property that is the context for the resolution to happen in.</param>
        /// <param name="resolvedString">The string that should be resolved to a value.</param>
        /// <param name="fallbackValue">The value that the resolver should return if the string cannot be resolved to anything, or if there is an error in creating a resolver, or if resolution itself throws an exception.</param>
        /// <param name="namedArgs">The extra named args that this resolver has access to. Passing in a named arg that already exists will silently override the pre-existing named arg.</param>
        public static ValueResolver<TResult> Get<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue, params NamedValue[] namedArgs)
        {
            return ValueResolverCreator.GetResolver<TResult>(property, resolvedString, fallbackValue, namedArgs);
        }

        /// <summary>
        /// Gets a nicely formatted string that lists all the errors in the given set of value resolvers. The returned value is null if there are no errors.
        /// </summary>
        public static string GetCombinedErrors(ValueResolver r1 = null, ValueResolver r2 = null, ValueResolver r3 = null, ValueResolver r4 = null, ValueResolver r5 = null, ValueResolver r6 = null, ValueResolver r7 = null, ValueResolver r8 = null)
        {
            return GetCombinedErrors(r1, r2, r3, r4, r5, r6, r7, r8, null);
        }

        /// <summary>
        /// Gets a nicely formatted string that lists all the errors in the given set of value resolvers. The returned value is null if there are no errors.
        /// </summary>
        public static string GetCombinedErrors(ValueResolver r1, ValueResolver r2, ValueResolver r3, ValueResolver r4, ValueResolver r5, ValueResolver r6, ValueResolver r7, ValueResolver r8, params ValueResolver[] remainder)
        {
            SB.Length = 0;

            if (r1 != null) { if (r1.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r1.ErrorMessage); } }
            if (r2 != null) { if (r2.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r2.ErrorMessage); } }
            if (r3 != null) { if (r3.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r3.ErrorMessage); } }
            if (r4 != null) { if (r4.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r4.ErrorMessage); } }
            if (r5 != null) { if (r5.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r5.ErrorMessage); } }
            if (r6 != null) { if (r6.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r6.ErrorMessage); } }
            if (r7 != null) { if (r7.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r7.ErrorMessage); } }
            if (r8 != null) { if (r8.ErrorMessage != null) { if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); } SB.Append(r8.ErrorMessage); } }

            if (remainder != null)
            {
                for (int i = 0; i < remainder.Length; i++)
                {
                    if (remainder[i] != null && remainder[i].HasError)
                    {
                        if (SB.Length > 0) { SB.AppendLine(); SB.AppendLine(); SB.AppendLine("And,"); SB.AppendLine(); }
                        SB.Append(remainder[i].ErrorMessage);
                    }
                }
            }

            return SB.Length == 0 ? null : SB.ToString();
        }

        /// <summary>
        /// Draws error boxes for all errors in the given value resolvers, or does nothing if there are no errors. This is equivalent to calling DrawError() on all resolvers passed to this method.
        /// </summary>
        public static void DrawErrors(ValueResolver r1 = null, ValueResolver r2 = null, ValueResolver r3 = null, ValueResolver r4 = null, ValueResolver r5 = null, ValueResolver r6 = null, ValueResolver r7 = null, ValueResolver r8 = null)
        {
            DrawErrors(r1, r2, r3, r4, r5, r6, r7, r8, null);
        }

        /// <summary>
        /// Draws error boxes for all errors in the given value resolvers, or does nothing if there are no errors. This is equivalent to calling DrawError() on all resolvers passed to this method.
        /// </summary>
        public static void DrawErrors(ValueResolver r1 = null, ValueResolver r2 = null, ValueResolver r3 = null, ValueResolver r4 = null, ValueResolver r5 = null, ValueResolver r6 = null, ValueResolver r7 = null, ValueResolver r8 = null, params ValueResolver[] remainder)
        {
            if (r1 != null) r1.DrawError();
            if (r2 != null) r2.DrawError();
            if (r3 != null) r3.DrawError();
            if (r4 != null) r4.DrawError();
            if (r5 != null) r5.DrawError();
            if (r6 != null) r6.DrawError();
            if (r7 != null) r7.DrawError();
            if (r8 != null) r8.DrawError();

            if (remainder != null)
            {
                for (int i = 0; i < remainder.Length; i++)
                {
                    if (remainder[i] != null)
                    {
                        remainder[i].DrawError();
                    }
                }
            }
        }
    }

    /// <summary>
    /// <para>A ValueResolver resolves a string to a value of a given type, given an InspectorProperty instance to use as context. Call <see cref="ValueResolver.Get{TResult}(InspectorProperty, string)"/> to get an instance of a ValueResolver.</para>
    /// <para>Value resolvers are a globally extendable system that can be hooked into and modified or changed by creating and registering a <see cref="ValueResolverCreator"/>.</para>
    /// <para>See Odin's tutorials for details and examples of how to use ValueResolvers.</para>
    /// </summary>
    public sealed class ValueResolver<TResult> : ValueResolver
    {
        /// <summary>
        /// The delegate that does the actual value resolution. You should not call this manually, but instead call <see cref="GetValue(int)"/>.
        /// </summary>
        public ValueResolverFunc<TResult> Func;

        /// <summary>
        /// The type of value that this resolver instance is supposed to get. Always equal to typeof(<see cref="TResult"/>).
        /// </summary>
        public override Type ValueType { get { return typeof(TResult); } }

        /// <summary>
        /// Gets a value from the value resolver.
        /// </summary>
        /// <param name="selectionIndex">The selection index at which to get the value, in the case of multi-selection. Defaults to 0.</param>
        /// <returns>The value that was gotten.</returns>
        public TResult GetValue(int selectionIndex = 0)
        {
            if (selectionIndex < 0 || selectionIndex >= this.Context.Property.ParentValues.Count)
            {
                throw new IndexOutOfRangeException();
            }

            this.Context.NamedValues.UpdateValues(ref this.Context, selectionIndex);

            try
            {
                var result = this.Func(ref this.Context, selectionIndex);

                if (this.Context.ErrorMessage != null && this.Context.ErrorMessageIsDueToException)
                {
                    this.Context.ErrorMessage = null;
                    this.Context.ErrorMessageIsDueToException = false;
                }

                return result;
            }
            catch (Exception ex)
            {
                while (ex is System.Reflection.TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                if (Event.current == null) throw ex;
                if (ex.IsExitGUIException()) throw ex.AsExitGUIException();

                if (Event.current.type == EventType.Repaint)
                {
                    this.Context.ErrorMessage = "Value resolution threw an exception: " + ex.Message + "\n\n" + ex.StackTrace;
                    this.Context.ErrorMessageIsDueToException = true;
                }

                if (this.Context.LogExceptions)
                {
                    Debug.LogException(ex);
                }

                if (this.Context.HasFallbackValue)
                {
                    return (TResult)this.Context.FallbackValue;
                }

                return default(TResult);
            }
        }

        /// <summary>
        /// Gets a value from the value resolver in a weakly typed manner.
        /// </summary>
        /// <param name="selectionIndex">The selection index at which to get the value, in the case of multi-selection. Defaults to 0.</param>
        /// <returns>The value that was gotten.</returns>
        public override object GetWeakValue(int selectionIndex = 0)
        {
            return this.GetValue();
        }
    }
}
#endif