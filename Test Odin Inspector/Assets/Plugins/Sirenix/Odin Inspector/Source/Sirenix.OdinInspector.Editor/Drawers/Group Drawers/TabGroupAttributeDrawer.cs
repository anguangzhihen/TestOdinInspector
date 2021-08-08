#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TabGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System.Collections.Generic;
    using Utilities.Editor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws all properties grouped together with the <see cref="TabGroupAttribute"/>
    /// </summary>
    /// <seealso cref="TabGroupAttribute"/>
    public class TabGroupAttributeDrawer : OdinGroupDrawer<TabGroupAttribute>, IOnSelfStateChangedNotification
    {
        public const string CurrentTabIndexKey = "CurrentTabIndex";
        public const string CurrentTabNameKey = "CurrentTabName";
        public const string TabCountKey = "TabCount";

        private bool isChangingTabName;
        private GUITabGroup tabGroup;
        //private LocalPersistentContext<int> currentPage;
        private List<Tab> tabs;
        private bool initialized;

        private class Tab
        {
            public string TabName;
            public List<InspectorProperty> InspectorProperties = new List<InspectorProperty>();
            public ValueResolver<string> Title;
        }

        protected override void Initialize()
        {
            this.tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(this.Property);
            this.tabs = new List<Tab>();
            var addLastTabs = new List<Tab>();

            for (int j = 0; j < this.Property.Children.Count; j++)
            {
                var child = this.Property.Children[j];
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

            this.Property.State.Create<int>(CurrentTabIndexKey, true, 0);
            this.Property.State.Create<int>(TabCountKey, false, this.tabs.Count);
            var currentIndex = this.GetClampedCurrentIndex();

            var currentTab = this.tabs[currentIndex];

            var selectedTabGroup = this.tabGroup.RegisterTab(currentTab.TabName);
            this.tabGroup.SetCurrentPage(selectedTabGroup);

            this.isChangingTabName = true;
            this.Property.State.Create<string>(CurrentTabNameKey, false, currentTab.TabName);
            this.isChangingTabName = false;

            this.initialized = true;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var attribute = this.Attribute;

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

            this.GetClampedCurrentIndex();

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            tabGroup.BeginGroup(true, attribute.Paddingless ? GUIStyle.none : null);

            this.Property.State.Set(TabCountKey, this.tabs.Count);

            for (int i = 0; i < this.tabs.Count; i++)
            {
                var page = tabGroup.RegisterTab(this.tabs[i].TabName);
                page.Title = this.tabs[i].Title.GetValue();

                if (this.tabGroup.NextPage == null && this.tabGroup.CurrentPage == page)
                {
                    this.Property.State.Set<int>(CurrentTabIndexKey, i);
                }

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

        private int GetClampedCurrentIndex()
        {
            var currentIndex = this.Property.State.Get<int>(CurrentTabIndexKey);

            if (currentIndex < 0)
            {
                currentIndex = 0;
                this.Property.State.Set<int>(CurrentTabIndexKey, currentIndex);
            }
            else if (currentIndex >= this.tabs.Count)
            {
                currentIndex = this.tabs.Count - 1;
                this.Property.State.Set<int>(CurrentTabIndexKey, currentIndex);
            }

            return currentIndex;
        }

        public void OnSelfStateChanged(string state)
        {
            if (!this.initialized) return;

            if (state == CurrentTabIndexKey)
            {
                var index = this.GetClampedCurrentIndex();
                var tab = this.tabs[index];

                this.isChangingTabName = true;
                this.Property.State.Set<string>(CurrentTabNameKey, tab.TabName);
                this.isChangingTabName = false;

                this.tabGroup.GoToPage(this.tabs[index].TabName);
            }
            else if (state == CurrentTabNameKey && !this.isChangingTabName)
            {
                var name = this.Property.State.Get<string>(CurrentTabNameKey);
                int index = -1;

                for (int i = 0; i < this.tabs.Count; i++)
                {
                    if (this.tabs[i].TabName == name)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    Debug.LogError("There is no tab named '" + name + "' in the tab group '" + this.Property.NiceName + "'!");

                    index = this.Property.State.Get<int>(CurrentTabIndexKey);

                    this.isChangingTabName = true;
                    this.Property.State.Set<string>(CurrentTabNameKey, this.tabs[index].TabName);
                    this.isChangingTabName = false;
                }
                else
                {
                    this.Property.State.Set<int>(CurrentTabIndexKey, index);
                }
            }
        }
    }
}
#endif