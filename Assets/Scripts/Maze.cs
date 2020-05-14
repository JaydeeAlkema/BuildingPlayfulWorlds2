using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
	#region Variables
	[SerializeField] private IntVector2 size = default;
	[SerializeField] private MazeCell cellPrefab = default;
	[SerializeField] private MazePassage passagePrefab = default;
	[SerializeField] private MazeWall wallPrefab = default;
	[SerializeField] private MazeCell[,] cells = default;
	#endregion

	#region Methods
	public IntVector2 GetRandomCoordinates() => new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
	public bool ContainsCoordinates(IntVector2 coordinate) => coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
	public MazeCell GetCell(IntVector2 coordinates) => cells[coordinates.x, coordinates.z];
	#endregion

	#region Functions
	public void Generate()
	{
		cells = new MazeCell[size.x, size.z];
		List<MazeCell> activeCells = new List<MazeCell>();
		DoFirstGenerationStep(activeCells);
		while(activeCells.Count > 0)
		{
			DoNextGenerationStep(activeCells);
		}
	}

	private void DoFirstGenerationStep(List<MazeCell> activeCells)
	{
		activeCells.Add(CreateCell(GetRandomCoordinates()));
	}

	private void DoNextGenerationStep(List<MazeCell> activeCells)
	{
		int currentIndex = activeCells.Count - 1;
		MazeCell currentCell = activeCells[currentIndex];
		if(currentCell.IsFullyInitialized)
		{
			activeCells.RemoveAt(currentIndex);
			return;
		}
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
		if(ContainsCoordinates(coordinates))
		{
			MazeCell neighbor = GetCell(coordinates);
			if(neighbor == null)
			{
				neighbor = CreateCell(coordinates);
				CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
			else
			{
				CreateWall(currentCell, neighbor, direction);
			}
		}
		else
		{
			CreateWall(currentCell, null, direction);
		}
	}

	private MazeCell CreateCell(IntVector2 coordinates)
	{
		MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
		cells[coordinates.x, coordinates.z] = newCell;
		newCell.coordinates = coordinates;
		newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
		newCell.transform.parent = transform;
		newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
		return newCell;
	}

	private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazePassage passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());
	}

	private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazeWall wall = Instantiate(wallPrefab) as MazeWall;
		wall.Initialize(cell, otherCell, direction);
		if(otherCell != null)
		{
			wall = Instantiate(wallPrefab) as MazeWall;
			wall.Initialize(otherCell, cell, direction.GetOpposite());
		}
	}
	#endregion
}