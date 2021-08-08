#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StringMemberHelper.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using System;
    using UnityEngine;

    /// <summary>
    /// This class has been made fully obsolete, and has been replaced by <see cref="Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolver" />. 
    /// It was a helper class to handle strings for labels and other similar purposes.
    /// </summary>
    [Obsolete("StringMemberHelper is obsolete. Use the ValueResolver system instead.",
#if SIRENIX_INTERNAL
        true
#else
        false
#endif
        )]
    public class StringMemberHelper
    {
        private string buffer;
        private ValueResolver<string> resolver;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage { get { return this.resolver.ErrorMessage; } }

        /// <summary>
        /// Obsolete. Use other constructor.
        /// </summary>
        [Obsolete("Use a contructor with an InspectorProperty argument instead.", true)]
        public StringMemberHelper(Type objectType, string path, bool allowInstanceMember = true, bool allowStaticMember = true)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Obsolete. Use other constructor.
        /// </summary>
        [Obsolete("Use a contructor with an InspectorProperty argument instead.", true)]
        public StringMemberHelper(Type objectType, string path, ref string errorMessage, bool allowInstanceMember = true, bool allowStaticMember = true)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use a contructor with an InspectorProperty argument instead.", true)]
        public StringMemberHelper(Type objectType, bool isStatic, string text)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a StringMemberHelper to get a display string.
        /// </summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        public StringMemberHelper(InspectorProperty property, string text)
        {
            this.resolver = ValueResolver.GetForString(property, text);
        }

        /// <summary>
        /// Creates a StringMemberHelper to get a display string.
        /// </summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>/// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        public StringMemberHelper(InspectorProperty property, string text, ref string errorMessage) : this(property, text)
        {
            if (errorMessage == null)
            {
                errorMessage = this.ErrorMessage;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the string is retrived from a from a member. 
        /// </summary>
        public bool IsDynamicString
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        public Type ObjectType { get { return this.resolver.Context.Property.ParentType; } }

        /// <summary>
        /// Gets the string from the StringMemberHelper.
        /// Only updates the string buffer in Layout events.
        /// </summary>
        /// <param name="entry">The property entry, to get the instance reference from.</param>
        /// <returns>The current display string.</returns>
        public string GetString(IPropertyValueEntry entry)
        {
            return this.GetString(entry.Property.ParentValues[0]);
        }

        /// <summary>
        /// Gets the string from the StringMemberHelper.
        /// Only updates the string buffer in Layout events.
        /// </summary>
        /// <param name="property">The property, to get the instance reference from.</param>
        /// <returns>The current string.</returns>
        public string GetString(InspectorProperty property)
        {
            return this.GetString(property.ParentValues[0]);
        }

        /// <summary>
        /// Gets the string from the StringMemberHelper.
        /// Only updates the string buffer in Layout events.
        /// </summary>
        /// <param name="instance">The instance, for evt. member references.</param>
        /// <returns>The current string.</returns>
        public string GetString(object instance)
        {
            if (this.buffer == null || Event.current == null || Event.current.type == EventType.Layout)
            {
                this.buffer = this.ForceGetString(instance);
            }

            return this.buffer;
        }

        /// <summary>
        /// Gets the string from the StringMemberHelper.
        /// </summary>
        /// <param name="entry">The property entry, to get the instance reference from.</param>
        /// <returns>The current string.</returns>
        public string ForceGetString(IPropertyValueEntry entry)
        {
            if (entry.Property != this.resolver.Context.Property) throw new ArgumentException("You *must* provide the entry for the property to this call that you originally provided to the constructor. Yes, this is silly. That's why this class is obsolete!");

            return this.resolver.GetValue();
        }

        /// <summary>
        /// Gets the string from the StringMemberHelper.
        /// </summary>
        /// <param name="property">The property, to get the instance reference from.</param>
        /// <returns>The current string.</returns>
        public string ForceGetString(InspectorProperty property)
        {
            if (property != this.resolver.Context.Property) throw new ArgumentException("You *must* provide the property to this call that you originally provided to the constructor. Yes, this is silly. That's why this class is obsolete!");

            return this.resolver.GetValue();
        }

        /// <summary>
        /// Gets the string from the StringMemberHelper.
        /// </summary>
        /// <param name="instance">The instance, for evt. member references.</param>
        /// <returns>The current string.</returns>
        public string ForceGetString(object instance)
        {
            return this.resolver.GetValue();
        }
    }
}
#endif