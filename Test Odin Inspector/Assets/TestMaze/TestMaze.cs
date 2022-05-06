using System;
using System.Collections;
using System.Collections.Generic;
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

}

public abstract class MazeProcess
{
	public static MazeProcess currentMazeProcess = new RandomBinaryTreeProcess();

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

