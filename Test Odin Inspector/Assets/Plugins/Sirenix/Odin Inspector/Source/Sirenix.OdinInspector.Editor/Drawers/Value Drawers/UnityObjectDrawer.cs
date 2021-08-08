#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityObjectDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities.Editor;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Unity object drawer.
    /// </summary>
    [DrawerPriority(0, 0, 0.25)] // Set priority so that vanilla Unity CustomPropertyDrawers can draw UnityObject types by default
    public sealed class UnityObjectDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
        where T : UnityEngine.Object
    {
        private bool drawAsPreview;

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return !property.IsTreeRoot;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.drawAsPreview = false;
            var flags = GeneralDrawerConfig.Instance.SquareUnityObjectEnableFor;
            this.drawAsPreview = (int)flags != 0 && (
                (flags & GeneralDrawerConfig.UnityObjectType.Components) != 0 && typeof(Component).IsAssignableFrom(typeof(T)) ||
                (flags & GeneralDrawerConfig.UnityObjectType.GameObjects) != 0 && typeof(GameObject).IsAssignableFrom(typeof(T)) ||
                (flags & GeneralDrawerConfig.UnityObjectType.Materials) != 0 && typeof(Material).IsAssignableFrom(typeof(T)) ||
                (flags & GeneralDrawerConfig.UnityObjectType.Sprites) != 0 && typeof(Sprite).IsAssignableFrom(typeof(T)) ||
                (flags & GeneralDrawerConfig.UnityObjectType.Textures) != 0 && typeof(Texture).IsAssignableFrom(typeof(T)));

            if (!this.drawAsPreview && (flags & GeneralDrawerConfig.UnityObjectType.Others) != 0)
            {
                bool isOther =
                    !typeof(Component).IsAssignableFrom(typeof(T)) &&
                    !typeof(GameObject).IsAssignableFrom(typeof(T)) &&
                    !typeof(Material).IsAssignableFrom(typeof(T)) &&
                    !typeof(Sprite).IsAssignableFrom(typeof(T)) &&
                    !typeof(Texture).IsAssignableFrom(typeof(T));

                if (isOther)
                {
                    this.drawAsPreview = true;
                }
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            if (!drawAsPreview)
            {
                if (entry.BaseValueType.IsInterface)
                {
                    entry.WeakSmartValue = SirenixEditorFields.PolymorphicObjectField(
                        label,
                        entry.WeakSmartValue,
                        entry.BaseValueType,
                        entry.Property.GetAttribute<AssetsOnlyAttribute>() == null);
                }
                else
                {
                    entry.WeakSmartValue = SirenixEditorFields.UnityObjectField(
                        label,
                        entry.WeakSmartValue as UnityEngine.Object,
                        entry.BaseValueType,
                        entry.Property.GetAttribute<AssetsOnlyAttribute>() == null);
                }
            }
            else
            {
                entry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(
                    label,
                    entry.WeakSmartValue as UnityEngine.Object,
                    entry.BaseValueType,
                    entry.Property.GetAttribute<AssetsOnlyAttribute>() == null,
                    GeneralDrawerConfig.Instance.SquareUnityObjectFieldHeight,
                    GeneralDrawerConfig.Instance.SquareUnityObjectAlignment);
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var unityObj = property.ValueEntry.WeakSmartValue as UnityEngine.Object;
            if (unityObj)
            {
                genericMenu.AddItem(new GUIContent("Open in new inspector"), false, () =>
                {
                    GUIHelper.OpenInspectorWindow(unityObj);
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Open in new inspector"));
            }
        }
    }
}
#endif