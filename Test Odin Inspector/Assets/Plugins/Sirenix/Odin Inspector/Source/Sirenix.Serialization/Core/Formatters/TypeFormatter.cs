//-----------------------------------------------------------------------
// <copyright file="TypeFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    // Registered by TypeFormatterLocator

    /// <summary>
    /// Formatter for the <see cref="Type"/> type which uses the reader/writer's <see cref="TwoWaySerializationBinder"/> to bind types.
    /// </summary>
    /// <seealso cref="Serialization.MinimalBaseFormatter{T}" />
    public sealed class TypeFormatter : MinimalBaseFormatter<Type>
    {
        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Type value, IDataReader reader)
        {
            string name;

            if (reader.PeekEntry(out name) == EntryType.String)
            {
                reader.ReadString(out name);
                value = reader.Context.Binder.BindToType(name, reader.Context.Config.DebugContext);

                if (value != null)
                {
                    this.RegisterReferenceID(value, reader);
                }
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Type value, IDataWriter writer)
        {
            writer.WriteString(null, writer.Context.Binder.BindToName(value, writer.Context.Config.DebugContext));
        }

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <returns>null.</returns>
        protected override Type GetUninitializedObject()
        {
            return null;
        }
    }
}