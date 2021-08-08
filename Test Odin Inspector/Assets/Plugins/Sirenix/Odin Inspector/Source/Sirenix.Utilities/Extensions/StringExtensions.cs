//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// String method extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Eg MY_INT_VALUE => MyIntValue
        /// </summary>
        public static string ToTitleCase(this string input)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                var current = input[i];
                if (current == '_' && i + 1 < input.Length)
                {
                    var next = input[i + 1];
                    if (char.IsLower(next))
                    {
                        next = char.ToUpper(next, CultureInfo.InvariantCulture);
                    }

                    builder.Append(next);
                    i++;
                }
                else
                {
                    builder.Append(current);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns whether or not the specified string is contained with this string
        /// </summary>
        public static bool Contains(this string source, string toCheck, StringComparison comparisonType)
        {
            return source.IndexOf(toCheck, comparisonType) >= 0;
        }

        /// <summary>
        /// Ex: "thisIsCamelCase" -> "This Is Camel Case"
        /// </summary>
        public static string SplitPascalCase(this string input)
        {
            if (input == null || input.Length == 0)
            {
                return input;
            }

            StringBuilder sb = new StringBuilder(input.Length);

            if (char.IsLetter(input[0]))
            {
                sb.Append(char.ToUpper(input[0]));
            }
            else
            {
                sb.Append(input[0]);
            }

            for (int i = 1; i < input.Length; i++)
            {
                char c = input[i];
                
                if (char.IsUpper(c) && !char.IsUpper(input[i - 1]))
                {
                    sb.Append(' ');
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if this string is null, empty, or contains only whitespace.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <returns><c>true</c> if this string is null, empty, or contains only whitespace; otherwise, <c>false</c>.</returns>
        public static bool IsNullOrWhitespace(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (char.IsWhiteSpace(str[i]) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}