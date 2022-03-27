using Sirenix.OdinInspector;
using UnityEngine;

namespace AGZH
{
	public enum CurveMode
	{
		Bezier,
		Catmull
	}

	public class TestCurve : MonoBehaviour
	{
		public bool segmentSphere = true;
		public int segment = 50;
		public bool uniformSpeedDraw = false;

		public CurveMode mode = CurveMode.Bezier;

		public void ToBezier()
		{
			if (mode == CurveMode.Bezier)
			{
				return;
			}

			if (mode == CurveMode.Catmull)
			{

				bezierPoints = CurveTool.CatmullPointsToBezier(catmullPoints);
				mode = CurveMode.Bezier;
			}
		}

		public Vector3 point1 = Vector3.zero;
		public Vector3 controlPoint1 = Vector3.right;
		public Vector3 controlPoint2 = Vector3.left * 2;
		public Vector3 point2 = Vector3.left;

		public Vector3[] bezierPoints
		{
			get { return new[] {point1, controlPoint1, controlPoint2, point2}; }
			set
			{
				point1 = value[0];
				controlPoint1 = value[1];
				controlPoint2 = value[2];
				point2 = value[3];
			}
		}

		public Vector3[] catmullPoints
		{
			get { return new[] {controlPoint1, point1, point2, controlPoint2}; }
		}

		public Vector3 GetPoint(float timePercent)
		{

			if (mode == CurveMode.Bezier)
			{
				return CurveTool.GetBezierPoint(bezierPoints, timePercent);
			}
			else
			{
				return CurveTool.GetCatmullPoint(catmullPoints, timePercent);
			}
		}


		// 使用参数化长度比例s，保证运动是匀速的
		public Vector3 GetPointUniformSpeed(float s)
		{
			if (mode == CurveMode.Bezier)
			{
				var paramCurve = ParametricCurve.CreateByBezier(bezierPoints);
				return paramCurve.GetPointByS(s);
			}
			else
			{
				var paramCurve = ParametricCurve.CreateByCatmull(catmullPoints);
				return paramCurve.GetPointByS(s);
			}
		}
	}

	
}

