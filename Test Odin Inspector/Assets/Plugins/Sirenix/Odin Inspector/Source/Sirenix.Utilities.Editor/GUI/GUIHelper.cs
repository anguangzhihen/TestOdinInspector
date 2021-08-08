#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIHelper.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Utilities;
    using UnityEditor;
    using UnityEngine;
    using UnityEditorInternal;

    /// <summary>
    /// Various helper function for GUI.
    /// </summary>
    [InitializeOnLoad]
    public static class GUIHelper
    {
        [NonSerialized]
        private static object defaultConfigKey = new object();
        private static readonly GUIContent tmpContent = new GUIContent("");
        private static readonly GUIScopeStack<bool> GUIEnabledStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<bool> HierarchyModeStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<bool> IsBoldLabelStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<Matrix4x4> MatrixStack = new GUIScopeStack<Matrix4x4>();
        private static readonly GUIScopeStack<Color> ColorStack = new GUIScopeStack<Color>();
        private static readonly GUIScopeStack<EventType> EventTypeStack = new GUIScopeStack<EventType>();
        private static readonly GUIScopeStack<Color> ContentColorStack = new GUIScopeStack<Color>();
        private static readonly GUIScopeStack<Color> LabelColorStack = new GUIScopeStack<Color>();
        private static readonly GUIScopeStack<int> IndentLevelStack = new GUIScopeStack<int>();
        private static readonly GUIScopeStack<float> LabelWidthStack = new GUIScopeStack<float>();
        private static readonly GUIScopeStack<int> LayoutMeasureInfoStack = new GUIScopeStack<int>();
        private static readonly GUIScopeStack<bool> ResponsiveVectorComponentFieldsStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<float> FadeGroupDurationStack = new GUIScopeStack<float>();
        private static readonly GUIScopeStack<float> TabPageSlideAnimationDurationStack = new GUIScopeStack<float>();
        private static readonly GUIScopeStack<bool> IsDrawingDictionaryKeyStack = new GUIScopeStack<bool>();
        private static readonly WeakValueGetter<Rect> GetRectOnGUILayoutEntry;
        private static readonly Type HostViewType;
        private static readonly Func<object> GUIViewGetter;
        private static readonly Func<object, EditorWindow> ActualViewGetter;
        private static readonly Func<Vector2> EditorScreenPointOffsetGetter;
        private static readonly Func<RectOffset> CurrentWindowBorderSizeGetter;
        private static readonly Func<int> GUILayoutEntriesCursorIndexGetter;
        private static readonly Func<IList> GUILayoutEntriesGetter;
        private static readonly Func<int> CurrentWindowIDGetter;
        private static readonly Func<bool> CurrentWindowHasFocusGetter;
        private static readonly Action<bool> SetBoldDefaultFontSetter;
        private static readonly Func<bool> GetBoldDefaultFontGetter;
        private static readonly Func<Rect> GetTopLevelLayoutRectGetter;
        private static readonly Func<GUIStyle> GetTopLevelLayoutStyleGetter;
        private static readonly Func<EditorWindow, bool> GetIsDockedWindowGetter;
        private static readonly Func<Rect> GetEditorWindowRectGetter;
        private static readonly Type inspectorWindowType;

        private static readonly Func<float> ContextWidthGetter;
        private static readonly Action<float> ContextWidthSetter;
        private static readonly Func<Stack<float>> ContextWidthStackGetter;
        private static readonly GUIScopeStack<float> ContextWidthStackOdinVersion = new GUIScopeStack<float>();

        private static readonly Func<float> ActualLabelWidthGetter;
        private static int numberOfFramesToRepaint;
        private static float betterContextWidth;

        /// <summary>
        /// Gets the bold default font.
        /// </summary>
        public static bool GetBoldDefaultFont()
        {
            return GetBoldDefaultFontGetter();
        }

        static GUIHelper()
        {
            HostViewType = typeof(Editor).Assembly.GetType("UnityEditor.HostView");
            var guiLayoutEntryType = typeof(GUI).Assembly.GetType("UnityEngine.GUILayoutEntry");
            var guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
            inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

            GetRectOnGUILayoutEntry = EmitUtilities.CreateWeakInstanceFieldGetter<Rect>(guiLayoutEntryType, guiLayoutEntryType.GetField("rect", Flags.AllMembers));
            EditorScreenPointOffsetGetter = DeepReflection.CreateValueGetter<Vector2>(typeof(GUIUtility), "s_EditorScreenPointOffset");
            CurrentWindowIDGetter = DeepReflection.CreateValueGetter<int>(guiViewType, "current.GetInstanceID()");
            CurrentWindowHasFocusGetter = DeepReflection.CreateValueGetter<bool>(guiViewType, "current.hasFocus");
            GetIsDockedWindowGetter = DeepReflection.CreateValueGetter<EditorWindow, bool>("docked");
            ContextWidthGetter = DeepReflection.CreateValueGetter<float>(typeof(EditorGUIUtility), "contextWidth");

            var contextWidthField = typeof(EditorGUIUtility).GetField("s_ContextWidth", Flags.AllMembers);

            if (contextWidthField != null)
            {
                ContextWidthSetter = EmitUtilities.CreateStaticFieldSetter<float>(contextWidthField);
            }
            else
            {
                contextWidthField = typeof(EditorGUIUtility).GetField("s_ContextWidthStack", Flags.AllMembers);

                ContextWidthStackGetter = EmitUtilities.CreateStaticFieldGetter<Stack<float>>(contextWidthField);
            }

            ActualLabelWidthGetter = EmitUtilities.CreateStaticFieldGetter<float>(typeof(EditorGUIUtility).GetField("s_LabelWidth", Flags.AllMembers));

            GUIViewGetter = DeepReflection.CreateWeakStaticValueGetter(guiViewType, guiViewType, "current");
            ActualViewGetter = DeepReflection.CreateWeakInstanceValueGetter<EditorWindow>(HostViewType, "actualView");
            var borderSizeGetter = DeepReflection.CreateWeakInstanceValueGetter<RectOffset>(HostViewType, "borderSize");

            //CurrentWindowGetter = () => actualViewGetter(guiViewGetter());
            CurrentWindowBorderSizeGetter = () => borderSizeGetter(GUIViewGetter());

            SetBoldDefaultFontSetter = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>),
                typeof(EditorGUIUtility).GetMethod("SetBoldDefaultFont", Flags.StaticAnyVisibility, null, new Type[] { typeof(bool) }, null ));

            GetBoldDefaultFontGetter = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>),
                typeof(EditorGUIUtility).GetMethod("GetBoldDefaultFont", Flags.StaticAnyVisibility, null, Type.EmptyTypes, null));

            GUILayoutEntriesCursorIndexGetter = DeepReflection.CreateValueGetter<int>(typeof(GUILayoutUtility), "current.topLevel.m_Cursor");
            GUILayoutEntriesGetter = DeepReflection.CreateValueGetter<IList>(typeof(GUILayoutUtility), "current.topLevel.entries");
            GetTopLevelLayoutRectGetter = DeepReflection.CreateValueGetter<Rect>(typeof(GUILayoutUtility), "current.topLevel.rect");
            GetTopLevelLayoutStyleGetter = DeepReflection.CreateValueGetter<GUIStyle>(typeof(GUILayoutUtility), "current.topLevel.style");
            GetEditorWindowRectGetter = DeepReflection.CreateValueGetter<Rect>(typeof(Editor).Assembly.GetType("UnityEditor.Toolbar"), "get.parent.screenPosition");
        }

        private static System.Diagnostics.Stopwatch smartProgressBarWatch = System.Diagnostics.Stopwatch.StartNew();
        private static int smartProgressBarDisplaysSinceLastUpdate = 0;

        public static void DisplaySmartUpdatingProgressBar(string title, string details, float progress, int updateIntervalByMS = 200, int updateIntervalByCall = 50)
        {
            bool updateProgressBar =
                    smartProgressBarWatch.ElapsedMilliseconds >= updateIntervalByMS
                || ++smartProgressBarDisplaysSinceLastUpdate >= updateIntervalByCall;

            if (updateProgressBar)
            {
                smartProgressBarWatch.Stop();
                smartProgressBarWatch.Reset();
                smartProgressBarWatch.Start();

                smartProgressBarDisplaysSinceLastUpdate = 0;

                EditorUtility.DisplayProgressBar(title, details, progress);
            }
        }

        public static bool DisplaySmartUpdatingCancellableProgressBar(string title, string details, float progress, int updateIntervalByMS = 200, int updateIntervalByCall = 50)
        {
            bool updateProgressBar =
                    smartProgressBarWatch.ElapsedMilliseconds >= updateIntervalByMS
                || ++smartProgressBarDisplaysSinceLastUpdate >= updateIntervalByCall;

            if (updateProgressBar)
            {
                smartProgressBarWatch.Stop();
                smartProgressBarWatch.Reset();
                smartProgressBarWatch.Start();

                smartProgressBarDisplaysSinceLastUpdate = 0;

                if (EditorUtility.DisplayCancelableProgressBar(title, details, progress))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// An alternative to GUI.FocusControl(null), which does not take focus away from the current GUI.Window.
        /// </summary>
        public static void RemoveFocusControl()
        {
            GUIUtility.hotControl = 0;
            DragAndDrop.activeControlID = 0;
            GUIUtility.keyboardControl = 0;
        }

        /// <summary>
        /// Whether the inspector is currently in the progress of drawing a dictionary key.
        /// </summary>
        public static bool IsDrawingDictionaryKey { get { return IsDrawingDictionaryKeyStack.Count == 0 ? false : IsDrawingDictionaryKeyStack.Peek(); } }

        /// <summary>
        /// Hides the following draw calls. Remember to call <see cref="EndDrawToNothing"/> when done.
        /// </summary>
        public static void BeginDrawToNothing()
        {
            GUILayout.BeginArea(new Rect(-9999, -9999, 1, 1), SirenixGUIStyles.None);
            GUILayout.BeginVertical();
        }

        /// <summary>
        /// Unhides the following draw calls after having called <see cref="BeginDrawToNothing"/>.
        /// </summary>
        public static void EndDrawToNothing()
        {
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Determines whether the specified EditorWindow is docked.
        /// </summary>
        /// <param name="window">The editor window.</param>
        /// <returns><c>true</c> if the editor window is docked. Otherwise <c>false</c>.</returns>
        public static bool IsDockedWindow(EditorWindow window)
        {
            return GetIsDockedWindowGetter(window);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Rect GetEditorWindowRect()
        {
            return GetEditorWindowRectGetter();
        }

        /// <summary>
        /// Opens a new inspector window for the specified object.
        /// </summary>
        /// <param name="unityObj">The unity object.</param>
        /// <exception cref="System.ArgumentNullException">unityObj</exception>
        public static void OpenInspectorWindow(UnityEngine.Object unityObj)
        {
            if (unityObj == null) throw new ArgumentNullException("unityObj");

            var windowRect = GUIHelper.GetEditorWindowRect();
            var windowSize = new Vector2(450, Mathf.Min(windowRect.height * 0.7f, 500));
            var rect = new Rect(windowRect.center - windowSize * 0.5f, windowSize);

            var inspectorInstance = ScriptableObject.CreateInstance(inspectorWindowType) as EditorWindow;
            inspectorInstance.Show();

            var prevSelection = Selection.objects;
            Selection.activeObject = unityObj;
            inspectorWindowType
                .GetProperty("isLocked", Flags.AllMembers)
                .GetSetMethod()
                .Invoke(inspectorInstance, new object[] { true });
            Selection.objects = prevSelection;
            inspectorInstance.position = rect;

            if (unityObj.GetType().InheritsFrom(typeof(Texture2D)))
            {
                OdinInspector.Editor.UnityEditorEventUtility.EditorApplication_delayCall += () =>
                {
                    // Unity's Texture drawer changes Selection.object
                    OdinInspector.Editor.UnityEditorEventUtility.EditorApplication_delayCall += () =>
                    {
                        // Lets change that back shall we?
                        Selection.objects = prevSelection;
                    };
                };
            }
        }

        internal static void OpenEditorInOdinDropDown(UnityEngine.Object obj, Rect btnRect)
        {
            var odinEditorWindow = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Editor.OdinEditorWindow");
            odinEditorWindow.GetMethods(Flags.StaticPublic)
                .First(x => x.Name == "InspectObjectInDropDown" && x.GetParameters().Last().ParameterType == typeof(float))
                .Invoke(null, new object[] { obj, btnRect, 400 });
        }

        /// <summary>
        /// Gets or sets a value indicating whether labels are currently bold.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is bold label; otherwise, <c>false</c>.
        /// </value>
        public static bool IsBoldLabel
        {
            get { return GetBoldDefaultFontGetter(); }
            set { SetBoldDefaultFontSetter(value); }
        }

        /// <summary>
        /// Gets the size of the current window border.
        /// </summary>
        /// <value>
        /// The size of the current window border.
        /// </value>
        public static RectOffset CurrentWindowBorderSize
        {
            get { return CurrentWindowBorderSizeGetter(); }
        }

        /// <summary>
        /// Gets the editor screen point offset.
        /// </summary>
        /// <value>
        /// The editor screen point offset.
        /// </value>
        public static Vector2 EditorScreenPointOffset
        {
            get { return EditorScreenPointOffsetGetter(); }
        }

        /// <summary>
        /// Gets the current editor gui context width. Only set these if you know what it does.
        /// Setting this has been removed. Use PushContextWidth and PopContextWidth instead.
        /// </summary>
        public static float ContextWidth
        {
            get { return ContextWidthGetter(); }

        }

        /// <summary>
        /// Unity EditorGUIUtility.labelWidth only works reliablly in Repaint events.
        /// BetterLabelWidth does a better job at giving you the correct LabelWidth in non-repaint events.
        /// </summary>
        public static float BetterLabelWidth
        {
            get
            {
                if (BetterContextWidth == 0)
                {
                    return EditorGUIUtility.labelWidth;
                }

                // Unity only ever knows the exact labelWidth in repaint events.
                // But you often need it in Layout events as well.
                // See BetterContextWidths to learn more. 

                // Unity uses the ContextWidth to calculate the labelWidth.
                PushContextWidth(BetterContextWidth);
                var val = EditorGUIUtility.labelWidth;
                PopContextWidth();
                return val;
            }
            set
            {
                EditorGUIUtility.labelWidth = value;
            }
        }

        /// <summary>
        /// Odin will set this for you whenever an Odin property tree is drawn.
        /// But if you're using BetterLabelWidth and BetterContextWidth without Odin, then 
        /// you need to set BetterContextWidth in the beginning of each GUIEvent.
        /// </summary>
        public static float BetterContextWidth
        {
            get
            {
                if (betterContextWidth == 0)
                {
                    return ContextWidth;
                }

                return betterContextWidth;
            }
            set
            {
                betterContextWidth = value;
            }
        }

        /// <summary>
        /// Gets the current indent amount.
        /// </summary>
        /// <value>
        /// The current indent amount.
        /// </value>
        public static float CurrentIndentAmount
        {
            get
            {
                return EditorGUI.indentLevel * 15;
            }
        }

        /// <summary>
        /// Gets the mouse screen position.
        /// </summary>
        /// <value>
        /// The mouse screen position.
        /// </value>
        public static Vector2 MouseScreenPosition
        {
            get
            {
                return Event.current.mousePosition + EditorScreenPointOffset;
            }
        }

        /// <summary>
        /// Gets the current editor window.
        /// </summary>
        /// <value>
        /// The current editor window.
        /// </value>
        public static EditorWindow CurrentWindow
        {
            get
            {
                var view = GUIViewGetter();

                if (view == null)
                {
                    return null;
                }

                // On Mac, In rare instances, such as when the user has clicked the eye dropper on a color field,
                // the current view will not be a type of HostView.
                // We can only get the current EditorWindow from a HostView, for now...
                if (!HostViewType.IsAssignableFrom(view.GetType()))
                {
                    return null;
                }

                return ActualViewGetter(view);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current editor window is focused.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current window has focus. Otherwise, <c>false</c>.
        /// </value>
        public static bool CurrentWindowHasFocus
        {
            get
            {
                return CurrentWindowHasFocusGetter();
            }
        }

        /// <summary>
        /// Gets the ID of the current editor window.
        /// </summary>
        /// <value>
        /// The ID of the current editor window.
        /// </value>
        public static int CurrentWindowInstanceID
        {
            get
            {
                try
                {
                    return CurrentWindowIDGetter();
                }
                catch
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a repaint has been requested.
        /// </summary>
        /// <value>
        ///   <c>true</c> if repaint has been requested. Otherwise <c>false</c>.
        /// </value>
        public static bool RepaintRequested
        {
            get { return numberOfFramesToRepaint > 0; }
        }

        /// <summary>
        /// Gets or sets the actual EditorGUIUtility.LabelWidth, regardless of the current hierarchy mode or context width.
        /// </summary>
        public static float ActualLabelWidth
        {
            get { return ActualLabelWidthGetter(); }
            set { GUIHelper.BetterLabelWidth = value; }
        }

        /// <summary>
        /// Requests a repaint.
        /// </summary>
        public static void RequestRepaint()
        {
            GUIHelper.numberOfFramesToRepaint = Math.Max(GUIHelper.numberOfFramesToRepaint, 2);
        }

        /// <summary>
        /// Requests a repaint.
        /// </summary>
        public static void RequestRepaint(int numberOfFramesToRepaint)
        {
            GUIHelper.numberOfFramesToRepaint = Math.Max(GUIHelper.numberOfFramesToRepaint, numberOfFramesToRepaint);
        }

        /// <summary>
        /// Begins the layout measuring. Remember to end with <see cref="EndLayoutMeasuring"/>.
        /// </summary>
        public static void BeginLayoutMeasuring()
        {
            if (Event.current.type != EventType.Layout)
            {
                LayoutMeasureInfoStack.Push(GUILayoutEntriesCursorIndexGetter());
            }
        }

        /// <summary>
        /// Ends the layout measuring started by <see cref="BeginLayoutMeasuring"/>
        /// </summary>
        /// <returns>The measured rect.</returns>
        public static Rect EndLayoutMeasuring()
        {
            if (Event.current.type != EventType.Layout)
            {
                var from = LayoutMeasureInfoStack.Pop();
                var to = GUILayoutEntriesCursorIndexGetter();
                IList entries = GUILayoutEntriesGetter();

                from = Mathf.Min(from, entries.Count - 1);
                to = Mathf.Min(to, entries.Count);
                if (from >= 0)
                {
                    var entry = entries[from];
                    var rect = GetRectOnGUILayoutEntry(ref entry);

                    if (from == to)
                    {
                        rect.width = 0;
                        rect.height = 0;
                    }
                    else
                    {
                        for (int i = from + 1; i < to; i++)
                        {
                            var entry1 = entries[i];
                            var tmpRect = GetRectOnGUILayoutEntry(ref entry1);

                            rect.xMin = Mathf.Min(rect.xMin, tmpRect.xMin);
                            rect.yMin = Mathf.Min(rect.yMin, tmpRect.yMin);
                            rect.xMax = Mathf.Max(rect.xMax, tmpRect.xMax);
                            rect.yMax = Mathf.Max(rect.yMax, tmpRect.yMax);
                        }
                    }

                    return rect;
                }
            }

            return new Rect(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the current layout rect.
        /// </summary>
        /// <returns>The current layout rect.</returns>
        public static Rect GetCurrentLayoutRect()
        {
            return GetTopLevelLayoutRectGetter();
        }

        /// <summary>
        /// Gets the current layout rect.
        /// </summary>
        /// <returns>The current layout rect.</returns>
        public static GUIStyle GetCurrentLayoutStyle()
        {
            try
            {
                return GetTopLevelLayoutStyleGetter() ?? GUIStyle.none;
            }
            catch
            {
                return GUIStyle.none;
            }
        }

        /// <summary>
        /// Gets the playmode color tint.
        /// </summary>
        /// <returns>The playmode color tint.</returns>
        public static Color GetPlaymodeTint()
        {
            Color tint = Color.white;

            // Preference string is formatted as "playmode tint;R;G;B;A"
            var a = EditorPrefs.GetString("Playmode tint", "playmode tint;1;1;1;1").Split(';');
            float.TryParse(a[1], out tint.r);
            float.TryParse(a[2], out tint.g);
            float.TryParse(a[3], out tint.b);
            float.TryParse(a[4], out tint.a);

            return tint;
        }

        /// <summary>
        /// Pushes a context width to the context width stack.
        /// Remember to pop the value again with <see cref="PopContextWidth"/>.
        /// </summary>
        /// <param name="width">The width to push.</param>
        public static void PushContextWidth(float width)
        {
            // Unity introduced a context width stack in 2019.3, and we're
            //  adopting that across all versions with a stack fallback.
            if (ContextWidthSetter != null)
            {
                ContextWidthStackOdinVersion.Push(width);
                ContextWidthSetter(width);
            }
            else
            {
                var unityStack = ContextWidthStackGetter();
                unityStack.Push(width);
            }
        }

        /// <summary>
        /// Pops a value pushed by <see cref="PushContextWidth(float)"/>.
        /// </summary>
        public static void PopContextWidth()
        {
            if (ContextWidthSetter != null)
            {
                var width = ContextWidthStackOdinVersion.Pop();
                ContextWidthSetter(width);
            }
            else
            {
                var unityStack = ContextWidthStackGetter();
                if (unityStack.Count > 0)
                {
                    unityStack.Pop();
                }
            }
        }

        /// <summary>
        /// Pushes a color to the GUI color stack. Remember to pop the color with <see cref="PopColor"/>.
        /// </summary>
        /// <param name="color">The color to push the GUI color..</param>
        /// <param name="blendAlpha">if set to <c>true</c> blend with alpha.</param>
        public static void PushColor(Color color, bool blendAlpha = false)
        {
            ColorStack.Push(GUI.color);

            if (blendAlpha)
            {
                color.a = color.a * GUI.color.a;
            }

            GUI.color = color;
        }

        /// <summary>
        /// Takes a screenshot of the GUI within the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns>The screenshot as a render texture.</returns>
        public static RenderTexture TakeGUIScreenshot(Rect rect)
        {
            RenderTexture rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height);
            Graphics.Blit(RenderTexture.active, rt);
            return rt;
        }

        /// <summary>
        /// Pops the GUI color pushed by <see cref="PushColor(Color, bool)"/>.
        /// </summary>
        public static void PopColor()
        {
            GUI.color = ColorStack.Pop();
        }

        /// <summary>
        /// Pushes a state to the GUI enabled stack. Remember to pop the state with <see cref="PopGUIEnabled"/>.
        /// </summary>
        /// <param name="enabled">If set to <c>true</c> GUI will be enabled. Otherwise GUI will be disabled.</param>
        public static void PushGUIEnabled(bool enabled)
        {
            GUIEnabledStack.Push(GUI.enabled);
            GUI.enabled = enabled;
        }

        /// <summary>
        /// Pops the GUI enabled pushed by <see cref="PushGUIEnabled(bool)"/>
        /// </summary>
        public static void PopGUIEnabled()
        {
            GUI.enabled = GUIEnabledStack.Pop();
        }

        /// <summary>
        /// Pushes a state to the IsDrawingDictionaryKey stack. Remember to pop the state with <see cref="PopIsDrawingDictionaryKey"/>.
        /// </summary>
        public static void PushIsDrawingDictionaryKey(bool enabled)
        {
            IsDrawingDictionaryKeyStack.Push(enabled);
        }

        /// <summary>
        /// Pops the state pushed by <see cref="PushIsDrawingDictionaryKey(bool)"/>
        /// </summary>
        public static void PopIsDrawingDictionaryKey()
        {
            IsDrawingDictionaryKeyStack.Pop();
        }

        /// <summary>
        /// Pushes the hierarchy mode to the stack. Remember to pop the state with <see cref="PopHierarchyMode"/>.
        /// </summary>
        /// <param name="hierarchyMode">The hierachy mode state to push.</param>
        /// <param name="preserveCurrentLabelWidth">Changing hierachy mode also changes how label-widths are calcualted. By default, we try to keep the current label width.</param>
        public static void PushHierarchyMode(bool hierarchyMode, bool preserveCurrentLabelWidth = true)
        {
            var actualLabelWidth = ActualLabelWidth;
            LabelWidthStack.Push(actualLabelWidth);
            var currentLabelWidth = preserveCurrentLabelWidth ? GUIHelper.BetterLabelWidth : actualLabelWidth;
            HierarchyModeStack.Push(EditorGUIUtility.hierarchyMode);
            EditorGUIUtility.hierarchyMode = hierarchyMode;
            GUIHelper.BetterLabelWidth = currentLabelWidth;
        }

        /// <summary>
        /// Pops the hierarchy mode pushed by <see cref="PushHierarchyMode(bool)"/>.
        /// </summary>
        public static void PopHierarchyMode()
        {
            EditorGUIUtility.hierarchyMode = HierarchyModeStack.Pop();
            ActualLabelWidth = LabelWidthStack.Pop();
        }

        /// <summary>
        /// Pushes bold label state to the stack. Remember to pop with <see cref="PopIsBoldLabel"/>.
        /// </summary>
        /// <param name="isBold">Value indicating if labels should be bold or not.</param>
        public static void PushIsBoldLabel(bool isBold)
        {
            IsBoldLabelStack.Push(IsBoldLabel);
            IsBoldLabel = isBold;
        }

        /// <summary>
        /// Pops the bold label state pushed by <see cref="PushIsBoldLabel(bool)"/>.
        /// </summary>
        public static void PopIsBoldLabel()
        {
            IsBoldLabel = IsBoldLabelStack.Pop();
        }

        /// <summary>
        /// Pushes the indent level to the stack. Remember to pop with <see cref="PopIndentLevel"/>.
        /// </summary>
        /// <param name="indentLevel">The indent level to push.</param>
        public static void PushIndentLevel(int indentLevel)
        {
            IndentLevelStack.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = indentLevel;
        }

        /// <summary>
        /// Pops the indent level pushed by <see cref="PushIndentLevel(int)"/>.
        /// </summary>
        public static void PopIndentLevel()
        {
            EditorGUI.indentLevel = IndentLevelStack.Pop();
        }

        /// <summary>
        /// Pushes the content color to the stack. Remember to pop with <see cref="PopContentColor"/>.
        /// </summary>
        /// <param name="color">The content color to push..</param>
        /// <param name="blendAlpha">If set to <c>true</c> blend with alpha.</param>
        public static void PushContentColor(Color color, bool blendAlpha = false)
        {
            ContentColorStack.Push(GUI.contentColor);

            if (blendAlpha)
            {
                color.a = color.a * GUI.contentColor.a;
            }

            GUI.contentColor = color;
        }

        /// <summary>
        /// Pops the content color pushed by <see cref="PushContentColor(Color, bool)"/>.
        /// </summary>
        public static void PopContentColor()
        {
            GUI.contentColor = ContentColorStack.Pop();
        }

        /// <summary>
        /// Pushes the label color to the stack. Remember to pop with <see cref="PopLabelColor"/>.
        /// </summary>
        /// <param name="color">The label color to push.</param>
        public static void PushLabelColor(Color color)
        {
            LabelColorStack.Push(EditorStyles.label.normal.textColor);
            EditorStyles.label.normal.textColor = color;
            SirenixGUIStyles.Foldout.normal.textColor = color;
            SirenixGUIStyles.Foldout.onNormal.textColor = color;
        }

        /// <summary>
        /// Pops the label color pushed by <see cref="PushLabelColor(Color)"/>.
        /// </summary>
        public static void PopLabelColor()
        {
            var color = LabelColorStack.Pop();
            EditorStyles.label.normal.textColor = color;
            SirenixGUIStyles.Foldout.normal.textColor = color;
            SirenixGUIStyles.Foldout.onNormal.textColor = color;
        }

        /// <summary>
        /// Pushes the GUI position offset to the stack. Remember to pop with <see cref="PopGUIPositionOffset"/>.
        /// </summary>
        /// <param name="offset">The GUI offset.</param>
        public static void PushGUIPositionOffset(Vector2 offset)
        {
            PushMatrix(GUI.matrix * Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one));
        }

        /// <summary>
        /// Pops the GUI position offset pushed by <see cref="PushGUIPositionOffset(Vector2)"/>.
        /// </summary>
        public static void PopGUIPositionOffset()
        {
            PopMatrix();
        }

        /// <summary>
        /// Pushes a GUI matrix to the stack. Remember to pop with <see cref="PopMatrix"/>.
        /// </summary>
        /// <param name="matrix">The GUI matrix to push.</param>
        public static void PushMatrix(Matrix4x4 matrix)
        {
            MatrixStack.Push(GUI.matrix);
            GUI.matrix = matrix;
        }

        /// <summary>
        /// Pops the GUI matrix pushed by <see cref="PushMatrix(Matrix4x4)"/>.
        /// </summary>
        public static void PopMatrix()
        {
            GUI.matrix = MatrixStack.Pop();
        }

        /// <summary>
        /// Ignores input on following GUI calls. Remember to end with <see cref="EndIgnoreInput"/>.
        /// </summary>
        public static void BeginIgnoreInput()
        {
            var e = Event.current.type;
            PushEventType(e == EventType.Layout || e == EventType.Repaint || e == EventType.Used ? e : EventType.Ignore);
        }

        /// <summary>
        /// Ends the ignore input started by <see cref="BeginIgnoreInput"/>.
        /// </summary>
        public static void EndIgnoreInput()
        {
            PopEventType();
        }

        /// <summary>
        /// Pushes the event type to the stack. Remember to pop with <see cref="PopEventType"/>.
        /// </summary>
        /// <param name="eventType">The type of event to push.</param>
        public static void PushEventType(EventType eventType)
        {
            EventTypeStack.Push(Event.current.type);
            Event.current.type = eventType;
        }

        /// <summary>
        /// Pops the event type pushed by <see cref="PopEventType"/>.
        /// </summary>
        public static void PopEventType()
        {
            Event.current.type = EventTypeStack.Pop();
        }

        /// <summary>
        /// Pushes the width to the editor GUI label width to the stack. Remmeber to Pop with <see cref="PopLabelWidth"/>.
        /// </summary>
        /// <param name="labelWidth">The editor GUI label width to push.</param>
        public static void PushLabelWidth(float labelWidth)
        {
            LabelWidthStack.Push(ActualLabelWidth);
            GUIHelper.BetterLabelWidth = labelWidth;
        }

        /// <summary>
        /// Pops editor gui label widths pushed by <see cref="PushLabelWidth(float)"/>.
        /// </summary>
        public static void PopLabelWidth()
        {
            GUIHelper.BetterLabelWidth = LabelWidthStack.Pop();
        }

        /// <summary>
        /// Pushes the value to the responsive vector component fields stack. Remeber to pop with <see cref="PopResponsiveVectorComponentFields"/>.
        /// </summary>
        public static void PushResponsiveVectorComponentFields(bool responsive)
        {
            ResponsiveVectorComponentFieldsStack.Push(SirenixEditorFields.ResponsiveVectorComponentFields);
            SirenixEditorFields.ResponsiveVectorComponentFields = responsive;
        }

        /// <summary>
        /// Pops responsive vector component fields value pushed by <see cref="PushResponsiveVectorComponentFields"/>.
        /// </summary>
        public static void PopResponsiveVectorComponentFields()
        {
            SirenixEditorFields.ResponsiveVectorComponentFields = ResponsiveVectorComponentFieldsStack.Pop();
        }

        /// <summary>
        /// Pushes the value to the fade group duration stack. Remeber to pop with <see cref="PopFadeGroupDuration"/>.
        /// </summary>
        public static void PushFadeGroupDuration(float duration)
        {
            FadeGroupDurationStack.Push(SirenixEditorGUI.DefaultFadeGroupDuration);
            SirenixEditorGUI.DefaultFadeGroupDuration = duration;
        }

        /// <summary>
        /// Pops fade group duration value pushed by <see cref="PushFadeGroupDuration"/>.
        /// </summary>
        public static void PopFadeGroupDuration()
        {
            SirenixEditorGUI.DefaultFadeGroupDuration = FadeGroupDurationStack.Pop();
        }

        /// <summary>
        /// Pushes the value to the tab page slide animation duration stack. Remember to pop with <see cref="PopTabPageSlideAnimationDuration"/>.
        /// </summary>
        /// <param name="duration"></param>
        public static void PushTabPageSlideAnimationDuration(float duration)
        {
            TabPageSlideAnimationDurationStack.Push(SirenixEditorGUI.TabPageSlideAnimationDuration);
            SirenixEditorGUI.TabPageSlideAnimationDuration = duration;
        }

        /// <summary>
        /// Pops tab page slide animation duration value pushed by <see cref="PushTabPageSlideAnimationDuration"/>.
        /// </summary>
        public static void PopTabPageSlideAnimationDuration()
        {
            SirenixEditorGUI.TabPageSlideAnimationDuration = TabPageSlideAnimationDurationStack.Pop();
        }

        /// <summary>
        /// Clears the repaint request.
        /// </summary>
        public static void ClearRepaintRequest()
        {
            if (numberOfFramesToRepaint > 0 && Event.current.type == EventType.Repaint)
            {
                numberOfFramesToRepaint--;
            }
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the config value.</typeparam>
        /// <param name="key">The key for the config.</param>
        /// <param name="name">The name of the config.</param>
        /// <returns>GUIConfig for the specified key and name.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, string name) where TValue : class, new()
        {
            var config = GUIContextCache<object, string, TValue>.GetConfig(key, name);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key for the config.</param>
        /// <param name="id">The ID for the config.</param>
        /// <returns>GUIConfig for the specified key and ID.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, int id) where TValue : class, new()
        {
            var config = GUIContextCache<object, int, TValue>.GetConfig(key, id);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="primaryKey">The primary key.</param>
        /// <param name="secondaryKey">The secondary key.</param>
        /// <returns>GUIConfig for the specified primary and secondary key.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object primaryKey, object secondaryKey) where TValue : class, new()
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key for the context.</param>
        /// <returns>GUIConfig for the specified key.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key) where TValue : class, new()
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(defaultConfigKey, key);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="key">Key for context.</param>
		/// <param name="name">Name for the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key, string name) where TValue : class
        {
            return GUIContextCache<object, string, TValue>.GetConfig(key, name);
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="key">Key for context.</param>
		/// <param name="id">Id of the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key, int id) where TValue : class
        {
            return GUIContextCache<object, int, TValue>.GetConfig(key, id);
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="primaryKey">Primary key for the context.</param>
		/// <param name="secondaryKey">Secondary key for the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object primaryKey, object secondaryKey) where TValue : class
        {
            return GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="key">Key for the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key) where TValue : class
        {
            return GUIContextCache<object, object, TValue>.GetConfig(defaultConfigKey, key);
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
		/// <param name="key">Key for the context.</param>
		/// <param name="name">Name for the context.</param>
		/// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, string name, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, string, TValue>.GetConfig(key, name);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
        /// <param name="key">Key for the context.</param>
        /// <param name="id">Id for the context.</param>
        /// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, int id, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, int, TValue>.GetConfig(key, id);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
        /// <param name="primaryKey">Primary key for the context.</param>
        /// <param name="secondaryKey">Secondary key for the context.</param>
        /// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object primaryKey, object secondaryKey, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
        /// <param name="key">Key for the context.</param>
        /// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(defaultValue, key);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified text.
        /// </summary>
        /// <param name="t">The text for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(string t)
        {
            tmpContent.image = null;
            tmpContent.text = t;
            tmpContent.tooltip = null;
            return tmpContent;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified text and tooltip.
        /// </summary>
        /// <param name="t">The text for the GUIContent.</param>
        /// <param name="tooltip">The tooltip for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(string t, string tooltip)
        {
            tmpContent.image = null;
            tmpContent.text = t;
            tmpContent.tooltip = tooltip;
            return tmpContent;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified image and tooltip.
        /// </summary>
        /// <param name="image">The image for the GUIContent.</param>
        /// <param name="tooltip">The tooltip for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(Texture image, string tooltip = null)
        {
            tmpContent.image = image;
            tmpContent.text = null;
            tmpContent.tooltip = tooltip;
            return tmpContent;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified text, image and tooltip.
        /// </summary>
        /// <param name="text">The text for the GUIContent.</param>
        /// <param name="image">The image for the GUIContent.</param>
        /// <param name="tooltip">The tooltip for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(string text, Texture image, string tooltip = null)
        {
            tmpContent.image = image;
            tmpContent.text = text;
            tmpContent.tooltip = tooltip;
            return tmpContent;
        }

        /// <summary>
        /// Indents the rect by the current indent amount.
        /// </summary>
        /// <param name="rect">The rect to indent.</param>
        /// <returns>Indented rect.</returns>
        public static Rect IndentRect(Rect rect)
        {
            var indent = CurrentIndentAmount;
            rect.x += indent;
            rect.width -= indent;
            return rect;
        }

        /// <summary>
        /// Indents the rect by the current indent amount.
        /// </summary>
        /// <param name="rect">The rect to indent.</param>
        public static void IndentRect(ref Rect rect)
        {
            var indent = CurrentIndentAmount;
            rect.x += indent;
            rect.width -= indent;
        }

        public static void PingObject(UnityEngine.Object obj)
        {
            if (obj == null) return;

            if (AssetDatabase.Contains(obj) && !AssetDatabase.IsMainAsset(obj))
            {
                var root = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(obj));

                if (root is Component)
                    root = (root as Component).gameObject;

                EditorGUIUtility.PingObject(root);
            }

            if (obj is Component)
                obj = (obj as Component).gameObject;

            EditorGUIUtility.PingObject(obj);
        }

        public static void SelectObject(UnityEngine.Object obj)
        {
            if (obj == null) return;

            if (AssetDatabase.Contains(obj) && !AssetDatabase.IsMainAsset(obj))
            {
                var root = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(obj));

                if (root is Component)
                    root = (root as Component).gameObject;

                Selection.activeObject = root;
            }
            else
            {
                if (obj is Component)
                    obj = (obj as Component).gameObject;

                Selection.activeObject = obj;
            }
        }


        /// <summary>
        /// Repaints the EditorWindow if a repaint has been requested.
        /// </summary>
        /// <param name="window">The window to repaint.</param>
        public static void RepaintIfRequested(this EditorWindow window)
        {
            EditorTimeHelper.Time.Update();
            if (GUIHelper.RepaintRequested)
            {
                if (window)
                {
                    window.Repaint();
                }

                GUIHelper.ClearRepaintRequest();
            }
        }

        /// <summary>
        /// Repaints the editor if a repaint has been requested. If the currently rendering window is not an InspectorWindow, Repaint() will be called on the current window as well.
        /// </summary>
        /// <param name="editor">The editor to repaint.</param>
        public static void RepaintIfRequested(this Editor editor)
        {
            EditorTimeHelper.Time.Update();

            if (GUIHelper.RepaintRequested)
            {
                if (editor) editor.Repaint();
                if (CurrentWindow) CurrentWindow.Repaint();

                GUIHelper.ClearRepaintRequest();
            }
        }

        private static readonly Dictionary<Type, Texture2D> typeTextureLookup = new Dictionary<Type, Texture2D>();

        /// <summary>
        /// Gets the best thumbnail icon given the provided arguments provided.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="preferObjectPreviewOverFileIcon"></param>
        /// <returns></returns>
        public static Texture2D GetAssetThumbnail(UnityEngine.Object obj, Type type, bool preferObjectPreviewOverFileIcon)
        {
            Texture2D icon = null;

            if (preferObjectPreviewOverFileIcon && obj)
            {
                icon = AssetPreview.GetAssetPreview(obj);
            }

            if (icon == null && obj)
            {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    icon = InternalEditorUtility.GetIconForFile(assetPath);
                }
            }

            if (obj && (icon == null || icon == EditorGUIUtility.FindTexture("DefaultAsset Icon")))
            {
                icon = AssetPreview.GetMiniThumbnail(obj);
            }

            if (icon == null && type != null)
            {
                typeTextureLookup.TryGetValue(type, out icon);

                if (icon == null)
                {
                    // TODO: Possibly find nice icons for structs, interface etc, unity types, normal types etc..
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        icon = EditorGUIUtility.FindTexture("cs Script Icon");
                    }
                    else
                    {
                        icon = AssetPreview.GetMiniTypeThumbnail(type);

                        if (icon == null)
                        {
                            icon = type.GetBaseClasses()
                                .Select(t =>
                                {
                                    try
                                    {
                                        // TODO:
                                        // Something in AssetPreview.GetMiniTypeThumbnail sometimes throws an NullReferenceException for some people.
                                        // I've been unable to find the root cause for this, so, for now, I'm going to just catch the exception here so that
                                        // users can still open the TypeSelector.
                                        return AssetPreview.GetMiniTypeThumbnail(t);
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                })
                                .FirstOrDefault(x => x);
                        }

                        if (icon == EditorGUIUtility.FindTexture("DefaultAsset Icon"))
                        {
                            if (typeof(ScriptableObject).IsAssignableFrom(type))
                            {
                                icon = AssetPreview.GetMiniTypeThumbnail(typeof(UnityEngine.BillboardAsset));
                            }
                            else
                            {
                                icon = EditorGUIUtility.FindTexture("cs Script Icon");
                            }
                        }
                    }

                    typeTextureLookup[type] = icon;
                }
            }

            return icon;
        }
    }
}
#endif