using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AGZH
{
	public class CurveTool
	{
		public static Vector3[] GetBezierPoints(Vector3[] points, int segment)
		{
			Vector3[] result = new Vector3[segment + 1];

			for (int i = 0; i <= segment; i++)
			{
				var t = (float) i / segment;
				var newPos = GetBezierPoint(points, t);
				result[i] = newPos;
			}

			return result;
		}

		public static Vector3 GetBezierPoint(Vector3[] points, float t)
		{
			Vector3 p0 = points[0];
			Vector3 p1 = points[1];
			Vector3 p2 = points[2];
			Vector3 p3 = points[3];

			float rest = (1f - t);
			Vector3 newPos = Vector3.zero;
			newPos += p0 * rest * rest * rest;
			newPos += p1 * t * 3f * rest * rest;
			newPos += p2 * 3f * t * t * rest;
			newPos += p3 * t * t * t;
			return newPos;
		}

		public static Vector3[] GetCatmullPoints(Vector3[] points, int segment)
		{
			Vector3[] result = new Vector3[segment + 1];

			for (int i = 0; i <= segment; i++)
			{
				var t = (float)i / segment;
				var newPos = GetCatmullPoint(points, t);
				result[i] = newPos;
			}

			return result;
		}

		public static Vector3 GetCatmullPoint(Vector3[] points, float t)
		{
			Vector3 a = points[0];
			Vector3 b = points[1];
			Vector3 c = points[2];
			Vector3 d = points[3];

			int pointCount = 4;
			int numSections = pointCount - 3;
			int tSec = (int)Mathf.Floor(t * numSections);
			int currPt = numSections - 1;
			if (currPt > tSec)
			{
				currPt = tSec;
			}
			float u = t * numSections - currPt;

			return .5f * (
				       (-a + 3f * b - 3f * c + d) * (u * u * u)
				       + (2f * a - 5f * b + 4f * c - d) * (u * u)
				       + (-a + c) * u
				       + 2f * b
			       );
		}

		public static Vector3[] CatmullPointsToBezier(Vector3[] catmull)
		{
			const float basis = 6f;
			Vector3[] bezier = new Vector3[4];
			bezier[0] = catmull[1];
			bezier[1] = catmull[1] + (-1 * catmull[0] + catmull[2]) / basis;
			bezier[2] = catmull[2] + (catmull[1] - catmull[3]) / basis;
			bezier[3] = catmull[2];
			return bezier;
		}
	}

	public class ParametricCurve
	{
		// Q(t) = A * t^3 + B * t^2 + C * t^1 + D
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

		public static ParametricCurve CreateByCatmull(Vector3[] points)
		{
			Vector3 a = points[0];
			Vector3 b = points[1];
			Vector3 c = points[2];
			Vector3 d = points[3];

			ParametricCurve curve = new ParametricCurve();
			curve.points = points;
			curve.A = .5f * (-a + 3f * b - 3f * c + d);
			curve.B = .5f * (2f * a - 5f * b + 4f * c - d);
			curve.C = .5f * (-a + c);
			curve.D = .5f * (2f * b);
			curve.arcLength = curve.GetArcLength(1f);
			return curve;
		}

		public Vector3[] points;
		public Vector3 A;
		public Vector3 B;
		public Vector3 C;
		public Vector3 D;
		public float arcLength = 0f;

		// Gauss-lengendreȨ�ر� https://pomax.github.io/bezierinfo/legendre-gauss.html
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

		// ���߹�ʽ
		public Vector3 GetPoint(float t)
		{
			return A * t * t * t + B * t * t + C * t + D;
		}

		// ���߹�ʽһ�׵���
		public Vector3 GetPointDer(float t)
		{
			return 3f * A * t * t + 2f * B * t + C;
		}

		// ��ȡ��������
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
			// ţ�ٵ�����
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

