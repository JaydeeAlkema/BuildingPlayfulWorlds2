using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Dungeon : MonoBehaviour
{
	#region Variables
	[SerializeField] private NavMeshSurface navMeshSurface = default;
	[SerializeField] private IntVector2 size = default;
	[SerializeField] private DungeonCell cellPrefab = default;
	[SerializeField] private DungeonPassage passagePrefab = default;
	[SerializeField] private DungeonWall wallPrefab = default;
	[SerializeField] private DungeonDoor doorPrefab = default;
	[SerializeField] private GameObject enemyPrefab = default;
	[Space]
	[SerializeField] [Range(0f, 0.1f)] private float doorProbability = default;
	[Space]
	[SerializeField] private DungeonRoomSettings[] roomSettings = default;
	[Space]
	[SerializeField] private DungeonCell[,] cells = default;
	[SerializeField] private List<DungeonRoom> rooms = new List<DungeonRoom>();

	private int roomIndex = 0;
	private int enemyIndex = 0;
	#endregion

	#region Methods & Properties
	public IntVector2 GetRandomCoordinates() => new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
	public bool ContainsCoordinates(IntVector2 coordinate) => coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
	public DungeonCell GetCell(IntVector2 coordinates) => cells[coordinates.x, coordinates.z];
	public List<DungeonRoom> Rooms { get => rooms; set => rooms = value; }
	public IntVector2 Size { get => size; set => size = value; }
	public float DoorProbability { get => doorProbability; set => doorProbability = value; }
	#endregion

	#region Functions
	public IEnumerator Generate()
	{
		cells = new DungeonCell[size.x, size.z];
		navMeshSurface.transform.localScale = new Vector3(size.x, 1f, size.z);
		List<DungeonCell> activeCells = new List<DungeonCell>();
		DoFirstGenerationStep(activeCells);
		while(activeCells.Count > 0)
		{
			DoNextGenerationStep(activeCells);
		}

		yield return StartCoroutine(BakeNavMeshSurfaces());
		yield return StartCoroutine(AddEnemiesToRooms());

		for(int i = 0; i < rooms.Count; i++)
		{
			rooms[i].Hide();
		}

		rooms[0].Show();    // This is the room where the player will be spawned and will always be a "safe" room.
		rooms[0].Settings = roomSettings[0];

		yield return new WaitForEndOfFrame();
	}

	private IEnumerator BakeNavMeshSurfaces()
	{
		Debug.Log("Baking NavMesh!");
		navMeshSurface.BuildNavMesh();
		Debug.Log("Baking NavMesh Complete!");
		yield return null;
	}

	private void DoFirstGenerationStep(List<DungeonCell> activeCells)
	{
		DungeonCell newCell = CreateCell(GetRandomCoordinates());
		newCell.Initialize(CreateRoom(-1));
		activeCells.Add(newCell);
	}

	private void DoNextGenerationStep(List<DungeonCell> activeCells)
	{
		int currentIndex = activeCells.Count - 1;
		DungeonCell currentCell = activeCells[currentIndex];
		if(currentCell.IsFullyInitialized)
		{
			activeCells.RemoveAt(currentIndex);
			return;
		}
		DungeonDirection direction = currentCell.RandomUninitializedDirection;
		IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
		if(ContainsCoordinates(coordinates))
		{
			DungeonCell neighbor = GetCell(coordinates);
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

	private DungeonCell CreateCell(IntVector2 coordinates)
	{
		DungeonCell newCell = Instantiate(cellPrefab) as DungeonCell;
		cells[coordinates.x, coordinates.z] = newCell;
		newCell.coordinates = coordinates;
		newCell.name = "Dungeon Cell " + coordinates.x + ", " + coordinates.z;
		newCell.transform.parent = transform;
		newCell.transform.localPosition = new Vector3((coordinates.x - size.x) * 10f, 0f, (coordinates.z - size.z) * 10f);
		return newCell;
	}

	private void CreatePassage(DungeonCell cell, DungeonCell otherCell, DungeonDirection direction)
	{
		DungeonPassage prefab = Random.value < doorProbability ? doorPrefab : passagePrefab;
		DungeonPassage passage = Instantiate(prefab) as DungeonPassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(prefab) as DungeonPassage;
		if(passage is DungeonDoor)
		{
			otherCell.Initialize(CreateRoom(cell.room.SettingsIndex));
		}
		else
		{
			otherCell.Initialize(cell.room);
		}
		passage.Initialize(otherCell, cell, direction.GetOpposite());
	}

	private void CreatePassageInSameRoom(DungeonCell cell, DungeonCell otherCell, DungeonDirection direction)
	{
		DungeonPassage passage = Instantiate(passagePrefab) as DungeonPassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(passagePrefab) as DungeonPassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());
		if(cell.room != otherCell.room)
		{
			DungeonRoom roomToAssimilate = otherCell.room;
			cell.room.Assimilate(roomToAssimilate);
			Destroy(roomToAssimilate);
		}
	}

	private void CreateWall(DungeonCell cell, DungeonCell otherCell, DungeonDirection direction)
	{
		DungeonWall wall = Instantiate(wallPrefab) as DungeonWall;
		wall.Initialize(cell, otherCell, direction);
		if(otherCell != null)
		{
			wall = Instantiate(wallPrefab) as DungeonWall;
			wall.Initialize(otherCell, cell, direction.GetOpposite());
		}
	}

	private DungeonRoom CreateRoom(int indexToExclude)
	{
		DungeonRoom newRoom = ScriptableObject.CreateInstance<DungeonRoom>();
		newRoom.SettingsIndex = roomIndex == 0 ? 0 : Random.Range(0, roomSettings.Length);

		if(newRoom.SettingsIndex == indexToExclude)
		{
			newRoom.SettingsIndex = (newRoom.SettingsIndex + 1) % roomSettings.Length;
		}
		newRoom.Settings = roomSettings[newRoom.SettingsIndex];
		rooms.Add(newRoom);

		roomIndex++;
		return newRoom;
	}

	private IEnumerator AddEnemiesToRooms()
	{
		for(int r = 0; r < rooms.Count; r++)
		{
			int amountOfEnemiesToSpawn = Random.Range(rooms[r].Settings.minEnemies, rooms[r].Settings.maxEnemies);
			for(int c = 0; c < amountOfEnemiesToSpawn; c++)
			{
				int randCellIndex = Random.Range(0, rooms[r].Cells.Count);

				// This seems to work 1 / 10th of the time
				//while(rooms[r].Cells[randCellIndex].occupied)
				//{
				//	randCellIndex = Random.Range(0, rooms[r].Cells.Count);
				//}

				GameObject newEnemy = Instantiate(enemyPrefab, rooms[r].Cells[randCellIndex].transform.position, Quaternion.identity, GameManager.Instance.EnemyOnInstantiateParent);
				newEnemy.name = "Enemy [" + enemyIndex + "]";
				rooms[r].EnemiesInRoom.Add(newEnemy);
				rooms[r].Cells[randCellIndex].occupied = true;
				enemyIndex++;
			}
		}
		yield return null;
	}
	#endregion
}