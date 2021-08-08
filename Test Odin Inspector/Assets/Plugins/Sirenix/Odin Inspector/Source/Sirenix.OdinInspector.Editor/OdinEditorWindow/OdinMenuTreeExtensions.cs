#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTreeExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Class with utility methods for <see cref="OdinMenuTree" />s and <see cref="OdinMenuItem" />s.
    /// </summary>
    /// <example>
    /// <code>
    /// OdinMenuTree tree = new OdinMenuTree();
    /// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
    ///     .AddThumbnailIcons();
    /// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
    /// // etc...
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuTree" />
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public static class OdinMenuTreeExtensions
    {
        /// <summary>
        /// Adds the menu item at the specified menu item path and populates the result list with all menu items created in order to add the menuItem at the specified path.
        /// </summary>
        /// <param name="tree">The tree instance.</param>
        /// <param name="result">The result list.</param>
        /// <param name="path">The menu item path.</param>
        /// <param name="menuItem">The menu item.</param>
        public static void AddMenuItemAtPath(this OdinMenuTree tree, ICollection<OdinMenuItem> result, string path, OdinMenuItem menuItem)
        {
            var curr = tree.Root;

            if (!string.IsNullOrEmpty(path))
            {
                if (path[0] == '/' || path[path.Length - 1] == '/')
                {
                    path = path.Trim();
                }

                var iFrom = 0;
                var iTo = 0;

                do
                {
                    iTo = path.IndexOf('/', iFrom);

                    string name;

                    if (iTo < 0)
                    {
                        iTo = path.Length - 1;
                        name = path.Substring(iFrom, iTo - iFrom + 1);
                    }
                    else
                    {
                        name = path.Substring(iFrom, iTo - iFrom);
                    }

                    var childs = curr.ChildMenuItems;

                    // OdinMenuItem child = curr.ChildMenuItems.FirstOrDefault(x => x.Name == name);
                    // If we assume people add menu items in a local order, then starting our search from then end of the list, should be faster.
                    OdinMenuItem child = null;
                    for (int i = childs.Count - 1; i >= 0; i--)
                    {
                        if (childs[i].Name == name)
                        {
                            child = childs[i];
                            break;
                        }
                    }

                    if (child == null)
                    {
                        child = new OdinMenuItem(tree, name, null);
                        curr.ChildMenuItems.Add(child);
                    }

                    result.Add(child);

                    curr = child;

                    iFrom = iTo + 1;
                } while (iTo != path.Length - 1);
            }

            // var oldItem = curr.ChildMenuItems.FirstOrDefault(x => x.Name == menuItem.Name);
            // If we assume people add menu items in a local order, then starting our search from then end of the list, should be faster.
            var currChilds = curr.ChildMenuItems;
            OdinMenuItem oldItem = null;
            for (int i = currChilds.Count - 1; i >= 0; i--)
            {
                if (currChilds[i].Name == menuItem.Name)
                {
                    oldItem = currChilds[i];
                    break;
                }
            }

            if (oldItem != null)
            {
                curr.ChildMenuItems.Remove(oldItem);
                menuItem.ChildMenuItems.AddRange(oldItem.ChildMenuItems);
            }

            curr.ChildMenuItems.Add(menuItem);
            result.Add(menuItem);
        }

        private static List<OdinMenuItem> cache = new List<OdinMenuItem>(5);

        /// <summary>
        /// Adds the menu item at specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="path">The menu item path.</param>
        /// <param name="menuItem">The menu item.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddMenuItemAtPath(this OdinMenuTree tree, string path, OdinMenuItem menuItem)
        {
            cache.Clear();
            AddMenuItemAtPath(tree, cache, path, menuItem);
            return cache;
        }

        /// <summary>
        /// Gets the menu item at the specified path, returns null non was found.
        /// </summary>
        public static OdinMenuItem GetMenuItem(this OdinMenuTree tree, string menuPath)
        {
            var curr = tree.Root;

            if (!string.IsNullOrEmpty(menuPath))
            {
                menuPath = menuPath.Trim('/') + "/"; // Adding the '/' Makes the algorithm simpler

                var iFrom = 0;
                var iTo = 0;

                do
                {
                    iTo = menuPath.IndexOf('/', iFrom);
                    var name = menuPath.Substring(iFrom, iTo - iFrom);
                    var child = curr.ChildMenuItems.FirstOrDefault(x => x.Name == name) ??
                                curr.ChildMenuItems.FirstOrDefault(x => x.SmartName == name);

                    if (child == null)
                    {
                        return null;
                    }

                    curr = child;

                    iFrom = iTo + 1;
                } while (iTo != menuPath.Length - 1);
            }

            return curr;
        }

        /// <summary>
        /// Adds all asset instances from the specified path and type into a single <see cref="OdinMenuItem"/> at the specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.. 
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu item path.</param>
        /// <param name="assetFolderPath">The asset folder path.</param>
        /// <param name="type">The type of objects.</param>
        /// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAllAssetsAtPathCombined(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false)
        {
            assetFolderPath = (assetFolderPath ?? "").TrimEnd('/') + "/";
            if (!assetFolderPath.ToLower().StartsWith("assets/"))
            {
                assetFolderPath = "Assets/" + assetFolderPath;
            }
            assetFolderPath = assetFolderPath.TrimEnd('/') + "/";

            var assets = AssetDatabase.GetAllAssetPaths()
                .Where(x =>
                {
                    if (includeSubDirectories)
                    {
                        return x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase);
                    }
                    return string.Compare(PathUtilities.GetDirectoryName(x).Trim('/'), assetFolderPath.Trim('/'), true) == 0;
                })
                .Select(x =>
                {
                    UnityEngine.Object tmp = null;

                    return (Func<object>)(() =>
                    {
                        if (tmp == null)
                        {
                            tmp = AssetDatabase.LoadAssetAtPath(x, type);
                        }

                        return tmp;
                    });
                })
                .ToList();


            string path, menu;
            SplitMenuPath(menuPath, out path, out menu);

            return tree.AddMenuItemAtPath(path, new OdinMenuItem(tree, menu, assets));
        }

        /// <summary>
        /// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu item path.</param>
        /// <param name="assetFolderPath">The asset folder path.</param>
        /// <param name="type">The type.</param>
        /// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
        /// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false, bool flattenSubDirectories = false)
        {
            assetFolderPath = (assetFolderPath ?? "").TrimEnd('/') + "/";
            if (!assetFolderPath.ToLower().StartsWith("assets/"))
            {
                assetFolderPath = "Assets/" + assetFolderPath;
            }
            assetFolderPath = assetFolderPath.TrimEnd('/') + "/";

            var assets = AssetDatabase.GetAllAssetPaths()
                .Where(x =>
                {
                    if (includeSubDirectories)
                    {
                        return x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase);
                    }
                    return string.Compare(PathUtilities.GetDirectoryName(x).Trim('/'), assetFolderPath.Trim('/'), true) == 0;
                });

            menuPath = menuPath ?? "";
            menuPath = menuPath.TrimStart('/');

            HashSet<OdinMenuItem> result = new HashSet<OdinMenuItem>();

            foreach (var assetPath in assets)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, type);

                if (obj == null)
                {
                    continue;
                }

                var name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                var path = menuPath;

                if (flattenSubDirectories == false)
                {
                    var subPath = (PathUtilities.GetDirectoryName(assetPath).TrimEnd('/') + "/");
                    subPath = subPath.Substring(assetFolderPath.Length);
                    if (subPath.Length != 0)
                    {
                        path = path.Trim('/') + "/" + subPath;
                    }
                }

                path = path.Trim('/') + "/" + name;
                string menu;
                SplitMenuPath(path, out path, out menu);
                tree.AddMenuItemAtPath(result, path, new OdinMenuItem(tree, menu, obj));
            }

            return result;
        }

        /// <summary>
        /// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu item path.</param>
        /// <param name="assetFolderPath">The asset folder path.</param>
        /// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
        /// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, bool includeSubDirectories = false, bool flattenSubDirectories = false)
        {
            return AddAllAssetsAtPath(tree, menuPath, assetFolderPath, typeof(UnityEngine.Object), includeSubDirectories, flattenSubDirectories);
        }

        /// <summary>
        /// Adds the asset at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuItemPath">The menu item path.</param>
        /// <param name="assetPath">The asset path.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath)
        {
            return AddAssetAtPath(tree, menuItemPath, assetPath, typeof(UnityEngine.Object));
        }

        /// <summary>
        /// Adds the asset at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuItemPath">The menu item path.</param>
        /// <param name="assetPath">The asset path.</param>
        /// <param name="type">The type.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath, Type type)
        {
            if (!assetPath.StartsWith("assets/", StringComparison.InvariantCultureIgnoreCase))
            {
                assetPath = "Assets/" + assetPath;
            }

            var obj = AssetDatabase.LoadAssetAtPath(assetPath, type);

            string name;
            SplitMenuPath(menuItemPath, out menuItemPath, out name);
            return tree.AddMenuItemAtPath(menuItemPath, new OdinMenuItem(tree, name, obj));
        }

        /// <summary>
        /// Sorts the entire tree of menu items recursively by name with respects to numbers.
        /// </summary>
        public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this OdinMenuTree tree, bool placeFoldersFirst = true)
        {
            var result = SortMenuItemsByName(tree.EnumerateTree(true), placeFoldersFirst);
            tree.MarkDirty();
            return result;
        }

        private class OdinMenuItemComparer : IComparer<OdinMenuItem>
        {
            public Comparison<OdinMenuItem> CustomComparison;
            public bool PlaceFoldersFirst;
            public bool IgnoreLeadingZeroes;
            public bool IgnoreWhiteSpace;
            public bool IgnoreCase;

            public OdinMenuItemComparer(Comparison<OdinMenuItem> customComparison = null)
            {
                this.CustomComparison = customComparison;
            }

            public int Compare(OdinMenuItem a, OdinMenuItem b)
            {
                if (this.CustomComparison != null)
                {
                    return this.CustomComparison(a, b);
                }

                if (this.PlaceFoldersFirst)
                {
                    if (a.ChildMenuItems.Count > 0 && b.ChildMenuItems.Count == 0) return -1;
                    if (b.ChildMenuItems.Count > 0 && a.ChildMenuItems.Count == 0) return 1;
                }

                return StringUtilities.NumberAwareStringCompare(a.SmartName, b.SmartName, this.IgnoreLeadingZeroes, this.IgnoreWhiteSpace, this.IgnoreCase);
            }
        }

        /// <summary>
        /// Sorts the collection of menu items recursively by name with respects to numbers. This is a stable sort, meaning that equivalently ordered items will remain in the same order as they start.
        /// </summary>
        public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this IEnumerable<OdinMenuItem> menuItems, bool placeFoldersFirst = true, bool ignoreLeadingZeroes = true, bool ignoreWhiteSpace = true, bool ignoreCase = false)
        {
            var comparer = new OdinMenuItemComparer();
            comparer.PlaceFoldersFirst = placeFoldersFirst;
            comparer.IgnoreLeadingZeroes = ignoreLeadingZeroes;
            comparer.IgnoreWhiteSpace = ignoreWhiteSpace;
            comparer.IgnoreCase = ignoreCase;
            return SortMenuItemsByName(menuItems, comparer);
        }

        /// <summary>
        /// Sorts the collection of menu items recursively using a given custom comparison. This is a stable sort, meaning that equivalently ordered items will remain in the same order as they start.
        /// </summary>
        public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this IEnumerable<OdinMenuItem> menuItems, Comparison<OdinMenuItem> comparison)
        {
            if (comparison == null) throw new ArgumentNullException("comparison");
            var comparer = new OdinMenuItemComparer(comparison);
            return SortMenuItemsByName(menuItems, comparer);
        }

        private static IEnumerable<OdinMenuItem> SortMenuItemsByName(IEnumerable<OdinMenuItem> menuItems, IComparer<OdinMenuItem> comparer)
        {
            OdinMenuItem first = null;

            foreach (var menuItem in menuItems)
            {
                if (first == null) first = menuItem;

                // We must use Linq's OrderBy to ensure that we get a stable sort.
                var newChildMenuItems = menuItem.ChildMenuItems.OrderBy(item => item, comparer).ToList();
                menuItem.SetChildMenuItems(newChildMenuItems);

                // List.Sort() is not stable, so we cannot use that to sort in-place without writing our own stable sorting algorithm for lists.
                //menuItem.ChildMenuItems.Sort(comparer);
            }

            if (first != null && first.MenuTree != null)
            {
                first.MenuTree.MarkDirty();
            }

            return menuItems;
        }

        /// <summary>
        /// Adds the specified object at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu path.</param>
        /// <param name="instance">The object instance.</param>
        /// <param name="forceShowOdinSerializedMembers">Set this to true if you want Odin serialzied members such as dictionaries and generics to be shown as well.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddObjectAtPath(this OdinMenuTree tree, string menuPath, object instance, bool forceShowOdinSerializedMembers = false)
        {
            string name;
            SplitMenuPath(menuPath, out menuPath, out name);

            if (forceShowOdinSerializedMembers && !(instance as UnityEngine.Object))
            {
                return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, new SerializedValueWrapper(instance)));
            }
            else
            {
                return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, instance));
            }
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection with the specified ObjectInstanceType.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcons<T>(this IEnumerable<OdinMenuItem> menuItems, Func<T, Texture> getIcon)
        {
            foreach (var item in menuItems)
            {
                if (item.Value != null && item.Value is T)
                {
                    var localItem = item;
                    localItem.IconGetter = () => getIcon((T)localItem.Value);
                }
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection with the specified ObjectInstanceType.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcons<T>(this IEnumerable<OdinMenuItem> menuItems, Func<T, Sprite> getIcon)
        {
            foreach (var item in menuItems)
            {
                if (item.Value != null && item.Value is T)
                {
                    var localItem = item;
                    localItem.IconGetter = () => AssetPreview.GetAssetPreview(getIcon((T)localItem.Value));
                }
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Func<OdinMenuItem, Texture> getIcon)
        {
            foreach (var item in menuItems)
            {
                var localItem = item;
                localItem.IconGetter = () => getIcon(localItem);
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Func<OdinMenuItem, Sprite> getIcon)
        {
            foreach (var item in menuItems)
            {
                var localItem = item;
                localItem.IconGetter = () => AssetPreview.GetAssetPreview(getIcon(localItem));
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to the last menu item in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Sprite icon)
        {
            menuItems.AddIcon(AssetPreview.GetAssetPreview(icon));

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to the last menu item in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, EditorIcon icon)
        {
            menuItems.AddIcon(icon.Highlighted, icon.Raw);

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to the last menu item in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon)
        {
            var last = menuItems.LastOrDefault();
            if (last != null)
            {
                last.Icon = icon;
                last.IconSelected = icon;
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to the last menu item in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon, Texture iconSelected)
        {
            var last = menuItems.LastOrDefault();
            if (last != null)
            {
                last.Icon = icon;
                last.IconSelected = iconSelected;
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, EditorIcon icon)
        {
            foreach (var item in menuItems)
            {
                item.Icon = icon.Highlighted;
                item.IconSelected = icon.Raw;
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Texture icon)
        {
            foreach (var item in menuItems)
            {
                item.Icon = icon;
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Texture icon, Texture iconSelected)
        {
            foreach (var item in menuItems)
            {
                item.Icon = icon;
                item.IconSelected = iconSelected;
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the asset mini thumbnail as an icon to all menu items in the collection. If the menu items object is null then a Unity folder icon is assigned.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddThumbnailIcons(this IEnumerable<OdinMenuItem> menuItems, bool preferAssetPreviewAsIcon = false)
        {
            foreach (var item in menuItems)
            {
                AddThumbnailIcon(item, preferAssetPreviewAsIcon);
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the asset mini thumbnail as an icon to all menu items in the collection. If the menu items object is null then a Unity folder icon is assigned.
        /// </summary>
        public static OdinMenuItem AddThumbnailIcon(this OdinMenuItem item, bool preferAssetPreviewAsIcon)
        {
            var instance = item.Value;

            var unityObject = instance as UnityEngine.Object;

            if (unityObject)
            {
                if (preferAssetPreviewAsIcon)
                {
                    item.IconGetter = () => GUIHelper.GetAssetThumbnail(unityObject, unityObject.GetType(), preferAssetPreviewAsIcon);
                }
                else
                {
                    item.Icon = GUIHelper.GetAssetThumbnail(unityObject, unityObject.GetType(), preferAssetPreviewAsIcon);
                }
                return item;
            }

            var type = instance as Type;
            if (type != null)
            {

                if (preferAssetPreviewAsIcon)
                {
                    item.IconGetter = () => GUIHelper.GetAssetThumbnail(null, type, preferAssetPreviewAsIcon);
                }
                else
                {
                    item.Icon = GUIHelper.GetAssetThumbnail(null, type, preferAssetPreviewAsIcon);
                }

                return item;
            }

            var assetPath = instance as string;
            if (assetPath != null)
            {
                if (assetPath != null)
                {
                    if (File.Exists(assetPath))
                    {
                        item.Icon = InternalEditorUtility.GetIconForFile(assetPath);
                    }
                    else if (Directory.Exists(assetPath))
                    {
                        item.Icon = EditorIcons.UnityFolderIcon;
                    }
                }
            }

            return item;
        }

        private static void SplitMenuPath(string menuPath, out string path, out string name)
        {
            menuPath = menuPath.Trim('/');
            var i = menuPath.LastIndexOf('/');

            if (i == -1)
            {
                path = "";
                name = menuPath;
            }
            else
            {
                path = menuPath.Substring(0, i);
                name = menuPath.Substring(i + 1);
            }
        }

        private static bool ReplaceDollarSignWithAssetName(ref string menuItem, string name)
        {
            if (menuItem == null)
            {
                return false;
            }

            if (menuItem == "$")
            {
                menuItem = name;
            }

            if (menuItem.StartsWith("$/"))
            {
                menuItem = name + menuItem.Substring(2);
            }

            if (menuItem.EndsWith("/$"))
            {
                menuItem = menuItem.Substring(0, menuItem.Length - 1) + name;
            }

            if (menuItem.Contains("/$/"))
            {
                menuItem = menuItem.Replace("/$/", "/" + name + "/");
                return true;
            }

            return false;
        }

        [ShowOdinSerializedPropertiesInInspector]
        private class SerializedValueWrapper
        {
            private object instance;

            [HideLabel, ShowInInspector, HideReferenceObjectPicker]
            public object Instance
            {
                get { return instance; }
                set { }
            }

            public SerializedValueWrapper(object obj)
            {
                this.instance = obj;
            }
        }
    }
}
#endif