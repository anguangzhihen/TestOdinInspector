//-----------------------------------------------------------------------
// <copyright file="SirenixBuildNameAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class SirenixBuildNameAttribute : Attribute
    {
        public string BuildName { get; private set; }

        public SirenixBuildNameAttribute(string buildName)
        {
            this.BuildName = buildName;
        }
    }
}