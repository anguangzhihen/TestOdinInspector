#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ExampleAsComponentDataAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples.Internal
{
#pragma warning disable

    using System;

    public class ExampleAsComponentDataAttribute : Attribute
    {
        public string[] AttributeDeclarations;
        public string[] Namespaces;
    }
}
#endif