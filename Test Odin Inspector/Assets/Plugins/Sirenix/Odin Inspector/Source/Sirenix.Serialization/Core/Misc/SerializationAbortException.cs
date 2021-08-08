//-----------------------------------------------------------------------
// <copyright file="SerializationAbortException.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// An exception thrown when the serialization system has encountered an issue so severe that serialization is being aborted. If this exception is caught in the serialization system somewhere, it should be rethrown.
    /// </summary>
    public class SerializationAbortException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SerializationAbortException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SerializationAbortException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}