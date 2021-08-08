//-----------------------------------------------------------------------
// <copyright file="Vector3DictionaryKeyPathProvider.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterDictionaryKeyPathProvider(typeof(Vector3DictionaryKeyPathProvider))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System.Globalization;
    using UnityEngine;

    /// <summary>
    /// Dictionary key path provider for <see cref="UnityEngine.Vector3"/>
    /// </summary>
    public sealed class Vector3DictionaryKeyPathProvider : BaseDictionaryKeyPathProvider<Vector3>
    {
        public override string ProviderID { get { return "v3"; } }

        public override int Compare(Vector3 x, Vector3 y)
        {
            int result = x.x.CompareTo(y.x);

            if (result == 0)
            {
                result = x.y.CompareTo(y.y);
            }

            if (result == 0)
            {
                result = x.z.CompareTo(y.z);
            }

            return result;
        }

        public override Vector3 GetKeyFromPathString(string pathStr)
        {
            int sep1 = pathStr.IndexOf('|');
            int sep2 = pathStr.IndexOf('|', sep1 + 1);

            string x = pathStr.Substring(1, sep1 - 1).Trim();
            string y = pathStr.Substring(sep1 + 1, sep2 - (sep1 + 1)).Trim();
            string z = pathStr.Substring(sep2 + 1, pathStr.Length - (sep2 + 2)).Trim();

            return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }

        public override string GetPathStringFromKey(Vector3 key)
        {
            var x = key.x.ToString("R", CultureInfo.InvariantCulture);
            var y = key.y.ToString("R", CultureInfo.InvariantCulture);
            var z = key.z.ToString("R", CultureInfo.InvariantCulture);
            return ("(" + x + "|" + y + "|" + z + ")").Replace('.', ',');
        }
    }
}