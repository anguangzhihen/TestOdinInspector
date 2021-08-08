//-----------------------------------------------------------------------
// <copyright file="BooleanSerializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Serializer for the <see cref="bool"/> type.
    /// </summary>
    /// <seealso cref="Serializer{System.Boolean}" />
    public sealed class BooleanSerializer : Serializer<bool>
    {
        /// <summary>
        /// Reads a value of type <see cref="bool" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override bool ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.Boolean)
            {
                bool value;
                if (reader.ReadBoolean(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return value;
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Boolean.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(bool);
            }
        }

        /// <summary>
        /// Writes a value of type <see cref="bool" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, bool value, IDataWriter writer)
        {
            FireOnSerializedType();
            writer.WriteBoolean(name, value);
        }
    }
}