using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class Test3 : MonoBehaviour
{
    //public float fValue = 0f;

    //[Button]
    //public void ChangeValueTwice()
    //{
    //    Undo.RecordObject(this, "Change Value 1");
    //    Undo.IncrementCurrentGroup();
    //    fValue = 1;
    //    Undo.RecordObject(this, "Change Value 2");
    //    fValue = 2;
    //}

    private bool startChange = false;
    private int groupId = 0;

    public int iValue1;
    public int iValue2;

    private void TryStartChange()
    {
        if (!startChange)
        {
            startChange = true;
            groupId = Undo.GetCurrentGroup();
        }
    }

    [Button]
    public void ChangeValue1()
    {
        TryStartChange();
        Undo.RecordObject(this, "Change Value 1");
        iValue1++;
    }

    [Button]
    public void ChangeValue2()
    {
        TryStartChange();
        Undo.RecordObject(this, "Change Value 2");
        iValue2++;
    }

    [Button]
    public void EndChange()
    {
        Undo.CollapseUndoOperations(groupId);
    }
}
