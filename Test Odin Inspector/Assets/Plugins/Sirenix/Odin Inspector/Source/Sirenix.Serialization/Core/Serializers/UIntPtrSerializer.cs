//-----------------------------------------------------------------------
// <copyright file="UIntPtrSerializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Serializer for the <see cref="UIntPtr"/> type.
    /// </summary>
    /// <seealso cref="Serializer{System.UIntPtr}" />
    public sealed class UIntPtrSerializer : Serializer<UIntPtr>
    {
        /// <summary>
        /// Reads a value of type <see cref="UIntPtr" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override UIntPtr ReadValue(IDataReader reader)
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
                return new UIntPtr(value);
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(UIntPtr);
            }
        }

        /// <summary>
        /// Writes a value of type <see cref="UIntPtr" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, UIntPtr value, IDataWriter writer)
        {
            FireOnSerializedType();
            writer.WriteUInt64(name, (ulong)value);
        }
    }
}