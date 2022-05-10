using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestMaze : MonoBehaviour
{
	public Vector2Int size = new Vector2Int(50, 50);

	public MazeCell[,] cells;

	public float generationStepDelay = 0.01f;

	public MazeCell cellPrefab;

	public MazePassage passagePrefab;

	public MazeWall wallPrefab;

	public Vector2Int RandomCoordinates
	{
		get
		{
			// Ϊ�˷���۲죬��(0, 0)�㿪ʼ����
			return new Vector2Int(0, 0);
			//return new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
		}
	}

	// ��Ϸ��ʼ����
	void Start()
	{
		StartCoroutine(Generate());
	}

	public bool ContainsCoordinates(Vector2Int coordinate)
	{
		return coordinate.x >= 0 && coordinate.x < size.x && coordinate.y >= 0 && coordinate.y < size.y;
	}

	public MazeCell GetCell(Vector2Int coordinates)
	{
		if (!ContainsCoordinates(coordinates))
		{
			return null;
		}
		return cells[coordinates.x, coordinates.y];
	}

	// ����������
	public IEnumerator Generate()
	{
		cells = new MazeCell[size.x, size.y];
		return MazeProcess.GetCurrentMazeProcess(this).Process();
	}

	// ����ǽ
	public MazeCell CreateCell(Vector2Int coordinates)
	{
		MazeCell newCell = Instantiate(cellPrefab);
		cells[coordinates.x, coordinates.y] = newCell;
		newCell.coordinates = coordinates;
		newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.y;
		newCell.transform.parent = transform;
		newCell.transform.localPosition =
			new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.y - size.y * 0.5f + 0.5f);
		return newCell;
	}

	// ����ͨ��
	public void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazePassage passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());
	}

	// ����ǽ
	public void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazeWall wall = Instantiate(wallPrefab) as MazeWall;
		wall.Initialize(cell, otherCell, direction);
		if (otherCell != null)
		{
			wall = Instantiate(wallPrefab) as MazeWall;
			wall.Initialize(otherCell, cell, direction.GetOpposite());
		}
	}

	public void RemoveEdge(MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		cell.RemoveEdge(direction);
		if (otherCell != null)
		{
			otherCell.RemoveEdge(direction.GetOpposite());
		}
	}

}

public abstract class MazeProcess
{
	// ��ʱʹ�õ��㷨
	public static MazeProcess currentMazeProcess = new EllerProcess();

	public static MazeProcess GetCurrentMazeProcess(TestMaze maze)
	{
		currentMazeProcess.maze = maze;
		return currentMazeProcess;
	}

	public TestMaze maze;

	public abstract IEnumerator Process();
}

// �����������㷨
public class RandomDFSProcess : MazeProcess
{
	public override IEnumerator Process()
	{
		WaitForSeconds delay = new WaitForSeconds(maze.generationStepDelay);
		List<MazeCell> activeCells = new List<MazeCell>();
		DoFirstGenerationStep(activeCells);
		while (activeCells.Count > 0)
		{
			DoNextGenerationStep(activeCells);
			yield return delay;
		}
	}

	private void DoFirstGenerationStep(List<MazeCell> activeCells)
	{
		activeCells.Add(maze.CreateCell(maze.RandomCoordinates));
	}

	private void DoNextGenerationStep(List<MazeCell> activeCells)
	{
		// ��ȡ���һ�������Cell
		int currentIndex = activeCells.Count - 1;
		MazeCell currentCell = activeCells[currentIndex];

		// ����ڽӵ�Cell���Ѿ��������Ƴ�
		if (currentCell.IsFullyInitialized)
		{
			activeCells.RemoveAt(currentIndex);
			return;
		}

		// ���ѡ��δ��ʼ�����ķ���
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		Vector2Int coordinates = currentCell.coordinates + direction.ToIntVector2();
		if (maze.ContainsCoordinates(coordinates))
		{
			MazeCell neighbor = maze.GetCell(coordinates);
			if (neighbor == null)
			{
				// û���ھӣ��򴴽��ھӣ�������ͨ��
				neighbor = maze.CreateCell(coordinates);
				maze.CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
			else
			{
				// ���ھӣ��򴴽�ǽ
				maze.CreateWall(currentCell, neighbor, direction);
			}
		}
		else
		{
			// �����߽�ֱ�Ӵ���ǽ
			maze.CreateWall(currentCell, null, direction);
		}
	}
}

// ��С������
public class RandomPrimProcess : MazeProcess
{
	public override IEnumerator Process()
	{
		WaitForSeconds delay = new WaitForSeconds(maze.generationStepDelay);
		List<MazeCell> activeCells = new List<MazeCell>();
		DoFirstGenerationStep(activeCells);
		while (activeCells.Count > 0)
		{
			DoNextGenerationStep(activeCells);
			yield return delay;
		}
	}

