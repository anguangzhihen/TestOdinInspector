#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StateUpdater.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using UnityEngine;

    public abstract class StateUpdater
    {
        private InspectorProperty property;
        private bool initialized;

        public string ErrorMessage;

        public InspectorProperty Property { get { return this.property; } }

        public virtual bool CanUpdateProperty(InspectorProperty property) { return true; }

        public virtual void OnStateUpdate() { }

        public void Initialize(InspectorProperty property)
        {
            if (this.initialized) return;

            this.property = property;

            try
            {
                this.Initialize();
            }
            finally
            {
                this.initialized = true;
            }
        }

        protected virtual void Initialize() { }
    }

    public abstract class AttributeStateUpdater<TAttribute> : StateUpdater where TAttribute : Attribute
    {

        private TAttribute attribute;

        /// <summary>
        /// Gets the attribute that the OdinAttributeStateUpdater applies to.
        /// </summary>
        public TAttribute Attribute
        {
            get
            {
                if (this.attribute == null)
                {
                    var updaters = this.Property.StateUpdaters;

                    int count = 0;

                    for (int i = 0; i < updaters.Length; i++)
                    {
                        var updater = updaters[i];

                        if (updater.GetType() == this.GetType())
                        {
                            if (object.ReferenceEquals(this, updater)) break;
                            else count++;
                        }
                    }

                    int savedCount = count;
                    var type = typeof(TAttribute);

                    for (int i = 0; i < this.Property.Attributes.Count; i++)
                    {
                        var attr = this.Property.Attributes[i];

                        if (attr.GetType() != type) continue;

                        if (count == 0)
                        {
                            this.attribute = (TAttribute)attr;
                            break;
                        }
                        else count--;
                    }

                    if (this.attribute == null)
                    {
                        Debug.LogError("Could not find attribute '" + typeof(TAttribute).GetNiceName() + "' number " + savedCount + " for the state updater '" + this.GetType().GetNiceName() + "' number " + savedCount + "; not enough attributes of the required type on the property - why are there more drawers for the attribute than there are attributes?");
                        this.attribute = this.Property.GetAttribute<TAttribute>();
                    }
                }

                return this.attribute;
            }
        }

    }
    public abstract class AttributeStateUpdater<TAttribute, TValue> : AttributeStateUpdater<TAttribute> where TAttribute : Attribute
    {
        private IPropertyValueEntry<TValue> valueEntry;

        /// <summary>
        /// Gets the strongly typed ValueEntry of the OdinAttributeStateUpdater's property.
        /// </summary>
        public IPropertyValueEntry<TValue> ValueEntry
        {
            get
            {
                if (this.valueEntry == null)
                {
                    this.valueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;

                    if (this.valueEntry == null)
                    {
                        this.Property.Update(true);
                        this.valueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;
                    }
                }

                return this.valueEntry;
            }
        }
    }
    public abstract class ValueStateUpdater<TValue> : StateUpdater
    {
        private IPropertyValueEntry<TValue> valueEntry;

        /// <summary>
        /// Gets the strongly typed ValueEntry of the OdinValueStateUpdater's property.
        /// </summary>
        public IPropertyValueEntry<TValue> ValueEntry
        {
            get
            {
                if (this.valueEntry == null)
                {
                    this.valueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;

                    if (this.valueEntry == null)
                    {
                        this.Property.Update(true);
                        this.valueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;
                    }
                }

                return this.valueEntry;
            }
        }
    }
}
#endif