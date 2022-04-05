using System;
using System.Collections;
using System.Collections.Generic;
using AGZH;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestCurveInfo : MonoBehaviour
{
	public class PolylineCurve
	{
		public CurveMode mode = CurveMode.Bezier;

		public Vector3[] points;

		public int segments = 10;

		public float GetLength()
		{
			float sum = 0f;
			Vector3? prePoint = null;
			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments;
				var nowPoint = GetPoint(t);
				if (prePoint != null)
				{
					sum += (prePoint.Value - nowPoint).magnitude;
				}
				prePoint = nowPoint;
			}
			return sum;
		}

		public Vector3 GetPoint(float t)
		{
			if (mode == CurveMode.Bezier)
			{
				return CurveTool.GetBezierPoint(points, t);
			}
			else
			{
				return CurveTool.GetCatmullPoint(points, t);
			}
		}
	}

	public TestCurve testCurve;

	[Button]
	public void Test()
	{
		var mode = CurveMode.Bezier;
		var polylineCurve = new PolylineCurve();
		polylineCurve.mode = mode;
		if (mode == CurveMode.Bezier)
		{
			polylineCurve.points = testCurve.bezierPoints;
		}
		else
		{
			polylineCurve.points = testCurve.catmullPoints;
		}
		polylineCurve.segments = 10;

		TimeCatch cat = new TimeCatch();
		float length = 0f;
		float time = 0f;

		cat.Start();
		length = polylineCurve.GetLength();
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 10: len = " + length + ", time = " + time);

		cat.Start();
		polylineCurve.segments = 100;
		length = polylineCurve.GetLength();
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 100: len = " + length + ", time = " + time);

		cat.Start();
		polylineCurve.segments = 1000;
		length = polylineCurve.GetLength();
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 1000: len = " + length + ", time = " + time);

		cat.Start();
		polylineCurve.segments = 10000;
		length = polylineCurve.GetLength();
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 10000: len = " + length + ", time = " + time);

		cat.Start();
		polylineCurve.segments = 100000;
		length = polylineCurve.GetLength();
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 100000: len = " + length + ", time = " + time);

		cat.Start();
		polylineCurve.segments = 1000000;
		length = polylineCurve.GetLength();
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 1000000: len = " + length + ", time = " + time);

		ParametricCurve parametricCurve = null;
		if (mode == CurveMode.Bezier)
		{
			parametricCurve = ParametricCurve.CreateByBezier(testCurve.bezierPoints);
		}
		else
		{
			parametricCurve = ParametricCurve.CreateByCatmull(testCurve.catmullPoints);
		}

		cat.Start();
		length = parametricCurve.GetArcLength(1f);
		time = cat.GetTotalSeconds();
		Debug.LogError("Parametric 10: len = " + length + ", time = " + time);
	}
}

public class TimeCatch
{
	public DateTime startTime;

	public void Start()
	{
		startTime = DateTime.Now;
	}

	public float GetTotalSeconds()
	{
		return (float)(DateTime.Now - startTime).TotalSeconds;
	}
}
