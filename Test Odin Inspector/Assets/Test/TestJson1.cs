using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestJson1 : MonoBehaviour
{
	#region Data1

	public class Data1A
	{
		public string name = "A";

		public Data1B dataB = new Data1B();
	}

	public class Data1B
	{
		public string name = "B";

		public string extData = "extData";
	}

	[Button]
	public void TestData1()
	{
		Data1A data = new Data1A();
		var str = JsonConvert.SerializeObject(data, Formatting.Indented);
		Debug.LogError(str);

		data = JsonConvert.DeserializeObject<Data1A>(str);
		Debug.LogError(data.dataB.extData);
	}

	#endregion

	#region Data2

	public class Data2A
	{
		public string name = "A";

		public Data2B dataB = new Data2B();
	}

	public class Data2B
	{
		public string name = "B";
	}

	public class Data2Ext
	{
		public Data2B data2B;
		public string extData = "extData";
	}

	public class Data2Base
	{
		public Data2A data;

		public List<Data2Ext> extData = new List<Data2Ext>();
	}

	[Button]
	public void TestData2()
	{
		Data2Base dataBase = new Data2Base();
		dataBase.data = new Data2A();
		dataBase.extData.Add(new Data2Ext() { data2B = dataBase.data.dataB });

		JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
		{
			PreserveReferencesHandling = PreserveReferencesHandling.Objects
		};

		var str = JsonConvert.SerializeObject(dataBase, Formatting.Indented, serializerSettings);
		Debug.LogError(str);

		dataBase = JsonConvert.DeserializeObject<Data2Base>(str, serializerSettings);
		Debug.LogError(dataBase.extData.Find(d => d.data2B == dataBase.data.dataB).extData);
	}

	#endregion

	#region Data3

	public class Data3A
	{
		public string name = "A";

		public Data3B dataB = new Data3B();
	}

	public class Data3B
	{
		public string name = "B";
	}

	[Button]
	public void TestData3()
	{
        var data = new Data3A();
	    var str = JsonConvert.SerializeObject(data, Formatting.Indented);
        Debug.LogError(str);

	    var dt = DataTree.Create(data);
	    foreach (var child in dt.GetChildNodes())
	    {
	        if (child.refObj == data.dataB)
	        {
	            child.extData = "extData";
	        }
	    }

	    JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
	    {
	        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
	    };

        var dtStr = JsonConvert.SerializeObject(dt, Formatting.Indented, serializerSettings);
	    Debug.LogError(dtStr);

        dt = JsonConvert.DeserializeObject<DataTree>(dtStr);
	    data = JsonConvert.DeserializeObject<Data3A>(str);

        DataTree.FillTree(dt, data);
	    foreach (var child in dt.GetChildNodes())
	    {
	        if (child.refObj == data.dataB)
	        {
	            Debug.LogError(child.extData);
	        }
	    }
	}

    #endregion
}

public class DataTree
{
	public DataNode root;

	// ´´½¨Ê÷
	public static DataTree Create(object obj)
	{
		DataTree tree = new DataTree();

		var rootNode = new DataNode();
		rootNode.refObj = obj;
		CreateCore(rootNode);

		tree.root = rootNode;
		return tree;
	}

	private static void CreateCore(DataNode parent)
	{
		if (parent.refObj == null)
		{
			return;
		}
		parent.children.Clear();
		var type = parent.refObj.GetType();

		if (parent.refObj is IList)
		{
			var list = (IList)parent.refObj;
			for (int i = 0; i < list.Count; i++)
			{
				var childValue = list[i];
				var childType = type.GetElementType();

				var childNode = CreateChild(parent, childType, childValue);
				if (childNode != null)
				{
					childNode.name = "$" + i.ToString();
				}
			}
		}
		else
		{
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var fieldInfo in fields)
			{
				var childType = fieldInfo.FieldType;
				var childValue = fieldInfo.GetValue(parent.refObj);
				var childNode = CreateChild(parent, childType, childValue);
				if (childNode != null)
				{
					childNode.name = fieldInfo.Name;
				}
			}
		}

		foreach (var child in parent.children)
		{
			CreateCore(child);
		}
	}

	private static DataNode CreateChild(DataNode parent, Type childType, object childValue)
	{
		if (childValue != null)
		{
			if (parent.GetParents().Any(n => n.refObj == childValue))
			{
				return null;
			}

			if (childType.IsValueType)
			{
				return null;
			}
			if (childValue is string)
			{
				return null;
			}
		}

		DataNode child = new DataNode();
		child.refObj = childValue;
		child.parent = parent;
		parent.children.Add(child);
		return child;
	}

	// Ìî³äÊ÷
	public static void FillTree(DataTree tree, object obj)
	{
		FillNodeCore(tree.root, obj);
	}

	private static void FillNodeCore(DataNode node, object obj)
	{
		if (obj == null || node == null)
		{
			return;
		}
		node.refObj = obj;

		foreach (var child in node.children)
		{
			if (child.name.StartsWith("$"))
			{
				var list = (IList)obj;
				var index = int.Parse(child.name.Substring(1));
				if (index >= 0 && index < list.Count)
				{
					FillNodeCore(child, list[index]);
				}
			}
			else
			{
				var type = obj.GetType();
				var field = type.GetField(child.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					var childObj = field.GetValue(obj);
					FillNodeCore(child, childObj);
				}
			}
		}
	}

	public IEnumerable<DataNode> GetChildNodes()
	{
		if (root == null)
		{
			yield break;
		}

		Queue<DataNode> queue = new Queue<DataNode>();
		queue.Enqueue(root);

		while (queue.Count > 0)
		{
			if (queue.Count > 10000)
			{
				Debug.LogError("queue.Count > 10000");
				yield break;
			}

			var node = queue.Dequeue();
			yield return node;

			foreach (var child in node.children)
			{
				queue.Enqueue(child);
			}
		}
	}
}

public class DataNode
{
    [JsonIgnore]
	public DataNode parent;

	public List<DataNode> children = new List<DataNode>();

	public string name;

	[JsonIgnore]
	public object refObj;

	public object extData;

	[JsonIgnore]
	public string path
	{
		get
		{
			List<DataNode> nodes = GetParents().ToList();

			StringBuilder sb = new StringBuilder();
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				if (nodes[i].name == null)
				{
					continue;
				}

				sb.Append("/");
				sb.Append(nodes[i].name);
			}

			var result = sb.ToString();
			if (string.IsNullOrEmpty(result))
			{
				return "/";
			}
			return result;
		}
	}

	public IEnumerable<DataNode> GetParents()
	{
		var node = this;
		while (node != null)
		{
			yield return node;
			node = node.parent;
		}
	}
}
