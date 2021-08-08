//-----------------------------------------------------------------------
// <copyright file="Flags.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization.Utilities
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// This class encapsulates common <see cref="BindingFlags"/> combinations.
    /// </summary>
    internal static class Flags
    {
        /// <summary>
        /// Search criteria encompassing all public and non-public members, including base members.
        /// Note that you also need to specify either the Instance or Static flag.
        /// </summary>
        public const BindingFlags AnyVisibility = BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Search criteria encompassing all public instance members, including base members.
        /// </summary>
        public const BindingFlags InstancePublic = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Search criteria encompassing all non-public instance members, including base members.
        /// </summary>
        public const BindingFlags InstancePrivate = BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Search criteria encompassing all public and non-public instance members, including base members.
        /// </summary>
        public const BindingFlags InstanceAnyVisibility = AnyVisibility | BindingFlags.Instance;

        /// <summary>
        /// Search criteria encompassing all public static members, including base members.
        /// </summary>
        public const BindingFlags StaticPublic = BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        /// Search criteria encompassing all non-public static members, including base members.
        /// </summary>
        public const BindingFlags StaticPrivate = BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Search criteria encompassing all public and non-public static members, including base members.
        /// </summary>
        public const BindingFlags StaticAnyVisibility = AnyVisibility | BindingFlags.Static;

        /// <summary>
        /// Search criteria encompassing all public instance members, excluding base members.
        /// </summary>
        public const BindingFlags InstancePublicDeclaredOnly = InstancePublic | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Search criteria encompassing all non-public instance members, excluding base members.
        /// </summary>
        public const BindingFlags InstancePrivateDeclaredOnly = InstancePrivate | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Search criteria encompassing all public and non-public instance members, excluding base members.
        /// </summary>
        public const BindingFlags InstanceAnyDeclaredOnly = InstanceAnyVisibility | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Search criteria encompassing all public static members, excluding base members.
        /// </summary>
        public const BindingFlags StaticPublicDeclaredOnly = StaticPublic | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Search criteria encompassing all non-public static members, excluding base members.
        /// </summary>
        public const BindingFlags StaticPrivateDeclaredOnly = StaticPrivate | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Search criteria encompassing all public and non-public static members, excluding base members.
        /// </summary>
        public const BindingFlags StaticAnyDeclaredOnly = StaticAnyVisibility | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Search criteria encompassing all members, including base and static members.
        /// </summary>
        public const BindingFlags StaticInstanceAnyVisibility = InstanceAnyVisibility | BindingFlags.Static;

        /// <summary>
        /// Search criteria encompassing all members (public and non-public, instance and static), including base members.
        /// </summary>
        public const BindingFlags AllMembers = StaticInstanceAnyVisibility | BindingFlags.FlattenHierarchy;
    }
}