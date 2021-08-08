#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NodeType.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    internal enum NodeType
    {
        TERNARY_CONDITIONAL,
        NULL_COALESCE,
        LOGICAL_OR,
        LOGICAL_AND,
        BITWISE_INCLUSIVE_OR,
        BITWISE_EXCLUSIVE_OR,
        BITWISE_AND,
        EQUALS,
        NOT_EQUALS,
        LESS_THAN,
        GREATER_THAN,
        GREATER_THAN_OR_EQUAL,
        LESS_THAN_OR_EQUAL,
        RELATIONAL_IS,
        RELATIONAL_AS,
        LEFT_SHIFT,
        RIGHT_SHIFT,
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        REMAINDER,
        //UNARY_PLUS, // This strictly speaking doesn't need to exist, as I think it does nothing at all (+10 = 10)
        UNARY_MINUS,
        UNARY_NOT,
        UNARY_COMPLEMENT,
        PRE_INCREMENT,
        PRE_DECREMENT,
        POST_INCREMENT,
        POST_DECREMENT,
        CONSTANT_SIGNED_INT32,
        CONSTANT_UNSIGNED_INT32,
        CONSTANT_SIGNED_INT64,
        CONSTANT_UNSIGNED_INT64,
        CONSTANT_FLOAT32,
        CONSTANT_FLOAT64,
        CONSTANT_DECIMAL,
        CONSTANT_STRING,
        CONSTANT_CHAR,
        CONSTANT_BOOLEAN,
        CONSTANT_NULL,
        MEMBER_ACCESS,
        MEMBER_ACCESS_NULL_CONDITIONAL,
        MEMBER_ACCESS_POINTER_DEREFERENCE,
        ELEMENT_ACCESS_NULL_CONDITIONAL,
        ELEMENT_ACCESS,
        THIS_ACCESS,
        BASE_ACCESS,
        TYPEOF,
        TYPEOF_VOID,
        SIZE_OF,
        DEFAULT_TYPED,
        DEFAULT_INFERRED,
        INVOCATION,
        IDENTIFIER,
        NUMBERED_EXPRESSION_ARGUMENT,
        NAMED_EXPRESSION_ARGUMENT,
        INSTANTIATE_TYPE,
        CHECKED,
        UNCHECKED,
        TYPE_CAST,
        ADDRESS_OF,
        DEREFERENCE_POINTER,
        PARENTHESIZED_EXPRESSION,
        ARRAY_OF,
        PROPERTY_QUERY,
        SIMPLE_ASSIGNMENT,
    }
}
#endif