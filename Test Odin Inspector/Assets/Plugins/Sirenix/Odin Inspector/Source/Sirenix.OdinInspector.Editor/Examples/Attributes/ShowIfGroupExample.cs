#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowIfGroupExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(ShowIfGroupAttribute))]
    internal class ShowIfGroupExample
    {
        public bool Toggle = true;

        [ShowIfGroup("Toggle")]
        [BoxGroup("Toggle/Shown Box")]
        public int A, B;
        
        [BoxGroup("Box")]
        public InfoMessageType EnumField = InfoMessageType.Info;

        [BoxGroup("Box")]
        [ShowIfGroup("Box/Toggle")]
        public Vector3 X, Y;

        // Like the regular If-attributes, ShowIfGroup also supports specifying values.
        // You can also chain multiple ShowIfGroup attributes together for more complex behaviour.
        [ShowIfGroup("Box/Toggle/EnumField", Value = InfoMessageType.Info)]
        [BoxGroup("Box/Toggle/EnumField/Border", ShowLabel = false)]
        public string Name;

        [BoxGroup("Box/Toggle/EnumField/Border")]
        public Vector3 Vector;
        
        // ShowIfGroup will by default use the name of the group,
        // but you can also use the MemberName property to override this.
        [ShowIfGroup("RectGroup", Condition = "Toggle")]
        public Rect Rect;
    }
}
#endif