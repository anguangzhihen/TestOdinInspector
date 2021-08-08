#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IDefinesGenericMenuItems.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEditor;

    /// <summary>
    /// An <see cref="OdinDrawer"/> can implement this interface to indicate that it defines right-click context menu items for properties that it draws.
    /// </summary>
    public interface IDefinesGenericMenuItems
    {
        /// <summary>
        /// Method that is invoked when a user has right-clicked a property, and the context menu is being built. The method is invoked in order of drawer priority.
        /// </summary>
        /// <param name="property">The property that has been right-clicked on.</param>
        /// <param name="genericMenu">The generic menu instance that is being built. Add items to this.</param>
        void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu);
    }
}
#endif