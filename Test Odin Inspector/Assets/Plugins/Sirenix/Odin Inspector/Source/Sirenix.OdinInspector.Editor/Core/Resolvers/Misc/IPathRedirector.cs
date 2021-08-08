#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IPathRedirector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public interface IPathRedirector
    {
        bool TryGetRedirectedProperty(string childName, out InspectorProperty property);
    }
}
#endif