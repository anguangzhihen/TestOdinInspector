//-----------------------------------------------------------------------
// <copyright file="ISearchFilterable.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// Implement this interface to create custom matching
    /// logic for search filtering in the inspector.
    /// </summary>
    /// <example>
    /// <para>The following example shows how you might do this:</para>
    /// <code>
    /// public class MyCustomClass : ISearchFilterable
    /// {
    ///     public bool SearchEnabled;
    ///     public string MyStr;
    ///     
    ///     public bool IsMatch(string searchString)
    ///     {
    ///         if (SearchEnabled)
    ///         {
    ///             return MyStr.Contains(searchString);
    ///         }
    ///         
    ///         return false;
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface ISearchFilterable
    {
        bool IsMatch(string searchString);
    }
}