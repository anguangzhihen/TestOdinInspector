#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IOnSelfStateChangedNotification.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public interface IOnSelfStateChangedNotification
    {
        void OnSelfStateChanged(string state);
    }
}
#endif