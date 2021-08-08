#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Tokenizer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    using System;
    using System.Globalization;
    using System.Text;

    public struct TokenizerState
    {
        public Token NextToken;
        public int TokenStringPosition;
        public int TokenStartedStringPosition;
        public string IdentifierValue;
        public int ExpressionArgumentNumber;
        public string TokenString;
        public char CharacterConstantValue;
        public float Float32ConstantValue;
        public double Float64ConstantValue;
        public long IntegerConstantValue;
        public ulong UnsignedIntegerConstantValue;
        public decimal DecimalConstantValue;
        public string StringConstantValue;
    }

    public class Tokenizer
    {
        private static readonly char NoMoreCharacters = (char)0xFFFF;

        private StringBuilder stringBuilder = new StringBuilder();

        private int expressionStringPosition;
        private string expressionString;

        public string ExpressionString { get { return this.expressionString; } }
        public int ExpressionStringPosition { get { return this.expressionStringPosition; } }

        public int TokenStartedStringPosition { get; private set; }

        public string IdentifierValue { get; private set; }
        public int ExpressionArgumentNumber { get; private set; }

        public char CharacterConstantValue { get; private set; }
        public float Float32ConstantValue { get; private set; }
        public double Float64ConstantValue { get; private set; }
        public long SignedIntegerConstantValue { get; private set; }
        public ulong UnsignedIntegerConstantValue { get; private set; }
        public decimal DecimalConstantValue { get; private set; }
        public string StringConstantValue { get; private set; }

        public bool TokenizeComments = false;
        public bool TokenizePreprocessors = true;

        public Tokenizer()
        {
        }

        public Tokenizer(string expressingString)
        {
            this.SetExpressionString(expressingString);
        }

        public TokenizerState GetState()
        {
            return new TokenizerState()
            {
                TokenString = this.ExpressionString,
                TokenStringPosition = this.ExpressionStringPosition,
                TokenStartedStringPosition = this.TokenStartedStringPosition,
                IdentifierValue = this.IdentifierValue,
                ExpressionArgumentNumber = this.ExpressionArgumentNumber,
                CharacterConstantValue = this.CharacterConstantValue,
                Float32ConstantValue = this.Float32ConstantValue,
                Float64ConstantValue = this.Float64ConstantValue,
                IntegerConstantValue = this.SignedIntegerConstantValue,
                UnsignedIntegerConstantValue = this.UnsignedIntegerConstantValue,
                DecimalConstantValue = this.DecimalConstantValue,
                StringConstantValue = this.StringConstantValue,
            };
        }

        public void SetState(TokenizerState state)
        {
            this.expressionStringPosition = state.TokenStringPosition;
            this.TokenStartedStringPosition = state.TokenStartedStringPosition;
            this.IdentifierValue = state.IdentifierValue;
            this.ExpressionArgumentNumber = state.ExpressionArgumentNumber;
            this.expressionString = state.TokenString;
            this.CharacterConstantValue = state.CharacterConstantValue;
            this.Float32ConstantValue = state.Float32ConstantValue;
            this.Float64ConstantValue = state.Float64ConstantValue;
            this.SignedIntegerConstantValue = state.IntegerConstantValue;
            this.UnsignedIntegerConstantValue = state.UnsignedIntegerConstantValue;
            this.DecimalConstantValue = state.DecimalConstantValue;
            this.StringConstantValue = state.StringConstantValue;
        }

        public void SetExpressionString(string expressionString)
        {
            this.expressionString = expressionString;
            this.expressionStringPosition = 0;
            this.stringBuilder.Length = 0;
        }

        public Token GetNextToken()
        {
            EatWhiteSpace();

            var tempPos = this.expressionStringPosition;

            char nextCharacter = EatNextCharacter();

            if (nextCharacter == NoMoreCharacters)
            {
                return Token.EOF;
            }

            this.TokenStartedStringPosition = tempPos;

            switch (nextCharacter)
            {
                case '+':
                    if (PeekNextCharacter() == '=')
                    {
                        EatNextCharacter();
                        return Token.ADDITION_ASSIGNMENT;
                    }
                    else if (PeekNextCharacter() == '+')
                    {
                        EatNextCharacter();
                        return Token.INCREMENT;
                    }
                    else if (char.IsDigit(PeekNextCharacter()))
                    {
                        return ParseNumber(nextCharacter);
                    }
                    return Token.PLUS;

                case '-':
                    if (PeekNextCharacter() == '=')
                    {
                        EatNextCharacter();
                        return Token.SUBTRACTION_ASSIGNMENT;
                    }
                    else if (PeekNextCharacter() == '-')
                    {
                        EatNextCharacter();
                        return Token.DECREMENT;
                    }
                    else if (PeekNextCharacter() == '>')
                    {
                        EatNextCharacter();
                        return Token.MEMBER_ACCESS_POINTER_DEREFERENCE;
                    }
                    else if (char.IsDigit(PeekNextCharacter()))
                    {
                        return ParseNumber(nextCharacter);
                    }
                    return Token.MINUS;

                case '*':
                    if (PeekNextCharacter() == '=')
                    {
                        EatNextCharacter(); // Consume =
                        return Token.MULTIPLICATION_ASSIGNMENT;
                    }
                    return Token.MULTIPLY;

                case '/':
                    {
                        char c = PeekNextCharacter();
                        if (c == '=')
                        {
                            EatNextCharacter(); // Consume =
                            return Token.DIVISION_ASSIGNMENT;
                        }
                        else if (c == '/')
                        {
                            // C++ style comment
                            while ((c = PeekNextCharacter()) != '\n' && c != NoMoreCharacters)
                            {
                                EatNextCharacter();
                            }

                            if (this.TokenizeComments)
                            {
                                return Token.COMMENT;
                            }
                            else
                            {
                                return GetNextToken();
                            }
                        }
                        else if (c == '*')
                        {
                            /* old c style comment */
                            EatNextCharacter(); // Consume *
                                                // Read to end of comment
                            while (true)
                            {
                                if (PeekNextCharacter() == '*')
                                {
                                    EatNextCharacter(); // Consume *
                                    if (PeekNextCharacter() == '/')
                                    {
                                        EatNextCharacter(); // Consume /
                                        break;
                                    }
                                }

                                if (PeekNextCharacter() == NoMoreCharacters) return GetNextToken();

                                EatNextCharacter(); // Consume next comment character
                            }

                            if (this.TokenizeComments)
                            {
                                return Token.COMMENT;
                            }
                            else
                            {
                                return GetNextToken();
                            }
                        }
                        return Token.DIVIDE;
                    }
                case '%':
                    if (PeekNextCharacter() == '=')
                    {
                        EatNextCharacter(); // Consume =
                        return Token.REMAINDER_ASSIGNMENT;
                    }
                    return Token.REMAINDER;

                case '?':
                    if (PeekNextCharacter() == '?')
                    {
                        EatNextCharacter();
                        return Token.NULL_COALESCE;
                    }
                    else if (PeekNextCharacter() == '.')
                    {
                        EatNextCharacter();
                        return Token.MEMBER_ACCESS_NULL_CONDITIONAL;
                    }
                    else if (PeekNextCharacter() == '[')
                    {
                        EatNextCharacter();
                        return Token.ELEMENT_ACCESS_NULL_CONDITIONAL;
                    }
                    return Token.QUESTION_MARK;
                case '(': return Token.LEFT_PARENTHESIS;
                case ')': return Token.RIGHT_PARENTHESIS;
                case '[': return Token.LEFT_BRACKET;
                case ']': return Token.RIGHT_BRACKET;
                case '.': return Token.POINT;
                case ',': return Token.COMMA;
                case '~': return Token.COMPLEMENT;
                case ';': return Token.SEMI_COLON;
                case '{': return Token.SCOPE_BEGIN;
                case '}': return Token.SCOPE_END;
                case ':':
                    {
                        char c = PeekNextCharacter();

                        if (c == ':')
                        {
                            EatNextCharacter();
                            return Token.DOUBLE_COLON;
                        }
                    }
                    return Token.COLON;

                case '|':
                    {
                        char c = PeekNextCharacter();
                        if (c == '|')
                        {
                            EatNextCharacter(); // Consume |
                            return Token.LOGICAL_OR;
                        }
                        else if (c == '=')
                        {
                            EatNextCharacter(); // Consume =
                            return Token.BITWISE_OR_ASSIGNMENT;
                        }
                    }
                    return Token.BITWISE_INCLUSIVE_OR;

                case '^':
                    if (PeekNextCharacter() == '=')
                    {
                        EatNextCharacter(); // Consume =
                        return Token.BITWISE_XOR_ASSIGNMENT;
                    }
                    return Token.BITWISE_EXCLUSIVE_OR;

                case '&':
                    {
                        char c = PeekNextCharacter();
                        if (c == '&')
                        {
                            EatNextCharacter(); // Consume &
                            return Token.LOGICAL_AND;
                        }
                        else if (c == '=')
                        {
                            EatNextCharacter(); // Consume =
                            return Token.BITWISE_AND_ASSIGNMENT;
                        }
                    }
                    return Token.BITWISE_AND;

                case '=':
                    if (PeekNextCharacter() == '=')
                    {
                        EatNextCharacter(); // Consume =
                        return Token.EQUALS;
                    }
                    return Token.SIMPLE_ASSIGNMENT;

                case '!':
                    if (PeekNextCharacter() == '=')
                    {
                        EatNextCharacter(); // Consume =
                        return Token.NOT_EQUALS;
                    }
                    return Token.NOT;

                case '>':
                    {
                        char c = PeekNextCharacter();
                        if (c == '=')
                        {
                            EatNextCharacter(); // Consume =
                            return Token.GREATER_THAN_OR_EQUAL;
                        }
                        else if (c == '>')
                        {
                            var positionBackup = this.expressionStringPosition;

                            EatNextCharacter(); // Consume >
                            if (PeekNextCharacter() == '=')
                            {
                                EatNextCharacter(); // Consume =
                                return Token.RIGHT_SHIFT_ASSIGNMENT;
                            }

                            this.expressionStringPosition = positionBackup;
                        }
                    }
                    return Token.GREATER_THAN;

                case '<':
                    {
                        char c = PeekNextCharacter();
                        if (c == '=')
                        {
                            EatNextCharacter(); // Consume =
                            return Token.LESS_THAN_OR_EQUAL;
                        }
                        else if (c == '<')
                        {
                            EatNextCharacter(); // Consume <
                            if (PeekNextCharacter() == '=')
                            {
                                EatNextCharacter(); // Consume =
                                return Token.LEFT_SHIFT_ASSIGNMENT;
                            }
                            return Token.LEFT_SHIFT;
                        }
                    }
                    return Token.LESS_THAN;

                case '\'':
                    {
                        char c = PeekNextCharacter();
                        if (c == '\\')
                        {
                            EatNextCharacter(); // Consume \
                            CharacterConstantValue = EatNextCharacter();
                            if (EatNextCharacter() != '\'')
                            {
                                throw new SyntaxException(this, "End aposthrope ' expected in character constant");
                            }
                            return Token.CHAR_CONSTANT;
                        }
                        else if (!char.IsControl(c))
                        {
                            CharacterConstantValue = EatNextCharacter();
                            if (EatNextCharacter() != '\'')
                            {
                                throw new SyntaxException(this, "End aposthrope ' expected in character constant");
                            }
                            return Token.CHAR_CONSTANT;
                        }
                    }
                    throw new NotImplementedException("Fancy char escape string parsing");

                case '$':
                    {
                        char c = PeekNextCharacter();
                        
                        if (c == '"')
                        {
                            throw new SyntaxException(this, @"Interpolated strings ($""string"" are not supported");
                        }

                        if (char.IsDigit(c))
                        {
                            this.stringBuilder.Length = 0;

                            while (char.IsDigit(PeekNextCharacter()))
                            {
                                this.stringBuilder.Append(EatNextCharacter());
                            }

                            this.ExpressionArgumentNumber = Convert.ToInt32(this.stringBuilder.ToString());
                            return Token.NUMBERED_EXPRESSION_ARGUMENT;
                        }

                        if (IsValidIdentifierStartCharacter(c))
                        {
                            EatNextCharacter();
                            string identifier = ParseIdentifier(c);
                            this.IdentifierValue = identifier;
                            return Token.NAMED_EXPRESSION_ARGUMENT;
                        }

                        return Token.UNNUMBERED_EXPRESSION_ARGUMENT;
                    }

                case '"':
                    {
                        char c;
                        this.stringBuilder.Length = 0;

                        while ((c = EatNextCharacter()) != '"')
                        {
                            if (c == NoMoreCharacters)
                            {
                                throw new SyntaxException(this, "Expected \" to end string");
                            }

                            if (c == '\\')
                            {
                                if (PeekNextCharacter() == '"')
                                {
                                    EatNextCharacter(); // Eat '"'.
                                    this.stringBuilder.Append('"');
                                }
                                else if (PeekNextCharacter() == '\\')
                                {
                                    EatNextCharacter(); // Eat the next '\'.
                                    this.stringBuilder.Append('\\');
                                }
                                else if (PeekNextCharacter() == 'n')
                                {
                                    EatNextCharacter(); // Eat 'n'.
                                    this.stringBuilder.Append('\n');
                                }
                                else if (PeekNextCharacter() == 'r')
                                {
                                    EatNextCharacter(); // Eat 'r'.
                                    this.stringBuilder.Append('\r');
                                }
                                else if (PeekNextCharacter() == 't')
                                {
                                    EatNextCharacter(); // Eat 't'.
                                    this.stringBuilder.Append('\t');
                                }
                                else
                                {
                                    throw new SyntaxException(this, @"String escape sequences apart from \\, \"", \n, \r and \t are currently not supported");
                                }
                            }
                            else
                            {
                                this.stringBuilder.Append(c);
                            }

                            // TODO add support for all valid C# string escape sequences
                        }

                        this.StringConstantValue = this.stringBuilder.ToString();
                        return Token.STRING_CONSTANT;
                    }
                case '@':
                    {
                        if (PeekNextCharacter() == '"')
                        {
                            throw new SyntaxException(this, @"Literal strings (@""string"" are not supported");
                        }

#if SIRENIX_INTERNAL
                        return Token.AT_SIGN;
#endif

                        throw new SyntaxException(this, "Invalid token '@'");
                    }

                case '#':
                    {
                        if (PeekNextCharacter() == '(')
                        {
                            EatNextCharacter(); // Eat '('

                            this.stringBuilder.Length = 0;

                            char c;
                            while ((c = PeekNextCharacter()) != ')' && c != NoMoreCharacters)
                            {
                                EatNextCharacter();
                                this.stringBuilder.Append(c);
                            }

                            if (c != ')')
                            {
                                throw new SyntaxException(this, "Expected a ')' to end the property query started by '#('");
                            }

                            EatNextCharacter(); // Eat ')'

                            this.IdentifierValue = this.stringBuilder.ToString();
                            return Token.PROPERTY_QUERY;
                        }
                        else
                        {
                            char c;
                            while ((c = PeekNextCharacter()) != '\n' && c != NoMoreCharacters)
                            {
                                if (c == '/')
                                {
                                    c = PeekNextCharacter();
                                    if (c == '/' || c == '*')
                                    {
                                        break;
                                    }
                                }

                                EatNextCharacter();
                            }

                            if (this.TokenizePreprocessors)
                            {
                                return Token.PREPROCESSOR;
                            }
                            else
                            {
                                return GetNextToken();
                            }
                        }
                    }

                default:
                    if (char.IsDigit(nextCharacter))
                    {
                        return ParseNumber(nextCharacter);
                    }
                    else if (IsValidIdentifierStartCharacter(nextCharacter))
                    {
                        IdentifierValue = ParseIdentifier(nextCharacter);
                        switch (IdentifierValue)
                        {
                            case "sizeof": return Token.SIZEOF;
                            case "true": return Token.TRUE;
                            case "false": return Token.FALSE;
                            case "is": return Token.RELATIONAL_IS;
                            case "as": return Token.RELATIONAL_AS;
                            case "new": return Token.NEW;
                            case "this": return Token.THIS;
                            case "base": return Token.BASE;
                            case "default": return Token.DEFAULT;
                            case "checked": return Token.CHECKED;
                            case "unchecked": return Token.UNCHECKED;
                            case "null": return Token.NULL;
                            case "typeof": return Token.TYPEOF;
                            case "void": return Token.VOID;
                            case "ref": return Token.REF;
                            case "out": return Token.OUT;
                            case "in": return Token.IN;
                            case "class": return Token.CLASS;
                            case "struct": return Token.STRUCT;
                            case "interface": return Token.INTERFACE;
                            case "return": return Token.RETURN;
                        }
                        return Token.IDENTIFIER;
                    }
                    throw new SyntaxException(this, "Unknown token '" + nextCharacter + "' at position " + (this.expressionStringPosition - 1)); // TODO: Line number?
            }
        }

        private void EatWhiteSpace()
        {
            while (char.IsWhiteSpace(PeekNextCharacter()))
            {
                EatNextCharacter();
            }
        }

        private char EatNextCharacter()
        {
            if (this.expressionStringPosition == this.expressionString.Length)
            {
                return NoMoreCharacters;
            }
            return this.expressionString[this.expressionStringPosition++];
        }

        private char PeekNextCharacter(int offset = 0)
        {
            if ((this.expressionStringPosition + offset) >= this.expressionString.Length)
            {
                return NoMoreCharacters;
            }
            return this.expressionString[this.expressionStringPosition + offset];
        }

        private string ParseIdentifier(char c)
        {
            this.stringBuilder.Length = 0;
            this.stringBuilder.Append(c);

            while (IsValidIdentifierPartCharacter(c = PeekNextCharacter()))
            {
                this.stringBuilder.Append(EatNextCharacter());
            }
            return this.stringBuilder.ToString();
        }

        private Token ParseNumber(char c)
        {
            bool negativeNumber = c == '-' ? true : false;
            bool floatingNumber = false;

            this.stringBuilder.Length = 0;

            if (c != '+')
            {
                this.stringBuilder.Append(c);
            }

            while (char.IsDigit(c = PeekNextCharacter()))
            {
                this.stringBuilder.Append(EatNextCharacter());
            }

            if (c == '.' && char.IsDigit(PeekNextCharacter(1)))
            {
                floatingNumber = true;
                EatNextCharacter(); // Consume .
                this.stringBuilder.Append('.'); // Use culture decimal separator instead
                while (char.IsDigit(c = PeekNextCharacter()))
                {
                    this.stringBuilder.Append(EatNextCharacter());
                }
            }

            if (c == 'e' || c == 'E')
            {
                throw new SyntaxException(this, "Exponential number literals are not supported");
            }

            if (this.stringBuilder.Length == 1 && this.stringBuilder[0] == '0')
            {
                if (c == 'b' || c == 'B')
                {
                    throw new SyntaxException(this, "Binary number literals are not supported");
                }

                if (c == 'x' || c == 'X')
                {
                    throw new SyntaxException(this, "Hexadecimal number literals are not supported");
                }
            }

            var numberString = this.stringBuilder.ToString();

            try
            {
                if (c == 'm' || c == 'M')
                {
                    EatNextCharacter(); // Consume d or D
                    DecimalConstantValue = decimal.Parse(numberString, CultureInfo.InvariantCulture);
                    return Token.DECIMAL;
                }

                if (c == 'f' || c == 'F' || c == 'd' || c == 'D') floatingNumber = true;

                if (floatingNumber)
                {
                    if (c == 'f' || c == 'F')
                    {
                        EatNextCharacter(); // Consume f or F
                        Float32ConstantValue = float.Parse(numberString, NumberStyles.Float, CultureInfo.InvariantCulture);
                        return Token.FLOAT32;
                    }
                    else if (c == 'd' || c == 'D')
                    {
                        EatNextCharacter(); // Consume d or D
                    }

                    Float64ConstantValue = double.Parse(numberString, NumberStyles.Float, CultureInfo.InvariantCulture);
                    return Token.FLOAT64;
                }
                else
                {
                    if (c == 'u' || c == 'U')
                    {
                        EatNextCharacter();
                        c = PeekNextCharacter();

                        if (c == 'l' || c == 'L')
                        {
                            if (negativeNumber)
                            {
                                throw new SyntaxException(this, "An ulong number literal cannot be negative.");
                            }

                            // ulong
                            EatNextCharacter();
                            UnsignedIntegerConstantValue = ulong.Parse(numberString, CultureInfo.InvariantCulture);
                            return Token.UNSIGNED_INT64;
                        }
                        else
                        {
                            if (negativeNumber)
                            {
                                throw new SyntaxException(this, "An uint number literal cannot be negative.");
                            }

                            // uint
                            UnsignedIntegerConstantValue = ulong.Parse(numberString, CultureInfo.InvariantCulture);

                            if (UnsignedIntegerConstantValue < uint.MinValue || UnsignedIntegerConstantValue > uint.MaxValue)
                            {
                                throw new SyntaxException(this, "uint constant '" + numberString + "' is out of uint number bounds. Use an ulong instead.");
                            }

                            return Token.UNSIGNED_INT32;
                        }
                    }
                    else if (c == 'l' || c == 'L')
                    {
                        // long
                        EatNextCharacter();
                        SignedIntegerConstantValue = long.Parse(numberString, CultureInfo.InvariantCulture);
                        return Token.SIGNED_INT64;
                    }
                    else
                    {
                        int intValue;
                        uint uintValue;
                        long longValue;
                        ulong ulongValue;

                        if (int.TryParse(numberString, out intValue))
                        {
                            SignedIntegerConstantValue = intValue;
                            return Token.SIGNED_INT32;
                        }
                        else if (uint.TryParse(numberString, out uintValue))
                        {
                            UnsignedIntegerConstantValue = uintValue;
                            return Token.UNSIGNED_INT32;
                        }
                        else if (long.TryParse(numberString, out longValue))
                        {
                            SignedIntegerConstantValue = longValue;
                            return Token.SIGNED_INT64;
                        }
                        else if (ulong.TryParse(numberString, out ulongValue))
                        {
                            UnsignedIntegerConstantValue = ulongValue;
                            return Token.UNSIGNED_INT64;
                        }

                        throw new SyntaxException(this, "Could not parse number literal '" + numberString + "'");
                    }
                }
            }
            catch (OverflowException)
            {
                throw new SyntaxException(this, "Number literal '" + numberString + "' overflowed bounds");
            }
        }

        private static bool IsValidIdentifierStartCharacter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '@' || char.IsLetter(c);
        }

        private static bool IsValidIdentifierPartCharacter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || (c >= '0' && c <= '9') || char.IsLetter(c);
        }
    }
}
#endif