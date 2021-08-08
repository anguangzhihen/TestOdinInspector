#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EquatableStructAtomHandler.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    public abstract class EquatableStructAtomHandler<T> : BaseAtomHandler<T> where T : struct
    {
        private static readonly Func<T, T, bool> Comparer;

        static EquatableStructAtomHandler()
        {
            Comparer = TypeExtensions.GetEqualityComparerDelegate<T>();
        }

        protected override bool CompareImplementation(T a, T b)
        {
            return Comparer(a, b);
        }

        protected override void CopyImplementation(ref T from, ref T to)
        {
            to = from;
        }

        public override T CreateInstance()
        {
            return default(T);
        }
    }
}
#endif