#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CollectionChangeInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization.Utilities;
    using Sirenix.Utilities;
    using System.Text;

    /// <summary>
    /// Contains information about a change that is going to occur/has occurred to a collection.
    /// </summary>
    /// <seealso cref="CollectionChangeType"/>
    public struct CollectionChangeInfo
    {
        public CollectionChangeType ChangeType;
        public object Key;
        public object Value;
        public int Index;
        public int SelectionIndex;

        public override string ToString()
        {
            using (var sbCache = Cache<StringBuilder>.Claim())
            {
                var sb = sbCache.Value;
                sb.Length = 0;

                sb.Append("CollectionChangeInfo { ");
                AppendValue(sb, "ChangeType", this.ChangeType, false);

                switch (ChangeType)
                {
                    case CollectionChangeType.Unspecified:
                        AppendValue(sb, "Key", this.Key);
                        AppendValue(sb, "Value", this.Value);
                        AppendValue(sb, "Index", this.Index);
                        break;
                    case CollectionChangeType.Add:
                        AppendValue(sb, "Value", this.Value);
                        break;
                    case CollectionChangeType.Insert:
                        AppendValue(sb, "Value", this.Value);
                        AppendValue(sb, "Index", this.Index);
                        break;
                    case CollectionChangeType.RemoveValue:
                        AppendValue(sb, "Value", this.Value);
                        break;
                    case CollectionChangeType.RemoveIndex:
                        AppendValue(sb, "Index", this.Index);
                        break;
                    case CollectionChangeType.RemoveKey:
                        AppendValue(sb, "Key", this.Key);
                        break;
                    case CollectionChangeType.SetKey:
                        AppendValue(sb, "Key", this.Key);
                        AppendValue(sb, "Value", this.Value);
                        break;
                    case CollectionChangeType.Clear:
                    default:
                        break;
                }

                AppendValue(sb, "SelectionIndex", this.SelectionIndex);
                sb.Append(" }");

                return sb.ToString();
            }
        }

        private static void AppendValue(StringBuilder sb, string name, object value, bool prependComma = true)
        {
            if (prependComma) sb.Append(", ");
            sb.Append(name);
            sb.Append(" = ");
            sb.Append(value ?? "null");
        }
    }
}
#endif