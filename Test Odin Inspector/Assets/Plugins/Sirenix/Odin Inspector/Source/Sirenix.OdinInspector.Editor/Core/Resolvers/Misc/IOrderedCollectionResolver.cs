#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IOrderedCollectionResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public interface IOrderedCollectionResolver : ICollectionResolver
    {
        void QueueRemoveAt(int index);

        void QueueRemoveAt(int index, int selectionIndex);

        void QueueInsertAt(int index, object[] values);

        void QueueInsertAt(int index, object value, int selectionIndex);
    }
}
#endif