#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinInspectorVersion.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;

    /// <summary>
    /// Installed Odin Inspector Version Info.
    /// </summary>
    public static class OdinInspectorVersion
    {
        private static string version;
        private static string buildName;

        /// <summary>
        /// Gets the name of the current running version of Odin Inspector.
        /// </summary>
        public static string BuildName
        {
            get
            {
                if (buildName == null)
                {
                    var attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildNameAttribute>(true);
                    buildName = attribute != null ? attribute.BuildName : "Source Code";
                }

                return buildName;
            }
        }

        /// <summary>
        /// Gets the current running version of Odin Inspector.
        /// </summary>
        public static string Version
        {
            get
            {
                if (version == null)
                {
                    var attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildVersionAttribute>(true);
                    version = attribute != null ? attribute.Version : "Source Code Mode";
                }

                return version;
            }
        }

        /// <summary>
        /// Whether the current version of Odin is an enterprise version.
        /// </summary>
        public static bool IsEnterprise
        {
            get
            {
                return false;
            }
        }
    }
}
#endif