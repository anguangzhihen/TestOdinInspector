#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;

    public abstract class OdinPropertyResolver
    {
        private bool hasUpdatedChildCountEver = false;
        private int lastUpdatedTreeID = -1;
        private int childCount;

        private static readonly Dictionary<Type, Func<OdinPropertyResolver>> Resolver_EmittedCreator_Cache = new Dictionary<Type, Func<OdinPropertyResolver>>(FastTypeComparer.Instance);

        public bool HasChildCountConflict { get; protected set; }
        public int MaxChildCountSeen { get; protected set; }

        public static OdinPropertyResolver Create(Type resolverType, InspectorProperty property)
        {
            if (resolverType == null)
            {
                throw new ArgumentNullException("resolverType");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (!typeof(OdinPropertyResolver).IsAssignableFrom(resolverType))
            {
                throw new ArgumentException("Type is not a PropertyResolver");
            }

            //var result = (OdinPropertyResolver)Activator.CreateInstance(resolverType);

            Func<OdinPropertyResolver> creator;

            if (!Resolver_EmittedCreator_Cache.TryGetValue(resolverType, out creator))
            {
                var builder = new DynamicMethod("OdinPropertyResolver_EmittedCreator_" + Guid.NewGuid(), typeof(OdinPropertyResolver), Type.EmptyTypes);
                var il = builder.GetILGenerator();

                il.Emit(OpCodes.Newobj, resolverType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);

                creator = (Func<OdinPropertyResolver>)builder.CreateDelegate(typeof(Func<OdinPropertyResolver>));
                Resolver_EmittedCreator_Cache.Add(resolverType, creator);
            }

            var result = creator();
            result.Property = property;
            result.Initialize();
            return result;
        }

        public static T Create<T>(InspectorProperty property) where T : OdinPropertyResolver, new()
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            var result = new T();
            result.Property = property;
            result.Initialize();
            return result;
        }

        protected virtual void Initialize()
        {
        }

        public virtual Type ResolverForType { get { return null; } }

        public InspectorProperty Property { get; private set; }

        public int ChildCount
        {
            get
            {
                var treeId = this.Property.Tree.UpdateID;

                if (this.lastUpdatedTreeID != treeId || !this.hasUpdatedChildCountEver)
                {
                    this.lastUpdatedTreeID = treeId;
                    this.childCount = this.CalculateChildCount();
                    this.hasUpdatedChildCountEver = true;
                }

                return this.childCount;
            }
        }

        public abstract InspectorPropertyInfo GetChildInfo(int childIndex);

        public abstract int ChildNameToIndex(string name);

        protected abstract int CalculateChildCount();

        public virtual bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            return true;
        }

        public void ForceUpdateChildCount()
        {
            if (this.hasUpdatedChildCountEver) // If we've never updated the child count yet, there's no reason to actually do this, as the latest value will be given by ChildCount anyways
            {
                this.lastUpdatedTreeID = this.Property.Tree.UpdateID;
                this.childCount = this.CalculateChildCount();
            }
        }
    }

    public abstract class OdinPropertyResolver<TValue> : OdinPropertyResolver
    {
        public sealed override Type ResolverForType { get { return typeof(TValue); } }

        public IPropertyValueEntry<TValue> ValueEntry { get { return (IPropertyValueEntry<TValue>)this.Property.ValueEntry; } }

        protected virtual bool AllowNullValues { get { return false; } }

        protected sealed override int CalculateChildCount()
        {
            var valueEntry = (IPropertyValueEntry<TValue>)this.Property.ValueEntry;

            this.HasChildCountConflict = false;
            int count = int.MaxValue;
            this.MaxChildCountSeen = int.MinValue;

            for (int i = 0; i < valueEntry.ValueCount; i++)
            {
                var value = valueEntry.Values[i];
                
                int indexCount;

                if (this.AllowNullValues)
                {
                    indexCount = this.GetChildCount(value);
                }
                else
                {
                    indexCount = value != null ? this.GetChildCount(value) : 0;
                }

                if (count != int.MaxValue && count != indexCount)
                {
                    this.HasChildCountConflict = true;
                }

                if (indexCount < count)
                {
                    count = indexCount;
                }

                if (indexCount > this.MaxChildCountSeen)
                {
                    this.MaxChildCountSeen = indexCount;
                }
            }

            return count;
        }

        protected abstract int GetChildCount(TValue value);
    }

    public abstract class OdinPropertyResolver<TValue, TAttribute> : OdinPropertyResolver<TValue> where TAttribute : Attribute
    {
    }
}
#endif