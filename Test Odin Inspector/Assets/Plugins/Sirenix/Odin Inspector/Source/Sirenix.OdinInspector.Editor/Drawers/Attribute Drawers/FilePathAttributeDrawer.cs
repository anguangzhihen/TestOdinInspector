#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FilePathAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using FilePathAttribute = Sirenix.OdinInspector.FilePathAttribute; // Needs to be fully typed out

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public sealed class FilePathAttributeDrawer : OdinAttributeDrawer<FilePathAttribute, string>, IDefinesGenericMenuItems
    {
        private ValueResolver<string> parentResolver;
        private ValueResolver<string> extensionsResolver;

        /// <summary>
        /// Initializes the drawer.
        /// </summary>
        protected override void Initialize()
        {
            this.parentResolver = ValueResolver.GetForString(this.Property, this.Attribute.ParentFolder);
            this.extensionsResolver = ValueResolver.GetForString(this.Property, this.Attribute.Extensions);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Display evt. errors in creating property context.
            ValueResolver.DrawErrors(this.parentResolver, this.extensionsResolver);

            // Draw the field.
            //EditorGUI.BeginChangeCheck();
            this.ValueEntry.SmartValue = SirenixEditorFields.FilePathField(
                label,
                this.ValueEntry.SmartValue,
                this.parentResolver.GetValue(),
                this.extensionsResolver.GetValue(),
                this.Attribute.AbsolutePath,
                this.Attribute.UseBackslashes);
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var parentProperty = property.FindParent(p => p.Info.HasSingleBackingMember, true);
            IPropertyValueEntry<string> entry = (IPropertyValueEntry<string>)property.ValueEntry;
            string parent = this.parentResolver.GetValue();

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }

            string path = entry.SmartValue;

            // Create an absolute path from the current value.
            if (path.IsNullOrWhitespace() == false)
            {
                if (Path.IsPathRooted(path) == false)
                {
                    if (parent.IsNullOrWhitespace() == false)
                    {
                        path = Path.Combine(parent, path);
                    }

                    path = Path.GetFullPath(path);
                }
            }
            else if (parent.IsNullOrWhitespace() == false)
            {
                // Use the parent path instead.
                path = Path.GetFullPath(parent);
            }
            else
            {
                // Default to Unity project.
                path = Path.GetDirectoryName(Application.dataPath);
            }

            // Find first existing directory.
            if (path.IsNullOrWhitespace() == false)
            {
                while (path.IsNullOrWhitespace() == false && Directory.Exists(path) == false)
                {
                    path = Path.GetDirectoryName(path);
                }
            }

            // Show in explorer
            if (path.IsNullOrWhitespace() == false)
            {
                genericMenu.AddItem(new GUIContent("Show in explorer"), false, () => System.Diagnostics.Process.Start(path));
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Show in explorer"));
            }
        }
    }
}
#endif