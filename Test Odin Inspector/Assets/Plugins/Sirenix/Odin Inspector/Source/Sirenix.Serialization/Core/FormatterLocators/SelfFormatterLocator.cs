//-----------------------------------------------------------------------
// <copyright file="SelfFormatterLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(SelfFormatterLocator), -60)]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;
    using Utilities;

    internal class SelfFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, out IFormatter formatter)
        {
            formatter = null;

            if (!typeof(ISelfFormatter).IsAssignableFrom(type)) return false;

            if ((step == FormatterLocationStep.BeforeRegisteredFormatters && type.IsDefined<AlwaysFormatsSelfAttribute>())
                || step == FormatterLocationStep.AfterRegisteredFormatters)
            {
                formatter = (IFormatter)Activator.CreateInstance(typeof(SelfFormatterFormatter<>).MakeGenericType(type));
                return true;
            }

            return false;
        }
    }
}