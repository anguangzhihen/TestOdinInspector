#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AnimationCurveDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Animation curve property drawer.
    /// </summary>
    public sealed class AnimationCurveDrawer : DrawWithUnityBaseDrawer<AnimationCurve>
    {
        private AnimationCurve[] curvesLastFrame;

        private static Action clearCache;
        private static IAtomHandler<AnimationCurve> atomHandler = AtomHandlerLocator.GetAtomHandler<AnimationCurve>();

        static AnimationCurveDrawer()
        {
            MethodInfo mi = null;
            var type = AssemblyUtilities.GetTypeByCachedFullName("UnityEditorInternal.AnimationCurvePreviewCache");
            if (type != null)
            {
                var method = type.GetMethod("ClearCache", Flags.StaticAnyVisibility);
                var pars = method.GetParameters();
                if (pars != null && pars.Length == 0)
                {
                    mi = method;
                }
            }

            if (mi != null)
            {
                clearCache = EmitUtilities.CreateStaticMethodCaller(mi);
            }
#if SIRENIX_INTERNAL
            else
            {
                Debug.LogError("AnimationCurve fix no longer works, has Unity fixed it?");
            }
#endif
        }

        protected override void Initialize()
        {
            base.Initialize();

            if (clearCache != null)
            {
                // Unity bugfix:
                // The preview of animations curves doesn't work well with reordering, 
                // I suspect they use ControlId's as the pointer to the preview cache lookup.
                clearCache();

                this.curvesLastFrame = new AnimationCurve[this.ValueEntry.ValueCount];

                for (int i = 0; i < this.ValueEntry.ValueCount; i++)
                {
                    var value = this.ValueEntry.Values[i];
                    this.curvesLastFrame[i] = atomHandler.CreateInstance();
                    atomHandler.Copy(ref value, ref this.curvesLastFrame[i]);
                }
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (clearCache != null)
            {
                for (int i = 0; i < this.ValueEntry.ValueCount; i++)
                {
                    if (atomHandler.Compare(this.curvesLastFrame[i], this.ValueEntry.Values[i]) == false)
                    {
                        // An animation curve was changed from the outside!
                        clearCache();
                        break;
                    }
                }
            }

            base.DrawPropertyLayout(label);

            if (clearCache != null)
            {
                for (int i = 0; i < this.ValueEntry.ValueCount; i++)
                {
                    var value = this.ValueEntry.Values[i];
                    atomHandler.Copy(ref value, ref this.curvesLastFrame[i]);
                }
            }
        }
    }
}
#endif