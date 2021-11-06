using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestControlID : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        [ShowIf("@this.mode == Mode.Show")]
        public int testInt;

        public Mode mode = Mode.Show;
    }

    public enum Mode
    {
        Show,
        DontShow,
    }

    [OnValueChanged("Choose")]
    public int chooseIndex;

    public void Choose()
    {
        if (chooseIndex >= 0 && chooseIndex < datas.Length)
        {
            data = datas[chooseIndex];
        }
    }

    public Data data;

    public Data[] datas;
}
