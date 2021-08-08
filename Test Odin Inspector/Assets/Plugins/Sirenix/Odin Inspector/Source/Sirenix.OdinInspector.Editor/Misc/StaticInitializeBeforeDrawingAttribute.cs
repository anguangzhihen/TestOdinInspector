#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StaticInitializeBeforeDrawingAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public class StaticInitializeBeforeDrawingAttribute : Attribute
    {
        public StaticInitializeBeforeDrawingAttribute(params Type[] types)
        {
            this.Types = types;
        }

        public Type[] Types;
    }
}
#endif