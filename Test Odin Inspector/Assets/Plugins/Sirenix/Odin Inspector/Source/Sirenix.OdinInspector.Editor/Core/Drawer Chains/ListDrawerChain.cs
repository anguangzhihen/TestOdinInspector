#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ListDrawerChain.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Collections.Generic;

    public class ListDrawerChain : DrawerChain
    {
        private int index = -1;
        private IList<OdinDrawer> list;

        public ListDrawerChain(InspectorProperty property, IList<OdinDrawer> list)
            : base(property)
        {
            this.list = list;
        }

        public override OdinDrawer Current
        {
            get
            {
                if (this.index >= 0 && this.index < this.list.Count)
                {
                    return this.list[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public override bool MoveNext()
        {
            this.index++;
            return this.Current != null;
        }

        public override void Reset()
        {
            this.index = -1;
        }
    }
}
#endif