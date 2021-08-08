using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using UnityEngine;

public class Test1 : MonoBehaviour
{
	[OnValueChanged("OnAgeChanged")]
	public int age = 18;

	public void OnAgeChanged()
	{
		Debug.LogError("Œ“µƒƒÍ¡‰£∫" + age);

        var action = ActionResolver.Get(PropertyTree.Create(this).RootProperty, "");
		action.DoActionForAllSelectionIndices();
	}

	public static void Print()
	{
		Debug.LogError("Print");
	}
}
