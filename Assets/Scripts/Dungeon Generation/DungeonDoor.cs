using UnityEngine;

public class DungeonDoor : DungeonPassage
{
	public Transform hinge = default;

	private static Quaternion
	normalRotation = Quaternion.Euler(0f, -90f, 0f),
	mirroredRotation = Quaternion.Euler(0f, 90f, 0f);

	private bool isMirrored;
	private bool isOpen = false;

	private DungeonDoor GetOtherSideOfDoor() => otherCell.GetEdge(direction.GetOpposite()) as DungeonDoor;

	public override void Initialize(DungeonCell primary, DungeonCell other, DungeonDirection direction)
	{
		base.Initialize(primary, other, direction);
		if(GetOtherSideOfDoor() != null)
		{
			isMirrored = true;
			hinge.localScale = new Vector3(-4f, 4f, 4f);
			Vector3 p = hinge.localPosition;
			p.x = -p.x;
			hinge.localPosition = p;
		}
	}

	public override void OnPlayerEntered()
	{
		GetOtherSideOfDoor().hinge.localRotation = hinge.localRotation = isMirrored ? mirroredRotation : normalRotation;
		GetOtherSideOfDoor().cell.room.Show();
	}

	public override void OnPlayerExited()
	{
		GetOtherSideOfDoor().hinge.localRotation = hinge.localRotation = Quaternion.identity;
		GetOtherSideOfDoor().cell.room.Hide();
	}
}
