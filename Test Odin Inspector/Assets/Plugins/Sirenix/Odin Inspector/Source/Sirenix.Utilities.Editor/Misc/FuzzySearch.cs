#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FuzzySearch.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    /// <summary>
    /// Compare strings and produce a distance score between them.
    /// </summary>
    public static class FuzzySearch
    {
        public static bool Contains(string searchTerm, string text)
        {
            if (searchTerm == null || searchTerm.Length == 0 || text == null || text.Length == 0)
            {
                return false;
            }

            int i = 0;
            int j = 0;
            int stLength = searchTerm.Length;
            int txtLength = text.Length;

            char a = searchTerm[i];
            while (a == ' ' && ++i < stLength) a = searchTerm[i]; // Skip spaces
            bool doAbbriviation = char.IsUpper(a);
            if (!doAbbriviation && a >= 65 && a <= 90) a = (char)(a + 32); // To lower

            char b;

            var prevWasLetter = false;

            do
            {
                b = text[j++];

                if (doAbbriviation)
                {
                    if (!prevWasLetter)
                    {
                        b = char.ToUpper(b);
                    }
                }
                else
                {
                    if (b >= 65 && b <= 90) b = (char)(b + 32); // To lower
                }

                if (a == b)
                {
                    i++;
                    if (i >= stLength) break;

                    a = searchTerm[i];
                    while (a == ' ' && ++i < stLength) a = searchTerm[i]; // Skip spaces
                    doAbbriviation = char.IsUpper(a);
                    if (!doAbbriviation && a >= 65 && a <= 90) a = (char)(a + 32); // To lower
                }

                prevWasLetter = char.IsLetter(b); // TODO: Inline IsLetter
            } while (j < txtLength);

            return i >= stLength;
        }

        public static bool Contains(string searchTerm, string text, out int score)
        {
            score = 0;

            if (searchTerm == null || searchTerm.Length == 0 || text == null || text.Length == 0)
            {
                return false;
            }

            int i = 0;
            int j = 0;
            int stLength = searchTerm.Length;
            int txtLength = text.Length;

            char a = searchTerm[i];
            while (a == ' ' && ++i < stLength) a = searchTerm[i]; // Skip spaces
            bool doAbbriviation = char.IsUpper(a);
            if (!doAbbriviation && a >= 65 && a <= 90) a = (char)(a + 32); // To lower

            char b;

            const int majorBonus = 50;
            const int minorBonus = 20;

            int bonus = majorBonus;
            var prevWasLetter = false;

            do
            {
                b = text[j++];

                if (doAbbriviation)
                {
                    if (!prevWasLetter)
                    {
                        bonus = majorBonus;
                        b = char.ToUpper(b);
                    }
                }
                else
                {
                    if (b >= 65 && b <= 90) b = (char)(b + 32); // To lower
                }

                if (a == b)
                {
                    score += bonus;
                    bonus += minorBonus;

                    i++;
                    if (i >= stLength) break;

                    a = searchTerm[i];
                    while (a == ' ' && ++i < stLength) a = searchTerm[i]; // Skip spaces
                    doAbbriviation = char.IsUpper(a);
                    if (!doAbbriviation && a >= 65 && a <= 90) a = (char)(a + 32); // To lower
                }
                else if (doAbbriviation == false || char.IsUpper(b)) // TODO: Inline ToUpper
                {
                    bonus = prevWasLetter ? minorBonus : majorBonus;
                }

                prevWasLetter = char.IsLetter(b); // TODO: Inline IsLetter
            } while (j < txtLength);

            score -= txtLength - i;
            return i >= stLength;
        }

        /// <summary>
        /// Determines whether if the source is within the search.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="ignoreCase">Should the algorithm ignore letter case?.</param>
        /// <param name="abbreviation">Should the algorithm attempt to search on an abbreviation of the source?.</param>
        /// <param name="threshold">Threshold for what is considered to be within the search. 0 will return everything and 1 will only return exact matches.</param>
        /// <returns>True if the source is within the search. Otherwise false.</returns>
        [System.Obsolete("Use FuzzySearch.Contains(searchTerm, text, out score) instead.")]
        public static bool Contains(ref string source, ref string target, float threshold = 0.8f, bool ignoreCase = true, bool abbreviation = true)
        {
            return Compare(ref source, ref target, ignoreCase, abbreviation) >= threshold;
        }

        /// <summary>
        /// Compares the target to the source and returns a distance score.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="ignoreCase"></param>
        /// <param name="abbreviation"></param>
        /// <returns>Distance score. 0 is no match, and 1 is exact match.</returns>
        [System.Obsolete("Use FuzzySearch.Contains(searchTerm, text, out score) instead.")]
        public static float Compare(ref string source, ref string target, bool ignoreCase = true, bool abbreviation = true)
        {
            int tLength = target != null ? target.Length : 0;
            int sLength = source != null ? source.Length : 0;

            if (tLength == 0) return 1f;
            if (sLength == 0) return 0f;

            int t = 0;
            int s = 0;
            char tChar = target[0];
            if (tChar >= 65 && tChar <= 90) tChar = (char)(tChar + 32);

            while (t < tLength && s < sLength)
            {
                if (tChar != ' ')
                {
                    char sChar = source[s];
                    s++;

                    if (sChar != ' ')
                    {
                        if (sChar >= 65 && sChar <= 90) sChar = (char)(sChar + 32);

                        if (sChar == tChar)
                        {
                            t++;
                            if (t == tLength) break;

                            tChar = target[t];
                            if (tChar >= 65 && tChar <= 90) tChar = (char)(tChar + 32);
                        }
                    }
                }
                else
                {
                    t++;
                }
            }

            return (float)t / tLength;
        }
    }
}
#endif