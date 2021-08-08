//-----------------------------------------------------------------------
// <copyright file="SelfFormatterFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Formatter for types that implement the <see cref="ISelfFormatter"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BaseFormatter{T}" />
    public sealed class SelfFormatterFormatter<T> : BaseFormatter<T> where T : ISelfFormatter
    {
        /// <summary>
        /// Calls <see cref="ISelfFormatter.Deserialize" />  on the value to deserialize.
        /// </summary>
        protected override void DeserializeImplementation(ref T value, IDataReader reader)
        {
            value.Deserialize(reader);
        }

        /// <summary>
        /// Calls <see cref="ISelfFormatter.Serialize" />  on the value to deserialize.
        /// </summary>
        protected override void SerializeImplementation(ref T value, IDataWriter writer)
        {
            value.Serialize(writer);
        }
    }
}