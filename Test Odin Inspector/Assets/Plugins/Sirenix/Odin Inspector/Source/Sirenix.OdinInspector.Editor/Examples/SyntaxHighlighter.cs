#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SyntaxHighlighter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.Utilities.Editor.Expressions;
    using Sirenix.Utilities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    internal class SyntaxHighlighter
    {
        public static Color BackgroundColor = new Color(0.118f, 0.118f, 0.118f, 1f);
        public static Color TextColor = new Color(0.863f, 0.863f, 0.863f, 1f);
        public static Color KeywordColor = new Color(0.337f, 0.612f, 0.839f, 1f);
        public static Color IdentifierColor = new Color(0.306f, 0.788f, 0.69f, 1f);
        public static Color CommentColor = new Color(0.341f, 0.651f, 0.29f, 1f);
        public static Color LiteralColor = new Color(0.71f, 0.808f, 0.659f, 1f);
        public static Color StringLiteralColor = new Color(0.839f, 0.616f, 0.522f, 1f);

        private Tokenizer tokenizer;

        private StringBuilder result = new StringBuilder();
        //private HashSet<string> memberIdentities = new HashSet<string>();
        private List<TokenBuffer> statement = new List<TokenBuffer>();
        private int textPosition;

        public static string Parse(string text)
        {
            return new SyntaxHighlighter().ParseText(text);
        }

        public string ParseText(string text)
        {
            this.result.Length = 0;
            //this.memberIdentities.Clear();
            this.statement.Clear();
            this.textPosition = 0;

            this.tokenizer = new Tokenizer(text)
            {
                TokenizeComments = true,
                TokenizePreprocessors = true,
            };

            this.ReadDeclaration();

            return this.result.ToString();
        }

        private void ReadDeclaration()
        {
            Token token = Token.UNKNOWN;

            while (token != Token.EOF)
            {
                token = this.tokenizer.GetNextToken();

                if (token == Token.EOF)
                {
                    break;
                }
                else if (token == Token.COMMENT)
                {
                    this.AppendWhitespace(this.tokenizer.TokenStartedStringPosition);
                    this.Colorize(this.tokenizer.TokenStartedStringPosition, this.tokenizer.ExpressionStringPosition - this.tokenizer.TokenStartedStringPosition, CommentColor);
                    continue;
                }
                else if (token == Token.PREPROCESSOR)
                {
                    this.AppendWhitespace(this.tokenizer.TokenStartedStringPosition);
                    this.Append(this.tokenizer.TokenStartedStringPosition, this.tokenizer.ExpressionStringPosition - this.tokenizer.TokenStartedStringPosition);
                    continue;
                }

                this.statement.Add(new TokenBuffer(token, this.tokenizer));

                if (this.statement[0].Token == Token.LEFT_BRACKET)
                {
                    if (token == Token.RIGHT_BRACKET)
                    {
                        this.AppendDeclaration(this.statement, ref this.textPosition);
                        this.statement.Clear();
                    }
                }
                else if (token == Token.SCOPE_BEGIN)
                {
                    if (this.statement.Any(i => i.Token == Token.LEFT_PARENTHESIS))
                    {
                        this.AppendMember(this.statement);
                        this.statement.Clear();
                        this.ReadImplementation();
                    }
                    else if (this.statement.Any(i => i.Token == Token.CLASS || i.Token == Token.STRUCT || i.Token == Token.INTERFACE))
                    {
                        this.AppendDeclaration(this.statement, ref this.textPosition);
                        this.statement.Clear();
                    }
                    else
                    {
                        this.AppendMember(this.statement);
                        this.statement.Clear();
                        this.ReadImplementation();
                    }
                }
                else if (token == Token.SCOPE_END)
                {
                    this.AppendMember(this.statement);
                    this.statement.Clear();
                }
                else if (token == Token.SEMI_COLON)
                {
                    this.AppendMember(this.statement);
                    this.statement.Clear();
                }
            }

            if (this.statement.Count > 0)
            {
                this.AppendDeclaration(this.statement, ref this.textPosition);
                this.statement.Clear();
            }
        }

        private void ReadImplementation()
        {
            Token token = Token.UNKNOWN;

            while (token != Token.EOF && token != Token.SCOPE_END)
            {
                token = this.tokenizer.GetNextToken();
                this.statement.Add(new TokenBuffer(token, this.tokenizer));
            }

            this.AppendImplementation(this.statement);
            this.statement.Clear();
        }

        private void AppendDeclaration(List<TokenBuffer> statementBuffer, ref int prevIndex)
        {
            for (int i = 0; i < statementBuffer.Count; i++)
            {
                var t = statementBuffer[i];

                this.AppendWhitespace(t.StartIndex);

                if (t.Token == Token.IDENTIFIER)
                {
                    string id = t.GetString(this.tokenizer);
                    if (TypeExtensions.IsCSharpKeyword(id))
                    {
                        Colorize(id, KeywordColor);
                    }
                    else
                    {
                        Colorize(id, IdentifierColor);
                    }

                    prevIndex = t.EndIndex;
                }
                else
                {
                    this.AppendToken(t);
                }
            }
        }

        private void AppendMember(List<TokenBuffer> statement)
        {
            for (int i = 0; i < statement.Count; i++)
            {
                var t = statement[i];
                this.AppendWhitespace(t.StartIndex);

                if (t.Token == Token.IDENTIFIER)
                {
                    string id = t.GetString(this.tokenizer);
                    if (TypeExtensions.IsCSharpKeyword(id))
                    {
                        Colorize(id, KeywordColor);
                    }
                    else
                    {
                        var next = i + 1 < statement.Count ? statement[i + 1].Token : Token.UNKNOWN;

                        if (next != Token.UNKNOWN && (next == Token.SIMPLE_ASSIGNMENT || next == Token.SEMI_COLON || next == Token.SCOPE_BEGIN || next == Token.LEFT_PARENTHESIS || next == Token.COMMA))
                        {
                            // This should be the name of a member.
                            //this.memberIdentities.Add(id);
                            this.Append(id);
                        }
                        else
                        {
                            this.Colorize(id, IdentifierColor);
                        }
                    }

                    this.textPosition = t.EndIndex;
                }
                else
                {
                    AppendToken(t);
                }
            }
        }

        private void AppendImplementation(List<TokenBuffer> statement)
        {
            for (int i = 0; i < statement.Count; i++)
            {
                var t = statement[i];
                this.AppendWhitespace(t.StartIndex);

                if (t.Token == Token.IDENTIFIER)
                {
                    string id = t.GetString(this.tokenizer);
                    if (TypeExtensions.IsCSharpKeyword(id))
                    {
                        Colorize(id, KeywordColor);
                    }
                    else
                    {
                        this.result.Append(id);
                    }

                    this.textPosition = t.EndIndex;
                }
                else
                {
                    AppendToken(t);
                }
            }
        }

        private void AppendToken(TokenBuffer buffer)
        {
            this.AppendWhitespace(buffer.StartIndex);

            switch (buffer.Token)
            {
                case Token.IDENTIFIER:
                    {
                        string id = this.tokenizer.ExpressionString.Substring(buffer.StartIndex, buffer.Length);

                        if (TypeExtensions.IsCSharpKeyword(id))
                        {
                            Colorize(id, KeywordColor);
                        }
                        else
                        {
                            Colorize(id, IdentifierColor);
                        }
                    }
                    break;

                case Token.SIZEOF:
                case Token.NEW:
                case Token.TRUE:
                case Token.FALSE:
                case Token.RELATIONAL_IS:
                case Token.RELATIONAL_AS:
                case Token.THIS:
                case Token.BASE:
                case Token.CHECKED:
                case Token.UNCHECKED:
                case Token.DEFAULT:
                case Token.NULL:
                case Token.TYPEOF:
                case Token.VOID:
                case Token.REF:
                case Token.OUT:
                case Token.IN:
                case Token.CLASS:
                case Token.STRUCT:
                case Token.INTERFACE:
                case Token.RETURN:
                    this.Colorize(buffer.StartIndex, buffer.Length, KeywordColor);
                    break;

                case Token.SIGNED_INT32:
                case Token.UNSIGNED_INT32:
                case Token.SIGNED_INT64:
                case Token.UNSIGNED_INT64:
                case Token.FLOAT32:
                case Token.FLOAT64:
                case Token.DECIMAL:
                    this.Colorize(buffer.StartIndex, buffer.Length, LiteralColor);
                    break;

                case Token.CHAR_CONSTANT:
                case Token.STRING_CONSTANT:
                    this.Colorize(buffer.StartIndex, buffer.Length, StringLiteralColor);
                    break;

                case Token.COMMENT:
                    this.Colorize(buffer.StartIndex, buffer.Length, CommentColor);
                    break;

                case Token.EOF:
                    return;

                default:
                    this.result.Append(this.tokenizer.ExpressionString, buffer.StartIndex, buffer.Length);
                    break;
            }

            this.textPosition = buffer.EndIndex;
        }

        private void Colorize(string text, Color color)
        {
            this.result.Append("<color=#");
            this.result.Append(ColorUtility.ToHtmlStringRGBA(color));
            this.result.Append(">");
            this.Append(text);
            this.result.Append("</color>");
        }
        private void Colorize(int start, int length, Color color)
        {
            this.result.Append("<color=#");
            this.result.Append(ColorUtility.ToHtmlStringRGBA(color));
            this.result.Append(">");
            this.Append(start, length);
            this.result.Append("</color>");
        }
        private void Append(int start, int length)
        {
            this.result.Append(this.tokenizer.ExpressionString, start, length);
            this.textPosition = start + length;
        }
        private void Append(string text)
        {
            this.result.Append(text);
            this.textPosition += text.Length;
        }
        private void AppendWhitespace(int position)
        {
            if (position - this.textPosition > 0)
            {
                this.result.Append(this.tokenizer.ExpressionString, this.textPosition, position - this.textPosition);
                this.textPosition = position;
            }
        }

        private struct TokenBuffer
        {
            public Token Token;
            public int StartIndex;
            public int EndIndex;

            public int Length
            {
                get { return this.EndIndex - this.StartIndex; }
            }

            public TokenBuffer(Token token, Tokenizer tokenizer)
            {
                this.Token = token;
                this.StartIndex = tokenizer.TokenStartedStringPosition;
                this.EndIndex = tokenizer.ExpressionStringPosition;
            }

            public string GetString(Tokenizer tokenizer)
            {
                return tokenizer.ExpressionString.Substring(this.StartIndex, this.Length);
            }

            public override string ToString()
            {
                return this.Token.ToString();
            }
        }
    }
}
#endif