#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SyntaxException.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    using System;

    internal class SyntaxException : Exception
    {
        public readonly Tokenizer Tokenizer;
        public readonly ASTNode ASTNode;

        public SyntaxException(Tokenizer tokenizer, string message) : base(message)
        {
            this.Tokenizer = tokenizer;
        }

        public SyntaxException(ASTNode node, string message) : base(message)
        {
            this.ASTNode = node;
        }

        public string GetNiceErrorMessage(string expression, bool richText)
        {
            return this.Message + "\n\n" + GetCodeErrorSnippet(expression, richText);
        }

        public string GetCodeErrorSnippet(string expression, bool richText)
        {
            int start, length;

            if (this.Tokenizer != null)
            {
                start = this.Tokenizer.TokenStartedStringPosition;
                length = this.Tokenizer.ExpressionStringPosition - start;
            }
            else
            {
                start = this.ASTNode.NodeStartIndex;
                length = this.ASTNode.NodeEndIndex - start;
            }

            return expression.Substring(0, start) + (richText ? "<color=#B70000FF>" : ">>>>") + expression.Substring(start, length) + (richText ? "</color>" : "<<<<") + expression.Substring(start + length);
        }
    }
}
#endif