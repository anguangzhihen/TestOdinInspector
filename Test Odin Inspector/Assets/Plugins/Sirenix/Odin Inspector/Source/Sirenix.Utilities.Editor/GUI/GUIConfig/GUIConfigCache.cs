#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIConfigCache.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Utilities;

    internal static class GUIContextCache<TPrimaryKey, TSecondaryKey, TValue>
    {
        private static GUIFrameCounter guiFrameCounter = new GUIFrameCounter();

        [NonSerialized]
        private static DoubleLookupDictionary<TPrimaryKey, TSecondaryKey, IControlContext> configs = new DoubleLookupDictionary<TPrimaryKey, TSecondaryKey, IControlContext>();

        private const int NUMBER_OF_FRAMES_CACHED = 1000;

        public static GUIContext<TValue> GetConfig(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
        {
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            if (secondaryKey == null)
            {
                throw new ArgumentNullException("secondaryKey");
            }

            RemoveUnusedConfigs();

            IControlContext iControlConfig;
            GUIContext<TValue> config;
            configs.TryGetInnerValue(primaryKey, secondaryKey, out iControlConfig);
            config = iControlConfig as GUIContext<TValue>;

            if (config == null)
            {
                config = new GUIContext<TValue>();
                configs[primaryKey][secondaryKey] = config;
                iControlConfig = config;
            }

            ;

            iControlConfig.LastRenderedFrameId = guiFrameCounter.Update().FrameCount;
            return config;
        }

        private static bool hasRemovedInLayoutEvent;

        private static void RemoveUnusedConfigs()
        {
            guiFrameCounter.Update();

            if (Event.current == null)
            {
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                if (hasRemovedInLayoutEvent == false)
                {
                    configs.RemoveWhere(x => x.LastRenderedFrameId + NUMBER_OF_FRAMES_CACHED < guiFrameCounter.FrameCount);
                    hasRemovedInLayoutEvent = true;
                }
            }
            else
            {
                hasRemovedInLayoutEvent = false;
            }
        }
    }
}
#endif