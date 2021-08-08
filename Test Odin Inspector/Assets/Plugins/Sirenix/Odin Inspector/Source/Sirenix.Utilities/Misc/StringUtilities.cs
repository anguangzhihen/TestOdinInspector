//-----------------------------------------------------------------------
// <copyright file="StringUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
#pragma warning disable

	using System;
	using System.Text;

    /// <summary>
    /// Not yet documented.
    /// </summary>
	public static class StringUtilities
	{
        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="bytes">Not yet documented.</param>
        /// <param name="decimals">Not yet documented.</param>
        /// <returns>Not yet documented.</returns>
		public static string NicifyByteSize(int bytes, int decimals = 1)
		{
			StringBuilder builder = new StringBuilder();

			if (bytes < 0)
			{
				builder.Append('-');
				bytes = Math.Abs(bytes);
			}

			int decimalLength = 0;
			string m = null;
			if (bytes > 1000000000)
			{
				builder.Append(bytes / 1000000000);
				bytes -= (bytes / 1000000000) * 1000000000;
				decimalLength = 9;
				m = " GB";
			}
			else if (bytes > 1000000)
			{
				builder.Append(bytes / 1000000);
				bytes -= (bytes / 1000000) * 1000000;
				decimalLength = 6;
				m = " MB";
			}
			else if (bytes > 1000)
			{
				builder.Append(bytes / 1000);
				bytes -= (bytes / 1000) * 1000;
				decimalLength = 3;
				m = " KB";
			}
			else
			{
				builder.Append(bytes);
				decimals = 0;
				decimalLength = 0;
				m = " bytes";
			}

			if (decimals > 0 && decimalLength > 0 && bytes > 0)
			{
				string d = bytes.ToString().PadLeft(decimalLength, '0');
				d = d.Substring(0, decimals < d.Length ? decimals : d.Length).TrimEnd('0');

				if (d.Length > 0)
				{
					builder.Append('.');
					builder.Append(d);
				}
			}

			builder.Append(m);
			return builder.ToString();
		}

        public static bool FastEndsWith(this string str, string endsWith)
        {
            if (str.Length < endsWith.Length) return false;

            int strLength = str.Length;

            for (int i = 0; i < endsWith.Length; i++)
            {
                if (str[str.Length - (1 + i)] != endsWith[endsWith.Length - (1 + i)]) return false;
            }

            return true;
        }

        /// <summary>
        /// Compares two strings in a number-aware manner, IE, "[2] Foo" is considered to come before "[10] Bar".
        /// </summary>
        public static int NumberAwareStringCompare(string a, string b, bool ignoreLeadingZeroes = true, bool ignoreWhiteSpace = true, bool ignoreCase = false)
        {
            int len1 = a.Length;
            int len2 = b.Length;
            int i1 = 0;
            int i2 = 0;

            while (true)
            {
                // If we reach the end of the string and the lengths are different, then the longer string is higher
                bool end1 = i1 == len1;
                bool end2 = i2 == len2;

                if (end1 && end2)
                {
                    // However, we do need to account for having skipped 0's and whitespaces, and in those cases where 
                    // only the skipping made a difference in when we reach the end of each string, we need to compare 
                    // the *actual* string lengths, not just how far into each string we are.

                    if (len1 == len2) return 0;
                    else if (len1 < len2) return -1;
                    return 1;
                }
                else if (end1)
                {
                    return -1;
                }
                else if (end2)
                {
                    return 1;
                }

                if (ignoreWhiteSpace)
                {
                    while (i1 < len1 && char.IsWhiteSpace(a[i1]))
                        i1++;
                    while (i2 < len2 && char.IsWhiteSpace(b[i2]))
                        i2++;
                }

                char c1 = a[i1];
                char c2 = b[i2];

                if (char.IsDigit(c1) && char.IsDigit(c2))
                {
                    if (ignoreLeadingZeroes)
                    {
                        while (i1 < len1 && a[i1] == '0')
                            i1++;
                        while (i2 < len2 && b[i2] == '0')
                            i2++;
                    }

                    int digEnd1 = i1;
                    int digEnd2 = i2;

                    while (digEnd1 < len1 && char.IsDigit(a[digEnd1]))
                        digEnd1++;
                    while (digEnd2 < len2 && char.IsDigit(b[digEnd2]))
                        digEnd2++;

                    int dig1Length = digEnd1 - i1;
                    int dig2Length = digEnd2 - i2;

                    // Numbers with more digits are always bigger
                    if (dig1Length != dig2Length)
                        return dig1Length - dig2Length;

                    // Now we have to do a digit-by-digit comparison
                    while (i1 < digEnd1)
                    {
                        if (a[i1] != b[i2])
                            return a[i1] - b[i2];
                        i1++;
                        i2++;
                    }
                }
                else
                {
                    // Regular character comparison
                    if (ignoreCase)
                    {
                        if (c1 != c2)
                        {
                            c1 = char.ToLower(c1);
                            c2 = char.ToLower(c2);

                            if (c1 != c2)
                            {
                                return c1 - c2;
                            }
                        }
                    }
                    else
                    {
                        // We still need to convert to same case, because we only want case to matter for the "same char"
                        var lc1 = char.ToLower(c1);
                        var lc2 = char.ToLower(c2);

                        if (lc1 == lc2)
                        {
                            // It's the "same char"
                            if (c1 != c2)
                            {
                                // Inverse, since lowercase chars are "lesser" than uppercase chars
                                return c2 - c1;
                            }
                        }
                        else
                        {
                            return lc1 - lc2;
                        }

                        //if (c1 != c2)
                        //{
                        //    else
                        //    {
                        //        return c1 - c2;
                        //    }
                        //}
                    }

                    i1++;
                    i2++;
                }
            }
        }
    }
}