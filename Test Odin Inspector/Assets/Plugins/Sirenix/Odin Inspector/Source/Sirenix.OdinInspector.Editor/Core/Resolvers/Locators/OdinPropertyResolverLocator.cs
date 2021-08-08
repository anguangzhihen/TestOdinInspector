#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinPropertyResolverLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Base class for locator of <see cref="OdinPropertyResolver"/>. Use <see cref="DefaultOdinPropertyResolverLocator"/> for default implementation.
    /// </summary>
    public abstract class OdinPropertyResolverLocator
    {
        /// <summary>
        /// Gets an <see cref="OdinPropertyResolver"/> instance for the specified property.
        /// </summary>
        /// <param name="property">The property to get an <see cref="OdinPropertyResolver"/> instance for.</param>
        /// <returns>An instance of <see cref="OdinPropertyResolver"/> to resolver the specified property.</returns>
        public abstract OdinPropertyResolver GetResolver(InspectorProperty property);
    }
}
#endif