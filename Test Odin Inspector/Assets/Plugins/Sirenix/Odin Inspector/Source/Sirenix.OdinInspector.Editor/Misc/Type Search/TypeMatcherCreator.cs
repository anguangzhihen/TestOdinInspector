#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeMatcherCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    public abstract class TypeMatcherCreator
    {
        public abstract bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher);
    }
}
#endif