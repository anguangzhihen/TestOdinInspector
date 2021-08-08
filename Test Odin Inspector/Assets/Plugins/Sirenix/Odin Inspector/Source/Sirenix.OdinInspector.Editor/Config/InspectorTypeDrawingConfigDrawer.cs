#if UNITY_EDITOR
#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorTypeDrawingConfigDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

//#define PROFILE_LOGGING

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;
    using Sirenix.OdinInspector.Internal;

    /// <summary>
    /// <para>Draws an <see cref="InspectorTypeDrawingConfig"/> instance, and contains methods getting all types that should be drawn by Odin.</para>
    /// <para>Note that this class keeps a lot of static state, and is only intended to draw the instance of <see cref="InspectorTypeDrawingConfig"/> that exists in the <see cref="InspectorConfig"/> singleton asset. If used to draw other instances, odd behaviour may occur.</para>
    /// </summary>
    /// <seealso cref="InspectorTypeDrawingConfig"/>.
    /// <seealso cref="EditorCompilation"/>.
    public class InspectorTypeDrawingConfigDrawer : OdinValueDrawer<InspectorTypeDrawingConfig>
    {
#pragma warning disable 0618

        private static readonly HashSet<Type> NeverDrawTypes;

#pragma warning restore 0618

        private static bool initializedForDrawing = false;

        private static readonly TypeGroup UserTypesRootGroup = new TypeGroup("User Types");
        private static readonly TypeGroup PluginTypesRootGroup = new TypeGroup("Plugin Types");
        private static readonly TypeGroup UnityTypesRootGroup = new TypeGroup("Unity Types");
        private static readonly TypeGroup OtherTypesRootGroup = new TypeGroup("Other Types");

        private static readonly List<Type> PossibleEditorTypes = new List<Type>();
        private static readonly HashSet<Type> PossibleDrawnTypes = new HashSet<Type>();

        static InspectorTypeDrawingConfigDrawer()
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();

            using (ProfileSection.Start("InspectorTypeDrawingConfigDrawer static constructor"))
            {
                NeverDrawTypes = new HashSet<Type>(FastTypeComparer.Instance);
                {
                    var networkView = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.NetworkView");
                    if (networkView != null) NeverDrawTypes.Add(networkView);
                    var gUIText = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.GUIText");
                    if (gUIText != null) NeverDrawTypes.Add(gUIText);
                }

                Type[] unityObjectTypes;

                using (ProfileSection.Start("Finding all UnityObject types"))
                {
                    unityObjectTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.All)
                        .Where(type =>
                            !type.Assembly.IsDynamic() &&
                            typeof(UnityEngine.Object).IsAssignableFrom(type) &&
                            //TypeExtensions.IsValidIdentifier(type.FullName) &&
                            !type.IsDefined<CompilerGeneratedAttribute>() &&
                            !type.IsDefined<ObsoleteAttribute>() &&
                            !typeof(Joint).IsAssignableFrom(type) &&
                            !NeverDrawTypes.Contains(type))
                        .ToArray();
                }

                Dictionary<Type, Type> haveDrawersAlready = new Dictionary<Type, Type>(FastTypeComparer.Instance);
                Dictionary<Type, Type> derivedClassDrawnTypes = new Dictionary<Type, Type>(FastTypeComparer.Instance);

                using (ProfileSection.Start("Search for editors"))
                {
                    foreach (var type in unityObjectTypes)
                    {
                        if (typeof(Editor).IsAssignableFrom(type))
                        {
                            try
                            {
                                bool editorForChildClasses;

                                var drawnType = InspectorTypeDrawingConfig.GetEditorDrawnType(type, out editorForChildClasses);

                                if (drawnType != null)
                                {
                                    if (!haveDrawersAlready.ContainsKey(drawnType))
                                    {
                                        haveDrawersAlready.Add(drawnType, type);
                                    }

                                    if (editorForChildClasses && !derivedClassDrawnTypes.ContainsKey(drawnType))
                                    {
                                        derivedClassDrawnTypes.Add(drawnType, type);
                                    }
                                }

                                if (/*type.IsVisible && */InspectorTypeDrawingConfig.UnityInspectorEditorIsValidBase(type, null))
                                {
                                    PossibleEditorTypes.Add(type);
                                }
                            }
                            catch (TypeLoadException)
                            {
                            }
                            catch (ReflectionTypeLoadException)
                            {
                            }
                        }
                    };
                }
                
                using (ProfileSection.Start("Assign editors to Unity objects"))
                {
                    HashSet<Type> stopBaseTypeLookUpTypes = new HashSet<Type>(FastTypeComparer.Instance)
                    {
                        typeof(object),
                        typeof(Component),
                        typeof(Behaviour),
                        typeof(MonoBehaviour),
                        typeof(UnityEngine.Object),
                        typeof(ScriptableObject),
                        typeof(StateMachineBehaviour),
                        //typeof(Networking.NetworkBehaviour)
                    };

                    if (UnityNetworkingUtility.NetworkBehaviourType != null)
                    {
                        // UnityEngine.Networking has been removed in Unity 2019+. 
                        stopBaseTypeLookUpTypes.Add(UnityNetworkingUtility.NetworkBehaviourType);
                    }

                    //Debug.Log("Searching the following " + unityObjectTypes.Length + " types for Odin-drawable types:\n\n" + string.Join("\n", unityObjectTypes.Select(n => n.GetNiceFullName()).ToArray()));

                    unityObjectTypes.Where(type => /*type.IsVisible && */!type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsGenericType && !typeof(Editor).IsAssignableFrom(type) && !typeof(EditorWindow).IsAssignableFrom(type))
                                    .ForEach(type =>
                                    {
                                        Type preExistingEditorType;
                                        bool haveDrawerAlready = haveDrawersAlready.TryGetValue(type, out preExistingEditorType);

                                        if (!haveDrawerAlready)
                                        {
                                            Type baseType = type.BaseType;

                                            while (baseType != null && !stopBaseTypeLookUpTypes.Contains(baseType))
                                            {
                                                Type editor;

                                                if (derivedClassDrawnTypes.TryGetValue(baseType, out editor))
                                                {
                                                    haveDrawerAlready = true;
                                                    preExistingEditorType = editor;
                                                    break;
                                                }

                                                baseType = baseType.BaseType;
                                            }
                                        }

                                        if (!haveDrawerAlready)
                                        {
                                            PossibleDrawnTypes.Add(type);
                                        }

                                        AddTypeToGroups(type, preExistingEditorType);
                                    });
                }
                //Debug.Log("Found the following " + PossibleDrawnTypes.Count + " types that Odin can draw:\n\n" + string.Join("\n", PossibleDrawnTypes.Select(n => n.GetNiceFullName()).ToArray()));

                // Remove editor entries for any types that are not eligible to be drawn
                using (ProfileSection.Start("Remove non-eligible editor entries"))
                {
                    bool fixedAny = false;

                    foreach (var type in InspectorConfig.Instance.DrawingConfig.GetAllDrawnTypesWithEntries())
                    {
                        if (!PossibleDrawnTypes.Contains(type))
                        {
                            InspectorConfig.Instance.DrawingConfig.ClearEditorEntryForDrawnType(type);
                            fixedAny = true;
                        }
                    }

                    if (fixedAny)
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            //watch.Stop();
            //Debug.Log("Init took " + watch.Elapsed.TotalMilliseconds + " ms");
        }
        
        private enum DisplayType
        {
            AllUnityObjects,
            AllComponents,
            AllScriptableObjects,
            UserScripts
        }

        private class TypeGroup
        {
            public class TypePair
            {
                public readonly Type DrawnType;
                public readonly Type PreExistingEditorType;

                public TypePair(Type drawnType, Type preExistingEditorType)
                {
                    this.DrawnType = drawnType;
                    this.PreExistingEditorType = preExistingEditorType;
                }
            }

            public readonly string Name;
            public readonly List<TypeGroup> SubGroups = new List<TypeGroup>();
            public readonly List<TypePair> SubTypes = new List<TypePair>();
            private readonly List<bool> SubTypesVisible = new List<bool>();
            public bool HasConflict { get; private set; }
            public bool AllSubTypesVisible { get; private set; }
            public bool IsSearchVisible { get; private set; }
            public bool IsExpanded { get; set; }
            public bool HasEligibleTypes { get; private set; }

            public TypeGroup(string name)
            {
                this.Name = name;
                this.HasEligibleTypes = false;
            }

            public TypeGroup GetChildGroup(string name)
            {
                for (int i = 0; i < this.SubGroups.Count; i++)
                {
                    if (this.SubGroups[i].Name == name)
                    {
                        return this.SubGroups[i];
                    }
                }

                TypeGroup newGroup = new TypeGroup(name);
                this.SubGroups.Add(newGroup);
                return newGroup;
            }

            public void ExpandAll()
            {
                this.IsExpanded = true;

                for (int i = 0; i < this.SubGroups.Count; i++)
                {
                    this.SubGroups[i].ExpandAll();
                }
            }

            public void SetSharedEditorType(Type editorType)
            {
                this.HasConflict = false;

                for (int i = 0; i < this.SubTypes.Count; i++)
                {
                    if (this.SubTypes[i].PreExistingEditorType == null)
                    {
                        InspectorConfig.Instance.DrawingConfig.SetEditorType(this.SubTypes[i].DrawnType, editorType);
                    }
                }

                for (int i = 0; i < this.SubGroups.Count; i++)
                {
                    this.SubGroups[i].SetSharedEditorType(editorType);
                }
            }

            public void ClearEditorTypes()
            {
                this.HasConflict = false;

                for (int i = 0; i < this.SubTypes.Count; i++)
                {
                    InspectorConfig.Instance.DrawingConfig.ClearEditorEntryForDrawnType(this.SubTypes[i].DrawnType);
                }

                for (int i = 0; i < this.SubGroups.Count; i++)
                {
                    this.SubGroups[i].ClearEditorTypes();
                }
            }

            public Type GetSharedEditorType()
            {
                if (this.HasConflict)
                {
                    return null;
                }
                else
                {
                    if (this.SubTypes.Count > 0)
                    {
                        for (int i = 0; i < this.SubTypes.Count; i++)
                        {
                            if (this.SubTypes[i].PreExistingEditorType == null)
                            {
                                return InspectorConfig.Instance.DrawingConfig.GetEditorType(this.SubTypes[i].DrawnType);
                            }
                        }
                    }

                    for (int i = 0; i < this.SubGroups.Count; i++)
                    {
                        var result = this.SubGroups[i].GetSharedEditorType();

                        if (result != null)
                        {
                            return result;
                        }
                    }
                }

                return null;
            }

            public void UpdateHasEligibleTypes()
            {
                this.HasEligibleTypes = false;

                for (int i = 0; i < this.SubGroups.Count; i++)
                {
                    this.SubGroups[i].UpdateHasEligibleTypes();

                    if (this.SubGroups[i].HasEligibleTypes)
                    {
                        this.HasEligibleTypes = true;
                    }
                }

                for (int i = 0; i < this.SubTypes.Count; i++)
                {
                    if (this.SubTypes[i].PreExistingEditorType == null)
                    {
                        this.HasEligibleTypes = true;
                        break;
                    }
                }
            }

            public void UpdateConflicts()
            {
                this.HasConflict = false;

                for (int i = 0; i < this.SubGroups.Count; i++)
                {
                    this.SubGroups[i].UpdateConflicts();

                    if (this.SubGroups[i].HasConflict)
                    {
                        this.HasConflict = true;
                    }
                }

                Type editor = null;

                if (!this.HasConflict)
                {
                    for (int i = 0; i < this.SubTypes.Count; i++)
                    {
                        if (this.SubTypes[i].PreExistingEditorType == null)
                        {
                            editor = InspectorConfig.Instance.DrawingConfig.GetEditorType(this.SubTypes[i].DrawnType);
                            continue;
                        }
                    }

                    for (int i = 0; i < this.SubTypes.Count; i++)
                    {
                        if (this.SubTypes[i].PreExistingEditorType == null && InspectorConfig.Instance.DrawingConfig.GetEditorType(this.SubTypes[i].DrawnType) != editor)
                        {
                            this.HasConflict = true;
                            break;
                        }
                    }
                }

                if (!this.HasConflict && this.SubGroups.Count > 0)
                {
                    Type firstGroupEditor = null;

                    for (int i = 0; i < this.SubGroups.Count; i++)
                    {
                        if (this.SubGroups[i].HasEligibleTypes)
                        {
                            firstGroupEditor = this.SubGroups[i].GetSharedEditorType();
                            break;
                        }
                    }

                    bool compareEditor = this.SubTypes.Count > 0;

                    if (compareEditor && firstGroupEditor != editor)
                    {
                        this.HasConflict = true;
                    }

                    if (!this.HasConflict)
                    {
                        for (int i = 0; i < this.SubGroups.Count; i++)
                        {
                            if (!this.SubGroups[i].HasEligibleTypes) continue;

                            Type curEditor = this.SubGroups[i].GetSharedEditorType();

                            if ((compareEditor && curEditor != editor) || curEditor != firstGroupEditor)
                            {
                                this.HasConflict = true;
                                break;
                            }
                        }
                    }
                }
            }

            public void Sort()
            {
                this.SubGroups.Sort((a, b) => a.Name.CompareTo(b.Name));
                this.SubTypes.Sort((a, b) => a.DrawnType.Name.CompareTo(b.DrawnType.Name));

                foreach (var group in this.SubGroups)
                {
                    group.Sort();
                }
            }

            public bool IsTypeVisible(Type type)
            {
                for (int i = 0; i < this.SubTypes.Count; i++)
                {
                    if (this.SubTypes[i].DrawnType == type)
                    {
                        if (this.SubTypesVisible.Count > i)
                        {
                            return this.SubTypesVisible[i];
                        }

                        return false;
                    }
                }

                return false;
            }

            public void UpdateSearch(string search, DisplayType displayType)
            {
                this.IsSearchVisible = false;
                this.AllSubTypesVisible = true;

                this.SubTypesVisible.SetLength(this.SubTypes.Count);

                foreach (var group in this.SubGroups)
                {
                    group.UpdateSearch(search, displayType);

                    if (group.IsSearchVisible)
                    {
                        this.IsSearchVisible = true;
                    }

                    if (!group.AllSubTypesVisible)
                    {
                        this.AllSubTypesVisible = false;
                    }
                }

                bool searchIsNullOrWhitespace = search.IsNullOrWhitespace();

                if (searchIsNullOrWhitespace)
                {
                    if (displayType == DisplayType.AllUnityObjects)
                    {
                        this.IsSearchVisible = true;
                        this.AllSubTypesVisible = true;

                        for (int i = 0; i < this.SubTypesVisible.Count; i++)
                        {
                            this.SubTypesVisible[i] = true;
                        }

                        return;
                    }
                }

                for (int i = 0; i < this.SubTypes.Count; i++)
                {
                    Type type = this.SubTypes[i].DrawnType;

                    if ((displayType == DisplayType.AllScriptableObjects && !typeof(ScriptableObject).IsAssignableFrom(type))
                        || (displayType == DisplayType.AllComponents && !typeof(Component).IsAssignableFrom(type))
                        || (displayType == DisplayType.UserScripts && !((AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly) & AssemblyTypeFlags.CustomTypes) != 0 )))
                    {
                        this.SubTypesVisible[i] = false;
                        this.AllSubTypesVisible = false;
                        continue;
                    }

                    if (searchIsNullOrWhitespace || type.FullName.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.IsSearchVisible = true;
                        this.SubTypesVisible[i] = true;
                    }
                    else
                    {
                        this.SubTypesVisible[i] = false;
                        this.AllSubTypesVisible = false;
                    }
                }
            }
        }

        private struct ProfileSection : IDisposable
        {
#if PROFILE_LOGGING
            private System.Diagnostics.Stopwatch watch;
            private string name;

            public ProfileSection(string name)
            {
                this.name = name;
                this.watch = System.Diagnostics.Stopwatch.StartNew();
            }
#endif

            public static ProfileSection Start(string name)
            {
#if PROFILE_LOGGING
                return new ProfileSection(name);
#else
                return new ProfileSection();
#endif
            }

            public void Dispose()
            {
#if PROFILE_LOGGING
                watch.Stop();
                Debug.Log(name + " took " + watch.Elapsed.TotalMilliseconds + " ms");
#endif
            }
        }

        internal static void UpdateRootGroupHasEligibletypes()
        {
            UserTypesRootGroup.UpdateHasEligibleTypes();
            PluginTypesRootGroup.UpdateHasEligibleTypes();
            UnityTypesRootGroup.UpdateHasEligibleTypes();
            OtherTypesRootGroup.UpdateHasEligibleTypes();
        }

        internal static void UpdateRootGroupConflicts()
        {
            UserTypesRootGroup.UpdateConflicts();
            PluginTypesRootGroup.UpdateConflicts();
            UnityTypesRootGroup.UpdateConflicts();
            OtherTypesRootGroup.UpdateConflicts();
        }

        private static void SortRootGroups()
        {
            UserTypesRootGroup.Sort();
            PluginTypesRootGroup.Sort();
            UnityTypesRootGroup.Sort();
            OtherTypesRootGroup.Sort();
        }

        private static void UpdateRootGroupsSearch(string search, DisplayType displayType)
        {
            UserTypesRootGroup.UpdateSearch(search, displayType);
            PluginTypesRootGroup.UpdateSearch(search, displayType);
            UnityTypesRootGroup.UpdateSearch(search, displayType);
            OtherTypesRootGroup.UpdateSearch(search, displayType);
        }

        private static void AddTypeToGroups(Type type, Type preExistingEditorType)
        {
            var assemblyType = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);

            TypeGroup group;

            switch (assemblyType)
            {
                case AssemblyTypeFlags.UserTypes:
                case AssemblyTypeFlags.UserEditorTypes:
                    group = UserTypesRootGroup;
                    break;

                case AssemblyTypeFlags.PluginTypes:
                case AssemblyTypeFlags.PluginEditorTypes:
                    group = PluginTypesRootGroup;
                    break;

                case AssemblyTypeFlags.UnityTypes:
                case AssemblyTypeFlags.UnityEditorTypes:
                    group = UnityTypesRootGroup;
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
                    group = OtherTypesRootGroup;
                    break;
            }

            if (type.Namespace != null)
            {
                string[] groups = type.Namespace.Split('.');

                for (int i = 0; i < groups.Length; i++)
                {
                    group = group.GetChildGroup(groups[i]);
                }
            }

            group.SubTypes.Add(new TypeGroup.TypePair(type, preExistingEditorType));
        }

        /// <summary>
        /// Determines whether Odin is capable of creating a custom editor for a given type.
        /// </summary>
        public static bool OdinCanCreateEditorFor(Type type)
        {
            return PossibleDrawnTypes.Contains(type);
        }

        /// <summary>
        /// Gets an array of all assigned editor types, and the types they have to draw.
        /// </summary>
        public static TypeDrawerPair[] GetEditors()
        {
            return GetEditorsForCompilation(UserTypesRootGroup)
                .AppendWith(GetEditorsForCompilation(PluginTypesRootGroup))
                .AppendWith(GetEditorsForCompilation(UnityTypesRootGroup))
                .AppendWith(GetEditorsForCompilation(OtherTypesRootGroup))
                .ToArray();
        }

        private static GUIStyle iconStyle;

        private static GUIStyle IconStyle
        {
            get
            {
                if (iconStyle == null)
                {
                    iconStyle = new GUIStyle() { margin = new RectOffset(5, 0, 4, 0) };
                }

                return iconStyle;
            }
        }

        private static IEnumerable<TypeDrawerPair> GetEditorsForCompilation(TypeGroup group)
        {
            foreach (var type in group.SubTypes)
            {
                var editor = InspectorConfig.Instance.DrawingConfig.GetEditorType(type.DrawnType);

                if (editor != null && editor != typeof(InspectorTypeDrawingConfig.MissingEditor))
                {
                    yield return new TypeDrawerPair(type.DrawnType, editor);
                }
            }

            foreach (var subGroup in group.SubGroups)
            {
                foreach (var pair in GetEditorsForCompilation(subGroup))
                {
                    yield return pair;
                }
            }
        }

        protected override void Initialize()
        {
            if (!initializedForDrawing)
            {
                UpdateRootGroupHasEligibletypes();
                UpdateRootGroupConflicts();
                SortRootGroups();
                UpdateRootGroupsSearch("", DisplayType.AllUnityObjects);

                initializedForDrawing = true;
            }

            this.searchText = "";
            this.displayType = DisplayType.AllUnityObjects;
        }

        private string searchText;
        private DisplayType displayType;
        private Vector2 scrollPos;

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            GUILayout.Label("Draw Odin for", GUILayoutOptions.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            SirenixEditorGUI.VerticalLineSeparator();

            GUI.changed = false;
            this.searchText = SirenixEditorGUI.ToolbarSearchField(this.searchText, false);
            if (GUI.changed)
            {
                UpdateRootGroupsSearch(this.searchText, this.displayType);
            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent(" Reset to default ")))
            {
                var asset = InspectorConfig.Instance;

                if (EditorUtility.DisplayDialog("Reset " + asset.name + " to default", "Are you sure you want to reset all settings on " + asset.name + " to default values? This cannot be undone.", "Yes", "No"))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
                    AssetDatabase.Refresh();
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            SirenixEditorGUI.BeginVerticalList(true, false, GUILayoutOptions.ExpandHeight(false));
            {
                this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayoutOptions.ExpandWidth(true));
                {
                    DrawRootTypeGroup(InspectorDefaultEditors.UserTypes, ValueEntry, this.searchText);
                    DrawRootTypeGroup(InspectorDefaultEditors.PluginTypes, ValueEntry, this.searchText);
                    DrawRootTypeGroup(InspectorDefaultEditors.UnityTypes, ValueEntry, this.searchText);
                    DrawRootTypeGroup(InspectorDefaultEditors.OtherTypes, ValueEntry, this.searchText);
                }
                EditorGUILayout.EndScrollView();
            }
            SirenixEditorGUI.EndVerticalList();
        }

        private void DrawRootTypeGroup(InspectorDefaultEditors editorCategory, IPropertyValueEntry<InspectorTypeDrawingConfig> entry, string searchText)
        {
            TypeGroup typeGroup;

            switch (editorCategory)
            {
                case InspectorDefaultEditors.UserTypes:
                    typeGroup = UserTypesRootGroup;
                    break;

                case InspectorDefaultEditors.PluginTypes:
                    typeGroup = PluginTypesRootGroup;
                    break;

                case InspectorDefaultEditors.UnityTypes:
                    typeGroup = UnityTypesRootGroup;
                    break;

                case InspectorDefaultEditors.OtherTypes:
                default:
                    typeGroup = OtherTypesRootGroup;
                    break;
            }

            if (typeGroup.SubTypes.Count == 0 && typeGroup.SubGroups.Count == 0)
            {
                SirenixEditorGUI.BeginListItem();
                {
                    SirenixEditorGUI.BeginIndentedHorizontal();
                    {
                        GUIHelper.PushGUIEnabled(false);
                        {
                            SirenixEditorGUI.IconButton(EditorIcons.TriangleRight, IconStyle, 16);
                            GUILayoutUtility.GetRect(16, 16, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(false).Width(16));
                            GUILayout.Label(typeGroup.Name);
                        }
                        GUIHelper.PopGUIEnabled();
                    }
                    SirenixEditorGUI.EndIndentedHorizontal();
                }
                SirenixEditorGUI.EndListItem();
            }
            else
            {
                bool useToggle = true;

                var rect = SirenixEditorGUI.BeginListItem();
                {
                    bool toggleExpansion = false;

                    SirenixEditorGUI.BeginIndentedHorizontal();
                    {
                        EditorIcon icon = (typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()) ? EditorIcons.TriangleDown : EditorIcons.TriangleRight;

                        toggleExpansion = SirenixEditorGUI.IconButton(icon, IconStyle, 16);

                        if (useToggle)
                        {
                            EditorGUI.showMixedValue = typeGroup.HasConflict;

                            bool isToggled = typeGroup.HasConflict || typeGroup.GetSharedEditorType() == typeof(OdinEditor);

                            GUI.changed = false;
                            isToggled = EditorGUI.Toggle(GUILayoutUtility.GetRect(16, 16, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(false).Width(16)), isToggled);

                            if (GUI.changed)
                            {
                                typeGroup.ClearEditorTypes();

                                if (isToggled)
                                {
                                    // Add rule flag
                                    InspectorConfig.Instance.DefaultEditorBehaviour |= editorCategory;
                                }
                                else
                                {
                                    // Remove rule flag
                                    InspectorConfig.Instance.DefaultEditorBehaviour = InspectorConfig.Instance.DefaultEditorBehaviour & ~editorCategory;
                                }

                                EditorUtility.SetDirty(InspectorConfig.Instance);
                                InspectorConfig.Instance.UpdateOdinEditors();
                            }

                            EditorGUI.showMixedValue = false;
                        }
                        else
                        {
                            GUILayout.Label("TODO: DROPDOWN!");
                        }

                        GUILayout.Label(typeGroup.Name);
                    }
                    SirenixEditorGUI.EndIndentedHorizontal();

                    if (toggleExpansion || (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)))
                    {
                        typeGroup.IsExpanded = !typeGroup.IsExpanded;
                        Event.current.Use();
                    }
                }
                SirenixEditorGUI.EndListItem();

                if (SirenixEditorGUI.BeginFadeGroup(typeGroup, typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()))
                {
                    EditorGUI.indentLevel++;

                    foreach (var subType in typeGroup.SubTypes)
                    {
                        if (typeGroup.IsTypeVisible(subType.DrawnType))
                        {
                            this.DrawType(subType, entry);
                        }
                    }

                    foreach (var subGroup in typeGroup.SubGroups)
                    {
                        this.DrawTypeGroup(subGroup, entry, searchText);
                    }

                    EditorGUI.indentLevel--;
                }
                SirenixEditorGUI.EndFadeGroup();
            }
        }

        private void DrawTypeGroup(TypeGroup typeGroup, IPropertyValueEntry<InspectorTypeDrawingConfig> entry, string searchText)
        {
            if (!typeGroup.IsSearchVisible)
            {
                return;
            }

            bool useToggle = true;

            var rect = SirenixEditorGUI.BeginListItem();
            {
                bool toggleExpansion = false;

                SirenixEditorGUI.BeginIndentedHorizontal();
                {
                    EditorIcon icon = (typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()) ? EditorIcons.TriangleDown : EditorIcons.TriangleRight;

                    toggleExpansion = SirenixEditorGUI.IconButton(icon, IconStyle, 16);

                    if (!typeGroup.HasEligibleTypes)
                    {
                        toggleExpansion |= SirenixEditorGUI.IconButton(EditorIcons.Transparent, 20);
                    }
                    else
                    {
                        if (useToggle)
                        {
                            EditorGUI.showMixedValue = typeGroup.HasConflict;

                            bool isToggled = typeGroup.HasConflict || typeGroup.GetSharedEditorType() == typeof(OdinEditor);

                            GUI.changed = false;
                            isToggled = EditorGUI.Toggle(GUILayoutUtility.GetRect(16, 16, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(false).Width(16)), isToggled);

                            if (GUI.changed)
                            {
                                typeGroup.SetSharedEditorType(isToggled ? typeof(OdinEditor) : null);
                                UpdateRootGroupConflicts();
                                InspectorConfig.Instance.UpdateOdinEditors();
                            }

                            EditorGUI.showMixedValue = false;
                        }
                        else
                        {
                            GUILayout.Label("TODO: DROPDOWN!");
                        }
                    }

                    GUILayout.Label(typeGroup.Name);
                }
                SirenixEditorGUI.EndIndentedHorizontal();

                if (toggleExpansion || (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)))
                {
                    typeGroup.IsExpanded = !typeGroup.IsExpanded;
                    Event.current.Use();
                }
            }
            SirenixEditorGUI.EndListItem();

            if (SirenixEditorGUI.BeginFadeGroup(typeGroup, typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()))
            {
                EditorGUI.indentLevel++;

                foreach (var subType in typeGroup.SubTypes)
                {
                    if (typeGroup.IsTypeVisible(subType.DrawnType))
                    {
                        this.DrawType(subType, entry);
                    }
                }

                foreach (var subGroup in typeGroup.SubGroups)
                {
                    this.DrawTypeGroup(subGroup, entry, searchText);
                }

                EditorGUI.indentLevel--;
            }
            SirenixEditorGUI.EndFadeGroup();
        }

        private void DrawType(TypeGroup.TypePair typeToDraw, IPropertyValueEntry<InspectorTypeDrawingConfig> entry)
        {
            Type currentEditorType = typeToDraw.PreExistingEditorType;
            bool conflict = false;

            if (currentEditorType == null)
            {
                for (int i = 0; i < entry.Values.Count; i++)
                {
                    var type = entry.Values[i].GetEditorType(typeToDraw.DrawnType);

                    if (i == 0)
                    {
                        currentEditorType = type;
                    }
                    else if (type != currentEditorType)
                    {
                        currentEditorType = null;
                        conflict = true;
                        break;
                    }
                }
            }

            bool useToggle = true;

            SirenixEditorGUI.BeginListItem();
            {
                SirenixEditorGUI.BeginIndentedHorizontal();
                SirenixEditorGUI.IconButton(EditorIcons.Transparent, IconStyle, 16);

                if (typeToDraw.PreExistingEditorType != null)
                {
                    SirenixEditorGUI.IconButton(EditorIcons.Transparent, IconStyle, 16);
                    GUILayout.Label(typeToDraw.DrawnType.GetNiceName());
                    GUILayout.Label("Drawn by '" + typeToDraw.PreExistingEditorType + "'", SirenixGUIStyles.RightAlignedGreyMiniLabel);

                    for (int i = 0; i < entry.Values.Count; i++)
                    {
                        if (entry.Values[i].HasEntryForType(typeToDraw.DrawnType))
                        {
                            entry.Values[i].ClearEditorEntryForDrawnType(typeToDraw.DrawnType);
                        }
                    }
                }
                else
                {
                    EditorGUI.showMixedValue = conflict;

                    if (useToggle)
                    {
                        bool isToggled = currentEditorType == typeof(OdinEditor);

                        GUI.changed = false;
                        isToggled = EditorGUI.Toggle(GUILayoutUtility.GetRect(16, 16, EditorStyles.toggle, GUILayoutOptions.ExpandWidth(false).Width(16)), isToggled);

                        if (GUI.changed)
                        {
                            for (int i = 0; i < entry.Values.Count; i++)
                            {
                                entry.Values[i].SetEditorType(typeToDraw.DrawnType, isToggled ? typeof(OdinEditor) : null);
                            }

                            UpdateRootGroupConflicts();
                            InspectorConfig.Instance.UpdateOdinEditors();
                        }

                        GUILayout.Label(typeToDraw.DrawnType.GetNiceName());
                    }
                    else
                    {
                        GUILayout.Label("TODO: DROPDOWN!");
                    }
                }

                SirenixEditorGUI.EndIndentedHorizontal();
                EditorGUI.showMixedValue = false;
            }
            SirenixEditorGUI.EndListItem();
        }
    }
}
#endif
#endif