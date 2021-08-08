#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StateUpdaterLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public abstract class StateUpdaterLocator
    {
        public abstract StateUpdater[] GetStateUpdaters(InspectorProperty property);
    }
}
#endif