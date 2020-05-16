using UnityEngine;

public enum DungeonDirection
{
	North,
	East,
	South,
	West
}

public static class DungeonDirections
{
	public const int Count = 4;

	public static DungeonDirection GetRandomValue() => (DungeonDirection)Random.Range(0, Count);
	public static DungeonDirection GetOpposite(this DungeonDirection direction) => opposites[(int)direction];
	public static IntVector2 ToIntVector2(this DungeonDirection direction) => vectors[(int)direction];
	public static Quaternion ToRotation(this DungeonDirection direction) => rotations[(int)direction];

	private static DungeonDirection[] opposites = {
		DungeonDirection.South,
		DungeonDirection.West,
		DungeonDirection.North,
		DungeonDirection.East
	};

	private static IntVector2[] vectors = {
		new IntVector2(0, 1),
		new IntVector2(1, 0),
		new IntVector2(0, -1),
		new IntVector2(-1, 0)
	};

	private static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};
}