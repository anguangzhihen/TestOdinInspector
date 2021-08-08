//-----------------------------------------------------------------------
// <copyright file="OnValueChangedAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    ///    OnValueChanged works on properties and fields, and calls the specified function 
    ///    whenever the value has been changed via the inspector.
    /// </para>
    /// </summary>
	/// <remarks>
    /// <note type="note">Note that this attribute only works in the editor! Properties changed by script will not call the function.</note>
    /// </remarks>
	/// <example>
    /// <para>The following example shows how OnValueChanged is used to provide a callback for a property.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	/// {
	///		[OnValueChanged("MyCallback")]
	///		public int MyInt;
	///
	///		private void MyCallback()
	///		{
	///			// ..
	///		}
	/// }
    /// </code>
    /// </example>
	/// <example>
    /// <para>The following example show how OnValueChanged can be used to get a component from a prefab property.</para>
    /// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[OnValueChanged("OnPrefabChange")]
	///		public GameObject MyPrefab;
	///
	///		// RigidBody component of MyPrefab.
	///		[SerializeField, HideInInspector]
	///		private RigidBody myPrefabRigidbody;
	///
	///		private void OnPrefabChange()
	///		{
	///			if(MyPrefab != null)
	///			{
	///				myPrefabRigidbody = MyPrefab.GetComponent&lt;Rigidbody&gt;();
	///			}
	///			else
	///			{
	///				myPrefabRigidbody = null;
	///			}
	///		}
	/// }
    /// </code>
    /// </example>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class OnValueChangedAttribute : Attribute
    {
        /// <summary>
        /// Name of callback member function. Obsolete; use the Action member instead.
        /// </summary>
        [Obsolete("Use the Action member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
        public string MethodName { get { return this.Action; } set { this.Action = value; } }

        /// <summary>
		/// A resolved string that defines the action to perform when the value is changed, such as an expression or method invocation.
        /// </summary>
        public string Action;

        /// <summary>
        /// Whether to perform the action when a child value of the property is changed.
        /// </summary>
        public bool IncludeChildren;

        /// <summary>
        /// Whether to perform the action when an undo or redo event occurs via UnityEditor.Undo.undoRedoPerformed. True by default.
        /// </summary>
        public bool InvokeOnUndoRedo = true;

        /// <summary>
        /// Whether to perform the action when the property is initialized. This will generally happen when the property is first viewed/queried (IE when the inspector is first opened, or when its containing foldout is first expanded, etc), and whenever its type or a parent type changes, or it is otherwise forced to rebuild.
        /// </summary>
        public bool InvokeOnInitialize = false;

        /// <summary>
        /// Adds a callback for when the property's value is changed.
        /// </summary>
        /// <param name="action">A resolved string that defines the action to perform when the value is changed, such as an expression or method invocation.</param>
		/// <param name="includeChildren">Whether to perform the action when a child value of the property is changed.</param>
        public OnValueChangedAttribute(string action, bool includeChildren = false)
        {
            this.Action = action;
            this.IncludeChildren = includeChildren;
        }
    }
}