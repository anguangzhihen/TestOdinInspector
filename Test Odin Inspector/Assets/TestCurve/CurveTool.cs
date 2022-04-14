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

        public static float[][] gaussWX5 =
	    {
	        new []{ 0.5688888888888889f, 0.0000000000000000f },
	        new []{ 0.4786286704993665f, -0.5384693101056831f },
	        new []{ 0.4786286704993665f, 0.5384693101056831f },
	        new []{ 0.2369268850561891f, -0.9061798459386640f },
	        new []{ 0.2369268850561891f, 0.9061798459386640f },
	    };

	    public static float[][] gaussWX10 =
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

        public static float[][] gaussWX15 =
	    {
	        new []{ 0.2025782419255613f, 0.0000000000000000f },
	        new []{ 0.1984314853271116f, -0.2011940939974345f },
	        new []{ 0.1984314853271116f, 0.2011940939974345f },
	        new []{ 0.1861610000155622f, -0.3941513470775634f },
	        new []{ 0.1861610000155622f, 0.3941513470775634f },
	        new []{ 0.1662692058169939f, -0.5709721726085388f },
	        new []{ 0.1662692058169939f, 0.5709721726085388f },
	        new []{ 0.1395706779261543f, -0.7244177313601701f },
	        new []{ 0.1395706779261543f, 0.7244177313601701f },
	        new []{ 0.1071592204671719f, -0.8482065834104272f },
	        new []{ 0.1071592204671719f, 0.8482065834104272f },
	        new []{ 0.0703660474881081f, -0.9372733924007060f },
	        new []{ 0.0703660474881081f, 0.9372733924007060f },
	        new []{ 0.0307532419961173f, -0.9879925180204854f },
	        new []{ 0.0307532419961173f, 0.9879925180204854f },
        };

	    public static float[][] gaussWX20 =
	    {
	        new[] {0.1527533871307258f, -0.0765265211334973f},
	        new[] {0.1527533871307258f, 0.0765265211334973f},
	        new[] {0.1491729864726037f, -0.2277858511416451f},
	        new[] {0.1491729864726037f, 0.2277858511416451f},
	        new[] {0.1420961093183820f, -0.3737060887154195f},
	        new[] {0.1420961093183820f, 0.3737060887154195f},
	        new[] {0.1316886384491766f, -0.5108670019508271f},
	        new[] {0.1316886384491766f, 0.5108670019508271f},
	        new[] {0.1181945319615184f, -0.6360536807265150f},
	        new[] {0.1181945319615184f, 0.6360536807265150f},
	        new[] {0.1019301198172404f, -0.7463319064601508f},
	        new[] {0.1019301198172404f, 0.7463319064601508f},
	        new[] {0.0832767415767048f, -0.8391169718222188f},
	        new[] {0.0832767415767048f, 0.8391169718222188f},
	        new[] {0.0626720483341091f, -0.9122344282513259f},
	        new[] {0.0626720483341091f, 0.9122344282513259f},
	        new[] {0.0406014298003869f, -0.9639719272779138f},
	        new[] {0.0406014298003869f, 0.9639719272779138f},
	        new[] {0.0176140071391521f, -0.9931285991850949f},
	        new[] {0.0176140071391521f, 0.9931285991850949f},
	    };
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

		// Gauss-lengendre权重表 https://pomax.github.io/bezierinfo/legendre-gauss.html
		public float[][] gaussWX =
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

		public float[] SToTs(float[] sArr)
		{
			float[] result = new float[sArr.Length];
			for (int i = 0; i < sArr.Length; i++)
			{
				result[i] = S2T(sArr[i]);
			}
			return result;
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


