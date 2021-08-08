//-----------------------------------------------------------------------
// <copyright file="SingleSerializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Serializer for the <see cref="float"/> type.
    /// </summary>
    /// <seealso cref="Serializer{System.Single}" />
    public sealed class SingleSerializer : Serializer<float>
    {
        /// <summary>
        /// Reads a value of type <see cref="float" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override float ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.FloatingPoint || entry == EntryType.Integer)
            {
                float value;
                if (reader.ReadSingle(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return value;
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.FloatingPoint.ToString() + " or " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(float);
            }
        }

        /// <summary>
        /// Writes a value of type <see cref="float" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, float value, IDataWriter writer)
        {
            FireOnSerializedType();
            writer.WriteSingle(name, value);
        }
    }
}