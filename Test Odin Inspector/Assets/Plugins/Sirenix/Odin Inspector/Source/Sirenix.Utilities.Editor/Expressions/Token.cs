#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Token.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    public enum Token
    {
        UNKNOWN,                            // Unknown token
        SIGNED_INT32,                       // -123, 123
        UNSIGNED_INT32,                     // 123
        SIGNED_INT64,                       // -123, 123
        UNSIGNED_INT64,                     // 123
        FLOAT32,                            // 123.456F
        FLOAT64,                            // 123.456
        DECIMAL,                            // 123.456M
        IDENTIFIER,                         // xyz
        SIZEOF,                             // sizeof
        CHAR_CONSTANT,                      // 'c'
        STRING_CONSTANT,                    // "abc"
        LOGICAL_OR,                         // ||
        LOGICAL_AND,                        // &&
        EOF,                                // End of File or stream
        PLUS,                               // +
        MINUS,                              // -
        MULTIPLY,                           // *
        DIVIDE,                             // /
        REMAINDER,                          // %
        NOT,                                // !, not
        COMPLEMENT,                         // ~
        LEFT_PARENTHESIS,                   // (
        RIGHT_PARENTHESIS,                  // )
        LEFT_BRACKET,                       // [
        RIGHT_BRACKET,                      // ]
        POINT,                              // .
        COMMA,                              // ,
        QUESTION_MARK,                      // ?
        COLON,                              // :
        DOUBLE_COLON,                       // ::
        SEMI_COLON,                         // ;
        BITWISE_INCLUSIVE_OR,               // |
        BITWISE_EXCLUSIVE_OR,               // ^
        BITWISE_AND,                        // &
        EQUALS,                             // ==
        NOT_EQUALS,                         // !=
        GREATER_THAN,                       // >
        GREATER_THAN_OR_EQUAL,              // >=
        LESS_THAN,                          // <
        LESS_THAN_OR_EQUAL,                 // <=
        LEFT_SHIFT,                         // <<
        //RIGHT_SHIFT,                      // >> ( Instead look for two GREATER_THAN in a row - this is so nested generics like List<List<int>> can work. )
        SIMPLE_ASSIGNMENT,                  // =
        ADDITION_ASSIGNMENT,                // +=
        SUBTRACTION_ASSIGNMENT,             // -=
        MULTIPLICATION_ASSIGNMENT,          // *=
        DIVISION_ASSIGNMENT,                // /=
        REMAINDER_ASSIGNMENT,               // %=
        LEFT_SHIFT_ASSIGNMENT,              // <<=
        RIGHT_SHIFT_ASSIGNMENT,             // >>=
        BITWISE_AND_ASSIGNMENT,             // &=
        BITWISE_OR_ASSIGNMENT,              // |=
        BITWISE_XOR_ASSIGNMENT,             // ^=
        TRUE,                               // true
        FALSE,                              // false
        NUMBERED_EXPRESSION_ARGUMENT,       // $0, $1, etc
        UNNUMBERED_EXPRESSION_ARGUMENT,     // $
        NAMED_EXPRESSION_ARGUMENT,          // $name
        NULL_COALESCE,                      // ??
        RELATIONAL_IS,                      // is
        RELATIONAL_AS,                      // as
        INCREMENT,                          // ++
        DECREMENT,                          // --
        MEMBER_ACCESS_POINTER_DEREFERENCE,  // ->
        ELEMENT_ACCESS_NULL_CONDITIONAL,    // ?[
        MEMBER_ACCESS_NULL_CONDITIONAL,     // ?.
        NEW,                                // new
        THIS,                               // this
        BASE,                               // base
        CHECKED,                            // checked
        UNCHECKED,                          // unchecked
        DEFAULT,                            // default
        DELEGATE,                           // delegate
        NULL,                               // null
        TYPEOF,                             // typeof
        VOID,                               // void
        REF,                                // ref
        OUT,                                // out
        IN,                                 // in
        COMMENT,                            // Either "// comment" or "/* comment */"
        SCOPE_BEGIN,                        // {
        SCOPE_END,                          // }
        CLASS,                              // class
        STRUCT,                             // struct
        INTERFACE,                          // interface
        PREPROCESSOR,                       // #if, #define, #region etc
        RETURN,                             // return
        PROPERTY_QUERY,                     // #(propertyName)
#if SIRENIX_INTERNAL
        AT_SIGN,                            // @state, or @state = value
#endif
    }

    internal static class TokenExtensions
    {
        // TODO: What is this even for?? This is not the default behaviour?
        public static string ToTokenString(this Token token)
        {
            return token.ToString();
        }
    }
}
#endif