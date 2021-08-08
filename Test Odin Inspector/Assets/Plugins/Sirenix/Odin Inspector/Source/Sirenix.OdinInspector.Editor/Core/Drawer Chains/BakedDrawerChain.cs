#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BakedDrawerChain.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class BakedDrawerChain : DrawerChain
    {
        private OdinDrawer[] bakedDrawerChain;
        private int index = -1;

        private int lastUpdatedId = -1;

        public BakedDrawerChain(InspectorProperty property, IEnumerable<OdinDrawer> chain)
            : base(property)
        {
            this.bakedDrawerChain = chain.ToArray();
        }

        public BakedDrawerChain(DrawerChain bakedChain)
            : base(bakedChain.Property)
        {
            this.BakedChain = bakedChain;
            this.Rebake();
        }

        public OdinDrawer[] BakedDrawerArray { get { return this.bakedDrawerChain; } }

        public DrawerChain BakedChain { get; private set; }

        public int CurrentIndex { get { return this.index; } }

        public override OdinDrawer Current
        {
            get
            {
                if (this.index >= 0 && this.index < this.bakedDrawerChain.Length)
                {
                    return this.bakedDrawerChain[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public override bool MoveNext()
        {
            do
            {
                this.index++;

                if (this.Current != null)
                {
                    this.Property.IncrementDrawerChainIndex();
                }
            } while (this.Current != null && this.Current.SkipWhenDrawing);

            return this.Current != null;
        }

        public override void Reset()
        {
            this.index = -1;
        }

        public void Rebake()
        {
            if (this.BakedChain != null)
            {
                this.BakedChain.Reset();
                this.bakedDrawerChain = this.BakedChain.ToArray();
            }
        }
    }

    public static partial class DrawerChainExtensions
    {
        public static BakedDrawerChain Bake(this DrawerChain chain)
        {
            if (chain == null) throw new ArgumentNullException("chain");

            var baked = chain as BakedDrawerChain;

            if (baked != null)
            {
                baked.Rebake();
                return baked;
            }
            else
            {
                return new BakedDrawerChain(chain);
            }
        }
    }
}
#endif