	private void DoFirstGenerationStep(List<MazeCell> activeCells)
	{
		activeCells.Add(maze.CreateCell(maze.RandomCoordinates));
	}

	private void DoNextGenerationStep(List<MazeCell> activeCells)
	{
		// ���ѡ��һ��Cell��������Prim�㷨Ӧ���ǰ��ձ߲����ģ��������ÿ����ȡ���ĸ��ʿ��ܲ�̫һ����
		int currentIndex = Random.Range(0, activeCells.Count);
		MazeCell currentCell = activeCells[currentIndex];

		// ����ڽӵ�Cell���Ѿ��������Ƴ�
		if (currentCell.IsFullyInitialized)
		{
			activeCells.RemoveAt(currentIndex);
			return;
		}

		// ���ѡ��δ��ʼ�����ķ���
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		Vector2Int coordinates = currentCell.coordinates + direction.ToIntVector2();
		if (maze.ContainsCoordinates(coordinates))
		{
			MazeCell neighbor = maze.GetCell(coordinates);
			if (neighbor == null)
			{
				// û���ھӣ��򴴽��ھӣ�������ͨ��
				neighbor = maze.CreateCell(coordinates);
				maze.CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
			else
			{
				// ���ھӣ��򴴽�ǽ
				maze.CreateWall(currentCell, neighbor, direction);
			}
		}
		else
		{
			// �����߽�ֱ�Ӵ���ǽ
			maze.CreateWall(currentCell, null, direction);
		}
	}
}

// ������
public class RandomBinaryTreeProcess : MazeProcess
{
	public override IEnumerator Process()
	{
		WaitForSeconds delay = new WaitForSeconds(maze.generationStepDelay);

		// ��(0, 0)������(19, 19)
		for (int y = 0; y < maze.size.y; y++)
		{
			for (int x = 0; x < maze.size.x; x++)
			{
				var coordinate = new Vector2Int(x, y);

				// ѡ��ǰ�����Cell�����д���
				MazeCell cell = maze.GetCell(coordinate);
				if (cell == null)
				{
					cell = maze.CreateCell(coordinate);
				}

				// ��������򶫷�
				MazeDirection direction;
				if (x == maze.size.x - 1 && y == maze.size.y - 1)
				{
					maze.CreateWall(cell, null, MazeDirection.North);
					maze.CreateWall(cell, null, MazeDirection.East);
					break;
				}
				if (x == maze.size.x - 1)
				{
					maze.CreateWall(cell, null, MazeDirection.East);
					direction = MazeDirection.North;
				}
				else if (y == maze.size.y - 1)
				{
					maze.CreateWall(cell, null, MazeDirection.North);
					direction = MazeDirection.East;
				}
				else
				{
					direction = Random.Range(0, 2) == 0 ? MazeDirection.North : MazeDirection.East;
				}

				// ������ķ��򴴽�ͨ��
				var neighbor = maze.GetCell(coordinate + direction.ToIntVector2());
				if (neighbor == null)
				{
					neighbor = maze.CreateCell(coordinate + direction.ToIntVector2());
				}
				maze.CreatePassage(cell, neighbor, direction);

				// �󷽳��Դ���ǽ
				if (cell.GetEdge(MazeDirection.South) == null)
				{
					var before = maze.GetCell(coordinate + MazeDirection.South.ToIntVector2());
					maze.CreateWall(cell, before, MazeDirection.South);
				}
				if (cell.GetEdge(MazeDirection.West) == null)
				{
					var before = maze.GetCell(coordinate + MazeDirection.West.ToIntVector2());
					maze.CreateWall(cell, before, MazeDirection.West);
				}

				yield return delay;
			}
		}
	}
}

// ����ѷ������߲����㷨������������
public class WilsonProcess : MazeProcess
{
	private int totalCount;
	private int initedCount;

