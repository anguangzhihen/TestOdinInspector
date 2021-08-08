#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TrialUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if ODIN_TRIAL
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization.Utilities;
    using System;
    using System.Text;

    internal unsafe static class TrialUtilities
    {
        public static bool IsExpired { get { return TimeLeft.TotalHours < 0; } }

        public static bool IsReallyExpired { get { return TimeLeft.TotalDays < -3; } }

        public static TimeSpan TimeLeft { get { return EndTime - DateTime.Now; } }

        public static string TimeLeftString
        {
            get
            {
                using (var sbCache = Cache<StringBuilder>.Claim())
                {
                    var sb = sbCache.Value;
                    sb.Length = 0;
                    var time = TimeLeft;

                    if (time.TotalHours < 0)
                        time = time.Negate();

                    if (time.Days > 0)
                    {
                        int days = (int)Math.Ceiling(time.TotalDays);
                        sb.Append(days);
                        sb.Append(days < 2 ? " day" : " days");
                    }
                    else
                    {
                        sb.Append(time.Hours);
                        sb.Append(time.Hours == 1 ? " hour and " : " hours and ");
                        sb.Append(time.Minutes);
                        sb.Append(time.Minutes == 1 ? " minute" : " minutes");
                    }
                    return sb.ToString();
                }
            }
        }

        public static DateTime EndTime
        {
            get
            {
#if SIRENIX_INTERNAL
                return DateTime.Now.AddDays(0.9);
#else
                fixed (char* dataPtr = "f94d96de81d04ad5803f2fb70c45cb10\0\0\0\0685d")
                {
                    long* date = (long*)(dataPtr + 32);
                    return DateTime.FromBinary(*date);
                }
#endif
            }
        }

        public static string EndTimeString
        {
            get
            {
                return EndTime.ToString("d");
            }
        }
    }
}
#endif
#endif