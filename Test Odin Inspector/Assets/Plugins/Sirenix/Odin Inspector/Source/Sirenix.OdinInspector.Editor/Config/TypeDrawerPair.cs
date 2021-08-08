#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeDrawerPair.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using UnityEngine;

    /// <summary>
    /// <para>Contains information about an editor type which is assigned to draw a certain type in the inspector.</para>
    /// <para>This class uses the <see cref="InspectorTypeDrawingConfig.TypeBinder"/> instance to bind types to names, and names to types.</para>
    /// </summary>
    /// <seealso cref="InspectorTypeDrawingConfigDrawer"/>.
    /// <seealso cref="InspectorTypeDrawingConfig"/>.
    /// <seealso cref="EditorCompilation"/>.
    [Serializable]
    public struct TypeDrawerPair : IEquatable<TypeDrawerPair>
    {
        /// <summary>
        /// A default, empty <see cref="TypeDrawerPair"/> value.
        /// </summary>
        public static readonly TypeDrawerPair Default = default(TypeDrawerPair);

        /// <summary>
        /// The name of the type to be drawn.
        /// </summary>
        [SerializeField]
        public string DrawnTypeName;

        /// <summary>
        /// The name of the editor type.
        /// </summary>
        [SerializeField]
        public string EditorTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDrawerPair"/> struct.
        /// </summary>
        /// <param name="drawnType">The drawn type.</param>
        /// <exception cref="System.ArgumentNullException">drawnType is null</exception>
        public TypeDrawerPair(Type drawnType)
        {
            if (drawnType == null)
            {
                throw new ArgumentNullException("drawnType");
            }

            this.DrawnTypeName = InspectorTypeDrawingConfig.TypeBinder.BindToName(drawnType);
            this.EditorTypeName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDrawerPair"/> struct.
        /// </summary>
        /// <param name="drawnType">The drawn type.</param>
        /// <param name="editorType">The editor type.</param>
        /// <exception cref="System.ArgumentNullException">drawnType is null</exception>
        public TypeDrawerPair(Type drawnType, Type editorType)
            : this(drawnType)
        {
            if (editorType == null)
            {
                this.EditorTypeName = string.Empty;
            }
            else
            {
                this.EditorTypeName = InspectorTypeDrawingConfig.TypeBinder.BindToName(editorType);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="TypeDrawerPair" /> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="TypeDrawerPair" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="TypeDrawerPair" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(TypeDrawerPair other)
        {
            return other.EditorTypeName == this.EditorTypeName
                && other.DrawnTypeName == this.DrawnTypeName;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.EditorTypeName != null ? this.EditorTypeName.GetHashCode() * 7 : 0)
                     ^ (this.DrawnTypeName != null ? this.DrawnTypeName.GetHashCode() * 13 : 0);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null) || obj.GetType() != typeof(TypeDrawerPair))
            {
                return false;
            }

            return this.Equals((TypeDrawerPair)obj);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(TypeDrawerPair x, TypeDrawerPair y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(TypeDrawerPair x, TypeDrawerPair y)
        {
            return !x.Equals(y);
        }
    }
}
#endif