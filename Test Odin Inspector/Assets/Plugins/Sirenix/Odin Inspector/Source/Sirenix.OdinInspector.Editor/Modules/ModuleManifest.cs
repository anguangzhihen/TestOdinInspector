#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ModuleManifest.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    public class ModuleManifest
    {
        public string ID;
        public Version Version;
        public List<string> Files;

        public static void Save(string path, ModuleManifest manifest)
        {
            List<string> lines = new List<string>(manifest.Files.Count + 4)
            {
                "ManifestVersion: 1",
                "ModuleID: " + manifest.ID,
                "ModuleVersion: " + manifest.Version.ToString(),
                "ModuleFiles:"
            };

            foreach (var file in manifest.Files)
            {
                lines.Add("  " + file);
            }

            File.WriteAllLines(path, lines.ToArray());
        }

        public static ModuleManifest Load(string path)
        {
            if (!File.Exists(path)) return null;

            string[] lines = File.ReadAllLines(path);

            if (lines.Length == 0) return null;

            if (lines[0].TrimEnd() == "ManifestVersion: 1")
            {
                return Load_Version1(lines);
            }
            else
            {
                Debug.LogError("Cannot read Odin module manifest file with version '" + lines[0] + "'.");
                return null;
            }
        }

        private static ModuleManifest Load_Version1(string[] lines)
        {
            var dataEntries = lines.Where(n => !n.StartsWith("#") && !string.IsNullOrEmpty(n.Trim())).Select(n => LineData.Parse(n)).ToArray();

            if (dataEntries.Length < 4) return null;

            ModuleManifest manifest = new ModuleManifest()
            {
                Files = new List<string>()
            };

            for (int i = 1; i < dataEntries.Length; i++)
            {
                var data = dataEntries[i];

                switch (data.Key)
                {
                    case "ModuleID":
                        manifest.ID = data.Data;
                        break;
                    case "ModuleVersion":
                        manifest.Version = new Version(data.Data);
                        break;
                    case "ModuleFiles":

                        int index = i + 1;

                        while (index < dataEntries.Length)
                        {
                            var fileData = dataEntries[index++];

                            if (fileData.Key != null || fileData.Data.Length < 3 || fileData.Data[0] != ' ' || fileData.Data[1] != ' ')
                                break;

                            manifest.Files.Add(fileData.Data.Trim());
                        }

                        i = index - 1;
                        break;
                }
            }

            return manifest;
        }

        private struct LineData
        {
            public string Key;
            public string Data;

            public static LineData Parse(string line)
            {
                if (line.Length > 2 && line[0] == ' ' && line[1] == ' ')
                {
                    return new LineData()
                    {
                        Key = null,
                        Data = line.TrimEnd()
                    };
                }

                int separator = line.IndexOf(':');

                if (separator < 0)
                {
                    return new LineData()
                    {
                        Key = null,
                        Data = line.Trim()
                    };
                }

                return new LineData()
                {
                    Key = line.Substring(0, separator).Trim(),
                    Data = separator + 1 == line.Length ? "" : line.Substring(separator + 1).Trim()
                };
            }

            public override string ToString()
            {
                if (Key == null)
                {
                    return "{ " + this.Data + " }";
                }

                return "{ " + this.Key + ", " + this.Data + " }";
            }
        }
    }
}
#endif