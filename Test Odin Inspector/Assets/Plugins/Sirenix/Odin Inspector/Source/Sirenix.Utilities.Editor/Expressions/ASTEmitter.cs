#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ASTEmitter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using UnityEditor;

    public class EmitContext
    {
        public bool IsStatic;
        public Type Type;
        public Type ReturnType;
        public Type[] Parameters;
        public string[] ParameterNames;
    }

#if !ODIN_LIMITED_VERSION
    [InitializeOnLoad]
    internal class ASTEmitter : ASTVisitor
    {
        private static class CommonMembers
        {
            public static readonly MethodInfo Object_ReferenceEquals = typeof(object).GetMethod("ReferenceEquals");
            public static readonly MethodInfo Object_ToString = typeof(object).GetMethod("ToString");
            public static readonly MethodInfo String_Concat = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
            public static readonly MethodInfo String_get_Chars = typeof(string).GetMethod("get_Chars", new Type[] { typeof(int) });
            public static readonly MethodInfo Type_GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

            public static readonly Type TypeOf_InspectorProperty = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Editor.InspectorProperty, Sirenix.OdinInspector.Editor");
            public static readonly MethodInfo InspectorProperty_PropertyQueryLookup = TypeOf_InspectorProperty.GetMethod("PropertyQueryLookup", Flags.StaticPrivate);

            public static readonly MethodInfo UnityObject_Equals = typeof(UnityEngine.Object).GetOperatorMethod(Operator.Equality, typeof(UnityEngine.Object), typeof(UnityEngine.Object));
        }

        private static readonly Type[] ActionDefinitions = new Type[]
        {
            typeof(ExpressionAction),
            typeof(ExpressionAction<>),
            typeof(ExpressionAction<,>),
            typeof(ExpressionAction<,,>),
            typeof(ExpressionAction<,,,>),
            typeof(ExpressionAction<,,,,>),
            typeof(ExpressionAction<,,,,,>),
            typeof(ExpressionAction<,,,,,,>),
            typeof(ExpressionAction<,,,,,,,>),
            typeof(ExpressionAction<,,,,,,,,>),
        };

        private static readonly Type[] FuncDefinitions = new Type[]
        {
            null,
            typeof(ExpressionFunc<>),
            typeof(ExpressionFunc<,>),
            typeof(ExpressionFunc<,,>),
            typeof(ExpressionFunc<,,,>),
            typeof(ExpressionFunc<,,,,>),
            typeof(ExpressionFunc<,,,,,>),
            typeof(ExpressionFunc<,,,,,,>),
            typeof(ExpressionFunc<,,,,,,,>),
            typeof(ExpressionFunc<,,,,,,,,>),
            typeof(ExpressionFunc<,,,,,,,,,>),
        };

        private static Dictionary<Type, Type> GenericResolutionMap = new Dictionary<Type, Type>();
        private static List<OverloadScore> OverloadScores = new List<OverloadScore>();

        public EmitContext Context;
        private List<Action<ILGenerator>> actions = new List<Action<ILGenerator>>();
        private bool isVisitingMembersForValueAssignment;

        static ASTEmitter()
        {
            IdentifierLookups.InitializeAsync();
        }

        public Delegate EmitMethod(string name, ASTNode ast, EmitContext context)
        {
            return EmitMethod(name, ast, context, null);
        }

        public Delegate EmitMethod(string name, ASTNode ast, EmitContext context, Type delegateType)
        {
            if (delegateType != null && typeof(Delegate).IsAssignableFrom(delegateType) == false)
            {
                throw new ArgumentException("delegateType must be a type of delegate.");
            }

            this.Context = context;
            if (this.Context.IsStatic || this.Context.Type.IsStatic())
            {
                this.Context.IsStatic = true;
            }

            this.actions.Clear();
            this.Visit(ast);

            Type expressionResult = ast.TypeOfValue;
            if (expressionResult == null)
            {
                throw new SyntaxException(ast, "Expression does not have a valid return value");
            }
            else if (context.ReturnType != null && expressionResult != context.ReturnType)
            {
                throw new InvalidOperationException("Expected return type of " + context.ReturnType + " but instead got " + expressionResult);
            }

            if (expressionResult == typeof(void))
            {
                expressionResult = null;
            }

            Type[] parameters;
            if (this.Context.IsStatic)
            {
                parameters = this.Context.Parameters ?? Type.EmptyTypes;
            }
            else
            {
                parameters = new Type[this.Context.Parameters != null ? this.Context.Parameters.Length + 1 : 1];
                parameters[0] = this.Context.Type;
                for (int i = 1; i < parameters.Length; i++)
                {
                    parameters[i] = this.Context.Parameters[i - 1];
                }
            }

            DynamicMethod method = new DynamicMethod(name, expressionResult, parameters, true);

            var il = method.GetILGenerator();
            for (int i = 0; i < this.actions.Count; i++)
            {
                this.actions[i](il);
            }

            il.Emit(OpCodes.Ret);

            if (delegateType == null)
            {
                if (expressionResult != null)
                {
                    var funcParameters = new Type[parameters.Length + 1];
                    funcParameters[funcParameters.Length - 1] = expressionResult;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        funcParameters[i] = parameters[i];
                    }

                    if (funcParameters.Length > 0 && funcParameters.Length < FuncDefinitions.Length)
                    {
                        delegateType = FuncDefinitions[funcParameters.Length].MakeGenericType(funcParameters);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid number of arguments: " + funcParameters.Length);
                    }
                }
                else
                {
                    if (parameters.Length < ActionDefinitions.Length)
                    {
                        delegateType = ActionDefinitions[parameters.Length].MakeGenericType(parameters);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid number of arguments: " + parameters.Length);
                    }
                }
            }

            return method.CreateDelegate(delegateType);
        }

        protected override void Add(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.Addition, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void AddressOf(ASTNode ast)
        {
            throw new SyntaxException(ast, "Address-of operator '&' is not supported");
        }

        protected override void ArrayOf(ASTNode ast)
        {
            Visit(ast.Children[0]);

            var type = ast.Children[0].NodeValue as Type;

            if (type == null) throw new SyntaxException(ast.Children[0], "Expected type identifier");

            int rank = (int)ast.NodeValue;

            ast.NodeValue = rank == 1 ? type.MakeArrayType() : type.MakeArrayType(rank);
        }

        protected override void BaseAccess(ASTNode ast)
        {
            if (Context.IsStatic) throw new SyntaxException(ast, "Cannot use 'base' in a static context");
            ast.TypeOfValue = Context.Type.BaseType;
            this.actions.Add(il => il.Emit(OpCodes.Ldarg_0));
        }

        protected override void BitwiseAnd(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.BitwiseAnd, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void BitwiseExclusiveOr(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.ExclusiveOr, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void BitwiseInclusiveOr(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.BitwiseOr, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void Checked(ASTNode ast)
        {
            throw new SyntaxException(ast, "The 'checked' keyword is not supported");
        }

        protected override void ConstantBoolean(ASTNode ast)
        {
            ast.TypeOfValue = typeof(bool);
            this.actions.Add((il) => EmitConstant(il, (bool)ast.NodeValue));
        }

        protected override void ConstantChar(ASTNode ast)
        {
            ast.TypeOfValue = typeof(char);
            this.actions.Add((il) => EmitConstant(il, (char)ast.NodeValue));
        }

        protected override void ConstantDecimal(ASTNode ast)
        {
            ast.TypeOfValue = typeof(decimal);
            this.actions.Add(il => EmitConstant(il, (decimal)ast.NodeValue));
        }

        protected override void ConstantFloat32(ASTNode ast)
        {
            ast.TypeOfValue = typeof(float);
            this.actions.Add((il) => EmitConstant(il, (float)ast.NodeValue));
        }

        protected override void ConstantFloat64(ASTNode ast)
        {
            ast.TypeOfValue = typeof(double);
            this.actions.Add((il) => EmitConstant(il, (double)ast.NodeValue));
        }

        protected override void ConstantNull(ASTNode ast)
        {
            ast.TypeOfValue = typeof(object);;
            this.actions.Add((il) => EmitConstant(il, null));
        }

        protected override void ConstantSignedInt32(ASTNode ast)
        {
            ast.TypeOfValue = typeof(int);
            this.actions.Add((il) => EmitConstant(il, (int)ast.NodeValue));
        }

        protected override void ConstantSignedInt64(ASTNode ast)
        {
            ast.TypeOfValue = typeof(long);
            this.actions.Add((il) => EmitConstant(il, (long)ast.NodeValue));
        }

        protected override void ConstantString(ASTNode ast)
        {
            ast.TypeOfValue = typeof(string);
            this.actions.Add((il) => EmitConstant(il, (string)ast.NodeValue));
        }

        protected override void ConstantUnsignedInt32(ASTNode ast)
        {
            ast.TypeOfValue = typeof(uint);
            this.actions.Add((il) => EmitConstant(il, (uint)ast.NodeValue));
        }

        protected override void ConstantUnsignedInt64(ASTNode ast)
        {
            ast.TypeOfValue = typeof(ulong);
            this.actions.Add((il) => EmitConstant(il, (ulong)ast.NodeValue));
        }

        protected override void DefaultInferred(ASTNode ast)
        {
            throw new SyntaxException(ast, "default values with inferred types are not supported yet");
        }

        protected override void DefaultTyped(ASTNode ast)
        {
            Visit(ast.Children[0]);

            var type = ast.Children[0].NodeValue as Type;

            if (type == null)
            {
                throw new SyntaxException(ast, "Expected type identifier after 'default('");
            }

            if (type.IsValueType)
            {
                if (type.IsPrimitive || type.IsEnum)
                {
                    this.actions.Add(il => EmitConstant(il, Activator.CreateInstance(type)));
                }
                else
                {
                    this.actions.Add(il =>
                    {
                        var loc = il.DeclareLocal(type);

                        il.Emit(OpCodes.Ldloca, loc);
                        il.Emit(OpCodes.Initobj, type);
                        il.Emit(OpCodes.Ldloc, loc);
                    });
                }
            }
            else
            {
                this.actions.Add((il) => il.Emit(OpCodes.Ldnull));
            }

            ast.TypeOfValue = type;
        }

        protected override void DereferencePointer(ASTNode ast)
        {
            throw new SyntaxException(ast, "Unsafe operations are not supported");
        }

        protected override void Divide(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.Division, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void ElementAccess(ASTNode ast)
        {
            ElementAccess(ast, false);
        }

        protected void ElementAccess(ASTNode ast, bool containerVisited)
        {
            // Emit container and all argument values
            for (int i = 0; i < ast.Children.Count; i++)
            {
                if (i == 0 && containerVisited) continue;

                Visit(ast.Children[i]);
            }

            bool isBaseAccess = ast.Children[0].NodeType == NodeType.BASE_ACCESS;
            Type container = ast.Children[0].TypeOfValue;
            Type[] elementArgs = new Type[ast.Children.Count - 1];

            for (int i = 1; i < ast.Children.Count; i++)
            {
                elementArgs[i - 1] = ast.Children[i].TypeOfValue;
            }

            if (container.IsArray)
            {
                int rank = container.GetArrayRank();
                var type = container.GetElementType();

                if (rank != elementArgs.Length)
                {
                    throw new SyntaxException(ast, "Expected '" + rank + "' element access arguments, but got '" + elementArgs.Length + "'");
                }

                for (int i = 0; i < elementArgs.Length; i++)
                {
                    if (!IsAnyInteger(elementArgs[i]))
                    {
                        throw new SyntaxException(ast.Children[i + 1], "Array indexer argument '" + i + "' is not any integer type '" + elementArgs.Length + "'");
                    }
                }

                if (rank == 1)
                {
                    this.actions.Add((il) => il.Emit(OpCodes.Ldelem, type));
                }
                else
                {
                    MethodInfo method = container.GetMethod("Get");
                    this.actions.Add((il) => EmitMethodCall(il, method, container));
                }

                ast.TypeOfValue = type;
            }
            else if (container == typeof(string))
            {
                if (elementArgs.Length != 1)
                {
                    throw new SyntaxException(ast, "Wrong number of arguments given for string element accessor (requires 1)");
                }

                bool convertType = false;

                if (elementArgs[0] != typeof(int))
                {
                    if (!elementArgs[0].IsCastableTo(typeof(int), requireImplicitCast: true))
                    {
                        throw new SyntaxException(ast, "The type '" + elementArgs[0] + "' is not implicitly castable to type 'int'");
                    }

                    convertType = true;
                }

                this.actions.Add(il =>
                {
                    if (convertType)
                    {
                        EmitTypeConversion(ast, il, elementArgs[0], typeof(int));
                    }

                    EmitMethodCall(il, CommonMembers.String_get_Chars, typeof(string));
                });

                ast.TypeOfValue = typeof(char);
            }
            else
            {
                MethodInfo method = container.GetMethod("get_Item", Flags.InstanceAnyVisibility, null, elementArgs, null);

                if (method == null)
                {
                    throw new SyntaxException(ast, "Type '" + container.GetNiceFullName() + "' does not have a compatible index accessor");
                }

                ast.TypeOfValue = method.ReturnType;
                this.actions.Add((il) =>
                {
                    EmitConvertParamsIfNecessary(ast, il, method, elementArgs);
                    EmitMethodCall(il, method, container, isBaseAccess);
                });
            }
        }

        protected override void ElementAccessNullConditional(ASTNode ast)
        {
            ValueAccessNullConditional(ast, () => ElementAccess(ast, true));
        }

        protected void ValueAccessNullConditional(ASTNode ast, Action valueAccess)
        {
            Visit(ast.Children[0]);

            var typeOfMaybeNullValue = ast.Children[0].TypeOfValue;

            if (typeOfMaybeNullValue == null) throw new SyntaxException(ast.Children[0], "Cannot access a static context via conditional operator '?.', use a normal '.' operator instead");

            bool isNullableValueType = false;

            if (typeOfMaybeNullValue.IsValueType)
            {
                if (!typeOfMaybeNullValue.IsGenericType || typeOfMaybeNullValue.GetGenericTypeDefinition() != typeof(Nullable<>)) throw new SyntaxException(ast.Children[0], "Cannot use null conditional operator on non-nullable value type '" + typeOfMaybeNullValue.GetNiceName() + "'");

                isNullableValueType = true;
            }

            Label nullCase = default(Label);

            this.actions.Add(il =>
            {
                EmitNullConditionalBefore(il, out nullCase, typeOfMaybeNullValue, isNullableValueType);
            });

                           // Value?
            valueAccess(); // AccessedValue

            this.actions.Add(il =>
            {
                EmitNullConditionalAfter(il, nullCase, ast.TypeOfValue);
            });
        }

        private static void EmitDefaultValueForType(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                if (type.IsPrimitive || type.IsEnum)
                {
                    EmitConstant(il, Activator.CreateInstance(type));
                }
                else
                {
                    var loc = il.DeclareLocal(type);

                    il.Emit(OpCodes.Ldloca, loc);
                    il.Emit(OpCodes.Initobj, type);
                    il.Emit(OpCodes.Ldloc, loc);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
        }

        protected override void NumberedExpressionArgument(ASTNode ast)
        {
            int argumentNumber = (int)ast.NodeValue;
            if (this.Context.Parameters == null || this.Context.Parameters.Length <= argumentNumber || argumentNumber < 0)
            {
                throw new SyntaxException(ast, "Invalid argument number " + argumentNumber);
            }

            this.actions.Add(il => il.Emit(OpCodes.Ldarg, argumentNumber + (this.Context.IsStatic ? 0 : 1)));
            ast.TypeOfValue = this.Context.Parameters[argumentNumber];
        }

        protected override void NamedExpressionArgument(ASTNode ast)
        {
            string argumentName = (string)ast.NodeValue;
            if (this.Context.ParameterNames == null)
            {
                throw new SyntaxException(ast, "Invalid expression argument name " + argumentName + "; no named expression arguments have been provided");
            }

            int parameterIndex = -1;

            for (int i = 0; i < this.Context.ParameterNames.Length; i++)
            {
                if (this.Context.ParameterNames[i] == argumentName)
                {
                    parameterIndex = i;
                    break;
                }
            }
            
            if (parameterIndex == -1)
                throw new SyntaxException(ast, "Invalid expression argument name " + argumentName + "; only the following named expression arguments are available: " + string.Join(", ", this.Context.ParameterNames.Where(n => n != null).Select(n => "$" + n).ToArray()));

            this.actions.Add(il => il.Emit(OpCodes.Ldarg, parameterIndex + (this.Context.IsStatic ? 0 : 1)));
            ast.TypeOfValue = this.Context.Parameters[parameterIndex];
        }

        protected override void Equals(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.Equality, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void GreaterThan(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.GreaterThan, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void GreaterThanOrEqual(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.GreaterThanOrEqual, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void Identifier(ASTNode ast)
        {
            Identifier(ast, null, false, !Context.IsStatic, true);
        }

        private void Identifier(ASTNode ast, MemberInfo contextMember, bool isBaseAccess, bool allowInstanceAccess, bool allowStaticAccess)
        {
            Type[] genericArgs = null;

            if (ast.Children.Count > 0)
            {
                genericArgs = new Type[ast.Children.Count];

                for (int i = 0; i < ast.Children.Count; i++)
                {
                    Visit(ast.Children[i]);
                    genericArgs[i] = (Type)ast.Children[i].NodeValue;
                }
            }

            bool stackIsPrepared = contextMember != null;
            contextMember = contextMember ?? Context.Type;

            MemberInfo member = ResolveIdentifier(ast, contextMember, (string)ast.NodeValue, stackIsPrepared, genericArgs);

            bool memberIsStatic = member is Type || member is NamespaceInfo || member.IsStatic();

            if (!(member is UnresolvedMethodOverload) && !(member is Type) && !(member is NamespaceInfo) && member.DeclaringType != null)
            {
                if (memberIsStatic && !allowStaticAccess)
                {
                    throw new SyntaxException(ast, "Cannot access static members via an instance");
                }
                else if (!memberIsStatic && !allowInstanceAccess)
                {
                    throw new SyntaxException(ast, "Cannot access instance members in a static context");
                }
            }

            if (member is UnresolvedMethodOverload)
            {
                (member as UnresolvedMethodOverload).SetAccess(allowInstanceAccess, allowStaticAccess);
            }
            
            if (member is FieldInfo)
            {
                FieldInfo field = member as FieldInfo;
                ast.TypeOfValue = field.FieldType;
                var fieldTypeIsValueType = ast.TypeOfValue.IsValueType;

                if (memberIsStatic)
                {
                    if (field.IsLiteral)
                    {
                        if (fieldTypeIsValueType && this.isVisitingMembersForValueAssignment)
                        {
                            throw new SyntaxException(ast, "Cannot set a value in a const");
                        }

                        this.actions.Add(il => EmitConstant(il, field.GetValue(null)));
                    }
                    else if (field.DeclaringType.IsEnum)
                    {
                        if (fieldTypeIsValueType && this.isVisitingMembersForValueAssignment)
                        {
                            throw new SyntaxException(ast, "Cannot set a value in an enum");
                        }

                        this.actions.Add(il => EmitEnumField(il, field));
                    }
                    else
                    {
                        if (fieldTypeIsValueType && this.isVisitingMembersForValueAssignment)
                        {
                            this.actions.Add((il) => il.Emit(OpCodes.Ldsflda, field));
                        }
                        else
                        {
                            this.actions.Add((il) => il.Emit(OpCodes.Ldsfld, field));
                        }
                    }
                }
                else
                {
                    var loadAddress = fieldTypeIsValueType && this.isVisitingMembersForValueAssignment;

                    this.actions.Add((il) =>
                    {
                        if (!stackIsPrepared)
                        {
                            il.Emit(OpCodes.Ldarg_0); // 'this' argument
                        }

                        if (loadAddress)
                        {
                            il.Emit(OpCodes.Ldflda, field);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldfld, field);
                        }
                    });
                }
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo property = member as PropertyInfo;
                var getMethod = property.GetGetMethod(true);

                if (getMethod == null) throw new SyntaxException(ast, "Property '" + property.Name + "' has no getter. Whoever wrote this property should question their own sanity, and seek professional help.");

                ast.TypeOfValue = property.PropertyType;
                bool propertyTypeIsValueType = ast.TypeOfValue.IsValueType;

                if (propertyTypeIsValueType && this.isVisitingMembersForValueAssignment)
                {
                    throw new SyntaxException(ast, "Cannot assign a value to members of a struct returned from a property getter.");
                }

                if (memberIsStatic)
                {
                    this.actions.Add((il) => EmitMethodCall(il, getMethod, null));
                }
                else
                {
                    this.actions.Add((il) =>
                    {
                        if (!stackIsPrepared)
                        {
                            il.Emit(OpCodes.Ldarg_0); // 'this' argument
                        }

                        EmitMethodCall(il, getMethod, (contextMember as Type) ?? getMethod.DeclaringType);
                    });
                }
            }
            else
            {
                if (!stackIsPrepared && member is UnresolvedMethodOverload)
                {
                    (member as UnresolvedMethodOverload).LoadThisInstance = il => il.Emit(OpCodes.Ldarg_0);
                }
                else if (!stackIsPrepared && !memberIsStatic && (member is MethodInfo || member is EventInfo))
                {
                    // Load the 'this' instance to call on later, here
                    this.actions.Add((il) => il.Emit(OpCodes.Ldarg_0));
                }

                if (isBaseAccess)
                {
                    ast.NodeValue = new BaseAccessMember(member);
                }
                else
                {
                    ast.NodeValue = member;
                }
            }
        }

        protected override void InstantiateType(ASTNode ast)
        {
            Visit(ast.Children[0]);

            var typeAst = ast.Children[0];

            Type type = typeAst.NodeValue as Type;

            if (type == null)
            {
                throw new SyntaxException(typeAst, "Expected type identifier after 'new'");
            }

            ast.TypeOfValue = type;

            LocalBuilder structLocal = null;

            if (type.IsValueType)
            {
                this.actions.Add(il =>
                {
                    structLocal = il.DeclareLocal(type);
                    il.Emit(OpCodes.Ldloca, structLocal);
                });
            }

            if (type.IsValueType && ast.Children.Count == 1)
            {
                this.actions.Add(il =>
                {
                    il.Emit(OpCodes.Initobj, type);
                    il.Emit(OpCodes.Ldloc, structLocal);
                });
                return;
            }

            for (int i = 1; i < ast.Children.Count; i++)
            {
                Visit(ast.Children[i]);
            }

            Type[] args = new Type[ast.Children.Count - 1];

            for (int i = 0; i < args.Length; i++)
            {
                var child = ast.Children[i + 1];

                if (child.NodeType == NodeType.CONSTANT_NULL)
                {
                    args[i] = typeof(NullParameter);
                }
                else
                {
                    args[i] = child.TypeOfValue;
                    if (args[i] == null) throw new SyntaxException(ast.Children[i + 1], "Expected a value expression");
                }
            }
            
            var constructors = type.GetConstructors(Flags.InstanceAnyVisibility);

            var useConstructor = (ConstructorInfo)FindOverload(ast, constructors, args, true, true);

            if (useConstructor == null)
            {
                throw new SyntaxException(ast, "Type '" + type.GetNiceFullName() + "' has no valid constructor overloads for the given parameters of (" + string.Join(", ", args.Select(n => n.GetNiceName()).ToArray()) + ") amongst the following candidates:\n\n" + string.Join("\n", constructors.Select(n => n.GetFullName()).ToArray()));
            }

            if (type.IsValueType)
            {
                this.actions.Add(il =>
                {
                    EmitConvertParamsIfNecessary(ast, il, useConstructor, args);
                    il.Emit(OpCodes.Call, useConstructor);
                    il.Emit(OpCodes.Ldloc, structLocal);
                });
            }
            else
            {
                this.actions.Add(il =>
                {
                    EmitConvertParamsIfNecessary(ast, il, useConstructor, args);
                    il.Emit(OpCodes.Newobj, useConstructor);
                });
            }
        }

        protected override void Invocation(ASTNode ast)
        {
            Invocation(ast, false);
        }

        protected void Invocation(ASTNode ast, bool isTypeCastDelegateHack)
        {
            bool isNullConditionalInvocation = false;

            if (ast.Children[0].NodeType == NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
            {
                // We'll handle null conditionality here
                isNullConditionalInvocation = true;
                ast.Children[0].NodeType = NodeType.MEMBER_ACCESS;
            }
            else if (ast.Children[0].NodeType == NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL)
            {
                // We'll handle null conditionality here
                isNullConditionalInvocation = true;
                ast.Children[0].NodeType = NodeType.ELEMENT_ACCESS;
            }

            if (isTypeCastDelegateHack && isNullConditionalInvocation)
            {
                throw new SyntaxException(ast, "Cannot perform a null conditional invocation on a delegate in this particular syntactical structure; please report an issue with the exact expression that is causing this issue so we can fix it. Thank you");
            }

            Label nullCase = default(Label);

            if (!isTypeCastDelegateHack)
            {
                // Evaluate method/lambda to call and all of its arguments - if it's an instance method/event/lambda, 
                // this will also load that instance onto the stack.

                Visit(ast.Children[0]);

                if (isNullConditionalInvocation)
                {
                    var contextType = ast.Children[0].Children[0].TypeOfValue;

                    bool isNullableValueType = false;

                    if (contextType.IsValueType)
                    {
                        if (!contextType.IsGenericType || contextType.GetGenericTypeDefinition() != typeof(Nullable<>)) throw new SyntaxException(ast.Children[0].Children[0], "Cannot null conditionally invoke a method on a non-nullable value type");

                        isNullableValueType = true;
                    }

                    this.actions.Add(il =>
                    {
                        EmitNullConditionalBefore(il, out nullCase, contextType, isNullableValueType);
                    });
                }

                for (int i = 1; i < ast.Children.Count; i++)
                {
                    Visit(ast.Children[i]);
                }
            }

            var invokee = ast.Children[0];
            bool isBaseAccess = false;
            
            MethodInfo method;

            var member = invokee.NodeValue;

            if (member is BaseAccessMember)
            {
                member = (member as BaseAccessMember).InnerMember;
                isBaseAccess = true;
            }

            if (member is EventInfo)
            {
                // Event invocation
                EventInfo @event = member as EventInfo;
                method = @event.GetRaiseMethod(true);
            }
            else if (member is MethodInfo)
            {
                // Method invocation
                method = member as MethodInfo;
            }
            else if (typeof(Delegate).IsAssignableFrom(invokee.TypeOfValue))
            {
                // Delegate invocation
                method = invokee.TypeOfValue.GetMethod("Invoke", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                if (method == null) throw new SyntaxException(ast, "Could not find Invoke method on delegate of type '" + invokee.TypeOfValue.GetNiceFullName() + "'");
            }
            else
            {
                throw new SyntaxException(ast, "Cannot do a method invocation on a value of type '" + invokee.TypeOfValue.GetNiceFullName() + "'");
            }

            Type[] parameterTypes = new Type[ast.Children.Count - 1];

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var child = ast.Children[i + 1];

                if (child.NodeType == NodeType.CONSTANT_NULL)
                {
                    parameterTypes[i] = typeof(NullParameter);
                }
                else
                {
                    parameterTypes[i] = child.TypeOfValue;
                }
            }

            if (method is UnresolvedMethodOverload)
            {
                var overload = method as UnresolvedMethodOverload;

                method = overload.ResolveOverload(ast, parameterTypes);

                if (method == null)
                {
                    throw new SyntaxException(ast, "Unable to find a method overload compatible with the given parameters of (" + string.Join(", ", parameterTypes.Select(n => n.GetNiceName()).ToArray()) + ") amongst the following candidates:\n\n" + overload.GetAllCandidatesString("\n"));
                }

                if (!method.IsStatic && overload.LoadThisInstance != null)
                {
                    // Unresolved method overloads will not have loaded a "this" instance, and therefore
                    // we need to load it now

                    this.actions.Add(il =>
                    {
                        var parameters = method.GetParameters();

                        LocalBuilder[] paramLocals = null;

                        if (parameters.Length > 0)
                        {
                            // We have to pop all method parameters off the stack to load a 'this' argument
                            paramLocals = new LocalBuilder[parameters.Length];

                            for (int i = 0; i < parameters.Length; i++)
                            {
                                paramLocals[i] = il.DeclareLocal(parameters[i].ParameterType);
                            }

                            for (int i = parameters.Length - 1; i >= 0; i--)
                            {
                                il.Emit(OpCodes.Stloc, paramLocals[i]);
                            }
                        }

                        overload.LoadThisInstance(il);

                        if (parameters.Length > 0)
                        {
                            // We have to push all method parameters back onto the stack now
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                il.Emit(OpCodes.Ldloc, paramLocals[i]);
                            }
                        }
                    });
                }
            }

            if (method.IsGenericMethodDefinition)
            {
                method = InferGenericMethod(ast, method, parameterTypes);
            }

            var methodParameters = method.GetParameters();

            if (parameterTypes.Length != methodParameters.Length)
            {
                throw new SyntaxException(ast, "Wrong number of parameters given; expected " + methodParameters.Length + " but got " + parameterTypes.Length);
            }

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var givenParam = parameterTypes[i];
                var methodParam = methodParameters[i].ParameterType;

                var valid = true;

                if (givenParam == typeof(NullParameter))
                {
                    if (methodParam.IsValueType) valid = false;
                }
                else if (!givenParam.IsCastableTo(methodParam, requireImplicitCast: true))
                {
                    valid = false;
                }

                if (!valid)
                {
                    throw new SyntaxException(ast.Children[i + 1], "The given parameter of type '" + givenParam.GetNiceFullName() + "' is not assignable or implicitly castable to '" + methodParam.GetNiceFullName() + "'");
                }
            }

            ast.TypeOfValue = method.ReturnType;

            Type instanceType = null;

            if (!method.IsStatic)
            {
                instanceType = ast.Children[0].GetHighestPushedStackType();
            }

            this.actions.Add((il) =>
            {
                EmitConvertParamsIfNecessary(ast, il, method, parameterTypes);
                EmitMethodCall(il, method, instanceType, isBaseAccess);
            });

            if (isNullConditionalInvocation)
            {
                this.actions.Add(il =>
                {
                    EmitNullConditionalAfter(il, nullCase, ast.TypeOfValue);
                });
            }
        }

        protected override void LeftShift(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.LeftShift, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void LessThan(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.LessThan, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void LessThanOrEqual(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.LessThanOrEqual, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void LogicalAnd(ASTNode ast)
        {
            Label earlyOut = default(Label);
            Label end = default(Label);

            this.actions.Add(il =>
            {
                earlyOut = il.DefineLabel();
                end = il.DefineLabel();
            });

            Visit(ast.Children[0]);

            this.actions.Add(il =>
            {
                if (ast.Children[0].TypeOfValue != typeof(bool))
                {
                    EmitTypeConversion(ast, il, ast.Children[0].TypeOfValue, typeof(bool));
                }

                il.Emit(OpCodes.Brfalse, earlyOut); // If the first condition was false, early out
            });

            Visit(ast.Children[1]);

            this.actions.Add(il =>
            {
                if (ast.Children[1].TypeOfValue != typeof(bool))
                {
                    EmitTypeConversion(ast, il, ast.Children[1].TypeOfValue, typeof(bool));
                }

                il.Emit(OpCodes.Br, end);       // Goto end (from child 1 eval)
                il.MarkLabel(earlyOut);         // Start early out
                il.Emit(OpCodes.Ldc_I4_0);      // Load 'false' onto stack
                il.MarkLabel(end);              // Start end
            });

            ast.TypeOfValue = typeof(bool);

            //this.EmitOperator(ast, Operator.LogicalAnd, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void LogicalOr(ASTNode ast)
        {
            Label earlyOut = default(Label);
            Label end = default(Label);

            this.actions.Add(il =>
            {
                earlyOut = il.DefineLabel();
                end = il.DefineLabel();
            });

            Visit(ast.Children[0]);

            this.actions.Add(il =>
            {
                if (ast.Children[0].TypeOfValue != typeof(bool))
                {
                    EmitTypeConversion(ast, il, ast.Children[0].TypeOfValue, typeof(bool));
                }

                il.Emit(OpCodes.Brtrue, earlyOut); // If the first condition was true, early out
            });

            Visit(ast.Children[1]);

            this.actions.Add(il =>
            {
                if (ast.Children[1].TypeOfValue != typeof(bool))
                {
                    EmitTypeConversion(ast, il, ast.Children[1].TypeOfValue, typeof(bool));
                }

                il.Emit(OpCodes.Br, end);       // Goto end (from child 1 eval)
                il.MarkLabel(earlyOut);         // Start early out
                il.Emit(OpCodes.Ldc_I4_1);      // Load 'true' onto stack
                il.MarkLabel(end);              // Start end
            });

            ast.TypeOfValue = typeof(bool);

            //this.EmitOperator(ast, Operator.LogicalOr, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void MemberAccess(ASTNode ast)
        {
            MemberAccess(ast, false);
        }

        protected void MemberAccess(ASTNode ast, bool parentVisited)
        {
            var parentAst = ast.Children[0];
            var identifierAst = ast.Children[1];

            if (!parentVisited)
            {
                Visit(parentAst);
            }

            bool allowInstanceAccess = false;
            bool allowStaticAccess = false;

            if (parentAst.TypeOfValue != null)
            {
                // There is an actual value loaded, this is an instance access
                allowInstanceAccess = true;
            }
            else
            {
                // There is no actual value loaded, we can only allow static
                allowStaticAccess = true;
            }

            Identifier(identifierAst, (parentAst.NodeValue as MemberInfo) ?? parentAst.TypeOfValue, parentAst.NodeType == NodeType.BASE_ACCESS, allowInstanceAccess, allowStaticAccess);

            ast.NodeValue = identifierAst.NodeValue;
            ast.TypeOfValue = identifierAst.TypeOfValue;
        }

        protected override void MemberAccessNullConditional(ASTNode ast)
        {
            ValueAccessNullConditional(ast, () => MemberAccess(ast, true));
        }

        protected override void MemberAccessPointerDereference(ASTNode ast)
        {
            throw new SyntaxException(ast, "Unsafe operations are not supported");
        }

        protected override void Multiply(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.Multiply, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void NotEquals(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.Inequality, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void NullCoalesce(ASTNode ast)
        {
            if (ast.Children[0].NodeType == NodeType.CONSTANT_NULL)
            {
                Visit(ast.Children[1]);
                ast.TypeOfValue = ast.Children[1].TypeOfValue;
                return;
            }

            Visit(ast.Children[0]);

            Label label = default(Label);

            this.actions.Add(il =>
            {
                label = il.DefineLabel();                       // (...), value_to_check

                il.Emit(OpCodes.Dup);                           // (...), value_to_check, value_to_check
                il.Emit(OpCodes.Brtrue, label);                 // (...), value_to_check
                il.Emit(OpCodes.Pop);                           // (...), 
            });

            Visit(ast.Children[1]);

            Type value = ast.Children[0].TypeOfValue,
                 replacement = ast.Children[1].TypeOfValue;

            if (!replacement.IsCastableTo(value, requireImplicitCast: true))
            {
                throw new SyntaxException(ast, "Value '" + replacement.GetNiceFullName() + "' is not assignable or implicitly castable to type '" + value.GetNiceFullName() + "'");
            }

            this.actions.Add(il =>
            {
                EmitTypeConversion(ast, il, replacement, value);
                il.Emit(OpCodes.Nop);
                il.MarkLabel(label);
            });

            ast.TypeOfValue = value;
        }

        protected override void ParenthesizedExpression(ASTNode ast)
        {
            Visit(ast.Children[0]);
            ast.TypeOfValue = ast.Children[0].TypeOfValue;
        }

        protected override void PostDecrement(ASTNode ast)
        {
            throw new SyntaxException(ast, "Post-decrement operations are not supported");
        }

        protected override void PostIncrement(ASTNode ast)
        {
            throw new SyntaxException(ast, "Post-increment operations are not supported");
        }

        protected override void PreDecrement(ASTNode ast)
        {
            throw new SyntaxException(ast, "Pre-decrement operations are not supported");
        }

        protected override void PreIncrement(ASTNode ast)
        {
            throw new SyntaxException(ast, "Pre-increment operations are not supported");
        }

        protected override void PropertyQuery(ASTNode ast)
        {
            int propertyArgIndex = -1;

            if (this.Context.ParameterNames != null)
            {
                for (int i = 0; i < this.Context.ParameterNames.Length; i++)
                {
                    if (this.Context.ParameterNames[i] == "property" && this.Context.Parameters[i] == CommonMembers.TypeOf_InspectorProperty)
                    {
                        propertyArgIndex = i;
                        break;
                    }
                }

            }

            if (propertyArgIndex < 0)
            {
                throw new SyntaxException(ast, "The property query operation is only supported in expressions with a named parameter 'property' of type 'InspectorProperty'.");
            }

            var method = CommonMembers.InspectorProperty_PropertyQueryLookup;
            var propertyName = (string)ast.NodeValue;

            if (!this.Context.IsStatic)
                propertyArgIndex++;

            this.actions.Add(il =>
            {
                il.Emit(OpCodes.Ldarg, propertyArgIndex);
                il.Emit(OpCodes.Ldstr, propertyName);
                il.Emit(OpCodes.Call, method);
            });

            ast.TypeOfValue = CommonMembers.TypeOf_InspectorProperty;
        }

        protected override void RelationalAs(ASTNode ast)
        {
            var valueAst = ast.Children[0];
            var typeAst = ast.Children[1];

            Visit(valueAst);
            Visit(typeAst);

            Type type = typeAst.NodeValue as Type;

            if (type == null)
            {
                throw new SyntaxException(typeAst, "Expected type identifier");
            }

            if (type.IsValueType)
            {
                throw new SyntaxException(typeAst, "Cannot use value types with 'as' keyword");
            }

            var valueType = valueAst.TypeOfValue;

            if (valueType.IsValueType)
            {
                if (type.IsAssignableFrom(valueType))
                {
                    this.actions.Add(il => il.Emit(OpCodes.Box, valueType));
                }
                else
                {
                    throw new SyntaxException(ast, "Cannot convert '" + valueType.GetNiceFullName() + "' to '" + type.GetNiceFullName() + "'");
                }
            }
            else
            {
                this.actions.Add(il => il.Emit(OpCodes.Isinst, type));
            }

            ast.TypeOfValue = type;
        }

        protected override void RelationalIs(ASTNode ast)
        {
            var valueAst = ast.Children[0];
            var typeAst = ast.Children[1];

            Visit(valueAst);
            Visit(typeAst);

            Type type = typeAst.NodeValue as Type;

            if (type == null)
            {
                throw new SyntaxException(typeAst, "Expected type identifier");
            }

            if (type.IsValueType)
            {
                throw new SyntaxException(typeAst, "Cannot use value types with 'is' keyword");
            }

            var valueType = valueAst.TypeOfValue;

            if (valueType.IsValueType)
            {
                this.actions.Add(il =>
                {
                    il.Emit(OpCodes.Pop);
                    EmitConstant(il, type.IsAssignableFrom(valueType));
                });
            }
            else
            {
                this.actions.Add(il =>
                {
                    il.Emit(OpCodes.Isinst, type);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Cgt_Un);
                });
            }

            ast.TypeOfValue = typeof(bool);
        }

        protected override void Remainder(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.Modulus, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void RightShift(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.RightShift, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void SimpleAssignment(ASTNode ast)
        {
            //throw new SyntaxException(ast, "Simple assignment AST:\n\n" + ast.ToPrettyPrint());

            var assignTo = ast.Children[0];
            var assignValue = ast.Children[1];

            if (assignTo.NodeType == NodeType.ELEMENT_ACCESS || assignTo.NodeType == NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL)
            {
                SimpleAssignment_ElementAccess(ast, assignTo, assignValue);
            }
            else if (assignTo.NodeType == NodeType.IDENTIFIER || assignTo.NodeType == NodeType.MEMBER_ACCESS || assignTo.NodeType == NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
            {
                SimpleAssignment_IdentifierAccess(ast, assignTo, assignValue);
            }
            else
            {
                throw new SyntaxException(assignTo, "Cannot assign a value to the expression on the left-hand side of the assignment; the left-hand expression is of kind '" + assignTo.NodeType + "'!");
            }
        }

        private void SimpleAssignment_IdentifierAccess(ASTNode ast, ASTNode assignTo, ASTNode assignValue)
        {
            ASTNode assignToIdentifier;
            bool allowInstanceAccess = false;
            bool allowStaticAccess = false;
            MemberInfo contextMember;

            bool ldArg0IfIsInstance = false;
            bool isNullConditionalAssignment = false;

            if (assignTo.NodeType == NodeType.IDENTIFIER)
            {
                assignToIdentifier = assignTo;
                allowInstanceAccess = !this.Context.IsStatic;
                allowStaticAccess = true;

                if (allowInstanceAccess)
                {
                    // We are in an instance context and accessing the "root" instance; we must prepare the stack later
                    ldArg0IfIsInstance = true;
                }

                contextMember = this.Context.Type;
            }
            else if (assignTo.NodeType == NodeType.MEMBER_ACCESS || assignTo.NodeType == NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
            {
                if (assignTo.NodeType == NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
                {
                    // We'll handle the null conditionality here
                    isNullConditionalAssignment = true;
                    assignTo.NodeType = NodeType.MEMBER_ACCESS;
                }

                var memberAccessParent = assignTo.Children[0];
                var memberAccessIdentifier = assignTo.Children[1];

                try
                {
                    this.isVisitingMembersForValueAssignment = true;
                    Visit(memberAccessParent);
                }
                finally
                {
                    this.isVisitingMembersForValueAssignment = false;
                }

                if (memberAccessParent.TypeOfValue != null)
                {
                    // There is an actual value loaded, this is an instance access
                    allowInstanceAccess = true;
                }
                else
                {
                    // There is no actual value loaded, we can only allow static
                    allowStaticAccess = true;
                }

                contextMember = (memberAccessParent.NodeValue as MemberInfo) ?? memberAccessParent.TypeOfValue;
                assignToIdentifier = memberAccessIdentifier;
            }
            else
            {
                throw new SyntaxException(assignTo, "Cannot assign a value to the expression on the left-hand side of the assignment!");
            }

            if (assignToIdentifier.Children.Count > 0)
            {
                throw new SyntaxException(assignTo, "Cannot assign a value to a generic identifier!");
            }

            MemberInfo member = ResolveIdentifier(assignToIdentifier, contextMember, (string)assignToIdentifier.NodeValue, true, null);

            if (member is UnresolvedMethodOverload || member is MethodInfo)
            {
                throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a method declaration!");
            }
            else if (member is Type)
            {
                throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a type declaration!");
            }
            else if (member is NamespaceInfo)
            {
                throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a namespace!");
            }
            else if (member is EventInfo)
            {
                throw new SyntaxException(assignToIdentifier, "Cannot assign a value to an event!");
            }

            var field = member as FieldInfo;
            var prop = member as PropertyInfo;

            if (field == null && prop == null)
            {
                throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a '" + member.GetType().Name + "'!");
            }

            bool memberIsStatic = member.IsStatic();

            if (memberIsStatic && !allowStaticAccess)
            {
                throw new SyntaxException(assignToIdentifier, "Cannot access static members via an instance");
            }
            else if (!memberIsStatic && !allowInstanceAccess)
            {
                throw new SyntaxException(assignToIdentifier, "Cannot access instance members in a static context");
            }

            if (!memberIsStatic && ldArg0IfIsInstance)
            {
                this.actions.Add(il => il.Emit(OpCodes.Ldarg_0));
            }

            Label nullCase = default(Label);

            if (isNullConditionalAssignment)
            {
                var contextType = contextMember as Type;

                if (contextType == null) throw new SyntaxException(assignTo, "Cannot assign to left-hand side of expression using a null conditional assignment");
                if (contextType.IsValueType) throw new SyntaxException(assignTo, "Cannot assign values to members of a value type using a null conditional assignment - not even to a nullable value type");

                this.actions.Add(il =>
                {
                    EmitNullConditionalBefore(il, out nullCase, contextType, false);
                });
            }

            // Now that the stack is prepared, we need to load the value to assign
            Visit(assignValue);

            if (assignValue.TypeOfValue == null)
            {
                throw new SyntaxException(assignValue, "Expression to assign does not evaluate to any value");
            }

            var typeOfMember = member.GetReturnType();

            if (assignValue.TypeOfValue == typeof(object) && IsConstantNull(assignValue))
            {
                if (typeOfMember.IsValueType)
                {
                    throw new SyntaxException(assignValue, "Cannot assign null to a value type '" + typeOfMember.GetNiceFullName() + "'");
                }
                else
                {
                    assignValue.TypeOfValue = typeOfMember;
                }
            }

            // This is for containing the final returned value
            LocalBuilder dupLocal = null;

            this.actions.Add(il =>
            {
                dupLocal = il.DeclareLocal(assignValue.TypeOfValue);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, dupLocal);

                EmitTypeConversion(assignValue, il, assignValue.TypeOfValue, typeOfMember);
            });

            if (field != null)
            {
                if (field.IsLiteral)
                {
                    throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a const field");
                }
                else if (field.DeclaringType.IsEnum)
                {
                    throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a const field");
                }
                else
                {
                    if (memberIsStatic)
                    {
                        this.actions.Add(il => il.Emit(OpCodes.Stsfld, field));
                    }
                    else
                    {
                        this.actions.Add(il => il.Emit(OpCodes.Stfld, field));
                    }
                }
            }
            else
            {
                var setter = prop.GetSetMethod(true);

                if (setter == null)
                {
                    throw new SyntaxException(assignToIdentifier, "Property '" + prop.Name + "' has no setter");
                }

                this.actions.Add(il => EmitMethodCall(il, setter, setter.DeclaringType, false));
            }

            // Load the assigned value, since this expression in the end evaluates to that
            this.actions.Add(il => il.Emit(OpCodes.Ldloc, dupLocal));

            if (isNullConditionalAssignment)
            {
                this.actions.Add(il =>
                {
                    EmitNullConditionalAfter(il, nullCase, assignValue.TypeOfValue);
                });
            }

            ast.TypeOfValue = assignValue.TypeOfValue;
        }

        private static void EmitNullConditionalAfter(ILGenerator il, Label nullCase, Type valueType)
        {
            var end = il.DefineLabel();

            il.Emit(OpCodes.Br, end);
            il.MarkLabel(nullCase);

            if (valueType != null && valueType != typeof(void))
            {
                EmitDefaultValueForType(il, valueType);
            }

            il.MarkLabel(end);
            il.Emit(OpCodes.Nop);
        }

        private static void EmitNullConditionalBefore(ILGenerator il, out Label nullCase, Type contextType, bool isNullableValueType)
        {
            nullCase = il.DefineLabel();

                                                                    // Context?

            if (isNullableValueType)
            {
                var getHasValueProp = contextType.GetProperty("HasValue");
                var getHasValueMethod = getHasValueProp.GetGetMethod(true);

                var valueLocal = il.DeclareLocal(contextType);

                il.Emit(OpCodes.Stloc, valueLocal);                 // --
                il.Emit(OpCodes.Ldloc, valueLocal);                 // Context?
                il.Emit(OpCodes.Ldloca, valueLocal);                // Context?, &Context?

                il.Emit(OpCodes.Call, getHasValueMethod);           // Context?, (0/1)
            }
            else
            {
                il.Emit(OpCodes.Dup);                               // Context?, Context?

                if (typeof(UnityEngine.Object).IsAssignableFrom(contextType))
                {
                    il.Emit(OpCodes.Ldnull);                                    // Context?, Context?, null
                    il.Emit(OpCodes.Call, CommonMembers.UnityObject_Equals);    // Context?, (0, 1)
                    il.Emit(OpCodes.Ldc_I4_0);                                  // Context?, (0, 1), 0
                    il.Emit(OpCodes.Ceq);                                       // Context?, (1, 0)
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);                                    // Context?, Context?, null
                    il.Emit(OpCodes.Ceq);                                       // Context?, (0, 1)
                    il.Emit(OpCodes.Ldc_I4_0);                                  // Context?, (0, 1), 0
                    il.Emit(OpCodes.Ceq);                                       // Context?, (1, 0)
                }
            }

            var notNullCase = il.DefineLabel();

            il.Emit(OpCodes.Brtrue, notNullCase);                   // Context?

            // Value is null
            il.Emit(OpCodes.Pop);                                   // 
            il.Emit(OpCodes.Br, nullCase);

            il.MarkLabel(notNullCase);
            il.Emit(OpCodes.Nop);
        }

        private void SimpleAssignment_ElementAccess(ASTNode ast, ASTNode assignTo, ASTNode assignValue)
        {
            bool isNullConditionalAccess = false;

            if (assignTo.NodeType == NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL)
            {
                // We'll handle null conditionality on this level
                isNullConditionalAccess = true;
                assignTo.NodeType = NodeType.ELEMENT_ACCESS;
            }

            try
            {
                this.isVisitingMembersForValueAssignment = true;
                Visit(assignTo.Children[0]);
            }
            finally
            {
                this.isVisitingMembersForValueAssignment = false;
            }

            Label nullCase = default(Label);

            if (isNullConditionalAccess)
            {
                var contextType = assignTo.Children[0].TypeOfValue;

                if (contextType.IsValueType) throw new SyntaxException(assignTo, "Cannot make a null conditional element assignment to a value type");

                this.actions.Add(il =>
                {
                    EmitNullConditionalBefore(il, out nullCase, contextType, false);
                });
            }

            // Emit all argument values
            for (int i = 1; i < assignTo.Children.Count; i++)
            {
                Visit(assignTo.Children[i]);
            }

            Type container = assignTo.Children[0].TypeOfValue;
            Type[] elementArgs = new Type[assignTo.Children.Count - 1];

            for (int i = 1; i < assignTo.Children.Count; i++)
            {
                elementArgs[i - 1] = assignTo.Children[i].TypeOfValue;
            }

            if (container == typeof(string))
            {
                throw new SyntaxException(ast, "Cannot write values to a string indexer");
            }

            // Now visit value to assign
            Visit(assignValue);

            if (assignValue.TypeOfValue == null)
            {
                throw new SyntaxException(assignValue, "Expression to assign does not evaluate to any value");
            }

            bool valueIsConstNull = false;

            if (assignValue.TypeOfValue == typeof(object) && IsConstantNull(assignValue))
            {
                valueIsConstNull = true;
            }

            // This is for containing the final returned value
            LocalBuilder dupLocal = null;

            this.actions.Add(il =>
            {
                dupLocal = il.DeclareLocal(assignValue.TypeOfValue);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, dupLocal);
            });

            if (container.IsArray)
            {
                int rank = container.GetArrayRank();
                var elementType = container.GetElementType();

                if (valueIsConstNull)
                {
                    if (elementType.IsValueType) throw new SyntaxException(assignValue, "Cannot assign null to value type '" + elementType.GetNiceFullName() + "'");
                    else assignValue.TypeOfValue = elementType;
                }

                if (rank != elementArgs.Length)
                {
                    throw new SyntaxException(assignTo, "Expected '" + rank + "' array indexer arguments, but got '" + elementArgs.Length + "'");
                }

                for (int i = 0; i < elementArgs.Length; i++)
                {
                    if (!IsAnyInteger(elementArgs[i]))
                    {
                        throw new SyntaxException(assignTo.Children[i + 1], "Array indexer argument '" + i + "' is not any integer type '" + elementArgs.Length + "'");
                    }
                }

                if (rank == 1)
                {
                    this.actions.Add((il) =>
                    {
                        EmitTypeConversion(assignValue, il, assignValue.TypeOfValue, elementType);
                        il.Emit(OpCodes.Stelem, elementType);
                        il.Emit(OpCodes.Ldloc, dupLocal);
                    });
                }
                else
                {
                    MethodInfo method = container.GetMethod("Set");
                    this.actions.Add((il) =>
                    {
                        EmitTypeConversion(assignValue, il, assignValue.TypeOfValue, elementType);
                        EmitMethodCall(il, method, container);
                        il.Emit(OpCodes.Ldloc, dupLocal);
                    });
                }
            }
            else
            {
                Type[] setArgs = new Type[elementArgs.Length + 1];

                Array.Copy(elementArgs, setArgs, elementArgs.Length);
                setArgs[setArgs.Length - 1] = assignValue.TypeOfValue;

                MethodInfo method = container.GetMethod("set_Item", Flags.InstanceAnyVisibility, null, setArgs, null);

                if (method == null)
                {
                    throw new SyntaxException(ast, "Type '" + container.GetNiceFullName() + "' does not have a compatible index accessor");
                }

                this.actions.Add((il) =>
                {
                    EmitConvertParamsIfNecessary(assignTo, il, method, setArgs);
                    EmitMethodCall(il, method, container, assignTo.Children[0].NodeType == NodeType.BASE_ACCESS, false);
                    il.Emit(OpCodes.Ldloc, dupLocal);
                });
            }

            if (isNullConditionalAccess)
            {
                this.actions.Add(il =>
                {
                    EmitNullConditionalAfter(il, nullCase, assignValue.TypeOfValue);
                });
            }

            ast.TypeOfValue = assignValue.TypeOfValue;
        }

        protected override void SizeOf(ASTNode ast)
        {
            Visit(ast.Children[0]);

            Type type = ast.Children[0].NodeValue as Type;

            if (type == null)
            {
                throw new SyntaxException(ast.Children[0], "Expected type identifier");
            }

            int size;

            try
            {
                size = Marshal.SizeOf(type);
            }
            catch (ArgumentException)
            {
                throw new SyntaxException(ast, "Cannot get the size of type '" + type.GetNiceFullName() + "'; it cannot be marshaled as an unmanaged structure");
            }

            this.actions.Add(il => il.Emit(OpCodes.Ldc_I4, size));
            ast.TypeOfValue = typeof(int);

        }

        protected override void Subtract(ASTNode ast)
        {
            this.EmitOperator(ast, Operator.Subtraction, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
        }

        protected override void TernaryConditional(ASTNode ast)
        {
            var condition = ast.Children[0];
            var value1 = ast.Children[1];
            var value2 = ast.Children[2];

            Visit(condition);

            var type = condition.TypeOfValue;

            if (type == null) throw new SyntaxException(condition, "Expected boolean value for ternary operator");

            if (type != typeof(bool))
            {
                if (!type.IsCastableTo(typeof(bool), true))
                {
                    throw new SyntaxException(condition, "Type '" + type.GetNiceFullName() + "' is not implicitly castable to type 'bool'");
                }

                this.actions.Add(il => EmitTypeConversion(condition, il, type, typeof(bool)));
            }

            Label secondValue = default(Label), 
                  end = default(Label);

            this.actions.Add(il =>
            {
                secondValue = il.DefineLabel();
                end = il.DefineLabel();

                il.Emit(OpCodes.Brtrue, secondValue);
            });

            Visit(value2);

            this.actions.Add(il =>
            {
                il.Emit(OpCodes.Br, end);
                il.Emit(OpCodes.Nop);
                il.MarkLabel(secondValue);
            });

            Visit(value1);

            this.actions.Add(il =>
            {
                il.Emit(OpCodes.Nop);
                il.MarkLabel(end);
            });

            if (value1.TypeOfValue != value2.TypeOfValue)
            {
                throw new SyntaxException(ast, "Expected same type as result values of ternary conditional, but got types '" + value1.TypeOfValue + "' and '" + value2.TypeOfValue + "'");
            }

            ast.TypeOfValue = value1.TypeOfValue;
        }

        protected override void ThisAccess(ASTNode ast)
        {
            if (Context.IsStatic) throw new SyntaxException(ast, "Cannot use 'this' in a static context");
            this.actions.Add(il => il.Emit(OpCodes.Ldarg_0));
            ast.TypeOfValue = Context.Type;
        }

        protected override void TypeCast(ASTNode ast)
        {
            var typeAst = ast.Children[0];
            var valueAst = ast.Children[1];

            Visit(typeAst);
            Visit(valueAst);

            Type castToType = typeAst.NodeValue as Type;

            if (castToType == null)
            {
                if (typeAst.TypeOfValue != null && typeof(Delegate).IsAssignableFrom(typeAst.TypeOfValue))
                {
                    Invocation(ast, true);
                    return;
                }

                throw new SyntaxException(typeAst, "Expected type identifier");
            }

            var valueType = valueAst.TypeOfValue;

            if (valueType == typeof(void))
            {
                throw new SyntaxException(ast, "Cannot cast type 'void' to anything");
            }

            if (IsAnyNumber(valueType) && IsAnyNumber(castToType))
            {
                this.actions.Add(il => EmitConvertToNumber(il, castToType));
            }
            else if (castToType.IsAssignableFrom(valueType))
            {
                if (valueType.IsValueType && !castToType.IsValueType)
                {
                    this.actions.Add(il => il.Emit(OpCodes.Box, valueType));
                }
            }
            else if (valueType.IsAssignableFrom(castToType))
            {
                this.actions.Add(il =>
                {
                    if (valueType.IsValueType && !castToType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, valueType);
                    }
                    
                    if (!valueType.IsValueType && castToType.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox_Any, castToType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Castclass, castToType);
                    }
                });
            }
            else
            {
                var castMethod = valueType.GetCastMethod(castToType, false);

                if (castMethod == null)
                {
                    throw new SyntaxException(ast, "There is no type conversion from '" + valueType.GetNiceFullName() + "' to '" + castToType.GetNiceFullName() + "'");
                }

                this.actions.Add(il => EmitMethodCall(il, castMethod, null));
            }

            ast.TypeOfValue = castToType;
        }

        protected override void TypeOf(ASTNode ast)
        {
            Visit(ast.Children[0]);

            Type type = ast.Children[0].NodeValue as Type;

            if (type == null)
            {
                throw new SyntaxException(ast.Children[0], "Expected type identifier");
            }

            this.actions.Add(il =>
            {
                il.Emit(OpCodes.Ldtoken, type);
                EmitMethodCall(il, CommonMembers.Type_GetTypeFromHandle, null);
            });

            ast.TypeOfValue = typeof(Type);
        }

        protected override void TypeOfVoid(ASTNode ast)
        {
            this.actions.Add(il =>
            {
                il.Emit(OpCodes.Ldtoken, typeof(void));
                EmitMethodCall(il, CommonMembers.Type_GetTypeFromHandle, null);
            });

            ast.TypeOfValue = typeof(Type);
        }

        protected override void UnaryComplement(ASTNode ast)
        {
            Visit(ast.Children[0]);

            var type = ast.Children[0].TypeOfValue;

            if (IsAnyNumber(type))
            {
                this.actions.Add(il => il.Emit(OpCodes.Not));
                ast.TypeOfValue = type;
            }
            else
            {
                var operatorMethod = type.GetOperatorMethod(Operator.BitwiseComplement);
                this.actions.Add(il => EmitMethodCall(il, operatorMethod, null));
                ast.TypeOfValue = operatorMethod.ReturnType;
            }
        }

        protected override void UnaryMinus(ASTNode ast)
        {
            Visit(ast.Children[0]);

            var type = ast.Children[0].TypeOfValue;

            if (IsAnyNumber(type))
            {
                if (type == typeof(ulong))
                {
                    throw new SyntaxException(ast, "Cannot apply unary operator '-' to type ulong");
                }

                this.actions.Add(il => il.Emit(OpCodes.Neg));

                if (IsFloatingPoint(type))
                {
                    ast.TypeOfValue = type;
                }
                else if (type == typeof(uint) || type == typeof(long))
                {
                    ast.TypeOfValue = typeof(long);
                }
                else
                {
                    ast.TypeOfValue = typeof(int);
                }
            }
            else
            {
                var operatorMethod = type.GetOperatorMethod(Operator.BitwiseComplement);

                if (operatorMethod == null)
                {
                    throw new SyntaxException(ast, "Type '" + type.GetNiceFullName() + "' has no method override for unary complement operator '~'");
                }

                this.actions.Add(il => EmitMethodCall(il, operatorMethod, null));
                ast.TypeOfValue = operatorMethod.ReturnType;
            }
        }

        protected override void UnaryNot(ASTNode ast)
        {
            Visit(ast.Children[0]);

            var type = ast.Children[0].TypeOfValue;

            if (type == typeof(bool))
            {
                this.actions.Add(il =>
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                });
                
                ast.TypeOfValue = typeof(bool);
            }
            else
            {
                MethodInfo castMethod;
                var operatorMethod = type.GetOperatorMethod(Operator.LogicalNot);

                if (operatorMethod != null)
                {
                    this.actions.Add(il => EmitMethodCall(il, operatorMethod, null));
                    ast.TypeOfValue = operatorMethod.ReturnType;
                }
                else if ((castMethod = type.GetCastMethod(typeof(bool), requireImplicitCast: true)) != null)
                {
                    this.actions.Add(il =>
                    {
                        EmitMethodCall(il, castMethod, null);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    });
                    ast.TypeOfValue = typeof(bool);
                }
                else
                {
                    throw new SyntaxException(ast, "Type '" + type.GetNiceFullName() + "' has no method override for unary not operator '!' and no implicit cast to bool.");
                }
            }
        }

        protected override void Unchecked(ASTNode ast)
        {
            throw new SyntaxException(ast, "The 'unchecked' keyword is not supported");
        }
        
    #region Identifier Utilities
        private static MemberInfo ResolveIdentifier(ASTNode ast, MemberInfo context, string identifier, bool onlyLocals, Type[] genericArgs)
        {
            IdentifierLookups.EnsureInitialized();

            // Check if it's a predefined type
            if (!onlyLocals)
            {
                Type type;
                if (IdentifierLookups.PredefinedTypes.TryGetValue(identifier, out type)) return type;
            }

            if (onlyLocals && context == null) throw new ArgumentException();

            string genericIdentifier = null;

            if (genericArgs != null)
                genericIdentifier = identifier + "`" + genericArgs.Length;

            if (context is Type)
            {
                var type = context as Type;
                
                // Search local members
                var members = type.GetAllMembers(identifier, Flags.StaticInstanceAnyVisibility).ToList();
                
                if (members.Count > 0)
                {
                    if (genericArgs != null)
                    {
                        List<MemberInfo> possibles = new List<MemberInfo>();

                        for (int i = 0; i < members.Count; i++)
                        {
                            MemberInfo genericResult;

                            if (IsMemberCompatibleWithGenericArgs(members[i], genericArgs, out genericResult))
                            {
                                possibles.Add(genericResult);
                            }
                        }

                        if (possibles.Count == 1) return possibles[0];
                        else if (possibles.Count > 1) return new UnresolvedMethodOverload(possibles.Cast<MethodInfo>().ToArray());
                    }
                    else if (members.Count == 1)
                    {
                        return members[0];
                    }
                    else if (members.All(m => m is MethodInfo))
                    {
                        return new UnresolvedMethodOverload(members.Cast<MethodInfo>().ToArray());
                    }
                    else if (members.All(m => m is PropertyInfo))
                    {
                        // They are all - we pick the first, as that will be the property that is 'furthest down' with the highest prioritized implementation
                        // We'll probably do a callvirt anyways, so it probably won't matter which one we pick.
                        return members[0];
                    }
                }

                // Is it the actual context type name? (For non-generic types - this doesn't work on generics)
                if (!type.IsGenericType && identifier == type.Name)
                {
                    return type;
                }

                // Search nested types
                {
                    string useIdentifier = identifier;

                    if (genericArgs != null)
                    {
                        useIdentifier = genericIdentifier;
                    }

                    Type nested = null;
                    Type current = type;

                    // Travel up the current nested scope, and keep checking each scope
                    while (nested == null && current != null)
                    {
                        nested = current.GetNestedType(useIdentifier, Flags.AnyVisibility);
                        current = current.DeclaringType;
                    }

                    if (nested != null)
                    {
                        if (genericArgs != null)
                        {
                            if (!nested.AreGenericConstraintsSatisfiedBy(genericArgs))
                            {
                                throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + nested.GetNiceFullName() + "'");
                            }

                            nested = nested.MakeGenericType(genericArgs);
                        }

                        return nested;
                    }
                }

                if (onlyLocals) goto END_OF_METHOD;

                string @namespace = type.Namespace ?? "";

                // Look for types with the identifier as a short name in the context's namespace
                Dictionary<string, Type> namespaceTypes;
                Type found;

                if (IdentifierLookups.TypesByNamespace.TryGetValue(@namespace, out namespaceTypes))
                {
                    if (genericArgs != null)
                    {
                        if (namespaceTypes.TryGetValue(identifier + "`" + genericArgs.Length, out found))
                        {
                            if (!found.AreGenericConstraintsSatisfiedBy(genericArgs))
                            {
                                throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + found.GetNiceFullName() + "'");
                            }

                            return found.MakeGenericType(genericArgs);
                        }
                    }
                    else
                    {
                        if (namespaceTypes.TryGetValue(identifier, out found))
                        {
                            return found;
                        }
                    }
                }
            }
            else if (context is NamespaceInfo)
            {
                NamespaceInfo @namespace = context as NamespaceInfo,
                              childNamespace;

                if (@namespace.ChildNamespaces.TryGetValue(identifier, out  childNamespace))
                {
                    return childNamespace;
                }

                Dictionary<string, Type> namespaceTypes;
                Type type;

                string useIdentifier = identifier;

                if (genericArgs != null)
                {
                    useIdentifier = identifier + "`" + genericArgs.Length;
                }

                if (IdentifierLookups.TypesByNamespace.TryGetValue(@namespace.FullName, out namespaceTypes) && namespaceTypes.TryGetValue(useIdentifier, out type))
                {
                    if (genericArgs != null)
                    {
                        if (!type.AreGenericConstraintsSatisfiedBy(genericArgs))
                        {
                            throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + type.GetNiceFullName() + "'");
                        }

                        type = type.MakeGenericType(genericArgs);
                    }

                    return type;
                }

                throw new SyntaxException(ast, "Could not find identifier '" + identifier + "' in namespace '" + @namespace.FullName + "'");
            }

            if (onlyLocals) goto END_OF_METHOD;

            // Look for root namespaces (important this happens before short name lookups
            if (genericArgs == null)
            {
                NamespaceInfo @namespace;

                // Look for whether it's a root namespace
                if (IdentifierLookups.RootNamespaces.TryGetValue(identifier, out @namespace))
                {
                    return @namespace;
                }
            }

            // Look for types with the identifier as a short name
            List<Type> typesWithShortName;
            {
                var useIdentifier = identifier;

                if (genericArgs != null)
                {
                    useIdentifier += "`" + genericArgs.Length;
                }

                if (IdentifierLookups.TypesByShortName.TryGetValue(useIdentifier, out typesWithShortName))
                {
                    if (typesWithShortName.Count == 1)
                    {
                        if (genericArgs != null)
                        {
                            if (!typesWithShortName[0].AreGenericConstraintsSatisfiedBy(genericArgs))
                            {
                                throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + typesWithShortName[0].GetNiceFullName() + "'");
                            }

                            return typesWithShortName[0].MakeGenericType(genericArgs);
                        }

                        return typesWithShortName[0];
                    }
                }
            }

            if (typesWithShortName != null && typesWithShortName.Count > 1)
            {
                throw new SyntaxException(ast, "There are " + typesWithShortName.Count + " eligible types with the name '" + identifier + "', please fully quality your type name as one of the following: \n\n" + string.Join("\n", typesWithShortName.Select(t => t.GetNiceFullName() + " (" + t.AssemblyQualifiedName + ")").ToArray()));
            }

            END_OF_METHOD:

            if (context != null)
            {
                if (context is Type)
                {
                    throw new SyntaxException(ast, "Unable to locate identifier '" + identifier + "' in context of type '" + (context as Type).GetNiceFullName() + "' (Note that extension methods are not supported)");
                }
                else if (context is NamespaceInfo)
                {
                    throw new SyntaxException(ast, "Unable to locate identifier '" + identifier + "' in context of namespace '" + (context as NamespaceInfo).FullName + "' (Note that extension methods are not supported)");
                }
            }
            
            throw new SyntaxException(ast, "Unable to locate identifier '" + identifier + "' (Note that extension methods are not supported)");
        }

        private static bool IsMemberCompatibleWithGenericArgs(MemberInfo member, Type[] args, out MemberInfo genericResult)
        {
            if (member is Type)
            {
                var type = member as Type;
                if (type.IsGenericType && type.AreGenericConstraintsSatisfiedBy(args))
                {
                    genericResult = type.MakeGenericType(args);
                    return true;
                }
            }
            else if (member is MethodInfo)
            {
                var method = member as MethodInfo;
                if (method.IsGenericMethod && method.AreGenericConstraintsSatisfiedBy(args))
                {
                    genericResult = method.MakeGenericMethod(args);
                    return true;
                }
            }

            genericResult = null;
            return false;
        }

        internal class NamespaceInfo : MemberInfo
        {
            private string name;

            public Dictionary<string, NamespaceInfo> ChildNamespaces = new Dictionary<string, NamespaceInfo>();

            public override MemberTypes MemberType { get { return MemberTypes.Custom; } }

            public readonly string FullName;

            public override string Name { get { return this.name; } }

            public override Type DeclaringType { get { throw new NotSupportedException(); } }

            public override Type ReflectedType { get { throw new NotSupportedException(); } }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return new object[0];
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return new object[0]; 
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return false;
            }

            public NamespaceInfo(string name, string fullName)
            {
                this.name = name;
                this.FullName = fullName;
            }
        }

        private class BaseAccessMember : MemberInfo
        {
            public BaseAccessMember(MemberInfo innerMember)
            {
                this.InnerMember = innerMember;
            }

            public readonly MemberInfo InnerMember;

            public override MemberTypes MemberType { get { throw new NotSupportedException(); } }

            public override string Name { get { throw new NotSupportedException(); } }

            public override Type DeclaringType { get { throw new NotSupportedException(); } }

            public override Type ReflectedType { get { throw new NotSupportedException(); } }

            public override object[] GetCustomAttributes(bool inherit) { throw new NotSupportedException(); }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit) { throw new NotSupportedException(); }

            public override bool IsDefined(Type attributeType, bool inherit) { throw new NotSupportedException(); }
        }

        private class UnresolvedMethodOverload : MethodInfo
        {
            private MethodInfo[] overloadCandidates;

            private bool allowInstanceAccess = true;
            private bool allowStaticAccess = true;

            public UnresolvedMethodOverload(MethodInfo[] overloadCandidates)
            {
                this.overloadCandidates = overloadCandidates;

                bool hasOverrides = false;

                for (int i = 0; i < this.overloadCandidates.Length; i++)
                {
                    var cand = this.overloadCandidates[i];

                    if (cand.GetBaseDefinition() != cand)
                    {
                        hasOverrides = true;
                        break;
                    }
                }

                if (hasOverrides)
                {
                    List<MethodInfo> list = new List<MethodInfo>(this.overloadCandidates);
                    List<MethodInfo> toRemove = new List<MethodInfo>();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var current = list[i];

                        while (current.GetBaseDefinition() != current)
                        {
                            current = current.GetBaseDefinition();
                            toRemove.Add(current);
                        }
                    }

                    for (int i = 0; i < toRemove.Count; i++)
                    {
                        list.Remove(toRemove[i]);
                    }

                    this.overloadCandidates = list.ToArray();
                }
            }
            
            public MethodInfo ResolveOverload(ASTNode ast, Type[] parameters)
            {
                return (MethodInfo)FindOverload(ast, this.overloadCandidates, parameters, allowInstanceAccess, allowStaticAccess);
            }

            public string GetAllCandidatesString(string separator)
            {
                return string.Join(separator, this.overloadCandidates.Select(n => n.GetFullName()).ToArray());
            }

            public Action<ILGenerator> LoadThisInstance;

            public override ICustomAttributeProvider ReturnTypeCustomAttributes { get { throw new NotSupportedException(); } }

            public override RuntimeMethodHandle MethodHandle { get { throw new NotSupportedException(); } }

            public override MethodAttributes Attributes { get { return MethodAttributes.Static; } } // Pretend to be static

            public override string Name { get { throw new NotSupportedException(); } }

            public override Type DeclaringType { get { throw new NotSupportedException(); } }

            public override Type ReflectedType { get { throw new NotSupportedException(); } }

            public override MethodInfo GetBaseDefinition() { throw new NotSupportedException(); }

            public override object[] GetCustomAttributes(bool inherit) { throw new NotSupportedException(); }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit) { throw new NotSupportedException(); }

            public override MethodImplAttributes GetMethodImplementationFlags() { throw new NotSupportedException(); }

            public override ParameterInfo[] GetParameters() { throw new NotSupportedException(); }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) { throw new NotSupportedException(); }

            public override bool IsDefined(Type attributeType, bool inherit) { throw new NotSupportedException(); }

            public void SetAccess(bool allowInstanceAccess, bool allowStaticAccess)
            {
                this.allowInstanceAccess = allowInstanceAccess;
                this.allowStaticAccess = allowStaticAccess;
            }
        }

        private struct OverloadScore
        {
            public int Score;
            public MethodBase Method;
        }

        private static bool CanAccessOverloadCandidate(MethodBase candidate, bool allowInstanceAccess, bool allowStaticAccess)
        {
            if (candidate is ConstructorInfo) return true;

            if (candidate.IsStatic)
            {
                return allowStaticAccess;
            }

            return allowInstanceAccess;
        }

        private static MethodBase FindOverload(ASTNode ast, MethodBase[] candidates, Type[] parameters, bool allowInstanceAccess, bool allowStaticAccess)
        {
            // Look for precise matches
            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (!CanAccessOverloadCandidate(candidate, allowInstanceAccess, allowStaticAccess)) continue;
                var candidateParams = candidate.GetParameters();
                if (candidateParams.Length != parameters.Length) continue;

                bool valid = true;

                for (int j = 0; j < parameters.Length; j++)
                {
                    if (candidateParams[j].ParameterType != parameters[j])
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid) continue;

                return candidate;
            }
            
            // Look for generic matches
            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];

                if (!CanAccessOverloadCandidate(candidate, allowInstanceAccess, allowStaticAccess)) continue;

                var methodInfo = candidate as MethodInfo;

                if (methodInfo == null || !candidate.IsGenericMethodDefinition) continue;

                var candidateParams = candidate.GetParameters();

                if (candidateParams.Length != parameters.Length) continue;

                bool valid = true;

                lock (GenericResolutionMap)
                {
                    GenericResolutionMap.Clear();

                    for (int j = 0; j < parameters.Length; j++)
                    {
                        var candParam = candidateParams[j].ParameterType;
                        var givenParam = parameters[j];

                        if (candParam.IsGenericParameter)
                        {
                            if (!candParam.GenericParameterIsFulfilledBy(givenParam))
                            {
                                valid = false;
                                break;
                            }

                            if (GenericResolutionMap.ContainsKey(candParam) && GenericResolutionMap[candParam] != givenParam)
                            {
                                valid = false;
                                break;
                            }

                            GenericResolutionMap[candParam] = givenParam;
                        }
                        else
                        {
                            if (!candParam.IsAssignableFrom(givenParam))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    if (!valid) continue;

                    var genericArgs = candidate.GetGenericArguments();

                    if (GenericResolutionMap.Count != genericArgs.Length) continue;

                    for (int j = 0; j < genericArgs.Length; j++)
                    {
                        genericArgs[j] = GenericResolutionMap[genericArgs[j]];
                    }

                    return methodInfo.MakeGenericMethod(genericArgs);
                }
            }

            lock (OverloadScores)
            {
                OverloadScores.Clear();

                // Look for polymorphic matches
                for (int i = 0; i < candidates.Length; i++)
                {
                    var candidate = candidates[i];
                    if (!CanAccessOverloadCandidate(candidate, allowInstanceAccess, allowStaticAccess)) continue;
                    var candidateParams = candidate.GetParameters();
                    if (candidateParams.Length != parameters.Length) continue;

                    var score = new OverloadScore() { Method = candidate };
                    
                    bool valid = true;

                    for (int j = 0; j < parameters.Length; j++)
                    {
                        var candParam = candidateParams[j].ParameterType;
                        var givenParam = parameters[j];

                        if (!candParam.IsValueType && givenParam == typeof(NullParameter))
                        {
                            continue;
                        }
                        else if (candParam.IsAssignableFrom(givenParam))
                        {
                            score.Score += givenParam.GetInheritanceDistance(candParam) + 1;
                        }
                        else if (givenParam.IsCastableTo(candParam, requireImplicitCast: true))
                        {
                            score.Score += 1;
                        }
                        else
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid) continue;

                    OverloadScores.Add(score);
                }

                if (OverloadScores.Count > 0)
                {
                    if (OverloadScores.Count > 1)
                    {
                        OverloadScores.Sort((a, b) => a.Score.CompareTo(b.Score));

                        if (OverloadScores[0].Score == OverloadScores[1].Score)
                        {
                            throw new SyntaxException(ast, "Ambiguous method overload match");
                        }
                    }

                    return OverloadScores[0].Method;
                }
            }

            // TODO: Params support
            // TODO: Optional parameter support
            // TODO: Named parameter support

            return null;
        }

        internal static class IdentifierLookups
        {
            private static volatile bool initialized = false;
            private static readonly object Initialize_LOCK = new object();

            private const string ASYNC_THREAD_NAME = "Async IdentifierLookups Initialization Thread";

            public static Dictionary<string, Type> PredefinedTypes = new Dictionary<string, Type>()
            {
                { "bool", typeof(bool) },
                { "byte", typeof(byte) },
                { "char", typeof(char) },
                { "decimal", typeof(decimal) },
                { "double", typeof(double) },
                { "float", typeof(float) },
                { "int", typeof(int) },
                { "long", typeof(long) },
                { "object", typeof(object) },
                { "sbyte", typeof(sbyte) },
                { "short", typeof(short) },
                { "string", typeof(string) },
                { "uint", typeof(uint) },
                { "ulong", typeof(ulong) },
                { "ushort", typeof(ushort) },
            };

            public static Dictionary<string, Dictionary<string, Type>> TypesByNamespace = new Dictionary<string, Dictionary<string, Type>>(2048);
            public static Dictionary<string, List<Type>> TypesByShortName = new Dictionary<string, List<Type>>(2048);
            public static Dictionary<string, NamespaceInfo> RootNamespaces = new Dictionary<string, NamespaceInfo>(64);

            public static void EnsureInitialized()
            {
                if (initialized) return;

                lock (Initialize_LOCK)
                {
                    if (initialized) { return; }

                    RunInitializeTask();
                }
            }

            public static void InitializeAsync()
            {
                if (initialized) return;

                var thread = new Thread(RunInitializeTask);
                thread.Name = ASYNC_THREAD_NAME;
                thread.Start();
            }

            private static void RunInitializeTask()
            {
                if (initialized) return;

                lock (Initialize_LOCK)
                {
                    if (initialized) return;

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.IsDynamic()) continue;

                        var assemblyName = assembly.GetName().Name;

                        if (assemblyName == "Mono.Cecil") continue;
                        if (assemblyName == "Boo.Lang") continue;

                        foreach (var type in assembly.SafeGetTypes())
                        {
                            if (type.IsNested) continue;

                            // This line causes crashes occasionally, as the attribute-related reflection .NET API's are not thread-safe in
                            // most versions of Unity.
                            //if (type.IsDefined(typeof(CompilerGeneratedAttribute), true)) continue;

                            if (type.IsNotPublic)
                            {
                                var flags = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);

                                if ((flags & AssemblyTypeFlags.OtherTypes) == AssemblyTypeFlags.OtherTypes)
                                {
                                    continue;
                                }
                            }

                            // Register namespace stuff
                            {
                                Dictionary<string, Type> types;

                                var @namespace = type.Namespace ?? "";

                                if (!TypesByNamespace.TryGetValue(@namespace, out types))
                                {
                                    RegisterNamespace(@namespace);
                                    types = new Dictionary<string, Type>();
                                    TypesByNamespace.Add(@namespace, types);
                                }

                                if (types.ContainsKey(type.Name))
                                {
                                    //UnityEngine.Debug.Log("Duplicate type " + type.Name + " -> " + type.AssemblyQualifiedName + " in namespace " + @namespace + ", the other type is " + types[type.Name].Name + " -> " + types[type.Name].AssemblyQualifiedName);
                                    continue;
                                }

                                types.Add(type.Name, type);
                            }

                            // Register by short name
                            {
                                List<Type> types;

                                if (!TypesByShortName.TryGetValue(type.Name, out types))
                                {
                                    types = new List<Type>();
                                    TypesByShortName.Add(type.Name, types);
                                }

                                types.Add(type);
                            }
                        }
                    }

                    initialized = true;
                }
            }

            private static void RegisterNamespace(string @namespace)
            {
                if (@namespace == "")
                {
                    RootNamespaces.Add("", new NamespaceInfo("", ""));
                    return;
                }

                var current = RootNamespaces;
                var names = @namespace.Split('.');
                string currentFullName = null;

                for (int i = 0; i < names.Length; i++)
                {
                    var name = names[i];

                    currentFullName = currentFullName == null ? name : currentFullName + "." + name;

                    NamespaceInfo n;

                    if (!current.TryGetValue(name, out n))
                    {
                        n = new NamespaceInfo(name, currentFullName);
                        current.Add(name, n);
                    }

                    current = n.ChildNamespaces;
                }
            }
        }

        private static MethodInfo InferGenericMethod(ASTNode ast, MethodInfo method, Type[] parameters)
        {
            if (!method.IsGenericMethodDefinition) throw new ArgumentException();

            var methodParams = method.GetParameters();

            if (parameters.Length != methodParams.Length)
                throw new SyntaxException(ast, "Wrong number of parameters given to generic method; expected " + methodParams.Length + " but got " + parameters.Length);

            lock (GenericResolutionMap)
            {
                GenericResolutionMap.Clear();

                for (int j = 0; j < parameters.Length; j++)
                {
                    var candParam = methodParams[j];
                    var givenParam = parameters[j];

                    if (candParam.ParameterType.IsGenericParameter)
                    {
                        if (!candParam.ParameterType.GenericParameterIsFulfilledBy(givenParam))
                        {
                            throw new SyntaxException(ast, "The constraints of generic parameter '" + candParam.Name + "' are not fulfilled by given type '" + givenParam.GetNiceFullName() + "'. The constraints are: " + candParam.ParameterType.GetGenericParameterConstraintsString());
                        }

                        if (GenericResolutionMap.ContainsKey(candParam.ParameterType) && GenericResolutionMap[candParam.ParameterType] != givenParam)
                        {
                            throw new SyntaxException(ast, "The generic parameter '" + candParam.Name + "' cannot be inferred to be both type '" + GenericResolutionMap[candParam.ParameterType].GetNiceFullName() + "' and '" + givenParam.GetNiceFullName() + "'.");
                        }

                        GenericResolutionMap[candParam.ParameterType] = givenParam;
                    }
                    else
                    {
                        if (!candParam.ParameterType.IsAssignableFrom(givenParam))
                        {
                            throw new SyntaxException(ast, "The given argument '" + givenParam.GetNiceFullName() + "' cannot be assigned to parameter '" + candParam.Name + "' of type '" + candParam.ParameterType.GetNiceFullName()  + "'.");
                        }
                    }
                }

                var genericArgs = method.GetGenericArguments();

                if (GenericResolutionMap.Count == genericArgs.Length)
                {
                    for (int j = 0; j < genericArgs.Length; j++)
                    {
                        genericArgs[j] = GenericResolutionMap[genericArgs[j]];
                    }

                    return method.MakeGenericMethod(genericArgs);
                }
            }

            throw new SyntaxException(ast, "Could not infer generic usage of method from given parameters");
        }
    #endregion Identifier Utilities

    #region Operator Utilities

        private static bool Is64BitInteger(Type type)
        {
            EnumConv(ref type);

            return type == typeof(long)
                || type == typeof(ulong);
        }

        private static bool Is32BitOrLessInteger(Type type)
        {
            EnumConv(ref type);

            return type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(int)
                || type == typeof(byte)
                || type == typeof(ushort)
                || type == typeof(uint);
        }

        private static bool IsSignedInteger(Type type)
        {
            EnumConv(ref type);

            return type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(int)
                || type == typeof(long);
        }

        private static bool IsAnyInteger(Type type)
        {
            EnumConv(ref type);

            return Is64BitInteger(type)
                || Is32BitOrLessInteger(type);
        }

        private static bool IsFloatingPoint(Type type)
        {
            EnumConv(ref type);

            return type == typeof(float)
                || type == typeof(double);
        }

        private static bool IsAnyNumber(Type type)
        { 
            EnumConv(ref type);

            return IsAnyInteger(type)
                || IsFloatingPoint(type);
        }

        private static bool IntegersHaveSameSign(Type a, Type b)
        {
            EnumConv(ref a);
            EnumConv(ref b);

            return IsAnyInteger(a)
                && IsAnyInteger(b)
                && (IsSignedInteger(a) == IsSignedInteger(b));
        }

        private static int GetBitCountOfNumber(Type type)
        {
            EnumConv(ref type);

            if (type == typeof(byte) || type == typeof(sbyte)) return 8;
            if (type == typeof(short) || type == typeof(ushort)) return 16;
            if (type == typeof(int) || type == typeof(uint) || type == typeof(float)) return 32;
            if (type == typeof(long) || type == typeof(ulong) || type == typeof(double)) return 64;

            throw new ArgumentException("Type '" + type.FullName + "' is not a number.");
        }

        private static void EnumConv(ref Type e)
        {
            if (e.IsEnum) e = Enum.GetUnderlyingType(e);
        }

        private Type GetAndValidateNumberMathOperationResult(ASTNode ast, Operator op, Type a, Type b)
        {
            EnumConv(ref a);
            EnumConv(ref b);

            if (IsNumberBooleanOperator(op)) return typeof(bool);

            int aBits = GetBitCountOfNumber(a),
                bBits = GetBitCountOfNumber(b);

            if (IsBitwiseOperator(op))
            {
                if (op == Operator.BitwiseComplement) throw new NotSupportedException();

                if (IsFloatingPoint(a) || IsFloatingPoint(b)) throw new SyntaxException(ast, "Cannot use bitwise operations on floating point numbers");

                if (op == Operator.RightShift || op == Operator.LeftShift)
                {
                    if (!Is32BitOrLessInteger(b) || b == typeof(uint)) throw new SyntaxException(ast, "Cannot bitshift by a number of type '" + b.GetNiceName() + "'");
                    return a;
                }

                // Bitwise and/or/exclusive or
                if (a == b) return a;
                if (aBits == 64 && bBits == 64 && !IntegersHaveSameSign(a, b)) throw new SyntaxException(ast, "Cannot apply bitwise and/or/exclusive or operations to numbers of type 'long' and 'ulong'");
                if (aBits == 64 || bBits == 64) return a;
                if (aBits < 32 && bBits < 32) return typeof(int);

                // We are dealing with two 32-bit numbers
                // There are three cases here

                bool aSigned = IsSignedInteger(a),
                     bSigned = IsSignedInteger(b);

                if (aSigned && bSigned)
                {
                    return typeof(int);
                }
                else if (!aSigned && !bSigned)
                {
                    return typeof(uint);
                }
                else // Differing signs
                {
                    return typeof(long);
                }
            }

            if (!IsMathOperator(op)) throw new NotImplementedException(op.ToString());

            Type highestBits = aBits > bBits ? a : b;

            if (IsFloatingPoint(a))
            {
                if (IsFloatingPoint(b)) return highestBits;
                return a;
            }

            if (IsFloatingPoint(b))
            {
                if (IsFloatingPoint(a)) return highestBits;
                return b;
            }

            // Both are integers
            if (!IntegersHaveSameSign(a, b))
            {

                if (aBits == 32 && bBits == 64 && !IsSignedInteger(b))
                {
                    return b;
                }
                else if (aBits == 64 && bBits == 32 && !IsSignedInteger(a))
                {
                    return aBits > bBits ? a : b;
                }

                throw new ArgumentException("Math operator (+-/*%) is ambiguous on operands of type '" + a.GetNiceName() + "' and '" + b.GetNiceName() + "'");
            }

            return aBits > bBits ? a : b;
        }

        private static void EmitConvertToNumber(ILGenerator il, Type to)
        {
            EnumConv(ref to);

            if (to == typeof(float))
                il.Emit(OpCodes.Conv_R4);
            else if (to == typeof(double))
                il.Emit(OpCodes.Conv_R8);
            else if (to == typeof(sbyte))
                il.Emit(OpCodes.Conv_I1);
            else if (to == typeof(short))
                il.Emit(OpCodes.Conv_I2);
            else if (to == typeof(int))
                il.Emit(OpCodes.Conv_I4);
            else if (to == typeof(long))
                il.Emit(OpCodes.Conv_I8);
            else if (to == typeof(byte))
                il.Emit(OpCodes.Conv_U1);
            else if (to == typeof(ushort))
                il.Emit(OpCodes.Conv_U2);
            else if (to == typeof(uint))
                il.Emit(OpCodes.Conv_U4);
            else if (to == typeof(ulong))
                il.Emit(OpCodes.Conv_U8);
            else throw new NotImplementedException(to.ToString());
        }

        private static void EmitOperatorOpCodes(ILGenerator il, Operator op)
        {
            switch (op)
            {
                case Operator.Equality:
                    il.Emit(OpCodes.Ceq);
                    break;
                case Operator.Inequality:
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case Operator.Addition:
                    il.Emit(OpCodes.Add);
                    break;
                case Operator.Subtraction:
                    il.Emit(OpCodes.Sub);
                    break;
                case Operator.Multiply:
                    il.Emit(OpCodes.Mul);
                    break;
                case Operator.Division:
                    il.Emit(OpCodes.Div);
                    break;
                case Operator.LessThan:
                    il.Emit(OpCodes.Clt);
                    break;
                case Operator.GreaterThan:
                    il.Emit(OpCodes.Cgt);
                    break;
                case Operator.LessThanOrEqual:
                    il.Emit(OpCodes.Cgt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case Operator.GreaterThanOrEqual:
                    il.Emit(OpCodes.Clt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case Operator.Modulus:
                    il.Emit(OpCodes.Rem);
                    break;
                case Operator.RightShift:
                    il.Emit(OpCodes.Shr);
                    break;
                case Operator.LeftShift:
                    il.Emit(OpCodes.Shl);
                    break;
                case Operator.BitwiseAnd:
                case Operator.LogicalAnd:
                    il.Emit(OpCodes.And);
                    break;
                case Operator.BitwiseOr:
                case Operator.LogicalOr:
                    il.Emit(OpCodes.Or);
                    break;
                case Operator.BitwiseComplement:
                    il.Emit(OpCodes.Not);
                    break;
                case Operator.ExclusiveOr:
                    il.Emit(OpCodes.Xor);
                    break;
                default:
                    throw new NotImplementedException(op.ToString());
            }
        }

        private void EmitOperator(ASTNode ast, Operator op, ASTNode leftAst, ASTNode rightAst, out Type resultType, bool useAwfulImplicitCastHack = false, Type leftOverride = null, Type rightOverride = null)
        {
            if (op == Operator.BitwiseComplement) throw new NotSupportedException();

            Type left, right;

            if (!useAwfulImplicitCastHack)
            {
                Visit(leftAst);                                                                 // (...), left_value
                Visit(rightAst);                                                                // (...), left_value, right_value

                left = leftAst.TypeOfValue;
                right = rightAst.TypeOfValue;
            }
            else
            {
                left = leftOverride;
                right = rightOverride;
            }
            
            // Strings have special, hardcoded concacenation behaviour
            if (op == Operator.Addition)
            {
                if (left == typeof(string) && right == typeof(string))
                {
                    this.actions.Add(il =>
                    {
                        // (...), left_value_string, right_value_string
                        il.Emit(OpCodes.Call, CommonMembers.String_Concat);                 // (...), concatenated_string
                    });

                    resultType = typeof(string);
                    return;
                }

                if (left == typeof(string))
                {
                    this.actions.Add((il) =>
                    {
                        // (...), left_value_string, right_value
                        if (right.IsValueType)
                        {
                            il.Emit(OpCodes.Box, right);                                    // (...), left_value_string, right_value_boxed
                        }

                        il.Emit(OpCodes.Callvirt, CommonMembers.Object_ToString);           // (...), left_value_string, right_value_string
                        il.Emit(OpCodes.Call, CommonMembers.String_Concat);                 // (...), concatenated_string
                    });

                    resultType = typeof(string);
                    return;
                }

                if (right == typeof(string))
                {
                    this.actions.Add((il) =>
                    {
                        var rightLocal = il.DeclareLocal(typeof(string));
                        // (...), left_value, right_value_string
                        il.Emit(OpCodes.Stloc, rightLocal);                                 // (...), left_value

                        if (left.IsValueType)
                        {
                            il.Emit(OpCodes.Box, left);                                     // (...), left_value_boxed
                        }

                        il.Emit(OpCodes.Callvirt, CommonMembers.Object_ToString);           // (...), left_value_string
                        il.Emit(OpCodes.Ldloc, rightLocal);                                 // (...), left_value_string, right_value_string
                        il.Emit(OpCodes.Call, CommonMembers.String_Concat);                 // (...), concatenated_string
                    });

                    resultType = typeof(string);
                    return;
                }
            }

            if (IsAnyNumber(left) && IsAnyNumber(right))
            {
                Type convertTo;

                if (left.IsEnum && right.IsEnum)
                {
                    if (left != right)
                    {
                        throw new SyntaxException(ast, "Cannot use the " + op + " operator on enums of differing types");
                    }

                    if (IsNumberBooleanOperator(op))
                    {
                        resultType = typeof(bool);
                    }
                    else
                    {
                        resultType = left;
                    }

                    convertTo = left;

                    EnumConv(ref convertTo);
                }
                else if (left.IsEnum && !right.IsEnum)
                {
                    if (IsFloatingPoint(right))
                    {
                        throw new SyntaxException(ast, "Cannot perform math operations with enums and floating points numbers");
                    }

                    Type enumNumberType = Enum.GetUnderlyingType(left);
                    Type numResult = GetAndValidateNumberMathOperationResult(ast, op, enumNumberType, right);
                    
                    if (numResult != enumNumberType)
                    {
                        if (!IntegersHaveSameSign(numResult, enumNumberType))
                        {
                            throw new SyntaxException(ast, "Cannot perform math operations with an enum of the underlying type '" + enumNumberType.GetNiceName() + "' and '" + right + "'");
                        }
                    }

                    convertTo = enumNumberType;
                    resultType = left;
                }
                else if (!left.IsEnum && right.IsEnum)
                {
                    throw new SyntaxException(ast, "Can only do math operations with enums if the enum is on the left-hand side");
                }
                else
                {
                    resultType = GetAndValidateNumberMathOperationResult(ast, op, left, right);
                    convertTo = resultType;
                }

                if (IsNumberBooleanOperator(op))
                {
                    if (IsFloatingPoint(left) && IsAnyInteger(right))
                    {
                        this.actions.Add((il) =>
                        {
                                                                                                // (...), left_value, right_value
                            EmitConvertToNumber(il, left);                                      // (...), left_value, right_value_converted
                            EmitOperatorOpCodes(il, op);                                        // (...), operation_result
                        });
                    }
                    else if (IsAnyInteger(left) && IsFloatingPoint(right))
                    {
                        this.actions.Add((il) =>
                        {
                            var rightLocal = il.DeclareLocal(right);
                                                                                                // (...), left_value, right_value
                            il.Emit(OpCodes.Stloc, rightLocal);                                 // (...), left_value
                            EmitConvertToNumber(il, right);                                     // (...), left_value_converted
                            il.Emit(OpCodes.Ldloc, rightLocal);                                 // (...), left_value_converted, right_value
                            EmitOperatorOpCodes(il, op);                                        // (...), operation_result
                        });
                    }
                    else
                    {

                        this.actions.Add((il) =>
                        {
                                                                                                // (...), left_value, right_value
                            EmitOperatorOpCodes(il, op);                                        // (...), operation_result
                        });
                    }
                }
                else if (IsBitwiseOperator(op))
                {
                    this.actions.Add((il) =>
                    {
                        // (...), left_value, right_value
                        EmitOperatorOpCodes(il, op);                                        // (...), operation_result
                    });
                }
                else
                {
                    this.actions.Add((il) =>
                    {
                        if (left != right)
                        {
                            var rightLocal = il.DeclareLocal(right);
                                                                                            // (...), left_value, right_value
                            il.Emit(OpCodes.Stloc, rightLocal);                             // (...), left_value
                            EmitConvertToNumber(il, convertTo);                             // (...), left_value_converted
                            il.Emit(OpCodes.Ldloc, rightLocal);                             // (...), left_value_converted, right_value
                            EmitConvertToNumber(il, convertTo);                             // (...), left_value_converted, right_value_converted
                        }
                        EmitOperatorOpCodes(il, op);                                        // (...), operation_result
                    });
                }
                return;
            }

            if (IsLogicalBooleanOperator(op))
            {
                if (left == typeof(bool) && right == typeof(bool))
                {
                    resultType = typeof(bool);

                    this.actions.Add((il) =>
                    {
                        // (...), left_value, right_value
                        EmitOperatorOpCodes(il, op);                                        // (...), operation_result
                    });
                    return;
                }
            }
            
            // In operations on reference types and a null constant, null is coerced to the type of the other operand object
            if (!left.IsValueType && IsConstantNull(rightAst))
            {
                right = left;
            }
            else if (!right.IsValueType && IsConstantNull(leftAst))
            {
                left = right;
            }

            var operatorMethod = GetOperatorMethod(op, left, right);

            if (operatorMethod != null)
            {
                this.actions.Add((il) =>
                {
                    EmitConvertParamsIfNecessary(ast, il, operatorMethod, left, right);          // (...), left_value, right_value
                    EmitMethodCall(il, operatorMethod, null);                                    // (...), operator_result
                });

                resultType = operatorMethod.ReturnType;
                return;
            }

            if (right != left) // No need to check the other way around in this case
            {
                operatorMethod = GetOperatorMethod(op, right, left);

                if (operatorMethod != null)
                {
                    this.actions.Add((il) =>
                    {
                        var leftLocal = il.DeclareLocal(left);
                        var rightLocal = il.DeclareLocal(right);                                // (...), left_value, right_value

                        il.Emit(OpCodes.Stloc, rightLocal);                                     // (...), left_value
                        il.Emit(OpCodes.Stloc, leftLocal);                                      // (...)
                        il.Emit(OpCodes.Ldloc, rightLocal);                                     // (...), right_value
                        il.Emit(OpCodes.Ldloc, leftLocal);                                      // (...), right_value, left_value

                        EmitConvertParamsIfNecessary(ast, il, operatorMethod, right, left);
                        EmitMethodCall(il, operatorMethod, null);                               // (...), operator_result
                    });

                    resultType = operatorMethod.ReturnType;
                    return;
                }
            }
            
            // ==/!= with System.Object on either end becomes a simple reference comparison
            if ((left == typeof(object) || right == typeof(object)) && (op == Operator.Equality || op == Operator.Inequality))
            {
                this.actions.Add(il =>
                {
                    il.Emit(OpCodes.Ceq);                       // (...), left_value, right_value

                    if (op == Operator.Inequality)
                    {
                        il.Emit(OpCodes.Ldc_I4_0);              // (...), 0/1, 0
                        il.Emit(OpCodes.Ceq);                   // (...), 1/0 (inverse)
                    }
                });

                resultType = typeof(bool);
                return;
            }

            // Check if we can implicitly cast either operand to a type that this operator can use with the other operand.
            // Only do this if we're not already in one of those check. We only support one "level" of implicit cast operator coercion support.
            if (!useAwfulImplicitCastHack)
            {
                Type nonCastedOperand,
                     castedOperand;

                for (int i = 0; i < 2; i++)
                {
                    castedOperand = i == 0 ? left : right;
                    nonCastedOperand = i == 0 ? right : left;

                    var casts = GetImplicitTypeCastTargets(castedOperand);

                    if (casts == null) continue;

                    foreach (var cast in casts)
                    {
                        int actionsCountBefore = this.actions.Count;

                        // Awful, awful hack, but this is the best I got.
                        // 
                        // In essence, we try running the EmitOperator method
                        // on each potential weakly cast type, and if it works,
                        // then set things in the stack up so it can work properly.
                        // If it doesn't work, we catch the exception, roll back
                        // any changes made by the method call, and try the next
                        // option.

                        Type tempResultType;

                        try
                        {
                            EmitOperator(ast, op, leftAst, rightAst, out tempResultType, useAwfulImplicitCastHack: true, leftOverride: i == 0 ? cast : nonCastedOperand, rightOverride: i == 0 ? nonCastedOperand : cast);
                        }
                        catch (SyntaxException)
                        {
                            // Failure, roll back and move on
                            while (this.actions.Count > actionsCountBefore)
                            {
                                this.actions.RemoveAt(this.actions.Count - 1);
                            }

                            continue;
                        }

                        // Success! Now we inject a type cast in before the actions
                        // that were set by the successful EmitOperator call.
                        if (i == 0)
                        {
                            // Cast left operand
                            this.actions.Insert(actionsCountBefore, il =>
                            {
                                var rightLocal = il.DeclareLocal(right);                    // (...), left_value, right_value
                                il.Emit(OpCodes.Stloc, rightLocal);                         // (...), left_value
                                EmitTypeConversion(rightAst, il, left, cast);               // (...), left_value_cast
                                il.Emit(OpCodes.Ldloc, rightLocal);                         // (...), left_value_cast, right_value
                            });
                        }
                        else
                        {
                            // Cast right operand
                            this.actions.Insert(actionsCountBefore, il =>                   // (...), left_value, right_value
                            {
                                EmitTypeConversion(rightAst, il, right, cast);              // (...), left_value, right_value_cast
                            });
                        }

                        resultType = tempResultType;
                        return;
                    }
                }
            }

            // ==/!= with reference types on both ends and no custom equality operators defined becomes a simple reference comparison.
            if ((op == Operator.Equality || op == Operator.Inequality) && (!left.IsValueType && !right.IsValueType))
            {
                this.actions.Add(il =>
                {
                    il.Emit(OpCodes.Ceq);                       // (...), left_value, right_value

                    if (op == Operator.Inequality)
                    {
                        il.Emit(OpCodes.Ldc_I4_0);              // (...), 0/1, 0
                        il.Emit(OpCodes.Ceq);                   // (...), 1/0 (inverse)
                    }
                });

                resultType = typeof(bool);
                return;
            }

            throw new SyntaxException(ast, "No valid " + op.ToString().ToLower() + " operators found for types '" + left.GetNiceFullName() + "' and '" + right.GetNiceFullName() + "'");
        }

        private static MethodInfo GetOperatorMethod(Operator op, Type left, Type right)
        {
            if (!IsOverridableOperator(op)) return null;

            return left.GetOperatorMethods(op).FirstOrDefault(m => IsValidOperatorFor(m, left, right))
                ?? right.GetOperatorMethods(op).FirstOrDefault(m => IsValidOperatorFor(m, left, right));
        }

        private static bool IsValidOperatorFor(MethodInfo method, Type left, Type right)
        {
            var parameters = method.GetParameters();

            if (parameters.Length != 2) return false;
            if (method.ReturnType == null || method.ReturnType == typeof(void)) return false;
            if (!left.IsCastableTo(parameters[0].ParameterType, requireImplicitCast: true)) return false;
            if (!right.IsCastableTo(parameters[1].ParameterType, requireImplicitCast: true)) return false;

            return true;
        }

        private static bool IsLogicalBooleanOperator(Operator op)
        {
            switch (op)
            {
                case Operator.Equality:
                case Operator.Inequality:
                case Operator.LogicalAnd:
                case Operator.LogicalOr:
                case Operator.ExclusiveOr:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsNumberBooleanOperator(Operator op)
        {
            switch (op)
            {
                case Operator.Equality:
                case Operator.Inequality:
                case Operator.LessThan:
                case Operator.GreaterThan:
                case Operator.LessThanOrEqual:
                case Operator.GreaterThanOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsMathOperator(Operator op)
        {
            switch (op)
            {
                case Operator.Addition:
                case Operator.Subtraction:
                case Operator.Multiply:
                case Operator.Division:
                case Operator.Modulus:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsBitwiseOperator(Operator op)
        {
            switch (op)
            {
                case Operator.LeftShift:
                case Operator.RightShift:
                case Operator.BitwiseAnd:
                case Operator.BitwiseOr:
                case Operator.BitwiseComplement:
                case Operator.ExclusiveOr:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsOverridableOperator(Operator op)
        {
            switch (op)
            {
                case Operator.LessThanOrEqual:
                case Operator.GreaterThanOrEqual:
                case Operator.LogicalAnd:
                case Operator.LogicalOr:
                    return false;
                default:
                    return true;
            }
        }
    #endregion Operator Utilities

        private void EmitConvertParamsIfNecessary(ASTNode ast, ILGenerator il, MethodBase method, params Type[] args)
        {
            var methodParams = method.GetParameters();

            bool anyConversionNecessary = false;

            for (int i = 0; i < args.Length; i++)
            {
                Type param = methodParams[i].ParameterType,
                     arg = args[i];

                if (param == arg) continue;
                if (!param.IsValueType && !arg.IsValueType) continue;

                anyConversionNecessary = true;
            }

            if (!anyConversionNecessary) return;

            LocalBuilder[] locals = new LocalBuilder[args.Length]; 

            for (int i = 0; i < args.Length; i++)
            {
                locals[i] = il.DeclareLocal(args[i]);
            }

            for (int i = args.Length - 1; i >= 0; i--)
            {
                il.Emit(OpCodes.Stloc, locals[i]);
            }

            for (int i = 0; i < args.Length; i++)
            {
                il.Emit(OpCodes.Ldloc, locals[i]);
                EmitTypeConversion(ast, il, args[i], methodParams[i].ParameterType);
            }
        }

        private static void EmitTypeConversion(ASTNode ast, ILGenerator il, Type from, Type to)
        {
            if (from == to) return;
            if (from == typeof(NullParameter)) return;

            if (IsAnyNumber(from) && IsAnyNumber(to))
            {
                EmitConvertToNumber(il, to);
                return;
            }

            if (from.IsValueType)
            {
                if (to == typeof(object) || to == typeof(ValueType) || to.IsInterface || (from.IsEnum && to == typeof(Enum)))
                {
                    il.Emit(OpCodes.Box, from);
                    return;
                }
            }
            else if (to.IsAssignableFrom(from))
            {
                return;
            }
            
            var method = from.GetCastMethod(to, requireImplicitCast: true);

            if (method == null)
            {
                throw new SyntaxException(ast, "The type '" + from.GetNiceFullName() + "' is not implicitly castable to type '" + to.GetNiceFullName() + "'");
            }

            EmitMethodCall(il, method, null);
        }

        private static void EmitMethodCall(ILGenerator il, MethodInfo method, Type instanceType, bool isBaseAccess = false, bool fixValueTypeLoading = true)
        {
            if (method.IsStatic)
            {
                il.Emit(OpCodes.Call, method);
                return;
            }

            if (instanceType == null) instanceType = method.DeclaringType;

            if (fixValueTypeLoading && instanceType.IsValueType)
            {
                // Instance methods on value types are not invoked on the values themselves,
                // but on addresses to the location of the value in memory.
                var loc = il.DeclareLocal(instanceType);

                var parameters = method.GetParameters();

                LocalBuilder[] paramLocals = null;

                if (parameters.Length > 0)
                {
                    // We have to pop all method parameters off the stack to mess with the 'this' argument
                    paramLocals = new LocalBuilder[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        paramLocals[i] = il.DeclareLocal(parameters[i].ParameterType);
                    }

                    for (int i = parameters.Length - 1; i >= 0; i--)
                    {
                        il.Emit(OpCodes.Stloc, paramLocals[i]);
                    }
                }

                il.Emit(OpCodes.Stloc, loc);
                il.Emit(OpCodes.Ldloca, loc);

                if (parameters.Length > 0)
                {
                    // We have to push all method parameters back onto the stack now that we're done messing with the 'this' argument
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        il.Emit(OpCodes.Ldloc, paramLocals[i]);
                    }
                }
            }

            if (instanceType.IsEnum && instanceType.IsValueType)
            {
                // Method invocation on non-boxed enum needs a constrained opcode beforehand
                il.Emit(OpCodes.Constrained, instanceType);
            }

            if (!isBaseAccess && (method.IsVirtual || method.IsAbstract))
            {
                il.Emit(OpCodes.Callvirt, method);
            }
            else
            {
                il.Emit(OpCodes.Call, method);
            }
        }

        private static void EmitConstant(ILGenerator il, object constant, Type type = null)
        {
            unchecked
            {
                if (constant == null)
                {
                    il.Emit(OpCodes.Ldnull);
                    return;
                }

                if (type == null)
                {
                    type = constant.GetType();
                }

                if (type == typeof(int)
                    || type == typeof(byte)
                    || type == typeof(sbyte)
                    || type == typeof(short)
                    || type == typeof(ushort))
                {
                    il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(constant));
                }
                else if (type == typeof(uint))
                {
                    il.Emit(OpCodes.Ldc_I4, (int)(uint)constant);
                }
                else if (type == typeof(long))
                {
                    il.Emit(OpCodes.Ldc_I8, (long)constant);
                }
                else if (type == typeof(ulong))
                {
                    il.Emit(OpCodes.Ldc_I8, (long)(ulong)constant);
                }
                else if (type == typeof(float))
                {
                    il.Emit(OpCodes.Ldc_R4, (float)constant);
                }
                else if (type == typeof(double))
                {
                    il.Emit(OpCodes.Ldc_R8, (double)constant);
                }
                else if (type == typeof(string))
                {
                    il.Emit(OpCodes.Ldstr, (string)constant);
                }
                else if (type == typeof(char))
                {
                    il.Emit(OpCodes.Ldc_I4, (int)(char)constant);
                }
                else if (type == typeof(decimal))
                {
                    var bits = decimal.GetBits((decimal)constant);
                    var constructor = typeof(decimal).GetConstructor(new Type[] { typeof(int[]) });

                    var arrLocal = il.DeclareLocal(typeof(int[]));

                    il.Emit(OpCodes.Ldc_I4, bits.Length);                   // (...), bits_length
                    il.Emit(OpCodes.Newarr, typeof(int));                   // (...), bits_array
                    il.Emit(OpCodes.Stloc, arrLocal);                       // (...)

                    for (int i = 0; i < bits.Length; i++)
                    {
                        il.Emit(OpCodes.Ldloc, arrLocal);                   // (...), bits_array
                        il.Emit(OpCodes.Ldc_I4, i);                         // (...), bits_array, i
                        il.Emit(OpCodes.Ldc_I4, bits[i]);                   // (...), bits_array, i, bits[i]
                        il.Emit(OpCodes.Stelem_I4);                         // (...), 
                    }

                    il.Emit(OpCodes.Ldloc, arrLocal);                       // (...), bits_array
                    il.Emit(OpCodes.Newobj, constructor);                     // (...), decimal
                }
                else if (type == typeof(bool))
                {
                    il.Emit((bool)constant ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                }
                else if (type.IsEnum)
                {
                    EmitConstant(il, constant, Enum.GetUnderlyingType(type));
                }
                else
                {
                    throw new NotSupportedException("Type " + type.GetNiceFullName() + " is not supported as a constant.");
                }
            }
        }

        private static void EmitEnumField(ILGenerator il, FieldInfo field)
        {
            var value = field.GetValue(null);
            var num = Enum.GetUnderlyingType(field.DeclaringType);

            if (num == typeof(ulong))
            {
                il.Emit(OpCodes.Ldc_I8, (long)(ulong)value);
            }
            else if (num == typeof(long))
            {
                il.Emit(OpCodes.Ldc_I8, (long)value);
            }
            else if (num == typeof(uint))
            {
                il.Emit(OpCodes.Ldc_I4, (int)(uint)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
            }
        }

        private static bool IsConstantNull(ASTNode node)
        {
            while (node.NodeType == NodeType.PARENTHESIZED_EXPRESSION)
                node = node.Children[0];

            return node.NodeType == NodeType.CONSTANT_NULL;
        }

        private static List<Type> GetImplicitTypeCastTargets(Type type)
        {
            if (type == typeof(object)) return null;

            List<Type> result = null;

            var methods = type.GetAllMembers<MethodInfo>(Flags.StaticAnyVisibility);

            foreach (var method in methods)
            {
                if (method.Name != "op_Implicit") continue;
                var parameters = method.GetParameters();
                if (parameters.Length != 1) continue;
                if (parameters[0].ParameterType != method.DeclaringType) continue;

                if (result == null) result = new List<Type>();

                result.Add(method.ReturnType);
            }

            return result;
        }
        
        private static class NullParameter
        {
        }
    }
#endif
}
#endif