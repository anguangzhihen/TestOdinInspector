#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ASTVisitor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    using System;

    internal abstract class ASTVisitor
    {
        public void Visit(ASTNode ast)
        {
            switch (ast.NodeType)
            {
                case NodeType.TERNARY_CONDITIONAL:
                    this.TernaryConditional(ast);
                    break;
                case NodeType.NULL_COALESCE:
                    this.NullCoalesce(ast);
                    break;
                case NodeType.LOGICAL_OR:
                    this.LogicalOr(ast);
                    break;
                case NodeType.LOGICAL_AND:
                    this.LogicalAnd(ast);
                    break;
                case NodeType.BITWISE_INCLUSIVE_OR:
                    this.BitwiseInclusiveOr(ast);
                    break;
                case NodeType.BITWISE_EXCLUSIVE_OR:
                    this.BitwiseExclusiveOr(ast);
                    break;
                case NodeType.BITWISE_AND:
                    this.BitwiseAnd(ast);
                    break;
                case NodeType.EQUALS:
                    this.Equals(ast);
                    break;
                case NodeType.NOT_EQUALS:
                    this.NotEquals(ast);
                    break;
                case NodeType.LESS_THAN:
                    this.LessThan(ast);
                    break;
                case NodeType.GREATER_THAN:
                    this.GreaterThan(ast);
                    break;
                case NodeType.GREATER_THAN_OR_EQUAL:
                    this.GreaterThanOrEqual(ast);
                    break;
                case NodeType.LESS_THAN_OR_EQUAL:
                    this.LessThanOrEqual(ast);
                    break;
                case NodeType.RELATIONAL_IS:
                    this.RelationalIs(ast);
                    break;
                case NodeType.RELATIONAL_AS:
                    this.RelationalAs(ast);
                    break;
                case NodeType.LEFT_SHIFT:
                    this.LeftShift(ast);
                    break;
                case NodeType.RIGHT_SHIFT:
                    this.RightShift(ast);
                    break;
                case NodeType.ADD:
                    this.Add(ast);
                    break;
                case NodeType.SUBTRACT:
                    this.Subtract(ast);
                    break;
                case NodeType.MULTIPLY:
                    this.Multiply(ast);
                    break;
                case NodeType.DIVIDE:
                    this.Divide(ast);
                    break;
                case NodeType.REMAINDER:
                    this.Remainder(ast);
                    break;
                case NodeType.UNARY_MINUS:
                    this.UnaryMinus(ast);
                    break;
                case NodeType.UNARY_NOT:
                    this.UnaryNot(ast);
                    break;
                case NodeType.UNARY_COMPLEMENT:
                    this.UnaryComplement(ast);
                    break;
                case NodeType.PRE_INCREMENT:
                    this.PreIncrement(ast);
                    break;
                case NodeType.PRE_DECREMENT:
                    this.PreDecrement(ast);
                    break;
                case NodeType.POST_INCREMENT:
                    this.PostIncrement(ast);
                    break;
                case NodeType.POST_DECREMENT:
                    this.PostDecrement(ast);
                    break;
                case NodeType.CONSTANT_SIGNED_INT32:
                    this.ConstantSignedInt32(ast);
                    break;
                case NodeType.CONSTANT_UNSIGNED_INT32:
                    this.ConstantUnsignedInt32(ast);
                    break;
                case NodeType.CONSTANT_SIGNED_INT64:
                    this.ConstantSignedInt64(ast);
                    break;
                case NodeType.CONSTANT_UNSIGNED_INT64:
                    this.ConstantUnsignedInt64(ast);
                    break;
                case NodeType.CONSTANT_FLOAT32:
                    this.ConstantFloat32(ast);
                    break;
                case NodeType.CONSTANT_FLOAT64:
                    this.ConstantFloat64(ast);
                    break;
                case NodeType.CONSTANT_DECIMAL:
                    this.ConstantDecimal(ast);
                    break;
                case NodeType.CONSTANT_STRING:
                    this.ConstantString(ast);
                    break;
                case NodeType.CONSTANT_CHAR:
                    this.ConstantChar(ast);
                    break;
                case NodeType.CONSTANT_BOOLEAN:
                    this.ConstantBoolean(ast);
                    break;
                case NodeType.CONSTANT_NULL:
                    this.ConstantNull(ast);
                    break;
                case NodeType.MEMBER_ACCESS:
                    this.MemberAccess(ast);
                    break;
                case NodeType.MEMBER_ACCESS_NULL_CONDITIONAL:
                    this.MemberAccessNullConditional(ast);
                    break;
                case NodeType.MEMBER_ACCESS_POINTER_DEREFERENCE:
                    this.MemberAccessPointerDereference(ast);
                    break;
                case NodeType.ELEMENT_ACCESS:
                    this.ElementAccess(ast);
                    break;
                case NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL:
                    this.ElementAccessNullConditional(ast);
                    break;
                case NodeType.THIS_ACCESS:
                    this.ThisAccess(ast);
                    break;
                case NodeType.BASE_ACCESS:
                    this.BaseAccess(ast);
                    break;
                case NodeType.TYPEOF:
                    this.TypeOf(ast);
                    break;
                case NodeType.TYPEOF_VOID:
                    this.TypeOfVoid(ast);
                    break;
                case NodeType.SIZE_OF:
                    this.SizeOf(ast);
                    break;
                case NodeType.DEFAULT_TYPED:
                    this.DefaultTyped(ast);
                    break;
                case NodeType.DEFAULT_INFERRED:
                    this.DefaultInferred(ast);
                    break;
                case NodeType.INVOCATION:
                    this.Invocation(ast);
                    break;
                case NodeType.IDENTIFIER:
                    this.Identifier(ast);
                    break;
                case NodeType.NUMBERED_EXPRESSION_ARGUMENT:
                    this.NumberedExpressionArgument(ast);
                    break;
                case NodeType.NAMED_EXPRESSION_ARGUMENT:
                    this.NamedExpressionArgument(ast);
                    break;
                case NodeType.INSTANTIATE_TYPE:
                    this.InstantiateType(ast);
                    break;
                case NodeType.CHECKED:
                    this.Checked(ast);
                    break;
                case NodeType.UNCHECKED:
                    this.Unchecked(ast);
                    break;
                case NodeType.TYPE_CAST:
                    this.TypeCast(ast);
                    break;
                case NodeType.ADDRESS_OF:
                    this.AddressOf(ast);
                    break;
                case NodeType.DEREFERENCE_POINTER:
                    this.DereferencePointer(ast);
                    break;
                case NodeType.PARENTHESIZED_EXPRESSION:
                    this.ParenthesizedExpression(ast);
                    break;
                case NodeType.ARRAY_OF:
                    this.ArrayOf(ast);
                    break;
                case NodeType.PROPERTY_QUERY:
                    this.PropertyQuery(ast);
                    break;
                case NodeType.SIMPLE_ASSIGNMENT:
                    this.SimpleAssignment(ast);
                    break;
                default:
                    throw new NotImplementedException(ast.NodeType.ToString());
            }
        }

        protected abstract void TernaryConditional(ASTNode ast);
        protected abstract void NullCoalesce(ASTNode ast);
        protected abstract void LogicalOr(ASTNode ast);
        protected abstract void LogicalAnd(ASTNode ast);
        protected abstract void BitwiseInclusiveOr(ASTNode ast);
        protected abstract void BitwiseExclusiveOr(ASTNode ast);
        protected abstract void BitwiseAnd(ASTNode ast);
        protected abstract void Equals(ASTNode ast);
        protected abstract void NotEquals(ASTNode ast);
        protected abstract void LessThan(ASTNode ast);
        protected abstract void GreaterThan(ASTNode ast);
        protected abstract void GreaterThanOrEqual(ASTNode ast);
        protected abstract void LessThanOrEqual(ASTNode ast);
        protected abstract void RelationalIs(ASTNode ast);
        protected abstract void RelationalAs(ASTNode ast);
        protected abstract void LeftShift(ASTNode ast);
        protected abstract void RightShift(ASTNode ast);
        protected abstract void Add(ASTNode ast);
        protected abstract void Subtract(ASTNode ast);
        protected abstract void Multiply(ASTNode ast);
        protected abstract void Divide(ASTNode ast);
        protected abstract void Remainder(ASTNode ast);
        protected abstract void UnaryMinus(ASTNode ast);
        protected abstract void UnaryNot(ASTNode ast);
        protected abstract void UnaryComplement(ASTNode ast);
        protected abstract void PreIncrement(ASTNode ast);
        protected abstract void PreDecrement(ASTNode ast);
        protected abstract void PostIncrement(ASTNode ast);
        protected abstract void PostDecrement(ASTNode ast);
        protected abstract void ConstantSignedInt32(ASTNode ast);
        protected abstract void ConstantUnsignedInt32(ASTNode ast);
        protected abstract void ConstantSignedInt64(ASTNode ast);
        protected abstract void ConstantUnsignedInt64(ASTNode ast);
        protected abstract void ConstantFloat32(ASTNode ast);
        protected abstract void ConstantFloat64(ASTNode ast);
        protected abstract void ConstantDecimal(ASTNode ast);
        protected abstract void ConstantString(ASTNode ast);
        protected abstract void ConstantChar(ASTNode ast);
        protected abstract void ConstantBoolean(ASTNode ast);
        protected abstract void ConstantNull(ASTNode ast);
        protected abstract void MemberAccess(ASTNode ast);
        protected abstract void MemberAccessNullConditional(ASTNode ast);
        protected abstract void MemberAccessPointerDereference(ASTNode ast);
        protected abstract void ElementAccess(ASTNode ast);
        protected abstract void ElementAccessNullConditional(ASTNode ast);
        protected abstract void ThisAccess(ASTNode ast);
        protected abstract void BaseAccess(ASTNode ast);
        protected abstract void TypeOf(ASTNode ast);
        protected abstract void TypeOfVoid(ASTNode ast);
        protected abstract void SizeOf(ASTNode ast);
        protected abstract void DefaultTyped(ASTNode ast);
        protected abstract void DefaultInferred(ASTNode ast);
        protected abstract void Invocation(ASTNode ast);
        protected abstract void Identifier(ASTNode ast);
        protected abstract void InstantiateType(ASTNode ast);
        protected abstract void NumberedExpressionArgument(ASTNode ast);
        protected abstract void NamedExpressionArgument(ASTNode ast);
        protected abstract void Checked(ASTNode ast);
        protected abstract void Unchecked(ASTNode ast);
        protected abstract void TypeCast(ASTNode ast);
        protected abstract void AddressOf(ASTNode ast);
        protected abstract void DereferencePointer(ASTNode ast);
        protected abstract void ParenthesizedExpression(ASTNode ast);
        protected abstract void ArrayOf(ASTNode ast);
        protected abstract void PropertyQuery(ASTNode ast);
        protected abstract void SimpleAssignment(ASTNode ast);
    }
}
#endif