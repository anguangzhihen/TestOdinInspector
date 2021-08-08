#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityMathematicsModuleDefinition.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Sirenix.Utilities.Editor;
using System;

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    public class UnityMathematicsModuleDefinition : ModuleDefinition
    {
        public override string ID
        {
            get
            {
                return "Unity.Mathematics";
            }
        }

        public override string NiceName
        {
            get
            {
                return "Unity.Mathematics support";
            }
        }

        public override Version LatestVersion
        {
            get
            {
                return new Version(1, 0, 1, 0);
            }
        }

        public override string Description
        {
            get
            {
                return "This small module contains a set of custom drawers to improve the performance, look and functionality of drawing Unity.Mathematics structs in the inspector.";
            }
        }

        public override string DependenciesDescription
        {
            get
            {
                return "com.unity.mathematics package v1.0+";
            }
        }

        public override string BuildFromPath
        {
            get
            {
                return "../Sirenix Solution/Sirenix.OdinInspector.SmallModules/Packages/com.unity.mathematics/";
            }
        }

        public override bool CheckSupportsCurrentEnvironment()
        {
            return UnityPackageUtility.HasPackageInstalled("com.unity.mathematics", new Version(1, 0));
        }
    }
}
#endif