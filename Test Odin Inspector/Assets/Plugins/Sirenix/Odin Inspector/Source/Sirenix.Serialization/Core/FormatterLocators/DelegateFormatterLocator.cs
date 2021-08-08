//-----------------------------------------------------------------------
// <copyright file="DelegateFormatterLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(DelegateFormatterLocator), -50)]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    internal class DelegateFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, out IFormatter formatter)
        {
            if (!typeof(Delegate).IsAssignableFrom(type))
            {
                formatter = null;
                return false;
            }

            formatter = (IFormatter)Activator.CreateInstance(typeof(DelegateFormatter<>).MakeGenericType(type));
            return true;
        }
    }
}