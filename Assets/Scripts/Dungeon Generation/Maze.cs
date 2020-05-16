using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
	#region Variables
	[SerializeField] private IntVector2 size = default;
	[SerializeField] private MazeCell cellPrefab = default;
	[SerializeField] private MazePassage passagePrefab = default;
	[SerializeField] private MazeWall wallPrefab = default;
	[SerializeField] private MazeDoor doorPrefab = default;
	[Space]
	[SerializeField] [Range(0f, 0.1f)] private float doorProbability = default;
	[Space]
	[SerializeField] private MazeRoomSettings[] roomSettings = default;
	[Space]
	[SerializeField] private MazeCell[,] cells = default;
	[SerializeField] private List<MazeRoom> rooms = new List<MazeRoom>();

	#endregion

	#region Methods & Properties
	public IntVector2 GetRandomCoordinates() => new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
	public bool ContainsCoordinates(IntVector2 coordinate) => coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
	public MazeCell GetCell(IntVector2 coordinates) => cells[coordinates.x, coordinates.z];
	public List<MazeRoom> Rooms { get => rooms; set => rooms = value; }
	public IntVector2 Size { get => size; set => size = value; }
	public float DoorProbability { get => doorProbability; set => doorProbability = value; }
	#endregion

	#region Functions
	public IEnumerator Generate()
	{
		cells = new MazeCell[size.x, size.z];
		List<MazeCell> activeCells = new List<MazeCell>();
		DoFirstGenerationStep(activeCells);
		while(activeCells.Count > 0)
		{
			DoNextGenerationStep(activeCells);
		}
		for(int i = 0; i < rooms.Count; i++)
		{
			rooms[i].Hide();
		}
		rooms[0].Show();

		yield return new WaitForEndOfFrame();
	}

	private void DoFirstGenerationStep(List<MazeCell> activeCells)
	{
		MazeCell newCell = CreateCell(GetRandomCoordinates());
		newCell.Initialize(CreateRoom(-1));
		activeCells.Add(newCell);
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
			else if(currentCell.room.SettingsIndex == neighbor.room.SettingsIndex)
			{
				CreatePassageInSameRoom(currentCell, neighbor, direction);
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
		MazePassage prefab = Random.value < doorProbability ? doorPrefab : passagePrefab;
		MazePassage passage = Instantiate(prefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(prefab) as MazePassage;
		if(passage is MazeDoor)
		{
			otherCell.Initialize(CreateRoom(cell.room.SettingsIndex));
		}
		else
		{
			otherCell.Initialize(cell.room);
		}
		passage.Initialize(otherCell, cell, direction.GetOpposite());
	}

	private void CreatePassageInSameRoom(MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazePassage passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());
		if(cell.room != otherCell.room)
		{
			MazeRoom roomToAssimilate = otherCell.room;
			cell.room.Assimilate(roomToAssimilate);
			Destroy(roomToAssimilate);
		}
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

	private MazeRoom CreateRoom(int indexToExclude)
	{
		MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
		newRoom.SettingsIndex = Random.Range(0, roomSettings.Length);
		if(newRoom.SettingsIndex == indexToExclude)
		{
			newRoom.SettingsIndex = (newRoom.SettingsIndex + 1) % roomSettings.Length;
		}
		newRoom.Settings = roomSettings[newRoom.SettingsIndex];
		rooms.Add(newRoom);
		return newRoom;
	}
	#endregion
}