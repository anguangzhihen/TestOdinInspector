#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnumSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// A feature-rich enum selector with support for flag enums.
    /// </summary>
    /// <example>
    /// <code>
    /// KeyCode someEnumValue;
    ///
    /// [OnInspectorGUI]
    /// void OnInspectorGUI()
    /// {
    ///     // Use the selector manually. See the documentation for OdinSelector for more information.
    ///     if (GUILayout.Button("Open Enum Selector"))
    ///     {
    ///         EnumSelector&lt;KeyCode&gt; selector = new EnumSelector&lt;KeyCode&gt;();
    ///         selector.SetSelection(this.someEnumValue);
    ///         selector.SelectionConfirmed += selection =&gt; this.someEnumValue = selection.FirstOrDefault();
    ///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
    ///     }
    ///
    ///     // Draw an enum dropdown field which uses the EnumSelector popup:
    ///     this.someEnumValue = EnumSelector&lt;KeyCode&gt;.DrawEnumField(new GUIContent("My Label"), this.someEnumValue);
    /// }
    ///
    /// // All Odin Selectors can be rendered anywhere with Odin. This includes the EnumSelector.
    /// EnumSelector&lt;KeyCode&gt; inlineSelector;
    ///
    /// [ShowInInspector]
    /// EnumSelector&lt;KeyCode&gt; InlineSelector
    /// {
    ///     get { return this.inlineSelector ?? (this.inlineSelector = new EnumSelector&lt;KeyCode&gt;()); }
    ///     set { }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinSelector{T}"/>
    /// <seealso cref="TypeSelector"/>
    /// <seealso cref="GenericSelector{T}"/>
    /// <seealso cref="OdinMenuTree"/>
    /// <seealso cref="OdinEditorWindow"/>
    public class EnumSelector<T> : OdinSelector<T>
    {
        private class EnumMember
        {
            public T value;
            public string name;
            public string niceName;
            public bool isObsolete;
            public string message;
            public bool hide;
        }

        private static readonly Type InspectorNameAttribute_Type = typeof(UnityEngine.Object).Assembly.GetType("UnityEngine.InspectorNameAttribute");
        private static readonly FieldInfo InspectorNameAttribute_displayName;

        private static readonly StringBuilder SB = new StringBuilder();
        private static readonly Func<T, T, bool> EqualityComparer = PropertyValueEntry<T>.EqualityComparer;

        private static Color highlightLineColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0, 1f) : new Color(0.015f, 0.68f, 0.015f, 1f);
        private static Color selectedMaskBgColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0, 0.1f) : new Color(0.02f, 0.537f, 0, 0.31f);
        private static readonly List<EnumMember> enumVals = new List<EnumMember>();
        private static readonly bool isFlagEnum = typeof(T).IsDefined<FlagsAttribute>();
        private static readonly string title = typeof(T).Name.SplitPascalCase();
        private float maxEnumLabelWidth = 0;

        public static bool DrawSearchToolbar = true;

        static EnumSelector()
        {
            if (InspectorNameAttribute_Type != null)
            {
                InspectorNameAttribute_displayName = InspectorNameAttribute_Type.GetField("displayName", Flags.InstanceAnyVisibility);
            }

            enumVals = new List<EnumMember>();

            if (typeof(T).IsEnum)
            {
                var fields = typeof(T).GetFields(Flags.StaticPublicDeclaredOnly);

                foreach (var field in fields)
                {
                    try
                    {
                        var obs = field.GetAttribute<ObsoleteAttribute>(true);
                        var lblText = field.GetAttribute<LabelTextAttribute>(true);
                        var msg = field.GetAttribute<InfoBoxAttribute>(true);
                        var hide = field.GetAttribute<HideInInspector>();

                        EnumMember val = new EnumMember();
                        val.value = (T)Enum.Parse(typeof(T), field.Name);
                        val.name = field.Name;
                        val.niceName = val.name.SplitPascalCase();
                        val.isObsolete = obs != null;
                        val.message = obs == null ? "" : obs.Message;
                        val.hide = hide != null;

                        if (lblText != null)
                        {
                            val.niceName = lblText.Text ?? "";

                            if (lblText.NicifyText)
                            {
                                val.niceName = ObjectNames.NicifyVariableName(val.niceName);
                            }
                        }

                        if (InspectorNameAttribute_displayName != null)
                        {
                            object[] inspectorNames = field.GetCustomAttributes(InspectorNameAttribute_Type, false);

                            if (inspectorNames.Length > 0)
                            {
                                val.niceName = ((string)InspectorNameAttribute_displayName.GetValue(inspectorNames[0])) ?? "";
                            }
                        }

                        if (msg != null)
                        {
                            val.message = msg.Message ?? "";
                        }

                        enumVals.Add(val);
                    }
                    catch { continue; }
                }
            }
        }

        private ulong curentValue;
        private ulong curentMouseOverValue;

        /// <summary>
        /// By default, the enum type will be drawn as the title for the selector. No title will be drawn if the string is null or empty.
        /// </summary>
        public override string Title
        {
            get
            {
                if (GeneralDrawerConfig.Instance.DrawEnumTypeTitle)
                {
                    return title;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is flag enum.
        /// </summary>
        public bool IsFlagEnum { get { return isFlagEnum; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumSelector{T}"/> class.
        /// </summary>
        public EnumSelector()
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException(typeof(T).GetNiceFullName() + " is not an enum type.");
            }

            if (Event.current != null)
            {
                foreach (var item in Enum.GetNames(typeof(T)))
                {
                    maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(item)).x);
                }

                if (this.Title != null)
                {
                    var titleAndSearch = Title + "                      ";
                    maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(titleAndSearch)).x);
                }
            }
        }

        /// <summary>
        /// Populates the tree with all enum values.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Selection.SupportsMultiSelect = isFlagEnum;
            tree.Config.DrawSearchToolbar = DrawSearchToolbar;
            foreach (var item in enumVals)
            {
                if (item.hide) continue;
                tree.Add(item.niceName, item);
            }

            //tree.AddRange(enumValues, x => Enum.GetName(typeof(T), x).SplitPascalCase());

            if (isFlagEnum)
            {
                tree.DefaultMenuStyle.Offset += 15;
                if (!enumVals.Where(x => x.value != null).Select(x => Convert.ToInt64(x.value)).Contains(0))
                {
                    tree.MenuItems.Insert(0, new OdinMenuItem(tree, GetNoneValueString(), new EnumMember() { value = (T)(object)0, name = "None", niceName = "None", isObsolete = false, message = "" }));
                }
                tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumFlagItem);
                this.DrawConfirmSelectionButton = false;
            }
            else
            {
                tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumItem);
            }

            tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumInfo);
        }

        private void DrawEnumInfo(OdinMenuItem obj)
        {
            var member = obj.Value as EnumMember;
            if (member == null) return;


            var hasMessage = !string.IsNullOrEmpty(member.message);

            if (member.isObsolete)
            {
                var rect = obj.Rect.Padding(5, 3).AlignRight(16).AlignCenterY(16);
                GUI.DrawTexture(rect, EditorIcons.TestInconclusive);
            }
            else if (hasMessage)
            {
                var rect = obj.Rect.Padding(5, 3).AlignRight(16).AlignCenterY(16);
                GUI.DrawTexture(rect, EditorIcons.ConsoleInfoIcon);
            }

            if (hasMessage)
            {
                GUI.Label(obj.Rect, new GUIContent("", member.message));
            }
        }

        private bool wasMouseDown = false;

        private void DrawEnumItem(OdinMenuItem obj)
        {
            if (Event.current.type == EventType.MouseDown && obj.Rect.Contains(Event.current.mousePosition))
            {
                obj.Select();
                Event.current.Use();
                wasMouseDown = true;
            }

            if (wasMouseDown)
            {
                GUIHelper.RequestRepaint();
            }

            if (wasMouseDown == true && Event.current.type == EventType.MouseDrag && obj.Rect.Contains(Event.current.mousePosition))
            {
                obj.Select();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                wasMouseDown = false;
                if (obj.IsSelected && obj.Rect.Contains(Event.current.mousePosition))
                {
                    obj.MenuTree.Selection.ConfirmSelection();
                }
            }
        }

        [OnInspectorGUI, PropertyOrder(-1000)]
        private void SpaceToggleEnumFlag()
        {
            if (this.SelectionTree != OdinMenuTree.ActiveMenuTree)
            {
                return;
            }

            if (isFlagEnum && Event.current.keyCode == KeyCode.Space && Event.current.type == EventType.KeyDown && this.SelectionTree != null)
            {
                foreach (var item in this.SelectionTree.Selection)
                {
                    this.ToggleEnumFlag(item);
                }

                this.TriggerSelectionChanged();

                Event.current.Use();
            }
        }

        /// <summary>
        /// When ShowInPopup is called, without a specified window width, this method gets called.
        /// Here you can calculate and give a good default width for the popup.
        /// The default implementation returns 0, which will let the popup window determine the width itself. This is usually a fixed value.
        /// </summary>
        protected override float DefaultWindowWidth()
        {
            return Mathf.Clamp(maxEnumLabelWidth + 50, 160, 400);
        }

        private void DrawEnumFlagItem(OdinMenuItem obj)
        {
            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) && obj.Rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    ToggleEnumFlag(obj);

                    this.TriggerSelectionChanged();
                }
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint)
            {
                var val = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
                var isPowerOfTwo = (val & (val - 1)) == 0;

                if (val != 0 && !isPowerOfTwo)
                {
                    var isMouseOver = obj.Rect.Contains(Event.current.mousePosition);
                    if (isMouseOver)
                    {
                        curentMouseOverValue = val;
                    }
                    else if (val == curentMouseOverValue)
                    {
                        curentMouseOverValue = 0;
                    }
                }

                var chked = (val & this.curentValue) == val && !((val == 0 && this.curentValue != 0));
                var highlight = val != 0 && isPowerOfTwo && (val & this.curentMouseOverValue) == val && !((val == 0 && this.curentMouseOverValue != 0));

                if (highlight)
                {
                    EditorGUI.DrawRect(obj.Rect.AlignLeft(6).Padding(2), highlightLineColor);
                }

                if (chked || isPowerOfTwo)
                {
                    var rect = obj.Rect.AlignLeft(30).AlignCenter(EditorIcons.TestPassed.width, EditorIcons.TestPassed.height);
                    if (chked)
                    {
                        if (isPowerOfTwo)
                        {
                            if (!EditorGUIUtility.isProSkin)
                            {
                                var tmp = GUI.color;
                                GUI.color = new Color(1, 0.7f, 1, 1);
                                GUI.DrawTexture(rect, EditorIcons.TestPassed);
                                GUI.color = tmp;
                            }
                            else
                            {
                                GUI.DrawTexture(rect, EditorIcons.TestPassed);
                            }
                        }
                        else
                        {
                            EditorGUI.DrawRect(obj.Rect.AlignTop(obj.Rect.height - (EditorGUIUtility.isProSkin ? 1 : 0)), selectedMaskBgColor);
                        }
                    }
                    else
                    {
                        GUI.DrawTexture(rect, EditorIcons.TestNormal);
                    }
                }
            }
        }

        private void ToggleEnumFlag(OdinMenuItem obj)
        {
            var val = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
            if ((val & this.curentValue) == val)
            {
                this.curentValue = val == 0 ? 0 : (this.curentValue & ~val);
            }
            else
            {
                this.curentValue = this.curentValue | val;
            }

            if (Event.current.clickCount >= 2)
            {
                Event.current.Use();
            }
        }

        /// <summary>
        /// Gets the currently selected enum value.
        /// </summary>
        public override IEnumerable<T> GetCurrentSelection()
        {
            if (isFlagEnum)
            {
                yield return (T)Enum.ToObject(typeof(T), this.curentValue);
            }
            else
            {
                if (this.SelectionTree.Selection.Count > 0)
                {
                    yield return (T)Enum.ToObject(typeof(T), GetMenuItemEnumValue(this.SelectionTree.Selection.Last()));
                }
            }
        }

        /// <summary>
        /// Selects an enum.
        /// </summary>
        public override void SetSelection(T selected)
        {
            if (isFlagEnum)
            {
                this.curentValue = (ulong)Convert.ToInt64(selected);
            }
            else
            {
                var selection = this.SelectionTree.EnumerateTree().Where(x => Convert.ToInt64(GetMenuItemEnumValue(x)) == Convert.ToInt64(selected));
                this.SelectionTree.Selection.AddRange(selection);
            }
        }

        private static object GetMenuItemEnumValue(OdinMenuItem item)
        {
            var memmebr = item.Value as EnumMember;
            if (memmebr != null && memmebr.value != null)
            {
                return memmebr.value;
            }
            return 0;
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null)
        {
            int id;
            bool hasFocus;
            Rect rect;
            Action<EnumSelector<T>> bindSelector;
            Func<IEnumerable<T>> getResult;

            SirenixEditorGUI.GetFeatureRichControlRect(label, out id, out hasFocus, out rect);

            if (DrawSelectorButton(rect, contentLabel, style ?? EditorStyles.popup, id, out bindSelector, out getResult))
            {
                var selector = new EnumSelector<T>();

                if (!EditorGUI.showMixedValue)
                {
                    selector.SetSelection(value);
                }

                var window = selector.ShowInPopup(new Vector2(rect.xMin, rect.yMax));

                if (isFlagEnum)
                {
                    window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
                }

                bindSelector(selector);
            }

            if (getResult != null)
            {
                value = getResult().FirstOrDefault();
            }

            return value;
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(GUIContent label, T value, GUIStyle style = null)
        {
            string display;

            if (EditorGUI.showMixedValue)
            {
                display = SirenixEditorGUI.MixedValueDashChar;
            }
            else
            {
                display = GetValueString(value);
            }

            return DrawEnumField(label, new GUIContent(display), value, style);
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(Rect rect, GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null)
        {
            int id;
            bool hasFocus;
            Action<EnumSelector<T>> bindSelector;
            Func<IEnumerable<T>> getResult;

            SirenixEditorGUI.GetFeatureRichControl(rect, out id, out hasFocus);

            if (DrawSelectorButton(rect, contentLabel, style ?? EditorStyles.popup, id, out bindSelector, out getResult))
            {
                var selector = new EnumSelector<T>();

                if (!EditorGUI.showMixedValue)
                {
                    selector.SetSelection(value);
                }

                var window = selector.ShowInPopup(new Vector2(rect.xMin, rect.yMax));

                if (isFlagEnum)
                {
                    window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
                }

                bindSelector(selector);
            }

            if (getResult != null)
            {
                value = getResult().FirstOrDefault();
            }

            return value;
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(Rect rect, GUIContent label, T value, GUIStyle style = null)
        {
            var display = (isFlagEnum && Convert.ToInt64(value) == 0) ? GetNoneValueString() : (EditorGUI.showMixedValue ? SirenixEditorGUI.MixedValueDashChar : value.ToString().SplitPascalCase());
            return DrawEnumField(rect, label, new GUIContent(display), value, style);
        }

        private static string GetNoneValueString()
        {
            var name = Enum.GetName(typeof(T), 0);
            if (name != null) return name.SplitPascalCase();
            return "None";
        }

        private static string GetValueString(T value)
        {
            for (int i = 0; i < enumVals.Count; i++)
            {
                var val = enumVals[i];

                if (EqualityComparer(val.value, value))
                {
                    return val.niceName;
                }
            }

            if (isFlagEnum)
            {
                var val64 = Convert.ToInt64(value);

                if (val64 == 0)
                {
                    return GetNoneValueString();
                }

                SB.Length = 0;

                for (int i = 0; i < enumVals.Count; i++)
                {
                    var val = enumVals[i];
                    var flags = Convert.ToInt64(val.value);

                    if (flags == 0) continue;

                    if ((val64 & flags) == flags)
                    {
                        if (SB.Length > 0) SB.Append(", ");
                        SB.Append(val.niceName);
                    }
                }

                return SB.ToString();
            }

            //var display = (isFlagEnum && Convert.ToInt64(value) == 0) ? GetNoneValueString() : (EditorGUI.showMixedValue ? SirenixEditorGUI.MixedValueDashChar : GetValueString(value));

            return value.ToString().SplitPascalCase();
        }
    }
}
#endif