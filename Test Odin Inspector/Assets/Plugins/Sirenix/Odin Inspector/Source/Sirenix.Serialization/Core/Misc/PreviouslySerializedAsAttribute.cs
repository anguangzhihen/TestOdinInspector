//-----------------------------------------------------------------------
// <copyright file="PreviouslySerializedAsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Indicates that an instance field or auto-property was previously serialized with a different name, so that values serialized with the old name will be properly deserialized into this member.
    ///
    /// This does the same as Unity's FormerlySerializedAs attribute, except it can also be applied to properties.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PreviouslySerializedAsAttribute : Attribute
    {
        /// <summary>
        /// The former name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviouslySerializedAsAttribute"/> class.
        /// </summary>
        /// <param name="name">The former name.</param>
        public PreviouslySerializedAsAttribute(string name)
        {
            this.Name = name;
        }
    }
}