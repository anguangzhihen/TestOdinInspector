#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerPriority.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Text;

    /// <summary>
    /// <para>
    /// DrawerPriority is used in conjunction with <see cref="DrawerPriorityAttribute"/>
    /// to specify the priority of any given drawer. It consists of 3 components:
    /// Super, Wrapper, Value, where Super is the most significant component,
    /// and Standard is the least significant component.
    /// </para>
    /// </summary>
    /// <seealso cref="DrawerPriorityLevel"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    public struct DrawerPriority : IEquatable<DrawerPriority>, IComparable<DrawerPriority>
    {
        /// <summary>
        /// Auto priority is defined by setting all of the components to zero.
        /// If no <see cref="DrawerPriorityAttribute"/> is defined on a drawer, it will default to AutoPriority.
        /// </summary>
        public static readonly DrawerPriority AutoPriority = new DrawerPriority(0, 0, 0);

        /// <summary>
        /// The standard priority. Mostly used by <see cref="OdinValueDrawer{T}"/>s.
        /// </summary>
        public static readonly DrawerPriority ValuePriority = new DrawerPriority(0, 0, 1);

        /// <summary>
        /// The attribute priority. Mostly used by <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>s.
        /// </summary>
        public static readonly DrawerPriority AttributePriority = new DrawerPriority(0, 0, 1000);

        /// <summary>
        /// The wrapper priority. Mostly used by drawers used to decorate properties.
        /// </summary>
        public static readonly DrawerPriority WrapperPriority = new DrawerPriority(0, 1, 0);

        /// <summary>
        /// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
        /// These drawers typically don't draw the property itself, and calls CallNextDrawer.
        /// </summary>
        public static readonly DrawerPriority SuperPriority = new DrawerPriority(1, 0, 0);

        /// <summary>
        /// The value priority. Mostly used by <see cref="OdinValueDrawer{T}"/>s and <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>s.
        /// </summary>
        public double Value;

        /// <summary>
        /// The wrapper priority. Mostly used by drawers used to decorate properties.
        /// </summary>
        public double Wrapper;

        /// <summary>
        /// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
        /// These drawers typically don't draw the property itself, and calls CallNextDrawer.
        /// </summary>
        public double Super;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawerPriority"/> struct.
        /// </summary>
        /// <param name="priority">The priority.</param>
        public DrawerPriority(DrawerPriorityLevel priority)
        {
            DrawerPriority set;

            switch (priority)
            {
                case DrawerPriorityLevel.AutoPriority:
                    set = AutoPriority;
                    break;

                case DrawerPriorityLevel.ValuePriority:
                    set = ValuePriority;
                    break;

                case DrawerPriorityLevel.AttributePriority:
                    set = AttributePriority;
                    break;

                case DrawerPriorityLevel.WrapperPriority:
                    set = WrapperPriority;
                    break;

                case DrawerPriorityLevel.SuperPriority:
                    set = SuperPriority;
                    break;

                default:
                    throw new NotImplementedException(priority.ToString());
            }

            this.Value = set.Value;
            this.Wrapper = set.Wrapper;
            this.Super = set.Super;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawerPriority"/> struct.
        /// </summary>
        /// <param name="super">
        /// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
        /// These drawers typically don't draw the property itself, and calls CallNextDrawer.</param>
        /// <param name="wrapper">The wrapper priority. Mostly used by drawers used to decorate properties.</param>
        /// <param name="value">The value priority. Mostly used by <see cref="OdinValueDrawer{T}"/>s and <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>s.</param>
        public DrawerPriority(double super = 0, double wrapper = 0, double value = 0)
        {
            this.Super = super;
            this.Wrapper = wrapper;
            this.Value = value;
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(DrawerPriority lhs, DrawerPriority rhs)
        {
            if (lhs == rhs) return false;

            if (lhs.Super > rhs.Super) return true;
            else if (lhs.Super != rhs.Super) return false;

            if (lhs.Wrapper > rhs.Wrapper) return true;
            else if (lhs.Wrapper != rhs.Wrapper) return false;

            if (lhs.Value > rhs.Value) return true;
            else return false;
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(DrawerPriority lhs, DrawerPriority rhs)
        {
            if (lhs == rhs) return false;

            if (lhs.Super < rhs.Super) return true;
            else if (lhs.Super != rhs.Super) return false;

            if (lhs.Wrapper < rhs.Wrapper) return true;
            else if (lhs.Wrapper != rhs.Wrapper) return false;

            if (lhs.Value < rhs.Value) return true;
            else return false;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <=(DrawerPriority lhs, DrawerPriority rhs)
        {
            return lhs < rhs || lhs == rhs;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >=(DrawerPriority lhs, DrawerPriority rhs)
        {
            return lhs > rhs || lhs == rhs;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static DrawerPriority operator +(DrawerPriority lhs, DrawerPriority rhs)
        {
            lhs.Super += rhs.Super;
            lhs.Wrapper += rhs.Wrapper;
            lhs.Value += rhs.Value;
            return lhs;
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static DrawerPriority operator -(DrawerPriority lhs, DrawerPriority rhs)
        {
            lhs.Super -= rhs.Super;
            lhs.Wrapper -= rhs.Wrapper;
            lhs.Value -= rhs.Value;
            return lhs;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(DrawerPriority lhs, DrawerPriority rhs)
        {
            return lhs.Super == rhs.Super &&
                   lhs.Wrapper == rhs.Wrapper &&
                   lhs.Value == rhs.Value;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(DrawerPriority lhs, DrawerPriority rhs)
        {
            return lhs.Super != rhs.Super ||
                   lhs.Wrapper != rhs.Wrapper ||
                   lhs.Value != rhs.Value;
        }

        /// <summary>
        /// Gets the priority level.
        /// </summary>
        public DrawerPriorityLevel GetPriorityLevel()
        {
            if (this.Super > 0)
            {
                return DrawerPriorityLevel.SuperPriority;
            }
            else if (this.Wrapper > 0)
            {
                return DrawerPriorityLevel.WrapperPriority;
            }
            else if (this.Value >= AttributePriority.Value)
            {
                return DrawerPriorityLevel.AttributePriority;
            }
            else if (this.Value > 0)
            {
                return DrawerPriorityLevel.ValuePriority;
            }
            else
            {
                return DrawerPriorityLevel.AutoPriority;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return new StringBuilder(this.GetPriorityLevel().ToString())
                .Append(" (")
                .Append(this.Super)
                .Append(", ")
                .Append(this.Wrapper)
                .Append(", ")
                .Append(this.Value)
                .Append(')')
                .ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return new StringBuilder(this.GetPriorityLevel().ToString())
                .Append(" (")
                .Append(this.Super.ToString(format))
                .Append(", ")
                .Append(this.Wrapper.ToString(format))
                .Append(", ")
                .Append(this.Value.ToString(format))
                .Append(')')
                .ToString();
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
            if (obj is DrawerPriority)
            {
                DrawerPriority b = (DrawerPriority)obj;
                return this == b;
            }
            return false;
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
                int hash = 1417;
                hash = hash * 219 + this.Super.GetHashCode();
                hash = hash * 219 + this.Wrapper.GetHashCode();
                hash = hash * 219 + this.Value.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(DrawerPriority other)
        {
            return this == other;
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(DrawerPriority other)
        {
            if (this > other)
            {
                return 1;
            }
            else if (this < other)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
#endif