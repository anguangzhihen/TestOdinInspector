//-----------------------------------------------------------------------
// <copyright file="GuidSerializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Globalization;

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Serializer for the <see cref="Guid"/> type.
    /// </summary>
    /// <seealso cref="Serializer{System.Guid}" />
    public sealed class GuidSerializer : Serializer<Guid>
    {
        /// <summary>
        /// Reads a value of type <see cref="Guid" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override Guid ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.Guid)
            {
                Guid value;
                if (reader.ReadGuid(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return value;
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Guid.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(Guid);
            }
        }

        /// <summary>
        /// Writes a value of type <see cref="Guid" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, Guid value, IDataWriter writer)
        {
            FireOnSerializedType();
            writer.WriteGuid(name, value);
        }
    }
}