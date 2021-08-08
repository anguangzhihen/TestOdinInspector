//-----------------------------------------------------------------------
// <copyright file="DateTimeOffsetFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(DateTimeOffsetFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System.Globalization;
    using System;

    /// <summary>
    /// Custom formatter for the <see cref="DateTimeOffset"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{System.DateTimeOffset}" />
    public sealed class DateTimeOffsetFormatter : MinimalBaseFormatter<DateTimeOffset>
    {
        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref DateTimeOffset value, IDataReader reader)
        {
            string name;

            if (reader.PeekEntry(out name) == EntryType.String)
            {
                string str;
                reader.ReadString(out str);
                DateTimeOffset.TryParse(str, out value);
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref DateTimeOffset value, IDataWriter writer)
        {
            writer.WriteString(null, value.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}