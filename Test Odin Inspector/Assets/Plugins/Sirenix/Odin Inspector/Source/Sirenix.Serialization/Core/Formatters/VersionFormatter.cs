//-----------------------------------------------------------------------
// <copyright file="VersionFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(VersionFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Custom formatter for the <see cref="Version"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{System.Version}" />
    public sealed class VersionFormatter : MinimalBaseFormatter<Version>
    {
        protected override Version GetUninitializedObject()
        {
            return null;
        }

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Version value, IDataReader reader)
        {
            int major = 0,
                minor = 0,
                build = 0,
                revision = 0;

            reader.ReadInt32(out major);
            reader.ReadInt32(out minor);
            reader.ReadInt32(out build);
            reader.ReadInt32(out revision);

            if (major < 0 || minor < 0)
            {
                value = new Version();
            }
            else if (build < 0)
            {
                value = new Version(major, minor);
            }
            else if (revision < 0)
            {
                value = new Version(major, minor, build);
            }
            else
            {
                value = new Version(major, minor, build, revision);
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Version value, IDataWriter writer)
        {
            writer.WriteInt32(null, value.Major);
            writer.WriteInt32(null, value.Minor);
            writer.WriteInt32(null, value.Build);
            writer.WriteInt32(null, value.Revision);
        }
    }
}