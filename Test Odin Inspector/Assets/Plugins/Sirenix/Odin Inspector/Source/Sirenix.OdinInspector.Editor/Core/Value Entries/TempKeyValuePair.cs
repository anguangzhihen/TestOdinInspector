#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TempKeyValuePair.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    [ShowOdinSerializedPropertiesInInspector]
    public class TempKeyValuePair<TKey, TValue>
    {
        [ShowInInspector]
        public TKey Key;

        [ShowInInspector]
        public TValue Value;
    }
}
#endif