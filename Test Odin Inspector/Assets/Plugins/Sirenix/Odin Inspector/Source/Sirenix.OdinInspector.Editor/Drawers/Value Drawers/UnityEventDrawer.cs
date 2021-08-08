#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityEventDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
    using System;
    using System.Reflection;
    using Sirenix.Utilities;

    /// <summary>
    /// Unity event drawer.
    /// </summary>
    [DrawerPriority(0, 0, 0.45)]
    // Just over default priority for Unity-declared legacy property drawers (0.4)
    //  but under default priority for user-declared legacy property drawers (0.5)
    // This lets users override Unity event drawing with legacy property drawers, 
    //  but also still has Odin override Unity's default UnityEventDrawer.
    public sealed class UnityEventDrawer<T> : UnityPropertyDrawer<UnityEditorInternal.UnityEventDrawer, T> where T : UnityEventBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            base.delayApplyValueUntilRepaint = true;
            this.drawer = new UnityEditorInternal.UnityEventDrawer();

            if (UnityPropertyHandlerUtility.IsAvailable)
            {
                this.propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(this.drawer);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            FieldInfo fieldInfo;
            SerializedProperty unityProperty = this.Property.Tree.GetUnityPropertyForPath(this.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                if (UnityVersion.IsVersionOrGreater(2017, 1))
                {
                    this.CallNextDrawer(label);
                    return;
                }
                else if (!typeof(T).IsDefined<SerializableAttribute>())
                {
                    SirenixEditorGUI.ErrorMessageBox("You have likely forgotten to mark your custom UnityEvent class '" + typeof(T).GetNiceName() + "' with the [Serializable] attribute! Could not get a Unity SerializedProperty for the property '" + this.Property.NiceName + "' of type '" + this.ValueEntry.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");
                    return;
                }
            }

            base.DrawPropertyLayout(label);
        }
    }
}
#endif