#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowPropertiesInTheInspectorExamples.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(ShowInInspectorAttribute), Name = "Inspect Properties")]
    internal class ShowPropertiesInTheInspectorExamples
    {
        [SerializeField, HideInInspector]
        private int evenNumber;

        [ShowInInspector]
        public int EvenNumber
        {
            get { return this.evenNumber; }
            set { this.evenNumber = value - (value % 2); }
        }
    }
}
#endif