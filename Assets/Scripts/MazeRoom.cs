using System.Collections.Generic;
using UnityEngine;

public class MazeRoom : ScriptableObject
{
	#region Variables
	[SerializeField] private int settingsIndex;
	[SerializeField] private MazeRoomSettings settings;
	[SerializeField] List<MazeCell> cells = new List<MazeCell>();
	#endregion

	#region Properties
	public MazeRoomSettings Settings { get => settings; set => settings = value; }
	public int SettingsIndex { get => settingsIndex; set => settingsIndex = value; }
	public List<MazeCell> Cells { get => cells; set => cells = value; }
	#endregion

	public void Add(MazeCell cell)
	{
		cell.room = this;
		cells.Add(cell);
	}

	public void Assimilate(MazeRoom room)
	{
		for(int i = 0; i < room.cells.Count; i++)
		{
			Add(room.cells[i]);
		}
	}
}
