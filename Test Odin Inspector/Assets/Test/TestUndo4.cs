using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestUndo4 : MonoBehaviour
{
	// Start is called before the first frame update
	public class TestUndo4WrapData : ScriptableObject, ISerializationCallbackReceiver
	{
		public int age = 10;

		public void OnBeforeSerialize()
		{
			Debug.LogError("OnBeforeSerialize age = " + age);
		}

		public void OnAfterDeserialize()
		{
			Debug.LogError("OnAfterDeserialize age = " + age);
		}
	}

	TestUndo4WrapData _testUndo4WrapData;
	TestUndo4WrapData testUndo4WrapData 
	{
		get
		{
			if(_testUndo4WrapData == null )
			{
				_testUndo4WrapData = new TestUndo4WrapData();
			}
			return _testUndo4WrapData;
		}
		
	}

	[Button]
    public void ChangeAgeTo20()
    {
        Undo.RecordObject(testUndo4WrapData, "ChangeAgeTo20");
		testUndo4WrapData.age = 20;
    }
}
