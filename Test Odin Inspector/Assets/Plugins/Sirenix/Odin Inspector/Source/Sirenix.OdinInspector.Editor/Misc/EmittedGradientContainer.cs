#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EmittedGradientContainer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Reflection;
    using UnityEngine;

    public class EmittedGradientContainer : EmittedScriptableObject<Gradient>
    {
        public Gradient value;

        public override FieldInfo BackingFieldInfo
        {
            get
            {
                return typeof(EmittedGradientContainer).GetField("value");
            }
        }

        public override Gradient GetValue()
        {
            return this.value;
        }

        public override void SetValue(Gradient value)
        {
            this.value = value;
        }
    }
}
#endif