	public override IEnumerator Process()
	{
		WaitForSeconds delay = new WaitForSeconds(maze.generationStepDelay);

		// ������һ����
		var firstCell = maze.CreateCell(maze.RandomCoordinates);
		for (int j = 0; j < MazeDirections.Count; j++)
		{
			var dir = (MazeDirection)j;
			var testPoint = firstCell.coordinates + dir.ToIntVector2();
			if (!maze.ContainsCoordinates(testPoint))
			{
				maze.CreateWall(firstCell, null, dir);
			}
		}

		totalCount = maze.size.x * maze.size.y;
		initedCount = 1;


		while (true)
		{
			if (initedCount > totalCount)
			{
				throw new Exception("initedCount > totalCount");
			}

			if (totalCount == initedCount)
			{
				yield break;
			}

			var startPoint = RandomStartPoint();
			List<Vector2Int> paths = RandomWalk(startPoint);

			//StringBuilder sb = new StringBuilder();
			//foreach (var p in paths)
			//{
			//	sb.Append(p);
			//	sb.Append(" | ");
			//}
			//Debug.LogError(sb);

			// ����·��
			MazeCell finalCell = maze.GetCell(paths[paths.Count - 1]);
			MazeCell lastCell = null;
			for (int i = 0; i < paths.Count - 1; i++)
			{
				var point = paths[i];
				yield return delay;
				MazeCell cell = maze.CreateCell(point);
				for (int j = 0; j < MazeDirections.Count; j++)
				{
					var dir = (MazeDirection)j;
					var testPoint = point + dir.ToIntVector2();

					// ����һ��Cell����һ���������ĵ㣬����ͨ��
					var testCell = maze.GetCell(testPoint);
					if ((lastCell != null && testCell == lastCell) || (testCell == finalCell && i == paths.Count - 2))
					{
						maze.CreatePassage(testCell, cell, (cell.coordinates - testCell.coordinates).ToDirection());
					}
					// ������������Ѿ���Cell��ֱ�Ӵ�ǽ
					else if (!maze.ContainsCoordinates(testPoint) || testCell != null)
					{
						maze.CreateWall(cell, null, dir);
					}
				}
				lastCell = cell;
			}
			initedCount += (paths.Count - 1);
		}
	}

	// ���ѡ��ʼ��
	public Vector2Int RandomStartPoint()
	{
		int skips = Random.Range(0, totalCount - initedCount);
		int row = 0;
		int col = 0;
		for (int i = 0; i < totalCount; i++)
		{
			row = i / maze.size.y;
			col = i % maze.size.y;
			var cell = maze.GetCell(new Vector2Int(col, row));
			if (cell == null)
			{
				if (skips == 0)
				{
					break;
				}
				skips--;
			}
		}
		return new Vector2Int(col, row);
	}

