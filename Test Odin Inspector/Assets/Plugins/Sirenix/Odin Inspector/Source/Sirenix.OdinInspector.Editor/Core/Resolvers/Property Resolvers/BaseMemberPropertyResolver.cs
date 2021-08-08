#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BaseMemberPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    public abstract class BaseMemberPropertyResolver<TValue> : OdinPropertyResolver<TValue>, IMaySupportPrefabModifications
    {
        private InspectorPropertyInfo[] infos;
        private Dictionary<string, int> namesToIndex;

        public virtual bool MaySupportPrefabModifications { get { return true; } }

        public sealed override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            if (object.ReferenceEquals(this.infos, null))
            {
                this.LazyInitialize();
            }

            return this.infos[childIndex];
        }

        public sealed override int ChildNameToIndex(string name)
        {
            if (object.ReferenceEquals(this.infos, null))
            {
                this.LazyInitialize();
            }

            int result;
            if (this.namesToIndex.TryGetValue(name, out result)) return result;
            return -1;
        }

        protected sealed override int GetChildCount(TValue value)
        {
            if (object.ReferenceEquals(this.infos, null))
            {
                this.LazyInitialize();
            }

            return this.infos.Length;
        }

        protected abstract InspectorPropertyInfo[] GetPropertyInfos();

        private bool initializing;

        private void LazyInitialize()
        {
            if (this.initializing)
                throw new Exception("Illegal API call was made: cannot query members of a property that are dependent on children being initialized, during the initialization of the property's children.");

            this.initializing = true;

            try
            {
                this.infos = this.GetPropertyInfos();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.initializing = false;
            }

            this.namesToIndex = new Dictionary<string, int>();

            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                this.namesToIndex[info.PropertyName] = i;
            }
        }
    }

    public abstract class BaseMemberPropertyResolver<TValue, TAttribute> : OdinPropertyResolver<TValue, TAttribute>, IMaySupportPrefabModifications
        where TAttribute : Attribute
    {
        private InspectorPropertyInfo[] infos;
        private Dictionary<string, int> namesToIndex;
        private bool initializing;

        public virtual bool MaySupportPrefabModifications { get { return true; } }

        protected override void Initialize()
        {
            if (this.initializing)
                throw new Exception("Illegal API call was made: cannot query members of a property that are dependent on children being initialized, during the initialization of the property's children.");

            this.initializing = true;
            this.infos = this.GetPropertyInfos();
            this.initializing = false;

            this.namesToIndex = new Dictionary<string, int>();

            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                this.namesToIndex[info.PropertyName] = i;
            }
        }

        public sealed override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            return this.infos[childIndex];
        }

        public sealed override int ChildNameToIndex(string name)
        {
            int result;
            if (this.namesToIndex.TryGetValue(name, out result)) return result;
            return -1;
        }

        protected sealed override int GetChildCount(TValue value)
        {
            return this.infos.Length;
        }

        protected abstract InspectorPropertyInfo[] GetPropertyInfos();
    }
}
#endif