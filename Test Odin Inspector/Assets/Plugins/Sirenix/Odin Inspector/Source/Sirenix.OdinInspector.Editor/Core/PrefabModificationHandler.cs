#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PrefabModificationHandler.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

//#define PREFAB_DEBUG

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Serialization;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// <para>Handles all prefab modifications that apply to the targets of a property tree, if any. This class determines which properties have modifications, what the modifications are, auto-applies modifications if the current instance values do not correspond to the prefab values, and also provides an API for modifying those modifications.</para>
    /// <para>NOTE: This class is liable to see a lot of changes, as the prefab modification system is slated to be redesigned for increased extendability in the future. Do not depend overly on the current API.</para>
    /// </summary>
    public sealed class PrefabModificationHandler
    {
        private readonly bool targetSupportsPrefabSerialization;

        private ImmutableList<UnityEngine.Object> immutableTargetPrefabs;
        private bool hasPrefabs;
        private bool allTargetsHaveSamePrefab;
        private Dictionary<string, PrefabModification>[] prefabValueModifications;
        private Dictionary<string, PrefabModification>[] prefabListLengthModifications;
        private Dictionary<string, PrefabModification>[] prefabDictionaryModifications;
        private PropertyTree prefabPropertyTree;
        private int[] prefabPropertyTreeIndexMap;
        private bool allowAutoRegisterPrefabModifications = true;

        public PrefabModificationHandler(PropertyTree tree)
        {
            this.Tree = tree;

            this.prefabValueModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
            this.prefabListLengthModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
            this.prefabDictionaryModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
            this.prefabPropertyTreeIndexMap = new int[tree.WeakTargets.Count];

            var targetType = tree.TargetType;

            this.targetSupportsPrefabSerialization = !tree.IsStatic && typeof(UnityEngine.Object).IsAssignableFrom(targetType) && typeof(ISupportsPrefabSerialization).IsAssignableFrom(targetType);
        }

        public PropertyTree Tree { get; private set; }

        /// <summary>
        /// The prefabs for each prefab instance represented by the property tree, if any.
        /// </summary>
        public ImmutableList<UnityEngine.Object> TargetPrefabs { get { return this.immutableTargetPrefabs; } }

        /// <summary>
        /// Whether any of the values the property tree represents are prefab instances.
        /// </summary>
        public bool HasPrefabs { get { return this.hasPrefabs; } }

        /// <summary>
        /// A prefab tree for the prefabs of this property tree's prefab instances, if any exist.
        /// </summary>
        public PropertyTree PrefabPropertyTree { get { return this.prefabPropertyTree; } }

        public bool HasNestedOdinPrefabData { get; private set; }

        public void Update()
        {
            this.hasPrefabs = false;
            this.HasNestedOdinPrefabData = false;

            if (this.Tree.IsStatic) return;

            UnityEngine.Object[] prefabs = new UnityEngine.Object[this.Tree.WeakTargets.Count];

            if (typeof(UnityEngine.Object).IsAssignableFrom(this.Tree.TargetType))
            {
                int prefabCount = 0;
                for (int i = 0; i < this.Tree.WeakTargets.Count; i++)
                {
                    var target = (UnityEngine.Object)this.Tree.WeakTargets[i];

                    bool isPrefab = false;
                    UnityEngine.Object prefab = null;

                    if (target != null)
                    {
                        var prefabType = PrefabUtility.GetPrefabType(target);

                        if (!this.HasNestedOdinPrefabData && OdinPrefabSerializationEditorUtility.ObjectHasNestedOdinPrefabData(target))
                        {
                            this.HasNestedOdinPrefabData = true;
                        }
                    
                        if (prefabType == PrefabType.PrefabInstance && (prefab = OdinPrefabSerializationEditorUtility.GetCorrespondingObjectFromSource(target)) != null)
                        {
                            isPrefab = true;
                        }
                    }

                    if (isPrefab)
                    {
                        prefabs[i] = prefab;
                        this.hasPrefabs = true;

                        this.prefabPropertyTreeIndexMap[i] = prefabCount;
                        prefabCount++;

                        if (this.targetSupportsPrefabSerialization)
                        {
                            ISupportsPrefabSerialization cast = (ISupportsPrefabSerialization)this.Tree.WeakTargets[i];
                            var modificationList = UnitySerializationUtility.PrefabModificationCache.DeserializePrefabModificationsCached(target, cast.SerializationData.PrefabModifications, cast.SerializationData.PrefabModificationsReferencedUnityObjects);

                            var listLengthModifications = this.prefabListLengthModifications[i] ?? new Dictionary<string, PrefabModification>();
                            var valueModifications = this.prefabValueModifications[i] ?? new Dictionary<string, PrefabModification>();
                            var dictionaryModifications = this.prefabDictionaryModifications[i] ?? new Dictionary<string, PrefabModification>();

                            listLengthModifications.Clear();
                            valueModifications.Clear();
                            dictionaryModifications.Clear();

                            //
                            // We have to be careful about Unity's crappy prefab system screwing us with
                            // duplicate modifications.
                            //
                            // As a rule, modifications that come earliest are the ones we want to keep.
                            //

                            for (int j = 0; j < modificationList.Count; j++)
                            {
                                var mod = modificationList[j];

                                switch (mod.ModificationType)
                                {
                                    case PrefabModificationType.Value:
                                        if (!valueModifications.ContainsKey(mod.Path))
                                        {
                                            valueModifications[mod.Path] = mod;
                                        }
                                        break;

                                    case PrefabModificationType.ListLength:
                                        if (!listLengthModifications.ContainsKey(mod.Path))
                                        {
                                            listLengthModifications[mod.Path] = mod;
                                        }
                                        break;

                                    case PrefabModificationType.Dictionary:
                                        if (!dictionaryModifications.ContainsKey(mod.Path))
                                        {
                                            dictionaryModifications[mod.Path] = mod;
                                        }
                                        break;

                                    default:
                                        throw new NotImplementedException(mod.ModificationType.ToString());
                                }
                            }

                            //
                            // There might be modifications already registered this frame, that haven't been serialized yet
                            // If so, we must not lose them. They *always* override pre-existing modifications.
                            //

                            var registeredModifications = UnitySerializationUtility.GetRegisteredPrefabModifications(target);

                            if (registeredModifications != null)
                            {
                                for (int j = 0; j < registeredModifications.Count; j++)
                                {
                                    var mod = registeredModifications[j];

                                    if (mod.ModificationType == PrefabModificationType.Value)
                                    {
                                        valueModifications[mod.Path] = mod;
                                    }
                                    else if (mod.ModificationType == PrefabModificationType.ListLength)
                                    {
                                        listLengthModifications[mod.Path] = mod;
                                    }
                                    else if (mod.ModificationType == PrefabModificationType.Dictionary)
                                    {
                                        dictionaryModifications[mod.Path] = mod;
                                    }
                                }
                            }

                            this.prefabListLengthModifications[i] = listLengthModifications;
                            this.prefabValueModifications[i] = valueModifications;
                            this.prefabDictionaryModifications[i] = dictionaryModifications;
                        }
                    }
                    else
                    {
                        this.prefabPropertyTreeIndexMap[i] = -1;
                    }
                }

                if (prefabCount > 0)
                {
                    var prefabsNoNull = new UnityEngine.Object[prefabCount];

                    for (int i = 0; i < prefabs.Length; i++)
                    {
                        int index = this.prefabPropertyTreeIndexMap[i];

                        if (index >= 0)
                        {
                            prefabsNoNull[index] = prefabs[i];
                        }
                    }

                    if (this.prefabPropertyTree != null)
                    {
                        if (this.prefabPropertyTree.WeakTargets.Count != prefabsNoNull.Length)
                        {
                            this.prefabPropertyTree = null;
                        }
                        else
                        {
                            for (int i = 0; i < this.prefabPropertyTree.WeakTargets.Count; i++)
                            {
                                if (!object.ReferenceEquals(this.prefabPropertyTree.WeakTargets[i], prefabsNoNull[i]))
                                {
                                    this.prefabPropertyTree = null;
                                    break;
                                }
                            }
                        }
                    }

                    if (this.prefabPropertyTree == null)
                    {
                        this.prefabPropertyTree = PropertyTree.Create(prefabsNoNull);
                    }

                    this.prefabPropertyTree.UpdateTree();
                }

                this.allTargetsHaveSamePrefab = false;

                if (prefabCount == this.Tree.WeakTargets.Count)
                {
                    this.allTargetsHaveSamePrefab = true;

                    var firstPrefab = prefabs[0];

                    for (int i = 1; i < prefabs.Length; i++)
                    {
                        if (!object.ReferenceEquals(firstPrefab, prefabs[i]))
                        {
                            this.allTargetsHaveSamePrefab = false;
                            break;
                        }
                    }
                }
            }

            this.immutableTargetPrefabs = new ImmutableList<UnityEngine.Object>(prefabs.Cast<UnityEngine.Object>().ToArray());

            if (this.HasPrefabs && this.Tree.UnitySerializedObject != null && !Application.isPlaying)
            {
                for (int i = 0; i < this.Tree.WeakTargets.Count; i++)
                {
                    var targetPrefab = this.TargetPrefabs[i];

                    if (targetPrefab != null)
                    {
                        var target = (UnityEngine.Object)this.Tree.WeakTargets[i];

                        var prefabType = PrefabUtility.GetPrefabType(target);

                        if (prefabType != PrefabType.PrefabInstance)
                        {
                            continue;
                        }

                        PropertyModification[] mods = PrefabUtility.GetPropertyModifications(target);

                        if (mods != null)
                        {
                            for (int j = 0; j < mods.Length; j++)
                            {
                                var mod = mods[j];

                                if (mod.target != targetPrefab)
                                {
                                    continue;
                                }

                                string path = mod.propertyPath;
                                bool isArraySize = false;

                                if (FastStringEndsWith(path, ".Array.size"))
                                {
                                    path = path.Substring(0, path.Length - ".Array.size".Length);
                                    isArraySize = true;
                                }

                                InspectorProperty closestProperty;
                                var prop = this.Tree.GetPropertyAtUnityPath(path, out closestProperty);

                                if (prop != null)
                                {
                                    if (isArraySize)
                                    {
                                        prop.BaseValueEntry.ListLengthChangedFromPrefab = true;
                                    }
                                    else
                                    {
                                        prop.BaseValueEntry.ValueChangedFromPrefab = true;
                                    }
                                }
                                else if (closestProperty != null && closestProperty.ValueEntry != null && closestProperty.ValueEntry.IsMarkedAtomic)
                                {
                                    closestProperty.BaseValueEntry.ValueChangedFromPrefab = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool FastStringEndsWith(string str, string endsWith)
        {
            int strLength = str.Length;
            int endsWithLength = endsWith.Length;

            if (strLength < endsWithLength) return false;
            if (strLength == endsWithLength) return str == endsWith;

            for (int i = 1; i <= endsWithLength; i++)
            {
                if (str[strLength - i] != endsWith[endsWithLength - i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool TargetHasRegisteredModificationsWaitingForApply()
        {
            for (int i = 0; i < this.Tree.WeakTargets.Count; i++)
            {
                UnityEngine.Object value = (UnityEngine.Object)this.Tree.WeakTargets[i];

                //if (UnitySerializationUtility.GetRegisteredPrefabModifications(value) != null)
                if (UnitySerializationUtility.HasModificationsWaitingForDelayedApply(value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the prefab modification type of a given property, if any.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <param name="forceAutoRegister"></param>
        /// <returns>
        /// The prefab modification type of the property if it has one, otherwise null.
        /// </returns>
        public PrefabModificationType? GetPrefabModificationType(InspectorProperty property, bool forceAutoRegister = false)
        {
            if (!this.HasPrefabs)
            {
                return null;
            }

            bool registerModification;
            var result = this.PrivateGetPrefabModificationType(property, out registerModification);

            //
            // If there is a change, and this change is not verified with the registered
            // modifications or has changed from the registered modifications, and all
            // targets have the same prefab, then we have to make sure this change is
            // registered.
            //
            // This case happens when, for example, somebody changes a prefab value outside
            // of the inspector.
            //

            if (result != null && (forceAutoRegister || (this.allowAutoRegisterPrefabModifications && registerModification && this.allTargetsHaveSamePrefab && !this.TargetHasRegisteredModificationsWaitingForApply())))
            {
                switch (result.Value)
                {
                    case PrefabModificationType.Value:

                        for (int i = 0; i < this.Tree.WeakTargets.Count; i++)
                        {
#if PREFAB_DEBUG
                            Debug.Log(Event.current.type + " (id " + this.Tree.UpdateID + "): Registering non-inspector-triggered prefab value change for property " + property.Path);
                            result = this.PrivateGetPrefabModificationType(property, out registerModification);
#endif

                            this.RegisterPrefabValueModification(property, i);
                        }

                        break;

                    case PrefabModificationType.ListLength:

                        for (int i = 0; i < this.Tree.WeakTargets.Count; i++)
                        {
#if PREFAB_DEBUG
                            Debug.Log(Event.current.type + " (id " + this.Tree.UpdateID + "): Registering non-inspector-triggered prefab list length change for property " + property.Path);
                            result = this.PrivateGetPrefabModificationType(property, out registerModification);
#endif

                            property.Children.Update();
                            this.RegisterPrefabListLengthModification(property, i, property.Children.Count);
                        }

                        break;

                    case PrefabModificationType.Dictionary:

                        for (int i = 0; i < this.Tree.WeakTargets.Count; i++)
                        {
#if PREFAB_DEBUG
                            Debug.Log(Event.current.type + " (id " + this.Tree.UpdateID + "): Registering non-inspector-triggered prefab dictionary length change for property " + property.Path);
                            result = this.PrivateGetPrefabModificationType(property, out registerModification);
#endif

                            property.Children.Update();
                            this.RegisterPrefabDictionaryDeltaModification(property, i);
                        }

                        break;

                    default:
                        break;
                }
            }

            return result;
        }

        private static bool PropertyCanHaveModifications(InspectorProperty property)
        {
            if (!property.SupportsPrefabModifications) return false;

            // Special rule: dictionary key value pairs never *directly* have modifications,
            // nor do their keys. Their parent dictionary can have key modifications, and their values
            // can have modifications, but pairs and their keys *can't* have them.
            if (property.ValueEntry != null && property.ValueEntry.TypeOfValue.IsGenericType && property.ValueEntry.TypeOfValue.GetGenericTypeDefinition() == typeof(EditableKeyValuePair<,>))
            {
                return false;
            }

            return true;
        }

        private PrefabModificationType? PrivateGetPrefabModificationType(InspectorProperty property, out bool registerModification)
        {
            if (Application.isPlaying || !this.HasPrefabs || !this.allTargetsHaveSamePrefab)
            {
                // We do not display prefab modifications if we are playing, or
                // there are no prefabs, or if not all targets have the same prefab
                // or if the property cannot have modifications.

                registerModification = false;
                return null;
            }

            if (!PropertyCanHaveModifications(property))
            {
                registerModification = false;

                if (property.Index == 0 && property.Parent != null && property.Parent.Parent != null && property.Parent.ValueEntry != null && property.Parent.ValueEntry.TypeOfValue.IsGenericType && property.Parent.ValueEntry.TypeOfValue.GetGenericTypeDefinition() == typeof(EditableKeyValuePair<,>))
                {
                    var dictionaryProp = property.Parent.Parent;

                    for (int i = 0; i < this.prefabDictionaryModifications.Length; i++)
                    {
                        var key = (dictionaryProp.ChildResolver as IKeyValueMapResolver).GetKey(i, property.Parent.Index);
                        var mods = this.prefabDictionaryModifications[i];

                        PrefabModification mod;

                        if (mods != null && mods.TryGetValue(dictionaryProp.PrefabModificationPath, out mod))
                        {
                            if (mod.DictionaryKeysAdded != null && mod.DictionaryKeysAdded.Contains(key))
                            {
                                return PrefabModificationType.Value;
                            }
                        }
                    }
                }

                return null;
            }

            registerModification = true;

            for (int i = 0; i < this.prefabValueModifications.Length; i++)
            {
                var mods = this.prefabValueModifications[i];

                if (mods != null)
                {
                    var prop = property;

                    do
                    {
                        if (mods.ContainsKey(prop.PrefabModificationPath))
                        {
                            var entry = prop.ValueEntry;

                            if (entry != null)
                            {
                                var mod = mods[prop.PrefabModificationPath];
                                registerModification = entry.ValueIsPrefabDifferent(mod.ModifiedValue, i);
                            }
                            else
                            {
                                registerModification = false;
                            }

                            return PrefabModificationType.Value;
                        }

                        prop = prop.ParentValueProperty;
                    } while (prop != null && prop.ValueEntry.TypeOfValue.IsValueType);
                }
            }

            for (int i = 0; i < this.prefabListLengthModifications.Length; i++)
            {
                var mods = this.prefabListLengthModifications[i];

                PrefabModification mod;

                if (mods != null && mods.TryGetValue(property.PrefabModificationPath, out mod))
                {
                    registerModification = mod.NewLength != property.Children.Count;
                    return PrefabModificationType.ListLength;
                }
            }

            for (int i = 0; i < this.prefabDictionaryModifications.Length; i++)
            {
                var mods = this.prefabDictionaryModifications[i];

                if (mods != null && mods.ContainsKey(property.PrefabModificationPath))
                {
                    registerModification = false;
                    return PrefabModificationType.Dictionary;
                }
            }

            if (this.prefabPropertyTree == null || property.ValueEntry == null)
            {
                registerModification = false;
                return null;
            }

            var prefabProperty = this.prefabPropertyTree.GetPropertyAtPrefabModificationPath(property.PrefabModificationPath);

            if (prefabProperty == null || prefabProperty.ValueEntry == null)
            {
                // This property doesn't even exist as a value in the prefab
                // This happens, for example, for collection elements that exist
                // because a collection length has been increased in the instance
                return PrefabModificationType.Value;
            }

            if (!prefabProperty.ValueEntry.TypeOfValue.IsValueType && prefabProperty.ValueEntry.TypeOfValue != property.ValueEntry.TypeOfValue)
            {
                // Different types means there's a modification
                return PrefabModificationType.Value;
            }

            //if (prefabProperty.ValueEntry.IsMarkedAtomic || prefabProperty.ValueEntry.TypeOfValue == typeof(string))
            //{
            //    // Compare atomic type values (and strings) directly

            for (int i = 0; i < prefabProperty.ValueEntry.ValueCount; i++)
            {
                if (property.ValueEntry.ValueIsPrefabDifferent(prefabProperty.ValueEntry.WeakValues[i], i))
                {
                    return PrefabModificationType.Value;
                }
            }
            //}

            if (prefabProperty.ValueEntry.TypeOfValue.IsValueType && !prefabProperty.ValueEntry.ValueTypeValuesAreEqual(property.ValueEntry))
            {
                // Compare value type values directly
                return PrefabModificationType.Value;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.TypeOfValue) && !object.ReferenceEquals(property.ValueEntry.WeakSmartValue, prefabProperty.ValueEntry.WeakSmartValue))
            {
                UnityEngine.Object instanceValue = (UnityEngine.Object)property.ValueEntry.WeakSmartValue;
                UnityEngine.Object prefabValue = (UnityEngine.Object)prefabProperty.ValueEntry.WeakSmartValue;

                if (instanceValue == null || prefabValue == null)
                {
                    // One is null while the other is not, since the references didn't match
                    return PrefabModificationType.Value;
                }
                
                var instanceParentAsset = OdinPrefabSerializationEditorUtility.GetCorrespondingObjectFromSource(instanceValue);

                if (instanceParentAsset != prefabValue)
                {
                    // This value is not the same "conceptual" local value as is there on the prefab
                    return PrefabModificationType.Value;
                }
            }

            if (prefabProperty.Children != null && property.Children != null && prefabProperty.ChildResolver is ICollectionResolver && prefabProperty.Children.Count != property.Children.Count)
            {
                // Different amount of children in a collection means a dictionary or list length change

                // TODO: This is super, super, super fucking ugly. We really need a new prefab modification system design.
                if (prefabProperty.ChildResolver is IKeyValueMapResolver)
                {
                    return PrefabModificationType.Dictionary;
                }
                else
                {
                    return PrefabModificationType.ListLength;
                }
            }

            registerModification = false;
            return null;
        }

        /// <summary>
        /// Registers a modification of type <see cref="PrefabModificationType.ListLength" /> for a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target to register a modification for.</param>
        /// <param name="newLength">The modified list length.</param>
        /// <exception cref="System.ArgumentException">
        /// Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.
        /// or
        /// newLength cannot be negative!
        /// </exception>
        public void RegisterPrefabListLengthModification(InspectorProperty property, int targetIndex, int newLength)
        {
            if (!targetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var valueMods = this.prefabValueModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (listLengthMods == null)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            if (newLength < 0)
            {
                throw new ArgumentException("newLength cannot be negative!");
            }

            try
            {
                this.allowAutoRegisterPrefabModifications = false;

                PrefabModification mod = new PrefabModification()
                {
                    ModificationType = PrefabModificationType.ListLength,
                    Path = property.PrefabModificationPath,
                    NewLength = newLength
                };

                this.Update();

                this.RemovePrefabModificationsForInvalidIndices(property, listLengthMods, valueMods, dictionaryMods, newLength);
                listLengthMods[property.PrefabModificationPath] = mod;

                UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
            }
            finally
            {
                this.allowAutoRegisterPrefabModifications = true;
            }
        }

        /// <summary>
        /// Registers a modification of type <see cref="PrefabModificationType.Value" /> for a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target to register a modification for.</param>
        /// <param name="forceImmediate">Whether to force the change to be registered immediately, rather than at the end of frame.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public void RegisterPrefabValueModification(InspectorProperty property, int targetIndex, bool forceImmediate = false)
        {
            const int MAX_REFERENCE_PATHS_COUNT = 5;

            if (!this.targetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            try
            {
                this.allowAutoRegisterPrefabModifications = false;

                var propPath = property.PrefabModificationPath;
                PrefabModification mod = new PrefabModification();

                Dictionary<string, PrefabModification> extraModChanges = null;

                // Initialize modification with property values and reference paths
                {
                    property.Update(true); // Make damn sure we have the latest values

                    mod.Path = propPath;
                    mod.ModifiedValue = property.ValueEntry.WeakValues[targetIndex];

                    //
                    // To handle references properly, we'll need to trawl the entire
                    // property tree for reference paths to save.
                    //

                    var value = property.ValueEntry.WeakValues[targetIndex];

                    if (!object.ReferenceEquals(value, null) && !(value is UnityEngine.Object) && (property.ValueEntry.ValueState == PropertyValueState.Reference || !(property.BaseValueEntry.TypeOfValue.IsValueType || property.BaseValueEntry.TypeOfValue == typeof(string))))
                    {
                        //string refPath;
                        //if (this.ObjectIsReferenced(value, out refPath) && this.GetReferenceCount(mod.ModifiedValue) == 1 && property.ValueEntry.TargetReferencePath != null)
                        //{
                        //    var refProp = this.GetPropertyAtPath(property.ValueEntry.TargetReferencePath);
                        //    mod.ReferencePaths = new List<string>() { refProp.DeepReflectionPath };
                        //}
                        //else
                        {
                            mod.ReferencePaths = new List<string>();

                            foreach (var prop in this.Tree.EnumerateTree(true))
                            {
                                if (prop.ValueEntry == null || prop.Info.TypeOfValue.IsValueType || prop.Path == property.Path)
                                {
                                    continue;
                                }

                                prop.Update(true);

                                if (object.ReferenceEquals(value, prop.ValueEntry.WeakValues[targetIndex]))
                                {
                                    if (mod.ReferencePaths.Count < MAX_REFERENCE_PATHS_COUNT)
                                    {
                                        mod.ReferencePaths.Add(prop.PrefabModificationPath);
                                    }

                                    // Also update the reference value to know about this new reference to it

                                    PrefabModification refMod;
                                    if (valueMods.TryGetValue(prop.PrefabModificationPath, out refMod))
                                    {
                                        if (refMod.ReferencePaths == null || !refMod.ReferencePaths.Contains(property.PrefabModificationPath))
                                        {
                                            if (refMod.ReferencePaths == null)
                                            {
                                                refMod.ReferencePaths = new List<string>();
                                            }

                                            if (refMod.ReferencePaths.Count < MAX_REFERENCE_PATHS_COUNT)
                                            {
                                                refMod.ReferencePaths.Add(property.PrefabModificationPath);

                                                if (extraModChanges == null)
                                                {
                                                    extraModChanges = new Dictionary<string, PrefabModification>();
                                                }

                                                extraModChanges[prop.PrefabModificationPath] = refMod;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // This may hang around from before references were disabled for Unity objects, so set that to null just to be sure
                    else if (value is UnityEngine.Object)
                    {
                        mod.ReferencePaths = null;
                    }
                }

                if (forceImmediate)
                {
                    this.Update();
                    this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
                    valueMods[propPath] = mod;

                    // If we are setting a value mod on anything with a list length or dictionary mod, remove those mods
                    listLengthMods.Remove(propPath);
                    dictionaryMods.Remove(propPath);

                    if (extraModChanges != null)
                    {
                        foreach (var item in extraModChanges)
                        {
                            valueMods[item.Key] = item.Value;
                        }
                    }

                    UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
                }
                else
                {
                    this.Tree.DelayAction(() =>
                    {
                        this.Update();

                        this.Tree.UpdateTree();
                        this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);

                        valueMods[propPath] = mod;

                        // If we are setting a value mod on anything with a list length or dictionary mod, remove those mods
                        listLengthMods.Remove(propPath);
                        dictionaryMods.Remove(propPath);

                        if (extraModChanges != null)
                        {
                            foreach (var item in extraModChanges)
                            {
                                valueMods[item.Key] = item.Value;
                            }
                        }

                        UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
                    });
                }
            }
            finally
            {
                this.allowAutoRegisterPrefabModifications = true;
            }
        }

        /// <summary>
        /// Calculates a delta between the current dictionary property and its prefab counterpart, and registers that delta as a <see cref="PrefabModificationType.Dictionary" /> modification.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public void RegisterPrefabDictionaryDeltaModification(InspectorProperty property, int targetIndex)
        {
            if (!this.targetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            try
            {
                this.allowAutoRegisterPrefabModifications = false;

                var propPath = property.PrefabModificationPath;
                PrefabModification mod;

                if (valueMods.TryGetValue(propPath, out mod))
                {
                    // A value modification already exists for this dictionary, meaning we 
                    // should not do a delta, but instead a raw value one.
                    this.RegisterPrefabValueModification(property, targetIndex);
                    return;
                }

                bool modificationIsNew = false;

                if (!dictionaryMods.TryGetValue(propPath, out mod))
                {
                    modificationIsNew = true;

                    mod = new PrefabModification();
                    mod.ModificationType = PrefabModificationType.Dictionary;
                    mod.Path = propPath;
                }

                InspectorProperty prefabProperty = this.prefabPropertyTree.GetPropertyAtPath(property.Path);

                if (prefabProperty == null)
                {
                    // Cannot register a delta modification when there is no prefab dictionary to perform a delta on
                    return;
                }

                var prefabIndex = this.prefabPropertyTreeIndexMap[targetIndex];

                var prefabDict = prefabProperty.ValueEntry.WeakValues[prefabIndex] as IDictionary;
                var propDict = property.ValueEntry.WeakValues[targetIndex] as IDictionary;

                if (prefabDict == null || propDict == null)
                {
                    // Cannot register a delta modification when one of the dictionaries to compare is null
                    return;
                }

                //
                // First find removed keys
                //

                foreach (var key in prefabDict.Keys)
                {
                    if (!propDict.Contains(key))
                    {
                        // This is a removed key
                        if (mod.DictionaryKeysRemoved == null)
                        {
                            mod.DictionaryKeysRemoved = new object[] { key };
                        }
                        else
                        {
                            mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysRemoved, key);
                        }
                    }
                }

                //
                // Then find added keys
                //
                foreach (var key in propDict.Keys)
                {
                    if (!prefabDict.Contains(key))
                    {
                        // This is a removed key
                        if (mod.DictionaryKeysAdded == null)
                        {
                            mod.DictionaryKeysAdded = new object[] { key };
                        }
                        else
                        {
                            mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysAdded, key);
                        }
                    }
                }

                if (modificationIsNew && (mod.DictionaryKeysAdded == null || mod.DictionaryKeysAdded.Length == 0) && (mod.DictionaryKeysRemoved == null || mod.DictionaryKeysRemoved.Length == 0))
                {
                    // There is no delta - register no modification at all.
                    return;
                }

                this.Update();

                this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);

                dictionaryMods[propPath] = mod;

                UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
            }
            finally
            {
                this.allowAutoRegisterPrefabModifications = true;
            }
        }

        /// <summary>
        /// Adds a remove key modification to the dictionary modifications of a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to be removed.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public void RegisterPrefabDictionaryRemoveKeyModification(InspectorProperty property, int targetIndex, object key)
        {
            if (!this.targetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            try
            {
                this.allowAutoRegisterPrefabModifications = false;

                var propPath = property.PrefabModificationPath;
                PrefabModification mod;

                if (!dictionaryMods.TryGetValue(propPath, out mod))
                {
                    mod = new PrefabModification();
                    mod.ModificationType = PrefabModificationType.Dictionary;
                    mod.Path = propPath;
                }

                bool actuallySetRemoveKey = true;

                // If this key is in the added keys array, just remove it from there and don't add it to removed keys
                if (mod.DictionaryKeysAdded != null)
                {
                    for (int i = 0; i < mod.DictionaryKeysAdded.Length; i++)
                    {
                        if (key.Equals(mod.DictionaryKeysAdded[i]))
                        {
                            mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysAdded, i);
                            actuallySetRemoveKey = false;
                            i--;
                        }
                    }
                }

                if (actuallySetRemoveKey)
                {
                    if (mod.DictionaryKeysRemoved == null)
                    {
                        mod.DictionaryKeysRemoved = new object[] { key };
                    }
                    else
                    {
                        mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysRemoved, key);
                    }
                }

                this.Update();
                this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
                dictionaryMods[propPath] = mod;

                UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
            }
            finally
            {
                this.allowAutoRegisterPrefabModifications = true;
            }
        }

        /// <summary>
        /// Adds an add key modification to the dictionary modifications of a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to be added.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public void RegisterPrefabDictionaryAddKeyModification(InspectorProperty property, int targetIndex, object key)
        {
            if (!this.targetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            try
            {
                this.allowAutoRegisterPrefabModifications = false;

                var propPath = property.PrefabModificationPath;
                PrefabModification mod;

                if (!dictionaryMods.TryGetValue(propPath, out mod))
                {
                    mod = new PrefabModification();
                    mod.ModificationType = PrefabModificationType.Dictionary;
                    mod.Path = propPath;
                }

                if (mod.DictionaryKeysAdded == null)
                {
                    mod.DictionaryKeysAdded = new object[] { key };
                }
                else
                {
                    mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysAdded, key);
                }

                // If this key is in the removed keys array, remove it from there
                if (mod.DictionaryKeysRemoved != null)
                {
                    for (int i = 0; i < mod.DictionaryKeysRemoved.Length; i++)
                    {
                        if (key.Equals(mod.DictionaryKeysRemoved[i]))
                        {
                            mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysRemoved, i);
                            i--;
                        }
                    }
                }

                this.Update();
                this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
                dictionaryMods[propPath] = mod;

                UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
            }
            finally
            {
                this.allowAutoRegisterPrefabModifications = true;
            }
        }

        /// <summary>
        /// Removes all dictionary modifications on a property for a given dictionary key value.
        /// </summary>
        /// <param name="property">The property to remove a key modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to remove modifications for.</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public void RemovePrefabDictionaryModification(InspectorProperty property, int targetIndex, object key)
        {
            if (object.ReferenceEquals(key, null))
            {
                throw new ArgumentNullException("key");
            }

            if (property.ValueEntry == null || !typeof(UnityEngine.Object).IsAssignableFrom(this.Tree.TargetType))
            {
                // Nothing to do here
                return;
            }

            if (!this.targetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            Dictionary<string, PrefabModification> listLengthMods = this.prefabListLengthModifications[targetIndex];
            Dictionary<string, PrefabModification> valueMods = this.prefabValueModifications[targetIndex];
            Dictionary<string, PrefabModification> dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (listLengthMods == null || valueMods == null || dictionaryMods == null)
            {
                Debug.LogError("Target of type " + this.Tree.TargetType + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            try
            {
                this.allowAutoRegisterPrefabModifications = false;

                bool changed = false;
                var removePath = property.PrefabModificationPath;
                PrefabModification mod;

                if (dictionaryMods.TryGetValue(removePath, out mod))
                {
                    if (mod.DictionaryKeysRemoved != null)
                    {
                        for (int i = 0; i < mod.DictionaryKeysRemoved.Length; i++)
                        {
                            if (key.Equals(mod.DictionaryKeysRemoved[i]))
                            {
                                changed = true;
                                mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysRemoved, i);
                                i--;
                            }
                        }
                    }

                    if (mod.DictionaryKeysAdded != null)
                    {
                        for (int i = 0; i < mod.DictionaryKeysAdded.Length; i++)
                        {
                            if (key.Equals(mod.DictionaryKeysAdded[i]))
                            {
                                changed = true;
                                mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysAdded, i);
                                i--;
                            }
                        }

                        if (changed)
                        {
                            // Also remove all modifications that were added because of this added element
                            string checkPath = removePath + "." + DictionaryKeyUtility.GetDictionaryKeyString(key);
                            HashSet<string> toRemove = new HashSet<string>();

                            foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
                            {
                                if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
                                {
                                    toRemove.Add(path);
                                }
                            }

                            foreach (string path in toRemove)
                            {
                                listLengthMods.Remove(path);
                                valueMods.Remove(path);
                                dictionaryMods.Remove(path);
                            }
                        }
                    }

                    if ((mod.DictionaryKeysRemoved == null || mod.DictionaryKeysRemoved.Length == 0)
                        && (mod.DictionaryKeysRemoved == null || mod.DictionaryKeysRemoved.Length == 0))
                    {
                        // If there are no modifications left, completely remove this modification
                        dictionaryMods.Remove(removePath);
                    }

                    changed = true;
                }

                if (changed)
                {
                    // Register modification changes for next serialization call
                    UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
                }
            }
            finally
            {
                this.allowAutoRegisterPrefabModifications = true;
            }
        }

        /// <summary>
        /// Removes all prefab modifications of a given type on a given property.
        /// </summary>
        /// <param name="property">The property to remove modifications for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="modificationType">Type of the modification to remove.</param>
        public void RemovePrefabModification(InspectorProperty property, int targetIndex, PrefabModificationType modificationType)
        {
            if (property.ValueEntry == null || !typeof(UnityEngine.Object).IsAssignableFrom(this.Tree.TargetType))
            {
                // Nothing to do here
                return;
            }

            try
            {
                this.allowAutoRegisterPrefabModifications = false;

                if (property.ValueEntry.SerializationBackend.IsUnity)
                {
                    var target = (UnityEngine.Object)this.Tree.WeakTargets[targetIndex];
                    var prefab = this.TargetPrefabs[targetIndex];
                    var unityMods = PrefabUtility.GetPropertyModifications(target).ToList();

                    if (modificationType == PrefabModificationType.Value)
                    {
                        for (int i = 0; i < unityMods.Count; i++)
                        {
                            var mod = unityMods[i];

                            if (mod.target == prefab && mod.propertyPath.StartsWith(property.UnityPropertyPath, StringComparison.InvariantCulture))
                            {
                                // Remove modifications on both the path, and the children of the path
                                unityMods.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    else if (modificationType == PrefabModificationType.ListLength)
                    {
                        var sizePath = property.UnityPropertyPath + ".Array.size";

                        // Remove the actual size modification
                        for (int i = 0; i < unityMods.Count; i++)
                        {
                            var mod = unityMods[i];

                            if (mod.target == prefab && mod.propertyPath == sizePath)
                            {
                                unityMods.RemoveAt(i);
                                i--;
                            }
                        }

                        // Also remove all modifications on new elements of the list
                        // created by the list length modification we're removing
                        this.RemovePrefabModificationsForInvalidIndices(property, prefab, unityMods);
                    }

                    // Make sure we play nice with undo, and that the value isn't registered
                    // as changed and marked dirty from the prefab again.

                    PrefabUtility.SetPropertyModifications(target, unityMods.ToArray());
                    string name = Undo.GetCurrentGroupName();
                    Undo.FlushUndoRecordObjects();
                    this.Tree.RootProperty.RecordForUndo();
                    PrefabUtility.SetPropertyModifications(target, unityMods.ToArray());
                }
                else if (property.ValueEntry.SerializationBackend == SerializationBackend.Odin)
                {
                    // Removing sirenix prefab modifications is a little more tricky

                    if (!this.targetSupportsPrefabSerialization)
                    {
                        Debug.LogError("Target of type " + this.Tree.TargetType + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                        return;
                    }

                    Dictionary<string, PrefabModification> listLengthMods = this.prefabListLengthModifications[targetIndex];
                    Dictionary<string, PrefabModification> valueMods = this.prefabValueModifications[targetIndex];
                    Dictionary<string, PrefabModification> dictionaryMods = this.prefabDictionaryModifications[targetIndex];

                    if (listLengthMods == null || valueMods == null || dictionaryMods == null)
                    {
                        Debug.LogError("Target of type " + this.Tree.TargetType + " at index " + targetIndex + " is not a prefab!");
                        return;
                    }

                    string removePath = property.PrefabModificationPath;
                    PrefabModification mod;

                    bool removed = false;

                    if (modificationType == PrefabModificationType.Value && valueMods.ContainsKey(removePath))
                    {
                        this.Update();

                        // Remove the actual mod
                        valueMods.Remove(removePath);

                        // Also remove all modifications on children of the path
                        {
                            string checkPath = removePath + ".";
                            HashSet<string> toRemove = new HashSet<string>();

                            foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
                            {
                                if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
                                {
                                    toRemove.Add(path);
                                }
                            }

                            foreach (string path in toRemove)
                            {
                                listLengthMods.Remove(path);
                                valueMods.Remove(path);
                                dictionaryMods.Remove(path);
                            }
                        }

                        removed = true;
                    }
                    else if (modificationType == PrefabModificationType.ListLength && listLengthMods.TryGetValue(removePath, out mod))
                    {
                        this.Update();

                        // Remove the actual mod
                        listLengthMods.Remove(removePath);

                        InspectorProperty prefabProperty = this.prefabPropertyTree.GetPropertyAtPath(property.Path);

                        if (prefabProperty != null)
                        {
                            var collectionResolver = prefabProperty.ChildResolver as ICollectionResolver;
                            int prefabChildCount = collectionResolver != null ? collectionResolver.MaxCollectionLength : prefabProperty.Children.Count;

                            // Also remove all modifications on new elements of the list
                            // created by the list length modification we're removing
                            this.RemovePrefabModificationsForInvalidIndices(property, listLengthMods, valueMods, dictionaryMods, prefabChildCount);
                        }

                        removed = true;
                    }
                    else if (modificationType == PrefabModificationType.Dictionary && dictionaryMods.TryGetValue(removePath, out mod))
                    {
                        this.Update();

                        // Remove the actual mod
                        dictionaryMods.Remove(removePath);

                        // Also remove all modifications on dictionary items
                        // added by the modification we're removing
                        if (mod.DictionaryKeysAdded != null)
                        {
                            HashSet<string> toRemove = new HashSet<string>();

                            for (int i = 0; i < mod.DictionaryKeysAdded.Length; i++)
                            {
                                string keyStr = DictionaryKeyUtility.GetDictionaryKeyString(mod.DictionaryKeysAdded[i]);
                                string checkPath = removePath + "." + keyStr;

                                foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
                                {
                                    if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
                                    {
                                        toRemove.Add(path);
                                    }
                                }
                            }

                            foreach (string path in toRemove)
                            {
                                listLengthMods.Remove(path);
                                valueMods.Remove(path);
                                dictionaryMods.Remove(path);
                            }
                        }

                        removed = true;
                    }

                    if (removed)
                    {
                        // Register modification changes for next serialization call
                        UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.Tree.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
                    }
                }
            }
            finally
            {
                this.allowAutoRegisterPrefabModifications = true;
            }
        }

        private void RemoveInvalidPrefabModifications(string startPath, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods)
        {
            HashSet<string> toRemove = new HashSet<string>();

            foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
            {
                if (!path.StartsWith(startPath)) continue;

                var prop = this.Tree.GetPropertyAtPrefabModificationPath(path);

                if (prop == null || !prop.SupportsPrefabModifications)
                {
                    toRemove.Add(path);
                }
            }

            foreach (string path in toRemove)
            {
                listLengthMods.Remove(path);
                valueMods.Remove(path);
                dictionaryMods.Remove(path);
            }
        }

        private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods, int newLength)
        {
            string removePath = property.PrefabModificationPath;

            HashSet<string> toRemove = new HashSet<string>();
            string checkPath = removePath + ".[";

            foreach (string path in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
            {
                if (!path.StartsWith(checkPath, StringComparison.InvariantCulture))
                {
                    continue;
                }

                int arrayEndIndex = path.IndexOf("]", checkPath.Length, StringComparison.InvariantCulture);

                if (arrayEndIndex <= checkPath.Length)
                {
                    continue;
                }

                string indexStr = path.Substring(checkPath.Length, arrayEndIndex - checkPath.Length);
                int index;

                if (!int.TryParse(indexStr, out index))
                {
                    continue;
                }

                if (index >= newLength) // It's an invalid element modification
                {
                    toRemove.Add(path);
                }
            }

            foreach (string path in toRemove)
            {
                listLengthMods.Remove(path);
                valueMods.Remove(path);
                dictionaryMods.Remove(path);
            }
        }

        private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, UnityEngine.Object prefab, List<PropertyModification> unityMods)
        {
            var checkPath = property.UnityPropertyPath + ".Array.data[";
            InspectorProperty prefabProperty = this.prefabPropertyTree.GetPropertyAtPath(property.Path);

            if (prefabProperty != null)
            {
                HashSet<string> toRemove = new HashSet<string>();
                var collectionResolver = prefabProperty.ChildResolver as ICollectionResolver;
                int prefabChildCount = collectionResolver != null ? collectionResolver.MaxCollectionLength : prefabProperty.Children.Count;

                if (prefabChildCount < property.Children.Count)
                {
                    foreach (var mod in unityMods)
                    {
                        var path = mod.propertyPath;

                        if (!path.StartsWith(checkPath, StringComparison.InvariantCulture))
                        {
                            continue;
                        }

                        int arrayEndIndex = path.IndexOf("]", checkPath.Length, StringComparison.InvariantCulture);

                        if (arrayEndIndex <= checkPath.Length)
                        {
                            continue;
                        }

                        string indexStr = path.Substring(checkPath.Length, arrayEndIndex - checkPath.Length);
                        int index;

                        if (!int.TryParse(indexStr, out index))
                        {
                            continue;
                        }

                        if (index >= prefabChildCount) // It's an invalid element modifications
                        {
                            toRemove.Add(path);
                        }
                    }
                }

                for (int i = 0; i < unityMods.Count; i++)
                {
                    var mod = unityMods[i];

                    if (mod.target == prefab && toRemove.Contains(mod.propertyPath))
                    {
                        unityMods.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all prefab modifications in this property tree for a given selection index.
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public List<PrefabModification> GetPrefabModifications(int targetIndex)
        {
            if (!this.targetSupportsPrefabSerialization)
            {
                return new List<PrefabModification>();
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null || listLengthMods == null || dictionaryMods == null)
            {
                return new List<PrefabModification>();
            }

            return listLengthMods.Values
                   .AppendWith(valueMods.Values)
                   .AppendWith(dictionaryMods.Values)
                   .ToList();
        }
    }
}
#endif