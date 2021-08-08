#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SerializationBackend.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Class that describes the different possible serialization backends that a property can have,
    /// and specifies the capabilities of each backend.
    /// </summary>
    public abstract class SerializationBackend
    {
        /// <summary>
        /// The property is serialized by Unity's polymorphic serialization backend via the [SerializeReference] attribute. Polymorphism, null values and cyclical references are supported.
        /// </summary>
        public static readonly SerializationBackend UnityPolymorphic = new UnityPolymorphicSerializationBackend();

        /// <summary>
        /// The property is serialized by Unity's classic serialization backend. Polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are not supported.
        /// </summary>
        public static readonly SerializationBackend Unity = new UnityClassicSerializationBackend();
        
        /// <summary>
        /// The property is serialized by Odin. Polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are supported.
        /// </summary>
        public static readonly SerializationBackend Odin = new OdinSerializationBackend();

        /// <summary>
        /// <para>The property is not serialized by anything - possibly because it is a method, possibly because it is a field or property shown in the inspector without being serialized.</para>
        /// <para>In the case of fields or properties, polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are supported, but will not be saved.</para>
        /// </summary>
        public static readonly SerializationBackend None = new NoneSerializationBackend();

        public abstract bool SupportsGenerics { get; }
        public abstract bool SupportsPolymorphism { get; }
        public abstract bool SupportsCyclicReferences { get; }
        public abstract bool IsUnity { get; }

        public abstract bool CanSerializeType(Type type);
        public abstract bool CanSerializeMember(MemberInfo member);
    }

    /// <summary>
    /// <para>The property is not serialized by anything - possibly because it is a method, possibly because it is a field or property shown in the inspector without being serialized.</para>
    /// <para>In the case of fields or properties, polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are supported, but will not be saved.</para>
    /// </summary>
    public class NoneSerializationBackend : SerializationBackend
    {
        public override string ToString()
        {
            return "None";
        }

        public override bool SupportsGenerics { get { return true; } }

        public override bool SupportsPolymorphism { get { return true; } }

        public override bool SupportsCyclicReferences { get { return true; } }

        public override bool IsUnity { get { return false; } }

        public override bool CanSerializeMember(MemberInfo member)
        {
            return Odin.CanSerializeMember(member) || member.IsDefined<ShowInInspectorAttribute>(true);
        }

        public override bool CanSerializeType(Type type)
        {
            return true;
        }
    }

    /// <summary>
    /// The property is serialized by Unity's classic serialization backend. Polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are not supported.
    /// </summary>
    public class UnityClassicSerializationBackend : SerializationBackend
    {
        public override string ToString()
        {
            return "Unity (Classic)";
        }

        public override bool SupportsGenerics { get { return UnityVersion.IsVersionOrGreater(2020, 1); } }

        public override bool SupportsPolymorphism { get { return false; } }

        public override bool SupportsCyclicReferences { get { return false; } }

        public override bool IsUnity { get { return true; } }

        public override bool CanSerializeMember(MemberInfo member)
        {
            return UnitySerializationUtility.GuessIfUnityWillSerialize(member);
        }

        public override bool CanSerializeType(Type type)
        {
            return UnitySerializationUtility.GuessIfUnityWillSerialize(type);
        }
    }

    /// <summary>
    /// The property is serialized by Unity's polymorphic serialization backend via the [SerializeReference] attribute. Polymorphism, null values and cyclical references are supported.
    /// </summary>
    public class UnityPolymorphicSerializationBackend : SerializationBackend
    {
        public override string ToString()
        {
            return "Unity (Polymorphic)";
        }

        public static readonly Type SerializeReferenceAttribute = typeof(SerializeField).Assembly.GetType("UnityEngine.SerializeReference");

        public override bool SupportsGenerics { get { return true; } }

        public override bool SupportsPolymorphism { get { return true; } }

        public override bool SupportsCyclicReferences { get { return true; } }

        public override bool IsUnity { get { return true; } }

        public override bool CanSerializeMember(MemberInfo member)
        {
            if (SerializeReferenceAttribute == null) return false;

            try
            {
                return member is FieldInfo && member.IsDefined(SerializeReferenceAttribute, false);
            }
            catch
            {
                return false;
            }
        }

        public override bool CanSerializeType(Type type)
        {
            return SerializeReferenceAttribute != null;
        }
    }

    /// <summary>
    /// The property is serialized by Odin. Polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are supported.
    /// </summary>
    public class OdinSerializationBackend : SerializationBackend
    {
        public override string ToString()
        {
            return "Odin";
        }

        public override bool SupportsGenerics { get { return true; } }

        public override bool SupportsPolymorphism { get { return true; } }

        public override bool SupportsCyclicReferences { get { return true; } }

        public override bool IsUnity { get { return false; } }

        public override bool CanSerializeMember(MemberInfo member)
        {
            return SerializationPolicies.Unity.ShouldSerializeMember(member);
        }

        public override bool CanSerializeType(Type type)
        {
            return true;
        }
    }
}
#endif