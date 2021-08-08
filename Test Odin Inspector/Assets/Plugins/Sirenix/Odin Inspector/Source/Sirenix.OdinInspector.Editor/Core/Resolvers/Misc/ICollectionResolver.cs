#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ICollectionResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    public interface ICollectionResolver : IApplyableResolver, IRefreshableResolver
    {
        bool IsReadOnly { get; }

        Type ElementType { get; }

        int MaxCollectionLength { get; }

        event Action<CollectionChangeInfo> OnBeforeChange;
        event Action<CollectionChangeInfo> OnAfterChange;

        void QueueRemove(object[] values);

        void QueueRemove(object value, int selectionIndex);

        void QueueAdd(object[] values);

        void QueueAdd(object value, int selectionIndex);

        void QueueClear();

        bool CheckHasLengthConflict();

        [Obsolete("Use the overload that takes a CollectionChangeInfo instead.", false)]
        void EnqueueChange(Action action);
        void EnqueueChange(Action action, CollectionChangeInfo info);
    }
}
#endif