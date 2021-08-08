#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StrongDictionaryPropertyResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;

    [ResolverPriority(-1)]
    public class StrongDictionaryPropertyResolver<TDictionary, TKey, TValue> : BaseKeyValueMapResolver<TDictionary>, IHasSpecialPropertyPaths, IPathRedirector, IMaySupportPrefabModifications
        where TDictionary : IDictionary<TKey, TValue>
    {
        private int lastUpdateID;

        private struct TempKeyInfo
        {
            public TKey Key;
            public bool IsInvalid;
        }

        private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();
        private Dictionary<int, TempKeyInfo> tempKeys = new Dictionary<int, TempKeyInfo>();

        private Dictionary<TDictionary, int> dictIndexMap = new Dictionary<TDictionary, int>();
        private List<TKey>[] keys;
        private List<TKey>[] oldKeys;
        private List<Attribute> childAttrs;

        private static readonly bool KeyTypeSupportsPersistentPaths = DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TKey));

        public bool MaySupportPrefabModifications { get { return KeyTypeSupportsPersistentPaths; } }

        public override Type ElementType { get { return typeof(EditableKeyValuePair<TKey, TValue>); } }

        public bool ValueApplyIsTemporary;

        protected override void Initialize()
        {
            base.Initialize();

            this.keys = new List<TKey>[this.Property.Tree.WeakTargets.Count];
            this.oldKeys = new List<TKey>[this.Property.Tree.WeakTargets.Count];

            for (int i = 0; i < this.keys.Length; i++)
            {
                this.keys[i] = new List<TKey>();
                this.oldKeys[i] = new List<TKey>();
            }

            var propAttrs = this.Property.Attributes;
            List<Attribute> attrs = new List<Attribute>(propAttrs.Count);

            for (int i = 0; i < propAttrs.Count; i++)
            {
                var attr = propAttrs[i];
                if (attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true)) continue;
                attrs.Add(attr);
            }

            this.childAttrs = attrs;
        }

        public bool TryGetRedirectedProperty(string childName, out InspectorProperty property)
        {
            this.EnsureUpdated();

            property = null;

            if (childName.Length == 0 || childName[0] != '{') return false;

            try
            {
                bool isKey = FastEndsWith(childName, "#key");

                if (isKey)
                {
                    childName = childName.Substring(0, childName.Length - 4);
                }

                var key = (TKey)DictionaryKeyUtility.GetDictionaryKeyValue(childName, typeof(TKey));
                var keyList = this.keys[0];

                for (int i = 0; i < keyList.Count; i++)
                {
                    if (PropertyValueEntry<TKey>.EqualityComparer(key, keyList[i]))
                    {
                        property = isKey ? this.Property.Children[i].Children["Key"] : this.Property.Children[i].Children["Value"];
                        return true;
                    }
                }
            }
            catch (Exception) { return false; }

            return false;
        }

        public string GetSpecialChildPath(int childIndex)
        {
            this.EnsureUpdated();

            var keys = this.keys[0];

            if (childIndex >= keys.Count)
            {
                // Force the update
                this.Update();
                keys = this.keys[0];
            }

            var key = this.keys[0][childIndex];
            return this.Property.Path + "." + DictionaryKeyUtility.GetDictionaryKeyString(key) + "#entry";
        }

        public override object GetKey(int selectionIndex, int childIndex)
        {
            this.EnsureUpdated();

            return this.keys[selectionIndex][childIndex];
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            this.EnsureUpdated();

            InspectorPropertyInfo result;

            if (!this.childInfos.TryGetValue(childIndex, out result))
            {
                result = InspectorPropertyInfo.CreateValue(
                    name: CollectionResolverUtilities.DefaultIndexToChildName(childIndex),
                    order: childIndex,
                    serializationBackend: this.Property.BaseValueEntry.SerializationBackend,
                    getterSetter: new GetterSetter<TDictionary, EditableKeyValuePair<TKey, TValue>>(
                        getter: this.CreateGetter(childIndex),
                        setter: this.CreateSetter(childIndex)),
                    attributes: this.childAttrs);

                this.childInfos[childIndex] = result;
            }

            return result;
        }

        private ValueGetter<TDictionary, EditableKeyValuePair<TKey, TValue>> CreateGetter(int childIndex)
        {
            return (ref TDictionary dict) =>
            {
                this.EnsureUpdated();

                var keys = this.keys[this.dictIndexMap[dict]];

                if (childIndex >= keys.Count)
                {
                    this.Update();
                    keys = this.keys[this.dictIndexMap[dict]];
                }

                TKey key = keys[childIndex];
                TempKeyInfo tempKeyInfo;
                TValue value;

                dict.TryGetValue(key, out value);
                bool hasTempKey = this.tempKeys.TryGetValue(childIndex, out tempKeyInfo);

                return new EditableKeyValuePair<TKey, TValue>(hasTempKey ? tempKeyInfo.Key : key, value, hasTempKey ? tempKeyInfo.IsInvalid : false, hasTempKey);
            };
        }

        private ValueSetter<TDictionary, EditableKeyValuePair<TKey, TValue>> CreateSetter(int childIndex)
        {
            return (ref TDictionary dict, EditableKeyValuePair<TKey, TValue> value) =>
            {
                this.EnsureUpdated();

                var selectionIndex = this.dictIndexMap[dict];
                var keys = this.keys[selectionIndex];

                if (childIndex >= keys.Count)
                {
                    this.Update();
                    selectionIndex = this.dictIndexMap[dict];
                    keys = this.keys[selectionIndex];
                }

                TKey oldKey = keys[childIndex];
                TValue oldValue;

                dict.TryGetValue(oldKey, out oldValue);

                TKey newKey = value.Key;
                TValue newValue = value.Value;

                bool keysAreEqual = PropertyValueEntry<TKey>.EqualityComparer(oldKey, newKey);

                if (!keysAreEqual)
                {
                    // Key has changed
                    if (dict.ContainsKey(newKey))
                    {
                        // Ignore if new key already exists in dictionary
                        // and assign a temporary invalid key
                        this.tempKeys[childIndex] = new TempKeyInfo() { Key = newKey, IsInvalid = true };
                    }
                    else if (!this.ValueApplyIsTemporary)
                    {
                        bool isOdinPrefab = this.Property.SupportsPrefabModifications && this.ValueEntry.SerializationBackend == SerializationBackend.Odin;

                        this.tempKeys.Remove(childIndex);

                        var removeInfo = new CollectionChangeInfo() { ChangeType = CollectionChangeType.RemoveKey, Key = oldKey, SelectionIndex = selectionIndex };
                        this.InvokeOnBeforeChange(removeInfo);
                        dict.Remove(oldKey);
                        this.InvokeOnAfterChange(removeInfo);

                        var setInfo = new CollectionChangeInfo() { ChangeType = CollectionChangeType.SetKey, Key = newKey, Value = newValue, SelectionIndex = this.dictIndexMap[dict] };
                        this.InvokeOnBeforeChange(setInfo);
                        dict.Add(newKey, newValue);
                        this.InvokeOnAfterChange(setInfo);

                        if (isOdinPrefab)
                        {
                            for (int i = 0; i < this.Property.Tree.WeakTargets.Count; i++)
                            {
                                this.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryRemoveKeyModification(this.Property, i, oldKey);
                                this.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryAddKeyModification(this.Property, i, newKey);
                            }
                        }

                        //
                        // Changing just one key may have changed the entire ordering of the dictionary.
                        // Keep everything valid by refreshing all properties.
                        //
                        this.childInfos.Clear();
                        this.Property.Children.ClearAndDisposeChildren();

                        //
                        // Get the value entry which now represents the new key, and register a value
                        // modification for it immediately, so as not to lose the old value.
                        //
                        // ( Calling update with a new value compared to the prefab registers the
                        //   appropriate value modifications immediately )
                        //

                        if (isOdinPrefab)
                        {
                            this.Update();

                            string keyStr = DictionaryKeyUtility.GetDictionaryKeyString(newKey);
                            var keyEntry = this.Property.Children[keyStr];
                            keyEntry.Update(true);

                            foreach (var child in keyEntry.Children.Recurse())
                            {
                                child.Update(true);
                            }
                        }
                    }
                    else
                    {
                        this.tempKeys[childIndex] = new TempKeyInfo() { Key = newKey, IsInvalid = false };
                    }
                }
                else if (!PropertyValueEntry<TValue>.EqualityComparer(oldValue, newValue))
                {
                    // Only value has changed, this is much simpler
                    var setInfo = new CollectionChangeInfo() { ChangeType = CollectionChangeType.SetKey, Key = newKey, Value = newValue, SelectionIndex = this.dictIndexMap[dict] };
                    this.InvokeOnBeforeChange(setInfo);
                    dict[newKey] = newValue;
                    this.InvokeOnAfterChange(setInfo);
                }

                if (value.IsTempKey && keysAreEqual)
                {
                    // The temp key set has set the same key back, so it's not invalid any more; it was cancelled
                    this.tempKeys.Remove(childIndex);
                }
            };
        }

        protected override int GetChildCount(TDictionary value)
        {
            return value.Count;
        }

        protected override void OnCollectionChangesApplied()
        {
            base.OnCollectionChangesApplied();

            if (this.Property.SupportsPrefabModifications)
            {
                int count = this.Property.Tree.WeakTargets.Count;

                for (int i = 0; i < count; i++)
                {
                    this.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(this.Property, i);
                }
            }
        }

        protected override void Add(TDictionary collection, object value)
        {
            var pair = (KeyValuePair<TKey, TValue>)value;
            collection.Add(pair);
            this.HandleAddSetPrefabValueModification(pair.Key);
        }

        protected override void Remove(TDictionary collection, object value)
        {
            collection.Remove((KeyValuePair<TKey, TValue>)value);
        }

        protected override void RemoveKey(TDictionary map, object key)
        {
            map.Remove((TKey)key);
        }

        protected override void Set(TDictionary map, object key, object value)
        {
            map[(TKey)key] = (TValue)value;
            this.HandleAddSetPrefabValueModification(key);
        }

        protected override void Clear(TDictionary collection)
        {
            collection.Clear();
        }

        protected override bool CollectionIsReadOnly(TDictionary collection)
        {
            return collection.IsReadOnly;
        }

        private void HandleAddSetPrefabValueModification(object key)
        {
            if (this.Property.SupportsPrefabModifications)
            {
                this.Update();

                var count = this.Property.Tree.WeakTargets.Count;

                for (int i = 0; i < count; i++)
                {
                    this.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryAddKeyModification(this.Property, i, key);

                    var child = this.Property.Children[DictionaryKeyUtility.GetDictionaryKeyString(key)];

                    if (child != null)
                    {
                        // We also need to register a value modification immediately so as not to lose the value we've just set
                        this.Property.Tree.PrefabModificationHandler.RegisterPrefabValueModification(child, i, forceImmediate: true);
                    }
                }
            }
        }

        private void EnsureUpdated()
        {
            if (this.Property.Tree.UpdateID != this.lastUpdateID)
            {
                this.Update();
            }
        }

        private void Update()
        {
            this.dictIndexMap.Clear();
            this.lastUpdateID = this.Property.Tree.UpdateID;

            for (int i = 0; i < this.keys.Length; i++)
            {
                // Swap lists and keep the old one for a change comparison
                var oldKeyList = this.keys[i];
                var keyList = this.oldKeys[i];

                this.oldKeys[i] = oldKeyList;
                this.keys[i] = keyList;

                keyList.Clear();

                var dict = (TDictionary)this.Property.ValueEntry.WeakValues[i];

                if (object.ReferenceEquals(dict, null)) continue;

                this.dictIndexMap[dict] = i;

                var castDict = dict as Dictionary<TKey, TValue>;

                if (castDict != null)
                {
                    // Reduce garbage allocation
                    foreach (var pair in castDict.GFIterator())
                    {
                        keyList.Add(pair.Key);
                    }
                }
                else
                {
                    foreach (var key in dict.Keys)
                    {
                        keyList.Add(key);
                    }
                }

                if (keyList.Count > 1)
                {
                    var comparer = DictionaryKeyUtility.KeyComparer<TKey>.Default;

                    var a = keyList[0];
                    for (int j = 1; j < keyList.Count; j++)
                    {
                        var b = keyList[j];
                        if (comparer.Compare(a, b) > 0)
                        {
                            keyList.Sort(comparer);
                            break;
                        }
                        a = b;
                    }
                }

                if (keyList.Count != oldKeyList.Count)
                {
                    this.childInfos.Clear();
                    this.Property.Children.ClearAndDisposeChildren();
                }
                else
                {
                    for (int j = 0; j < keyList.Count; j++)
                    {
                        if (!PropertyValueEntry<TKey>.EqualityComparer(keyList[j], oldKeyList[j]))
                        {
                            this.childInfos.Clear();
                            this.Property.Children.ClearAndDisposeChildren();
                            break;
                        }
                    }
                }
            }
        }

        public override int ChildNameToIndex(string name)
        {
            return CollectionResolverUtilities.DefaultChildNameToIndex(name);
        }

        private static bool FastEndsWith(string str, string endsWith)
        {
            if (str.Length < endsWith.Length) return false;

            int start = str.Length - endsWith.Length;

            for (int i = 0; i < endsWith.Length; i++)
            {
                if (str[start + i] != endsWith[i]) return false;
            }

            return true;
        }
    }
}
#endif