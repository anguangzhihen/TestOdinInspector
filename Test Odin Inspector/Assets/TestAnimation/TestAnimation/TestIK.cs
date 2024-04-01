using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TestIK : MonoBehaviour
{
	const float k_SqrEpsilon = 1e-8f;

	public Transform root;
    public Transform mid;
    public Transform tip;

    public Transform target;


    void Start()
    {
        
    }

	public void Update()
	{
		Test();
	}

	[Button]
    void Test()
	{
		Vector3 aPosition = root.position;
		Vector3 bPosition = mid.position;
		Vector3 cPosition = tip.position;

        Vector3 tPosition = target.position;
		Quaternion tRotation = target.rotation;

		Vector3 ab = bPosition - aPosition;
		Vector3 bc = cPosition - bPosition;
		Vector3 ac = cPosition - aPosition;
		Vector3 at = tPosition - aPosition;

		float abLen = ab.magnitude;
		float bcLen = bc.magnitude;
		float acLen = ac.magnitude;
		float atLen = at.magnitude;

		// 角B的度数
		float oldAbcAngle = TriangleAngle(acLen, abLen, bcLen);
		float newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

		Vector3 axis = Vector3.Cross(ab, bc);
		if (axis.sqrMagnitude < k_SqrEpsilon)
		{
			axis = Vector3.zero;

			if (axis.sqrMagnitude < k_SqrEpsilon)
				axis = Vector3.Cross(at, bc);

			if (axis.sqrMagnitude < k_SqrEpsilon)
				axis = Vector3.up;
		}
		axis = Vector3.Normalize(axis);

		//float a = 0.5f * (oldAbcAngle - newAbcAngle);
		//float sin = Mathf.Sin(a);
		//float cos = Mathf.Cos(a);
		//Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
		//mid.rotation = deltaR * mid.rotation;

		// 根据新算出来角度获取mid的旋转（mid只有一个轴能旋转）
		mid.localRotation = Quaternion.Euler(0f, 0f, 180f - newAbcAngle * Mathf.Rad2Deg);

		cPosition = tip.position;
		ac = cPosition - aPosition;

		//root.rotation = QuaternionExt.FromToRotation(ac, at) * root.rotation;
		root.rotation = Quaternion.LookRotation(at) * Quaternion.Inverse(Quaternion.LookRotation(ac)) *  root.rotation;

		tip.rotation = tRotation;
	}

	static float TriangleAngle(float aLen, float aLen1, float aLen2)
	{
		float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
		return Mathf.Acos(c);
	}
}
