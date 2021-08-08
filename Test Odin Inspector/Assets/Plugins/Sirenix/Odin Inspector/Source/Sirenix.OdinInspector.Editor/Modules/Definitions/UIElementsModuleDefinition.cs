#if UNITY_EDITOR
////-----------------------------------------------------------------------
//// <copyright file="UIElementsModuleDefinition.cs" company="Sirenix IVS">
//// Copyright (c) Sirenix IVS. All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------

//namespace Sirenix.OdinInspector.Editor.Modules
//{
#pragma warning disable

//    using Sirenix.Utilities;
//    using System;

//    public class UIElementsModuleDefinition : ModuleDefinition
//    {
//        public override string ID { get { return "UIElements"; } }

//        public override string BuildFromPath
//        {
//            get
//            {
//                return "../Sirenix Solution/Sirenix.OdinInspector.UIElements/Package";
//            }
//        }

//        public override Version LatestVersion
//        {
//            get
//            {
//                return new Version(1, 0, 0, 0);
//            }
//        }

//        public override string NiceName
//        {
//            get
//            {
//                return "UIElements support for Odin";
//            }
//        }

//        public override string Description
//        {
//            get
//            {
//                return @"This upcoming module adds a UIElements integration to Odin.";
//            }
//        }

//        public override string DependenciesDescription
//        {
//            get
//            {
//                return "Unity 2019.3+";
//            }
//        }

//        public override bool CheckSupportsCurrentEnvironment()
//        {
//            return UnityVersion.IsVersionOrGreater(2019, 3);
//        }

//        public override ModuleData GetModuleDataForPackaging()
//        {
//            var data = base.GetModuleDataForPackaging();

//            data.Files.RemoveAll(file =>
//            {
//                if (file.Path.EndsWith("package.json")
//                 || file.Path.EndsWith("package.json.meta")
//                 || file.Path.EndsWith(".asmdef")
//                 || file.Path.EndsWith(".asmdef.meta"))
//                    return true;
//                return false;
//            });

//            return data;
//        }
//    }
//}
#endif