#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AttributeListExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension method for List&lt;Attribute&gt;
    /// </summary>
    public static class AttributeListExtensions
    {
        /// <summary>
        /// Determines whether the list contains a specific attribute.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns>
        ///   <c>true</c> if the specified attribute list has attribute; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasAttribute<T>(this IList<Attribute> attributeList)
            where T : Attribute
        {
            var count = attributeList.Count;

            for (int i = 0; i < count; i++)
                if (attributeList[i] is T)
                    return true;

            return false;
        }

        /// <summary>
        /// Adds the attribute if not exist.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static T GetOrAddAttribute<T>(this List<Attribute> attributeList)
            where T : Attribute, new()
        {
            var count = attributeList.Count;
            T attr;

            for (int i = 0; i < count; i++)
            {
                attr = attributeList[i] as T;

                if (attr != null)
                    return attr;
            }

            attr = new T();
            attributeList.Add(attr);
            return attr;
        }

        /// <summary>
        /// Adds the attribute if not exist.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this IList<Attribute> attributeList)
            where T : Attribute
        {
            T attr;

            for (int i = 0; i < attributeList.Count; i++)
            {
                attr = attributeList[i] as T;
                if (attr != null)
                {
                    return attr;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the attribute if not exist.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static T Add<T>(this List<Attribute> attributeList)
            where T : Attribute, new()
        {
            var attr = new T();
            attributeList.Add(attr);
            return attr;
        }

        /// <summary>
        /// Adds the attribute if not exist.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <param name="attr">The attribute.</param>
        /// <returns></returns>
        [Obsolete("This method is obsolete. Do something else!", true)]
        public static bool GetOrAddAttribute<T>(this List<Attribute> attributeList, T attr)
            where T : Attribute
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the type of the attribute of.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static bool RemoveAttributeOfType<T>(this List<Attribute> attributeList)
            where T : Attribute
        {
            var count = attributeList.Count;
            bool removed = false;

            for (int i = 0; i < count; i++)
            {
                if (attributeList[i] is T)
                {
                    attributeList.RemoveAt(i);
                    i--;
                    count--;
                    removed = true;
                }
            }

            return removed;
        }
    }
}
#endif