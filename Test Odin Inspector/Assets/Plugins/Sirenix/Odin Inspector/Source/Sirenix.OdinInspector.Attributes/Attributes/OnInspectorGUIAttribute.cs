//-----------------------------------------------------------------------
// <copyright file="OnInspectorGUIAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>OnInspectorGUI is used on any property, and will call the specified function whenever the inspector code is running.</para>
    /// <para>Use this to create custom inspector GUI for an object.</para>
    /// </summary>
	/// <example>
    /// <para></para>
    /// <code>
    /// public MyComponent : MonoBehaviour
	/// {
	///		[OnInspectorGUI]
	///		private void MyInspectorGUI()
	///		{
	///			GUILayout.Label("Label drawn from callback");
	///		}
	/// }
    /// </code>
    /// </example>
	/// <example>
    ///	<para>The following example shows how a callback can be set before another property.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
	/// {
	///		[OnInspectorGUI("MyInspectorGUI", false)]
	///		public int MyField;
	///
	///		private void MyInspectorGUI()
	///		{
	///			GUILayout.Label("Label before My Field property");
	///		}
	/// }
    /// </code>
    /// </example>
	/// <example>
    ///	<para>The following example shows how callbacks can be added both before and after a property.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
	/// {
	///		[OnInspectorGUI("GUIBefore", "GUIAfter")]
	///		public int MyField;
	///
	///		private void GUIBefore()
	///		{
	///			GUILayout.Label("Label before My Field property");
	///		}
    ///
	///		private void GUIAfter()
	///		{
	///			GUILayout.Label("Label after My Field property");
	///		}
	/// }
    /// </code>
    /// </example>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class OnInspectorGUIAttribute : ShowInInspectorAttribute
    {
        /// <summary>
        /// The resolved action string that defines the action to be invoked before the property is drawn, if any.
        /// </summary>
        public string Prepend;

        /// <summary>
        /// The resolved action string that defines the action to be invoked after the property is drawn, if any.
        /// </summary>
        public string Append;

        /// <summary>
        /// The name of the method to be called before the property is drawn, if any. Obsolete; use the Prepend member instead.
        /// </summary>
        [Obsolete("Use the Prepend member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string PrependMethodName;

        /// <summary>
        /// The name of the method to be called after the property is drawn, if any. Obsolete; use the Append member instead.
        /// </summary>
        [Obsolete("Use the Append member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string AppendMethodName;

        /// <summary>
        /// Calls a function decorated with this attribute, when the inspector is being drawn.
        /// </summary>
        public OnInspectorGUIAttribute()
        {
        }

        /// <summary>
        /// Adds callbacks to the specified action when the property is being drawn.
        /// </summary>
        /// <param name="action">The resolved action string that defines the action to be invoked.</param>
        /// <param name="append">If <c>true</c> the method will be called after the property has been drawn. Otherwise the method will be called before.</param>
        public OnInspectorGUIAttribute(string action, bool append = true)
        {
            if (append)
            {
                this.Append = action;
            }
            else
            {
                this.Prepend = action;
            }
        }

        /// <summary>
        /// Adds callbacks to the specified actions when the property is being drawn.
        /// </summary>
        /// <param name="prepend">The resolved action string that defines the action to be invoked before the property is drawn, if any.</param>
        /// <param name="append">The resolved action string that defines the action to be invoked after the property is drawn, if any.</param>
        public OnInspectorGUIAttribute(string prepend, string append)
        {
            this.Prepend = prepend;
            this.Append = append;
        }
    }
}