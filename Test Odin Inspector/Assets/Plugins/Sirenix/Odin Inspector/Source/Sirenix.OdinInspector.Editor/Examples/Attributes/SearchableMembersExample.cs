#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SearchableMembersExample.cs" company="Sirenix IVS">
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

    [AttributeExample(typeof(SearchableAttribute), "The Searchable attribute can be applied to individual members in a type, to make only that member searchable.")]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "System.Linq", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
    internal class SearchableMembersExample
    {
        [Searchable]
        public ExampleClass searchableClass = new ExampleClass();

        [Searchable]
        public List<ExampleStruct> searchableList = new List<ExampleStruct>(Enumerable.Range(1, 10).Select(i => new ExampleStruct(i)));

        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        public List<FilterableBySquareStruct> customFiltering = new List<FilterableBySquareStruct>(Enumerable.Range(1, 10).Select(i => new FilterableBySquareStruct(i)));

        [Serializable]
        public class ExampleClass
        {
            public string SomeString = "Saehrimnir is a tasty delicacy";
            public int SomeInt = 13579;

            public DataContainer DataContainerOne = new DataContainer() { Name = "Example Data Set One" };
            public DataContainer DataContainerTwo = new DataContainer() { Name = "Example Data Set Two" };
        }

        [Serializable, Searchable] // You can also apply it on a type like this, and it will become searchable wherever it appears
        public class DataContainer
        {
            public string Name;
            public List<ExampleStruct> Data = new List<ExampleStruct>(Enumerable.Range(1, 10).Select(i => new ExampleStruct(i)));
        }

        [Serializable]
        public struct FilterableBySquareStruct : ISearchFilterable
        {
            public int Number;

            [ShowInInspector, DisplayAsString, EnableGUI]
            public int Square { get { return this.Number * this.Number; } }

            public FilterableBySquareStruct(int nr)
            {
                this.Number = nr;
            }

            public bool IsMatch(string searchString)
            {
                return searchString.Contains(Square.ToString());
            }
        }

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