#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BaseAtomHandler.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    public abstract class BaseAtomHandler<T> : IAtomHandler<T>
    {
        private static readonly bool IsValueType = typeof(T).IsValueType;

        public Type AtomType { get { return typeof(T); } }

        public bool Compare(T a, T b)
        {
            if (IsValueType)
            {
                return this.CompareImplementation(a, b);
            }

            if (object.ReferenceEquals(a, b))
            {
                return true;
            }
            else if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
            {
                return false;
            }

            return this.CompareImplementation(a, b);
        }

        protected abstract bool CompareImplementation(T a, T b);

        public abstract T CreateInstance();

        public bool Compare(object a, object b)
        {
            return this.Compare((T)a, (T)b);
        }

        public void Copy(ref T from, ref T to)
        {
            if (IsValueType)
            {
                this.CopyImplementation(ref from, ref to);
            }
            else
            {
                if (object.ReferenceEquals(from, to))
                {
                    return;
                }

                if (object.ReferenceEquals(from, null))
                {
                    to = default(T);
                }
                else if (object.ReferenceEquals(to, null))
                {
                    to = this.CreateInstance();
                    this.CopyImplementation(ref from, ref to);
                }
                else
                {
                    this.CopyImplementation(ref from, ref to);
                }
            }
        }

        protected abstract void CopyImplementation(ref T from, ref T to);

        public void Copy(ref object from, ref object to)
        {
            T tFrom = (T)from;
            T tTo = (T)to;

            this.Copy(ref tFrom, ref tTo);

            from = tFrom;
            to = tTo;
        }

        object IAtomHandler.CreateInstance()
        {
            return this.CreateInstance();
        }
    }
}
#endif