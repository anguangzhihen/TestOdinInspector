using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AGZH
{
    [CustomEditor(typeof(TestCurve))]
	public class TestCurveEditor : Editor
    {
	    private TestCurve script;

		public void OnEnable()
		{
            script = (TestCurve)target;
		}

	    void OnSceneGUI()
	    {
		    float size = 0f;

			Handles.color = Color.yellow;
		    size = Mathf.Clamp(HandleUtility.GetHandleSize(script.point1) * 0.5f, 0, 1f);
		    Handles.SphereHandleCap(0, script.point1, Quaternion.identity, size, EventType.Repaint);
		    size = Mathf.Clamp(HandleUtility.GetHandleSize(script.point2) * 0.5f, 0, 1f);
		    Handles.SphereHandleCap(0, script.point2, Quaternion.identity, size, EventType.Repaint);

			Handles.color = Color.blue;
		    Handles.DrawLine(script.controlPoint1, script.controlPoint2);
		    size = Mathf.Clamp(HandleUtility.GetHandleSize(script.controlPoint1) * 0.25f, 0, 0.5f);
			Handles.SphereHandleCap(0, script.controlPoint1, Quaternion.identity, size, EventType.Repaint);
		    size = Mathf.Clamp(HandleUtility.GetHandleSize(script.controlPoint2) * 0.25f, 0, 0.5f);
		    Handles.SphereHandleCap(0, script.controlPoint2, Quaternion.identity, size, EventType.Repaint);

			// »æÖÆÇúÏß
		    var color = Color.yellow;
		    color.a = 0.5f;
			Handles.color = color;
		    Vector3[] drawPoints = null;

			if (script.uniformSpeedDraw)
		    {
				var paramCurve = ParametricCurve.CreateByBezier(new Vector3[] { script.point1, script.controlPoint1, script.controlPoint2, script.point2 });
			    drawPoints = paramCurve.GetPointsByS(script.segment);
		    }
			else
		    {
				drawPoints = CurveTool.GetBezierPoints(script.point1, script.controlPoint1, script.controlPoint2, script.point2, script.segment);
			}

		    for (int i = 0; i < drawPoints.Length - 1; i++)
		    {
			    Handles.DrawLine(drawPoints[i], drawPoints[i + 1]);

			    if (script.segmentSphere && i != 0)
			    {
				    var pos = drawPoints[i];
				    size = Mathf.Clamp(HandleUtility.GetHandleSize(pos) * 0.25f, 0, 0.5f);
				    Handles.SphereHandleCap(0, pos, Quaternion.identity, size, EventType.Repaint);
			    }
		    }

			script.point1 = Handles.PositionHandle(script.point1, Quaternion.identity);
		    script.controlPoint1 = Handles.PositionHandle(script.controlPoint1, Quaternion.identity);
		    script.controlPoint2 = Handles.PositionHandle(script.controlPoint2, Quaternion.identity);
		    script.point2 = Handles.PositionHandle(script.point2, Quaternion.identity);
		}
	}
}
