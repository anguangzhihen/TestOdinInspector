#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GenericMenuExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Collection of extension methods for <see cref="UnityEditor.GenericMenu"/>.
    /// </summary>
    public static class GenericMenuExtensions
    {
        private static readonly FieldInfo GenericMenu_MenuItems;
        private static readonly FieldInfo MenuItem_Content;
        private static readonly FieldInfo MenuItem_Func;
        private static readonly FieldInfo MenuItem_Func2;
        private static readonly FieldInfo MenuItem_On;

        static GenericMenuExtensions()
        {
            GenericMenu_MenuItems = typeof(GenericMenu).GetField("menuItems", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var menuItemType = typeof(GenericMenu).GetNestedType("MenuItem", BindingFlags.Public | BindingFlags.NonPublic);

            if (menuItemType != null)
            {
                MenuItem_Content = menuItemType.GetField("content", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MenuItem_Func = menuItemType.GetField("func", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MenuItem_Func2 = menuItemType.GetField("func2", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MenuItem_On = menuItemType.GetField("on", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (GenericMenu_MenuItems == null)
            {
                Debug.LogError("Could not find private Unity member GenericMenu.menuItems in this version of Unity. Some Odin functionality may be disabled.");
            }

            if (MenuItem_Content == null)
            {
                Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.content in this version of Unity. Some Odin functionality may be disabled.");
            }

            if (MenuItem_Func == null)
            {
                Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.func in this version of Unity. Some Odin functionality may be disabled.");
            }

            if (MenuItem_Func2 == null)
            {
                Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.func2 in this version of Unity. Some Odin functionality may be disabled.");
            }

            if (MenuItem_On == null)
            {
                Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.on in this version of Unity. Some Odin functionality may be disabled.");
            }
        }

        /// <summary>
        /// Removes all menu items with a given name from the GenericMenu.
        /// </summary>
        /// <param name="menu">The GenericMenu to remove items from.</param>
        /// <param name="name">The name of the items to remove.</param>
        /// <returns>True if any items were removed, otherwise false.</returns>
        public static bool RemoveMenuItems(this GenericMenu menu, string name)
        {
            if (GenericMenu_MenuItems == null || MenuItem_Content == null)
            {
                Debug.LogWarning("Cannot remove menu item from GenericMenu, as private Unity members were missing.");
                return false;
            }

            ArrayList menuItems = (ArrayList)GenericMenu_MenuItems.GetValue(menu);

            bool removed = false;

            for (int i = 0; i < menuItems.Count; i++)
            {
                var obj = menuItems[i];

                GUIContent content = (GUIContent)MenuItem_Content.GetValue(obj);

                if (content.text == name)
                {
                    menuItems.RemoveAt(i--);
                    removed = true;
                }
            }

            return removed;
        }

        /// <summary>
        /// Replaces the first found menu item with a given name with a new menu item, or if no such element is found, adds a new one.
        /// </summary>
        /// <param name="menu">The GenericMenu to replace items in.</param>
        /// <param name="name">The name of the items to remove.</param>
        /// <param name="func">The func to replace or add.</param>
        /// <param name="on">The on value to set the new menu item with.</param>
        /// <returns>True if an item was replaced, otherwise false.</returns>
        public static bool ReplaceOrAdd(this GenericMenu menu, string name, bool on, GenericMenu.MenuFunction func)
        {
            if (GenericMenu_MenuItems == null || MenuItem_Content == null || MenuItem_Func == null || MenuItem_Func2 == null || MenuItem_On == null)
            {
                Debug.LogWarning("Cannot replace menu items in GenericMenu, as private Unity members were missing.");
                return false;
            }

            ArrayList menuItems = (ArrayList)GenericMenu_MenuItems.GetValue(menu);

            bool replaced = false;

            for (int i = 0; i < menuItems.Count; i++)
            {
                var obj = menuItems[i];

                GUIContent content = (GUIContent)MenuItem_Content.GetValue(obj);

                if (content.text == name)
                {
                    MenuItem_Func.SetValue(obj, func);
                    MenuItem_Func2.SetValue(obj, null);
                    MenuItem_On.SetValue(obj, on);
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                menu.AddItem(new GUIContent(name), on, func);
            }

            return replaced;
        }

        /// <summary>
        /// Replaces the first found menu item with a given name with a new menu item, or if no such element is found, adds a new one.
        /// </summary>
        /// <param name="menu">The GenericMenu to replace items in.</param>
        /// <param name="name">The name of the items to remove.</param>
        /// <param name="on">The on value to set the new menu item with.</param>
        /// <param name="func2">The func to replace or add.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>
        /// True if an item was replaced, otherwise false.
        /// </returns>
        public static bool ReplaceOrAdd(this GenericMenu menu, string name, bool on, GenericMenu.MenuFunction2 func2, object userData)
        {
            if (GenericMenu_MenuItems == null || MenuItem_Content == null || MenuItem_Func == null || MenuItem_Func2 == null || MenuItem_On == null)
            {
                Debug.LogWarning("Cannot replace menu items in GenericMenu, as private Unity members were missing.");
                return false;
            }

            ArrayList menuItems = (ArrayList)GenericMenu_MenuItems.GetValue(menu);

            bool replaced = false;

            for (int i = 0; i < menuItems.Count; i++)
            {
                var obj = menuItems[i];

                GUIContent content = (GUIContent)MenuItem_Content.GetValue(obj);

                if (content.text == name)
                {
                    MenuItem_Func.SetValue(obj, null);
                    MenuItem_Func2.SetValue(obj, func2);
                    MenuItem_On.SetValue(obj, on);
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                menu.AddItem(new GUIContent(name), on, func2, userData);
            }

            return replaced;
        }
    }
}
#endif