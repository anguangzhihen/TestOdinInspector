#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="WrapAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Draws short properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeInt16Drawer : OdinAttributeDrawer<WrapAttribute, short>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(UnityEngine.GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = (short)MathUtilities.Wrap(this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
        }
    }

    /// <summary>
    /// Draws int properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeInt32Drawer : OdinAttributeDrawer<WrapAttribute, int>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(UnityEngine.GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = (int)MathUtilities.Wrap(this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeInt64Drawer : OdinAttributeDrawer<WrapAttribute, long>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(UnityEngine.GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = (long)MathUtilities.Wrap(this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeFloatDrawer : OdinAttributeDrawer<WrapAttribute, float>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = (float)MathUtilities.Wrap(this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeDoubleDrawer : OdinAttributeDrawer<WrapAttribute, double>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = (double)MathUtilities.Wrap(this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeDecimalDrawer : OdinAttributeDrawer<WrapAttribute, decimal>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = (decimal)MathUtilities.Wrap((double)this.ValueEntry.SmartValue, this.Attribute.Min, this.Attribute.Max);
        }
    }

    /// <summary>
    /// Draws Vector2 properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeVector2Drawer : OdinAttributeDrawer<WrapAttribute, Vector2>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = new Vector2(
                MathUtilities.Wrap(this.ValueEntry.SmartValue.x, (float)this.Attribute.Min, (float)this.Attribute.Max),
                MathUtilities.Wrap(this.ValueEntry.SmartValue.y, (float)this.Attribute.Min, (float)this.Attribute.Max));
        }
    }

    /// <summary>
    /// Draws Vector3 properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeVector3Drawer : OdinAttributeDrawer<WrapAttribute, Vector3>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = new Vector3(
                MathUtilities.Wrap(this.ValueEntry.SmartValue.x, (float)this.Attribute.Min, (float)this.Attribute.Max),
                MathUtilities.Wrap(this.ValueEntry.SmartValue.y, (float)this.Attribute.Min, (float)this.Attribute.Max),
                MathUtilities.Wrap(this.ValueEntry.SmartValue.z, (float)this.Attribute.Min, (float)this.Attribute.Max));
        }
    }

    /// <summary>
    /// Draws Vector4 properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeVector4Drawer : OdinAttributeDrawer<WrapAttribute, Vector4>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.CallNextDrawer(label);
            this.ValueEntry.SmartValue = new Vector4(
                MathUtilities.Wrap(this.ValueEntry.SmartValue.x, (float)this.Attribute.Min, (float)this.Attribute.Max),
                MathUtilities.Wrap(this.ValueEntry.SmartValue.y, (float)this.Attribute.Min, (float)this.Attribute.Max),
                MathUtilities.Wrap(this.ValueEntry.SmartValue.z, (float)this.Attribute.Min, (float)this.Attribute.Max),
                MathUtilities.Wrap(this.ValueEntry.SmartValue.w, (float)this.Attribute.Min, (float)this.Attribute.Max));
        }
    }
}
#endif