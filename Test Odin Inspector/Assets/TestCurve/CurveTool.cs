using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AGZH
{
	public class CurveTool
	{
		public static Vector3[] GetBezierPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int segment)
		{
			Vector3[] result = new Vector3[segment + 1];

			for (int i = 0; i <= segment; i++)
			{
				var t = (float) i / segment;
				var newPos = GetBezierPoint(p0, p1, p2, p3, t);
				result[i] = newPos;
			}

			return result;
		}

		public static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			float rest = (1f - t);
			Vector3 newPos = Vector3.zero;
			newPos += p0 * rest * rest * rest;
			newPos += p1 * t * 3f * rest * rest;
			newPos += p2 * 3f * t * t * rest;
			newPos += p3 * t * t * t;
			return newPos;
		}
	}

	public class ParametricCurve
	{
		public static ParametricCurve CreateByBezier(Vector3[] points)
		{
			ParametricCurve curve = new ParametricCurve();
			curve.points = points;
			curve.A = -1 * points[0] + 3 * points[1] - 3 * points[2] + points[3];
			curve.B = 3 * points[0] - 6 * points[1] + 3 * points[2];
			curve.C = -3 * points[0] + 3 * points[1];
			curve.D = points[0];
			curve.arcLength = curve.GetArcLength(1f);
			return curve;
		}

		public Vector3[] points;
		public Vector3 A;
		public Vector3 B;
		public Vector3 C;
		public Vector3 D;
		public float arcLength = 0f;

		// Gauss-lengendre权重表 https://pomax.github.io/bezierinfo/legendre-gauss.html
		private float[][] gaussWX =
		{
			new []{ 0.2955242247147529f, -0.1488743389816312f },
			new []{ 0.2955242247147529f, 0.1488743389816312f },
			new []{ 0.2692667193099963f, -0.4333953941292472f },
			new []{ 0.2692667193099963f, 0.4333953941292472f },
			new []{ 0.2190863625159820f, -0.6794095682990244f },
			new []{ 0.2190863625159820f, 0.6794095682990244f },
			new []{ 0.1494513491505806f, -0.8650633666889845f },
			new []{ 0.1494513491505806f, 0.8650633666889845f },
			new []{ 0.0666713443086881f, -0.9739065285171717f },
			new []{ 0.0666713443086881f, 0.9739065285171717f },
		};

		// 曲线公式
		public Vector3 GetPoint(float t)
		{
			return A * t * t * t + B * t * t + C * t + D;
		}

		// 曲线公式一阶导数
		public Vector3 GetPointDer(float t)
		{
			return 3f * A * t * t + 2f * B * t + C;
		}

		// 获取参数长度
		public float GetArcLength(float t)
		{
			var halfT = t / 2f;

			float sum = 0f;
			foreach (var wx in gaussWX)
			{
				var w = wx[0];
				var x = wx[1];
				sum += w * GetPointDer(halfT * x + halfT).magnitude;
			}
			sum *= halfT;
			return sum;
		}

		public float T2S(float t)
		{
			return GetArcLength(t) / arcLength;
		}

		public float T2SDer(float t)
		{
			return GetPointDer(t).magnitude / arcLength;
		}

		public float S2T(float s)
		{
			const int NEWTON_SEGMENT = 4;

			s = Mathf.Clamp01(s);
			float t = s;
			// 牛顿迭代法
			for (int i = 0; i < NEWTON_SEGMENT; i++)
			{
				t = t - (T2S(t) - s) / T2SDer(t);
			}
			return t;
		}

		public Vector3 GetPointByS(float s)
		{
			var t = S2T(s);
			return GetPoint(t);
		}

		public Vector3[] GetPointsByS(int segment)
		{
			Vector3[] result = new Vector3[segment + 1];

			for (int i = 0; i <= segment; i++)
			{
				var s = (float)i / segment;
				var newPos = GetPointByS(s);
				result[i] = newPos;
			}

			return result;
		}
	}
}


