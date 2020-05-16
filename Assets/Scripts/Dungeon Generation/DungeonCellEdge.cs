using UnityEngine;

public abstract class DungeonCellEdge : DungeonCell
{
	public DungeonCell cell, otherCell;

	public DungeonDirection direction = default;

	public virtual void Initialize(DungeonCell cell, DungeonCell otherCell, DungeonDirection direction)
	{
		this.cell = cell;
		this.otherCell = otherCell;
		this.direction = direction;
		cell.SetEdge(direction, this);
		transform.parent = cell.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = direction.ToRotation();
	}

	public virtual void OnPlayerEntered() { }

	public virtual void OnPlayerExited() { }
}