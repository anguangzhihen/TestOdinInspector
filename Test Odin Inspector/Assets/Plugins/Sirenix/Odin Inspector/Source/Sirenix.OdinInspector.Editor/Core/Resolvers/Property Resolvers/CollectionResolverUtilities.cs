#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CollectionResolverUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public static class CollectionResolverUtilities
    {
        public static string DefaultIndexToChildName(int index)
        {
            return "$" + index;
        }

        public static int DefaultChildNameToIndex(string name)
        {
            if (name.Length <= 1) return -1;

            int index;

            string indexStr;
            if (name[0] == '$')
            {
                indexStr = name.Substring(1);
            }
            else if (name.Length > 2 && name[0] == '[' && name[name.Length - 1] == ']')
            {
                indexStr = name.Substring(1, name.Length - 2);
            }
            else return -1;

            if (int.TryParse(indexStr, out index) && index >= 0)
            {
                return index;
            }

            return -1;
        }
    }
}
#endif