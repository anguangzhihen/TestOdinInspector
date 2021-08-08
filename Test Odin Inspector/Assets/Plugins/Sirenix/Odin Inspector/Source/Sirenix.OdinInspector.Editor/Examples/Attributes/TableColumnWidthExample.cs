#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TableColumnWidthExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [AttributeExample(typeof(TableColumnWidthAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
    internal class TableColumnWidthExample
    {
        [TableList]
        public List<MyItem> List = new List<MyItem>()
        {
            new MyItem(),
            new MyItem(),
            new MyItem(),
        };

        [Serializable]
        public class MyItem
        {
            [PreviewField(Height = 20)]
            [TableColumnWidth(30, Resizable = false)]
            public Texture2D Icon;

            [TableColumnWidth(60)]
            public int ID;

            public string Name;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
            [OnInspectorInit]
            private void CreateData()
            {
                Icon = ExampleHelper.GetTexture();
            }
#endif
        }
    }
}
#endif