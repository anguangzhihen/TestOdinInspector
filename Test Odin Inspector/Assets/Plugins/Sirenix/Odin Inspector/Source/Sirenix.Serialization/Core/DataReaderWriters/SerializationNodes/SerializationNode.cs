//-----------------------------------------------------------------------
// <copyright file="SerializationNode.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// A serialization node as used by the <see cref="DataFormat.Nodes"/> format.
    /// </summary>
    [Serializable]
    public struct SerializationNode
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// The entry type of the node.
        /// </summary>
        public EntryType Entry;

        /// <summary>
        /// The data contained in the node. Depending on the entry type and name, as well as nodes encountered prior to this one, the format can vary wildly.
        /// </summary>
        public string Data;
    }
}