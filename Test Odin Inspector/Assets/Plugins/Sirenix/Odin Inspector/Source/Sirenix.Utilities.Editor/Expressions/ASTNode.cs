#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ASTNode.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class ASTNode
    {
        public NodeType NodeType;
        public object NodeValue;
        public int NodeStartIndex;
        public int NodeEndIndex;
        public Type TypeOfValue;
        public readonly ASTNodeChildren Children;
        public ASTNode Parent;
        
        public ASTNode()
        {
            this.Children = new ASTNodeChildren(this);
        }

        public override string ToString()
        {
            return ToPrettyPrint();
        }

        public ASTNode DeepCopy()
        {
            ASTNode copy = new ASTNode();

            copy.NodeType = this.NodeType;
            copy.NodeValue = this.NodeValue;
            copy.NodeStartIndex = this.NodeStartIndex;
            copy.NodeEndIndex = this.NodeEndIndex;
            copy.TypeOfValue = this.TypeOfValue;
            copy.Parent = this.Parent;

            for (int i = 0; i < this.Children.Count; i++)
            {
                copy.Children[i] = this.Children[i].DeepCopy();
            }

            return copy;
        }

        public string ToPrettyPrint()
        {
            var sb = new StringBuilder();

            this.PrettyPrint(sb, 0);
            return sb.ToString();
        }

        private void PrettyPrint(StringBuilder sb, int depth)
        {
            if (sb.Length > 0) sb.AppendLine();

            for (int i = 0; i < depth * 4; i++)
            {
                sb.Append(' ');
            }

            sb.Append(this.NodeType.ToString());

            if (this.NodeType == NodeType.CONSTANT_NULL || !object.ReferenceEquals(this.NodeValue, null))
            {
                sb.Append(": ");
                sb.Append(object.ReferenceEquals(this.NodeValue, null) ? "null" : this.NodeValue.ToString());
            }

            for (int i = 0; i < this.Children.Count; i++)
            {
                this.Children[i].PrettyPrint(sb, depth + 1);
            }
        }

        public Type GetHighestPushedStackType()
        {
            if (this.TypeOfValue != null) return this.TypeOfValue;

            for (int i = this.Children.Count - 1; i >= 0; i--)
            {
                var result = this.Children[i].GetHighestPushedStackType();

                if (result != null) return result;
            }

            return null;
        }

        public class ASTNodeChildren
        {
            private ASTNode child0;
            private ASTNode child1;
            private ASTNode child2;
            private ASTNode child3;
            private List<ASTNode> children;
            private ASTNode owner;

            public ASTNodeChildren(ASTNode owner)
            {
                this.owner = owner;
            }

            public int Count
            {
                get
                {
                    if (this.children != null) return this.children.Count;
                    if (child0 == null) return 0;
                    if (child1 == null) return 1;
                    if (child2 == null) return 2;
                    if (child3 == null) return 3;
                    return 4;
                }
            }

            public ASTNode this[int index]
            {
                get
                {
                    if (this.children != null) return this.children[index];
                    switch (index)
                    {
                        case 0: return child0;
                        case 1: return child1;
                        case 2: return child2;
                        case 3: return child3;
                        default: throw new IndexOutOfRangeException();
                    }
                }
                set
                {
                    if (index < 0) throw new IndexOutOfRangeException();
                    if (this.children != null)
                    {
                        while (this.children.Count <= index)
                        {
                            this.children.Add(null);
                        }

                        this.children[index] = value;
                    }
                    else
                    {
                        switch (index)
                        {
                            case 0: child0 = value; break;
                            case 1: child1 = value; break;
                            case 2: child2 = value; break;
                            case 3: child3 = value; break;
                            default:
                                this.children = new List<ASTNode>()
                            {
                                child0, child1, child2, child3
                            };
                                this[index] = value;
                                break;
                        }
                    }

                    value.Parent = this.owner;
                }
            }

            public void Clear()
            {
                child0 = null;
                child1 = null;
                child2 = null;
                child3 = null;

                if (this.children != null)
                    this.children.Clear();
            }
        }
    }
}
#endif