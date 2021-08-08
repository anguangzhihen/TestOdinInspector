#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NamedValues.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    using Sirenix.Serialization.Utilities;
    using Sirenix.Utilities;
    using System;
    using System.Text;

    public struct NamedValue
    {
        public string Name;
        public Type Type;
        public object CurrentValue;
        public NamedValueGetter ValueGetter;

        public NamedValue(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
            this.CurrentValue = null;
            this.ValueGetter = null;
        }

        public NamedValue(string name, Type type, object value)
        {
            this.Name = name;
            this.Type = type;
            this.CurrentValue = value;
            this.ValueGetter = null;
        }

        public NamedValue(string name, Type type, NamedValueGetter valueGetter)
        {
            this.Name = name;
            this.Type = type;
            this.CurrentValue = null;
            this.ValueGetter = valueGetter;
        }

        public void Update(ref ActionResolverContext context, int selectionIndex)
        {
            if (this.ValueGetter != null)
            {
                this.CurrentValue = this.ValueGetter(ref context, selectionIndex);
            }
        }
    }

    public struct NamedValues
    {
        private const int BASE_VALUES_COUNT = 8;

        private NamedValue v0;
        private NamedValue v1;
        private NamedValue v2;
        private NamedValue v3;
        private NamedValue v4;
        private NamedValue v5;
        private NamedValue v6;
        private NamedValue v7;
        private NamedValue[] array;
        private int count;

        public int Count { get { return this.count; } }

        public NamedValue this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return this.v0;
                    case 1: return this.v1;
                    case 2: return this.v2;
                    case 3: return this.v3;
                    case 4: return this.v4;
                    case 5: return this.v5;
                    case 6: return this.v6;
                    case 7: return this.v7;
                    default:
                        if (this.array != null && (index - BASE_VALUES_COUNT) < this.array.Length) return this.array[index - BASE_VALUES_COUNT];
                        else throw new IndexOutOfRangeException();
                }
            }
        }

        public void UpdateValues(ref ActionResolverContext context, int selectionIndex)
        {
            if (this.v0.Type != null) { if (this.v0.ValueGetter != null) { this.v0.CurrentValue = this.v0.ValueGetter(ref context, selectionIndex); } } else return;
            if (this.v1.Type != null) { if (this.v1.ValueGetter != null) { this.v1.CurrentValue = this.v1.ValueGetter(ref context, selectionIndex); } } else return;
            if (this.v2.Type != null) { if (this.v2.ValueGetter != null) { this.v2.CurrentValue = this.v2.ValueGetter(ref context, selectionIndex); } } else return;
            if (this.v3.Type != null) { if (this.v3.ValueGetter != null) { this.v3.CurrentValue = this.v3.ValueGetter(ref context, selectionIndex); } } else return;
            if (this.v4.Type != null) { if (this.v4.ValueGetter != null) { this.v4.CurrentValue = this.v4.ValueGetter(ref context, selectionIndex); } } else return;
            if (this.v5.Type != null) { if (this.v5.ValueGetter != null) { this.v5.CurrentValue = this.v5.ValueGetter(ref context, selectionIndex); } } else return;
            if (this.v6.Type != null) { if (this.v6.ValueGetter != null) { this.v6.CurrentValue = this.v6.ValueGetter(ref context, selectionIndex); } } else return;
            if (this.v7.Type != null) { if (this.v7.ValueGetter != null) { this.v7.CurrentValue = this.v7.ValueGetter(ref context, selectionIndex); } } else return;

            var arr = this.array;
            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Type == null) break;
                    var valueGetter = arr[i].ValueGetter;
                    if (valueGetter != null)
                    {
                        arr[i].CurrentValue = valueGetter(ref context, selectionIndex);
                    }
                }
            }
        }

        public void Set(string name, object value)
        {
            if (this.v0.Name == name) { this.v0.CurrentValue = value; return; }
            if (this.v1.Name == name) { this.v1.CurrentValue = value; return; }
            if (this.v2.Name == name) { this.v2.CurrentValue = value; return; }
            if (this.v3.Name == name) { this.v3.CurrentValue = value; return; }
            if (this.v4.Name == name) { this.v4.CurrentValue = value; return; }
            if (this.v5.Name == name) { this.v5.CurrentValue = value; return; }
            if (this.v6.Name == name) { this.v6.CurrentValue = value; return; }
            if (this.v7.Name == name) { this.v7.CurrentValue = value; return; }

            var arr = this.array;
            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    var n = arr[i].Name;
                    if (n == null) break;
                    if (n == name)
                    {
                        arr[i].CurrentValue = value;
                        return;
                    }
                }
            }

            throw new ArgumentException("No named value '" + name + "' found to set.");
        }

        public object GetValue(string name)
        {
            if (this.v0.Name == name) { return this.v0.CurrentValue; }
            if (this.v1.Name == name) { return this.v1.CurrentValue; }
            if (this.v2.Name == name) { return this.v2.CurrentValue; }
            if (this.v3.Name == name) { return this.v3.CurrentValue; }
            if (this.v4.Name == name) { return this.v4.CurrentValue; }
            if (this.v5.Name == name) { return this.v5.CurrentValue; }
            if (this.v6.Name == name) { return this.v6.CurrentValue; }
            if (this.v7.Name == name) { return this.v7.CurrentValue; }

            var arr = this.array;
            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    var n = arr[i].Name;
                    if (n == null) break;
                    if (n == name)
                    {
                        return arr[i].CurrentValue;
                    }
                }
            }

            throw new ArgumentException("No named value '" + name + "' found to get.");
        }

        public bool TryGetValue(string name, out NamedValue value)
        {
            if (this.v0.Name == name) { value = this.v0; return true; }
            if (this.v1.Name == name) { value = this.v1; return true; }
            if (this.v2.Name == name) { value = this.v2; return true; }
            if (this.v3.Name == name) { value = this.v3; return true; }
            if (this.v4.Name == name) { value = this.v4; return true; }
            if (this.v5.Name == name) { value = this.v5; return true; }
            if (this.v6.Name == name) { value = this.v6; return true; }
            if (this.v7.Name == name) { value = this.v7; return true; }

            var arr = this.array;
            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    var n = arr[i].Name;
                    if (n == null) break;
                    if (n == name)
                    {
                        value = arr[i];
                        return true;
                    }
                }
            }

            value = default(NamedValue);
            return false;
        }

        public void Add(string name, Type type, NamedValueGetter valueGetter)
        {
            this.Add(new NamedValue()
            {
                Name = name,
                Type = type,
                CurrentValue = null,
                ValueGetter = valueGetter,
            });
        }

        public void Add(string name, Type type, object value)
        {
            this.Add(new NamedValue()
            {
                Name = name,
                Type = type,
                CurrentValue = value,
                ValueGetter = null,
            });
        }

        public void Add(NamedValue value)
        {
            switch (this.count)
            {
                case 0: this.v0 = value; break;
                case 1: this.v1 = value; break;
                case 2: this.v2 = value; break;
                case 3: this.v3 = value; break;
                case 4: this.v4 = value; break;
                case 5: this.v5 = value; break;
                case 6: this.v6 = value; break;
                case 7: this.v7 = value; break;
                case 8:
                    this.array = new NamedValue[4];
                    this.array[0] = value;
                    break;
                default:
                    {
                        int index = this.count - BASE_VALUES_COUNT;
                        if (index < this.array.Length) this.array[index] = value;
                        else
                        {
                            var newArray = new NamedValue[this.array.Length * 2];
                            Array.Copy(this.array, newArray, this.array.Length);
                            this.array = newArray;
                            this.array[index] = value;
                        }
                    }
                    break;
            }

            this.count++;
        }

        public string GetValueOverviewString()
        {
            var count = this.count;
            using (var sbCache = Cache<StringBuilder>.Claim())
            {
                var sb = sbCache.Value;
                sb.Length = 0;

                for (int i = 0; i < count; i++)
                {
                    if (i > 0)
                    {
                        sb.AppendLine();
                    }

                    var value = this[i];

                    sb.Append(value.Name);
                    sb.Append(" (");
                    sb.Append(value.Type.GetNiceName());
                    sb.Append(")");
                }

                return sb.ToString();
            }
        }
    }
}
#endif