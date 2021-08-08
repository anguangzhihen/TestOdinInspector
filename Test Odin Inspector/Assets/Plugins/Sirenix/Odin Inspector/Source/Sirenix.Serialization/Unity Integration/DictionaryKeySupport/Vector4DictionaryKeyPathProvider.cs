//-----------------------------------------------------------------------
// <copyright file="Vector4DictionaryKeyPathProvider.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Sirenix.Serialization;

[assembly: RegisterDictionaryKeyPathProvider(typeof(Vector4DictionaryKeyPathProvider))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System.Globalization;
    using UnityEngine;

    public sealed class Vector4DictionaryKeyPathProvider : BaseDictionaryKeyPathProvider<Vector4>
    {
        public override string ProviderID { get { return "v4"; } }

        public override int Compare(Vector4 x, Vector4 y)
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

            if (result == 0)
            {
                result = x.w.CompareTo(y.w);
            }

            return result;
        }

        public override Vector4 GetKeyFromPathString(string pathStr)
        {
            int sep1 = pathStr.IndexOf('|');
            int sep2 = pathStr.IndexOf('|', sep1 + 1);
            int sep3 = pathStr.IndexOf('|', sep2 + 1);

            string x = pathStr.Substring(1, sep1 - 1).Trim();
            string y = pathStr.Substring(sep1 + 1, sep2 - (sep1 + 1)).Trim();
            string z = pathStr.Substring(sep2 + 1, sep3 - (sep2 + 1)).Trim();
            string w = pathStr.Substring(sep3 + 1, pathStr.Length - (sep3 + 2)).Trim();

            return new Vector4(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
        }

        public override string GetPathStringFromKey(Vector4 key)
        {
            var x = key.x.ToString("R", CultureInfo.InvariantCulture);
            var y = key.y.ToString("R", CultureInfo.InvariantCulture);
            var z = key.z.ToString("R", CultureInfo.InvariantCulture);
            var w = key.w.ToString("R", CultureInfo.InvariantCulture);

            return ("(" + x + "|" + y + "|" + z + "|" + w + ")").Replace('.', ',');
        }
    }
}