//-----------------------------------------------------------------------
// <copyright file="EasyBaseFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Provides an easy way of implementing custom formatters.
    /// </summary>
    /// <typeparam name="T">The type which can be serialized and deserialized by the formatter.</typeparam>
    public abstract class EasyBaseFormatter<T> : BaseFormatter<T>
    {
        /// <summary>
        /// Reads through all entries in the current node one at a time, and calls <see cref="EasyBaseFormatter{T}.ReadDataEntry(ref T, string, EntryType, IDataReader, DeserializationContext)" /> for each entry.
        /// </summary>
        /// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="BaseFormatter{T}.GetUninitializedObject" />.</param>
        /// <param name="reader">The reader to deserialize with.</param>
        protected sealed override void DeserializeImplementation(ref T value, IDataReader reader)
        {
            int count = 0;
            string name;
            EntryType entry;

            while ((entry = reader.PeekEntry(out name)) != EntryType.EndOfNode && entry != EntryType.EndOfArray && entry != EntryType.EndOfStream)
            {
                this.ReadDataEntry(ref value, name, entry, reader);

                count++;

                if (count > 1000)
                {
                    reader.Context.Config.DebugContext.LogError("Breaking out of infinite reading loop!");
                    break;
                }
            }
        }

        /// <summary>
        /// Calls <see cref="EasyBaseFormatter{T}.WriteDataEntries(ref T, IDataWriter)" /> directly.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to serialize with.</param>
        protected sealed override void SerializeImplementation(ref T value, IDataWriter writer)
        {
            this.WriteDataEntries(ref value, writer);
        }

        /// <summary>
        /// Reads a data entry into the value denoted by the entry name.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="entryName">The name of the entry.</param>
        /// <param name="entryType">The type of the entry.</param>
        /// <param name="reader">The reader currently used for deserialization.</param>
        protected abstract void ReadDataEntry(ref T value, string entryName, EntryType entryType, IDataReader reader);

        /// <summary>
        /// Write the serialized values of a value of type <see cref="t" />.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer currently used for serialization.</param>
        protected abstract void WriteDataEntries(ref T value, IDataWriter writer);
    }
}