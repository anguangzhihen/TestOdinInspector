#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MethodDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Base drawer to inherit from to draw methods.
    /// </summary>
    public abstract class MethodDrawer : OdinDrawer
    {
        public sealed override bool CanDrawProperty(InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Method && this.CanDrawMethodProperty(property);
        }

        protected virtual bool CanDrawMethodProperty(InspectorProperty property)
        {
            return true;
        }
    }
}
#endif