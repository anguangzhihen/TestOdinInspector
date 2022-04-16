using AGZH;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class TestCurveInfo : MonoBehaviour
{
	public class PolylineCurve
	{
		public CurveMode mode = CurveMode.Bezier;

		public Vector3[] points;

		public int segments = 10;

		private float[] ts = null;

		private float[] ss = null;

		public float length = 0f;

		public void Calc()
		{
			length = GetLength();
		}

		public float GetLength()
		{
			float sum = 0f;
			Vector3? prePoint = null;
			ss = new float[segments + 1];
			ts = new float[segments + 1];
			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments;
				ts[i] = t;
				var nowPoint = GetPoint(t);
				if (prePoint != null)
				{
					sum += (prePoint.Value - nowPoint).magnitude;
				}
				ss[i] = sum;
				prePoint = nowPoint;
			}
			for (int i = 0; i < ss.Length; i++)
			{
				ss[i] /= sum;
			}
			return sum;
		}

		public float SToT(float s)
		{
			for (int i = 0; i < ss.Length - 1; i++)
			{
				if (s >= ss[i] && s <= ss[i + 1])
				{
					var percent = (s - ss[i]) / (ss[i + 1] - ss[i]);
					return ts[i] + (ts[i + 1] - ts[i]) * percent;
				}
			}
			return 0;
		}

		public float[] SToTs(float[] sArr)
		{
			float[] result = new float[sArr.Length];
			for (int i = 0; i < sArr.Length; i++)
			{
				result[i] = SToT(sArr[i]);
			}
			return result;
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

		public static float[] Get0To1DivideArray(int count, float maxValue = 1f)
		{
			var result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = ((float) i / count) * maxValue;
			}
			return result;
		}

		public static float[] Random(int count)
		{
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			var result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = UnityEngine.Random.Range(0f, 1f);
			}
			return result;

		}
	}

	public TestCurve testCurve;

	public class TsInfo
	{
		public TsInfo(ParametricCurve curve)
		{
			this.curve = curve;
		}

		private ParametricCurve curve;

		private float[] ts = null;

		private float[] ss = null;

		public void Calc(int segments)
		{
			ss = new float[segments + 1];
			ts = new float[segments + 1];
			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments;
				ts[i] = t;
				ss[i] = curve.T2S(t);
			}
		}

		public float SToT(float s)
		{
			for (int i = 0; i < ss.Length - 1; i++)
			{
				if (s >= ss[i] && s <= ss[i + 1])
				{
					var percent = (s - ss[i]) / (ss[i + 1] - ss[i]);
					return ts[i] + (ts[i + 1] - ts[i]) * percent;
				}
			}
			return 0;
		}

		public float[] SToTs(float[] sArr)
		{
			float[] result = new float[sArr.Length];
			for (int i = 0; i < sArr.Length; i++)
			{
				result[i] = SToT(sArr[i]);
			}
			return result;
		}

	}

	[Button]
	public void Test()
	{
		/* 结论：
		 *		1.计算长度，使用高斯10求积够用；
		 *		2.长度为100的曲线，计算ts数组，使用高斯10，100到1000，基本够用
		 */

		//float[] testS = PolylineCurve.Get0To1DivideArray(101, 0.1f);
		float[] testS = PolylineCurve.Random(101);

		StringBuilder tsResult = new StringBuilder();
		StringBuilder lengthResult = new StringBuilder();

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

		polylineCurve.segments = 1000;
		polylineCurve.Calc();
		var tsStandard = polylineCurve.SToTs(testS);

		polylineCurve.segments = 10;
		TimeCatch cat = new TimeCatch();
		float length = 0f;
		float time = 0f;
		float[] ts = null;

		cat.Start();
		for (int i = 0; i < 100; i++)
			polylineCurve.Calc();
		length = polylineCurve.length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 10: len = " + length + ", time = " + time);
		ts = polylineCurve.SToTs(testS);
		tsResult.Append("Polyline 10,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Polyline 10,").Append(length).Append(",").Append(time).Append("\n");

		cat.Start();
		polylineCurve.segments = 100;
		for (int i = 0; i < 100; i++)
			polylineCurve.Calc();
		length = polylineCurve.length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 100: len = " + length + ", time = " + time);
		ts = polylineCurve.SToTs(testS);
		tsResult.Append("Polyline 100,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Polyline 100,").Append(length).Append(",").Append(time).Append("\n");

		cat.Start();
		polylineCurve.segments = 1000;
		for (int i = 0; i < 100; i++)
			polylineCurve.Calc();
		length = polylineCurve.length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 1000: len = " + length + ", time = " + time);
		ts = polylineCurve.SToTs(testS);
		tsResult.Append("Polyline 1000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Polyline 1000,").Append(length).Append(",").Append(time).Append("\n");

		cat.Start();
		polylineCurve.segments = 10000;
		for (int i = 0; i < 100; i++)
			polylineCurve.Calc();
		length = polylineCurve.length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 10000: len = " + length + ", time = " + time);
		ts = polylineCurve.SToTs(testS);
		tsResult.Append("Polyline 10000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Polyline 10000,").Append(length).Append(",").Append(time).Append("\n");

		cat.Start();
		polylineCurve.segments = 100000;
		for (int i = 0; i < 100; i++)
			polylineCurve.Calc();
		length = polylineCurve.length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Polyline 100000: len = " + length + ", time = " + time);
		ts = polylineCurve.SToTs(testS);
		tsResult.Append("Polyline 100000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Polyline 100000,").Append(length).Append(",").Append(time).Append("\n");

		ParametricCurve parametricCurve = null;
		if (mode == CurveMode.Bezier)
		{
			parametricCurve = ParametricCurve.CreateByBezier(testCurve.bezierPoints);
		}
		else
		{
			parametricCurve = ParametricCurve.CreateByCatmull(testCurve.catmullPoints);
		}
		var tsInfo = new TsInfo(parametricCurve);

	    parametricCurve.gaussWX = CurveTool.gaussWX5;
		cat.Start();
		for (int i = 0; i < 100; i++)
			length = parametricCurve.GetArcLength(1f);
		parametricCurve.arcLength = length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Parametric 5: len = " + length + ", time = " + time);
		ts = parametricCurve.SToTs(testS);
		tsResult.Append("Parametric 5,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Parametric 5,").Append(length).Append(",").Append(time).Append("\n");

		tsInfo.Calc(10); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 5 segments 10,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(100); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 5 segments 100,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(1000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 5 segments 1000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(10000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 5 segments 10000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");

	    parametricCurve.gaussWX = CurveTool.gaussWX10;
		cat.Start();
		for (int i = 0; i < 100; i++)
			length = parametricCurve.GetArcLength(1f);
		parametricCurve.arcLength = length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Parametric 10: len = " + length + ", time = " + time);
		ts = parametricCurve.SToTs(testS);
		tsResult.Append("Parametric 10,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Parametric 10,").Append(length).Append(",").Append(time).Append("\n");

		tsInfo.Calc(10); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 10 segments 10,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(100); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 10 segments 100,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(1000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 10 segments 1000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(10000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 10 segments 10000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");

	    parametricCurve.gaussWX = CurveTool.gaussWX15;
		cat.Start();
		for (int i = 0; i < 100; i++)
			length = parametricCurve.GetArcLength(1f);
		parametricCurve.arcLength = length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Parametric 15: len = " + length + ", time = " + time);
		ts = parametricCurve.SToTs(testS);
		tsResult.Append("Parametric 15,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Parametric 15,").Append(length).Append(",").Append(time).Append("\n");

		tsInfo.Calc(10); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 15 segments 10,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(100); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 15 segments 100,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(1000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 15 segments 1000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(10000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 15 segments 10000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");

	    parametricCurve.gaussWX = CurveTool.gaussWX20;
		cat.Start();
		for (int i = 0; i < 100; i++)
			length = parametricCurve.GetArcLength(1f);
		parametricCurve.arcLength = length;
		time = cat.GetTotalSeconds();
		Debug.LogError("Parametric 20: len = " + length + ", time = " + time);
		ts = parametricCurve.SToTs(testS);
		tsResult.Append("Parametric 20,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		lengthResult.Append("Parametric 20,").Append(length).Append(",").Append(time).Append("\n");

		tsInfo.Calc(10); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 20 segments 10,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(100); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 20 segments 100,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(1000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 20 segments 1000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");
		tsInfo.Calc(10000); ts = tsInfo.SToTs(testS);
		tsResult.Append("Parametric 20 segments 10000,").Append(CalcVariance(tsStandard, ts)).Append(",").Append(ArrayString(ts, true)).Append("\n");

		File.WriteAllText("D://Result_TS.csv", tsResult.ToString());
		File.WriteAllText("D://Result_Length.csv", lengthResult.ToString());
    }

	public string ArrayString(float[] ts, bool noBrackets = false)
	{
		StringBuilder sb = new StringBuilder();
		if(!noBrackets)
			sb.Append("[");
		foreach (var t in ts)
		{
			sb.Append(t);
			sb.Append(", ");
		}
		if(!noBrackets)
			sb.Append("]");
		return sb.ToString();
	}

	public float CalcVariance(float[] standard, float[] value)
	{
		float sum = 0f;
		for (int i = 0; i < standard.Length; i++)
		{
			var v = standard[i] - value[i];
			sum += v * v;
		}
		return sum;
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
