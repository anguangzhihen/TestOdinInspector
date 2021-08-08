//-----------------------------------------------------------------------
// <copyright file="ColorExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Globalization;
    using UnityEngine;

    /// <summary>
    /// Extension methods for the UnityEngine.Color type.
    /// </summary>
    public static class ColorExtensions
    {
        private static readonly char[] trimRGBStart = new char[] { 'R', 'r', 'G', 'g', 'B', 'b', 'A', 'a', '(' };

        /// <summary>
        /// Lerps between multiple colors.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static Color Lerp(this Color[] colors, float t)
        {
            t = Mathf.Clamp(t, 0, 1) * (colors.Length - 1);
            int a = (int)t;
            int b = Mathf.Min((int)t + 1, colors.Length - 1);
            return Color.Lerp(colors[a], colors[b], t - (int)t);
        }

        /// <summary>
        /// Moves the towards implementation for Color.
        /// </summary>
        /// <param name="from">From color.</param>
        /// <param name="to">To color.</param>
        /// <param name="maxDelta">The maximum delta.</param>
        public static Color MoveTowards(this Color from, Color to, float maxDelta)
        {
            Color result = new Color();
            result.r = Mathf.MoveTowards(from.r, to.r, maxDelta);
            result.g = Mathf.MoveTowards(from.g, to.g, maxDelta);
            result.b = Mathf.MoveTowards(from.b, to.b, maxDelta);
            result.a = Mathf.MoveTowards(from.a, to.a, maxDelta);

            from.r = result.r;
            from.g = result.g;
            from.b = result.b;
            from.a = result.a;

            return result;
        }

        /// <summary>
        /// Tries to parse a string to a Color. The following formats are supported:
        /// "new Color(0.4, 0, 0, 1)", "#FFEEBBFF", "#FFEECC", "FFEEBBFF", "FFEECC"
        /// </summary>
        /// <param name="colorStr">The color string.</param>
        /// <param name="color">The color.</param>
        /// <returns>Returns true if the parse was a success.</returns>
        public static bool TryParseString(string colorStr, out Color color)
        {
            color = default(Color);

            if (colorStr == null || colorStr.Length < 2 || colorStr.Length > 100)
            {
                return false;
            }

            if (colorStr.StartsWith("new Color", StringComparison.InvariantCulture))
            {
                colorStr = colorStr.Substring("new Color".Length, colorStr.Length - "new Color".Length).Replace("f", "");
            }

            bool couldBeHex = colorStr[0] == '#' || char.IsLetter(colorStr[0]) || char.IsNumber(colorStr[0]);
            bool couldBeRGB = colorStr[0] == 'R' || colorStr[0] == '(' || char.IsNumber(colorStr[0]);

            if (couldBeHex == false && couldBeRGB == false)
            {
                return false;
            }
            bool didConvert = false;
            if (couldBeRGB || couldBeHex && (didConvert = ColorUtility.TryParseHtmlString(colorStr, out color)) == false && couldBeRGB)
            {
                colorStr = colorStr.TrimStart(trimRGBStart).TrimEnd(')');
                string[] components = colorStr.Split(',');
                if (components.Length < 2 || components.Length > 4)
                {
                    return false;
                }

                Color result = new Color(0, 0, 0, 1);
                for (int i = 0; i < components.Length; i++)
                {
                    float component;
                    if (float.TryParse(components[i], out component) == false)
                    {
                        return false;
                    }

                    if (i == 0) result.r = component;
                    if (i == 1) result.g = component;
                    if (i == 2) result.b = component;
                    if (i == 3) result.a = component;
                }
                color = result;
                return true;
            }
            else if (didConvert)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a color to a string formatted to c#
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>new Color(r, g, b, a)</returns>
        public static string ToCSharpColor(this Color color)
        {
            return "new Color(" +
                        TrimFloat(color.r) + "f, " +
                        TrimFloat(color.g) + "f, " +
                        TrimFloat(color.b) + "f, " +
                        TrimFloat(color.a) + "f)";
        }

        /// <summary>
        /// Pows the color with the specified factor.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="factor">The factor.</param>
        public static Color Pow(this Color color, float factor)
        {
            color.r = Mathf.Pow(color.r, factor);
            color.g = Mathf.Pow(color.g, factor);
            color.b = Mathf.Pow(color.b, factor);
            color.a = Mathf.Pow(color.a, factor);
            return color;
        }

        /// <summary>
        /// Normalizes the RGB values of the color ignoring the alpha value.
        /// </summary>
        /// <param name="color">The color.</param>
        public static Color NormalizeRGB(this Color color)
        {
            Vector3 c = new Vector3(color.r, color.g, color.b).normalized;
            color.r = c.x;
            color.g = c.y;
            color.b = c.z;
            return color;
        }

        private static string TrimFloat(float value)
        {
            string str = value.ToString("F3", CultureInfo.InvariantCulture).TrimEnd('0');
            char lastChar = str[str.Length - 1];
            if (lastChar == '.' || lastChar == ',')
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }
}