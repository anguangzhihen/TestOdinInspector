#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CustomContextMenuAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector.Editor.ActionResolvers;

    /// <summary>
    /// Adds a generic menu option to properties marked with <see cref="CustomContextMenuAttribute"/>.
    /// </summary>
    /// <seealso cref="CustomContextMenuAttribute"/>
    /// <seealso cref="DisableContextMenuAttribute"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class CustomContextMenuAttributeDrawer : OdinAttributeDrawer<CustomContextMenuAttribute>, IDefinesGenericMenuItems
    {
        private class ContextMenuInfo
        {
            public string Name;
            public ActionResolver Action;
        }

        private ContextMenuInfo info;
        private PropertyContext<Dictionary<CustomContextMenuAttribute, ContextMenuInfo>> contextMenuInfos;
        private PropertyContext<bool> populated;

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (this.populated.Value)
            {
                // Another custom context menu drawer has already populated the menu with all custom menu items
                // this is done so we can have menu item ordering consistency.
                return;
            }
            else
            {
                this.populated.Value = true;
            }
            
            if (this.contextMenuInfos.Value != null && this.contextMenuInfos.Value.Count > 0)
            {
                if (genericMenu.GetItemCount() > 0)
                {
                    genericMenu.AddSeparator("");
                }

                foreach (var item in this.contextMenuInfos.Value.OrderBy(n => n.Key.MenuItem ?? ""))
                {
                    var info = item.Value;

                    if (info.Action == null)
                    {
                        genericMenu.AddDisabledItem(new GUIContent(item.Key.MenuItem + " (Invalid)"));
                    }
                    else
                    {
                        genericMenu.AddItem(new GUIContent(info.Name), false, () => info.Action.DoActionForAllSelectionIndices());
                    }
                }
            }
        }

        protected override void Initialize()
        {
            var property = this.Property;
            var attribute = this.Attribute;

            this.contextMenuInfos = property.Context.GetGlobal("CustomContextMenu", (Dictionary<CustomContextMenuAttribute, ContextMenuInfo>)null);
            this.populated = property.Context.GetGlobal("CustomContextMenu_Populated", false);

            if (contextMenuInfos.Value == null)
            {
                contextMenuInfos.Value = new Dictionary<CustomContextMenuAttribute, ContextMenuInfo>();
            }
            
            if (!contextMenuInfos.Value.TryGetValue(attribute, out this.info))
            {
                this.info = new ContextMenuInfo();
                this.info.Name = attribute.MenuItem;
                this.info.Action = ActionResolver.Get(this.Property, attribute.Action);

                contextMenuInfos.Value[attribute] = this.info;
            }

        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.populated.Value = false;

            if (this.info.Action.HasError)
            {
                this.info.Action.DrawError();
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif