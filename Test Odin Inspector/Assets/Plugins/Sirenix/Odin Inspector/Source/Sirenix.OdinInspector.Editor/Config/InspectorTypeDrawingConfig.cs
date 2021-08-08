#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorTypeDrawingConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Serialization;
    using System;
    using System.Collections.Generic;
    using Utilities;
    using UnityEngine;
    using UnityEditor;
    using System.Reflection;
    using System.Linq;

    /// <summary>
    /// <para>Contains configuration data for which types Odin should draw in the inspector.</para>
    /// </summary>
    /// <remarks>
    /// <para>Note that this class supports assigning arbitrary editor types to inspect any Unity object type. The Editor Types GUI in preferences simply does not, as of now, support assigning editors of any other type than <see cref="OdinEditor"/>. However, the API is open to further customization.</para>
    /// <para>When an editor is generated for a type, a new editor type is added to the GeneratedOdinEditors assembly, which is derived from the assigned editor type - in most cases, <see cref="OdinEditor"/>.</para>
    /// <para>You can check if an editor is compatible using <see cref="InspectorTypeDrawingConfig.UnityInspectorEditorIsValidBase(System.Type, System.Type)"/>.</para>
    /// </remarks>
    /// <seealso cref="InspectorTypeDrawingConfigDrawer"/>.
    /// <seealso cref="EditorCompilation"/>.
    [Serializable]
    public sealed class InspectorTypeDrawingConfig
    {
        private static readonly Dictionary<Type, Type> HardCodedDefaultEditors = new Dictionary<Type, Type>(); // Empty for now

        private static readonly FieldInfo CustomEditorInspectedTypeField = typeof(CustomEditor).GetField("m_InspectedType", Flags.InstanceAnyVisibility);

        private static readonly FieldInfo CustomEditorEditorForChildClassesField = typeof(CustomEditor).GetField("m_EditorForChildClasses", Flags.InstanceAnyVisibility);

        private static readonly PropertyInfo DebugLoggerProperty = typeof(Debug).GetProperty("unityLogger") ?? typeof(Debug).GetProperty("logger");

        /// <summary>
        /// <para>The type binder that the <see cref="InspectorTypeDrawingConfig"/> uses to bind types to names, and names to types.</para>
        /// <para>This is usually an instance of <see cref="DefaultSerializationBinder"/>.</para>
        /// </summary>
        public static readonly TwoWaySerializationBinder TypeBinder = new DefaultSerializationBinder();

        private static Type odinEditorType;

        private static Type OdinEditorType
        {
            get
            {
                if (odinEditorType == null)
                {
                    odinEditorType = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Editor.OdinEditor");
                }

                return odinEditorType;
            }
        }

        static InspectorTypeDrawingConfig()
        {
            if (CustomEditorInspectedTypeField == null || CustomEditorEditorForChildClassesField == null)
            {
                Debug.LogWarning("Could not find internal fields 'm_InspectedType' and/or 'm_EditorForChildClasses' in type UnityEditor.CustomEditor. Automatic inspector editor generation is highly unlikely to work.");
            }
        }

        [SerializeField, HideInInspector]
        private List<TypeDrawerPair> configs = new List<TypeDrawerPair>();

        private Dictionary<Type, Type> drawerCache = new Dictionary<Type, Type>();

        /// <summary>
        /// Resets the drawing configuration to the default values.
        /// </summary>
        public void ResetToDefault()
        {
            this.configs.Clear();
            this.drawerCache.Clear();

            EditorUtility.SetDirty(InspectorConfig.Instance);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Gets a list of all drawn types that have entries in the drawing config.
        /// </summary>
        public List<Type> GetAllDrawnTypesWithEntries()
        {
            return drawerCache.Keys.ToList();
        }

        /// <summary>
        /// Forces the config's internal drawer type to value type lookup cache to rebuild itself.
        /// </summary>
        public void UpdateCaches()
        {
            this.drawerCache.Clear();

            for (int i = 0; i < this.configs.Count; i++)
            {
                var config = this.configs[i];

                Type drawnType = TypeBinder.BindToType(config.DrawnTypeName);

                if (drawnType == null)
                {
                    continue;
                }

                Type drawerType;

                if (string.IsNullOrEmpty(config.EditorTypeName))
                {
                    drawerType = null;
                }
                else
                {
                    drawerType = TypeBinder.BindToType(config.EditorTypeName);

                    if (drawerType == null)
                    {
                        drawerType = typeof(MissingEditor);
                    }
                }

                this.drawerCache[drawnType] = drawerType;
            }
        }

        /// <summary>
        /// Clears the editor type entry for the given drawer, so it will be set to Unity's default.
        /// </summary>
        /// <param name="drawnType">The drawn type to clear the editor for.</param>
        /// <exception cref="System.ArgumentNullException">drawnType is null</exception>
        public void ClearEditorEntryForDrawnType(Type drawnType)
        {
            if (drawnType == null)
            {
                throw new ArgumentNullException("drawnType");
            }

            this.drawerCache.Remove(drawnType);
            string drawnTypeName = TypeBinder.BindToName(drawnType);

            for (int i = 0; i < this.configs.Count; i++)
            {
                var pair = this.configs[i];

                if (pair.DrawnTypeName == drawnTypeName)
                {
                    this.configs.RemoveAt(i);
                    i--;
                }
            }

            EditorUtility.SetDirty(InspectorConfig.Instance);
        }

        /// <summary>
        /// Assigns a given editor to draw a given type.
        /// </summary>
        /// <param name="drawnType">The drawn type to assign an editor type for.</param>
        /// <param name="editorType">The editor type to assign. When generating editors, a type derived from this editor will be created and set to draw the given drawn type.</param>
        /// <exception cref="System.ArgumentNullException">drawnType</exception>
        /// <exception cref="System.ArgumentException">The type " + editorType.GetNiceName() + " is not a valid base editor for type " + drawnType.GetNiceName() + ". Check criteria using <see cref="UnityInspectorEditorIsValidBase(Type, Type)"/>.</exception>
        public void SetEditorType(Type drawnType, Type editorType)
        {
            if (drawnType == null)
            {
                throw new ArgumentNullException("drawnType");
            }

            string drawnTypeName = TypeBinder.BindToName(drawnType);
            string editorTypeName = editorType == null ? "" : TypeBinder.BindToName(editorType);

            if (editorType != null)
            {
                if (!UnityInspectorEditorIsValidBase(editorType, drawnType))
                {
                    throw new ArgumentException("The type " + editorType.GetNiceName() + " is not a valid base editor for type " + drawnType.GetNiceName() + ".");
                }
            }

            this.drawerCache[drawnType] = editorType;
            bool added = false;

            for (int i = 0; i < this.configs.Count; i++)
            {
                var pair = this.configs[i];

                if (pair.DrawnTypeName == drawnTypeName)
                {
                    pair.EditorTypeName = editorTypeName;
                    this.configs[i] = pair;
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                this.configs.Add(new TypeDrawerPair(drawnType, editorType));
            }

            EditorUtility.SetDirty(InspectorConfig.Instance);
        }

        /// <summary>
        /// Determines whether an editor value has been assigned for a given drawn type.
        /// </summary>
        /// <param name="drawnType">The drawn type to check.</param>
        /// <exception cref="System.ArgumentNullException">drawnType is null</exception>
        public bool HasEntryForType(Type drawnType)
        {
            if (drawnType == null)
            {
                throw new ArgumentNullException("drawnType");
            }

            return this.drawerCache.ContainsKey(drawnType);
        }

        /// <summary>
        /// Gets which editor type would draw the given type. If the type has not been assigned a custom editor type in the config, the default editor type is returned using <see cref="GetDefaultEditorType(Type)"/>.
        /// </summary>
        /// <param name="drawnType">The drawn type to get an editor type for.</param>
        /// <returns>The editor that would draw the given type.</returns>
        /// <exception cref="System.ArgumentNullException">drawnType is null</exception>
        public Type GetEditorType(Type drawnType)
        {
            if (drawnType == null)
            {
                throw new ArgumentNullException("drawnType");
            }

            Type editorType;

            if (this.drawerCache.TryGetValue(drawnType, out editorType))
            {
                return editorType;
            }

            return GetDefaultEditorType(drawnType);
        }

        /// <summary>
        /// Gets the default editor that this type would have, if no custom editor was set for this type in particular. This is calculated using the value of <see cref="InspectorConfig.DefaultEditorBehaviour"/>.
        /// </summary>
        /// <param name="drawnType">The drawn type to get the default editor for.</param>
        /// <returns>The editor that would draw this type by default, or null, if there is no default Odin-defined editor for the drawn type.</returns>
        /// <exception cref="System.ArgumentNullException">drawnType is null</exception>
        public static Type GetDefaultEditorType(Type drawnType)
        {
            if (drawnType == null)
            {
                throw new ArgumentNullException("drawnType");
            }

            if (!InspectorTypeDrawingConfigDrawer.OdinCanCreateEditorFor(drawnType))
            {
                return null;
            }

            Type editorType;

            if (!HardCodedDefaultEditors.TryGetValue(drawnType, out editorType))
            {
                if (InspectorConfig.Instance.DefaultEditorBehaviour == InspectorDefaultEditors.None)
                {
                    return null;
                }

                var assemblyTypeFlag = AssemblyUtilities.GetAssemblyTypeFlag(drawnType.Assembly);

                bool useSirenixInspector;

                switch (assemblyTypeFlag)
                {
                    case AssemblyTypeFlags.UserTypes:
                    case AssemblyTypeFlags.UserEditorTypes:
                        useSirenixInspector = (InspectorConfig.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.UserTypes) == InspectorDefaultEditors.UserTypes;
                        break;

                    case AssemblyTypeFlags.PluginTypes:
                    case AssemblyTypeFlags.PluginEditorTypes:
                        useSirenixInspector = (InspectorConfig.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.PluginTypes) == InspectorDefaultEditors.PluginTypes;
                        break;

                    case AssemblyTypeFlags.UnityTypes:
                    case AssemblyTypeFlags.UnityEditorTypes:
                        useSirenixInspector = (InspectorConfig.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.UnityTypes) == InspectorDefaultEditors.UnityTypes;
                        break;

                    case AssemblyTypeFlags.OtherTypes:
                    // If we hit one of the below flags, or the default case, something actually went wrong.
                    // We don't care, though - just shove it into the other types category.
                    case AssemblyTypeFlags.All:
                    case AssemblyTypeFlags.GameTypes:
                    case AssemblyTypeFlags.EditorTypes:
                    case AssemblyTypeFlags.CustomTypes:
                    case AssemblyTypeFlags.None:
                    default:
                        useSirenixInspector = (InspectorConfig.Instance.DefaultEditorBehaviour & InspectorDefaultEditors.OtherTypes) == InspectorDefaultEditors.OtherTypes;
                        break;
                }

                if (useSirenixInspector)
                {
                    editorType = OdinEditorType;
                }
            }

            return editorType;
        }

        /// <summary>
        /// Checks whether the given editor can be assigned to draw any type using the <see cref="InspectorTypeDrawingConfig"/> class.
        /// </summary>
        /// <param name="editorType">Type of the editor to check.</param>
        /// <returns>True if the editor is valid, otherwise false</returns>
        public static bool UnityInspectorEditorIsValidBase(Type editorType)
        {
            return UnityInspectorEditorIsValidBase(editorType, null);
        }

        /// <summary>
        /// <para>Checks whether the given editor can be assigned to draw a given type using the <see cref="InspectorTypeDrawingConfig" /> class.</para>
        /// <para>This method checks the <see cref="CustomEditor"/> attribute on the type for whether the given type is compatible.</para>
        /// </summary>
        /// <param name="editorType">Type of the editor to check.</param>
        /// <param name="drawnType">Type of the drawn value to check. If this parameter is null, the drawn type is not checked for compatibility with the editor type; only the editor type itself is checked for validity.</param>
        /// <returns>True if the editor is valid, otherwise false</returns>
        /// <exception cref="System.ArgumentNullException">editorType</exception>
        public static bool UnityInspectorEditorIsValidBase(Type editorType, Type drawnType)
        {
            if (editorType == null)
            {
                throw new ArgumentNullException("editorType");
            }

            if (editorType.IsAbstract || !typeof(Editor).IsAssignableFrom(editorType) || editorType.FullName.StartsWith("UnityEditor", StringComparison.InvariantCulture))
            {
                return false;
            }

            if (CustomEditorInspectedTypeField == null)
            {
                return false;
            }

            var attribute = editorType.GetAttribute<CustomEditor>(true);

            if (attribute == null)
            {
                return true;
            }

            if (drawnType != null)
            {
                Type inspectedType = (Type)CustomEditorInspectedTypeField.GetValue(attribute);

                if (inspectedType == drawnType)
                {
                    return true;
                }
                else if (CustomEditorEditorForChildClassesField != null && inspectedType.IsAssignableFrom(drawnType))
                {
                    return (bool)CustomEditorEditorForChildClassesField.GetValue(attribute);
                }
            }

            return false;
        }

        /// <summary>
        /// <para>Gets the type that an editor draws, by extracting it from the editor's <see cref="CustomEditor"/> attribute, if it is declared.</para>
        /// <para>This method returns null for abstract editor types, as those can never draw anything.</para>
        /// </summary>
        /// <param name="editorType">Type of the editor.</param>
        /// <param name="editorForChildClasses">Whether the editor in question is also an editor for types derived from the given type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">editorType</exception>
        public static Type GetEditorDrawnType(Type editorType, out bool editorForChildClasses)
        {
            if (editorType == null)
            {
                throw new ArgumentNullException("editorType");
            }

            editorForChildClasses = false;

            if (editorType.IsAbstract || CustomEditorInspectedTypeField == null || CustomEditorEditorForChildClassesField == null)
            {
                return null;
            }

            UnityEngine.ILogger logger = null;

            if (DebugLoggerProperty != null)
            {
                logger = (UnityEngine.ILogger)DebugLoggerProperty.GetValue(null, null);
            }

            bool previous = true;

            if (logger != null)
            {
                previous = logger.logEnabled;
                logger.logEnabled = false;
            }

            var customEditorAttribute = editorType.GetAttribute<CustomEditor>();

            if (logger != null)
            {
                logger.logEnabled = previous;
            }

            if (customEditorAttribute != null)
            {
                editorForChildClasses = (bool)CustomEditorEditorForChildClassesField.GetValue(customEditorAttribute);
                return (Type)CustomEditorInspectedTypeField.GetValue(customEditorAttribute);
            }

            return null;
        }

        /// <summary>
        /// A type that indicates that a drawer is missing.
        /// </summary>
        public static class MissingEditor
        {
        }
    }
}
#endif