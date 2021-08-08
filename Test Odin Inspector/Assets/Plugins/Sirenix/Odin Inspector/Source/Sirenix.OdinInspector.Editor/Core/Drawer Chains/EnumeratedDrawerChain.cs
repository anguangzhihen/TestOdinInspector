#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnumeratedDrawerChain.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System.Collections.Generic;

    public abstract class EnumeratedDrawerChain : DrawerChain
    {
        private IEnumerator<OdinDrawer> enumerator;

        public EnumeratedDrawerChain(InspectorProperty property) : base(property)
        {
        }

        public override OdinDrawer Current
        {
            get
            {
                if (this.enumerator == null) return null;
                return this.enumerator.Current;
            }
        }

        public override bool MoveNext()
        {
            if (this.enumerator == null)
            {
                this.enumerator = this.GetEnumeratorInstance();
            }

            return this.enumerator.MoveNext();
        }

        public override void Reset()
        {
            if (this.enumerator != null)
            {
                this.enumerator.Dispose();
                this.enumerator = null;
            }
        }

        protected abstract IEnumerator<OdinDrawer> GetEnumeratorInstance();
    }
}
#endif