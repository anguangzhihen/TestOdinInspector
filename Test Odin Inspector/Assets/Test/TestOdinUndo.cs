using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class TestOdinUndo : MonoBehaviour
{
    // 包裹的第二层，SerializedObject内部是序列化的数据，方便Unity处理
    private SerializedObject _serializedObject;

    public SerializedObject serializedObject
    {
        get
        {
            if (_serializedObject == null)
            {
                _serializedObject = new SerializedObject(testOdinUndoWrapData);
            }
            return _serializedObject;
        }
    }

    // 包裹的第一层，是个ScriptableObject
    private TestOdinUndoWrapData _testOdinUndoWrapData;

    private TestOdinUndoWrapData testOdinUndoWrapData
    {
        get
        {
            if (_testOdinUndoWrapData == null)
            {
                _testOdinUndoWrapData = ScriptableObject.CreateInstance<TestOdinUndoWrapData>();
                _testOdinUndoWrapData.data = testOdinUndoData;
            }
            return _testOdinUndoWrapData;
        }
    }

    private TestOdinUndoData testOdinUndoData = new TestOdinUndoData();

    private PropertyTree _propertyTree;
    public PropertyTree propertyTree
    {
        get
        {
            if (_propertyTree == null)
            {
                // 创建PropertyTree使用SerializedObject
                _propertyTree = PropertyTree.Create(serializedObject);
            }
            return _propertyTree;
        }
    }

    [OnInspectorGUI]
    public void OnInspectorGUI()
    {
        // 调用Draw，传入true
        propertyTree.Draw(true);
    }
}

public class TestOdinUndoWrapData : ScriptableObject
{
    public TestOdinUndoData data;
}

[Serializable]
public class TestOdinUndoData
{
    public int iValue;
}
