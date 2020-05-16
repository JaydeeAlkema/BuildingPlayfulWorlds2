using System.Collections;
using UnityEngine;

public class PlayerMovementBehaviour : MonoBehaviour
{
	[SerializeField] private string horizontalInputName = default;
	[SerializeField] private string verticalInputName = default;
	[Space]
	private float movementSpeed = default;
	[SerializeField] private float walkSpeed = default;
	[SerializeField] private float runSpeed = default;
	[SerializeField] private float runBuildUpSpeed = default;
	[SerializeField] private KeyCode runKey = default;
	[SerializeField] private float slopeForce = default;
	[SerializeField] private float slopeForceRayLength = default;
	[Space]
	[SerializeField] private bool isJumping = false;
	[SerializeField] private AnimationCurve jumpfallOff = default;
	[SerializeField] private float jumpMultiplier = default;
	[SerializeField] private KeyCode jumpKey = default;
	[Space]
	[SerializeField] private DungeonCell cellUnderneathPlayer = default;
	[SerializeField] private DungeonCell previousCellUnderneathPlayer = default;
	private CharacterController charController = default;

	private void Awake()
	{
		charController = GetComponent<CharacterController>();
	}

	private void Update()
	{
		PlayerMovement();
		CheckForCellUnderneathPlayer();
	}

	private void PlayerMovement()
	{
		float horInput = Input.GetAxisRaw(horizontalInputName);
		float verInput = Input.GetAxisRaw(verticalInputName);

		Vector3 forwardMovement = transform.forward * verInput;
		Vector3 rightMovement = transform.right * horInput;

		charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1f) * movementSpeed);

		if((verInput != 0f || horInput != 0f) && OnSlope())
			charController.Move(Vector3.down * charController.height / 2f * slopeForce * Time.deltaTime);

		SetMovementSpeed();
		JumpInput();
	}

	private void SetMovementSpeed()
	{
		if(Input.GetKey(runKey))
			movementSpeed = Mathf.Lerp(movementSpeed, runSpeed, Time.deltaTime * runBuildUpSpeed);
		else
			movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, Time.deltaTime * runBuildUpSpeed);
	}

	private void JumpInput()
	{
		if(Input.GetKeyDown(jumpKey) && !isJumping)
		{
			isJumping = true;
			StartCoroutine(JumpEvent());
		}
	}

	private IEnumerator JumpEvent()
	{
		charController.slopeLimit = 90f;
		float timeInAir = 0f;
		do
		{
			float jumpForce = jumpfallOff.Evaluate(timeInAir);
			charController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
			timeInAir += Time.deltaTime;
			yield return null;
		} while(!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);
		charController.slopeLimit = 45f;
		isJumping = false;
	}

	private bool OnSlope()
	{
		if(isJumping)
			return false;

		if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, charController.height / 2f * slopeForceRayLength))
			if(hit.normal != Vector3.up)
				return true;
		return false;
	}

	private void CheckForCellUnderneathPlayer()
	{
		if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, charController.height / 2f * slopeForceRayLength))
			if(hit.collider.GetComponentInParent<DungeonCell>())
			{
				//Debug.Log("Hit: " + hit.collider.GetComponentInParent<DungeonCell>().name);
				hit.collider.GetComponentInParent<DungeonCell>().OnPlayerEntered();
			}
	}
}
