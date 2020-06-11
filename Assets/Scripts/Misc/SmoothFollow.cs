using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
	#region Variables
	[SerializeField] private Transform target = null;               // What to follow
	[SerializeField] private float smoothing = 10f;                 // How much smoothing there is (in seconds)
	[SerializeField] private Vector3 offset = Vector3.zero;         // Base offset from the target.
	[Space]
	[SerializeField] private bool follow = true;                    // is following the target allowed?
	[SerializeField] private bool rotate = false;                   // is rotating with the target allowed?

	private Vector3 velocity = Vector3.zero;                        // Velocity of this
	private Vector3 desiredPos = Vector3.zero;
	private Vector3 smoothedPos = Vector3.zero;
	#endregion

	#region Properties
	public Transform Target { get => target; set => target = value; }
	#endregion

	#region Monobehaviour Callbacks
	private void FixedUpdate()
	{
		if(!target) return;

		desiredPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, target.position.z + offset.z);
		smoothedPos = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothing);

		transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
		transform.position = smoothedPos;
	}
	#endregion
}
