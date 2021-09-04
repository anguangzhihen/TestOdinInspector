using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestOdinIntro : MonoBehaviour
{
    [Button]
    public void Click()
    {
        Debug.LogError("Hello world");
    }
}
