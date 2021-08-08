#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InstanceCreatorWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    internal class InstanceCreatorWindow : EditorWindow
    {
        // In Unity 2017.1's .NET 4.6 profile, Tuples implement System.ITuple. In Unity 2017.2 and up, tuples implement System.ITupleInternal instead for some reason.
        private static readonly Type tupleInterface = typeof(string).Assembly.GetType("System.ITuple") ?? typeof(string).Assembly.GetType("System.ITupleInternal");

        private static Dictionary<Type, TypeTreeNode> typeTrees = new Dictionary<Type, TypeTreeNode>();

        private Type chosenType;
        private bool hasSearchTerm;
        private TypeTreeNode RootNode;
        private bool searchChanged;
        private string searchTerm;
        private bool autoSelectFirst;
        private bool hideFoldoutLabels;

        private static GUIStyle toolbarBackgroundChainedTop;

        private static GUIStyle ToolbarBackgroundChainedTop
        {
            get
            {
                if (toolbarBackgroundChainedTop == null)
                {
                    toolbarBackgroundChainedTop = new GUIStyle(SirenixGUIStyles.ToolbarBackground)
                    {
                        overflow = new RectOffset(1, 1, 1, 0),
                    };
                }
                return toolbarBackgroundChainedTop;
            }
        }

        internal void Initialize()
        {
            this.autoSelectFirst = Event.current != null && Event.current.modifiers == EventModifiers.Control;
            this.RootNode = this.GetTypeTree(InstanceCreator.Type);
            if (this.chosenType != null)
            {
                this.SetSelectedType();
                this.chosenType = null;
            }
            else
            {
                this.titleContent = new GUIContent("Instance Creator");
                this.titleContent = new GUIContent("Create Instance of " + InstanceCreator.Type.GetNiceName());
            }
        }

        private static bool CanCreateInstance(Type baseType, ref Type type)
        {
            if (type.IsArray && type.InheritsFrom(baseType))
            {
                return true;
            }

            if (type.IsAbstract || type.IsInterface)
            {
                return false;
            }

            if (type.InheritsFrom(typeof(UnityEngine.Object))) return false;

            if (baseType.IsGenericType && type.IsGenericTypeDefinition)
            {
                var def = baseType.GetGenericTypeDefinition();

                if (type.ImplementsOpenGenericType(def))
                {
                    var args = baseType.GetGenericArguments();

                    if (type.AreGenericConstraintsSatisfiedBy(args))
                    {
                        var newType = type.MakeGenericType(args);
                        if (baseType.IsAssignableFrom(newType))
                        {
                            type = newType;
                            return true;
                        }
                    }

                    // Maybe in the future, we can do some fancy stuff with inferral - for now, it seems like it's not merited
                    // I'll leave the code here, for possible future use.
                    // - Tor 31/07/2018

                    //Type[] inferredArgs;

                    //if (type.TryInferGenericParameters(out inferredArgs, args))
                    //{
                    //    var newType = type.MakeGenericType(args);
                    //    if (baseType.IsAssignableFrom(newType))
                    //    {
                    //        type = newType;
                    //        return true;
                    //    }
                    //}
                }
            }

            if (!(type.IsGenericTypeDefinition && baseType.IsGenericType && type.GetGenericArguments().Length == baseType.GetGenericArguments().Length))
            {
                if (type.IsGenericTypeDefinition)
                {
                    return false;
                }
            }

            if (type == typeof(NullType) || (baseType == typeof(object) && (type.IsValueType || type == typeof(string) || type.IsEnum) && !type.IsDefined<CompilerGeneratedAttribute>()))
            {
                return true;
            }

            if (baseType.IsInterface && baseType.GetGenericArguments().Length > 0 && type.ImplementsOpenGenericInterface(baseType.GetGenericTypeDefinition()))
            {
                if (type.IsGenericTypeDefinition)
                {
                    if (!type.AreGenericConstraintsSatisfiedBy(baseType.GetGenericArguments()))
                    {
                        return false;
                    }
                }
            }

            return !type.InheritsFrom(typeof(UnityEngine.Object)) &&
                   !type.InheritsFrom(typeof(UnityEngine.ScriptableObject)) &&
                   !type.InheritsFrom(typeof(UnityEditor.SerializedObject)) &&
                   !type.Name.StartsWith("<", StringComparison.InvariantCulture)
                   && type.InheritsFrom(baseType);
        }

        private TypeTreeNode GetTypeTree(Type type)
        {
            TypeTreeNode rootNode;
            if (typeTrees.TryGetValue(type, out rootNode) == false)
            {
                var typeFlag = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);

                var additionalTypes = new List<Type>() {
                    typeof(NullType),
                    type,
                    typeof(string),
                    typeof(List<>).MakeGenericType(type),
                    type.MakeArrayType(),
                    type.MakeArrayType(2),
                    type.MakeArrayType(3)
                };

                var additions = additionalTypes
                    .Select(x => new
                    {
                        type = x,
                        flag = (x == typeof(NullType) || x.IsArray) ? typeFlag : AssemblyUtilities.GetAssemblyTypeFlag(x.Assembly)
                    })
                    .ToArray();

                rootNode = typeTrees[type] = new TypeTreeNode()
                {
                    ChildNodes = AssemblyUtilities
                    .GetAllAssemblies()
                    .Select(x => new { assembly = x, flag = AssemblyUtilities.GetAssemblyTypeFlag(x) })
                    .GroupBy(a => a.flag)
                    .Select(a => new TypeTreeNode()
                    {
                        AssemblyTypeFlag = a.Key,
                        IsVisible = (a.Key & AssemblyTypeFlags.UserTypes) != 0,
                        ChildNodes = AssemblyUtilities
                            .GetTypes(a.Key)
                            .Where(x => x != type)// ((a.Key & AssemblyTypeFlags.UserTypes) != 0 || x.IsPublic) && x != type) // Only include public types or types from User assembly.
                            .PrependWith(additions.Where(x => x.flag == a.Key).Select(x => x.type))
                            .Select(t => CanCreateInstance(type, ref t) ? t : null)
                            .Where(t => t != null)
                            .Distinct()
                            .GroupBy(t => ((t == typeof(NullType) || t.IsArray) ? type : t).Namespace ?? "ALSO!GROUP!ME")
                            .Select(g => new TypeTreeNode()
                            {
                                Namespace = (g.Key == "ALSO!GROUP!ME") ? null : g.Key,
                                ChildNodes = g
                                    .Select(t => new TypeTreeNode() { Type = t })
                                    .Where(x => x.Type == null || x.Type.GetNiceName().Contains('$') == false)
                                    .OrderBy(t => t.Type == typeof(NullType))
                                    .ThenBy(t => t.Type == null ? "" : t.Type.Name)
                                    .ToList()
                            })
                            .Where(x => x.Type == null || x.Type.GetNiceName().Contains('$') == false)
                            .OrderBy(x => x.Namespace)
                            .ThenBy(x => x.Type)
                            .ToList()
                    })
                    .OrderBy(x => x.AssemblyTypeFlag)
                    .ToList()
                };
                rootNode.IsVisible = true;
                rootNode.Initialize(this, null);
            }
            else
            {
                rootNode.Initialize(this, null);
            }

            this.hideFoldoutLabels = rootNode.EnumerateTree().Count(x => x.Type != null) < 20;

            if (this.hideFoldoutLabels)
            {
                foreach (var item in rootNode.EnumerateTree())
                {
                    item.IsVisible = true;
                }
            }
            else
            {
                var firstCollection = rootNode.EnumerateTree().FirstOrDefault(x => x.ChildNodes != null && x.ChildNodes.Count > 0);
                if (firstCollection != null)
                {
                    firstCollection.IsVisible = true;
                }
            }

            var firstTypeNode = rootNode.EnumerateTree().FirstOrDefault(x => x.Type != null);
            if (firstTypeNode != null)
            {
                firstTypeNode.ForceSetSelected = true;
            }

            if (this.autoSelectFirst && firstTypeNode != null)
            {
                this.chosenType = firstTypeNode.Type;
            }

            return rootNode;
        }

        private void OnGUI()
        {
            this.Focus();
            this.wantsMouseMove = true;

            if (InstanceCreator.Type == null)
            {
                this.Close();
                return;
            }
            else
            {
                this.RootNode = this.RootNode ?? this.GetTypeTree(InstanceCreator.Type);

                SirenixEditorGUI.BeginHorizontalToolbar(ToolbarBackgroundChainedTop);
                {
                    this.searchChanged = this.searchTerm != (this.searchTerm = SirenixEditorGUI.ToolbarSearchField(this.searchTerm, true)) || this.searchChanged;
                }
                SirenixEditorGUI.EndHorizontalToolbar();

                SirenixEditorGUI.BeginVerticalMenuList(this);
                {
                    this.RootNode.DrawItem();
                }
                SirenixEditorGUI.EndVerticalMenuList();

                SirenixEditorGUI.DrawBorders(new Rect(0, 0, this.position.width, this.position.height), 1, 1, 1, 1, SirenixGUIStyles.BorderColor);

                if (Event.current.type == EventType.Repaint)
                {
                    this.hasSearchTerm = this.searchTerm != null && (this.searchTerm.Length >= 2 || (this.searchTerm.Length == 1 && !char.IsLetter(this.searchTerm[0])));

                    if (this.searchChanged)
                    {
                        this.RootNode.UpdateSearchTerm();

                        this.searchChanged = false;
                    }
                }

                if (this.chosenType != null)
                {
                    this.SetSelectedType();
                    this.chosenType = null;
                    this.Close();
                    return;
                }

                this.RepaintIfRequested();
            }
        }

        private void SelectChosenType()
        {
            if (this.chosenType != null)
            {
                if (this.chosenType.InheritsFrom(typeof(ScriptableObject)))
                {
                    InstanceCreator.CreatedInstance = ScriptableObject.CreateInstance(this.chosenType);
                }
                else
                {
                    InstanceCreator.CreatedInstance = this.CreateInstance();
                }
                InstanceCreator.HasCreatedInstance = true;
                this.chosenType = null;
                this.Close();
            }
        }

        private void SetSelectedType()
        {
            if (this.chosenType == typeof(NullType))
            {
                InstanceCreator.CreatedInstance = null;
            }
            else if (this.chosenType.InheritsFrom(typeof(ScriptableObject)))
            {
                InstanceCreator.CreatedInstance = ScriptableObject.CreateInstance(this.chosenType);
            }
            else
            {
                InstanceCreator.CreatedInstance = this.CreateInstance();
            }
            InstanceCreator.HasCreatedInstance = true;
        }

        private object CreateInstance()
        {
            if (this.chosenType.IsArray)
            {
                return Array.CreateInstance(this.chosenType.GetElementType(), new long[this.chosenType.GetArrayRank()]);
                //return Activator.CreateInstance(this.chosenType, new object[] { (int)0 });
            }
            else if (this.chosenType == typeof(string))
            {
                return "";
            }
            else if (tupleInterface != null && tupleInterface.IsAssignableFrom(this.chosenType))
            {
                return FormatterServices.GetUninitializedObject(this.chosenType);
            }

            if (this.chosenType.GetConstructor(Type.EmptyTypes) != null)
            {
                if (this.chosenType.IsGenericTypeDefinition)
                {
                    var genericType = this.chosenType.MakeGenericType(InstanceCreator.Type.GetGenericArguments());
                    return Activator.CreateInstance(genericType);
                }

                return Activator.CreateInstance(this.chosenType);
            }

            return FormatterServices.GetUninitializedObject(this.chosenType);
        }

        public float GetWindowHeight()
        {
            var count = this.RootNode.EnumerateTree().Where(x => x.Type != null).Count();
            return Mathf.Clamp(count * 22 + 23, 23 + 22, 400);
        }

        private class NullType
        {
        }

        private class TypeTreeNode
        {
            public AssemblyTypeFlags AssemblyTypeFlag = AssemblyTypeFlags.None;
            public List<TypeTreeNode> ChildNodes;
            public bool ForceSetSelected = false;
            public bool IsVisible;
            public string Namespace;
            public TypeTreeNode ParentNode;
            public Type Type;

            private static Texture typeIcon = GUIHelper.GetAssetThumbnail(null, typeof(MonoBehaviour), false);

            private bool isAssemblyFlagNode;
            private bool isNameSpaceNode;
            private bool isTypeNode;
            private GUIContent label;
            private string labelName;
            private string searchString;
            private bool matchesSearchTerm = false;
            private InstanceCreatorWindow window;
            private bool drawHasNoEmptyConstructor;
            private GUIContent hasNoEmptyConstructorLabel;

            //private bool showNotSerializableLabel;
            //private static GUIContent isNotSerializableLabel = new GUIContent("Not serializable ");

            public void PrependType(Type type)
            {
                if (this.ChildNodes == null)
                {
                    this.ChildNodes = new List<TypeTreeNode>();
                }
                var specialType = type == typeof(NullType) ? InstanceCreator.Type : type;
                var flag = AssemblyUtilities.GetAssemblyTypeFlag(specialType.Assembly);
                var node = this.ChildNodes.FirstOrDefault(x => x.AssemblyTypeFlag == flag);
                if (node == null)
                {
                    node = new TypeTreeNode() { AssemblyTypeFlag = flag };
                    this.ChildNodes.Insert(0, node);
                }

                if (node.ChildNodes == null)
                {
                    node.ChildNodes = new List<TypeTreeNode>();
                }

                if (string.IsNullOrEmpty(specialType.Namespace))
                {
                    node.ChildNodes.Insert(0, new TypeTreeNode() { Type = type });
                }
                else
                {
                    var nsNode = node.ChildNodes.FirstOrDefault(x => x.Namespace == specialType.Namespace);
                    if (nsNode == null)
                    {
                        nsNode = new TypeTreeNode() { Namespace = specialType.Namespace };
                        node.ChildNodes.Insert(0, nsNode);
                    }

                    if (nsNode.ChildNodes == null)
                    {
                        nsNode.ChildNodes = new List<TypeTreeNode>();
                    }

                    nsNode.ChildNodes.Insert(0, new TypeTreeNode() { Type = type });
                }
            }

            public void DrawItem()
            {
                bool hasLabel = this.labelName != null && (this.matchesSearchTerm || this.window.hasSearchTerm == false);
                if (hasLabel)
                {
                    bool isSelected = false, isChosen = false;
                    if (this.ChildNodes != null)
                    {
                        if (this.window.hideFoldoutLabels == false)
                        {
                            SirenixEditorGUI.BeginMenuListItem(out isSelected, out isChosen, this.ForceSetSelected);
                            {
                                if (this.matchesSearchTerm)
                                {
                                    SirenixEditorGUI.Foldout(true, this.label);
                                }
                                else
                                {
                                    this.IsVisible = SirenixEditorGUI.Foldout(this.IsVisible, this.label);
                                }
                            }
                            SirenixEditorGUI.EndMenuListItem();
                        }
                    }
                    else
                    {
                        SirenixEditorGUI.BeginMenuListItem(out isSelected, out isChosen, this.ForceSetSelected);
                        {
                            //if (this.drawHasNoEmptyConstructor)
                            //{
                            //    GUIHelper.PushGUIEnabled(false);
                            //}

                            // Properbly a type
                            if (this.isTypeNode && this.Type == typeof(NullType))
                            {
                                EditorGUILayout.LabelField(this.label, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                            }
                            else
                            {
                                EditorGUILayout.LabelField(this.label);
                            }

                            //if (this.drawHasNoEmptyConstructor)
                            //{
                            //    GUIHelper.PopGUIEnabled();
                            //}

                            if (this.drawHasNoEmptyConstructor)
                            {
                                var rect = GUILayoutUtility.GetLastRect();

                                rect.width -= 16;

                                EditorIcons.AlertTriangle.Draw(new Rect(rect.xMax, rect.yMin, 16, 16));

                                GUI.Label(rect, this.hasNoEmptyConstructorLabel, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                                //isChosen = false;
                            }
                            //var rect = GUILayoutUtility.GetLastRect();
                            //if (this.isTypeNode && this.showNotSerializableLabel)
                            //{
                            //    GUI.Label(rect, isNotSerializableLabel, isSelected ? SirenixGUIStyles.RightAlignedWhiteMiniLabel : SirenixGUIStyles.RightAlignedGreyMiniLabel);
                            //}
                        }
                        SirenixEditorGUI.EndMenuListItem();
                    }

                    if (isSelected && Event.current.type == EventType.KeyDown)
                    {
                        if (Event.current.keyCode == KeyCode.RightArrow)
                        {
                            this.IsVisible = true;
                        }
                        else if (Event.current.keyCode == KeyCode.LeftArrow)
                        {
                            this.IsVisible = false;
                        }
                        else if (Event.current.keyCode == KeyCode.Return)
                        {
                            isChosen = true;
                        }
                    }

                    if (isChosen)
                    {
                        this.IsVisible = !this.IsVisible;

                        if (this.isTypeNode)
                        {
                            this.window.chosenType = this.Type;
                        }
                    }

                    this.ForceSetSelected = false;
                }

                if (this.labelName == null)
                {
                    this.IsVisible = true;
                }

                if (this.ChildNodes != null)
                {
                    if (this.matchesSearchTerm || SirenixEditorGUI.BeginFadeGroup(this, this.IsVisible))
                    {
                        if (hasLabel && !this.window.hideFoldoutLabels)
                        {
                            EditorGUI.indentLevel++;
                        }
                        for (int i = 0; i < this.ChildNodes.Count; i++)
                        {
                            this.ChildNodes[i].DrawItem();
                        }
                        if (hasLabel && !this.window.hideFoldoutLabels)
                        {
                            EditorGUI.indentLevel--;
                        }
                    }
                    if (this.matchesSearchTerm == false)
                    {
                        SirenixEditorGUI.EndFadeGroup();
                    }
                }
            }

            public IEnumerable<TypeTreeNode> EnumerateTree()
            {
                yield return this;

                if (this.ChildNodes != null)
                {
                    foreach (var item in this.ChildNodes)
                    {
                        foreach (var node in item.EnumerateTree())
                        {
                            yield return node;
                        }
                    }
                }
            }

            public void Initialize(InstanceCreatorWindow window, TypeTreeNode parent)
            {
                this.ParentNode = parent;
                this.isNameSpaceNode = string.IsNullOrEmpty(this.Namespace) == false;
                this.window = window;
                this.isTypeNode = this.Type != null;
                this.isAssemblyFlagNode = this.AssemblyTypeFlag != AssemblyTypeFlags.None;
                this.labelName = null;

                if (this.isNameSpaceNode)
                {
                    if (this.ChildNodes != null && this.ChildNodes.Count > 0)
                    {
                        this.labelName = this.Namespace;
                        this.label = new GUIContent(this.labelName);
                    }
                }
                else if (this.isAssemblyFlagNode)
                {
                    if (this.ChildNodes != null && this.ChildNodes.Count > 0)
                    {
                        this.labelName = this.AssemblyTypeFlag.ToString().SplitPascalCase();
                        this.label = new GUIContent(this.labelName);
                    }
                }
                else if (this.isTypeNode)
                {
                    this.labelName = this.Type == typeof(NullType) ? "Null (" + InstanceCreator.Type.GetNiceName() + ")" : this.Type.GetNiceName();
                    this.label = new GUIContent(this.labelName, typeIcon);
                    //this.showNotSerializableLabel = (this.Type.Attributes & TypeAttributes.Serializable) == 0 && this.Type != typeof(NullType) && this.Type.Assembly.GetAssemblyTypeFlag() == AssemblyTypeFlags.UserTypes;

                    if (this.Type.IsValueType
                        || this.Type == typeof(string)
                        || this.Type.IsArray
                        || (tupleInterface != null && tupleInterface.IsAssignableFrom(this.Type)))
                    {
                        // These can always be created
                        this.drawHasNoEmptyConstructor = false;
                    }
                    else
                    {
                        this.drawHasNoEmptyConstructor = this.Type.GetConstructor(Type.EmptyTypes) == null;
                    }

                    //this.drawHasNoEmptyConstructor =
                    //  !(!this.Type.IsPrimitive &&
                    //    !this.Type.IsValueType &&
                    //    !this.Type.IsEnum &&
                    //    this.Type != typeof(string) &&
                    //    this.Type.GetConstructor(Type.EmptyTypes) != null);

                    //if (this.Type.IsArray || (tupleInterface != null && tupleInterface.IsAssignableFrom(this.Type)))
                    //{
                    //    this.drawHasNoEmptyConstructor = false;
                    //}

                    if (this.drawHasNoEmptyConstructor)
                    {
                        this.hasNoEmptyConstructorLabel = new GUIContent("No default constructor found");
                    }
                }

                if (this.Type != null)
                {
                    this.searchString = this.Type.GetNiceFullName();
                }
                else
                {
                    this.searchString = this.labelName;
                }

                if (this.ChildNodes != null)
                {
                    foreach (var item in this.ChildNodes)
                    {
                        item.Initialize(this.window, this);
                    }
                }
            }

            public void UpdateSearchTerm()
            {
                this.matchesSearchTerm = false;

                if (this.ChildNodes != null)
                {
                    foreach (var item in this.ChildNodes)
                    {
                        item.UpdateSearchTerm();
                    }
                }

                if (this.isTypeNode && this.searchString != null)
                {
                    if (this.window.hasSearchTerm && FuzzySearch.Contains(this.window.searchTerm, this.searchString))
                    {
                        this.SetMatchesSearchTermRecursive(true);
                    }
                }
            }

            private void SetMatchesSearchTermRecursive(bool value)
            {
                this.matchesSearchTerm = value;
                if (this.ParentNode != null)
                {
                    this.ParentNode.SetMatchesSearchTermRecursive(value);
                }
            }
        }
    }

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public static class InstanceCreator
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static int ControlID { get; private set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Type CurrentSelectedType { get; internal set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool HasCreatedInstance { get; internal set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Type Type { get; private set; }

        internal static object CreatedInstance { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static object GetCreatedInstance()
        {
            if (HasCreatedInstance)
            {
                Type = null;
                ControlID = 0;
                var instance = CreatedInstance;
                CreatedInstance = null;
                HasCreatedInstance = false;
                return instance;
            }
            else
            {
                Debug.LogError("Check if HasCreatedInstance is true before calling GetCreatedInstance.");
                return null;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static void Show(Type type, int controlId, Rect buttonRect = default(Rect))
        {
            Type = type;
            ControlID = controlId;

            var window = EditorWindow.CreateInstance<InstanceCreatorWindow>();
            window.Initialize();

            if (Event.current != null && Event.current.modifiers == EventModifiers.Control)
            {
                UnityEngine.Object.DestroyImmediate(window);
                return;
            }

            if (buttonRect.width > 0 && buttonRect.height > 0)
            {
                var prevWidth = buttonRect.width;
                buttonRect.width = Mathf.Clamp(prevWidth, 250, 500);
                buttonRect.x += (prevWidth - buttonRect.width);

                var windowSize = new Vector2(buttonRect.width, Mathf.Min(buttonRect.width, window.GetWindowHeight()));

                if (Event.current != null)
                {
                    buttonRect.position = GUIUtility.GUIToScreenPoint(buttonRect.position);
                }

                window.ShowAsDropDown(buttonRect, windowSize);
            }
            else
            {
                var windowSize = new Vector2(500, 500);
                var windowRect = GUIHelper.GetEditorWindowRect();
                window.ShowAuxWindow();
                window.position = new Rect(windowRect.center - windowSize * 0.5f, new Vector2(windowSize.x, window.GetWindowHeight()));
            }

            window.minSize = new Vector2(20, 20);
            window.maxSize = window.position.size;
        }
    }
}
#endif