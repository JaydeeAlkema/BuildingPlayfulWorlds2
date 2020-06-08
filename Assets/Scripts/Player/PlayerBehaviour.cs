using System.Collections;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, IDamageable
{
	#region Variables
	[SerializeField] private LayerMask targetMask = default;                                // Target Layer.
	[SerializeField] private LayerMask groundMask = default;                                // Ground Layer.

	[Header("Player Properties")]
	[SerializeField] private float health = default;                                        // Health of the player
	[SerializeField] private float mana = default;                                          // Mana of the player.
	[SerializeField] private float maxMana = default;                                       // Maximum Mana of the player.
	[SerializeField] private float manaRegenAmount = default;                               // How much mana to regenerate every regen tick.
	[SerializeField] private float manaRegenInterval = default;                             // How much time in between regen ticks.
	[Space]
	[SerializeField] private GameObject currentTarget = default;                            // Current Target of the player.
	[SerializeField] private GameObject previousTarget = default;                           // Next Target of the player.

	[Header("Attack Properties")]
	[SerializeField] private GameObject primaryAttackPrefab = default;                      // Primary Attack Prefab.
	[SerializeField] private float primaryAttackCooldown = 1f;                              // Cooldown that starts after the use of the Primary Attack.
	[SerializeField] private GameObject secundaryAttackPrefab = default;                    // Primary Attack Prefab.
	[SerializeField] private float secundaryAttackCooldown = 15f;                           // Cooldown that starts after the use of the Secundary Attack.
	[SerializeField] private KeyCode primaryAttackKey = default;                            // Primary Attack Key.
	[SerializeField] private KeyCode secundaryAttackKey = default;                          // Secunday Attack Key.
	[Space]
	[SerializeField] private CharacterController charController = default;                  // Reference to the Character Controller component.
	[SerializeField] private string horizontalInputName = default;                          // Name of the Horizontal Input Axis name.
	[SerializeField] private string verticalInputName = default;                            // Name of the Vertical Input Axis name.

	[Header("Movement Properties")]
	private float movementSpeed = default;                                                  // Final movement speed depending on input.
	private float horInput = default;
	private float verInput = default;
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
	[SerializeField] private float walkSoundInterval = default;                             // The time between movement sounds.
	[SerializeField] private AudioSource audioSource = default;                             // Reference to the audio source component.
	[SerializeField] private AudioClip[] walkSoundAudioClips = default;                     // Array with all the walk sound effects.

	[Header("Debugging")]
	[SerializeField] private bool primaryAttackOnCooldown = false;
	[SerializeField] private bool secundaryAttackOnCooldown = false;
	#endregion

	#region Properties
	public float Health { get => health; set => health = value; }
	public float Mana { get => mana; set => mana = value; }
	public float MaxMana { get => maxMana; set => maxMana = value; }
	#endregion

	#region Monobehaviour Callbacks
	private void Awake()
	{
		charController = GetComponent<CharacterController>();
	}

	private void Start()
	{
		StartCoroutine(ManaRegen());
		StartCoroutine(PlayMovementAudio());
	}

	private void Update()
	{
		PlayerMovement();
		CheckForCellUnderneathPlayer();
		GetTargetUnderCrosshair();
		AttackInput();

		if(health <= 0) GameManager.Instance.GameState = GameState.GameOver;
		if(mana > MaxMana) mana = maxMana;

		if(currentTarget)
			if(currentTarget.GetComponent<EnemyBehaviour>() != null)
				if(currentTarget.GetComponent<EnemyBehaviour>().State == EnemyState.Dead) currentTarget = null;
	}
	#endregion

	#region Functions
	/// <summary>
	/// Handles the player movement.
	/// </summary>
	private void PlayerMovement()
	{
		horInput = Input.GetAxisRaw(horizontalInputName);
		verInput = Input.GetAxisRaw(verticalInputName);

		Vector3 forwardMovement = transform.forward * verInput;
		Vector3 rightMovement = transform.right * horInput;

		charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1f) * movementSpeed);

		if((verInput != 0f || horInput != 0f) && OnSlope())
			charController.Move(Vector3.down * charController.height / 2f * slopeForce * Time.deltaTime);

		SetMovementSpeed();
		JumpInput();
	}

	/// <summary>
	/// Sets the movementspeed
	/// </summary>
	private void SetMovementSpeed()
	{
		if(Input.GetKey(runKey))
			movementSpeed = Mathf.Lerp(movementSpeed, runSpeed, Time.deltaTime * runBuildUpSpeed);
		else
			movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, Time.deltaTime * runBuildUpSpeed);
	}

	/// <summary>
	/// Plays movement audio depending on the movement speed.
	/// </summary>
	/// <returns></returns>
	private IEnumerator PlayMovementAudio()
	{
		while(true)
		{
			if(charController.velocity != Vector3.zero)
			{
				int randIndex = Random.Range(0, walkSoundAudioClips.Length);
				audioSource.PlayOneShot(walkSoundAudioClips[randIndex], 0.1f);
				yield return new WaitForSeconds(walkSoundInterval / movementSpeed);
			}
			yield return null;
		}
	}

	/// <summary>
	/// Handles jump input.
	/// </summary>
	private void JumpInput()
	{
		if(Input.GetKeyDown(jumpKey) && !isJumping)
		{
			isJumping = true;
			StartCoroutine(JumpEvent());
		}
	}

	/// <summary>
	/// Jump Event.
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// Handles everything that should be done when walking over a slope.
	/// </summary>
	/// <returns></returns>
	private bool OnSlope()
	{
		if(isJumping)
			return false;

		if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, charController.height / 2f * slopeForceRayLength))
			if(hit.normal != Vector3.up)
				return true;
		return false;
	}

	/// <summary>
	/// Checks for Attack Input.
	/// </summary>
	private void AttackInput()
	{
		if(currentTarget)
		{
			if(currentTarget.GetComponent<EnemyBehaviour>() != null)
			{
				if(Input.GetKeyDown(primaryAttackKey))
				{
					AttackEvent(primaryAttackPrefab);
				}
			}
		}
		if(Input.GetKeyDown(secundaryAttackKey))
		{
			AttackEvent(secundaryAttackPrefab);
		}
	}

	/// <summary>
	/// Attack Event.
	/// </summary>
	/// <param name="attackSpellToSpawns"></param>
	private void AttackEvent(GameObject attackSpellToSpawns)
	{
		if(mana - attackSpellToSpawns.GetComponent<Spell>().ManaCost > 0)
		{
			GameObject spellGO = null;
			switch(attackSpellToSpawns.GetComponent<Spell>().SpellType)
			{
				case SpellType.TargetBased:
					if(!primaryAttackOnCooldown)
					{
						spellGO = Instantiate(attackSpellToSpawns, transform.position, transform.rotation);
						spellGO.GetComponent<Spell>().Target = currentTarget.transform;
						mana -= spellGO.GetComponent<Spell>().ManaCost;
						primaryAttackOnCooldown = true;
						StartCoroutine(PrimaryAttackCooldown());
					}
					break;

				case SpellType.GroundBased:
					if(!secundaryAttackOnCooldown)
					{
						Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward * 30);
						Vector3 spellGOSpawnPoint = Vector3.zero;
						if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
						{
							spellGOSpawnPoint = hit.point;
						}

						if(spellGOSpawnPoint != Vector3.zero)
						{
							spellGO = Instantiate(attackSpellToSpawns, spellGOSpawnPoint, transform.rotation);
							mana -= spellGO.GetComponent<Spell>().ManaCost;
						}
						secundaryAttackOnCooldown = true;
						StartCoroutine(SecundaryAttackCooldown());
					}
					break;

				default:
					break;
			}
		}
	}

	/// <summary>
	/// Toggles the primary attack cooldown.
	/// </summary>
	/// <returns></returns>
	private IEnumerator PrimaryAttackCooldown()
	{
		yield return new WaitForSeconds(primaryAttackCooldown);
		primaryAttackOnCooldown = false;
	}

	/// <summary>
	/// Toggles the Secundary attack cooldown.
	/// </summary>
	/// <returns></returns>
	private IEnumerator SecundaryAttackCooldown()
	{
		yield return new WaitForSeconds(secundaryAttackCooldown);
		secundaryAttackOnCooldown = false;
	}

	/// <summary>
	/// Checks for a cell underneath the player.
	/// </summary>
	private void CheckForCellUnderneathPlayer()
	{
		if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, charController.height / 2f * slopeForceRayLength))
			if(hit.collider.GetComponentInParent<DungeonCell>())
			{
				hit.collider.GetComponentInParent<DungeonCell>().OnPlayerEntered();
			}
	}

	/// <summary>
	/// Gets a target from the target layermask underneath the crosshair.
	/// </summary>
	private void GetTargetUnderCrosshair()
	{
		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward * 1000f);
		if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetMask))
		{
			if(hit.collider.GetComponent<EnemyBehaviour>() != null)
			{
				if(hit.collider.GetComponent<EnemyBehaviour>().State != EnemyState.Dead)
				{
					GameObject nextTarget = hit.collider.gameObject;

					if(nextTarget != currentTarget)
					{
						previousTarget = currentTarget;
						currentTarget = nextTarget;
					}
				}
			}
		}
		if(currentTarget != null)
			if(currentTarget.GetComponent<Outline>() != null) currentTarget.GetComponent<Outline>().enabled = true;

		if(previousTarget != null)
			if(previousTarget.GetComponent<Outline>() != null) previousTarget.GetComponent<Outline>().enabled = false;
	}

	/// <summary>
	/// IDamageable Damage Implementation
	/// Removes health from the health pool.
	/// </summary>
	/// <param name="damage"></param>
	void IDamageable.Damage(float damage)
	{
		health -= damage;
	}

	/// <summary>
	/// Changes the movement speed.
	/// Only used when stepping on de Poisoned Ground Spell.
	/// </summary>
	/// <param name="value"></param>
	void IDamageable.ImpactMovementSpeed(float value)
	{
		movementSpeed = value;
	}

	/// <summary>
	/// Actively Regenerates the players Mana.
	/// </summary>
	/// <returns></returns>
	private IEnumerator ManaRegen()
	{
		while(true)
		{
			yield return new WaitForSeconds(manaRegenInterval);
			if(mana < maxMana) mana += manaRegenAmount;
			if(mana > maxMana) mana = maxMana;
		}
	}
	#endregion
}
