#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityPropertyEmitter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Provides utilities for emitting ScriptableObject and MonoBehaviour-derived types with specific property names and types, and providing instances of <see cref="SerializedProperty"/> with those names and types.
    /// </summary>
    public static class UnityPropertyEmitter
    {
        public const string EMIT_ASSEMBLY_NAME = "Sirenix.OdinInspector.EmittedUnityProperties";
        public const string HOST_GO_NAME = "ODIN_EMIT_HOST_GO_ac922281-4f8a-4e1b-8a45-65af1a8350b3";
        public const HideFlags HOST_GO_HIDE_FLAGS = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

        private static AssemblyBuilder emittedAssembly;
        private static ModuleBuilder emittedModule;

        private static readonly Dictionary<Type, Type> PreCreatedScriptableObjectTypes = new Dictionary<Type, Type>()
        {
            {  typeof(AnimationCurve), typeof(EmittedAnimationCurveContainer) },
            {  typeof(Gradient), typeof(EmittedGradientContainer) },
        };

        // In order: field name, value type, emitted type
        private static readonly DoubleLookupDictionary<string, Type, Type> MonoBehaviourTypeCache = new DoubleLookupDictionary<string, Type, Type>();

        // In order: field name, value type, emitted type
        private static readonly DoubleLookupDictionary<string, Type, Type> ScriptableObjectTypeCache = new DoubleLookupDictionary<string, Type, Type>();

        private static GameObject hostGO;

        private static GameObject HostGO
        {
            get
            {
                if (hostGO == null)
                {
                    hostGO = GameObject.Find(HOST_GO_NAME);

                    if (hostGO == null)
                    {
                        hostGO = new GameObject(HOST_GO_NAME);
                        hostGO.hideFlags = HOST_GO_HIDE_FLAGS;
                    }
                }

                return hostGO;
            }
        }

        private static readonly object MarkedForDestruction_LOCK = new object();
        private static readonly List<UnityEngine.Object> MarkedForDestruction = new List<UnityEngine.Object>();

        static UnityPropertyEmitter()
        {
            EditorApplication.update += DestroyMarkedObjects;
        }

        /// <summary>
        /// A handle for a set of emitted Unity objects. When disposed (or collected by the GC) this handle will queue the emitted object instances for destruction.
        /// </summary>
        public class Handle : IDisposable
        {
            /// <summary>
            /// The unity property to represent.
            /// </summary>
            public readonly SerializedProperty UnityProperty;

            /// <summary>
            /// The Unity objects to represent.
            /// </summary>
            public readonly UnityEngine.Object[] Objects;

            private int disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handle"/> class.
            /// </summary>
            /// <param name="unityProperty">The unity property to represent.</param>
            /// <param name="objects">The objects to represent.</param>
            public Handle(SerializedProperty unityProperty, UnityEngine.Object[] objects)
            {
                this.UnityProperty = unityProperty;
                this.Objects = objects;
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="Handle"/> class.
            /// </summary>
            ~Handle()
            {
                this.Dispose();
            }

            public void Dispose()
            {
                //Debug.Log("Dispose trigger");

                if (Interlocked.Increment(ref this.disposed) == 1)
                {
                    lock (MarkedForDestruction_LOCK)
                    {
                        //Debug.Log("Actually disposing");
                        MarkedForDestruction.AddRange(this.Objects);
                    }
                }
            }
        }

        private static void DestroyMarkedObjects()
        {
            lock (MarkedForDestruction_LOCK)
            {
                for (int i = 0; i < MarkedForDestruction.Count; i++)
                {
                    var obj = MarkedForDestruction[i];

                    if (obj != null)
                    {
                        //Debug.Log("Actually destroying " + obj.name);
                        UnityEngine.Object.DestroyImmediate(obj);
                    }
                }

                MarkedForDestruction.Clear();
            }
        }

        /// <summary>
        /// Creates an emitted MonoBehaviour-based <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="fieldName">Name of the field to emit.</param>
        /// <param name="valueType">Type of the value to create a property for.</param>
        /// <param name="targetCount">The target count of the tree to create a property for.</param>
        /// <param name="gameObject">The game object that the MonoBehaviour of the property is located on.</param>
        /// <exception cref="System.ArgumentNullException">
        /// fieldName is null
        /// or
        /// valueType is null
        /// </exception>
        /// <exception cref="System.ArgumentException">Target count must be equal to or higher than 1.</exception>
        public static Handle CreateEmittedMonoBehaviourProperty(string fieldName, Type valueType, int targetCount, ref GameObject gameObject)
        {
            DestroyMarkedObjects();

            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (targetCount < 1)
            {
                throw new ArgumentException("Target count must be equal to or higher than 1.");
            }

            if (gameObject == null)
            {
                gameObject = HostGO;
            }

            Type resultType;

            if (!MonoBehaviourTypeCache.TryGetInnerValue(fieldName, valueType, out resultType))
            {
                resultType = EmitMonoBehaviourType(fieldName, valueType);
                MonoBehaviourTypeCache.AddInner(fieldName, valueType, resultType);
            }

            MonoBehaviour[] targets = new MonoBehaviour[targetCount];

            for (int i = 0; i < targetCount; i++)
            {
                targets[i] = (MonoBehaviour)gameObject.AddComponent(resultType);
                targets[i].hideFlags = gameObject.hideFlags;
            }

            var serializedObject = new SerializedObject(targets);
            return new Handle(serializedObject.FindProperty(fieldName), targets);
        }

        /// <summary>
        /// Creates an emitted ScriptableObject-based <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="fieldName">Name of the field to emit.</param>
        /// <param name="valueType">Type of the value to create a property for.</param>
        /// <param name="targetCount">The target count of the tree to create a property for.</param>
        /// <exception cref="System.ArgumentNullException">
        /// fieldName is null
        /// or
        /// valueType is null
        /// </exception>
        /// <exception cref="System.ArgumentException">Target count must be equal to or higher than 1.</exception>
        public static SerializedProperty CreateEmittedScriptableObjectProperty(string fieldName, Type valueType, int targetCount)
        {
            DestroyMarkedObjects();

            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (targetCount < 1)
            {
                throw new ArgumentException("Target count must be equal to or higher than 1.");
            }

            Type resultType;

            if (PreCreatedScriptableObjectTypes.TryGetValue(valueType, out resultType))
            {
                fieldName = "value";
            }
            else if (!ScriptableObjectTypeCache.TryGetInnerValue(fieldName, valueType, out resultType))
            {
                resultType = EmitScriptableObjectType(fieldName, valueType);
                ScriptableObjectTypeCache.AddInner(fieldName, valueType, resultType);
            }

            ScriptableObject[] targets = new ScriptableObject[targetCount];

            for (int i = 0; i < targetCount; i++)
            {
                targets[i] = ScriptableObject.CreateInstance(resultType);
            }

            var serializedObject = new SerializedObject(targets);
            return serializedObject.FindProperty(fieldName);
        }

        private static void EnsureEmitModule()
        {
            if (emittedAssembly == null)
            {
                FixUnityAboutWindowBeforeEmit.Fix();

                var assemblyName = new AssemblyName(EMIT_ASSEMBLY_NAME);

                assemblyName.CultureInfo = System.Globalization.CultureInfo.InvariantCulture;
                assemblyName.Flags = AssemblyNameFlags.None;
                assemblyName.ProcessorArchitecture = ProcessorArchitecture.MSIL;
                assemblyName.VersionCompatibility = System.Configuration.Assemblies.AssemblyVersionCompatibility.SameDomain;

                emittedAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                emittedModule = emittedAssembly.DefineDynamicModule(EMIT_ASSEMBLY_NAME, emitSymbolInfo: true);
            }
        }

        private static Type EmitMonoBehaviourType(string memberName, Type valueType)
        {
            string typeName = EMIT_ASSEMBLY_NAME + ".EmittedMBProperty_" + memberName + "_" + valueType.GetCompilableNiceFullName();
            Type inheritedType = typeof(EmittedMonoBehaviour<>).MakeGenericType(valueType);

            return EmitType(memberName, typeName, inheritedType, valueType);
        }

        private static Type EmitScriptableObjectType(string memberName, Type valueType)
        {
            string typeName = EMIT_ASSEMBLY_NAME + ".EmittedSOProperty_" + memberName + "_" + valueType.GetCompilableNiceFullName();
            Type inheritedType = typeof(EmittedScriptableObject<>).MakeGenericType(valueType);

            return EmitType(memberName, typeName, inheritedType, valueType);
        }

        private static Type EmitType(string memberName, string typeName, Type inheritedType, Type valueType)
        {
            EnsureEmitModule();

            MethodInfo abstractSetValueMethod = inheritedType.GetMethod("SetValue");
            MethodInfo abstractGetValueMethod = inheritedType.GetMethod("GetValue");
            MethodInfo abstractPropBackingFieldGet = inheritedType.GetProperty("BackingFieldInfo").GetGetMethod();

            TypeBuilder type = emittedModule.DefineType(typeName, TypeAttributes.Sealed | TypeAttributes.Class, inheritedType);
            type.SetCustomAttribute(new CustomAttributeBuilder(typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes), new object[0]));

            FieldBuilder valueField = type.DefineField(memberName, valueType, FieldAttributes.Public);
            valueField.SetCustomAttribute(new CustomAttributeBuilder(typeof(SerializeField).GetConstructor(Type.EmptyTypes), new object[0]));

            FieldBuilder backingFieldInfoField = type.DefineField("backingFieldInfo", typeof(FieldInfo), FieldAttributes.Private | FieldAttributes.Static);

            {
                MethodBuilder setValueMethod = type.DefineMethod(abstractSetValueMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, abstractSetValueMethod.ReturnType, abstractSetValueMethod.GetParameters().Select(n => n.ParameterType).ToArray());
                ILGenerator gen1 = setValueMethod.GetILGenerator();

                gen1.Emit(OpCodes.Ldarg_0);             // Load hidden "this" argument
                gen1.Emit(OpCodes.Ldarg_1);             // Load value argument
                gen1.Emit(OpCodes.Stfld, valueField);   // Set field to value
                gen1.Emit(OpCodes.Ret);                 // Return

                type.DefineMethodOverride(setValueMethod, abstractSetValueMethod);
            }

            {
                MethodBuilder getValueMethod = type.DefineMethod(abstractGetValueMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, abstractGetValueMethod.ReturnType, abstractGetValueMethod.GetParameters().Select(n => n.ParameterType).ToArray());
                ILGenerator gen2 = getValueMethod.GetILGenerator();

                gen2.Emit(OpCodes.Ldarg_0);             // Load hidden "this" argument
                gen2.Emit(OpCodes.Ldfld, valueField);   // Load value from field
                gen2.Emit(OpCodes.Ret);                 // Return value

                type.DefineMethodOverride(getValueMethod, abstractGetValueMethod);
            }

            {
                MethodBuilder propBackingFieldGet = type.DefineMethod(abstractPropBackingFieldGet.Name, MethodAttributes.Public | MethodAttributes.Virtual, abstractPropBackingFieldGet.ReturnType, abstractPropBackingFieldGet.GetParameters().Select(n => n.ParameterType).ToArray());
                ILGenerator gen3 = propBackingFieldGet.GetILGenerator();

                gen3.Emit(OpCodes.Ldsfld, backingFieldInfoField);    // Load field info value from static field
                gen3.Emit(OpCodes.Ret);                             // Return value

                type.DefineMethodOverride(propBackingFieldGet, abstractPropBackingFieldGet);
            }

            Type result = type.CreateType();

            FieldInfo backingValueField = result.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
            FieldInfo backingFieldInfo = result.GetField("backingFieldInfo", BindingFlags.NonPublic | BindingFlags.Static);
            backingFieldInfo.SetValue(null, backingValueField);

            return result;
        }
    }
}
#endif