//-----------------------------------------------------------------------
// <copyright file="OnStateUpdateAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

	/// <summary>
	/// <para>
	///    OnStateUpdate provides an event callback when the property's state should be updated, when the StateUpdaters run on the property instance.
	///    This generally happens at least once per frame, and the callback will be invoked even when the property is not visible. This can be used to
	///    approximate custom StateUpdaters like [ShowIf] without needing to make entire attributes and StateUpdaters for one-off cases.
	/// </para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how OnStateUpdate can be used to control the visible state of a property.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[OnStateUpdate("@$property.State.Visible = ToggleMyInt")]
	///		public int MyInt;
	///
	///		public bool ToggleMyInt;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how OnStateUpdate can be used to control the expanded state of a list.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[OnStateUpdate("@$property.State.Expanded = ExpandList")]
	///		public List&lt;string&gt; list;
	///		
	///		public bool ExpandList;
	/// }
	/// </code>
	/// <para>The following example shows how OnStateUpdate can be used to control the state of another property.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		public List&gt;string&lt; list;
	///		
	///		[OnStateUpdate("@#(list).State.Expanded = $value")]
	///		public bool ExpandList;
	/// }
	/// </code>
	/// </example>
	[DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [IncludeMyAttributes, HideInTables]
    public sealed class OnStateUpdateAttribute : Attribute
    {
        public string Action;

        public OnStateUpdateAttribute(string action)
        {
            this.Action = action;
        }
    }
}