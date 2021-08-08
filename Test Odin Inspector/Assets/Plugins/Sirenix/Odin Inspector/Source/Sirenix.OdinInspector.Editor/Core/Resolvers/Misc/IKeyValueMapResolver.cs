#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IKeyValueMapResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public interface IKeyValueMapResolver : ICollectionResolver
    {
        object GetKey(int selectionIndex, int childIndex);

        void QueueSet(object[] keys, object[] values);

        void QueueSet(object key, object value, int selectionIndex);

        void QueueRemoveKey(object[] keys);

        void QueueRemoveKey(object key, int selectionIndex);
    }
}
#endif