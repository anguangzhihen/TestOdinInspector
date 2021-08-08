#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerChainResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public abstract class DrawerChainResolver
    {
        public abstract DrawerChain GetDrawerChain(InspectorProperty property);
    }
}
#endif