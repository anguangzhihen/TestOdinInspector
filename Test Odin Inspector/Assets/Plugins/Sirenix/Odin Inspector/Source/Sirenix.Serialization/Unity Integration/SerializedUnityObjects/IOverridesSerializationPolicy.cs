//-----------------------------------------------------------------------
// <copyright file="IOverridesSerializationPolicy.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Indicates that an Odin-serialized Unity object provides its own serialization policy rather than using the default policy.
    /// <para/>
    /// Note that THE VALUES RETURNED BY THIS INTERFACE WILL OVERRIDE THE PARAMETERS PASSED TO <see cref="UnitySerializationUtility.SerializeUnityObject(UnityEngine.Object, ref SerializationData, bool, SerializationContext)"/> and <see cref="UnitySerializationUtility.DeserializeUnityObject(UnityEngine.Object, ref SerializationData, DeserializationContext)"/>.
    /// </summary>
    public interface IOverridesSerializationPolicy
    {
        ISerializationPolicy SerializationPolicy { get; }
        bool OdinSerializesUnityFields { get; }
    }
}