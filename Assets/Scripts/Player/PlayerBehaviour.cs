using System.Collections;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, IDamageable
{
	[SerializeField] private float health = default;                                        // Health of the player
	[Space]
	[SerializeField] private CharacterController charController = default;                  // Reference to the Character Controller component.
	[SerializeField] private string horizontalInputName = default;                          // Name of the Horizontal Input Axis name.
	[SerializeField] private string verticalInputName = default;                            // Name of the Vertical Input Axis name.
	[Space]
	private float movementSpeed = default;                                                  // Final movement speed depending on input.
	[SerializeField] private float walkSpeed = default;                                     // Base Walking speed.
	[SerializeField] private float runSpeed = default;                                      // Base Running speed.					
	[SerializeField] private float runBuildUpSpeed = default;                               // How fast the player transitions from Walking to running.
	[SerializeField] private KeyCode runKey = default;                                      // Which key to press to start running.
	[SerializeField] private float slopeForce = default;                                    // How hard the player gets pushed downwards on a slope.
	[SerializeField] private float slopeForceRayLength = default;                           // How far to check for a slope underneath a player.
	[Space]
	[SerializeField] private bool isJumping = false;                                        // If the player is jumping or not.
	[SerializeField] private AnimationCurve jumpfallOff = default;                          // What curve to follow when falling downwards.
	[SerializeField] private float jumpMultiplier = default;                                // How "hard" the player gets pushed upwards.
	[SerializeField] private KeyCode jumpKey = default;                                     // Which button to press to start jumping.

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
				hit.collider.GetComponentInParent<DungeonCell>().OnPlayerEntered();
			}
	}

	void IDamageable.Damage(float damage) => health -= damage;
}
