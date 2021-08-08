#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StaticInspectorWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Access the StaticInspectorWindow from Tools > Odin Inspector > Static Inspector.
    /// </summary>
    public class StaticInspectorWindow : OdinEditorWindow
    {
        /// <summary>
        /// Member filter for access modifiers.
        /// </summary>
        [Flags]
        public enum AccessModifierFlags
        {
            /// <summary>
            /// include public members.
            /// </summary>
            Public = 1 << 1,
            /// <summary>
            /// Include Non-public members.
            /// </summary>
            Private = 1 << 2,
            /// <summary>
            /// Include both public and non-public members.
            /// </summary>
            All = Public | Private
        }

        /// <summary>
        /// Member filter for member types.
        /// </summary>
        [Flags]
        public enum MemberTypeFlags
        {
            /// <summary>
            /// No members included.
            /// </summary>
            None = 0,
            /// <summary>
            /// Include field members.
            /// </summary>
            Fields = 1 << 0,
            /// <summary>
            /// Include property members.
            /// </summary>
            Properties = 1 << 1,
            /// <summary>
            /// Include method members.
            /// </summary>
            Methods = 1 << 2,
            /// <summary>
            /// Include group members.
            /// </summary>
            Groups = 1 << 3,
            /// <summary>
            /// Include members from the base types.
            /// </summary>
            BaseTypeMembers = 1 << 4,
            /// <summary>
            /// Include members marked with the Obsolete attribute.
            /// </summary>
            Obsolete = 1 << 5,
            /// <summary>
            /// Include all members except members marked with the Obsolete attribute.
            /// </summary>
            AllButObsolete = Fields | Properties | Methods | Groups | BaseTypeMembers,
        }

        private static GUIStyle btnStyle;

        private const string TargetTypeFlagsPrefKey = "OdinStaticInspectorWindow.TargetTypeFlags";
        private const string MemberTypeFlagsPrefKey = "OdinStaticInspectorWindow.MemberTypeFlags";
        private const string AccessModifierFlagsPrefKey = "OdinStaticInspectorWindow.AccessModifierFlags";

        [SerializeField, HideInInspector] private Type targetType;
        [SerializeField, HideInInspector] private AssemblyTypeFlags targetTypeFlags;
        [SerializeField, HideInInspector] private MemberTypeFlags memberTypes;
        [SerializeField, HideInInspector] private AccessModifierFlags accessModifiers;
        [SerializeField, HideInInspector] private string showMemberNameFilter;
        [SerializeField, HideInInspector] private string searchFilter;

        [NonSerialized] private PropertyTree tree;
        [NonSerialized] private AccessModifierFlags currAccessModifiers;
        [NonSerialized] private MemberTypeFlags currMemberTypes;
        [NonSerialized] private int focusSearch;

        /// <summary>
        /// Shows the window.
        /// </summary>
        public static void ShowWindow()
        {
            InspectType(null);
        }

        /// <summary>
        /// Opens a new static inspector window for the given type.
        /// </summary>
        public static StaticInspectorWindow InspectType(Type type, AccessModifierFlags? accessModifies = null, MemberTypeFlags? memberTypeFlags = null)
        {
            StaticInspectorWindow window = CreateInstance<StaticInspectorWindow>();

            window.titleContent = new GUIContent("Static Inspector", EditorIcons.MagnifyingGlass.Highlighted);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700f, 400f);

            window.targetTypeFlags = (AssemblyTypeFlags)EditorPrefs.GetInt(TargetTypeFlagsPrefKey, (int)(AssemblyTypeFlags.UserTypes | AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes | AssemblyTypeFlags.PluginTypes | AssemblyTypeFlags.PluginEditorTypes));

            if (accessModifies.HasValue) window.accessModifiers = accessModifies.Value;
            else window.accessModifiers = (AccessModifierFlags)EditorPrefs.GetInt(AccessModifierFlagsPrefKey, (int)(AccessModifierFlags.All));

            if (memberTypeFlags.HasValue) window.memberTypes = memberTypeFlags.Value;
            else window.memberTypes = (MemberTypeFlags)EditorPrefs.GetInt(MemberTypeFlagsPrefKey, (int)(MemberTypeFlags.AllButObsolete & ~MemberTypeFlags.BaseTypeMembers));

            window.currMemberTypes = window.memberTypes;
            window.currAccessModifiers = window.accessModifiers;
            window.focusSearch = 0;
            window.targetType = type;
            window.Show();

            if (type != null)
            {
                window.titleContent = new GUIContent(type.GetNiceName());
            }

            window.Repaint();
            return window;
        }

        private OdinSelector<Type> SelectType(Rect arg)
        {
            var p = new TypeSelector(this.targetTypeFlags, false);
            p.SelectionChanged += (types) =>
            {
                this.focusSearch = 0;
                var t = types.FirstOrDefault();
                if (t != null)
                {
                    this.targetType = t;
                    this.titleContent = new GUIContent(this.targetType.GetNiceName());
                }
            };

            p.SetSelection(this.targetType);
            p.ShowInPopup(new Rect(-300, 0, 300, 0));
            return p;
        }

        /// <summary>
        /// Draws the Odin Editor Window.
        /// </summary>
        protected override void OnGUI()
        {
            btnStyle = btnStyle ?? new GUIStyle(EditorStyles.toolbarDropDown);
            btnStyle.fixedHeight = 21;
            btnStyle.stretchHeight = false;

            this.DrawFirstToolbar();

            if (this.targetType != null)
            {
                this.DrawSecondToolbar();
            }

            base.OnGUI();
        }

        private void DrawFirstToolbar()
        {
            GUILayout.Space(1);

            var typeName = "       " + (this.targetType == null ? "Select Type" : this.targetType.GetNiceFullName()) + "   ";
            var rect = GUILayoutUtility.GetRect(0, 21, SirenixGUIStyles.ToolbarBackground);
            var rect2 = rect.AlignRight(80);
            var rect1 = rect.SetXMax(rect2.xMin);

            TypeSelector.DrawSelectorDropdown(rect1, new GUIContent(typeName), SelectType, btnStyle);

            EditorGUI.BeginChangeCheck();
            this.targetTypeFlags = EnumSelector<AssemblyTypeFlags>.DrawEnumField(rect2, null, new GUIContent("Type Filter"), this.targetTypeFlags, btnStyle);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(TargetTypeFlagsPrefKey, (int)this.targetTypeFlags);
            }

            if (Event.current.type == EventType.Repaint)
            {
                var icon = GUIHelper.GetAssetThumbnail(null, this.targetType ?? typeof(int), false);
                if (icon != null)
                {
                    rect1.x += 8;
                    GUI.DrawTexture(rect1.AlignLeft(16).AlignMiddle(16), icon, ScaleMode.ScaleToFit);
                }
            }
        }

        private void DrawSecondToolbar()
        {
            var rect = GUILayoutUtility.GetRect(0, 21);

            if (Event.current.type == EventType.Repaint)
            {
                SirenixGUIStyles.ToolbarBackground.Draw(rect, GUIContent.none, 0);
                SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1);
            }

            var accessRect = rect.AlignRight(80);
            var memberRect = accessRect.SubX(100).SetWidth(100);
            var searchRect = rect.SetXMax(memberRect.xMin);

            EditorGUI.BeginChangeCheck();

            this.memberTypes = EnumSelector<MemberTypeFlags>.DrawEnumField(memberRect, null, this.memberTypes, btnStyle);
            this.accessModifiers = EnumSelector<AccessModifierFlags>.DrawEnumField(accessRect, null, this.accessModifiers, btnStyle);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(AccessModifierFlagsPrefKey, (int)this.accessModifiers);
                EditorPrefs.SetInt(MemberTypeFlagsPrefKey, (int)this.memberTypes);
            }

            this.DrawSearchField(searchRect);
        }

        private void DrawSearchField(Rect rect)
        {
            rect = rect.HorizontalPadding(5).AlignMiddle(16);
            rect.xMin += 3;
            //rect.y += 1;
            this.searchFilter = SirenixEditorGUI.SearchField(rect, searchFilter, this.focusSearch++ < 4, "SirenixSearchField" + this.GetInstanceID());
        }

        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected override void DrawEditor(int index)
        {
#if !ODIN_LIMITED_VERSION
            this.DrawGettingStartedHelp();
            this.DrawTree();
#else
            SirenixEditorGUI.InfoMessageBox("The Static Inspector is only available in Odin Pro.");
#endif
            GUILayout.FlexibleSpace();
        }

