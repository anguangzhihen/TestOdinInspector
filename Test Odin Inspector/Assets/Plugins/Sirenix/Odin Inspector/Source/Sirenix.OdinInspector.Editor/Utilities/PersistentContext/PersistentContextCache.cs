#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PersistentContextCache.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using UnityEditor;
    using UnityEngine;
    using FilePathAttribute = Sirenix.OdinInspector.FilePathAttribute;

    [AlwaysFormatsSelf, StructLayout(LayoutKind.Explicit)]
    internal struct ContextKey : ISelfFormatter, IEquatable<ContextKey>
    {
        [FieldOffset(0)]  public Guid Key1234;

        [FieldOffset(0)]  public int Key1;
        [FieldOffset(4)]  public int Key2;
        [FieldOffset(8)]  public int Key3;
        [FieldOffset(12)] public int Key4;
        [FieldOffset(16)] public int Key5;

        private static readonly Serializer<object> ObjectSerializer = Serializer.Get<object>();

        public ContextKey(int key1, int key2, int key3, int key4, int key5)
        {
            this.Key1234 = default(Guid);
            this.Key1 = key1;
            this.Key2 = key2;
            this.Key3 = key3;
            this.Key4 = key4;
            this.Key5 = key5;
        }

        public void Deserialize(IDataReader reader)
        {
            reader.ReadGuid(out this.Key1234);
            reader.ReadInt32(out this.Key5);
        }

        public void Serialize(IDataWriter writer)
        {
            writer.WriteGuid(null, this.Key1234);
            writer.WriteInt32(null, this.Key5);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return this.Key1 + this.Key2 + this.Key3 + this.Key4 + this.Key5;
            }
        }

        public bool Equals(ContextKey other)
        {
            return other.Key1234 == this.Key1234 && this.Key5 == other.Key5;
        }
    }

    [AlwaysFormatsSelf]
    internal class IndexedDictionary : IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>, ISelfFormatter
    {
        private Dictionary<ContextKey, GlobalPersistentContext> dictionary;

        private List<ContextKey> indexer;

        private class CKC : IEqualityComparer<ContextKey>
        {
            public bool Equals(ContextKey x, ContextKey y)
            {
                return x.Key1234 == y.Key1234 && x.Key5 == y.Key5;
            }

            public int GetHashCode(ContextKey obj)
            {
                return obj.GetHashCode();
            }
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public GlobalPersistentContext this[ContextKey key]
        {
            get { return this.dictionary[key]; }
            set
            {
                if (this.dictionary.ContainsKey(key))
                {
                    this.dictionary[key] = value;
                }
                else
                {
                    this.Add(key, value);
                }
            }
        }

		public IndexedDictionary()
        {
            this.dictionary = new Dictionary<ContextKey, GlobalPersistentContext>(0, new CKC());
            this.indexer = new List<ContextKey>(0);
        }

        public KeyValuePair<ContextKey, GlobalPersistentContext> Get(int index)
        {
            var k = this.indexer[index];
            GlobalPersistentContext val;
            this.dictionary.TryGetValue(k, out val);
            return new KeyValuePair<ContextKey, GlobalPersistentContext>(k, val);
        }

        public ContextKey GeContextKey(int index)
        {
            return this.indexer[index];
        }

        public void Add(ContextKey key, GlobalPersistentContext value)
        {
            this.dictionary.Add(key, value);
            this.indexer.Add(key);
        }

        public void Clear()
        {
            this.indexer.Clear();
            this.dictionary.Clear();
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < this.Count)
            {
                var k = this.indexer[index];
                if (this.dictionary.Remove(k))
                {
                    this.indexer.RemoveAt(index);
                }
                else
                {
                    throw new Exception("Fuck");
                }
            }
        }

        public bool TryGetValue(ContextKey key, out GlobalPersistentContext value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<ContextKey, GlobalPersistentContext>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>)this.dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>)this.dictionary).GetEnumerator();
        }

        //[System.Runtime.Serialization.OnDeserialized]
        //private void OnDeserialized()
        //{
        //    this.indexer = new List<ContextKey>(this.dictionary.Count);
        //    this.indexer.AddRange(this.dictionary.Keys);
        //}

        private static readonly Dictionary<Type, Type> GlobalPersistentContext_GenericVariantCache = new Dictionary<Type, Type>(FastTypeComparer.Instance);

        private static readonly Serializer<Type> TypeSerializer = Serializer.Get<Type>();

        public void Serialize(IDataWriter writer)
        {
            writer.BeginArrayNode(this.indexer.Count);

            for (int i = 0; i < this.indexer.Count; i++)
            {
                writer.BeginStructNode(null, null);

                var key = this.indexer[i];
                var value = this.Get(i).Value;

                key.Serialize(writer);

                if (value == null)
                {
                    writer.WriteNull(null);
                }
                else
                {
                    TypeSerializer.WriteValue(value.ValueType, writer);
                    value.Serialize(writer);
                }

                writer.EndNode(null);
            }

            writer.EndArrayNode();
        }

        public void Deserialize(IDataReader reader)
        {
            long length;
            Type whoCares;
            string whoCaresLess;
            EntryType nextEntry;

            reader.EnterArray(out length);

            this.indexer = new List<ContextKey>((int)length);
            this.dictionary = new Dictionary<ContextKey, GlobalPersistentContext>((int)length, new CKC());

            for (int i = 0; i < length; i++)
            {
                reader.EnterNode(out whoCares);

                var key = default(ContextKey);

                key.Deserialize(reader);

                nextEntry = reader.PeekEntry(out whoCaresLess);

                if (nextEntry == EntryType.Null)
                {
                    reader.ReadNull();
                }
                else
                {
                    var type = TypeSerializer.ReadValue(reader);
                    GlobalPersistentContext value = null;

                    if (type != null)
                    {
                        Type contextType; 
                        
                        lock (GlobalPersistentContext_GenericVariantCache)
                        {
                            if (!GlobalPersistentContext_GenericVariantCache.TryGetValue(type, out contextType))
                            {
                                contextType = typeof(GlobalPersistentContext<>).MakeGenericType(type);
                                GlobalPersistentContext_GenericVariantCache.Add(type, contextType);
                            }
                        }

                        value = (GlobalPersistentContext)Activator.CreateInstance(contextType);
                        value.Deserialize(reader);

                        this.Add(key, value);
                    }
                }

                reader.ExitNode();
            }

            reader.ExitArray();
        }
    }

    /// <summary>
    /// Persistent Context cache object.
    /// </summary>
    [InitializeOnLoad]
    public class PersistentContextCache
    {
        private static readonly object instance_LOCK = new object();
        private static PersistentContextCache instance;

        public static PersistentContextCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instance_LOCK)
                    {
                        if (instance == null)
                        {
                            instance = new PersistentContextCache();
                        }
                    }
                }

                return instance;
            }
        }

        private const int MAX_CACHE_SIZE_UPPER_LIMIT = 1000000;

        private static readonly string tempCacheFilename = "PersistentContextCache_v3.cache";

        private const int defaultApproximateSizePerEntry = 50;
        private static bool configsLoaded = false;

        private static bool internalEnableCaching;

        private static int internalMaxCacheByteSize;

        private static bool internalWriteToFile;

        [NonSerialized]
        private bool isInitialized = false;

        [NonSerialized]
        private DateTime lastSave = DateTime.MinValue;

        private PersistentContextCache()
        {
        }
        
        static PersistentContextCache()
        {
            UnityEditorEventUtility.DelayAction(() => Instance.EnsureIsInitialized());
        }

        private void EnsureIsInitialized()
        {
            if (this.isInitialized == false)
            {
                this.isInitialized = true;
                EditorApplication.update -= UpdateCallback;
                EditorApplication.update += UpdateCallback;
                AppDomain.CurrentDomain.DomainUnload -= this.OnDomainUnload;
                AppDomain.CurrentDomain.DomainUnload += this.OnDomainUnload;

#pragma warning disable 612, 618 // The EditorApplication.playmodeStateChanged have been changed, and marked as obsolete in future versions of Unity.
                EditorApplication.playmodeStateChanged -= this.OnPlaymodeChanged;
                EditorApplication.playmodeStateChanged += this.OnPlaymodeChanged;
#pragma warning restore 612, 618 // The EditorApplication.playmodeStateChanged have been changed, and marked as obsolete in future versions of Unity.

                this.LoadCache();
            }
        }

        private void OnPlaymodeChanged()
        {
            var now = DateTime.Now;
            if (now - this.lastSave > TimeSpan.FromSeconds(1))
            {
                this.lastSave = now;
                this.SaveCache();
            }
        }

        private static string FormatSize(int size)
        {
            return
                size > 1000000 ? ((size / 1000000).ToString() + " MB") :
                size > 1000 ? ((size / 1000).ToString() + " kB") :
                (size.ToString() + " bytes");
        }

        private static void LoadConfigs()
        {
            if (!configsLoaded)
            {
                internalEnableCaching = EditorPrefs.GetBool("PersistentContextCache.EnableCaching", true);
                internalMaxCacheByteSize = EditorPrefs.GetInt("PersistentContextCache.MaxCacheByteSize", 1000000);
                internalWriteToFile = EditorPrefs.GetBool("PersistentContextCache.WriteToFile", true);
                configsLoaded = true;
            }
        }

        private static void UpdateCallback()
        {
            CachePurger.Run();
        }

        private int approximateSizePerEntry;

        [NonSerialized]
        private IndexedDictionary cache = new IndexedDictionary();

        /// <summary>
        /// Estimated cache size in bytes.
        /// </summary>
        public int CacheSize { get { return (this.approximateSizePerEntry > 0 ? this.approximateSizePerEntry : defaultApproximateSizePerEntry) * this.EntryCount; } }

        /// <summary>
        /// The current number of context entries in the cache.
        /// </summary>
        public int EntryCount
        {
            get
            {
                return this.cache.Count;
            }
        }

        /// <summary>
        /// If <c>true</c> then persistent context is disabled entirely.
        /// </summary>
        [ShowInInspector]
        public bool EnableCaching
        {
            get
            {
                LoadConfigs();
                return internalEnableCaching;
            }
            set
            {
                internalEnableCaching = value;
                EditorPrefs.SetBool("PersistentContextCache.EnableCaching", value);
            }
        }

        /// <summary>
        /// If <c>true</c> the context will be saved to a file in the temp directory.
        /// </summary>
        [ShowInInspector]
        [EnableIf("EnableCaching")]
        public bool WriteToFile
        {
            get
            {
                LoadConfigs();
                return internalWriteToFile;
            }
            set
            {
                internalWriteToFile = value;
                EditorPrefs.SetBool("PersistentContextCache.WriteToFile", value);
            }
        }

        /// <summary>
        /// The max size of the cache in bytes.
        /// </summary>
        [ShowInInspector]
        [EnableIf("EnableCaching")]
        [CustomValueDrawer("DrawCacheSize")]
        [SuffixLabel("KB", Overlay = true)]
        public int MaxCacheByteSize
        {
            get
            {
                LoadConfigs();
                return internalMaxCacheByteSize;
            }
            private set
            {
                internalMaxCacheByteSize = value;
                EditorPrefs.SetInt("PersistentContextCache.MaxCacheByteSize", value);
            }
        }

        [ShowInInspector]
        [FilePath, ReadOnly]
        private string CacheFileLocation
        {
            get { return Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/"); }
            set { }
        }

        [ShowInInspector]
        [ProgressBar(0, 100), SuffixLabel("$CurrentCacheSizeSuffix", Overlay = true), ReadOnly]
        private int CurrentCacheSize
        {
            get
            {
                LoadConfigs();
                return (int)((float)this.CacheSize / (float)this.MaxCacheByteSize * 100f);
            }
        }

        private string CurrentCacheSizeSuffix
        {
            get { return StringUtilities.NicifyByteSize(this.CacheSize, 1) + " / " + StringUtilities.NicifyByteSize(this.MaxCacheByteSize, 1); }
        }

        internal GlobalPersistentContext<TValue> GetContext<TValue>(int key1, int key2, int key3, int key4, int key5, out bool isNew)
        {
            var key = new ContextKey(key1, key2, key3, key4, key5);
            return this.TryGetContext<TValue>(key, out isNew);
        }

        private int DrawCacheSize(int value, GUIContent label)
        {
            value /= 1000;

            value = SirenixEditorFields.DelayedIntField("Max Cache Size", value);
            value = Mathf.Clamp(value, 1, MAX_CACHE_SIZE_UPPER_LIMIT);

            return value * 1000;
        }

        private void OnDomainUnload(object sender, EventArgs e)
        {
            this.SaveCache();
        }

        [Button(ButtonSizes.Medium), ButtonGroup]
        [EnableIf("EnableCaching")]
        private void LoadCache()
        {
            var filePath = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
            FileInfo file = new FileInfo(filePath);

#if SIRENIX_INTERNAL
            // This detects if a crash happened during deserialization of the persistent cache; if so it's likely due to the unsafe bug.
            // Delete the cache file so that the user has a chance to rebuild the project.
            string deserializedCacheFinishedKey = SirenixAssetPaths.OdinTempPath.Replace('/', '.') + ".CacheDeserializationFinished";
            if (EditorPrefs.GetBool(deserializedCacheFinishedKey, true) == false)
            {
                Debug.LogError("Detected failed deserialization of PersistentContextCache. Deleting cache file. Try forcing a rebuild.");

                if (file.Exists)
                {
                    file.Delete();
                }
            }

            EditorPrefs.SetBool(deserializedCacheFinishedKey, false);
#endif

            try
            {
                this.approximateSizePerEntry = defaultApproximateSizePerEntry;

                if (file.Exists)
                {
                    using (FileStream stream = file.OpenRead())
                    {
                        var context = new DeserializationContext();
                        context.Config.DebugContext.LoggingPolicy = LoggingPolicy.Silent; // Shut up...
                        context.Config.DebugContext.ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient; // ...  and do your job!

                        this.cache = SerializationUtility.DeserializeValue<IndexedDictionary>(stream,
                            DataFormat.Binary, new List<UnityEngine.Object>(), context);

                        if (this.cache == null)
                        {
                            this.cache = new IndexedDictionary();
                        }
                    }

                    if (this.EntryCount > 0)
                    {
                        this.approximateSizePerEntry = (int)(file.Length / this.EntryCount);
                    }
                }
                else
                {
                    this.cache.Clear();
                }
            }
            catch (Exception ex)
            {
                this.cache = new IndexedDictionary();
                Debug.LogError("Exception happened when loading Persistent Context from file.");
                Debug.LogException(ex);
            }
#if SIRENIX_INTERNAL
            finally
            {
                EditorPrefs.SetBool(deserializedCacheFinishedKey, true);
            }
#endif
        }

        [Button(ButtonSizes.Medium), ButtonGroup]
        [EnableIf("EnableCaching")]
        private void SaveCache()
        {
            if (this.WriteToFile && this.EnableCaching)
            {
                try
                {
                    this.approximateSizePerEntry = defaultApproximateSizePerEntry;
                    string file = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
                    FileInfo info = new FileInfo(file);

                    // If there's no entries, delete the old file and don't make a new one.
                    if (this.cache.Count == 0)
                    {
                        if (info.Exists)
                        {
                            this.DeleteCache();
                        }
                    }
                    else
                    {
                        // Create dictionary
                        if (!Directory.Exists(SirenixAssetPaths.OdinTempPath))
                        {
                            Directory.CreateDirectory(SirenixAssetPaths.OdinTempPath);
                        }

                        using (FileStream stream = info.OpenWrite())
                        {
                            List<UnityEngine.Object> unityReferences;
                            SerializationUtility.SerializeValue(this.cache, stream, DataFormat.Binary, out unityReferences);

                            // Log error if any Unity references were serialized.
                            if (unityReferences != null && unityReferences.Count > 0)
                            {
                                Debug.LogError("Cannot reference UnityEngine Objects with PersistentContext.");
                            }
                        }

                        // Update size estimate.
                        this.approximateSizePerEntry = (int)(info.Length / this.EntryCount);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception happened when saving Persistent Context to file.");
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Delete the persistent cache file.
        /// </summary>
        [Button(ButtonSizes.Medium), ButtonGroup]
        [EnableIf("EnableCaching")]
        public void DeleteCache()
        {
            this.approximateSizePerEntry = defaultApproximateSizePerEntry;
            this.cache.Clear();

            string path = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

#if SIRENIX_INTERNAL

        [Button]
        private void OpenCacheDebugWindow()
        {
            var target = new CacheDebugTarget
            {
                Entries = this.cache
                    .Select(i => new CacheDebugEntry(i.Key, i.Value))
                    .ToList()
            };
            OdinEditorWindow.InspectObject(target);
        }

#endif

        private GlobalPersistentContext<TValue> TryGetContext<TValue>(ContextKey key, out bool isNew)
        {
            this.EnsureIsInitialized();

            GlobalPersistentContext context;
            if (this.EnableCaching && this.cache.TryGetValue(key, out context) && context is GlobalPersistentContext<TValue>)
            {
                isNew = false;
                return (GlobalPersistentContext<TValue>)context;
            }
            else
            {
                isNew = true;
                GlobalPersistentContext<TValue> c = GlobalPersistentContext<TValue>.Create();
                this.cache[key] = c;

                return c;
            }
        }

        private static class CachePurger
        {
            private static readonly List<KeyValuePair<int, GlobalPersistentContext>> buffer = new List<KeyValuePair<int, GlobalPersistentContext>>();
            private static double lastUpdate;
            private static IEnumerator purger;

            public static void Run()
            {
                if (purger != null)
                {
                    double start = EditorApplication.timeSinceStartup;

                    // Dirty, dirty do while.
                    do
                    {
                        if (!purger.MoveNext())
                        {
                            EndPurge();
                            return;
                        }
                    }
                    while (EditorApplication.timeSinceStartup - start < 0.005f);
                }
                else if (EditorApplication.timeSinceStartup - lastUpdate > 1.0)
                {
                    lastUpdate = EditorApplication.timeSinceStartup;

                    if (Instance.CacheSize > Instance.MaxCacheByteSize)
                    {
                        int count = (Instance.CacheSize - Instance.MaxCacheByteSize) / (Instance.CacheSize / Instance.EntryCount) + 1;
                        purger = Purge(count);
                    }
                }
            }

            private static void EndPurge()
            {
                if (purger != null)
                {
                    purger = null;

                    if (buffer != null)
                    {
                        buffer.Clear();
                    }

                    lastUpdate = EditorApplication.timeSinceStartup;
                }
            }

            private static IEnumerator Purge(int count)
            {
                double searchStartTime = EditorApplication.timeSinceStartup;
                long newest = DateTime.Now.Ticks;

                // Search
                for (int i = 0; i < Instance.EntryCount; i++)
                {
                    var entry = Instance.cache.Get(i);
                    bool added = false;

                    if (entry.Value == null)
                    {
                        // For whatever reason, the value is broken. Just throw it out then.
                        buffer.Insert(0, new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
                    }
                    else if (entry.Value.TimeStamp < newest)
                    {
                        // Try and insert the current entry into the buffer.
                        for (int j = 0; j < buffer.Count; j++)
                        {
                            if (buffer[j].Value != null && buffer[j].Value.TimeStamp >= entry.Value.TimeStamp)
                            {
                                if (buffer.Count >= count)
                                {
                                    buffer[buffer.Count - 1] = new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value);
                                    break;
                                }
                                else
                                {
                                    buffer.Insert(j, new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
                                    break;
                                }
                            }
                        }
                    }

                    // If no place was found, but the buffer isn't full, then add the entry to the end.
                    if (!added && buffer.Count < count)
                    {
                        buffer.Add(new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
                        added = true;
                    }

                    if (added)
                    {
                        var val = buffer[buffer.Count - 1].Value;
                        if (val != null)
                        {
                            newest = val.TimeStamp;
                        }
                    }

                    yield return null;
                }

                // Complete purge.
                foreach (var i in buffer.OrderByDescending(e => e.Key))
                {
                    Instance.cache.RemoveAt(i.Key);

                    yield return null;
                }
            }
        }

#if SIRENIX_INTERNAL

        private class CacheDebugTarget
        {
            [ShowInInspector, TableList]
            public List<CacheDebugEntry> Entries;
        }

        private class CacheDebugEntry
        {
            [TableColumnWidth(250)]
            [ShowInInspector, DisplayAsString, HideReferenceObjectPicker]
            public object Key1;

            [TableColumnWidth(250)]
            [ShowInInspector, DisplayAsString, HideReferenceObjectPicker]
            public object Key2;

            [TableColumnWidth(250)]
            [ShowInInspector, DisplayAsString, HideReferenceObjectPicker]
            public object Key3;

            [TableColumnWidth(250)]
            [ShowInInspector, DisplayAsString, HideReferenceObjectPicker]
            public object Key4;

            [TableColumnWidth(250)]
            [ShowInInspector, DisplayAsString, HideReferenceObjectPicker]
            public object Key5;

            [ShowInInspector]
            [DisplayAsString, HideReferenceObjectPicker]
            public GlobalPersistentContext Value;

            public CacheDebugEntry(ContextKey key, GlobalPersistentContext value)
            {
                this.Key1 = key.Key1;
                this.Key2 = key.Key2;
                this.Key3 = key.Key3;
                this.Key4 = key.Key4;
                this.Key5 = key.Key5;
                this.Value = value;
            }
        }

#endif
    }
}
#endif