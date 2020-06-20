using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum State
{
	Idle,
	Chasing,
	Attacking,
	Dead
}

public class AIBehaviour : MonoBehaviour, IDamageable
{
	#region Variables
	[SerializeField] private float health = default;                                // How much health the AI has.
	[SerializeField] private float damage = default;                                // How much damage to deal to the player.
	[SerializeField] private int manaWorth = default;                               // How much Mana the player gets back for killing an AI.
	[SerializeField] private Image healthBar = default;                             // Reference to the Health Bar Image Component.
	[SerializeField] private GameObject floatingCanvas = default;                  // Reference to the floating canvas.

	[Header("Core Properties")]
	[SerializeField] private State state = State.Idle;                              // State of the AI.
	[SerializeField] private NavMeshAgent agent = default;                          // Reference to the NavMeshAgent component on the AI Gameobject.
	[SerializeField] private Animator anim = default;                               // Reference to the animator component.
	[SerializeField] private float onHitAudioVolume = 0.5f;                         // Volume of the on hit audio.
	[SerializeField] private AudioClip[] onHitAudioClips = default;                 // Array with all the on hit sound effects.

	[Header("Movement Properties")]
	[SerializeField] private float moveSpeed = 3.5f;                                // Walking speed of the AI.
	[SerializeField] private float movementAudioVolume = 0.1f;                      // Volume of the movement audio.
	[SerializeField] private AudioSource audioSource = default;                     // Reference to the audio source component.
	[SerializeField] private AudioClip[] walkSoundAudioClips = default;             // Array with all the walk sound effects.

	[Header("Targeting Properties")]
	[SerializeField] private LayerMask targetMask = default;                        // Which layers to check for target.
	[SerializeField] private Transform target = null;                               // Reference to the target transform.
	[SerializeField] private float targetDestinationUpdateInterval = 0.1f;          // How often the destination will be set for the AI.
	[SerializeField] private float targetDetectionCheckInterval = 0.1f;             // How much time between target detection.
	[SerializeField] private float targetInteractionInterval = 1f;                  // How much time between attacks.
	[SerializeField] private float targetDetectionRadius = 15f;                     // How far the AI can "See".
	[SerializeField] private float targetInteractionRadius = 1f;                    // How far the AI can "Interact" with the target.

	private float maxMoveSpeed;
	#endregion

	#region Properties
	public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
	public State State { get => state; set => state = value; }
	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		agent.speed = moveSpeed;
		maxMoveSpeed = moveSpeed;

		GetComponent<Outline>().enabled = false;
	}

	private void Update()
	{
		if(state != State.Dead)
		{
			anim.SetFloat("Velocity", agent.velocity.magnitude);
			anim.SetBool("Attacking", state == State.Attacking ? true : false);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(SearchForTarget());
		StartCoroutine(MoveToTarget());
		StartCoroutine(AttackTarget());
		StartCoroutine(PlayMovementAudio());
	}
	#endregion

	#region Functions
	/// <summary>
	/// Searches for the nearest target.
	/// </summary>
	/// <returns></returns>
	private IEnumerator SearchForTarget()
	{
		while(true)
		{
			agent.isStopped = false;

			// Find nearest collider.
			Collider[] colliders = Physics.OverlapSphere(transform.position, targetDetectionRadius, targetMask);
			Collider nearestCollider = null;
			float minSqrDistance = Mathf.Infinity;
			for(int i = 0; i < colliders.Length; i++)
			{
				float sqrDistanceToCenter = (transform.position - colliders[i].transform.position).sqrMagnitude;
				if(sqrDistanceToCenter < minSqrDistance)
				{
					minSqrDistance = sqrDistanceToCenter;
					nearestCollider = colliders[i];
				}
			}

			if(nearestCollider != null)
			{
				target = nearestCollider.transform;
				state = State.Chasing;
			}
			yield return new WaitForSeconds(targetDetectionCheckInterval);
		}
	}

	/// <summary>
	/// Moves the AI to the target at a set interval.
	/// </summary>
	/// <returns></returns>
	private IEnumerator MoveToTarget()
	{
		while(true)
		{
			if(state == State.Chasing)
			{
				if(Vector3.Distance(transform.position, target.position) < targetInteractionRadius) state = State.Attacking;

				agent.isStopped = false;
				agent.speed = moveSpeed;
				agent.destination = target.position;
			}
			yield return new WaitForSeconds(targetDestinationUpdateInterval);
		}
	}

	/// <summary>
	/// Attacks the target at a set interval
	/// </summary>
	/// <returns></returns>
	private IEnumerator AttackTarget()
	{
		while(true)
		{
			yield return new WaitForSeconds(targetInteractionInterval);
			if(state == State.Attacking)
			{
				if(Vector3.Distance(transform.position, target.position) > targetInteractionRadius) state = State.Chasing;
				if(target.GetComponent<AIBehaviour>() && target.GetComponent<AIBehaviour>().state == State.Dead) target = null; state = State.Idle;

				agent.isStopped = true;
				target.GetComponent<IDamageable>()?.Damage(damage);
			}
			yield return null;
		}
	}

	/// <summary>
	/// Plays movement audio depending on the movement speed.
	/// </summary>
	/// <returns></returns>
	private IEnumerator PlayMovementAudio()
	{
		while(true)
		{
			if(state == State.Chasing)
			{
				int randIndex = Random.Range(0, walkSoundAudioClips.Length);
				audioSource.PlayOneShot(walkSoundAudioClips[randIndex], movementAudioVolume);
				yield return new WaitForSeconds(0.35f);
			}
			yield return null;
		}
	}

	/// <summary>
	/// IDamageable Damage Implementation
	/// Decreases the health and handles what needs to happen when health falls bellow 0.
	/// </summary>
	/// <param name="damage"></param>
	void IDamageable.Damage(float damage)
	{
		health -= damage;
		ChangeHealthbarImageValue(damage);
		int randIndex = Random.Range(0, onHitAudioClips.Length);
		AudioManager.GetInstance().PlaySoundFX(onHitAudioClips[randIndex], transform.position, onHitAudioVolume);

		if(health <= 0)
		{
			state = State.Dead;
			GameManager.Instance.PlayerInstance.GetComponent<PlayerBehaviour>().Mana += manaWorth;
			anim.SetBool("Dead", true);
			gameObject.name = gameObject.name + " -DEAD-";
			RemoveAllActiveComponents();
		}
	}

	/// <summary>
	/// IDamageable Impact Movement Speed Implementation.
	/// This slows the AI down to a crawl.
	/// </summary>
	/// <param name="value"></param>
	void IDamageable.ImpactMovementSpeed(float value)
	{
		moveSpeed = value;
		agent.speed = moveSpeed;
	}

	/// <summary>
	/// Removes all the active components once the AI is considered Dead.
	/// This is done so the AI carcass can still exist in the world without having all the code still run on the background.
	/// </summary>
	private void RemoveAllActiveComponents()
	{
		target = null;
		agent.isStopped = true;
		Destroy(GetComponent<Outline>());
		Destroy(GetComponent<CapsuleCollider>());
		Destroy(GetComponent<NavMeshAgent>());
		Destroy(floatingCanvas);
		Destroy(this);
	}

	/// <summary>
	/// Changes the floating health bar image value.
	/// </summary>
	/// <param name="value"></param>
	private void ChangeHealthbarImageValue(float value)
	{
		healthBar.fillAmount -= value / 100f;
	}
	#endregion

	#region Debugging
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, targetDetectionRadius);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, targetInteractionRadius);
	}
	#endregion
}
