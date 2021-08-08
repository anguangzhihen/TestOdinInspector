#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ModuleData.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ModuleData
    {
        public string ID;
        public Version Version;
        public List<ModuleFile> Files;

        public class ModuleFile
        {
            public string Path;
            public byte[] Data;
        }

        public ModuleManifest ToManifest()
        {
            return new ModuleManifest()
            {
                ID = this.ID,
                Version = this.Version,
                Files = this.Files.Select(n => n.Path).ToList()
            };
        }

        public static byte[] Serialize(ModuleData data)
        {
            return SerializationUtility.SerializeValue(data, DataFormat.Binary);
        }

        public static ModuleData Deserialize(byte[] bytes)
        {
            return SerializationUtility.DeserializeValue<ModuleData>(bytes, DataFormat.Binary);
        }
    }
}
#endif