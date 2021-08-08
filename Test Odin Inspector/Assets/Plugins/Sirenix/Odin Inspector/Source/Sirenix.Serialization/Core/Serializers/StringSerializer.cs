//-----------------------------------------------------------------------
// <copyright file="StringSerializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Serializer for the <see cref="string"/> type.
    /// </summary>
    /// <seealso cref="Serializer{System.String}" />
    public sealed class StringSerializer : Serializer<string>
    {
        /// <summary>
        /// Reads a value of type <see cref="string" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override string ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.String)
            {
                string value;
                if (reader.ReadString(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return value;
            }
            else if (entry == EntryType.Null)
            {
                if (reader.ReadNull() == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return null;
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.String.ToString() + " or " + EntryType.Null.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(string);
            }
        }

        /// <summary>
        /// Writes a value of type <see cref="string" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, string value, IDataWriter writer)
        {
            FireOnSerializedType();

            if (value == null)
            {
                writer.WriteNull(name);
            }
            else
            {
                writer.WriteString(name, value);
            }
        }
    }
}