#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertySearchFilter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class PropertySearchFilter
    {
        protected static readonly Func<string, string, bool> FuzzyStringMatcher = FuzzySearch.Contains;
        protected static readonly Func<string, string, bool> ExactStringMatcher = (search, str) => str.Contains(search);

        public bool Recursive = true;
        public bool UseFuzzySearch = true;
        public SearchFilterOptions FilterOptions = SearchFilterOptions.All;
        public Func<InspectorProperty, string, bool> MatchFunctionOverride;

        public InspectorProperty Property;
        public string SearchTerm;
        public List<SearchResult> SearchResults;


        protected string searchFieldControlName = "PropertySearchFilter_" + Guid.NewGuid().ToString();

        public bool HasSearchResults { get { return this.SearchResults != null; } }

        public PropertySearchFilter()
        {
        }

        public PropertySearchFilter(InspectorProperty property)
        {
            this.Property = property;
        }

        public PropertySearchFilter(InspectorProperty property, SearchableAttribute config)
        {
            this.Property = property;

            this.UseFuzzySearch = config.FuzzySearch;
            this.Recursive = config.Recursive;
            this.FilterOptions = config.FilterOptions;
        }

        public virtual void UpdateSearch(string searchFilter)
        {
            this.SearchTerm = searchFilter;
            this.UpdateSearch();
        }

        public virtual void UpdateSearch()
        {
            this.SearchResults = this.Search(this.SearchTerm);
        }

        public virtual List<SearchResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(this.SearchTerm))
            {
                return null;
            }

            List<SearchResult> results = new List<SearchResult>();

            if (this.Recursive)
            {
                foreach (var child in this.Property.Children)
                {
                    results.AddRange(this.Search(searchTerm, child));
                }
            }
            else
            {
                foreach (var child in this.Property.Children)
                {
                    if (this.IsMatch(child, searchTerm))
                    {
                        results.Add(new SearchResult()
                        {
                            MatchedProperty = child
                        });
                    }
                }
            }

            return results;
        }

        protected virtual IEnumerable<SearchResult> Search(string searchTerm, InspectorProperty property)
        {
            if (this.IsMatch(property, searchTerm))
            {
                var result = new SearchResult();

                result.MatchedProperty = property;

                foreach (var child in property.Children)
                {
                    result.ChildResults.AddRange(this.Search(searchTerm, child));
                }

                yield return result;
            }
            else
            {
                foreach (var child in property.Children)
                {
                    foreach (var result in this.Search(searchTerm, child))
                    {
                        yield return result;
                    }
                }
            }
        }

        public virtual bool IsMatch(InspectorProperty property, string searchTerm)
        {
            if (this.MatchFunctionOverride != null)
            {
                return this.MatchFunctionOverride(property, searchTerm);
            }

            if (property.Name == "InternalOnInspectorGUI") return false;
            if (property.Info.PropertyType == PropertyType.Group && (property.Name == "#_DefaultTabGroup" || property.Name == "#_DefaultBoxGroup")) return false;

            Func<string, string, bool> stringMatcher = this.UseFuzzySearch ? FuzzyStringMatcher : ExactStringMatcher;

            if (this.HasSearchFlag(SearchFilterOptions.PropertyName) && stringMatcher(searchTerm, property.Name))
                return true;

            if (this.HasSearchFlag(SearchFilterOptions.PropertyNiceName) && stringMatcher(searchTerm, property.NiceName))
                return true;

            if (property.ValueEntry != null)
            {
                if (this.HasSearchFlag(SearchFilterOptions.TypeOfValue) && stringMatcher(searchTerm, property.ValueEntry.TypeOfValue.GetNiceFullName())) return true;

                object value = property.ValueEntry.WeakSmartValue;

                if (this.HasSearchFlag(SearchFilterOptions.ISearchFilterableInterface) && value is ISearchFilterable)
                {
                    return (value as ISearchFilterable).IsMatch(searchTerm);
                }
                else if (this.HasSearchFlag(SearchFilterOptions.ValueToString))
                {
                    string valueString = value == null ? "null" : value.ToString();
                    if (stringMatcher(searchTerm, valueString)) return true;
                }
            }

            return false;
        }

        public virtual bool HasSearchFlag(SearchFilterOptions flag)
        {
            return (this.FilterOptions & flag) == flag;
        }

        public virtual void DrawSearchResults()
        {
            if (this.SearchResults == null) return;

            InspectorProperty lastParent = this.Property;

            for (int i = 0; i < this.SearchResults.Count; i++)
            {
                var result = this.SearchResults[i];

                result.MatchedProperty.Update();

                bool indented = false;

                if (result.MatchedProperty.Parent != null && result.MatchedProperty.DrawCount == 0)
                {
                    EditorGUI.indentLevel++;
                    indented = true;

                    if (result.MatchedProperty.Parent != lastParent)
                    {
                        var current = result.MatchedProperty.Parent;
                        string deltaPath = BuildNiceRelativePath("", current);

                        while (true)
                        {
                            current = current.Parent;
                            if (current == null || current == this.Property) break;
                            //deltaPath = "." + current.Name + deltaPath;
                            deltaPath = BuildNiceRelativePath(deltaPath, current);
                        }

                        Rect labelRect;

                        if (!string.IsNullOrEmpty(deltaPath))
                        {
                            labelRect = EditorGUILayout.GetControlRect();//.AddXMin(GUIHelper.CurrentIndentAmount);
                            GUI.Label(labelRect, GUIHelper.TempContent(deltaPath), EditorStyles.miniBoldLabel);
                        }
                        else
                        {
                            labelRect = EditorGUILayout.GetControlRect(true, 2f);//.AddXMin(GUIHelper.CurrentIndentAmount);
                        }

                        SirenixEditorGUI.DrawHorizontalLineSeperator(labelRect.xMin, labelRect.yMax - 0.5f, labelRect.width);
                        GUILayout.Space(3f);
                    }
                }

                this.DrawSearchResult(result);

                if (indented)
                {
                    EditorGUI.indentLevel--;
                }

                lastParent = result.MatchedProperty.Parent;
            }
        }

        private static string BuildNiceRelativePath(string path, InspectorProperty addProperty)
        {
            if (addProperty.IsTreeRoot) return path;

            if (addProperty.Info.PropertyType == PropertyType.Group && (addProperty.Name == "#_DefaultTabGroup" || addProperty.Name == "#_DefaultBoxGroup"))
            {
                return path;
            }

            string niceName = addProperty.NiceName;

            if (addProperty.Info.PropertyType == PropertyType.Group)
            {
                niceName = niceName.TrimStart('#');
            }

            if (string.IsNullOrEmpty(path)) return niceName;
            return niceName + " > " + path;
        }

        public virtual void DrawSearchResult(SearchResult result)
        {
            result.MatchedProperty.Update();

            if (result.MatchedProperty.DrawCount == 0)
            {
                result.MatchedProperty.Draw();
            }

            InspectorProperty lastDeltaParent = null;

            for (int i = 0; i < result.ChildResults.Count; i++)
            {
                var childResult = result.ChildResults[i];

                EditorGUI.indentLevel++;

                if (childResult.MatchedProperty.Parent != result.MatchedProperty && childResult.MatchedProperty.Parent != lastDeltaParent)
                {
                    var current = childResult.MatchedProperty.Parent;
                    string deltaPath = BuildNiceRelativePath("", current);

                    while (true)
                    {
                        current = current.Parent;
                        if (current == null || current == this.Property) break;
                        //deltaPath = "." + current.Name + deltaPath;
                        deltaPath = BuildNiceRelativePath(deltaPath, current);
                    }

                    var labelRect = EditorGUILayout.GetControlRect().AddXMin(GUIHelper.CurrentIndentAmount);
                    GUI.Label(labelRect, GUIHelper.TempContent(deltaPath), SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                    SirenixEditorGUI.DrawHorizontalLineSeperator(labelRect.xMin, labelRect.yMax - 0.5f, labelRect.width);
                    lastDeltaParent = childResult.MatchedProperty.Parent;
                }

                this.DrawSearchResult(childResult);

                EditorGUI.indentLevel--;
            }
        }

        public virtual void DrawDefaultSearchFieldLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null);

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            else
            {
                rect = rect.AddXMin(GUIHelper.CurrentIndentAmount);
            }

            var newTerm = SirenixEditorGUI.SearchField(rect, this.SearchTerm, false, this.searchFieldControlName);

            if (newTerm != this.SearchTerm)
            {
                this.SearchTerm = newTerm;
                this.Property.Tree.DelayActionUntilRepaint(() =>
                {
                    this.UpdateSearch();
                    GUIHelper.RequestRepaint();
                });
            }

            var separatorRect = EditorGUILayout.GetControlRect(true, 3f);
            SirenixEditorGUI.DrawThickHorizontalSeperator(separatorRect.AddXMin(GUIHelper.CurrentIndentAmount));
            GUILayout.Space(2f);
        }
    }
}
#endif