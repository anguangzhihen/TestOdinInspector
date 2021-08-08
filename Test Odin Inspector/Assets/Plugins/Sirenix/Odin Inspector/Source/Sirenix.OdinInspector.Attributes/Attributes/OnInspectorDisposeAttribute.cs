//-----------------------------------------------------------------------
// <copyright file="OnInspectorDisposeAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>The OnInspectorDispose attribute takes in an action string as an argument (typically the name of a method to be invoked, or an expression to be executed), and executes that action when the property's drawers are disposed in the inspector.</para>
    /// <para>Disposing will happen at least once, when the inspector changes selection or the property tree is collected by the garbage collector, but may also happen several times before that, most often when the type of a polymorphic property changes and it refreshes its drawer setup and recreates all its children, disposing of the old setup and children.</para>
    /// </summary>
    /// <example>
    /// <para>The following example demonstrates how OnInspectorDispose works.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [OnInspectorDispose(@"@UnityEngine.Debug.Log(""Dispose event invoked!"")")]
    ///     [ShowInInspector, InfoBox("When you change the type of this field, or set it to null, the former property setup is disposed. The property setup will also be disposed when you deselect this example."), DisplayAsString]
    ///     public BaseClass PolymorphicField;
    ///     
    ///     public abstract class BaseClass { public override string ToString() { return this.GetType().Name; } }
    ///     public class A : BaseClass { }
    ///     public class B : BaseClass { }
    ///     public class C : BaseClass { }
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [DontApplyToListElements]
    [IncludeMyAttributes, HideInTables]
    public class OnInspectorDisposeAttribute : ShowInInspectorAttribute
    {
        public string Action;

        /// <summary>
        /// This constructor should be used when the attribute is placed directly on a method.
        /// </summary>
        public OnInspectorDisposeAttribute()
        {
        }

        /// <summary>
        /// This constructor should be used when the attribute is placed on a non-method member.
        /// </summary>
        public OnInspectorDisposeAttribute(string action)
        {
            this.Action = action;
        }
    }
}