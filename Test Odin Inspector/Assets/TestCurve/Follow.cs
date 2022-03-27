using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace AGZH
{
	public class Follow : MonoBehaviour
	{
		private bool isStart = false;
		private DateTime startTime;

		public TestCurve target;
		public bool uniformSpeed;
		public float totalTime = 5f;

		[ShowInInspector]
		[DisplayAsString]
		public string time
		{
			get
			{
				GUIHelper.RequestRepaint();

				if (target == null)
				{
					StopFollow();
				}

				if (!isStart)
				{
					return "0.0";
				}

				float dTime = (float)(DateTime.Now - startTime).TotalSeconds;
				dTime = Mathf.Clamp(dTime, 0f, totalTime);
				if (uniformSpeed)
				{
					var newPos = target.GetPointUniformSpeed(dTime / totalTime);
					transform.position = newPos;
				}
				else
				{
					var newPos = target.GetPoint(dTime / totalTime);
					transform.position = newPos;
				}
				return dTime.ToString("#0.0");
			}
		}

		[Button]
		public void StartFollow()
		{
			startTime = DateTime.Now;
			isStart = true;
		}

		[Button]
		public void StopFollow()
		{
			isStart = false;
		}
	}
}
