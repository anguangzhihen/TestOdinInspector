#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IHasSpecialPropertyPaths.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public interface IHasSpecialPropertyPaths
    {
        string GetSpecialChildPath(int childIndex);
    }
}
#endif