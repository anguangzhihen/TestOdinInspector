using UnityEngine;

namespace AGZH
{
	public class TestCurve : MonoBehaviour
	{
		public bool segmentSphere = true;
		public int segment = 50;
		public bool uniformSpeedDraw = false;

		public Vector3 point1 = Vector3.zero;
		public Vector3 controlPoint1 = Vector3.right;
		public Vector3 controlPoint2 = Vector3.left * 2;
		public Vector3 point2 = Vector3.left;

		public Vector3 GetPoint(float timePercent)
		{
			return CurveTool.GetBezierPoint(point1, controlPoint1, controlPoint2, point2, timePercent);
		}

		// 使用参数化长度比例s，保证运动是匀速的
		public Vector3 GetPointUniformSpeed(float s)
		{
			var paramCurve = ParametricCurve.CreateByBezier(new Vector3[] {point1, controlPoint1, controlPoint2, point2});
			return paramCurve.GetPointByS(s);
		}
	}

	
}

