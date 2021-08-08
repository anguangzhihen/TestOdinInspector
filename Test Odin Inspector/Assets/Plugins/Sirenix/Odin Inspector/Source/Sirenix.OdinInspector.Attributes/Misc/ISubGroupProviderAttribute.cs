//-----------------------------------------------------------------------
// <copyright file="ISubGroupProviderAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Internal
{
#pragma warning disable

    using System.Collections.Generic;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public interface ISubGroupProviderAttribute
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <returns>Not yet documented.</returns>
        IList<PropertyGroupAttribute> GetSubGroupAttributes();

        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="attr">Not yet documented.</param>
        /// <returns>Not yet documented.</returns>
        string RepathMemberAttribute(PropertyGroupAttribute attr);
    }
}