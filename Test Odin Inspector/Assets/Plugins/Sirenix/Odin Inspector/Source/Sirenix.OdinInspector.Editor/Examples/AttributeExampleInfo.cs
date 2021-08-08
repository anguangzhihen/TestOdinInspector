#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AttributeExampleInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using System;

    /// <summary>
    /// Descripes an attribute example.
    /// </summary>
    public class AttributeExampleInfo
    {
        private object previewObject;

        /// <summary>
        /// The type of the example object.
        /// </summary>
        public Type ExampleType;

        /// <summary>
        /// The name of the example.
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of the example.
        /// </summary>
        public string Description;

        /// <summary>
        /// Raw code of the example.
        /// </summary>
        public string Code;

        /// <summary>
        /// The example declared as a Unity component.
        /// </summary>
        public string CodeAsComponent;

        /// <summary>
        /// Sorting value of the example. Examples with lower order values should come before examples with higher order values.
        /// </summary>
        public float Order;

        /// <summary>
        /// Preview object of the example.
        /// </summary>
        public object PreviewObject
        {
            get
            {
                if (previewObject == null)
                {
                    previewObject = Activator.CreateInstance(this.ExampleType);
                }

                return previewObject;
            }
        }
    }
}
#endif