#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerOverview.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Work in progress
//namespace Sirenix.OdinInspector.Editor
//{
#pragma warning disable

//    using Sirenix.Utilities;
//    using Sirenix.Utilities.Editor;
//    using System.Linq;
//    using UnityEditor;
//    using UnityEngine;
//    using System;
//    using System.Collections.Generic;
//    using System.Globalization;

//    /// <summary>
//    /// Oveview of all custom drawers, Window can be found in "Tools > Odin Inspector > Preferences > DrawerOverview".
//    /// </summary>
//    [SirenixEditorConfig("Odin Inspector/Drawers/Drawer Overview")]
//    public class DrawerOverview : GlobalConfig<DrawerOverview>
//    {
//        //[FoldoutGroup("Filters")]
//        //[ShowInInspector]
//        //[OnValueChanged("UpdateSearch")]
//        //private string filterByAttribute;

//        //[FoldoutGroup("Filters")]
//        //[ShowInInspector]
//        //[OnValueChanged("UpdateSearch")]
//        //private string search;

//        [BoxGroup("Filters")]
//        [ShowInInspector]
//        [OnValueChanged("UpdateSearch")]
//        [EnumToggleButtonsAttribute]
//        private AssemblyTypes assembly;

//        [BoxGroup("Filters")]
//        [ShowInInspector]
//        [OnValueChanged("UpdateSearch")]
//        [EnumToggleButtonsAttribute]
//        private DrawerType drawerTypes = DrawerType.Attribute | DrawerType.Value |DrawerType.Group;

//        private static ValueDropdownList<bool> orderByNames = new ValueDropdownList<bool>()
//        {
//            { "Priority", false },
//            { "Name", true }
//        };

//        private List<Item> searchResult;

//        private void UpdateSearch()
//        {
//            if (Event.current.type == EventType.Repaint && this.searchResult == null)
//            {
//                this.searchResult = DrawerLocator
//                    .GetGroupDrawerInfos().Select(x => new Item(DrawerType.Group, x))
//                    .Append(
//                        DrawerLocator.GetAttributeDrawerInfos().GroupBy(x => x.DrawnAttributeType).Select(x =>
//                        new Item(DrawerType.Attribute, x.First(), x.Select(c => new Item(DrawerType.Attribute, c)).ToList())
//                    ))
//                    .Append(DrawerLocator.GetPropertyDrawerInfos().Select(c => new Item(DrawerType.Value, c)))
//                    .ToList();
//            }

//            if (this.searchResult == null)
//            {
//                return;
//            }

//            this.Filter(this.searchResult);
//            this.searchResult.Where(x => x.Childs != null).ForEach(x => this.Filter(x.Childs));

//            this.Sort(this.searchResult);
//            this.searchResult.Where(x => x.Childs != null).ForEach(x => this.Sort(x.Childs));
//        }

//        private void Filter(List<Item> searchResult)
//        {
//            for (int i = 0; i < searchResult.Count; i++)
//            {
//                var item = searchResult[i];
//                item.Show = (item.DrawerType & this.drawerTypes) != 0;
//                item.Show = item.Show && ((int)item.DrawerInfo.DrawerType.Assembly.GetAssemblyTypeFlag() & (int)this.assembly) != 0;
//            }
//        }

//        private void Sort(List<Item> searchResult)
//        {
//            searchResult.Sort((a, b) => b.Show.CompareTo(a.Show) == 0 ? b.DrawerInfo.Priority.CompareTo(a.DrawerInfo.Priority) : b.Show.CompareTo(a.Show));
//        }

//        [OnInspectorGUI]
//        private void Drawers()
//        {
//            if (this.searchResult == null)
//            {
//                this.UpdateSearch();
//                return;
//            }

//            SirenixEditorGUI.BeginHorizontalToolbar();
//            GUILayout.Label("Drawers");
//            SirenixEditorGUI.EndHorizontalToolbar();

//            SirenixEditorGUI.BeginVerticalList();
//            foreach (var item in this.searchResult)
//            {
//                GUIHelper.PushHierarchyMode(true);
//                item.DrawItem();
//                GUIHelper.PopHierarchyMode();
//            }
//            SirenixEditorGUI.EndVerticalList();
//        }

//        [Flags]
//        private enum DrawerType
//        {
//            Group = 1 << 1,
//            Attribute = 1 << 2,
//            Value = 1 << 3,
//        }

//        private enum OrderBy
//        {
//            Priority,
//            Name
//        }

//        [Flags]
//        private enum AssemblyTypes
//        {
//            CustomTypes = AssemblyTypeFlags.UserTypes | AssemblyTypeFlags.UserEditorTypes,
//            PluginTypes = ~(AssemblyTypeFlags.UserTypes | AssemblyTypeFlags.UserEditorTypes) & AssemblyTypeFlags.All
//        }

//        private class Item
//        {
//            public bool Show;
//            public GUIContent Name;
//            public GUIContent Priority;
//            public DrawerInfo DrawerInfo;
//            public List<Item> Childs;
//            public DrawerType DrawerType;

//            private bool isToggled = false;
//            private bool toggled = false;

//            public Item(DrawerType type, DrawerInfo info, List<Item> childs = null)
//            {
//                this.DrawerInfo = info;
//                this.DrawerType = type;
//                this.Childs = childs;

//                if (type == DrawerType.Attribute && childs != null)
//                {
//                    this.Name = new GUIContent("[" + info.DrawnAttributeType.GetNiceName() + "]");

//                    //if (childs.GroupBy(x => x.Priority.ToString()).Count() == 1)
//                    //{
//                    //    this.Priority = new GUIContent(childs[0].DrawerInfo.Priority.ToString());
//                    //    childs.ForEach(x => x.Priority = GUIContent.none);
//                    //}
//                    //else
//                    //{
//                    //    this.Priority = new GUIContent("--");
//                    //}
//                    this.Priority = GUIContent.none;
//                }
//                else if (type == DrawerType.Attribute && childs == null)
//                {
//                    this.Name = new GUIContent("      " + info.DrawerType.GetNiceName());
//                    this.Priority = new GUIContent(info.Priority.ToString());
//                }
//                else if (type == DrawerType.Group)
//                {
//                    this.Name = new GUIContent("[" + info.DrawnAttributeType.GetNiceName() + "]" + " : " + info.DrawerType.GetNiceName());
//                    this.Priority = new GUIContent(info.Priority.ToString());
//                }
//                else
//                {
//                    this.Name = new GUIContent(info.DrawerType.GetNiceName());
//                    this.Priority = new GUIContent(info.Priority.ToString());
//                }
//            }

//            public void DrawItem()
//            {
//                if (this.Show)
//                {
//                    SirenixEditorGUI.BeginListItem(true);
//                    EditorGUILayout.BeginHorizontal();
//                    {
//                        if (this.Childs == null)
//                        {
//                            GUILayout.Label(this.Name, GUILayoutOptions.ExpandWidth());
//                        }
//                        else
//                        {
//                            GUILayout.Label(this.Name, SirenixGUIStyles.LabelCentered, GUILayoutOptions.ExpandWidth());
//                        }

//                        GUILayout.FlexibleSpace();
//                        GUILayout.Label(this.Priority, GUILayoutOptions.Width(190));
//                    }
//                    EditorGUILayout.EndHorizontal();
//                    SirenixEditorGUI.EndListItem();

//                    if (this.Childs != null)
//                    {
//                        for (int i = 0; i < this.Childs.Count; i++)
//                        {
//                            this.Childs[i].DrawItem();
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
#endif