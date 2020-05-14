using UnityEngine;

public class MazeDoor : MazePassage
{
	public Transform hinge = default;

	private static Quaternion
	normalRotation = Quaternion.Euler(0f, -90f, 0f),
	mirroredRotation = Quaternion.Euler(0f, 90f, 0f);

	private bool isMirrored;
	private bool isOpen = false;

	private MazeDoor GetOtherSideOfDoor() => otherCell.GetEdge(direction.GetOpposite()) as MazeDoor;

	public override void Initialize(MazeCell primary, MazeCell other, MazeDirection direction)
	{
		base.Initialize(primary, other, direction);
		if(GetOtherSideOfDoor() != null)
		{
			isMirrored = true;
			hinge.localScale = new Vector3(-1f, 1f, 1f);
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