	// �������
	public List<Vector2Int> RandomWalk(Vector2Int startPoint)
	{
		List<Vector2Int> paths = new List<Vector2Int>();
		paths.Add(startPoint);
		int errorCount = 0;
		while (true)
		{
			var lastPoint = paths[paths.Count - 1];

			// ���һ������
			Vector2Int nextPoint;
			while (true)
			{
				if (errorCount++ == 100000)
				{
					throw new Exception("errorCount++ == 100000");
				}

				var randomDir = MazeDirections.RandomValue;
				nextPoint = lastPoint + randomDir.ToIntVector2();
				if (maze.ContainsCoordinates(nextPoint))
				{
					break;
				}
			}

			var foundIndex = paths.FindIndex(p => p.x == nextPoint.x && p.y == nextPoint.y);
			if (foundIndex >= 0)
			{
				// ·���Ѿ���������ɾ�������ģ����ó��ֻ�·
				paths.RemoveRange(foundIndex + 1, paths.Count - foundIndex - 1);
			}
			else
			{
				paths.Add(nextPoint);
				if (maze.GetCell(nextPoint) != null)
				{
					// �ҵ�·��
					break;
				}
			}
		}
		return paths;
	}
}

public class EllerProcess : MazeProcess
{
	public override IEnumerator Process()
	{
		WaitForSeconds delay = new WaitForSeconds(maze.generationStepDelay);

		int[] sets0 = new int[maze.size.x];
		int[] sets1 = new int[maze.size.x];

		ResetIntArray(sets0);
		ResetIntArray(sets1);

		// ��ʼ����һ��
		int nowY = 0;
		int groupIdMax = 10;
		for (int i = 0; i < maze.size.x; i++)
		{
			sets0[i] = groupIdMax++;
		}

		// ������һ��Cell
		CreateCellByRow(nowY);
		CreateWallByRow(nowY);
		for (int i = 0; i < sets0.Length; i++)
		{
			var cell = maze.GetCell(new Vector2Int(i, nowY));
			maze.CreateWall(cell, null, MazeDirection.South);
		}

		while (true)
		{
			// ����ϲ�����
			for (int i = 0; i < sets0.Length - 1; i++)
			{
				var value = sets0[i];
				if (sets0[i] == sets0[i + 1])
				{
					continue;
				}

				// ���Ժϲ��ھӣ����һ�кϲ������ھ�
				if (nowY == maze.size.y - 1 || Random.value >= 0.5f)
				{
					int targetValue = sets0[i + 1];
					for (int j = 0; j < sets0.Length; j++)
					{
						if (sets0[j] == targetValue)
						{
							sets0[j] = value;
						}
					}
					for (int j = 0; j < sets1.Length; j++)
					{
						if (sets1[j] == targetValue)
						{
							sets1[j] = value;
						}
					}

					yield return delay;
					// ��ǽ
					maze.RemoveEdge(maze.GetCell(new Vector2Int(i, nowY)), maze.GetCell(new Vector2Int(i + 1, nowY)), MazeDirection.East);
					maze.CreatePassage(maze.GetCell(new Vector2Int(i, nowY)), maze.GetCell(new Vector2Int(i + 1, nowY)), MazeDirection.East);
				}
			}

			nowY++;
			if (nowY >= maze.size.y)
			{
				break;
			}

			// ���������������һ��
			ResetIntArray(sets1);

			var groupId2CorX = GetGroupIdToCorX(sets0);
			foreach (var pair in groupId2CorX)
			{
				var groupId = pair.Key;
				var corXs = pair.Value;

				RandomSet(corXs);
				var count = Random.Range(1, corXs.Count + 1);
				//var count = 1;
				for (int i = 0; i < count; i++)
				{
					var x = corXs[i];
					sets1[x] = groupId;
				}
			}

			// ����û������ĸ���
			for (int i = 0; i < sets1.Length; i++)
			{
				if (sets1[i] == 0)
				{
					sets1[i] = groupIdMax++;
				}
			}

			// ������һ��Cell����ǽ����Ҫע���������ȴ�������ǽ��Ȼ�����ں��ھӵĲ������ҿ�ǽ
			CreateCellByRow(nowY);
			CreateWallByRow(nowY);
			for (int i = 0; i < sets1.Length; i++)
			{
				var cell = maze.GetCell(new Vector2Int(i, nowY));

				// ���������ǽ��ͨ��
				if (sets0[i] == sets1[i])
				{
					maze.CreatePassage(cell, maze.GetCell(new Vector2Int(i, nowY - 1)), MazeDirection.South);
				}
				else
				{
					yield return delay;
					maze.CreateWall(cell, maze.GetCell(new Vector2Int(i, nowY - 1)), MazeDirection.South);
				}
			}

			//StringBuilder sb = new StringBuilder();
			//sb.Clear();
			//for (int i = 0; i < sets1.Length; i++)
			//{
			//	sb.Append(sets1[i]);
			//	sb.Append(" | ");
			//}
			//sb.Append("\n");
			//for (int i = 0; i < sets0.Length; i++)
			//{
			//	sb.Append(sets0[i]);
			//	sb.Append(" | ");
			//}
			//Debug.LogError(sb.ToString());

			// ׼����һ��ѭ��
			var tmp = sets0;
			sets0 = sets1;
			sets1 = tmp;
		}

		// ������ǽ
		for (int i = 0; i < maze.size.x; i++)
		{
			yield return delay;
			var cell = maze.GetCell(new Vector2Int(i, maze.size.y - 1));
			maze.CreateWall(cell, null, MazeDirection.North);
		}
	}

	private void ResetIntArray(int[] arr)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = 0;
		}
	}


	private void CreateCellByRow(int y)
	{
		for (int i = 0; i < maze.size.x; i++)
		{
			maze.CreateCell(new Vector2Int(i, y));
		}
	}

	private void CreateWallByRow(int y)
	{
		maze.CreateWall(maze.GetCell(new Vector2Int(0, y)), null, MazeDirection.West);
		for (int i = 0; i < maze.size.x; i++)
		{
			var cell1 = maze.GetCell(new Vector2Int(i, y));
			var cell2 = maze.GetCell(new Vector2Int(i + 1, y));
			maze.CreateWall(cell1, cell2, MazeDirection.East);
		}
	}

	private Dictionary<int, List<int>> GetGroupIdToCorX(int[] sets)
	{
		// ֱ�ӷ���һ��Dictionary�����ܲ��Ǻܺ�
		Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
		for (int i = 0; i < sets.Length; i++)
		{
			var groupId = sets[i];

			List<int> corXs = null;
			if (!result.TryGetValue(groupId, out corXs))
			{
				corXs = new List<int>();
				result[groupId] = corXs;
			}
			corXs.Add(i);
		}
		return result;
	}

	private void RandomSet(List<int> set)
	{
		for (int i = 0; i < set.Count - 1; i++)
		{
			int index = Random.Range(i, set.Count);
			var tmp = set[index];
			set[index] = set[i];
			set[i] = tmp;
		}
	}
} 


