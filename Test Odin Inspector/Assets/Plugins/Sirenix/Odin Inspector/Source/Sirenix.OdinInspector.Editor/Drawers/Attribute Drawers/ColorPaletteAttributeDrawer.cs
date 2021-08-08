#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorPaletteAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Odin drawer for <see cref="ColorPaletteAttribute"/>.
    /// </summary>
    [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
    public sealed class ColorPaletteAttributeDrawer : OdinAttributeDrawer<ColorPaletteAttribute, Color>
    {
        private int paletteIndex;
        private string currentName;
        private LocalPersistentContext<string> persistentName;
        private bool showAlpha;
        private string[] names;
        private ValueResolver<string> nameGetter;

        /// <summary>
        /// Initializes the drawer.
        /// </summary>
        protected override void Initialize()
        {
            this.paletteIndex = 0;
            this.currentName = this.Attribute.PaletteName;
            this.showAlpha = this.Attribute.ShowAlpha;
            this.names = ColorPaletteManager.Instance.ColorPalettes.Select(x => x.Name).ToArray();

            if (this.Attribute.PaletteName == null)
            {
                this.persistentName = this.ValueEntry.Context.GetPersistent<string>(this, "ColorPaletteName", null);
                var list = this.names.ToList();
                this.currentName = this.persistentName.Value;

                if (this.currentName != null && list.Contains(this.currentName))
                {
                    this.paletteIndex = list.IndexOf(this.currentName);
                }
            }
            else
            {
                this.nameGetter = ValueResolver.GetForString(this.Property, this.Attribute.PaletteName);
            }

        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            SirenixEditorGUI.BeginIndentedHorizontal();
            {
                var hideLabel = label == null;
                if (hideLabel == false)
                {
                    GUILayout.Label(label, GUILayoutOptions.Width(GUIHelper.BetterLabelWidth - 4).ExpandWidth(false));
                }
                else
                {
                    GUILayout.Space(5);
                }

                //var colorPaletDropDown = entry.Context.Get(this, "colorPalette", 0);
                //var currentName = entry.Context.Get(this, "currentName", attribute.PaletteName);
                //var showAlpha = entry.Context.Get(this, "showAlpha", attribute.ShowAlpha);
                //var names = ColorPaletteManager.Instance.GetColorPaletteNames();

                ColorPalette colorPalette;
                var rect = EditorGUILayout.BeginHorizontal();
                {
                    rect.x -= 3;
                    rect.width = 25;

                    entry.SmartValue = SirenixEditorGUI.DrawColorField(rect, entry.SmartValue, false, this.showAlpha);
                    bool openInEditorShown = false;
                    GUILayout.Space(28);
                    SirenixEditorGUI.BeginInlineBox();
                    {
                        if (attribute.PaletteName == null || ColorPaletteManager.Instance.ShowPaletteName)
                        {
                            SirenixEditorGUI.BeginToolbarBoxHeader();
                            {
                                if (attribute.PaletteName == null)
                                {
                                    var newValue = EditorGUILayout.Popup(this.paletteIndex, this.names, GUILayoutOptions.ExpandWidth(true));
                                    if (this.paletteIndex != newValue)
                                    {
                                        this.paletteIndex = newValue;
                                        this.currentName = this.names[newValue];
                                        this.persistentName.Value = this.currentName;
                                        GUIHelper.RemoveFocusControl();
                                    }
                                }
                                else
                                {
                                    GUILayout.Label(this.currentName);
                                    GUILayout.FlexibleSpace();
                                }
                                openInEditorShown = true;
                                if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
                                {
                                    ColorPaletteManager.Instance.OpenInEditor();
                                }
                            }
                            SirenixEditorGUI.EndToolbarBoxHeader();
                        }

                        if (attribute.PaletteName == null)
                        {
                            colorPalette = ColorPaletteManager.Instance.ColorPalettes.FirstOrDefault(x => x.Name == this.names[this.paletteIndex]);
                        }
                        else
                        {
                            colorPalette = ColorPaletteManager.Instance.ColorPalettes.FirstOrDefault(x => x.Name == this.nameGetter.GetValue());
                        }

                        if (colorPalette == null)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                if (attribute.PaletteName != null)
                                {
                                    if (GUILayout.Button("Create color palette: " + this.nameGetter.GetValue()))
                                    {
                                        ColorPaletteManager.Instance.ColorPalettes.Add(new ColorPalette() { Name = this.nameGetter.GetValue() });
                                        ColorPaletteManager.Instance.OpenInEditor();
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            this.currentName = colorPalette.Name;
                            this.showAlpha = attribute.ShowAlpha && colorPalette.ShowAlpha;
                            if (openInEditorShown == false)
                            {
                                GUILayout.BeginHorizontal();
                            }
                            var color = entry.SmartValue;
                            var stretch = ColorPaletteManager.Instance.StretchPalette;
                            var size = ColorPaletteManager.Instance.SwatchSize;
                            var margin = ColorPaletteManager.Instance.SwatchSpacing;
                            if (DrawColorPaletteColorPicker(entry, colorPalette, ref color, colorPalette.ShowAlpha, stretch, size, 20, margin))
                            {
                                entry.SmartValue = color;
                                //entry.ApplyChanges();
                            }
                            if (openInEditorShown == false)
                            {
                                GUILayout.Space(4);
                                if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
                                {
                                    ColorPaletteManager.Instance.OpenInEditor();
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    SirenixEditorGUI.EndInlineBox();
                }
                EditorGUILayout.EndHorizontal();
            }

            SirenixEditorGUI.EndIndentedHorizontal();
        }

        internal static bool DrawColorPaletteColorPicker(object key, ColorPalette colorPalette, ref Color color, bool drawAlpha, bool stretchPalette, float width = 20, float height = 20, float margin = 0)
        {
            bool result = false;

            var rect = SirenixEditorGUI.BeginHorizontalAutoScrollBox(key, GUILayoutOptions.ExpandWidth(true).ExpandHeight(false));
            {
                if (stretchPalette)
                {
                    rect.width -= margin * colorPalette.Colors.Count - margin;
                    width = Mathf.Max(width, rect.width / colorPalette.Colors.Count);
                }

                bool isMouseDown = Event.current.type == EventType.MouseDown;
                var innerRect = GUILayoutUtility.GetRect((width + margin) * colorPalette.Colors.Count, height, GUIStyle.none);
                float spacing = width + margin;
                var cellRect = innerRect;
                cellRect.width = width;

                for (int i = 0; i < colorPalette.Colors.Count; i++)
                {
                    cellRect.x = spacing * i;

                    if (drawAlpha)
                    {
                        EditorGUIUtility.DrawColorSwatch(cellRect, colorPalette.Colors[i]);
                    }
                    else
                    {
                        var c = colorPalette.Colors[i];
                        c.a = 1;
                        SirenixEditorGUI.DrawSolidRect(cellRect, c);
                    }

                    if (isMouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        color = colorPalette.Colors[i];
                        result = true;
                        GUI.changed = true;
                        Event.current.Use();
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalAutoScrollBox();
            return result;
        }
    }
}
#endif