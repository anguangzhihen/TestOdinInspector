#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TabSubGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using UnityEngine;

    public class TabSubGroupAttributeDrawer : OdinGroupDrawer<TabGroupAttribute.TabSubGroupAttribute>
    {
        private GUITabGroup tabGroup;
        private List<Tab> tabs;

        private TabGroupAttribute tabGroupAttribute;
        private GUITabPage forcedTabGroup;

        private class Tab
        {
            public string TabName;
            public List<InspectorProperty> InspectorProperties = new List<InspectorProperty>();
            public ValueResolver<string> Title;
        }

        protected override bool CanDrawGroup(InspectorProperty property)
        {
            return property.Parent != null
                && property.Parent.Info.PropertyType == PropertyType.Group
                && property.Parent.GetAttribute<TabGroupAttribute>() != null;
        }

        protected override void Initialize()
        {
            this.tabGroupAttribute = this.Property.Parent.GetAttribute<TabGroupAttribute>();

            this.tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(this.Property);
            this.tabGroup.DrawNonSelectedTabsAsDisabled = true;
            this.tabs = new List<Tab>();
            var addLastTabs = new List<Tab>();

            for (int j = 0; j < this.Property.Parent.Children.Count; j++)
            {
                var child = this.Property.Parent.Children[j];
                var added = false;

                if (child.Info.PropertyType == PropertyType.Group)
                {
                    var attrType = child.GetAttribute<PropertyGroupAttribute>().GetType();

                    if (attrType.IsNested && attrType.DeclaringType == typeof(TabGroupAttribute))
                    {
                        // This is a tab subgroup; add all its children to a tab for that subgroup
                        var tab = new Tab();
                        tab.TabName = child.NiceName;
                        tab.Title = ValueResolver.GetForString(this.Property, child.Name.TrimStart('#'));
                        for (int i = 0; i < child.Children.Count; i++)
                        {
                            tab.InspectorProperties.Add(child.Children[i]);
                        }

                        this.tabs.Add(tab);
                        added = true;
                    }
                }

                if (!added)
                {
                    // This is a group member of the tab group itself, so it gets its own tab
                    var tab = new Tab();
                    tab.TabName = child.NiceName;
                    tab.Title = ValueResolver.GetForString(this.Property, child.Name.TrimStart('#'));
                    tab.InspectorProperties.Add(child);
                    addLastTabs.Add(tab);
                }
            }

            foreach (var tab in addLastTabs)
            {
                this.tabs.Add(tab);
            }

            for (int i = 0; i < this.tabs.Count; i++)
            {
                this.tabGroup.RegisterTab(this.tabs[i].TabName);
            }

            this.forcedTabGroup = this.tabGroup.RegisterTab(this.Property.Name.TrimStart('#'));
            this.tabGroup.SetCurrentPage(this.forcedTabGroup);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var attribute = this.tabGroupAttribute;

            if (attribute.HideTabGroupIfTabGroupOnlyHasOneTab && this.tabs.Count <= 1)
            {
                for (int i = 0; i < this.tabs.Count; i++)
                {
                    int pageCount = this.tabs[i].InspectorProperties.Count;
                    for (int j = 0; j < pageCount; j++)
                    {
                        var child = this.tabs[i].InspectorProperties[j];
                        child.Update();
                        child.Draw(child.Label);
                    }
                }
                return;
            }

            this.tabGroup.AnimationSpeed = 1 / SirenixEditorGUI.TabPageSlideAnimationDuration;
            this.tabGroup.FixedHeight = attribute.UseFixedHeight;

            if (Event.current.type == EventType.Layout)
            {
                this.tabGroup.SetCurrentPage(this.forcedTabGroup);
            }

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            tabGroup.BeginGroup(true, attribute.Paddingless ? GUIStyle.none : null);

            for (int i = 0; i < this.tabs.Count; i++)
            {
                var page = tabGroup.RegisterTab(this.tabs[i].TabName);
                page.Title = this.tabs[i].Title.GetValue();

                if (page.BeginPage())
                {
                    int pageCount = this.tabs[i].InspectorProperties.Count;
                    for (int j = 0; j < pageCount; j++)
                    {
                        var child = this.tabs[i].InspectorProperties[j];
                        child.Update(); // Since the property is not fetched through the property system, ensure it's updated before drawing it.
                        child.Draw(child.Label);
                    }
                }
                page.EndPage();
            }

            tabGroup.EndGroup();
            SirenixEditorGUI.EndIndentedVertical();
        }
    }
}
#endif