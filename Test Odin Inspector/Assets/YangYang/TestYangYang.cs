using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestYangYang : MonoBehaviour
{
	[Button]
	public void Test1()
	{
		string[] kinds = new[] {"A", "B"};


		string[] map = new[] {"A", "A", "B", "B"};

		IterateMap(kinds, map, mapTmp =>
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < map.Length; i++)
			{
				sb.Append(map[i]);
				sb.Append(" ");
			}

			Dictionary<int, int[]> index2Pres = new Dictionary<int, int[]>()
			{
				{ 1, new []{ 0 } },
				{ 3, new []{ 2 } },
			};
			int resultCount = 0;
			int successCount = 0;
			Iterate(map, index2Pres, result =>
			{
				//StringBuilder sb = new StringBuilder();
				//foreach (var i in result)
				//{
				//	sb.Append(i);
				//	sb.Append(map[i]);
				//	sb.Append(" ");
				//}
				//sb.Append(CheckTwoSame(map, result));
				//Debug.LogError(sb);
				resultCount++;
				if (CheckTwoSame(map, result))
				{
					successCount++;
				}
			});
			Debug.LogError(sb + " successCount " + successCount + ", resultCount " + resultCount);
		});


		
	}

	private void IterateMap(string[] kinds, string[] map, Action<string[]> getResultAction, int level = 0)
	{
		if (level >= map.Length)
		{
			getResultAction(map);
			return;
		}

		for (int i = 0; i < kinds.Length; i++)
		{
			map[level] = kinds[i];
			IterateMap(kinds, map, getResultAction, level + 1);
		}
	}

	private void Iterate(string[] map, Dictionary<int, int[]> index2Pres, Action<List<int>> getResultAction, List<int> used = null)
	{
		if (used == null)
		{
			used = new List<int>();
		}

		if (map.Length == used.Count)
		{
			getResultAction(used);
			return;
		}

		for (int nowIndex = 0; nowIndex < map.Length; nowIndex++)
		{
			if (used.Contains(nowIndex))
			{
				continue;
			}
			bool needContinue = false;
			if (index2Pres.TryGetValue(nowIndex, out var pres))
			{
				for (int j = 0; j < pres.Length; j++)
				{
					if (!used.Contains(pres[j]))
					{
						needContinue = true;
						break;
					}
				}
			}
			if (needContinue)
			{
				continue;
			}
			
			used.Add(nowIndex);

			Iterate(map, index2Pres, getResultAction, used);

			used.RemoveAt(used.Count - 1);
		}


	}

	private bool CheckTwoSame(string[] map, List<int> result)
	{
		if (result.Count % 2 == 1)
		{
			return false;
		}
		var len = result.Count / 2;
		for (int i = 0; i < len; i++)
		{
			if (map[result[i * 2]] != map[result[i * 2 + 1]])
			{
				return false;
			}
		}
		return true;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
