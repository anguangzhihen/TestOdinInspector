//-----------------------------------------------------------------------
// <copyright file="SirenixBuildVersionAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class SirenixBuildVersionAttribute : Attribute
    {
        public string Version { get; private set; }

        public SirenixBuildVersionAttribute(string version)
        {
            this.Version = version;
        }
    }
}