#if !ODIN_LIMITED_VERSION
        private void DrawGettingStartedHelp()
        {
            if (this.targetType == null)
            {
                SirenixEditorGUI.InfoMessageBox("Select a type here to begin static inspection.");
            }
        }

        private void DrawTree()
        {
            if (this.targetType == null)
            {
                this.tree = null;
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                this.currMemberTypes = this.memberTypes;
                this.currAccessModifiers = this.accessModifiers;
            }

            if (this.tree == null || this.tree.TargetType != this.targetType)
            {
                if (this.targetType.IsGenericType && !this.targetType.IsFullyConstructedGenericType())
                {
                    SirenixEditorGUI.ErrorMessageBox("Cannot statically inspect generic type definitions");
                    return;
                }

                this.tree = PropertyTree.CreateStatic(this.targetType);
            }

            var allowObsoleteMembers = (this.currMemberTypes & MemberTypeFlags.Obsolete) == MemberTypeFlags.Obsolete;
            var allowObsoleteMembersContext = this.tree.RootProperty.Context.GetGlobal("ALLOW_OBSOLETE_STATIC_MEMBERS", false);

            if (allowObsoleteMembersContext.Value != allowObsoleteMembers)
            {
                allowObsoleteMembersContext.Value = allowObsoleteMembers;
                this.tree.RootProperty.RefreshSetup();
            }

            tree.BeginDraw(false);

            bool drawPropertiesNormally = true;

            if (tree.AllowSearchFiltering && tree.RootProperty.Attributes.HasAttribute<SearchableAttribute>())
            {
                var attr = tree.RootProperty.GetAttribute<SearchableAttribute>();
                if (attr.Recursive)
                {
                    SirenixEditorGUI.WarningMessageBox("This type has been marked as recursively searchable. Be *CAREFUL* with using this search - recursively searching a static inspector can be *very dangerous* and can lead to freezes, crashes or other nasty errors if the static inspector search ends up recursing deeply into, for example, the .NET runtime internals, which would result in recursively searching through hundreds of thousands to millions of internal properties.");
                }

                if (tree.DrawSearch())
                {
                    drawPropertiesNormally = false;
                }
            }

            if (drawPropertiesNormally)
            {
                foreach (var prop in tree.EnumerateTree(false))
                {
                    if (this.DrawProperty(prop))
                    {
                        if (prop.Info.PropertyType != PropertyType.Group && prop.Info.GetMemberInfo() != null && prop.Info.GetMemberInfo().DeclaringType != this.targetType)
                        {
                            prop.Draw(new GUIContent(prop.Info.GetMemberInfo().DeclaringType.GetNiceName() + " -> " + prop.NiceName));
                        }
                        else
                        {
                            prop.Draw();
                        }
                    }
                    else
                    {
                        prop.Update();
                    }
                }
            }

            tree.EndDraw();
        }

        private bool DrawProperty(InspectorProperty property)
        {
            if (!string.IsNullOrEmpty(this.searchFilter) && !property.NiceName.Replace(" ", "").Contains(this.searchFilter.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (property.Info.PropertyType == PropertyType.Group)
            {
                return (this.currMemberTypes & MemberTypeFlags.Groups) == MemberTypeFlags.Groups;
            }

            var member = property.Info.GetMemberInfo();

            if (member != null)
            {
                bool includeBaseTypeMembers = (this.currMemberTypes & MemberTypeFlags.BaseTypeMembers) == MemberTypeFlags.BaseTypeMembers;
                if (!includeBaseTypeMembers && member.DeclaringType != null && member.DeclaringType != this.targetType)
                {
                    return false;
                }

                bool showPublic = (this.currAccessModifiers & AccessModifierFlags.Public) == AccessModifierFlags.Public;
                bool showPrivate = (this.currAccessModifiers & AccessModifierFlags.Private) == AccessModifierFlags.Private;
                bool showFields = (this.currMemberTypes & MemberTypeFlags.Fields) == MemberTypeFlags.Fields;
                bool showProperties = (this.currMemberTypes & MemberTypeFlags.Properties) == MemberTypeFlags.Properties;

                if (!showPublic || !showPrivate)
                {
                    bool isPublic = true;
                    var fieldInfo = member as FieldInfo;
                    var propertyInfo = member as PropertyInfo;
                    var methodInfo = member as MethodInfo;

                    if (fieldInfo != null)
                    {
                        isPublic = fieldInfo.IsPublic;
                    }
                    else if (propertyInfo != null)
                    {
                        var getMethod = propertyInfo.GetGetMethod();
                        isPublic = getMethod != null && getMethod.IsPublic;
                    }
                    else if (methodInfo != null)
                    {
                        isPublic = methodInfo.IsPublic;
                    }

                    if (isPublic && !showPublic) return false;
                    if (!isPublic && !showPrivate) return false;
                }

                if (member is FieldInfo && !showFields) return false;
                if (member is PropertyInfo && !showProperties) return false;
            }

            if (property.Info.PropertyType == PropertyType.Method)
            {
                bool showMethods = (this.currMemberTypes & MemberTypeFlags.Methods) == MemberTypeFlags.Methods;
                if (!showMethods) return false;
            }

            return true;
        }
#endif
    }
}
#endif