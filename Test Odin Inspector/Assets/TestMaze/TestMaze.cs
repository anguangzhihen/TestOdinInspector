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
			// 为了方便观察，从(0, 0)点开始生成
			return new Vector2Int(0, 0);
			//return new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
		}
	}

	// 游戏开始运行
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

	// 生成主函数
	public IEnumerator Generate()
	{
		cells = new MazeCell[size.x, size.y];
		return MazeProcess.GetCurrentMazeProcess(this).Process();
	}

	// 创建墙
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

	// 创建通道
	public void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazePassage passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());
	}

	// 创建墙
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
	// 当时使用的算法
	public static MazeProcess currentMazeProcess = new EllerProcess();

	public static MazeProcess GetCurrentMazeProcess(TestMaze maze)
	{
		currentMazeProcess.maze = maze;
		return currentMazeProcess;
	}

	public TestMaze maze;

	public abstract IEnumerator Process();
}

// 随机深度优先算法
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
		// 获取最后一个加入的Cell
		int currentIndex = activeCells.Count - 1;
		MazeCell currentCell = activeCells[currentIndex];

		// 如果邻接的Cell都已经生成则移除
		if (currentCell.IsFullyInitialized)
		{
			activeCells.RemoveAt(currentIndex);
			return;
		}

		// 随机选择未初始化过的方向
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		Vector2Int coordinates = currentCell.coordinates + direction.ToIntVector2();
		if (maze.ContainsCoordinates(coordinates))
		{
			MazeCell neighbor = maze.GetCell(coordinates);
			if (neighbor == null)
			{
				// 没有邻居，则创建邻居，并创建通道
				neighbor = maze.CreateCell(coordinates);
				maze.CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
			else
			{
				// 有邻居，则创建墙
				maze.CreateWall(currentCell, neighbor, direction);
			}
		}
		else
		{
			// 超出边界直接创建墙
			maze.CreateWall(currentCell, null, direction);
		}
	}
}

// 最小生成树
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
		// 随机选择一个Cell（真正的Prim算法应该是按照边操作的，我们这边每条边取到的概率可能不太一样）
		int currentIndex = Random.Range(0, activeCells.Count);
		MazeCell currentCell = activeCells[currentIndex];

		// 如果邻接的Cell都已经生成则移除
		if (currentCell.IsFullyInitialized)
		{
			activeCells.RemoveAt(currentIndex);
			return;
		}

		// 随机选择未初始化过的方向
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		Vector2Int coordinates = currentCell.coordinates + direction.ToIntVector2();
		if (maze.ContainsCoordinates(coordinates))
		{
			MazeCell neighbor = maze.GetCell(coordinates);
			if (neighbor == null)
			{
				// 没有邻居，则创建邻居，并创建通道
				neighbor = maze.CreateCell(coordinates);
				maze.CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
			else
			{
				// 有邻居，则创建墙
				maze.CreateWall(currentCell, neighbor, direction);
			}
		}
		else
		{
			// 超出边界直接创建墙
			maze.CreateWall(currentCell, null, direction);
		}
	}
}

// 二叉树
public class RandomBinaryTreeProcess : MazeProcess
{
	public override IEnumerator Process()
	{
		WaitForSeconds delay = new WaitForSeconds(maze.generationStepDelay);

		// 从(0, 0)出发到(19, 19)
		for (int y = 0; y < maze.size.y; y++)
		{
			for (int x = 0; x < maze.size.x; x++)
			{
				var coordinate = new Vector2Int(x, y);

				// 选择当前处理的Cell，逐行处理
				MazeCell cell = maze.GetCell(coordinate);
				if (cell == null)
				{
					cell = maze.CreateCell(coordinate);
				}

				// 随机北方或东方
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

				// 往随机的方向创建通道
				var neighbor = maze.GetCell(coordinate + direction.ToIntVector2());
				if (neighbor == null)
				{
					neighbor = maze.CreateCell(coordinate + direction.ToIntVector2());
				}
				maze.CreatePassage(cell, neighbor, direction);

				// 后方尝试创建墙
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

// 威尔逊随机游走擦除算法，均匀生成树
public class WilsonProcess : MazeProcess
{
	private int totalCount;
	private int initedCount;

	public override IEnumerator Process()
	{
		WaitForSeconds delay = new WaitForSeconds(maze.generationStepDelay);

		// 创建第一个点
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

			// 创建路径
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

					// 是上一个Cell或下一个点是最后的点，创建通道
					var testCell = maze.GetCell(testPoint);
					if ((lastCell != null && testCell == lastCell) || (testCell == finalCell && i == paths.Count - 2))
					{
						maze.CreatePassage(testCell, cell, (cell.coordinates - testCell.coordinates).ToDirection());
					}
					// 不包含坐标或已经有Cell，直接创墙
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

	// 随机选择开始点
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

	// 随机游走
	public List<Vector2Int> RandomWalk(Vector2Int startPoint)
	{
		List<Vector2Int> paths = new List<Vector2Int>();
		paths.Add(startPoint);
		int errorCount = 0;
		while (true)
		{
			var lastPoint = paths[paths.Count - 1];

			// 随机一个方向
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
				// 路径已经包含，则删除后续的，放置出现环路
				paths.RemoveRange(foundIndex + 1, paths.Count - foundIndex - 1);
			}
			else
			{
				paths.Add(nextPoint);
				if (maze.GetCell(nextPoint) != null)
				{
					// 找到路径
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

		// 初始化第一行
		int nowY = 0;
		int groupIdMax = 10;
		for (int i = 0; i < maze.size.x; i++)
		{
			sets0[i] = groupIdMax++;
		}

		// 创建第一行Cell
		CreateCellByRow(nowY);
		CreateWallByRow(nowY);
		for (int i = 0; i < sets0.Length; i++)
		{
			var cell = maze.GetCell(new Vector2Int(i, nowY));
			maze.CreateWall(cell, null, MazeDirection.South);
		}

		while (true)
		{
			// 随机合并集合
			for (int i = 0; i < sets0.Length - 1; i++)
			{
				var value = sets0[i];
				if (sets0[i] == sets0[i + 1])
				{
					continue;
				}

				// 尝试合并邻居，最后一行合并所有邻居
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
					// 砸墙
					maze.RemoveEdge(maze.GetCell(new Vector2Int(i, nowY)), maze.GetCell(new Vector2Int(i + 1, nowY)), MazeDirection.East);
					maze.CreatePassage(maze.GetCell(new Vector2Int(i, nowY)), maze.GetCell(new Vector2Int(i + 1, nowY)), MazeDirection.East);
				}
			}

			nowY++;
			if (nowY >= maze.size.y)
			{
				break;
			}

			// 随机向下至少延伸一格
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

			// 补齐没有延伸的格子
			for (int i = 0; i < sets1.Length; i++)
			{
				if (sets1[i] == 0)
				{
					sets1[i] = groupIdMax++;
				}
			}

			// 创建下一行Cell和竖墙，需要注意的是这边先创建好竖墙，然后再融合邻居的步骤中砸开墙
			CreateCellByRow(nowY);
			CreateWallByRow(nowY);
			for (int i = 0; i < sets1.Length; i++)
			{
				var cell = maze.GetCell(new Vector2Int(i, nowY));

				// 创建横向的墙和通道
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

			// 准备下一个循环
			var tmp = sets0;
			sets0 = sets1;
			sets1 = tmp;
		}

		// 补最后的墙
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
		// 直接返回一个Dictionary，可能不是很好
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


