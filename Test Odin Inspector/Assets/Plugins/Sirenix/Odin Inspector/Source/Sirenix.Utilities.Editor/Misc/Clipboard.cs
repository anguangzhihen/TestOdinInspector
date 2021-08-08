#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Clipboard.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using Serialization;
    using Sirenix.Serialization.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Functions for accessing the clipboard.
    /// </summary>
    public static class Clipboard
    {
        private static object obj;

        // TODO: Shouldn't this be used? CurrentCopyMode will always the default mode.
        private static CopyModes copyMode;

        /// <summary>
        /// Gets the current copy mode.
        /// </summary>
        public static CopyModes CurrentCopyMode
        {
            get
            {
                return copyMode;
            }
        }

        /// <summary>
        /// Copies the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="copyMode">The copy mode.</param>
        public static void Copy<T>(T obj, CopyModes copyMode)
        {
            if (obj == null)
            {
                return;
            }

            Clipboard.copyMode = copyMode;

            var type = obj.GetType();

            if (type == typeof(string) || obj as UnityEngine.Object || type.IsValueType || type.IsEnum)
            {
                copyMode = CopyModes.CopyReference;
                Clipboard.obj = obj;
            }
            else if (copyMode == CopyModes.DeepCopy)
            {
                using (var stream = new MemoryStream())
                using (var serializationContext = Sirenix.Serialization.Utilities.Cache<SerializationContext>.Claim())
                using (var deserializationContext = Sirenix.Serialization.Utilities.Cache<DeserializationContext>.Claim())
                {
                    serializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                    deserializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;

                    List<UnityEngine.Object> unityReferences;
                    SerializationUtility.SerializeValue(obj, stream, DataFormat.Binary, out unityReferences, serializationContext);
                    stream.Position = 0;
                    Clipboard.obj = SerializationUtility.DeserializeValue<object>(stream, DataFormat.Binary, unityReferences, deserializationContext);
                }
            }
            else if (copyMode == CopyModes.ShallowCopy)
            {
                if (obj.GetType().IsArray)
                {
                    Array oldArray = (Array)(object)obj;
                    Array newArray;

                    if (oldArray.Rank > 1)
                    {
                        long[] lengths = new long[oldArray.Rank];

                        for (int i = 0; i < lengths.Length; i++)
                        {
                            lengths[i] = oldArray.GetLongLength(i);
                        }

                        newArray = Array.CreateInstance(oldArray.GetType().GetElementType(), lengths);
                    }
                    else
                    {
                        newArray = Array.CreateInstance(oldArray.GetType().GetElementType(), oldArray.LongLength);
                    }

                    Array.Copy(oldArray, newArray, 0);

                    Clipboard.obj = newArray;
                }
                else
                {
                    Clipboard.obj = obj.GetType().GetMethod("MemberwiseClone", Flags.AllMembers).Invoke(obj, null);
                }
            }
            else
            {
                Clipboard.obj = obj;
            }

            if (obj as string != null)
            {
                GUIUtility.systemCopyBuffer = obj as string;
            }
            else
            {
                GUIUtility.systemCopyBuffer = null;
            }
        }

        /// <summary>
        /// Copies the specified object.
        /// </summary>
        public static void Copy<T>(T obj)
        {
            Copy(obj, CopyModes.DeepCopy);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public static void Clear()
        {
            GUIUtility.systemCopyBuffer = null;
            Clipboard.obj = null;
        }

        /// <summary>
        /// Determines whether this instance can paste the specified type.
        /// </summary>
        public static bool CanPaste(Type type)
        {
            bool isNullSystemBuffer = string.IsNullOrEmpty(GUIUtility.systemCopyBuffer);

            if (type == typeof(string) && isNullSystemBuffer == false)
            {
                return true;
            }

            if (isNullSystemBuffer && Clipboard.obj != null && Clipboard.obj.GetType().InheritsFrom(type))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this instance can paste the specified type.
        /// </summary>
        public static bool CanPaste<T>()
        {
            return CanPaste(typeof(T));
        }

        /// <summary>
        /// Determines whether or not the Clipboard contains any instance.
        /// </summary>
        public static bool IsEmpty()
        {
            return string.IsNullOrEmpty(GUIUtility.systemCopyBuffer) && obj == null;
        }

        /// <summary>
        /// Tries the paste.
        /// </summary>
        public static bool TryPaste<T>(out T value)
        {
            if (CanPaste<T>() == false)
            {
                value = default(T);
                return false;
            }

            value = (T)Paste();
            return true;
        }

        /// <summary>
        /// Copies or gets the current object in the clipboard.
        /// </summary>
        public static T Paste<T>()
        {
            return (T)Paste();
        }

        /// <summary>
        /// Copies or gets the current object in the clipboard.
        /// </summary>
        public static object Paste()
        {
            if (IsEmpty())
            {
                throw new Exception("No object in clipboard. Check CanPaste() before calling Paste().");
            }

            if (string.IsNullOrEmpty(GUIUtility.systemCopyBuffer) == false)
            {
                return GUIUtility.systemCopyBuffer;
            }
            else if (Clipboard.copyMode == CopyModes.CopyReference)
            {
                return Clipboard.obj;
            }
            else if (Clipboard.copyMode == CopyModes.DeepCopy)
            {
                using (var stream = new MemoryStream())
                using (var serializationContext = Sirenix.Serialization.Utilities.Cache<SerializationContext>.Claim())
                using (var deserializationContext = Sirenix.Serialization.Utilities.Cache<DeserializationContext>.Claim())
                {
                    serializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                    deserializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;

                    List<UnityEngine.Object> unityReferences;
                    SerializationUtility.SerializeValue(obj, stream, DataFormat.Binary, out unityReferences, serializationContext);
                    stream.Position = 0;
                    return SerializationUtility.DeserializeValue<object>(stream, DataFormat.Binary, unityReferences, deserializationContext);
                }
            }
            else if (Clipboard.copyMode == CopyModes.ShallowCopy)
            {
                return Clipboard.obj.GetType().GetMethod("MemberwiseClone", Flags.AllMembers).Invoke(Clipboard.obj, null);
            }

            return null;
        }
    }

    /// <summary>
    /// The various modes of copying an object to the clipboard.
    /// </summary>
    public enum CopyModes
    {
        /// <summary>
        /// Deep copy.
        /// </summary>
        DeepCopy,

        /// <summary>
        /// Shallow Copy.
        /// </summary>
        ShallowCopy,

        /// <summary>
        /// Reference Copy.
        /// </summary>
        CopyReference
    }
}
#endif