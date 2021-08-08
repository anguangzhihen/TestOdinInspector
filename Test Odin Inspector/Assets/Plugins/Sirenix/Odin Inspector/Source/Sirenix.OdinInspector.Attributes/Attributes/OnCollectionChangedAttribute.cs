//-----------------------------------------------------------------------
// <copyright file="OnCollectionChangedAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    ///    OnCollectionChanged can be put on collections, and provides an event callback when the collection is about to be changed through the inspector, 
    ///    and when the collection has been changed through the inspector. Additionally, it provides a CollectionChangeInfo struct containing information
    ///    about the exact changes made to the collection. This attribute works for all collections with a collection resolver, amongst them arrays, lists,
    ///    dictionaries, hashsets, stacks and linked lists.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <note type="note">Note that this attribute only works in the editor! Collections changed by script will not trigger change events!</note>
    /// </remarks>
    /// <example>
    /// <para>The following example shows how OnCollectionChanged can be used to get callbacks when a collection is being changed.</para>
    /// <code>
    /// [OnCollectionChanged("Before", "After")]
    /// public List&lt;string&gt; list;
    /// 
    /// public void Before(CollectionChangeInfo info)
    /// {
    ///     if (info.ChangeType == CollectionChangeType.Add || info.ChangeType == CollectionChangeType.Insert)
    ///     {
    ///         Debug.Log("Adding to the list!");
    ///     }
    ///     else if (info.ChangeType == CollectionChangeType.RemoveIndex || info.ChangeType == CollectionChangeType.RemoveValue)
    ///     {
    ///         Debug.Log("Removing from the list!");
    ///     }
    /// }
    /// 
    /// public void After(CollectionChangeInfo info)
    /// {
    ///     if (info.ChangeType == CollectionChangeType.Add || info.ChangeType == CollectionChangeType.Insert)
    ///     {
    ///         Debug.Log("Finished adding to the list!");
    ///     }
    ///     else if (info.ChangeType == CollectionChangeType.RemoveIndex || info.ChangeType == CollectionChangeType.RemoveValue)
    ///     {
    ///         Debug.Log("Finished removing from the list!");
    ///     }
    /// }
    /// </code>
    /// </example>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class OnCollectionChangedAttribute : Attribute
    {
        public string Before;
        public string After;

        public OnCollectionChangedAttribute() { }
        public OnCollectionChangedAttribute(string after)
        {
            this.After = after;
        }

        public OnCollectionChangedAttribute(string before, string after)
        {
            this.Before = before;
            this.After = after;
        }
    }
}