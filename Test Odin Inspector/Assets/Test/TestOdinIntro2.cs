using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestOdinIntro2 : MonoBehaviour
{
    public static int iValue;
}

[CustomEditor(typeof(TestOdinIntro2))]
public class TestOdinIntro2Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Click"))
        {
            Debug.LogError("Hello world2");
        }
    }
}
