#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GenericNumberUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class GenericNumberUtility
    {
        private static HashSet<Type> Numbers = new HashSet<Type>(FastTypeComparer.Instance)
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(IntPtr),
            typeof(UIntPtr),
        };

        private static HashSet<Type> Vectors = new HashSet<Type>(FastTypeComparer.Instance)
        {
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
        };

        public static bool IsNumber(Type type)
        {
            return Numbers.Contains(type);
        }

        public static bool IsVector(Type type)
        {
            return Vectors.Contains(type);
        }

        public static bool NumberIsInRange(object number, double min, double max)
        {
            if (number is sbyte)
            {
                var n = (sbyte)number;
                return n >= min && n <= max;
            }
            else if (number is byte)
            {
                var n = (byte)number;
                return n >= min && n <= max;
            }
            else if (number is short)
            {
                var n = (short)number;
                return n >= min && n <= max;
            }
            else if (number is ushort)
            {
                var n = (ushort)number;
                return n >= min && n <= max;
            }
            else if (number is int)
            {
                var n = (int)number;
                return n >= min && n <= max;
            }
            else if (number is uint)
            {
                var n = (uint)number;
                return n >= min && n <= max;
            }
            else if (number is long)
            {
                var n = (long)number;
                return n >= min && n <= max;
            }
            else if (number is ulong)
            {
                var n = (ulong)number;
                return n >= min && n <= max;
            }
            else if (number is float)
            {
                var n = (float)number;
                return n >= min && n <= max;
            }
            else if (number is double)
            {
                var n = (double)number;
                return n >= min && n <= max;
            }
            else if (number is decimal)
            {
                var n = (decimal)number;
                return n >= (decimal)min && n <= (decimal)max;
            }
            else if (number is Vector2)
            {
                var n = (Vector2)number;
                return n.x >= min && n.x <= max
                    && n.y >= min && n.y <= max;
            }
            else if (number is Vector3)
            {
                var n = (Vector3)number;
                return n.x >= min && n.x <= max
                    && n.y >= min && n.y <= max
                    && n.z >= min && n.z <= max;
            }
            else if (number is Vector4)
            {
                var n = (Vector4)number;
                return n.x >= min && n.x <= max
                    && n.y >= min && n.y <= max
                    && n.z >= min && n.z <= max
                    && n.w >= min && n.w <= max;
            }
            else if (number is IntPtr)
            {
                var n = (long)(IntPtr)number;
                return n >= min && n <= max;
            }
            else if (number is UIntPtr)
            {
                var n = (ulong)(UIntPtr)number;
                return n >= min && n <= max;
            }

            return false;
        }

        public static T Clamp<T>(T number, double min, double max)
        {
            if (number is sbyte)
            {
                var n = (sbyte)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is byte)
            {
                var n = (byte)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is short)
            {
                var n = (short)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is ushort)
            {
                var n = (ushort)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is int)
            {
                var n = (int)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is uint)
            {
                var n = (uint)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is long)
            {
                var n = (long)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is ulong)
            {
                var n = (ulong)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is float)
            {
                var n = (float)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is double)
            {
                var n = (double)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is decimal)
            {
                var n = (decimal)(object)number;
                if (n < (decimal)min) return ConvertNumber<T>(min);
                if (n > (decimal)max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is Vector2)
            {
                var n = (Vector2)(object)number;

                if (n.x < min) n.x = (float)min;
                else if (n.x > max) n.x = (float)max;

                if (n.y < min) n.y = (float)min;
                else if (n.y > max) n.y = (float)max;

                return (T)(object)n;
            }
            else if (number is Vector3)
            {
                var n = (Vector3)(object)number;

                if (n.x < min) n.x = (float)min;
                else if (n.x > max) n.x = (float)max;

                if (n.y < min) n.y = (float)min;
                else if (n.y > max) n.y = (float)max;

                if (n.z < min) n.z = (float)min;
                else if (n.z > max) n.z = (float)max;

                return (T)(object)n;
            }
            else if (number is Vector4)
            {
                var n = (Vector4)(object)number;

                if (n.x < min) n.x = (float)min;
                else if (n.x > max) n.x = (float)max;

                if (n.y < min) n.y = (float)min;
                else if (n.y > max) n.y = (float)max;

                if (n.z < min) n.z = (float)min;
                else if (n.z > max) n.z = (float)max;

                if (n.w < min) n.w = (float)min;
                else if (n.w > max) n.w = (float)max;

                return (T)(object)n;
            }
            else if (number is IntPtr)
            {
                var n = (long)(IntPtr)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is UIntPtr)
            {
                var n = (ulong)(IntPtr)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }

            return number;
        }

        public static T ConvertNumber<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static object ConvertNumberWeak(object value, Type to)
        {
            return Convert.ChangeType(value, to);
        }
    }
}
#endif