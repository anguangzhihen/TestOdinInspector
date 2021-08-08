#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IndentExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(IndentAttribute))]
    internal class IndentExamples
    {
        [Title("Nicely organize your properties.")]
        [Indent]
        public int A;

        [Indent(2)]
        public int B;

        [Indent(3)]
        public int C;

        [Indent(4)]
        public int D;

        [Title("Using the Indent attribute")]
        [Indent]
        public int E;

        [Indent(0)]
        public int F;

        [Indent(-1)]
        public int G;
    }
}
#endif