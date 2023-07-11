using System.Collections;
using System.Collections.Generic;
using AGZH;
using UnityEngine;

public class TestGetDerPoint : MonoBehaviour
{
    public TestCurve testCurve;
    public Transform target;
    
    void Update()
    {
        var paramCurve = ParametricCurve.CreateByBezier(testCurve.bezierPoints);
        transform.position = paramCurve.GetPoint(0.5f);
        target.position = transform.position + paramCurve.GetPointDer(0.5f);
    }
}
