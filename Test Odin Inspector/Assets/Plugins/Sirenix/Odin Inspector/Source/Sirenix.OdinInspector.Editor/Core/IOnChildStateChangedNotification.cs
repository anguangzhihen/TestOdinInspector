#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IOnChildStateChangedNotification.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public interface IOnChildStateChangedNotification
    {
        void OnChildStateChanged(int childIndex, string state);
    }
}
#endif