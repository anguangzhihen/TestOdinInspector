#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ToggleExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using System;

    [AttributeExample(typeof(ToggleAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System" })]
    internal class ToggleExample
    {
        [Toggle("Enabled")]
        public MyToggleable Toggler = new MyToggleable();

        public ToggleableClass Toggleable = new ToggleableClass();

        [Serializable]
        public class MyToggleable
        {
            public bool Enabled;
            public int MyValue;
        }

        // You can also use the Toggle attribute directly on a class definition.
        [Serializable, Toggle("Enabled")]
        public class ToggleableClass
        {
            public bool Enabled;
            public string Text;
        }
    }
}
#endif