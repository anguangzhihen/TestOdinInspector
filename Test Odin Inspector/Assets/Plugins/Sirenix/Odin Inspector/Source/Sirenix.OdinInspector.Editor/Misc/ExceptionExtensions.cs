#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ExceptionExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using UnityEngine;

    internal static class ExceptionExtensions
    {
        public static bool IsExitGUIException(this Exception ex)
        {
            do
            {
                if (ex is ExitGUIException) return true;
                ex = ex.InnerException;
            }
            while (ex != null);

            return false;
        }

        public static ExitGUIException AsExitGUIException(this Exception ex)
        {
            do
            {
                if (ex is ExitGUIException) return ex as ExitGUIException;
                ex = ex.InnerException;
            }
            while (ex != null);

            return null;
        }
    }
}
#endif