using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    [MenuItem("Test/Test2/Create Cube")]
    static void CreateCube()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(cube, "Create Cube");
    }

    [MenuItem("Test/Test2/Random Rotate")]
    static void RandomRotate()
    {
        var tr = Selection.activeTransform;
        if (tr != null)
        {
            Undo.RecordObject(tr, "Rotate " + tr.name);
            tr.rotation = Random.rotation;
 
            Undo.RecordObject(tr, "Rotate " + tr.name);
            tr.rotation = Random.rotation;
        }
    }

    public Test2SerializedData data = null;

    [Button]
    public void CreateValue()
    {
        data = ScriptableObject.CreateInstance<Test2SerializedData>();
    }

    [Button]
    public void ChangeValue()
    {
        Undo.RecordObject(data, "Test2SerializedData Changed");
        //data.SetStrValue(data.intValue.ToString());
        data.intValue++;
        Debug.LogError("change to " + data.intValue);
    }
}

public class Test2SerializedData : ScriptableObject
{
    public Test2SerializedData()
    {
        SetStrValue("");
    }

    public void SetStrValue(string lastRow)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                sb.Append("a");
            }
            sb.Append("\n");
        }
        sb.Append(lastRow);
        strValue = sb.ToString();
    }

    public string strValue;
    public int intValue;
}
