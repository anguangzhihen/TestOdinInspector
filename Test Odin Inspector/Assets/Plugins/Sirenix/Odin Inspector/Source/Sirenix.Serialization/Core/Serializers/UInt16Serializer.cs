//-----------------------------------------------------------------------
// <copyright file="UInt16Serializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Serializer for the <see cref="ushort"/> type.
    /// </summary>
    /// <seealso cref="Serializer{System.UInt16}" />
    public sealed class UInt16Serializer : Serializer<ushort>
    {
        /// <summary>
        /// Reads a value of type <see cref="ushort" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override ushort ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.Integer)
            {
                ushort value;
                if (reader.ReadUInt16(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return value;
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(ushort);
            }
        }

        /// <summary>
        /// Writes a value of type <see cref="ulong" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, ushort value, IDataWriter writer)
        {
            FireOnSerializedType();
            writer.WriteUInt16(name, value);
        }
    }
}