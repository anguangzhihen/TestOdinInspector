#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeFilterExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using System.Linq;
    using System;
    using System.Collections.Generic;
    using Sirenix.Utilities;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(TypeFilterAttribute), "The TypeFilter will instantiate the given type directly, It will also draw all child members in a foldout below the dropdown.")]
    [ExampleAsComponentData(Namespaces = new string[] { 
        "System",
        "System.Linq",
        "System.Collections.Generic",
        "Sirenix.Utilities",
    })]
    internal class TypeFilterExamples
    {
        [TypeFilter("GetFilteredTypeList")]
        public BaseClass A, B;

        [TypeFilter("GetFilteredTypeList")]
        public BaseClass[] Array = new BaseClass[3];

        public IEnumerable<Type> GetFilteredTypeList()
        {
            var q = typeof(BaseClass).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)                                          // Excludes BaseClass
                .Where(x => !x.IsGenericTypeDefinition)                             // Excludes C1<>
                .Where(x => typeof(BaseClass).IsAssignableFrom(x));                 // Excludes classes not inheriting from BaseClass

            // Adds various C1<T> type variants.
            q = q.AppendWith(typeof(C1<>).MakeGenericType(typeof(GameObject)));
            q = q.AppendWith(typeof(C1<>).MakeGenericType(typeof(AnimationCurve)));
            q = q.AppendWith(typeof(C1<>).MakeGenericType(typeof(List<float>)));

            return q;
        }

        public abstract class BaseClass
        {
            public int BaseField;
        }

        public class A1 : BaseClass { public int _A1; }
        public class A2 : A1 { public int _A2; }
        public class A3 : A2 { public int _A3; }
        public class B1 : BaseClass { public int _B1; }
        public class B2 : B1 { public int _B2; }
        public class B3 : B2 { public int _B3; }
        public class C1<T> : BaseClass { public T C; }
    }
}
#endif