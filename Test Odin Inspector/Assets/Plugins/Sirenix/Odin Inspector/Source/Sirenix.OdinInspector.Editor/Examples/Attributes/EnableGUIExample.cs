#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnableGUIExample.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(EnableGUIAttribute))]
    internal class EnableGUIExample
    {
        [ShowInInspector]
        public int GUIDisabledProperty { get { return 10; } }

        [ShowInInspector, EnableGUI]
        public int GUIEnabledProperty { get { return 10; } }
    }
}
#endif