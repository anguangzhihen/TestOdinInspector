#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MemberSerializationInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Sirenix.OdinInspector.Internal;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal static class SerializationFlagsExtensions
    {
        public static bool HasAny(this SerializationFlags e, SerializationFlags flags)
        {
            return (e & flags) != 0;
        }

        public static bool HasAll(this SerializationFlags e, SerializationFlags flags)
        {
            return (e & flags) == flags;
        }

        public static bool HasNone(this SerializationFlags e, SerializationFlags flags)
        {
            return (e & flags) == 0;
        }

        public static bool HasAny(this SerializationBackendFlags e, SerializationBackendFlags flags)
        {
            return (e & flags) != 0;
        }

        public static bool HasAll(this SerializationBackendFlags e, SerializationBackendFlags flags)
        {
            return (e & flags) == flags;
        }

        public static bool HasNone(this SerializationBackendFlags e, SerializationBackendFlags flags)
        {
            return (e & flags) == 0;
        }
    }

    internal class MemberSerializationInfo
    {
        public readonly string[] Notes;

        public readonly MemberInfo MemberInfo;

        public readonly SerializationFlags Info;

        public readonly SerializationBackendFlags Backend;

        public readonly InfoMessageType OdinMessageType;

        public readonly InfoMessageType UnityMessageType;

        private MemberSerializationInfo(MemberInfo member, string[] notes, SerializationFlags flags, SerializationBackendFlags serializationBackend)
        {
            Assert.IsNotNull(member);
            Assert.IsFalse(flags == 0);

            this.MemberInfo = member;
            this.Notes = notes;
            this.Info = flags;
            this.Backend = serializationBackend;

            this.OdinMessageType = InfoMessageType.None;
            this.UnityMessageType = InfoMessageType.None;

            // Should the member be serialized, but isn't?
            if (flags.HasAll(SerializationFlags.DefaultSerializationPolicy))
            {
                if (flags.HasNone(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin | SerializationFlags.NonSerializedAttribute) &&
                    flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute) &&
                    (flags.HasAll(SerializationFlags.Field) || flags.HasAll(SerializationFlags.AutoProperty)))
                {
                    if (serializationBackend.HasNone(SerializationBackendFlags.Odin))
                    {
                        this.OdinMessageType = InfoMessageType.Info;
                    }

                    if (flags.HasNone(SerializationFlags.Property) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()) == false)
                    {
                        this.UnityMessageType = InfoMessageType.Info;
                    }
                }
            }

            // Is the member serialized by both Odin and Unity?
            if (this.Info.HasAny(SerializationFlags.SerializedByOdin) && this.Info.HasAny(SerializationFlags.SerializedByUnity))
            {
                this.OdinMessageType = InfoMessageType.Warning;
                this.UnityMessageType = InfoMessageType.Warning;
            }

            // Does the member have both SerializeField and NonSerialized attributes?
            if (this.Info.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute))
            {
                this.UnityMessageType = InfoMessageType.Warning;
            }

            // Does the member have both SerializeField and OdinSerialize attributes?
            if (this.Info.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
            {
                if (this.Info.HasAll(SerializationFlags.SerializedByOdin))
                {
                    this.OdinMessageType = InfoMessageType.Warning;
                }
                if (this.Info.HasAll(SerializationFlags.SerializedByUnity))
                {
                    this.UnityMessageType = InfoMessageType.Warning;
                }
            }

            if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin) && this.Info.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.TypeSupportedByUnity) && this.Info.HasNone(SerializationFlags.SerializedByUnity | SerializationFlags.Property))
            {
                this.OdinMessageType = InfoMessageType.Warning;
            }

            // Does the member have a OdinSerialize attribute, but no Odin backend is available?
            if (!serializationBackend.HasAll(SerializationBackendFlags.Odin) &&
                flags.HasAny(SerializationFlags.OdinSerializeAttribute))
            {
                this.OdinMessageType = InfoMessageType.Error;
            }

            // Does the member have OdinSerialize attribute, but is not serialized by Odin?
            if (this.Info.HasAll(SerializationFlags.DefaultSerializationPolicy) && this.Info.HasAny(SerializationFlags.OdinSerializeAttribute) && !this.Info.HasAny(SerializationFlags.SerializedByOdin))
            {
                this.OdinMessageType = InfoMessageType.Error;
            }

            // Is the member marked for serialization but not serialized?
            if (this.Info.HasAny(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute) &&
                !this.Info.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin | SerializationFlags.NonSerializedAttribute))
            {
                if (serializationBackend.HasAll(SerializationBackendFlags.Odin))
                {
                    this.OdinMessageType = InfoMessageType.Error;
                }

                if (!this.Info.HasAny(SerializationFlags.Property) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
                {
                    this.UnityMessageType = InfoMessageType.Error;
                }
            }

            // Is the member public, not marked with NonSerialized, but not serialized?
            if (this.Info.HasAll(SerializationFlags.Public | SerializationFlags.Field) &&
                !this.Info.HasAny(SerializationFlags.NonSerializedAttribute) &&
                !this.Info.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
            {
                if (this.Info.HasAll(SerializationFlags.DefaultSerializationPolicy) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
                {
                    this.OdinMessageType = InfoMessageType.Error;
                }

                if (!this.Info.HasAny(SerializationFlags.Property) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
                {
                    this.UnityMessageType = InfoMessageType.Error;
                }
            }
        }

        public static List<MemberSerializationInfo> CreateSerializationOverview(Type type, SerializationBackendFlags serializationBackend, bool includeBaseTypes)
        {
            bool serializeUnityFields;
            var serializationPolicy = GetSerializationPolicy(type, out serializeUnityFields);

            return type.GetAllMembers(includeBaseTypes ? Flags.InstanceAnyVisibility : Flags.InstanceAnyDeclaredOnly)
                .Where(x => x is FieldInfo || x is PropertyInfo)
                .Where(x => !x.Name.StartsWith("<")) // Excludes backing fields.
                .Where(x => (x.DeclaringType.Assembly.GetAssemblyTypeFlag() & (AssemblyTypeFlags.UnityEditorTypes | AssemblyTypeFlags.UnityTypes)) == 0) // Exclude members from UnityEngine types
                .Where(x => !x.DeclaringType.Assembly.FullName.StartsWith("Sirenix."))
                .Select(x => CreateInfoFor(x, serializationBackend, serializeUnityFields, serializationPolicy)) // Creates MemberSerializationInfo
                .OrderByDescending(x => x.OdinMessageType == InfoMessageType.Error)
                .ThenByDescending(x => x.UnityMessageType == InfoMessageType.Error)
                .ThenByDescending(x => x.OdinMessageType == InfoMessageType.Warning)
                .ThenByDescending(x => x.UnityMessageType == InfoMessageType.Warning)
                .ThenByDescending(x => x.OdinMessageType == InfoMessageType.Info)
                .ThenByDescending(x => x.UnityMessageType == InfoMessageType.Info)
                .ThenByDescending(x => x.Info.HasAny(SerializationFlags.SerializedByOdin))
                .ThenByDescending(x => x.Info.HasAny(SerializationFlags.SerializedByUnity))
                .ThenByDescending(x => x.MemberInfo.Name)
                .ToList();
        }

        private static ISerializationPolicy GetSerializationPolicy(Type type, out bool serializeUnityFields)
        {
            serializeUnityFields = false;
            if (!typeof(IOverridesSerializationPolicy).IsAssignableFrom(type)) return SerializationPolicies.Unity;

            IOverridesSerializationPolicy policyOverride = null;

            var objects = Resources.FindObjectsOfTypeAll(type);

            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                if (!object.ReferenceEquals(obj, null))
                {
                    policyOverride = obj as IOverridesSerializationPolicy;
                    break;
                }
            }

            if (!type.IsAbstract && !type.IsGenericTypeDefinition)
            {
                var inst = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                policyOverride = inst as IOverridesSerializationPolicy;
            }

            if (policyOverride != null)
            {
                serializeUnityFields = policyOverride.OdinSerializesUnityFields;
                return policyOverride.SerializationPolicy ?? SerializationPolicies.Unity;
            }

            return SerializationPolicies.Unity;
        }

        private static MemberSerializationInfo CreateInfoFor(MemberInfo member, SerializationBackendFlags serializationBackend, bool serializeUnityFields, ISerializationPolicy serializationPolicy)
        {
            SerializationFlags flags = 0;

            // Is the member a field, property or auto-property?
            if (member is FieldInfo)
            {
                var f = member as FieldInfo;
                flags |= SerializationFlags.Field;

                if (f.IsPublic)
                {
                    flags |= SerializationFlags.Public;
                }
            }
            else if (member is PropertyInfo)
            {
                var p = member as PropertyInfo;
                flags |= SerializationFlags.Property;

                if (p.GetGetMethod() != null && p.GetGetMethod().IsPublic || p.GetSetMethod() != null && p.GetSetMethod().IsPublic)
                {
                    flags |= SerializationFlags.Public;
                }
                if (p.IsAutoProperty(true))
                {
                    flags |= SerializationFlags.AutoProperty;
                }
            }

            // Is the default serialization policy in use?
            if (serializationPolicy != null && serializationPolicy.ID == SerializationPolicies.Unity.ID)
            {
                flags |= SerializationFlags.DefaultSerializationPolicy;
            }

            // Will Unity serialize the member?
            if ((serializationBackend & SerializationBackendFlags.Unity) != 0 && UnitySerializationUtility.GuessIfUnityWillSerialize(member))
            {
                flags |= SerializationFlags.SerializedByUnity;
            }

            // Will Odin serialize the member?
            if ((serializationBackend & SerializationBackendFlags.Odin) != 0 && UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields, serializationPolicy))
            {
                flags |= SerializationFlags.SerializedByOdin;
            }

            // Does the member have a SerializeField attribute?
            if (member.IsDefined<SerializeField>())
            {
                flags |= SerializationFlags.SerializeFieldAttribute;
            }

            // Does the member have a OdinSerialize attribute?
            if (member.IsDefined<OdinSerializeAttribute>())
            {
                flags |= SerializationFlags.OdinSerializeAttribute;
            }

            // Does the member have a NonSerialized attribute?
            if (member.IsDefined<NonSerializedAttribute>())
            {
                flags |= SerializationFlags.NonSerializedAttribute;
            }

            // Does Unity support serializing the type?
            if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
            {
                flags |= SerializationFlags.TypeSupportedByUnity;
            }

            return new MemberSerializationInfo(member, CreateNotes(member, flags, serializationBackend, serializationPolicy.ID), flags, serializationBackend);
        }

        private static string[] CreateNotes(MemberInfo member, SerializationFlags flags, SerializationBackendFlags serializationBackend, string serializationPolicyId)
        {
            List<string> notes = new List<string>();
            StringBuilder buffer = new StringBuilder();

            // Handle the default serialization policy.
            if (serializationBackend.HasNone(SerializationBackendFlags.Odin) || flags.HasAll(SerializationFlags.DefaultSerializationPolicy))
            {
                // Member type
                if (flags.HasAll(SerializationFlags.Property | SerializationFlags.AutoProperty))
                {
                    buffer.AppendFormat("The auto property '{0}' ", member.GetNiceName());
                }
                else if (flags.HasAll(SerializationFlags.Property))
                {
                    buffer.AppendFormat("The non-auto property '{0}' ", member.GetNiceName());
                }
                else
                {
                    if (flags.HasAll(SerializationFlags.Public))
                    {
                        buffer.AppendFormat("The public field '{0}' ", member.GetNiceName());
                    }
                    else
                    {
                        buffer.AppendFormat("The field '{0}' ", member.GetNiceName());
                    }
                }

                // Is the member serialized?
                if (flags.HasAny(SerializationFlags.SerializedByOdin | SerializationFlags.SerializedByUnity))
                {
                    buffer.Append("is serialized by ");

                    // Who?
                    if (flags.HasAll(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
                    {
                        buffer.Append("both Unity and Odin ");
                    }
                    else if (flags.HasAll(SerializationFlags.SerializedByUnity))
                    {
                        buffer.Append("Unity ");
                    }
                    else
                    {
                        buffer.Append("Odin ");
                    }

                    buffer.Append("because ");

                    // Why?
                    var relevant = flags & (SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute);

                    if (flags.HasAll(SerializationFlags.OdinSerializeAttribute) && serializationBackend.HasAll(SerializationBackendFlags.Odin)) // The OdinSerialize attribute is only relevant when the Odin serialization backend is available.
                    {
                        relevant |= SerializationFlags.OdinSerializeAttribute;
                    }

                    if (flags.HasAll(SerializationFlags.Property | SerializationFlags.Public))
                    {
                        relevant &= ~SerializationFlags.Public;
                    }

                    switch (relevant)
                    {
                        case SerializationFlags.Public:
                            buffer.Append("its access modifier is public. ");
                            break;

                        case SerializationFlags.SerializeFieldAttribute:
                            buffer.Append("the [SerializeField] attribute is defined. ");
                            break;

                        case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute:
                            buffer.Append("the [SerializeField] attribute is defined, and it's public. ");
                            break;

                        case SerializationFlags.OdinSerializeAttribute:
                            buffer.Append("the [OdinSerialize] attribute is defined. ");
                            break;

                        case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute:
                            buffer.Append("the [OdinSerialize] attribute is defined, and it's public.");
                            break;

                        case SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute:
                            buffer.Append("the [SerializeField] and [OdinSerialize] attributes are defined. ");
                            break;

                        case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute:
                            buffer.Append("its access modifier is public and the [SerializeField] and [OdinSerialize] attribute are defined. ");
                            break;

                        case SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
                        case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
                        case SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
                        case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
                            buffer.Append("the [OdinSerialize] and [NonSerialized] attribute are defined. ");
                            break;

                        default:
                            buffer.Append("(MISSING CASE: " + relevant.ToString() + ")");
                            break;
                    }

                    // Empty the buffer.
                    if (buffer.Length > 0)
                    {
                        notes.Add(buffer.ToString());
                        buffer.Length = 0;
                    }

                    // Why is the value not serialized by Unity?
                    if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && flags.HasNone(SerializationFlags.SerializedByUnity))
                    {
                        buffer.Append("The member is not being serialized by Unity since ");

                        if (flags.HasAll(SerializationFlags.Property))
                        {
                            buffer.Append("Unity does not serialize properties.");
                        }
                        else if (UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()) == false)
                        {
                            buffer.Append("Unity does not support the type.");
                        }
                        else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
                        {
                            buffer.Append("the [NonSerialized] attribute is defined.");
                        }
                        else if (flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute) == false)
                        {
                            buffer.Append("it is neither a public field nor does it have the [SerializeField] attribute.");
                        }
                        else
                        {
                            buffer.Append("# Missing case, please report: " + flags.ToString());
                        }
                    }

                    // Empty the buffer.
                    if (buffer.Length > 0)
                    {
                        notes.Add(buffer.ToString());
                        buffer.Length = 0;
                    }

                    // Why is the value not serialized by Odin?
                    if (flags.HasAll(SerializationFlags.SerializedByOdin) == false)
                    {
                        buffer.Append("Member is not serialized by Odin because ");

                        if ((serializationBackend & SerializationBackendFlags.Odin) != 0)
                        {
                            if (flags.HasAll(SerializationFlags.SerializedByUnity))
                            {
                                buffer.Append("the member is already serialized by Unity. ");
                            }
                        }
                        else
                        {
                            buffer.Append("Odin serialization is not implemented. ");

                            if (flags.HasAll(SerializationFlags.OdinSerializeAttribute))
                            {
                                buffer.Append("The use of [OdinSerialize] attribute is invalid.");
                            }
                        }
                    }
                }
                else // Why not?
                {
                    // Property members with Odin implementation.
                    if (flags.HasAll(SerializationFlags.Property) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
                    {
                        buffer.Append("is skipped by Odin because ");

                        var prop = member as PropertyInfo;

                        if (prop.GetGetMethod(true) == null)
                        {
                            buffer.Append("the property has no getter. ");
                        }
                        else if (prop.GetSetMethod(true) == null)
                        {
                            buffer.Append("the property has no setter. ");
                        }
                        else if (flags.HasNone(SerializationFlags.OdinSerializeAttribute))
                        {
                            buffer.Append("the [OdinSerialize] attribute has not been applied to it. ");
                        }
                        else
                        {
                            buffer.Append("MISSING CASE (please report). ");
                        }

                        if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
                        {
                            buffer.Append("( Note: the [NonSerialized] attribute is unnecessary. ) ");
                        }
                    }
                    // Property members without Odin implementation.
                    else if (flags.HasAll(SerializationFlags.Property))
                    {
                        buffer.Append("is skipped by Unity because Unity does not serialize properties. ");

                        if (flags.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
                        {
                            buffer.Append("The use of [SerializeField] and [OdinSerialize] attributes is invalid. ");
                        }
                        else if (flags.HasAll(SerializationFlags.OdinSerializeAttribute))
                        {
                            buffer.Append("The use of [OdinSerialize] attribute is invalid. ");
                        }
                        else if (flags.HasAny(SerializationFlags.SerializeFieldAttribute))
                        {
                            buffer.Append("The use of [SerializeField] attribute is invalid. ");
                        }

                        if (flags.HasAny(SerializationFlags.NonSerializedAttribute))
                        {
                            buffer.Append("The use of [NonSerialized] attribute is unnecessary.");
                        }
                    }
                    // Field members.
                    else
                    {
                        // Backend ?
                        buffer.Append("is skipped by ");
                        switch (serializationBackend)
                        {
                            case SerializationBackendFlags.Unity:
                                buffer.Append("Unity ");
                                break;

                            case SerializationBackendFlags.Odin:
                                buffer.Append("Odin ");
                                break;

                            case SerializationBackendFlags.UnityAndOdin:
                                buffer.Append("both Unity and Odin ");
                                break;
                        }

                        buffer.Append("because ");

                        if (serializationBackend == SerializationBackendFlags.None)
                        {
                            buffer.Append("there is no serialization backend? ");
                        }
                        else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
                        {
                            buffer.Append("the [NonSerialized] attribute is defined. ");
                        }
                        else if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin) && flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
                        {
                            buffer.Append("the field is neither public nor is a [SerializeField] or [OdinSerialize] attribute defined. ");
                        }
                        else if (flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
                        {
                            buffer.Append("the field is neither public nor is the [SerializeField] attribute defined. ");
                        }
                        else if (serializationBackend == SerializationBackendFlags.Unity && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
                        {
                            buffer.Append("Unity does not support the type " + member.GetReturnType().GetNiceName());
                        }

                        // Empty the buffer.
                        if (buffer.Length > 0)
                        {
                            notes.Add(buffer.ToString());
                            buffer.Length = 0;
                        }

                        // Invalid use of OdinSerialize.
                        if ((serializationBackend & SerializationBackendFlags.Odin) == 0 && flags.HasAll(SerializationFlags.OdinSerializeAttribute))
                        {
                            notes.Add("Odin serialization is not implemented. The use of [OdinSerialize] attribute is invalid."); // Just add this line directly to the notes list.
                        }
                    }

                    // Using both [SerializeField] and [NonSerialized] attributes.
                    if (flags.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute) && flags.HasNone(SerializationFlags.OdinSerializeAttribute))
                    {
                        notes.Add("Use of [SerializeField] along with [NonSerialized] attributes is weird. Remove either the [SerializeField] or [NonSerialized] attribute.");
                    }
                }
            }
            else // Custom serialization policy.
            {
                if (flags.HasAll(SerializationFlags.AutoProperty))
                {
                    buffer.Append(flags.HasAll(SerializationFlags.Public) ? "The public auto property " : "The auto property ");
                }
                else if (flags.HasAll(SerializationFlags.Property))
                {
                    buffer.Append(flags.HasAll(SerializationFlags.Public) ? "The public property " : "The property ");
                }
                else
                {
                    buffer.Append(flags.HasAll(SerializationFlags.Public) ? "The public field " : "The field ");
                }
                buffer.AppendFormat("'{0}' ", member.GetNiceName());

                // Serialized by Unity.
                if (flags.HasAll(SerializationFlags.SerializedByUnity))
                {
                    buffer.Append("is serialized by Unity since ");

                    if (flags.HasAll(SerializationFlags.Field))
                    {
                        switch (flags & (SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
                        {
                            case SerializationFlags.Public:
                                buffer.Append("its access modifier is public. ");
                                break;

                            case SerializationFlags.SerializeFieldAttribute:
                                buffer.Append("the [SerializeField] attribute is defined. ");
                                break;

                            case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute:
                                buffer.Append("the [SerializeField] attribute is defined, and it's public. ");
                                break;

                            default:
                                buffer.Append("(MISSING CASE: " + flags.ToString() + ")");
                                break;
                        }
                    }
                    else
                    {
                        buffer.Append("is serialized by Unity? Unity should not be serializing any properties? ");
                    }
                }
                else
                {
                    buffer.Append("is skipped by Unity ");

                    if (flags.HasAll(SerializationFlags.Field))
                    {
                        if (flags.HasNone(SerializationFlags.TypeSupportedByUnity))
                        {
                            buffer.Append("because Unity does not support the type " + member.GetReturnType().GetNiceName());
                        }
                        else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
                        {
                            buffer.Append("because the [NonSerialized] attribute is defined. ");
                        }
                        else if (flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
                        {
                            buffer.Append("because the field is not public nor is the [SerializeField] attribute is not defined. ");
                        }
                        else
                        {
                            buffer.Append("(MISSING CASE: " + flags.ToString() + ")");
                        }
                    }
                    else
                    {
                        buffer.Append("because Unity does not serialize properties. ");
                    }
                }

                // Empty the buffer.
                if (buffer.Length > 0)
                {
                    notes.Add(buffer.ToString());
                    buffer.Length = 0;
                }

                // Serialized by Odin with custom serialization policy.
                if (flags.HasAll(SerializationFlags.AutoProperty))
                {
                    buffer.Append("The auto property ");
                }
                else if (flags.HasAll(SerializationFlags.Property))
                {
                    buffer.Append("The property ");
                }
                else
                {
                    buffer.Append("The field ");
                }

                if (flags.HasAll(SerializationFlags.SerializedByOdin))
                {
                    buffer.Append("is serialized by Odin because of custom serialization policy: " + serializationPolicyId);
                }
                else
                {
                    buffer.Append("is skipped by Odin because of custom serialization policy: " + serializationPolicyId);
                }

                // Empty the buffer.
                if (buffer.Length > 0)
                {
                    notes.Add(buffer.ToString());
                    buffer.Length = 0;
                }

                // Serialized by both Unity and Odin.
                if (flags.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.SerializedByUnity))
                {
                    notes.Add("The member is serialized by both Unity and Odin. Consider ensuring that only one serializer is in use.");
                }
            }

            // Empty the buffer.
            if (buffer.Length > 0)
            {
                notes.Add(buffer.ToString());
                buffer.Length = 0;
            }

            // Add notes on Unity serialization support.
            if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin))
            {
                if (flags.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.TypeSupportedByUnity) && flags.HasNone(SerializationFlags.SerializedByUnity | SerializationFlags.Property))
                {
                    buffer.Append("The type '" + member.GetReturnType().GetNiceName() + "' appears to be supported by Unity. Are you certain that you want to use Odin for serializing it?");
                }
                else if (flags.HasAll(SerializationFlags.SerializedByOdin) && flags.HasNone(SerializationFlags.TypeSupportedByUnity))
                {
                    buffer.Append("The type '" + member.GetReturnType().GetNiceName() + "' is not supported by Unity" + GuessWhyUnityDoesNotSupport(member.GetReturnType()));
                }
            }
            else if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && flags.HasNone(SerializationFlags.TypeSupportedByUnity))
            {
                buffer.Append("The type '" + member.GetReturnType().GetNiceName() + "' is not supported by Unity" + GuessWhyUnityDoesNotSupport(member.GetReturnType()));
            }

            // Empty the buffer.
            if (buffer.Length > 0)
            {
                notes.Add(buffer.ToString());
                buffer.Length = 0;
            }

            // Implement Odin support.
            if (serializationBackend.HasAll(SerializationBackendFlags.Unity)
                && serializationBackend.HasNone(SerializationBackendFlags.Odin)
                && flags.HasNone(SerializationFlags.TypeSupportedByUnity)
                && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute)
                && flags.HasAny(SerializationFlags.Field | SerializationFlags.AutoProperty))
            {
                string inheritFrom = "You could implement Odin serializing by inheriting " + member.DeclaringType.GetNiceName() + " from ";

                if (typeof(MonoBehaviour).IsAssignableFrom(member.DeclaringType))
                {
                    buffer.Append(inheritFrom + typeof(SerializedMonoBehaviour).GetNiceName());
                }
                else if (UnityNetworkingUtility.NetworkBehaviourType != null && UnityNetworkingUtility.NetworkBehaviourType.IsAssignableFrom(member.DeclaringType))
                {
                    buffer.Append(inheritFrom + " SerializedNetworkBehaviour");
                }
                else if (typeof(Behaviour).IsAssignableFrom(member.DeclaringType))
                {
                    buffer.Append(inheritFrom + typeof(SerializedBehaviour).GetNiceName());
                }
                else if (typeof(ScriptableObject).IsAssignableFrom(member.DeclaringType))
                {
                    buffer.Append(inheritFrom + typeof(SerializedScriptableObject).GetNiceName());
                }
            }

            // Empty the buffer.
            if (buffer.Length > 0)
            {
                notes.Add(buffer.ToString());
                buffer.Length = 0;
            }

            // Recommend using fields instead of auto properties.
            if (serializationBackend.HasAll(SerializationBackendFlags.Odin)
                && flags.HasAll(SerializationFlags.Property | SerializationFlags.SerializedByOdin))
            {
                buffer.Append("It's recommended to use backing fields for serialization instead of properties.");
            }

            // Empty the buffer.
            if (buffer.Length > 0)
            {
                notes.Add(buffer.ToString());
                buffer.Length = 0;
            }

            return notes.ToArray();
        }

        private static string GuessWhyUnityDoesNotSupport(Type type)
        {
            if (type == typeof(Coroutine))
            {
                return " because Unity will never serialize Coroutines.";
            }
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return " because Unity does not support delegates.";
            }
            if (type.IsInterface)
            {
                return " because the type is an interface.";
            }
            if (type.IsAbstract)
            {
                return " because the type is abstract.";
            }
            if (type == typeof(System.Object))
            {
                return " because Unity does not support serializing System.Object.";
            }
            if (typeof(Enum).IsAssignableFrom(type))
            {
                Type underlying = Enum.GetUnderlyingType(type);

                if (UnityVersion.IsVersionOrGreater(5, 6) && (underlying == typeof(long) || underlying == typeof(ulong)))
                {
                    return " because Unity does not support enums with underlying type of long or ulong.";
                }
                else if (UnityVersion.Major <= 5 && UnityVersion.Minor < 6 && underlying != typeof(int) && underlying != typeof(byte))
                {
                    return " because prior to Version 5.6 Unity only supports enums with underlying type of int or byte.";
                }

                return ". Was unable to determine why Unity does not support enum with underlying type of: " + underlying.GetNiceName() + ".";
            }
            if (typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(type) && type.IsGenericType)
            {
                return " because the type is a generic implementation of UnityEventBase.";
            }

            if (type.IsArray)
            {
                if (type.GetArrayRank() > 1 || type.GetElementType().IsArray || type.GetElementType().ImplementsOpenGenericClass(typeof(List<>)))
                {
                    return " because Unity does not support multi-dimensional arrays.";
                }
                else if (UnitySerializationUtility.GuessIfUnityWillSerialize(type.GetElementType()) == false)
                {
                    return " because Unity does not support the type " + type.GetElementType().GetNiceName() + " as an array element.";
                }
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetArgumentsOfInheritedOpenGenericClass(typeof(List<>))[0];
                if (elementType.IsArray)
                {
                    return " because Unity does not support Lists of arrays.";
                }
                else if (elementType.ImplementsOpenGenericClass(typeof(List<>)))
                {
                    return " because Unity does not support Lists of Lists.";
                }
                else if (UnitySerializationUtility.GuessIfUnityWillSerialize(elementType) == false)
                {
                    return " because Unity does not support the element type of " + elementType.GetNiceName() + ".";
                }
            }

            if (type.IsGenericType || type.GetGenericArguments().Length > 0)
            {
                return " because Unity does not support generic types.";
            }

            if (type.Assembly == typeof(string).Assembly)
            {
                return " because Unity does not serialize [Serializable] structs and classes if they are defined in mscorlib.";
            }

            if (type.IsDefined<SerializableAttribute>(false) == false)
            {
                return " because the type is missing a [Serializable] attribute.";
            }

            // No reason found.
            return ". Was unable to determine reason, please report this to Sirenix.";
        }
    }

    [Flags]
    internal enum SerializationFlags
    {
        Public = 1 << 1,
        Field = 1 << 2,
        Property = 1 << 3,
        AutoProperty = 1 << 4,
        SerializedByUnity = 1 << 5,
        SerializedByOdin = 1 << 6,
        SerializeFieldAttribute = 1 << 7,
        OdinSerializeAttribute = 1 << 8,
        NonSerializedAttribute = 1 << 9,
        TypeSupportedByUnity = 1 << 10,
        DefaultSerializationPolicy = 1 << 11,
    }

    [Flags]
    internal enum SerializationBackendFlags
    {
        None = 0,
        Unity = 1,
        Odin = 2,
        UnityAndOdin = Unity | Odin,
    }
}
#endif