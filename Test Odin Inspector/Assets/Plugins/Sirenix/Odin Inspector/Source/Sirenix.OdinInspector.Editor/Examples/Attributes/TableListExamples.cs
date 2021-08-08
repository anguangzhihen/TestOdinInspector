#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TableListExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using System;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(TableListAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
    internal class TableListExamples
    {
        [TableList(ShowIndexLabels = true)]
        public List<SomeCustomClass> TableListWithIndexLabels = new List<SomeCustomClass>()
        {
            new SomeCustomClass(),
            new SomeCustomClass(),
        };

        [TableList(DrawScrollView = true, MaxScrollViewHeight = 200, MinScrollViewHeight = 100)]
        public List<SomeCustomClass> MinMaxScrollViewTable = new List<SomeCustomClass>()
        {
            new SomeCustomClass(),
            new SomeCustomClass(),
        };

        [TableList(AlwaysExpanded = true, DrawScrollView = false)]
        public List<SomeCustomClass> AlwaysExpandedTable = new List<SomeCustomClass>()
        {
            new SomeCustomClass(),
            new SomeCustomClass(),
        };

        [TableList(ShowPaging = true)]
        public List<SomeCustomClass> TableWithPaging = new List<SomeCustomClass>()
        {
            new SomeCustomClass(),
            new SomeCustomClass(),
        };

        [Serializable]
        public class SomeCustomClass
        {
            [TableColumnWidth(57, Resizable = false)]
            [PreviewField(Alignment = ObjectFieldAlignment.Center)]
            public Texture Icon;

            [TextArea]
            public string Description;

            [VerticalGroup("Combined Column"), LabelWidth(22)]
            public string A, B, C;

            [TableColumnWidth(60)]
            [Button, VerticalGroup("Actions")]
            public void Test1() { }
        
            [TableColumnWidth(60)]
            [Button, VerticalGroup("Actions")]
            public void Test2() { }

#if UNITY_EDITOR // Editor-related code must be excluded from builds
            [OnInspectorInit]
            private void CreateData()
            {
                Description = ExampleHelper.GetString();
                Icon = ExampleHelper.GetTexture();
            }
#endif
        }
    }
}
#endif