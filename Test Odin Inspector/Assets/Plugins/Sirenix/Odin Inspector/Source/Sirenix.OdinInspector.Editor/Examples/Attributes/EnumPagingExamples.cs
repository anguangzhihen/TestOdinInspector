#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnumPagingExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(EnumPagingAttribute))]
    internal class EnumPagingExamples
    {
        [EnumPaging]
        public SomeEnum SomeEnumField;
        
        public enum SomeEnum
        {
            A, B, C
        }
    }
}
#endif