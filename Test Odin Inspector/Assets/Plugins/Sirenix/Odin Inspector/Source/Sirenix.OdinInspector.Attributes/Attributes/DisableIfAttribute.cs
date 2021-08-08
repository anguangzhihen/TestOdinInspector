//-----------------------------------------------------------------------
// <copyright file="DisableIfAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>DisableIf is used on any property, and can disable or enable the property in the inspector.</para>
    /// <para>Use this to disable properties when they are irrelevant.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how a property can be disabled by the state of a field.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		public bool DisableProperty;
    ///
    ///		[DisableIf("DisableProperty")]
    ///		public int MyInt;
    ///		
    ///	    public SomeEnum SomeEnumField;
    ///		
    ///		[DisableIf("SomeEnumField", SomeEnum.SomeEnumMember)]
    ///		public string SomeString;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following examples show how a property can be disabled by a function.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[EnableIf("MyDisableFunction")]
    ///		public int MyInt;
    ///
    ///		private bool MyDisableFunction()
    ///		{
    ///			// ...
    ///		}
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="ShowIfAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class DisableIfAttribute : Attribute
    {
        /// <summary>
        /// The name of a bool member field, property or method. Obsolete; use the Condition member instead.
        /// </summary>
        [Obsolete("Use the Condition member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string MemberName { get { return this.Condition; } set { this.Condition = value; } }

        /// <summary>
        /// A resolved string that defines the condition to check the value of, such as a member name or an expression.
        /// </summary>
        public string Condition;

        /// <summary>
        /// The optional condition value.
        /// </summary>
        public object Value;

        /// <summary>
        /// Disables a property in the inspector, based on the value of a resolved string.
        /// </summary>
        /// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
        public DisableIfAttribute(string condition)
        {
            this.Condition = condition;
        }

        /// <summary>
        /// Disables a property in the inspector, if the resolved string evaluates to the specified value.
        /// </summary>
        /// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
        /// <param name="optionalValue">Value to check against.</param>
        public DisableIfAttribute(string condition, object optionalValue)
        {
            this.Condition = condition;
            this.Value = optionalValue;
        }
    }
}