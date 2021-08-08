#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ModuleDataManager.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using System.IO;

    public class ModuleDataManager
    {
        public string DataPath;
        public string InstallPath;

        public virtual void SaveData(string id, byte[] data)
        {
            var path = this.DataPath.TrimEnd('/', '\\') + "/" + id + ".data";

            var file = new FileInfo(path);

            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            using (var fileStream = new FileStream(file.FullName, FileMode.Create))
            {
                fileStream.Write(data, 0, data.Length);
            }
        }

        public virtual bool HasData(string id)
        {
            var path = this.DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
            return File.Exists(path);
        }

        public virtual byte[] LoadData(string id)
        {
            var path = this.DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
            if (!File.Exists(path)) return null;

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }
    }
}
#endif