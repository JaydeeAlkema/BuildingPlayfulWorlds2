using System.Collections;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, IDamageable
{
	#region Variables
	[SerializeField] private float health = default;                                        // Health of the player
	[SerializeField] private float mana = default;                                          // Mana of the player.
	[SerializeField] private float maxMana = default;                                       // Maximum Mana of the player.
	[SerializeField] private float manaRegenAmount = default;                               // How much mana to regenerate every regen tick.
	[SerializeField] private float manaRegenInterval = default;                             // How much time in between regen ticks.
	[Space]
	[SerializeField] private GameObject currentTarget = default;                            // Current Target of the player.
	[SerializeField] private GameObject previousTarget = default;                               // Next Target of the player.
	[SerializeField] private LayerMask targetMask = default;                                // Which layermask to check for.
	[Space]
	[Header("Attack Spells")]
	[SerializeField] private GameObject primaryAttackPrefab = default;                      // Primary Attack Prefab.
	[SerializeField] private GameObject secundaryAttackPrefab = default;                    // Primary Attack Prefab.
	[SerializeField] private KeyCode primaryAttackKey = default;                            // Primary Attack Key.
	[SerializeField] private KeyCode secundaryAttackKey = default;                          // Secunday Attack Key.
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
	#endregion

	#region Properties
	public float Health { get => health; set => health = value; }
	public float Mana { get => mana; set => mana = value; }
	public float MaxMana { get => maxMana; set => maxMana = value; }
	#endregion

	private void Awake()
	{
		charController = GetComponent<CharacterController>();
	}

	private void Start()
	{
		StartCoroutine(ManaRegen());
	}

	private void Update()
	{
		PlayerMovement();
		CheckForCellUnderneathPlayer();
		GetTargetUnderCrosshair();
		AttackInput();

		if(health <= 0) GameManager.Instance.GameState = GameState.GameOver;
		if(mana > MaxMana) mana = maxMana;
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

	private void AttackInput()
	{
		if(currentTarget)
		{
			if(Input.GetKeyDown(primaryAttackKey))
			{
				AttackEvent(primaryAttackPrefab);
			}
			else if(Input.GetKeyDown(secundaryAttackKey))
			{
				AttackEvent(secundaryAttackPrefab);
			}
		}
	}

	private void AttackEvent(GameObject attackSpellToSpawns)
	{
		if(mana - attackSpellToSpawns.GetComponent<Spell>().ManaCost > 0)
		{
			GameObject spellGO = Instantiate(attackSpellToSpawns, transform.position, transform.rotation);
			spellGO.GetComponent<Spell>().Target = currentTarget.transform;
			mana -= spellGO.GetComponent<Spell>().ManaCost;
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

	private void GetTargetUnderCrosshair()
	{
		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward * 1000f);
		if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetMask))
		{
			GameObject nextTarget = hit.collider.gameObject;

			if(nextTarget != currentTarget)
			{
				previousTarget = currentTarget;
				currentTarget = nextTarget;
			}
		}
		if(currentTarget != null) currentTarget.GetComponent<Outline>().enabled = true;
		if(previousTarget != null) previousTarget.GetComponent<Outline>().enabled = false;
	}

	void IDamageable.Damage(float damage) => health -= damage;

	private IEnumerator ManaRegen()
	{
		while(true)
		{
			yield return new WaitForSeconds(manaRegenInterval);
			if(mana < maxMana) mana += manaRegenAmount;
			if(mana > maxMana) mana = maxMana;
		}
	}
}
