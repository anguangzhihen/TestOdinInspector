//-----------------------------------------------------------------------
// <copyright file="EnumSerializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Serializer for all enums.
    /// </summary>
    /// <typeparam name="T">The type of the enum to serialize and deserialize.</typeparam>
    /// <seealso cref="Serializer{T}" />
#if CSHARP_7_3_OR_NEWER
    public unsafe sealed class EnumSerializer<T> : Serializer<T> where T : unmanaged, Enum
    {
        private static readonly int SizeOf_T = sizeof(T);
#else
    public sealed class EnumSerializer<T> : Serializer<T>
    {
        static EnumSerializer()
        {
            if (typeof(T).IsEnum == false)
            {
                throw new Exception("Type " + typeof(T).Name + " is not an enum.");
            }
        }
#endif

        /// <summary>
        /// Reads an enum value of type <see cref="T" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override T ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.Integer)
            {
                ulong value;
                if (reader.ReadUInt64(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }

#if CSHARP_7_3_OR_NEWER
                return *(T*)&value;
#else
                return (T)Enum.ToObject(typeof(T), value);
#endif
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(T);
            }
        }

        /// <summary>
        /// Writes an enum value of type <see cref="T" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, T value, IDataWriter writer)
        {
            ulong ul;

            FireOnSerializedType();

#if CSHARP_7_3_OR_NEWER
            byte* toPtr = (byte*)&ul;
            byte* fromPtr = (byte*)&value;

            for (int i = 0; i < SizeOf_T; i++)
            {
                *toPtr++ = *fromPtr++;
            }
#else
            try
            {
                ul = Convert.ToUInt64(value as Enum);
            }
            catch (OverflowException)
            {
                unchecked
                {
                    ul = (ulong)Convert.ToInt64(value as Enum);
                }
            }
#endif

            writer.WriteUInt64(name, ul);
        }
    }
}