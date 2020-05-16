using UnityEngine;

public class DungeonCell : MonoBehaviour
{
	public IntVector2 coordinates = default;
	private int initializedEdgeCount = default;
	public DungeonRoom room;

	public bool IsFullyInitialized => initializedEdgeCount == DungeonDirections.Count;
	private DungeonCellEdge[] edges = new DungeonCellEdge[DungeonDirections.Count];
	public DungeonCellEdge GetEdge(DungeonDirection direction) => edges[(int)direction];

	public void Initialize(DungeonRoom room)
	{
		room.Add(this);
		transform.GetChild(0).GetComponent<Renderer>().material = room.Settings.floorMaterial;
		transform.GetChild(1).GetComponent<Renderer>().material = room.Settings.floorMaterial;
	}

	public void SetEdge(DungeonDirection direction, DungeonCellEdge edge)
	{
		edges[(int)direction] = edge;
		initializedEdgeCount += 1;
	}

	public DungeonDirection RandomUninitializedDirection
	{
		get
		{
			int skips = Random.Range(0, DungeonDirections.Count - initializedEdgeCount);
			for(int i = 0; i < DungeonDirections.Count; i++)
			{
				if(edges[i] == null)
				{
					if(skips == 0)
					{
						return (DungeonDirection)i;
					}
					skips -= 1;
				}
			}
			throw new System.InvalidOperationException("DungeonCell has no uninitialized directions left.");
		}
	}

	public void OnPlayerEntered()
	{
		room.Show();
		for(int i = 0; i < edges.Length; i++)
		{
			edges[i].OnPlayerEntered();
		}
	}

	public void OnPlayerExited()
	{
		room.Hide();
		for(int i = 0; i < edges.Length; i++)
		{
			edges[i].OnPlayerExited();
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}
}