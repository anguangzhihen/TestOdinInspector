//-----------------------------------------------------------------------
// <copyright file="ISerializableFormatterLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(ISerializableFormatterLocator), -110)]

namespace Sirenix.Serialization
{
#pragma warning disable

    using Utilities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    internal class ISerializableFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, out IFormatter formatter)
        {
            if (step != FormatterLocationStep.AfterRegisteredFormatters || !typeof(ISerializable).IsAssignableFrom(type))
            {
                formatter = null;
                return false;
            }

            formatter = (IFormatter)Activator.CreateInstance(typeof(SerializableFormatter<>).MakeGenericType(type));
            return true;
        }
    }
}