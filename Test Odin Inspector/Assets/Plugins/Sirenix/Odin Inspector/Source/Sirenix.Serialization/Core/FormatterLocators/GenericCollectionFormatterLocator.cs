//-----------------------------------------------------------------------
// <copyright file="GenericCollectionFormatterLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(GenericCollectionFormatterLocator), -100)]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    internal class GenericCollectionFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, out IFormatter formatter)
        {
            Type elementType;
            if (step != FormatterLocationStep.AfterRegisteredFormatters || !GenericCollectionFormatter.CanFormat(type, out elementType))
            {
                formatter = null;
                return false;
            }

            formatter = (IFormatter)Activator.CreateInstance(typeof(GenericCollectionFormatter<,>).MakeGenericType(type, elementType));
            return true;
        }
    }
}