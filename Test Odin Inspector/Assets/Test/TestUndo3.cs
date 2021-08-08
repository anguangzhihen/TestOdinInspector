using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class TestUndo3 : MonoBehaviour
{
    public Undo3WrapData wrapData;

    [Button]
    public void CreateData()
    {
        wrapData = ScriptableObject.CreateInstance<Undo3WrapData>();
    }

    [Button]
    public void ChangeData()
    {
        Undo.RecordObject(wrapData, "change undo3");
        wrapData.data.iValue++;
        Debug.Log("iValue change to " + wrapData.data.iValue);
    }

    [Button]
    public void PrintData()
    {
        Debug.Log("iValue: " + wrapData.data.iValue);
    }
}

public class Undo3WrapData : ScriptableObject, ISerializationCallbackReceiver
{
    public string serializedData;

    public Undo3Data data = new Undo3Data();

    public void OnBeforeSerialize()
    {
        serializedData = "iValue:" + data.iValue;
    }

    public void OnAfterDeserialize()
    {
        if (string.IsNullOrEmpty(serializedData))
        {
            return;
        }
        data.iValue = int.Parse(serializedData.Split(':')[1]);
    }
}

public class Undo3Data
{
    public int iValue;
}

