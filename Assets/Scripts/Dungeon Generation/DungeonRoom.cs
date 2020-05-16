using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : ScriptableObject
{
	#region Variables
	[SerializeField] private int settingsIndex;
	[SerializeField] private DungeonRoomSettings settings;
	[SerializeField] List<DungeonCell> cells = new List<DungeonCell>();
	[SerializeField] private List<GameObject> enemiesInRoom = new List<GameObject>();
	#endregion

	#region Properties
	public DungeonRoomSettings Settings { get => settings; set => settings = value; }
	public int SettingsIndex { get => settingsIndex; set => settingsIndex = value; }
	public List<DungeonCell> Cells { get => cells; set => cells = value; }
	public List<GameObject> EnemiesInRoom { get => enemiesInRoom; set => enemiesInRoom = value; }
	#endregion

	public void Add(DungeonCell cell)
	{
		cell.room = this;
		cells.Add(cell);
	}

	public void Assimilate(DungeonRoom room)
	{
		for(int i = 0; i < room.cells.Count; i++)
		{
			Add(room.cells[i]);
		}
	}

	public void Hide()
	{
		for(int i = 0; i < cells.Count; i++)
		{
			cells[i].Hide();
		}
		for(int e = 0; e < enemiesInRoom.Count; e++)
		{
			enemiesInRoom[e].SetActive(false);
		}
	}

	public void Show()
	{
		for(int i = 0; i < cells.Count; i++)
		{
			cells[i].Show();
		}
		for(int e = 0; e < enemiesInRoom.Count; e++)
		{
			enemiesInRoom[e].SetActive(true);
		}
	}
}
