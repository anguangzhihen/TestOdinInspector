using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MazeDirection
{
	North,
	East,
	South,
	West
}

public static class MazeDirections
{
	public const int Count = 4;

	public static MazeDirection RandomValue
	{
		get
		{
			return (MazeDirection)Random.Range(0, Count);
		}
	}

	private static MazeDirection[] opposites = {
		MazeDirection.South,
		MazeDirection.West,
		MazeDirection.North,
		MazeDirection.East
	};

	private static Vector2Int[] vectors = {
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0)
	};

	private static Quaternion[] rotations = {
		Quaternion.Euler(0f, 0f, 0f),
		Quaternion.Euler(0f, 0f, -90f),
		Quaternion.Euler(0f, 0f, -180f),
		Quaternion.Euler(0f, 0f, 90f)
	};

	public static Vector2Int ToIntVector2(this MazeDirection direction)
	{
		return vectors[(int)direction];
	}

	public static MazeDirection GetOpposite(this MazeDirection direction)
	{
		return opposites[(int)direction];
	}

	public static Quaternion ToRotation(this MazeDirection direction)
	{
		return rotations[(int)direction];
	}
}
