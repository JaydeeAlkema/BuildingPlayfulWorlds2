using UnityEngine;

/// <summary>
/// A simple class used by the floating healthbars so it always faces the player.
/// </summary>
public class RotateTowards : MonoBehaviour
{
	#region Variables
	[SerializeField] private Transform target = default;        // Where to rotate towards.
	#endregion

	#region Properties
	public Transform Target { get => target; set => target = value; }

	#endregion

	#region Monobehavour Callbacks
	private void Update()
	{
		if(!target)
			target = GameManager.Instance.PlayerInstance.transform;
		else
			transform.LookAt(target);
	}
	#endregion
}
