#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AnimationCurveAtomHandler.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Collections.Generic;
    using UnityEngine;

    [AtomHandler]
    public sealed class AnimationCurveAtomHandler : BaseAtomHandler<AnimationCurve>
    {
        public override AnimationCurve CreateInstance()
        {
            return new AnimationCurve();
        }

        protected override bool CompareImplementation(AnimationCurve a, AnimationCurve b)
        {
            if (a.postWrapMode != b.postWrapMode ||
                a.preWrapMode != b.preWrapMode ||
                a.keys.Length != b.keys.Length)
            {
                return false;
            }

            for (int i = 0; i < a.keys.Length; i++)
            {
                var aKey = a.keys[i];
                var bKey = b.keys[i];

                if (!EqualityComparer<Keyframe>.Default.Equals(aKey, bKey))
                {
                    return false;
                }
            }

            return true;
        }

        protected override void CopyImplementation(ref AnimationCurve from, ref AnimationCurve to)
        {
            to.postWrapMode = from.postWrapMode;
            to.preWrapMode = from.preWrapMode;

            while (to.keys.Length > from.keys.Length)
            {
                to.RemoveKey(to.keys.Length - 1);
            }

            while (to.keys.Length < from.keys.Length)
            {
                to.AddKey(UnityEngine.Random.Range(0f, 1f), 0f); // Just a random value, as it'll be set further down; adding the same time values several times does nothing
            }

            for (int i = 0; i < to.keys.Length; i++)
            {
                to.MoveKey(i, from.keys[i]);
            }
        }
    }
}
#endif