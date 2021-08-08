#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SearchableInspectorExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [AttributeExample(typeof(SearchableAttribute), "The Searchable attribute can be applied to a root inspected type, like a Component, ScriptableObject or OdinEditorWindow, to make the whole type searchable.")]
    [Searchable]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "System.Linq", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" }, AttributeDeclarations = new string[] { "Searchable" })]
    internal class SearchableInspectorExample
    {
        public List<string> strings = new List<string>(Enumerable.Range(1, 10).Select(i => "Str Element " + i));

        public List<ExampleStruct> searchableList = new List<ExampleStruct>(Enumerable.Range(1, 10).Select(i => new ExampleStruct(i)));

        [Serializable]
        public struct ExampleStruct
        {
            public string Name;
            public int Number;
            public ExampleEnum Enum;

            public ExampleStruct(int nr) : this()
            {
                this.Name = "Element " + nr;
                this.Number = nr;
#if UNITY_EDITOR // ExampleHelper is an editor-only class so we cannot use it in a build
                this.Enum = (ExampleEnum)ExampleHelper.RandomInt(0, 5);
#endif
            }
        }

        public enum ExampleEnum
        {
            One, Two, Three, Four, Five
        }
    }
}
#endif