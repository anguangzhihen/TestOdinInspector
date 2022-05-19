using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class TestDynamicDraw : MonoBehaviour
{
	[Serializable]
	public class DynamicProperty
	{
		public string[] strs = new string[]
		{
			"System.String",
			"谈话内容",
			"你好"
		};
	}

	public DynamicProperty dp = new DynamicProperty();

	public sealed class DynamicPropertyResolver : BaseMemberPropertyResolver<DynamicProperty>
	{
		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			var typeStr = this.ValueEntry.SmartValue.strs[0];
			var propertyName = this.ValueEntry.SmartValue.strs[1];

			// 动态类型会bug暂时不看了
			//var gs = new NormalGetterSetter();
			//gs.OwnerType = typeof(DynamicProperty);
			//gs.ValueType = Type.GetType(typeStr);
			//gs.getter = o =>
			//{
			//	return ((DynamicProperty) o).strs[2];
			//};
			//gs.setter = (o, v) =>
			//{
			//	((DynamicProperty) o).strs[2] = v.ToString();
			//};

			return new[]
			{
				InspectorPropertyInfo.CreateValue(propertyName, 0, Property.ValueEntry.SerializationBackend, new GetterSetter<DynamicProperty, string>(
					(ref DynamicProperty o) => o.strs[2],
					(ref DynamicProperty o, string v) => o.strs[2] = v
					))
			};
		}
	}

	public class NormalGetterSetter : IValueGetterSetter
	{
		public bool IsReadonly { get; set; }
		public Type OwnerType { get; set; }
		public Type ValueType { get; set; }

		public Func<object, object> getter;
		public Action<object, object> setter;

		public void SetValue(object owner, object value)
		{
			setter(owner, value);
		}

		public object GetValue(object owner)
		{
			return getter(owner);
		}
	}

	[Button]
	public void Test()
	{
		Debug.LogError(typeof(string));

		var type = Type.GetType("System.String");
		Debug.LogError(type);
	}
}
