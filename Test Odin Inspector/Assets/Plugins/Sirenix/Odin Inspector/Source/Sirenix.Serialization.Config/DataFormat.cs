//-----------------------------------------------------------------------
// <copyright file="DataFormat.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Specifies a data format to read and write in.
    /// </summary>
    public enum DataFormat
    {
        /// <summary>
        /// A custom packed binary format. This format is most efficient and almost allocation-free,
        /// but its serialized data is not human-readable.
        /// </summary>
        Binary = 0,

        /// <summary>
        /// A JSON format compliant with the json specification found at "http://www.json.org/".
        /// <para />
        /// This format has rather sluggish performance and allocates frightening amounts of string garbage.
        /// </summary>
        JSON = 1,

        /// <summary>
        /// A format that does not serialize to a byte stream, but to a list of data nodes in memory
        /// which can then be serialized by Unity.
        /// <para />
        /// This format is highly inefficient, and is primarily used for ensuring that Unity assets
        /// are mergeable by individual values when saved in Unity's text format. This makes
        /// serialized values more robust and data recovery easier in case of issues.
        /// <para />
        /// This format is *not* recommended for use in builds.
        /// </summary>
        Nodes = 2
    }
}