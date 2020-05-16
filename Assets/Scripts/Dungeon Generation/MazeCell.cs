using UnityEngine;

public class MazeCell : MonoBehaviour
{
	public IntVector2 coordinates = default;
	private int initializedEdgeCount = default;
	public MazeRoom room;

	public bool IsFullyInitialized => initializedEdgeCount == MazeDirections.Count;
	private MazeCellEdge[] edges = new MazeCellEdge[MazeDirections.Count];
	public MazeCellEdge GetEdge(MazeDirection direction) => edges[(int)direction];

	public void Initialize(MazeRoom room)
	{
		room.Add(this);
		transform.GetChild(0).GetComponent<Renderer>().material = room.Settings.floorMaterial;
		transform.GetChild(1).GetComponent<Renderer>().material = room.Settings.floorMaterial;
	}

	public void SetEdge(MazeDirection direction, MazeCellEdge edge)
	{
		edges[(int)direction] = edge;
		initializedEdgeCount += 1;
	}

	public MazeDirection RandomUninitializedDirection
	{
		get
		{
			int skips = Random.Range(0, MazeDirections.Count - initializedEdgeCount);
			for(int i = 0; i < MazeDirections.Count; i++)
			{
				if(edges[i] == null)
				{
					if(skips == 0)
					{
						return (MazeDirection)i;
					}
					skips -= 1;
				}
			}
			throw new System.InvalidOperationException("MazeCell has no uninitialized directions left.");
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