//-----------------------------------------------------------------------
// <copyright file="Operator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
#pragma warning disable

    /// <summary>
    /// Determines the type of operator.
    /// </summary>
    /// <seealso cref="TypeExtensions" />
    public enum Operator
    {
        /// <summary>
        /// The == operator.
        /// </summary>
        Equality,

        /// <summary>
        /// The != operator.
        /// </summary>
        Inequality,

        /// <summary>
        /// The + operator.
        /// </summary>
        Addition,

        /// <summary>
        /// The - operator.
        /// </summary>
        Subtraction,

        /// <summary>
        /// The * operator.
        /// </summary>
        Multiply,

        /// <summary>
        /// The / operator.
        /// </summary>
        Division,

        /// <summary>
        /// The &lt; operator.
        /// </summary>
        LessThan,

        /// <summary>
        /// The &gt; operator.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// The &lt;= operator.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// The &gt;= operator.
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// The % operator.
        /// </summary>
        Modulus,

        /// <summary>
        /// The &gt;&gt; operator.
        /// </summary>
        RightShift,

        /// <summary>
        /// The &lt;&lt; operator.
        /// </summary>
        LeftShift,

        /// <summary>
        /// The &amp; operator.
        /// </summary>
        BitwiseAnd,

        /// <summary>
        /// The | operator.
        /// </summary>
        BitwiseOr,

        /// <summary>
        /// The ^ operator.
        /// </summary>
        ExclusiveOr,

        /// <summary>
        /// The ~ operator.
        /// </summary>
        BitwiseComplement,

        /// <summary>
        /// The &amp;&amp; operator.
        /// </summary>
        LogicalAnd,

        /// <summary>
        /// The || operator.
        /// </summary>
        LogicalOr,

        /// <summary>
        /// The ! operator.
        /// </summary>
        LogicalNot,

    }
}