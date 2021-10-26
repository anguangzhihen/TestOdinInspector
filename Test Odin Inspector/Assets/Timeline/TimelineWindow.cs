using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TimelineWindow : EditorWindow
{
    [MenuItem("Tools/MyTimeline")]
    public static void ShowWindow()
    {
        var win = GetWindow<TimelineWindow>();
        win.Show();
        win.Focus();
    }
}
