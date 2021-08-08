#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UniqueDrawerKey.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// Gets a unique key for any given property within a drawer.
    /// </summary>
    /// <example>
    /// <code>
    ///
    /// public class MyCustomTypeDrawer&lt;T&gt; : OdinValueDrawer&lt;T&gt; where T : MyCustomBaseType
    /// {
    ///     protected override void DrawPropertyLayout(IPropertyValueEntry&lt;T&gt; entry, GUIContent label)
    ///     {
    ///         var isToggled = entry.Context(this, "toggled", false);
    ///         isToggled.Value = SirenixEditorGUI.Label(isToggled.Value, label);
    ///         if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry, this), isToggled.Value))
    ///         {
    ///             EditorGUI.indentLevel++;
    ///             this.CallNextDrawer(entry.Property, null);
    ///             EditorGUI.indentLevel--;
    ///         }
    ///         SirenixEditorGUI.EndFadeGroup();
    ///     }
    /// }
    /// </code>
    /// </example>
    public struct UniqueDrawerKey : IEquatable<UniqueDrawerKey>
    {
        private readonly InspectorProperty property;

        private readonly int drawCount;

        private readonly OdinDrawer drawer;

        private readonly int hashCode;

        private UniqueDrawerKey(InspectorProperty property, OdinDrawer drawer)
        {
            this.property = property;
            this.drawCount = property.DrawCount;
            this.drawer = drawer;

            unchecked
            {
                int hash = 17;
                hash = hash * 29 + this.property.GetHashCode();
                hash = hash * 29 + this.drawCount;
                hash = hash * 29 + this.drawer.GetHashCode();
                this.hashCode = hash;
            }
        }

        /// <summary>
        /// Gets a unique key for any given property within a drawer.
        /// </summary>
        /// <param name="entry">The property entry.</param>
        /// <param name="drawer">The drawer.</param>
        /// <returns></returns>
        public static UniqueDrawerKey Create(IPropertyValueEntry entry, OdinDrawer drawer)
        {
            return new UniqueDrawerKey(entry.Property, drawer);
        }

        /// <summary>
        /// Gets a unique key for any given property within a drawer.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="drawer">The drawer.</param>
        /// <returns></returns>
        public static UniqueDrawerKey Create(InspectorProperty property, OdinDrawer drawer)
        {
            return new UniqueDrawerKey(property, drawer);
        }

        /// <summary>
        /// Checks if two keys are identical.
        /// </summary>
        /// <param name="other">The other key.</param>
        public bool Equals(UniqueDrawerKey other)
        {
            return
                this.property == other.property &&
                this.drawCount == other.drawCount &&
                this.drawer == other.drawer;
        }

        /// <summary>
        /// Checks if two keys are identical.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj.GetType() != typeof(UniqueDrawerKey))
            {
                return false;
            }

            return this.Equals((UniqueDrawerKey)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return this.hashCode;
        }
    }
}
#endif