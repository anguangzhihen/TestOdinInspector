//-----------------------------------------------------------------------
// <copyright file="JsonConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Contains various string constants used by the <see cref="JsonDataWriter"/>, <see cref="JsonDataReader"/> and <see cref="JsonTextReader"/> classes.
    /// </summary>
    public static class JsonConfig
    {
        /// <summary>
        /// The named of a node id entry.
        /// </summary>
        public const string ID_SIG = "$id";

        /// <summary>
        /// The name of a type entry.
        /// </summary>
        public const string TYPE_SIG = "$type";

        /// <summary>
        /// The name of a regular array length entry.
        /// </summary>
        public const string REGULAR_ARRAY_LENGTH_SIG = "$rlength";

        /// <summary>
        /// The name of a primitive array length entry.
        /// </summary>
        public const string PRIMITIVE_ARRAY_LENGTH_SIG = "$plength";

        /// <summary>
        /// The name of a regular array content entry.
        /// </summary>
        public const string REGULAR_ARRAY_CONTENT_SIG = "$rcontent";

        /// <summary>
        /// The name of a primitive array content entry.
        /// </summary>
        public const string PRIMITIVE_ARRAY_CONTENT_SIG = "$pcontent";

        /// <summary>
        /// The beginning of the content of an internal reference entry.
        /// </summary>
        public const string INTERNAL_REF_SIG = "$iref";

        /// <summary>
        /// The beginning of the content of an external reference by index entry.
        /// </summary>
        public const string EXTERNAL_INDEX_REF_SIG = "$eref";

        /// <summary>
        /// The beginning of the content of an external reference by guid entry.
        /// </summary>
        public const string EXTERNAL_GUID_REF_SIG = "$guidref";

        /// <summary>
        /// The beginning of the content of an external reference by string entry.
        /// </summary>
        public const string EXTERNAL_STRING_REF_SIG = "$strref";
    }
}