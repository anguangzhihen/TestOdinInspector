//-----------------------------------------------------------------------
// <copyright file="EnableIfAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>EnableIf is used on any property, and can enable or disable the property in the inspector.</para>
    /// <para>Use this to enable properties when they are relevant.</para>
    /// </summary>
	/// <example>
    /// <para>The following example shows how a property can be enabled by the state of a field.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	/// {
	///		public bool EnableProperty;
	///
	///		[EnableIf("EnableProperty")]
	///		public int MyInt;
    ///		
    ///	    public SomeEnum SomeEnumField;
    ///		
    ///		[EnableIf("SomeEnumField", SomeEnum.SomeEnumMember)]
    ///		public string SomeString;
	/// }
    /// </code>
    /// </example>
	/// <example>
    /// <para>The following examples show how a property can be enabled by a function.</para>
    /// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[EnableIf("MyEnableFunction")]
	///		public int MyInt;
	///
	///		private bool MyEnableFunction()
	///		{
	///			// ...
	///		}
	/// }
    /// </code>
    /// </example>
	/// <seealso cref="DisableIfAttribute"/>
	/// <seealso cref="ShowIfAttribute"/>
	/// <seealso cref="HideIfAttribute"/>
	/// <seealso cref="DisableInEditorModeAttribute"/>
	/// <seealso cref="DisableInPlayModeAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class EnableIfAttribute : Attribute
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
        /// Enables a property in the inspector, based on the value of a resolved string.
        /// </summary>
        /// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
        public EnableIfAttribute(string condition)
        {
            this.Condition = condition;
        }

        /// <summary>
        /// Enables a property in the inspector, if the resolved string evaluates to the specified value.
        /// </summary>
        /// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
        /// <param name="optionalValue">Value to check against.</param>
        public EnableIfAttribute(string condition, object optionalValue)
        {
            this.Condition = condition;
            this.Value = optionalValue;
        }
    }
}