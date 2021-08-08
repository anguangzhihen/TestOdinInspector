#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideSerializableJsonDictionaryFromEditorWindowsInUnity2017Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;

    [DrawerPriority(9001, 0, 0)]
    internal class HideSerializableJsonDictionaryFromEditorWindowsInUnity2017Drawer<T> : OdinValueDrawer<T> where T : ScriptableObject
    {
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.FullName == "UnityEditor.Experimental.UIElements.SerializableJsonDictionary";
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var member = entry.Property.Info.GetMemberInfo();
            if (member.MemberType == System.Reflection.MemberTypes.Field && member.Name == "m_PersistentViewDataDictionary")
            {
                return;
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif