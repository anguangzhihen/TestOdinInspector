//-----------------------------------------------------------------------
// <copyright file="IAskIfCanFormatTypes.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    public interface IAskIfCanFormatTypes
    {
        bool CanFormatType(Type type);
    }
}